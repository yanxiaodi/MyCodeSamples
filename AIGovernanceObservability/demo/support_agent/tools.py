"""Synthetic tools with observable execution counts; no external side effects."""
import time


class ToolRepository:
    def __init__(self, *, simulate_timeout=False, timeout_seconds=0.8):
        self.simulate_timeout = simulate_timeout
        self.timeout_seconds = timeout_seconds
        self.body_counts = {"lookup_customer": 0, "delete_record": 0}

    def lookup_customer(self, customer_id):
        self.body_counts["lookup_customer"] += 1
        if self.simulate_timeout:
            time.sleep(self.timeout_seconds)
            raise TimeoutError("Simulated customer-service timeout.")
        time.sleep(0.06)
        if customer_id != "C-1042":
            raise LookupError("The synthetic customer was not found.")
        return {"name": "Avery Example", "status": "active", "support_status": "Awaiting document review"}

    def delete_record(self, customer_id):
        self.body_counts["delete_record"] += 1
        return {
            "operation": "delete_record",
            "customer_id": customer_id,
            "simulated": True,
            "record_changed": False,
        }
