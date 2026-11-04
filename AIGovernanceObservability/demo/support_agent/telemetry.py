"""Own the OTel pipelines; OpenLLMetry owns only the OpenAI SDK instrumentation."""
from __future__ import annotations

import json
import logging
import os
import threading
from collections import deque
from contextvars import ContextVar
from dataclasses import dataclass
from pathlib import Path

from opentelemetry import metrics, trace, _logs
from opentelemetry.sdk.resources import Resource
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import SpanExporter, SpanExportResult, SimpleSpanProcessor, BatchSpanProcessor
from opentelemetry.sdk.metrics import MeterProvider
from opentelemetry.sdk.metrics.export import InMemoryMetricReader, PeriodicExportingMetricReader
from opentelemetry.sdk._logs import LoggerProvider, LoggingHandler
from opentelemetry.sdk._logs.export import InMemoryLogRecordExporter, SimpleLogRecordProcessor, BatchLogRecordProcessor
from opentelemetry.exporter.otlp.proto.http.trace_exporter import OTLPSpanExporter
from opentelemetry.exporter.otlp.proto.http.metric_exporter import OTLPMetricExporter
from opentelemetry.exporter.otlp.proto.http._log_exporter import OTLPLogExporter
from opentelemetry.instrumentation.openai import OpenAIInstrumentor

provider_context = ContextVar("demo_provider", default="mock")
logger = logging.getLogger("support-demo")
_runtime = None
_configure_lock = threading.Lock()


class EvidenceExporter(SpanExporter):
    """Bounded local evidence for the demo; content capture remains off upstream."""
    def __init__(self, path: Path | None):
        self.path = path
        self._records = deque(maxlen=500)
        self._lock = threading.Lock()

    def export(self, spans):
        records = []
        for span in spans:
            # Local run evidence excludes polling noise. The OTLP pipeline still
            # receives every span, including the enclosing HTTP request.
            scope = span.instrumentation_scope.name
            if scope != "support-agent" and not scope.startswith("opentelemetry.instrumentation.openai"):
                continue
            records.append({
                "name": span.name, "trace_id": format(span.context.trace_id, "032x"),
                "span_id": format(span.context.span_id, "016x"),
                "parent_span_id": format(span.parent.span_id, "016x") if span.parent else None,
                "scope": span.instrumentation_scope.name, "status": span.status.status_code.name,
                "start_time_ms": span.start_time / 1_000_000,
                "duration_ms": round((span.end_time - span.start_time) / 1_000_000, 2),
                "attributes": dict(span.attributes or {}),
                "events": [{"name": e.name, "attributes": dict(e.attributes or {})} for e in span.events],
            })
        with self._lock:
            self._records.extend(records)
            if self.path:
                self.path.parent.mkdir(parents=True, exist_ok=True)
                with self.path.open("a", encoding="utf-8") as file:
                    for record in records:
                        file.write(json.dumps(record, default=str) + "\n")
        return SpanExportResult.SUCCESS

    def snapshot(self):
        with self._lock:
            return list(self._records)

    def shutdown(self):
        pass


@dataclass
class TelemetryRuntime:
    tracer_provider: TracerProvider
    meter_provider: MeterProvider
    logger_provider: LoggerProvider
    evidence: EvidenceExporter
    metric_reader: InMemoryMetricReader
    log_exporter: InMemoryLogRecordExporter
    otlp_endpoint: str | None

    def flush(self):
        self.tracer_provider.force_flush(timeout_millis=5000)
        self.meter_provider.force_flush(timeout_millis=5000)
        self.logger_provider.force_flush(timeout_millis=5000)

    def spans(self):
        return self.evidence.snapshot()

    def metric_points(self):
        data = self.metric_reader.get_metrics_data()
        points = []
        if not data:
            return points
        for resource in data.resource_metrics:
            for scope in resource.scope_metrics:
                for metric in scope.metrics:
                    for point in metric.data.data_points:
                        points.append({"name": metric.name, "unit": metric.unit,
                                       "attributes": dict(point.attributes or {}),
                                       "value": getattr(point, "value", getattr(point, "sum", 0)),
                                       "count": getattr(point, "count", None)})
        return points

    def logs(self):
        return [{"body": str(record.log_record.body),
                 "trace_id": format(record.log_record.trace_id or 0, "032x"),
                 "span_id": format(record.log_record.span_id or 0, "016x"),
                 "attributes": dict(record.log_record.attributes or {})}
                for record in self.log_exporter.get_finished_logs()[-200:]]


def configure_telemetry(*, otlp_endpoint=None, artifact_path=None, trace_content=False):
    global _runtime
    with _configure_lock:
        if _runtime is not None:
            return _runtime
        os.environ["TRACELOOP_TRACE_CONTENT"] = "true" if trace_content else "false"
        os.environ["TRACELOOP_METRICS_ENABLED"] = "true"
        resource = Resource.create({"service.name": "governed-support-agent", "service.version": "1.0.0"})
        evidence = EvidenceExporter(Path(artifact_path) if artifact_path else None)
        traces = TracerProvider(resource=resource)
        traces.add_span_processor(SimpleSpanProcessor(evidence))
        reader = InMemoryMetricReader()
        readers = [reader]
        logs = LoggerProvider(resource=resource)
        log_exporter = InMemoryLogRecordExporter()
        logs.add_log_record_processor(SimpleLogRecordProcessor(log_exporter))
        if otlp_endpoint:
            base = otlp_endpoint.rstrip("/")
            traces.add_span_processor(BatchSpanProcessor(OTLPSpanExporter(endpoint=base + "/v1/traces", timeout=3), schedule_delay_millis=200))
            readers.append(PeriodicExportingMetricReader(OTLPMetricExporter(endpoint=base + "/v1/metrics", timeout=3), export_interval_millis=1000))
            logs.add_log_record_processor(BatchLogRecordProcessor(OTLPLogExporter(endpoint=base + "/v1/logs", timeout=3), schedule_delay_millis=200))
        meters = MeterProvider(resource=resource, metric_readers=readers)
        trace.set_tracer_provider(traces)
        metrics.set_meter_provider(meters)
        _logs.set_logger_provider(logs)
        logger.setLevel(logging.INFO)
        logger.propagate = False
        logger.addHandler(LoggingHandler(level=logging.INFO, logger_provider=logs))
        OpenAIInstrumentor(
            enable_trace_context_propagation=False,
            get_common_metrics_attributes=lambda: {"app.demo.provider": provider_context.get()},
        ).instrument(tracer_provider=traces, meter_provider=meters, logger_provider=logs)
        _runtime = TelemetryRuntime(traces, meters, logs, evidence, reader, log_exporter, otlp_endpoint)
        return _runtime


def record_policy(decision, tool, rule, principal_role):
    attrs = {"app.governance.decision": decision, "app.tool.name": tool,
             "app.governance.rule": rule, "app.demo.provider": provider_context.get(),
             "app.demo.identity.role": principal_role}
    metrics.get_meter("support-agent").create_counter("app.governance.decisions").add(1, attrs)
    logger.info("Policy evaluated", extra=attrs)


def record_tool(tool, outcome, duration):
    attrs = {"app.tool.name": tool, "app.tool.outcome": outcome, "app.demo.provider": provider_context.get()}
    metrics.get_meter("support-agent").create_histogram("app.tool.duration", unit="s").record(duration, attrs)
    logger.info("Tool finished", extra=attrs)


def record_run(outcome, duration):
    attrs = {"app.run.outcome": outcome, "app.demo.provider": provider_context.get()}
    meter = metrics.get_meter("support-agent")
    meter.create_counter("app.agent.runs").add(1, attrs)
    meter.create_histogram("app.agent.duration", unit="s").record(duration, attrs)
    logger.info("Agent run finished", extra=attrs)
