package demo.customer_support

import rego.v1

default verdict := {
    "decision": "allow",
    "reason": "customer_support_action_allowed",
}

input_bodies contains lower(input) if {
    is_string(input)
}

input_bodies contains lower(input.body) if {
    is_string(input.body)
}

input_bodies contains lower(input.input.body) if {
    is_string(input.input.body)
}

input_bodies contains lower(input.snapshot.input.body) if {
    is_string(input.snapshot.input.body)
}

dangerous_input if {
    some body in input_bodies
    contains(body, "balance")
}

dangerous_input if {
    some body in input_bodies
    contains(body, "payment card")
}

dangerous_input if {
    some body in input_bodies
    contains(body, "credit card")
}

dangerous_input if {
    some body in input_bodies
    contains(body, "ssn")
}

dangerous_input if {
    some body in input_bodies
    contains(body, "delete all")
}

verdict := {
    "decision": "deny",
    "reason": "sensitive_customer_data_request_blocked",
    "message": "Requests for balances, payment data, secrets, or broad destructive actions are blocked by policy.",
} if {
    input.intervention_point == "input"
    dangerous_input
}

verdict := {
    "decision": "deny",
    "reason": "destructive_tool_blocked",
    "message": "Destructive tools are blocked by policy.",
} if {
    input.intervention_point == "pre_tool_call"
    input.tool.clearance == "destructive"
}

verdict := {
    "decision": "allow",
    "reason": "external_side_effect_requires_approval",
    "message": "Policy allows this external side effect only after host-mediated approval.",
    "result_labels": ["requires_approval", "external_side_effect"],
} if {
    input.intervention_point == "pre_tool_call"
    input.tool.clearance == "external_side_effect"
}
