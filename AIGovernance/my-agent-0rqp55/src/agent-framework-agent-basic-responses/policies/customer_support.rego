package demo.customer_support

import rego.v1

default verdict := {
    "decision": "allow",
    "reason": "customer_support_action_allowed",
}

verdict := {
    "decision": "deny",
    "reason": "destructive_record_deletion_blocked",
    "message": "Deleting customer records is blocked by policy.",
} if {
    input.intervention_point == "pre_tool_call"
    input.tool.name == "delete_record"
}

verdict := {
    "decision": "allow",
    "reason": "email_is_subject_to_maf_user_approval",
    "message": "The email tool is allowed by policy and requires MAF approval before execution.",
} if {
    input.intervention_point == "pre_tool_call"
    input.tool.name == "send_email"
}
