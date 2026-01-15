# Monitoring and Incident Response

[← Back to Index](00_INDEX.md) | [Previous: Human-in-the-Loop](16_HUMAN_IN_THE_LOOP.md) | [Next: Effectiveness Analysis →](18_EFFECTIVENESS_ANALYSIS.md)

---

## Overview

Comprehensive monitoring enables detection of attacks, supports incident response, and provides data for continuous improvement. This document covers logging strategies, detection patterns, alerting systems, and incident response playbooks specific to prompt injection.

---

## Logging Strategy

### What to Log

```
┌─────────────────────────────────────────────────────────────────┐
│              COMPREHENSIVE LLM LOGGING                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  INPUT LOGGING                                                  │
│  ├── User input (full text, hashed for privacy if needed)      │
│  ├── System prompt (hash only - don't log actual prompt)       │
│  ├── Context/documents provided                                │
│  ├── Tool/function call requests                               │
│  └── Session and user identifiers                              │
│                                                                 │
│  PROCESSING LOGGING                                             │
│  ├── Input validation results (all stages)                     │
│  ├── Detection system outputs (classifiers, etc.)              │
│  ├── Risk scores calculated                                    │
│  ├── Approval requests/decisions                               │
│  └── Model selection and parameters                            │
│                                                                 │
│  OUTPUT LOGGING                                                 │
│  ├── LLM response (full or sampled)                            │
│  ├── Output validation results                                 │
│  ├── Actions taken (tool calls, etc.)                          │
│  ├── Final response delivered to user                          │
│  └── Response latency metrics                                  │
│                                                                 │
│  METADATA                                                       │
│  ├── Timestamp (high precision)                                │
│  ├── Request ID (for correlation)                              │
│  ├── Session ID                                                │
│  ├── User ID (if available)                                    │
│  ├── Client information (IP, user agent - for forensics)       │
│  └── Environment (production, staging, etc.)                   │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Structured Logging Implementation

```python
import json
import hashlib
import time
import uuid
from dataclasses import dataclass, asdict
from typing import Optional, List, Dict, Any
from datetime import datetime

@dataclass
class LLMSecurityLog:
    """Structured log entry for LLM interactions."""
    
    # Identifiers
    request_id: str
    session_id: str
    user_id: Optional[str]
    timestamp: str
    
    # Input data
    input_text_hash: str
    input_length: int
    system_prompt_hash: str
    context_provided: bool
    
    # Security analysis
    input_validation: Dict[str, Any]
    detection_results: Dict[str, Any]
    risk_score: float
    risk_level: str
    
    # Processing
    approval_required: bool
    approval_status: Optional[str]
    tools_requested: List[str]
    tools_executed: List[str]
    
    # Output
    output_length: int
    output_validation: Dict[str, Any]
    blocked: bool
    block_reason: Optional[str]
    
    # Performance
    latency_ms: float
    
    # Environment
    environment: str
    model_used: str

class SecurityLogger:
    """
    Comprehensive security logging for LLM applications.
    """
    
    def __init__(self, 
                 log_destination: str = "stdout",
                 sampling_rate: float = 1.0,
                 hash_pii: bool = True):
        self.destination = log_destination
        self.sampling_rate = sampling_rate
        self.hash_pii = hash_pii
    
    def log_interaction(self,
                        request_id: str,
                        session_id: str,
                        user_id: str,
                        input_text: str,
                        system_prompt: str,
                        validation_results: dict,
                        detection_results: dict,
                        risk_assessment: dict,
                        output_text: str,
                        output_validation: dict,
                        tools_requested: list,
                        tools_executed: list,
                        blocked: bool,
                        block_reason: str,
                        latency_ms: float,
                        environment: str,
                        model: str):
        """
        Create comprehensive log entry for an LLM interaction.
        """
        
        log_entry = LLMSecurityLog(
            request_id=request_id,
            session_id=session_id,
            user_id=self._hash_if_needed(user_id),
            timestamp=datetime.utcnow().isoformat() + "Z",
            
            input_text_hash=self._hash(input_text),
            input_length=len(input_text),
            system_prompt_hash=self._hash(system_prompt),
            context_provided=bool(validation_results.get("context")),
            
            input_validation=validation_results,
            detection_results=detection_results,
            risk_score=risk_assessment.get("overall_score", 0),
            risk_level=risk_assessment.get("risk_level", "unknown"),
            
            approval_required=risk_assessment.get("approval_required", False),
            approval_status=risk_assessment.get("approval_status"),
            tools_requested=tools_requested,
            tools_executed=tools_executed,
            
            output_length=len(output_text) if output_text else 0,
            output_validation=output_validation,
            blocked=blocked,
            block_reason=block_reason,
            
            latency_ms=latency_ms,
            
            environment=environment,
            model_used=model
        )
        
        self._write_log(log_entry)
        
        return log_entry
    
    def log_security_event(self,
                           event_type: str,
                           severity: str,
                           details: dict,
                           request_id: str = None):
        """
        Log a security-specific event.
        """
        event = {
            "event_type": event_type,
            "severity": severity,
            "timestamp": datetime.utcnow().isoformat() + "Z",
            "request_id": request_id or str(uuid.uuid4()),
            "details": details
        }
        
        self._write_log(event, log_type="security_event")
    
    def _hash(self, text: str) -> str:
        """Create hash of text for logging."""
        return hashlib.sha256(text.encode()).hexdigest()[:16]
    
    def _hash_if_needed(self, value: str) -> str:
        """Hash PII if configured to do so."""
        if self.hash_pii and value:
            return self._hash(value)
        return value
    
    def _write_log(self, log_entry, log_type: str = "interaction"):
        """Write log entry to destination."""
        if isinstance(log_entry, LLMSecurityLog):
            log_dict = asdict(log_entry)
        else:
            log_dict = log_entry
        
        log_dict["log_type"] = log_type
        
        log_line = json.dumps(log_dict)
        
        if self.destination == "stdout":
            print(log_line)
        else:
            # Write to file, send to logging service, etc.
            pass
```

---

## Detection Patterns

### Anomaly Detection

```python
from collections import defaultdict
from datetime import datetime, timedelta
import statistics

class AnomalyDetector:
    """
    Detect anomalous patterns in LLM usage.
    """
    
    def __init__(self):
        self.user_history = defaultdict(list)
        self.session_history = defaultdict(list)
        self.global_stats = {
            "input_lengths": [],
            "latencies": [],
            "risk_scores": []
        }
    
    def analyze(self, interaction: dict) -> dict:
        """
        Analyze interaction for anomalies.
        """
        anomalies = []
        
        # 1. Request rate anomaly
        rate_anomaly = self._check_rate_anomaly(interaction)
        if rate_anomaly:
            anomalies.append(rate_anomaly)
        
        # 2. Input pattern anomaly
        input_anomaly = self._check_input_anomaly(interaction)
        if input_anomaly:
            anomalies.append(input_anomaly)
        
        # 3. Behavioral anomaly
        behavior_anomaly = self._check_behavioral_anomaly(interaction)
        if behavior_anomaly:
            anomalies.append(behavior_anomaly)
        
        # 4. Tool usage anomaly
        tool_anomaly = self._check_tool_anomaly(interaction)
        if tool_anomaly:
            anomalies.append(tool_anomaly)
        
        # 5. Sequential pattern anomaly (multi-turn attacks)
        sequence_anomaly = self._check_sequence_anomaly(interaction)
        if sequence_anomaly:
            anomalies.append(sequence_anomaly)
        
        # Update history
        self._update_history(interaction)
        
        return {
            "anomalies_detected": len(anomalies) > 0,
            "anomalies": anomalies,
            "overall_anomaly_score": self._calculate_anomaly_score(anomalies)
        }
    
    def _check_rate_anomaly(self, interaction: dict) -> Optional[dict]:
        """Check for unusual request rate."""
        user_id = interaction.get("user_id")
        session_id = interaction.get("session_id")
        
        # Get recent requests for this user
        recent = [
            r for r in self.user_history[user_id]
            if r["timestamp"] > datetime.utcnow() - timedelta(minutes=5)
        ]
        
        if len(recent) > 20:  # More than 20 requests in 5 minutes
            return {
                "type": "rate_anomaly",
                "severity": "medium",
                "description": f"High request rate: {len(recent)} in 5 minutes",
                "value": len(recent)
            }
        
        return None
    
    def _check_input_anomaly(self, interaction: dict) -> Optional[dict]:
        """Check for unusual input characteristics."""
        input_length = interaction.get("input_length", 0)
        
        if len(self.global_stats["input_lengths"]) > 100:
            mean = statistics.mean(self.global_stats["input_lengths"])
            stdev = statistics.stdev(self.global_stats["input_lengths"])
            
            z_score = (input_length - mean) / stdev if stdev > 0 else 0
            
            if abs(z_score) > 3:  # More than 3 standard deviations
                return {
                    "type": "input_length_anomaly",
                    "severity": "low",
                    "description": f"Unusual input length (z-score: {z_score:.2f})",
                    "value": input_length
                }
        
        return None
    
    def _check_behavioral_anomaly(self, interaction: dict) -> Optional[dict]:
        """Check for behavioral pattern anomalies."""
        user_id = interaction.get("user_id")
        current_action = interaction.get("action_type")
        
        # Get user's typical actions
        history = self.user_history[user_id][-50:]  # Last 50 interactions
        
        if history:
            typical_actions = [h.get("action_type") for h in history]
            
            if current_action and current_action not in typical_actions:
                return {
                    "type": "behavioral_anomaly",
                    "severity": "medium",
                    "description": f"First time action type: {current_action}",
                    "value": current_action
                }
        
        return None
    
    def _check_tool_anomaly(self, interaction: dict) -> Optional[dict]:
        """Check for unusual tool usage patterns."""
        tools_requested = interaction.get("tools_requested", [])
        
        # Check for sensitive tools
        sensitive_tools = ["send_email", "execute_code", "delete", "admin"]
        
        sensitive_requested = [t for t in tools_requested if t in sensitive_tools]
        
        if len(sensitive_requested) > 2:
            return {
                "type": "tool_anomaly",
                "severity": "high",
                "description": f"Multiple sensitive tools requested: {sensitive_requested}",
                "value": sensitive_requested
            }
        
        return None
    
    def _check_sequence_anomaly(self, interaction: dict) -> Optional[dict]:
        """Check for multi-turn attack patterns."""
        session_id = interaction.get("session_id")
        session_history = self.session_history[session_id]
        
        if len(session_history) >= 3:
            # Check for escalating risk scores (gradual manipulation)
            recent_risks = [h.get("risk_score", 0) for h in session_history[-5:]]
            
            if all(recent_risks[i] <= recent_risks[i+1] for i in range(len(recent_risks)-1)):
                if recent_risks[-1] - recent_risks[0] > 0.3:
                    return {
                        "type": "escalation_pattern",
                        "severity": "high",
                        "description": "Risk scores escalating across turns",
                        "value": recent_risks
                    }
        
        return None
    
    def _update_history(self, interaction: dict):
        """Update history for future comparisons."""
        user_id = interaction.get("user_id")
        session_id = interaction.get("session_id")
        
        interaction["timestamp"] = datetime.utcnow()
        
        self.user_history[user_id].append(interaction)
        self.session_history[session_id].append(interaction)
        
        # Keep history bounded
        if len(self.user_history[user_id]) > 1000:
            self.user_history[user_id] = self.user_history[user_id][-500:]
        
        # Update global stats
        if interaction.get("input_length"):
            self.global_stats["input_lengths"].append(interaction["input_length"])
            if len(self.global_stats["input_lengths"]) > 10000:
                self.global_stats["input_lengths"] = self.global_stats["input_lengths"][-5000:]
    
    def _calculate_anomaly_score(self, anomalies: list) -> float:
        """Calculate overall anomaly score."""
        if not anomalies:
            return 0.0
        
        severity_scores = {"low": 0.2, "medium": 0.5, "high": 0.8, "critical": 1.0}
        
        scores = [severity_scores.get(a["severity"], 0.5) for a in anomalies]
        return min(sum(scores), 1.0)
```

---

## Alerting System

### Alert Configuration

```python
from enum import Enum
from typing import List, Callable
from dataclasses import dataclass

class AlertSeverity(Enum):
    INFO = "info"
    WARNING = "warning"
    HIGH = "high"
    CRITICAL = "critical"

class AlertChannel(Enum):
    EMAIL = "email"
    SLACK = "slack"
    PAGERDUTY = "pagerduty"
    WEBHOOK = "webhook"
    SMS = "sms"

@dataclass
class AlertRule:
    name: str
    description: str
    condition: Callable[[dict], bool]
    severity: AlertSeverity
    channels: List[AlertChannel]
    throttle_minutes: int = 5
    auto_create_incident: bool = False

class AlertManager:
    """
    Manage security alerts for LLM systems.
    """
    
    def __init__(self):
        self.rules = []
        self.last_alerts = {}  # For throttling
        self.channels = {}
    
    def add_rule(self, rule: AlertRule):
        """Add an alert rule."""
        self.rules.append(rule)
    
    def register_channel(self, channel: AlertChannel, handler: Callable):
        """Register a channel handler."""
        self.channels[channel] = handler
    
    def evaluate(self, event: dict) -> List[dict]:
        """
        Evaluate event against all rules and fire alerts.
        """
        fired_alerts = []
        
        for rule in self.rules:
            if rule.condition(event):
                # Check throttle
                if self._is_throttled(rule.name, rule.throttle_minutes):
                    continue
                
                alert = self._create_alert(rule, event)
                
                # Send to channels
                for channel in rule.channels:
                    if channel in self.channels:
                        self.channels[channel](alert)
                
                # Update throttle
                self.last_alerts[rule.name] = datetime.utcnow()
                
                # Create incident if configured
                if rule.auto_create_incident:
                    self._create_incident(alert)
                
                fired_alerts.append(alert)
        
        return fired_alerts
    
    def _is_throttled(self, rule_name: str, throttle_minutes: int) -> bool:
        """Check if alert is throttled."""
        if rule_name not in self.last_alerts:
            return False
        
        elapsed = datetime.utcnow() - self.last_alerts[rule_name]
        return elapsed.total_seconds() < throttle_minutes * 60
    
    def _create_alert(self, rule: AlertRule, event: dict) -> dict:
        """Create alert object."""
        return {
            "alert_id": str(uuid.uuid4()),
            "rule_name": rule.name,
            "severity": rule.severity.value,
            "description": rule.description,
            "timestamp": datetime.utcnow().isoformat(),
            "event": event
        }
    
    def _create_incident(self, alert: dict):
        """Create incident for tracking."""
        # Integration with incident management system
        pass

# Pre-defined rules
DEFAULT_RULES = [
    AlertRule(
        name="injection_detected",
        description="Prompt injection attempt detected",
        condition=lambda e: e.get("detection_results", {}).get("injection_detected"),
        severity=AlertSeverity.HIGH,
        channels=[AlertChannel.SLACK, AlertChannel.EMAIL],
        throttle_minutes=1
    ),
    AlertRule(
        name="high_risk_blocked",
        description="High-risk action was blocked",
        condition=lambda e: e.get("blocked") and e.get("risk_level") in ["high", "critical"],
        severity=AlertSeverity.HIGH,
        channels=[AlertChannel.SLACK],
        throttle_minutes=5
    ),
    AlertRule(
        name="prompt_leakage",
        description="System prompt leakage detected",
        condition=lambda e: e.get("output_validation", {}).get("leakage_detected"),
        severity=AlertSeverity.CRITICAL,
        channels=[AlertChannel.SLACK, AlertChannel.PAGERDUTY],
        throttle_minutes=0,
        auto_create_incident=True
    ),
    AlertRule(
        name="anomaly_detected",
        description="Behavioral anomaly detected",
        condition=lambda e: e.get("anomaly_score", 0) > 0.7,
        severity=AlertSeverity.WARNING,
        channels=[AlertChannel.SLACK],
        throttle_minutes=10
    ),
]
```

---

## Incident Response Playbook

### Prompt Injection Incident Response

```yaml
# PROMPT INJECTION INCIDENT RESPONSE PLAYBOOK

incident_type: prompt_injection_attack
severity_levels: [low, medium, high, critical]

phases:
  
  detection:
    description: "Initial detection and triage"
    time_target: "< 15 minutes"
    steps:
      - Verify alert is not false positive
      - Identify affected user/session
      - Assess scope (single request vs. pattern)
      - Determine severity level
    artifacts:
      - Request logs
      - Detection system output
      - User session data
  
  containment:
    description: "Contain the incident to prevent further damage"
    time_target: "< 30 minutes"
    
    by_severity:
      low:
        - Log the incident
        - Continue monitoring
        - No immediate action required
      
      medium:
        - Block the specific session
        - Increase monitoring for user
        - Review recent activity
      
      high:
        - Immediately revoke session
        - Disable affected tools/capabilities
        - Alert security team
        - Begin forensic preservation
      
      critical:
        - Emergency disable of affected system component
        - Activate incident response team
        - Notify management
        - Consider system-wide pause
  
  eradication:
    description: "Remove the threat and close vulnerabilities"
    steps:
      - Identify attack vector used
      - Update detection rules to catch variant
      - Patch any vulnerabilities exploited
      - Update blocklists/allowlists
      - Test fixes in staging
  
  recovery:
    description: "Restore normal operations"
    steps:
      - Re-enable disabled components
      - Restore blocked users (if appropriate)
      - Verify defenses are working
      - Resume normal monitoring levels
  
  post_incident:
    description: "Learn from the incident"
    time_target: "Within 72 hours"
    steps:
      - Complete incident report
      - Hold post-mortem meeting
      - Update playbooks based on learnings
      - Share findings with team
      - Update training materials

severity_criteria:
  low:
    - Single failed injection attempt
    - No data access
    - Automated defense blocked attack
  
  medium:
    - Multiple attempts from same source
    - Partial success (behavior change but no data)
    - New attack technique observed
  
  high:
    - Successful injection with data access
    - Tool misuse achieved
    - Multiple users/sessions affected
  
  critical:
    - Data exfiltration confirmed
    - System prompt leaked
    - Lateral movement detected
    - Financial or safety impact

escalation_contacts:
  tier1: security-analyst@company.com
  tier2: security-lead@company.com
  tier3: incident-commander@company.com
  executive: ciso@company.com
```

---

## Dashboard Metrics

### Key Security Metrics to Track

```python
SECURITY_METRICS = {
    # Volume metrics
    "total_requests": {
        "description": "Total LLM requests",
        "aggregation": "count",
        "window": "1h"
    },
    "blocked_requests": {
        "description": "Requests blocked by security controls",
        "aggregation": "count",
        "window": "1h"
    },
    "block_rate": {
        "description": "Percentage of requests blocked",
        "aggregation": "rate",
        "window": "1h"
    },
    
    # Detection metrics
    "injection_attempts": {
        "description": "Detected injection attempts",
        "aggregation": "count",
        "window": "1h"
    },
    "detection_latency_p99": {
        "description": "99th percentile detection latency",
        "aggregation": "percentile_99",
        "window": "1h"
    },
    
    # Risk metrics
    "high_risk_requests": {
        "description": "Requests with high risk score",
        "aggregation": "count",
        "window": "1h"
    },
    "average_risk_score": {
        "description": "Average risk score across requests",
        "aggregation": "mean",
        "window": "1h"
    },
    
    # Tool usage metrics
    "tool_calls_total": {
        "description": "Total tool invocations",
        "aggregation": "count",
        "window": "1h"
    },
    "sensitive_tool_calls": {
        "description": "Sensitive tool invocations",
        "aggregation": "count",
        "window": "1h"
    },
    
    # HITL metrics
    "approval_requests": {
        "description": "Human approval requests",
        "aggregation": "count",
        "window": "24h"
    },
    "approval_latency": {
        "description": "Time to human approval",
        "aggregation": "mean",
        "window": "24h"
    },
    
    # Incident metrics
    "active_incidents": {
        "description": "Currently active security incidents",
        "aggregation": "gauge",
        "window": "current"
    },
    "mean_time_to_detect": {
        "description": "Average time to detect incidents",
        "aggregation": "mean",
        "window": "30d"
    }
}
```

---

## Summary

Effective monitoring and incident response requires:

1. **Comprehensive logging** of all LLM interactions
2. **Real-time anomaly detection** for attack patterns
3. **Tiered alerting** to avoid alert fatigue
4. **Documented playbooks** for consistent response
5. **Metrics dashboards** for visibility and trends

---

[← Back to Index](00_INDEX.md) | [Previous: Human-in-the-Loop](16_HUMAN_IN_THE_LOOP.md) | [Next: Effectiveness Analysis →](18_EFFECTIVENESS_ANALYSIS.md)
