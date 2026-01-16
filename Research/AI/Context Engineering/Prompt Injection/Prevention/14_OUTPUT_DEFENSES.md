# Output Validation and Defenses

[← Previous: Prompt Design Patterns](13_PROMPT_DESIGN_PATTERNS.md) | [Index](00_INDEX.md) | [Next: Agentic Security →](15_AGENTIC_SECURITY.md)

---

## Overview

Output validation catches attacks that bypass input filters. Validates responses before they reach users or downstream systems by detecting injection success, data leakage, and format violations.

## Summary

- **Five-stage pipeline**: injection detection, prompt leakage, sensitive data, schema validation, content sanitization
- **Injection detection**: pattern matching for role changes, instruction acknowledgment, jailbreak markers
- **Leakage prevention**: canary tokens, phrase matching, similarity scoring against system prompts
- **Data protection**: PII/credential regex patterns, automatic redaction
- **Format enforcement**: JSON schema validation, markdown/URL sanitization

---

## Output Defense Pipeline

```
┌─────────────────────────────────────────────────────────────────┐
│              OUTPUT DEFENSE PIPELINE                             │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  LLM Response                                                   │
│      │                                                          │
│      ▼                                                          │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  STAGE 1: INJECTION SUCCESS DETECTION                    │   │
│  │  • Check for "I will now ignore..." patterns            │   │
│  │  • Detect unexpected role changes                       │   │
│  │  • Identify goal hijacking indicators                   │   │
│  └─────────────────────────────────────────────────────────┘   │
│      │                                                          │
│      ▼                                                          │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  STAGE 2: PROMPT LEAKAGE DETECTION                       │   │
│  │  • Search for system prompt fragments                   │   │
│  │  • Check for canary token presence                      │   │
│  │  • Detect configuration exposure                        │   │
│  └─────────────────────────────────────────────────────────┘   │
│      │                                                          │
│      ▼                                                          │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  STAGE 3: SENSITIVE DATA DETECTION                       │   │
│  │  • PII patterns (SSN, credit cards, etc.)               │   │
│  │  • Credentials and secrets                              │   │
│  │  • Internal data markers                                │   │
│  └─────────────────────────────────────────────────────────┘   │
│      │                                                          │
│      ▼                                                          │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  STAGE 4: SCHEMA/FORMAT VALIDATION                       │   │
│  │  • JSON schema compliance                               │   │
│  │  • Expected field presence                              │   │
│  │  • Type checking                                        │   │
│  └─────────────────────────────────────────────────────────┘   │
│      │                                                          │
│      ▼                                                          │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  STAGE 5: CONTENT SAFETY                                 │   │
│  │  • Toxicity check                                       │   │
│  │  • Harmful content detection                            │   │
│  │  • Markdown/URL sanitization                            │   │
│  └─────────────────────────────────────────────────────────┘   │
│      │                                                          │
│      ▼                                                          │
│  Validated Output                                               │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## Injection Success Detection

Pattern matching identifies when LLM output indicates successful prompt injection:

```python
import re
from typing import List, Tuple

class InjectionSuccessDetector:
    """Detect patterns indicating successful prompt injection."""

    # Model following injected instructions
    SUCCESS_PATTERNS = [
        # Acknowledgment of new instructions
        (r'(okay|sure|alright),?\s*(i\'ll|i\s+will)\s+(now\s+)?(ignore|disregard)', 'instruction_acknowledgment'),
        (r'understood[.,]?\s*(i\'ll|i\s+will|my\s+new)', 'instruction_acknowledgment'),
        (r'as\s+(you|per\s+your)\s+(requested|instructed|asked)', 'following_injected_request'),
        
        # Role change indicators
        (r'i\s+am\s+now\s+(acting\s+as|pretending|in)\s+\w+\s+mode', 'role_change'),
        (r'(entering|switched\s+to|activating)\s+\w+\s+mode', 'role_change'),
        (r'my\s+new\s+(role|persona|identity)\s+is', 'role_change'),
        
        # System prompt revelation markers
        (r'(my|the)\s+(system\s+)?prompt\s+(is|says|contains|reads)', 'prompt_revelation'),
        (r'(here\s+are|these\s+are)\s+my\s+(instructions|rules|guidelines)', 'prompt_revelation'),
        (r'i\s+was\s+(instructed|told|programmed)\s+to', 'prompt_revelation'),
        
        # Jailbreak success indicators
        (r'(sure|okay)[,!]?\s*(here\'s|let\s+me)\s+\w+\s+(harmful|dangerous|illegal)', 'jailbreak_success'),
        (r'i\'ll\s+help\s+(you\s+)?(with\s+that|bypass|hack|exploit)', 'jailbreak_success'),
        
        # Unexpected capability claims
        (r'i\s+(can\s+now|now\s+have)\s+(access|ability|permission)\s+to', 'capability_escalation'),
        (r'(restrictions|limitations|safeguards)\s+(removed|disabled|lifted)', 'capability_escalation'),
    ]
    
    # Context patterns (suspicious when appearing in certain contexts)
    CONTEXT_PATTERNS = [
        (r'^(DAN|STAN|DUDE|Developer\s+Mode):', 'jailbreak_persona'),
        (r'\[.*?(evil|unfiltered|unrestricted).*?\]', 'jailbreak_marker'),
    ]
    
    def detect(self, response: str) -> Tuple[bool, List[dict]]:
        """Returns (is_suspicious, list of matched patterns)."""
        response_lower = response.lower()
        matches = []
        
        # Check success patterns
        for pattern, category in self.SUCCESS_PATTERNS:
            if re.search(pattern, response_lower):
                matches.append({
                    "category": category,
                    "pattern": pattern,
                    "severity": self._get_severity(category)
                })
        
        # Check context patterns
        for pattern, category in self.CONTEXT_PATTERNS:
            if re.search(pattern, response, re.IGNORECASE):
                matches.append({
                    "category": category,
                    "pattern": pattern,
                    "severity": "high"
                })
        
        is_suspicious = len(matches) > 0
        return is_suspicious, matches
    
    def _get_severity(self, category: str) -> str:
        severity_map = {
            "instruction_acknowledgment": "high",
            "role_change": "high",
            "prompt_revelation": "critical",
            "jailbreak_success": "critical",
            "capability_escalation": "critical",
            "following_injected_request": "medium",
        }
        return severity_map.get(category, "medium")
```

## Prompt Leakage Detection

Identifies system prompt exposure through canary tokens, phrase matching, and similarity analysis:

```python
from difflib import SequenceMatcher
import hashlib

class PromptLeakageDetector:
    """Detect system prompt leakage in LLM responses."""
    
    def __init__(self, system_prompt: str, canary_tokens: List[str] = None):
        self.system_prompt = system_prompt
        self.canary_tokens = canary_tokens or []
        
        # Extract distinctive phrases from system prompt
        self.prompt_phrases = self._extract_phrases(system_prompt)
        
        # Create hash of sensitive sections
        self.prompt_hash = hashlib.sha256(system_prompt.encode()).hexdigest()[:16]
    
    def _extract_phrases(self, text: str, min_length: int = 20) -> List[str]:
        """Extract distinctive phrases from system prompt (min 20 chars)."""
        phrases = []
        
        # Split by sentences and lines
        for line in text.split('\n'):
            line = line.strip()
            if len(line) >= min_length:
                phrases.append(line)
        
        return phrases
    
    def detect(self, response: str) -> dict:
        """Detect prompt leakage in response."""
        results = {
            "leakage_detected": False,
            "canary_leaked": False,
            "phrase_matches": [],
            "similarity_score": 0.0,
            "severity": "none"
        }
        
        # Check for canary tokens
        for canary in self.canary_tokens:
            if canary in response:
                results["leakage_detected"] = True
                results["canary_leaked"] = True
                results["severity"] = "critical"
                return results
        
        # Check for exact phrase matches
        response_lower = response.lower()
        for phrase in self.prompt_phrases:
            phrase_lower = phrase.lower()
            if phrase_lower in response_lower and len(phrase) > 30:
                results["phrase_matches"].append(phrase[:50] + "...")
                results["leakage_detected"] = True
        
        # Fuzzy matching for paraphrased leakage
        similarity = self._calculate_similarity(response)
        results["similarity_score"] = similarity
        
        if similarity > 0.4:  # High similarity to system prompt
            results["leakage_detected"] = True
        
        # Determine severity
        if results["phrase_matches"]:
            results["severity"] = "high"
        elif similarity > 0.3:
            results["severity"] = "medium"
        
        return results
    
    def _calculate_similarity(self, response: str) -> float:
        """Calculate similarity between response and system prompt."""
        matcher = SequenceMatcher(None, 
                                   self.system_prompt.lower(), 
                                   response.lower())
        return matcher.ratio()
```

## Sensitive Data Detection

Regex patterns detect and redact PII and credentials:

```python
import re
from typing import List, Dict

class SensitiveDataDetector:
    """Detect sensitive data patterns in LLM outputs."""
    
    PII_PATTERNS = {
        # US Social Security Number
        "ssn": r'\b\d{3}-\d{2}-\d{4}\b',
        
        # Credit Card Numbers (basic patterns)
        "credit_card_visa": r'\b4\d{3}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}\b',
        "credit_card_mc": r'\b5[1-5]\d{2}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}\b',
        "credit_card_amex": r'\b3[47]\d{2}[\s-]?\d{6}[\s-]?\d{5}\b',
        
        # Email addresses
        "email": r'\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b',
        
        # Phone numbers (US)
        "phone_us": r'\b(\+1[\s.-]?)?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}\b',
        
        # IP Addresses
        "ip_address": r'\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b',
        
        # Date of Birth patterns
        "dob": r'\b(0[1-9]|1[0-2])/(0[1-9]|[12]\d|3[01])/(\d{4})\b',
    }
    
    SECRET_PATTERNS = {
        # API Keys (generic patterns)
        "api_key_generic": r'\b[A-Za-z0-9]{32,}\b',
        "api_key_bearer": r'Bearer\s+[A-Za-z0-9\-._~+/]+=*',
        
        # AWS credentials
        "aws_access_key": r'AKIA[0-9A-Z]{16}',
        "aws_secret_key": r'[A-Za-z0-9/+=]{40}',
        
        # GitHub tokens
        "github_token": r'gh[pousr]_[A-Za-z0-9_]{36,}',
        
        # Generic passwords in context
        "password_context": r'password["\']?\s*[=:]\s*["\']?[^\s"\']{8,}',
        
        # Private keys
        "private_key": r'-----BEGIN\s+(?:RSA\s+)?PRIVATE\s+KEY-----',
        
        # Connection strings
        "connection_string": r'(?:mongodb|mysql|postgresql|redis)://[^\s]+',
    }
    
    def __init__(self, custom_patterns: Dict[str, str] = None):
        self.patterns = {**self.PII_PATTERNS, **self.SECRET_PATTERNS}
        if custom_patterns:
            self.patterns.update(custom_patterns)
    
    def detect(self, text: str) -> Dict:
        """Scan text for sensitive data patterns."""
        findings = {
            "contains_sensitive": False,
            "pii_found": [],
            "secrets_found": [],
            "all_matches": [],
            "severity": "none"
        }
        
        for pattern_name, pattern in self.patterns.items():
            matches = re.findall(pattern, text, re.IGNORECASE)
            
            if matches:
                findings["contains_sensitive"] = True
                
                match_info = {
                    "type": pattern_name,
                    "count": len(matches),
                    "samples": [self._redact(m) for m in matches[:3]]
                }
                
                findings["all_matches"].append(match_info)
                
                if pattern_name in self.PII_PATTERNS:
                    findings["pii_found"].append(match_info)
                else:
                    findings["secrets_found"].append(match_info)
        
        # Calculate severity
        if findings["secrets_found"]:
            findings["severity"] = "critical"
        elif findings["pii_found"]:
            findings["severity"] = "high"
        
        return findings
    
    def _redact(self, value: str) -> str:
        """Redact sensitive value for logging."""
        if isinstance(value, tuple):
            value = value[0]
        if len(value) > 8:
            return value[:4] + "****" + value[-4:]
        return "****"
    
    def redact_all(self, text: str) -> str:
        """Redact all sensitive data from text."""
        redacted = text
        
        for pattern_name, pattern in self.patterns.items():
            redacted = re.sub(
                pattern, 
                f"[REDACTED:{pattern_name.upper()}]", 
                redacted,
                flags=re.IGNORECASE
            )
        
        return redacted
```

## Schema Validation

Pydantic models enforce expected JSON structure and detect injection markers in fields:

```python
from pydantic import BaseModel, ValidationError, validator
from typing import Optional, List, Any
import json

class OutputSchemaValidator:
    """Validate LLM outputs against expected schemas."""

    def __init__(self, schema_class: type):
        """Initialize with a Pydantic model class."""
        self.schema_class = schema_class
    
    def validate(self, output: str) -> dict:
        """Validate output against schema."""
        result = {
            "valid": False,
            "parsed": None,
            "errors": [],
            "raw_output": output
        }
        
        # Try to parse as JSON
        try:
            parsed_json = json.loads(output)
        except json.JSONDecodeError as e:
            # Try to extract JSON from text
            parsed_json = self._extract_json(output)
            if parsed_json is None:
                result["errors"].append(f"JSON parse error: {e}")
                return result
        
        # Validate against schema
        try:
            validated = self.schema_class(**parsed_json)
            result["valid"] = True
            result["parsed"] = validated.dict()
        except ValidationError as e:
            result["errors"] = [str(err) for err in e.errors()]
        
        return result
    
    def _extract_json(self, text: str) -> Optional[dict]:
        """Try to extract JSON from text with other content."""
        # Look for JSON-like patterns
        import re
        
        # Try to find JSON object
        json_pattern = r'\{[^{}]*\}'
        matches = re.findall(json_pattern, text, re.DOTALL)
        
        for match in matches:
            try:
                return json.loads(match)
            except:
                continue
        
        return None


# Example schema definitions
class CustomerServiceResponse(BaseModel):
    """Expected schema for customer service responses."""
    response_text: str
    confidence: float
    sources: List[str] = []
    escalate_to_human: bool = False
    
    @validator('confidence')
    def confidence_range(cls, v):
        if not 0 <= v <= 1:
            raise ValueError('Confidence must be between 0 and 1')
        return v
    
    @validator('response_text')
    def no_injection_markers(cls, v):
        suspicious = ['system prompt', 'my instructions', 'ignore previous']
        if any(s in v.lower() for s in suspicious):
            raise ValueError('Response contains suspicious content')
        return v


class CodeAnalysisResponse(BaseModel):
    """Expected schema for code analysis responses."""
    analysis: str
    vulnerabilities: List[dict] = []
    suggestions: List[str] = []
    safe_to_execute: bool = False
```

## URL and Markdown Sanitization

Removes or validates images, links, and data URLs to prevent exfiltration via rendered content:

```python
import re
from urllib.parse import urlparse
from typing import Set

class MarkdownSanitizer:
    """Sanitize markdown to prevent data exfiltration."""

    # Allowlisted domains
    DEFAULT_ALLOWED_DOMAINS: Set[str] = {
        'example.com',
        'cdn.example.com',
        'images.example.com',
    }
    
    def __init__(self, allowed_domains: Set[str] = None):
        self.allowed_domains = allowed_domains or self.DEFAULT_ALLOWED_DOMAINS
    
    def sanitize(self, markdown: str) -> dict:
        """Sanitize markdown content."""
        result = {
            "sanitized": markdown,
            "removed_images": [],
            "removed_links": [],
            "warnings": []
        }
        
        # Remove/sanitize images
        result["sanitized"], result["removed_images"] = self._sanitize_images(
            result["sanitized"]
        )
        
        # Remove/sanitize suspicious links
        result["sanitized"], result["removed_links"] = self._sanitize_links(
            result["sanitized"]
        )
        
        # Remove HTML tags
        result["sanitized"] = self._remove_html(result["sanitized"])
        
        # Check for data URL schemes
        result["sanitized"], data_urls = self._remove_data_urls(
            result["sanitized"]
        )
        if data_urls:
            result["warnings"].append(f"Removed {len(data_urls)} data URLs")
        
        return result
    
    def _sanitize_images(self, text: str) -> tuple:
        """Remove or sanitize image markdown."""
        removed = []
        
        # Match markdown images: ![alt](url)
        image_pattern = r'!\[([^\]]*)\]\(([^)]+)\)'
        
        def replace_image(match):
            alt = match.group(1)
            url = match.group(2)
            
            # Parse URL
            try:
                parsed = urlparse(url)
                domain = parsed.netloc
                
                if domain in self.allowed_domains:
                    return match.group(0)  # Keep allowed images
                else:
                    removed.append({"alt": alt, "url": url})
                    return f"[Image removed: {alt}]"
            except:
                removed.append({"alt": alt, "url": url})
                return f"[Image removed: {alt}]"
        
        sanitized = re.sub(image_pattern, replace_image, text)
        return sanitized, removed
    
    def _sanitize_links(self, text: str) -> tuple:
        """Sanitize suspicious links."""
        removed = []
        
        # Match markdown links: [text](url)
        link_pattern = r'\[([^\]]+)\]\(([^)]+)\)'
        
        def replace_link(match):
            text = match.group(1)
            url = match.group(2)
            
            # Check for data exfiltration patterns
            suspicious_patterns = [
                r'[?&]data=',
                r'[?&]q=.*\{',  # Query with curly braces (possible template)
                r'[?&]callback=',
                r'javascript:',
                r'data:',
            ]
            
            for pattern in suspicious_patterns:
                if re.search(pattern, url, re.IGNORECASE):
                    removed.append({"text": text, "url": url, "reason": pattern})
                    return f"[Link removed: {text}]"
            
            return match.group(0)
        
        sanitized = re.sub(link_pattern, replace_link, text)
        return sanitized, removed
    
    def _remove_html(self, text: str) -> str:
        """Remove HTML tags."""
        return re.sub(r'<[^>]+>', '', text)
    
    def _remove_data_urls(self, text: str) -> tuple:
        """Remove data: URL schemes."""
        data_url_pattern = r'data:[^,]*,[^\s\)\"\']*'
        matches = re.findall(data_url_pattern, text)
        sanitized = re.sub(data_url_pattern, '[DATA_URL_REMOVED]', text)
        return sanitized, matches
```

## Complete Output Validator

Orchestrates all validation stages in a single pipeline:

```python
class ComprehensiveOutputValidator:
    """Complete output validation pipeline."""
    
    def __init__(self, 
                 system_prompt: str,
                 canary_tokens: List[str] = None,
                 schema_class: type = None,
                 allowed_domains: Set[str] = None):
        
        self.injection_detector = InjectionSuccessDetector()
        self.leakage_detector = PromptLeakageDetector(system_prompt, canary_tokens)
        self.sensitive_detector = SensitiveDataDetector()
        self.markdown_sanitizer = MarkdownSanitizer(allowed_domains)
        
        self.schema_validator = None
        if schema_class:
            self.schema_validator = OutputSchemaValidator(schema_class)
    
    def validate(self, response: str) -> dict:
        """Run complete validation pipeline."""
        results = {
            "passed": True,
            "sanitized_output": response,
            "stages": {},
            "risk_level": "low",
            "block_reason": None
        }
        
        # Stage 1: Injection success detection
        is_suspicious, injection_matches = self.injection_detector.detect(response)
        results["stages"]["injection_detection"] = {
            "suspicious": is_suspicious,
            "matches": injection_matches
        }
        if is_suspicious:
            results["passed"] = False
            results["risk_level"] = "critical"
            results["block_reason"] = "Injection success detected"
        
        # Stage 2: Prompt leakage detection
        leakage_result = self.leakage_detector.detect(response)
        results["stages"]["leakage_detection"] = leakage_result
        if leakage_result["leakage_detected"]:
            results["passed"] = False
            results["risk_level"] = "critical"
            results["block_reason"] = "Prompt leakage detected"
        
        # Stage 3: Sensitive data detection
        sensitive_result = self.sensitive_detector.detect(response)
        results["stages"]["sensitive_data"] = sensitive_result
        if sensitive_result["contains_sensitive"]:
            # Redact rather than block
            results["sanitized_output"] = self.sensitive_detector.redact_all(
                results["sanitized_output"]
            )
            if sensitive_result["severity"] == "critical":
                results["passed"] = False
                results["block_reason"] = "Critical sensitive data exposure"
        
        # Stage 4: Schema validation (if configured)
        if self.schema_validator:
            schema_result = self.schema_validator.validate(response)
            results["stages"]["schema_validation"] = schema_result
            if not schema_result["valid"]:
                results["risk_level"] = max(results["risk_level"], "medium")
        
        # Stage 5: Markdown sanitization
        sanitize_result = self.markdown_sanitizer.sanitize(results["sanitized_output"])
        results["stages"]["markdown_sanitization"] = sanitize_result
        results["sanitized_output"] = sanitize_result["sanitized"]
        
        return results
```

## Usage Example

```python
# Initialize validator
validator = ComprehensiveOutputValidator(
    system_prompt=system_prompt,
    canary_tokens=["CANARY_ABC123"],
    schema_class=CustomerServiceResponse,
    allowed_domains={"cdn.mycompany.com"}
)

# Validate LLM response
response = llm.generate(prompt)
validation = validator.validate(response)

if validation["passed"]:
    # Safe to return to user
    return validation["sanitized_output"]
else:
    # Log and handle the failure
    logger.warning(f"Output blocked: {validation['block_reason']}")
    logger.warning(f"Risk level: {validation['risk_level']}")
    
    # Return safe fallback
    return "I apologize, but I cannot provide that response."
```

---

## Key Takeaways

Output validation catches what input filters miss. Implement all five stages for defense-in-depth: injection detection blocks hijacked responses, leakage prevention protects system prompts, sensitive data detection prevents credential exposure, schema validation enforces structure, and markdown sanitization stops exfiltration via rendered content. Always combine with input validation and prompt design patterns.

## Sources

- [OWASP LLM Top 10](https://owasp.org/www-project-top-10-for-large-language-model-applications/) - LLM security guidelines
- [Pydantic Documentation](https://docs.pydantic.dev/) - Schema validation framework
- [Python regex patterns](https://docs.python.org/3/library/re.html) - Pattern matching reference

---

[← Previous: Prompt Design Patterns](13_PROMPT_DESIGN_PATTERNS.md) | [Index](00_INDEX.md) | [Next: Agentic Security →](15_AGENTIC_SECURITY.md)
