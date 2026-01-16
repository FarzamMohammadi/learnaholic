# Human-in-the-Loop (HITL) Defenses

[← Back to Index](00_INDEX.md) | [Previous: Agentic Security](15_AGENTIC_SECURITY.md) | [Next: Monitoring & Incident Response →](17_MONITORING_INCIDENT_RESPONSE.md)

---

## Overview

Human-in-the-loop (HITL) controls catch attacks that bypass automated defenses. A human reviewer provides the ultimate safety net. The challenge is determining when to involve humans without creating bottlenecks or approval fatigue.

## Summary

- Classify actions by risk level to determine when approval is needed
- Design async workflows that don't block user experience
- Use multi-factor risk scoring to prioritize human attention
- Implement tiered escalation for efficient security response

---

## When to Require Human Approval

### Action Classification Matrix

```
┌─────────────────────────────────────────────────────────────────┐
│              ACTION RISK CLASSIFICATION                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│             LOW RISK               HIGH RISK                    │
│          (Automatic OK)        (Human Required)                 │
│                                                                 │
│  READ      ┌─────────┐          ┌─────────┐                    │
│  OPERATIONS│ Search  │          │ Access  │                    │
│            │ internal│          │ sensitive│                    │
│            │ docs    │          │ data    │                     │
│            └─────────┘          └─────────┘                    │
│                                                                 │
│  WRITE     ┌─────────┐          ┌─────────┐                    │
│  OPERATIONS│ Save    │          │ Modify  │                    │
│            │ draft   │          │ prod    │                     │
│            └─────────┘          │ data    │                     │
│                                 └─────────┘                    │
│                                                                 │
│  EXTERNAL  ┌─────────┐          ┌─────────┐                    │
│  COMMS     │  N/A    │          │ Send    │                    │
│            │(always  │          │ emails, │                     │
│            │ review) │          │ API     │                     │
│            └─────────┘          │ calls   │                     │
│                                 └─────────┘                    │
│                                                                 │
│  FINANCIAL │ View    │          │ Any     │                    │
│            │ prices  │          │ trans-  │                     │
│            └─────────┘          │ action  │                     │
│                                 └─────────┘                    │
│                                                                 │
│  DESTRUCTIVE│  N/A   │          │ Delete, │                    │
│            │(always  │          │ revoke, │                     │
│            │ review) │          │ remove  │                     │
│            └─────────┘          └─────────┘                    │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Decision Framework

```python
from enum import Enum
from dataclasses import dataclass
from typing import Optional

class RiskLevel(Enum):
    LOW = "low"
    MEDIUM = "medium"
    HIGH = "high"
    CRITICAL = "critical"

class ActionCategory(Enum):
    READ_PUBLIC = "read_public"
    READ_SENSITIVE = "read_sensitive"
    WRITE_DRAFT = "write_draft"
    WRITE_PERSISTENT = "write_persistent"
    EXTERNAL_COMMUNICATION = "external_communication"
    FINANCIAL = "financial"
    DESTRUCTIVE = "destructive"
    CODE_EXECUTION = "code_execution"

@dataclass
class ApprovalDecision:
    requires_approval: bool
    reason: str
    suggested_approver: Optional[str]
    timeout_minutes: int
    auto_deny_on_timeout: bool

class HITLDecisionEngine:
    """Determine when human approval is required."""
    
    # Actions that ALWAYS require human approval
    ALWAYS_REQUIRE = {
        ActionCategory.EXTERNAL_COMMUNICATION,
        ActionCategory.FINANCIAL,
        ActionCategory.DESTRUCTIVE,
    }
    
    # Risk level thresholds for automatic approval
    AUTO_APPROVE_THRESHOLD = {
        ActionCategory.READ_PUBLIC: RiskLevel.MEDIUM,
        ActionCategory.READ_SENSITIVE: RiskLevel.LOW,
        ActionCategory.WRITE_DRAFT: RiskLevel.MEDIUM,
        ActionCategory.WRITE_PERSISTENT: RiskLevel.LOW,
        ActionCategory.CODE_EXECUTION: RiskLevel.LOW,
    }
    
    def decide(self,
               action_category: ActionCategory,
               risk_level: RiskLevel,
               context: dict) -> ApprovalDecision:
        """Decide if human approval is required."""
        
        # Always require approval for certain categories
        if action_category in self.ALWAYS_REQUIRE:
            return ApprovalDecision(
                requires_approval=True,
                reason=f"{action_category.value} always requires human approval",
                suggested_approver=self._get_approver(action_category, context),
                timeout_minutes=30,
                auto_deny_on_timeout=True
            )
        
        # Check risk threshold
        threshold = self.AUTO_APPROVE_THRESHOLD.get(action_category, RiskLevel.LOW)
        
        if risk_level.value > threshold.value:
            return ApprovalDecision(
                requires_approval=True,
                reason=f"Risk level {risk_level.value} exceeds threshold {threshold.value}",
                suggested_approver=self._get_approver(action_category, context),
                timeout_minutes=15,
                auto_deny_on_timeout=True
            )
        
        # Check for anomalies in context
        if self._detect_anomalies(context):
            return ApprovalDecision(
                requires_approval=True,
                reason="Anomalous behavior detected",
                suggested_approver="security_team",
                timeout_minutes=60,
                auto_deny_on_timeout=True
            )
        
        # Auto-approve
        return ApprovalDecision(
            requires_approval=False,
            reason="Within auto-approval parameters",
            suggested_approver=None,
            timeout_minutes=0,
            auto_deny_on_timeout=False
        )
    
    def _get_approver(self, category: ActionCategory, context: dict) -> str:
        """Determine appropriate approver based on action type."""
        approver_map = {
            ActionCategory.FINANCIAL: "finance_approver",
            ActionCategory.DESTRUCTIVE: "admin_approver",
            ActionCategory.EXTERNAL_COMMUNICATION: "comms_approver",
            ActionCategory.CODE_EXECUTION: "security_team",
        }
        return approver_map.get(category, context.get("default_approver", "manager"))
    
    def _detect_anomalies(self, context: dict) -> bool:
        """Detect anomalous patterns that warrant human review."""
        anomalies = []
        
        # Unusual time of day
        if context.get("hour") and (context["hour"] < 6 or context["hour"] > 22):
            anomalies.append("off_hours")
        
        # High velocity of actions
        if context.get("actions_last_hour", 0) > 50:
            anomalies.append("high_velocity")
        
        # First time for this action type
        if context.get("first_time_action"):
            anomalies.append("novel_action")
        
        # Geographic anomaly
        if context.get("unusual_location"):
            anomalies.append("geo_anomaly")
        
        return len(anomalies) > 0
```

---

## Approval Workflow Design

### Async Approval Pattern

```python
import asyncio
import uuid
from datetime import datetime, timedelta
from typing import Callable, Optional

class ApprovalRequest:
    def __init__(self,
                 action_description: str,
                 action_params: dict,
                 requester: str,
                 timeout_minutes: int = 30):
        
        self.request_id = str(uuid.uuid4())
        self.action_description = action_description
        self.action_params = action_params
        self.requester = requester
        self.created_at = datetime.utcnow()
        self.expires_at = self.created_at + timedelta(minutes=timeout_minutes)
        self.status = "pending"
        self.approver = None
        self.approval_time = None
        self.denial_reason = None

class ApprovalWorkflow:
    """Async approval workflow for HITL controls."""
    
    def __init__(self, 
                 notification_service,
                 storage):
        self.notification_service = notification_service
        self.storage = storage
        self.pending_requests = {}
    
    async def request_approval(self,
                                action: str,
                                params: dict,
                                requester: str,
                                approvers: list[str],
                                timeout_minutes: int = 30,
                                on_approve: Callable = None,
                                on_deny: Callable = None) -> ApprovalRequest:
        """Create an approval request and notify approvers."""
        
        request = ApprovalRequest(
            action_description=action,
            action_params=params,
            requester=requester,
            timeout_minutes=timeout_minutes
        )
        
        # Store request
        self.pending_requests[request.request_id] = {
            "request": request,
            "on_approve": on_approve,
            "on_deny": on_deny
        }
        await self.storage.save_request(request)
        
        # Format approval message
        message = self._format_approval_message(request)
        
        # Notify approvers
        for approver in approvers:
            await self.notification_service.send(
                to=approver,
                subject=f"Approval Required: {action}",
                body=message,
                actions=[
                    {"label": "Approve", "callback": f"/approve/{request.request_id}"},
                    {"label": "Deny", "callback": f"/deny/{request.request_id}"}
                ]
            )
        
        # Schedule timeout check
        asyncio.create_task(self._check_timeout(request.request_id, timeout_minutes))
        
        return request
    
    async def approve(self, 
                      request_id: str, 
                      approver: str,
                      comment: str = None) -> dict:
        """Handle approval."""
        
        if request_id not in self.pending_requests:
            return {"error": "Request not found or already processed"}
        
        entry = self.pending_requests[request_id]
        request = entry["request"]
        
        # Check if expired
        if datetime.utcnow() > request.expires_at:
            return {"error": "Request has expired"}
        
        # Update request
        request.status = "approved"
        request.approver = approver
        request.approval_time = datetime.utcnow()
        
        await self.storage.update_request(request)
        
        # Execute callback
        if entry["on_approve"]:
            await entry["on_approve"](request, approver, comment)
        
        # Cleanup
        del self.pending_requests[request_id]
        
        return {"status": "approved", "request_id": request_id}
    
    async def deny(self,
                   request_id: str,
                   approver: str,
                   reason: str) -> dict:
        """Handle denial."""
        
        if request_id not in self.pending_requests:
            return {"error": "Request not found or already processed"}
        
        entry = self.pending_requests[request_id]
        request = entry["request"]
        
        # Update request
        request.status = "denied"
        request.approver = approver
        request.denial_reason = reason
        
        await self.storage.update_request(request)
        
        # Execute callback
        if entry["on_deny"]:
            await entry["on_deny"](request, approver, reason)
        
        # Cleanup
        del self.pending_requests[request_id]
        
        return {"status": "denied", "request_id": request_id, "reason": reason}
    
    async def _check_timeout(self, request_id: str, timeout_minutes: int):
        """Check for timeout and auto-deny if configured."""
        
        await asyncio.sleep(timeout_minutes * 60)
        
        if request_id in self.pending_requests:
            entry = self.pending_requests[request_id]
            request = entry["request"]
            
            request.status = "timeout"
            await self.storage.update_request(request)
            
            # Auto-deny
            if entry["on_deny"]:
                await entry["on_deny"](request, "system", "Approval timed out")
            
            del self.pending_requests[request_id]
    
    def _format_approval_message(self, request: ApprovalRequest) -> str:
        """Format the approval request for human review."""
        
        # Sanitize params for display
        safe_params = {}
        sensitive_keys = ['password', 'token', 'key', 'secret']
        
        for k, v in request.action_params.items():
            if any(s in k.lower() for s in sensitive_keys):
                safe_params[k] = "****"
            else:
                safe_params[k] = str(v)[:200]  # Truncate long values
        
        return f"""
APPROVAL REQUEST
================

Request ID: {request.request_id}
Requested by: {request.requester}
Time: {request.created_at.isoformat()}
Expires: {request.expires_at.isoformat()}

ACTION: {request.action_description}

PARAMETERS:
{self._format_params(safe_params)}

Please review carefully before approving.
"""
    
    def _format_params(self, params: dict) -> str:
        return "\n".join(f"  - {k}: {v}" for k, v in params.items())
```

---

## Risk Scoring

### Multi-Factor Risk Assessment

```python
from dataclasses import dataclass
from typing import List

@dataclass
class RiskFactor:
    name: str
    score: float  # 0.0 to 1.0
    weight: float
    description: str

class RiskScorer:
    """Calculate risk scores for LLM actions."""
    
    def __init__(self):
        self.factor_weights = {
            "action_severity": 0.25,
            "data_sensitivity": 0.20,
            "user_trust": 0.15,
            "behavioral_anomaly": 0.15,
            "injection_indicators": 0.15,
            "context_validity": 0.10
        }
    
    def calculate_risk(self, context: dict) -> dict:
        """Calculate overall risk score from multiple factors."""
        factors = []
        
        # Factor 1: Action severity
        factors.append(RiskFactor(
            name="action_severity",
            score=self._assess_action_severity(context.get("action")),
            weight=self.factor_weights["action_severity"],
            description="How severe is the requested action"
        ))
        
        # Factor 2: Data sensitivity
        factors.append(RiskFactor(
            name="data_sensitivity",
            score=self._assess_data_sensitivity(context.get("data_accessed")),
            weight=self.factor_weights["data_sensitivity"],
            description="Sensitivity of data involved"
        ))
        
        # Factor 3: User trust level
        factors.append(RiskFactor(
            name="user_trust",
            score=self._assess_user_trust(context.get("user")),
            weight=self.factor_weights["user_trust"],
            description="Trust level of the requesting user"
        ))
        
        # Factor 4: Behavioral anomaly
        factors.append(RiskFactor(
            name="behavioral_anomaly",
            score=self._assess_behavioral_anomaly(context),
            weight=self.factor_weights["behavioral_anomaly"],
            description="Deviation from normal behavior"
        ))
        
        # Factor 5: Injection indicators
        factors.append(RiskFactor(
            name="injection_indicators",
            score=self._assess_injection_indicators(context.get("input_analysis")),
            weight=self.factor_weights["injection_indicators"],
            description="Presence of prompt injection indicators"
        ))
        
        # Factor 6: Context validity
        factors.append(RiskFactor(
            name="context_validity",
            score=self._assess_context_validity(context),
            weight=self.factor_weights["context_validity"],
            description="Validity of the request context"
        ))
        
        # Calculate weighted score
        total_score = sum(f.score * f.weight for f in factors)
        
        # Determine risk level
        if total_score >= 0.8:
            risk_level = "critical"
        elif total_score >= 0.6:
            risk_level = "high"
        elif total_score >= 0.4:
            risk_level = "medium"
        else:
            risk_level = "low"
        
        return {
            "overall_score": total_score,
            "risk_level": risk_level,
            "factors": [
                {
                    "name": f.name,
                    "score": f.score,
                    "weighted_contribution": f.score * f.weight,
                    "description": f.description
                }
                for f in factors
            ],
            "recommendation": self._get_recommendation(risk_level)
        }
    
    def _assess_action_severity(self, action: dict) -> float:
        """Assess severity of the requested action."""
        if not action:
            return 0.0
        
        severity_map = {
            "read_public": 0.1,
            "read_private": 0.4,
            "write_local": 0.3,
            "write_persistent": 0.5,
            "delete": 0.8,
            "send_external": 0.7,
            "financial": 0.9,
            "admin": 0.9
        }
        
        return severity_map.get(action.get("type"), 0.5)
    
    def _assess_data_sensitivity(self, data_accessed: list) -> float:
        """Assess sensitivity of data being accessed."""
        if not data_accessed:
            return 0.0
        
        sensitivity_levels = {
            "public": 0.1,
            "internal": 0.3,
            "confidential": 0.6,
            "secret": 0.8,
            "top_secret": 1.0
        }
        
        max_sensitivity = max(
            sensitivity_levels.get(d.get("classification"), 0.5)
            for d in data_accessed
        )
        
        return max_sensitivity
    
    def _assess_user_trust(self, user: dict) -> float:
        """Assess trust level of the user (inverse - lower trust = higher risk)."""
        if not user:
            return 0.8  # Unknown user is high risk
        
        trust_map = {
            "admin": 0.1,
            "trusted_employee": 0.2,
            "employee": 0.3,
            "contractor": 0.5,
            "external": 0.7,
            "anonymous": 0.9
        }
        
        return trust_map.get(user.get("trust_level"), 0.5)
    
    def _assess_behavioral_anomaly(self, context: dict) -> float:
        """Assess behavioral anomalies."""
        anomaly_score = 0.0
        
        # Unusual time
        hour = context.get("hour")
        if hour and (hour < 6 or hour > 22):
            anomaly_score += 0.2
        
        # High velocity
        if context.get("requests_last_hour", 0) > 100:
            anomaly_score += 0.3
        
        # New behavior pattern
        if context.get("first_time_action"):
            anomaly_score += 0.2
        
        # Failed attempts
        if context.get("recent_failures", 0) > 3:
            anomaly_score += 0.3
        
        return min(anomaly_score, 1.0)
    
    def _assess_injection_indicators(self, analysis: dict) -> float:
        """Assess presence of prompt injection indicators."""
        if not analysis:
            return 0.0
        
        indicators = analysis.get("indicators", [])
        
        if "injection_detected" in indicators:
            return 0.9
        elif "suspicious_patterns" in indicators:
            return 0.6
        elif "encoding_detected" in indicators:
            return 0.4
        
        return 0.0
    
    def _assess_context_validity(self, context: dict) -> float:
        """Assess validity of the request context."""
        invalid_score = 0.0
        
        # Missing required fields
        required = ["user", "session_id", "timestamp"]
        missing = [f for f in required if f not in context]
        invalid_score += len(missing) * 0.2
        
        # Session age
        if context.get("session_age_hours", 0) > 24:
            invalid_score += 0.2
        
        # Referrer mismatch
        if context.get("referrer_valid") is False:
            invalid_score += 0.3
        
        return min(invalid_score, 1.0)
    
    def _get_recommendation(self, risk_level: str) -> str:
        """Get recommendation based on risk level."""
        recommendations = {
            "critical": "BLOCK action and escalate to security team immediately",
            "high": "Require human approval before proceeding",
            "medium": "Proceed with enhanced logging and monitoring",
            "low": "Proceed normally with standard logging"
        }
        return recommendations.get(risk_level, "Proceed with caution")
```

---

## Escalation Patterns

### Tiered Escalation

```python
class EscalationManager:
    """Manage escalation of security events."""
    
    ESCALATION_TIERS = [
        {
            "tier": 1,
            "name": "Automated Response",
            "actions": ["log", "alert_dashboard"],
            "timeout_minutes": 5
        },
        {
            "tier": 2,
            "name": "Security Analyst",
            "actions": ["notify_analyst", "create_ticket"],
            "timeout_minutes": 15
        },
        {
            "tier": 3,
            "name": "Security Lead",
            "actions": ["notify_lead", "page_oncall"],
            "timeout_minutes": 30
        },
        {
            "tier": 4,
            "name": "Incident Response",
            "actions": ["activate_ir", "notify_management"],
            "timeout_minutes": None  # No auto-escalation
        }
    ]
    
    def escalate(self, event: dict, current_tier: int = 1) -> dict:
        """Escalate an event to the appropriate tier."""
        tier_config = self.ESCALATION_TIERS[current_tier - 1]
        
        # Execute tier actions
        for action in tier_config["actions"]:
            self._execute_action(action, event)
        
        # Schedule auto-escalation if configured
        if tier_config["timeout_minutes"] and current_tier < 4:
            self._schedule_auto_escalation(
                event, 
                current_tier + 1,
                tier_config["timeout_minutes"]
            )
        
        return {
            "escalated_to": tier_config["name"],
            "tier": current_tier,
            "actions_taken": tier_config["actions"]
        }
```

---

## Key Takeaways

HITL controls provide the strongest defense but must be applied strategically:

- Too few approvals leave gaps; too many cause approval fatigue
- Risk scoring automates the decision of when to escalate to humans
- Async workflows keep systems responsive while requests await approval
- Tiered escalation ensures right-level response without over-alerting

## Sources

- [OWASP LLM Security](https://owasp.org/www-project-top-10-for-large-language-model-applications/) - Risk classification patterns
- [NIST AI Risk Management Framework](https://www.nist.gov/itl/ai-risk-management-framework) - Human oversight controls

---

[← Back to Index](00_INDEX.md) | [Previous: Agentic Security](15_AGENTIC_SECURITY.md) | [Next: Monitoring & Incident Response →](17_MONITORING_INCIDENT_RESPONSE.md)
