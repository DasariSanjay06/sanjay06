# Malware Detection System

A comprehensive Python-based malware detection and blocking system that analyzes code and text for potential security threats, malicious patterns, and suspicious behaviors.

## Features

### 🛡️ Core Detection Capabilities

- **Signature-based Detection**: Identifies known malware by file hashes and signatures
- **Pattern Matching**: Detects malicious code patterns and suspicious constructs
- **Heuristic Analysis**: Analyzes code behavior and complexity for anomalies
- **AST Analysis**: Examines Abstract Syntax Trees for dangerous code constructs
- **Phishing Detection**: Identifies phishing attempts and social engineering
- **Code Obfuscation Detection**: Recognizes obfuscated and encoded malicious code

### 🔍 Detection Types

1. **Code Patterns**
   - Dynamic code execution (`exec`, `eval`)
   - Base64 encoded execution
   - Shell command injection
   - Network backdoors
   - Keyloggers
   - Cryptominers
   - Data exfiltration attempts

2. **Text Patterns**
   - Phishing keywords
   - Suspicious URLs
   - Ransomware messages
   - Social engineering attempts

3. **Behavioral Analysis**
   - Persistence mechanisms
   - System modifications
   - Network activities
   - File operations

### 🚨 Threat Levels

- **SAFE**: No threats detected
- **LOW**: Minor concerns
- **MEDIUM**: Moderate risk
- **HIGH**: Significant threat
- **CRITICAL**: Severe threat - automatic blocking

## Installation

The core system requires only Python 3.6+ standard library:

```bash
# Clone or download the files
git clone <repository>
cd malware-detection

# No external dependencies required for core functionality
python3 malware_detector.py
```

For optional features, install additional packages:

```bash
pip install -r requirements.txt
```

## Quick Start

### Basic Usage

```python
from malware_detector import MalwareDetector

# Initialize detector
detector = MalwareDetector(enable_blocking=True)

# Review code for security issues
code = """
import os
os.system('ls -la')
"""

result = detector.review_code(code)
print(f"Security Score: {result['security_score']}/100")
print(f"Safe to Execute: {result['safe_to_execute']}")
```

### Safe Code Execution

```python
from malware_detector import MalwareDetector, SafeCodeExecutor

detector = MalwareDetector()
executor = SafeCodeExecutor(detector)

# Only executes if code passes security checks
result = executor.execute_if_safe(user_code, timeout=5)
```

## Examples

Run the comprehensive examples:

```bash
python3 example_usage.py
```

This demonstrates:
1. Basic code review
2. Malicious pattern detection
3. Safe code execution with blocking
4. Code sanitization
5. File scanning
6. Phishing detection
7. Security report generation

## API Reference

### MalwareDetector

Main detection engine class.

#### Methods

- `review_code(code: str, auto_block: bool = True) -> Dict`
  - Reviews code for security issues
  - Returns security score, recommendations, and safety assessment

- `scan_file(filepath: str) -> Dict`
  - Scans a file for malware signatures and patterns

- `perform_comprehensive_scan(content: str, content_type: str) -> Dict`
  - Performs full security scan on content

- `generate_report(output_file: str) -> Dict`
  - Generates detailed JSON report of all scans

### SafeCodeExecutor

Secure code execution environment.

- `execute_if_safe(code: str, timeout: int = 5) -> Dict`
  - Executes code only if it passes security checks
  - Includes timeout protection

### Utility Functions

- `sanitize_code(code: str) -> str`
  - Removes obvious malicious patterns from code

- `quarantine_file(filepath: str, quarantine_dir: str) -> bool`
  - Moves suspicious files to quarantine

## Sample Detection Output

```json
{
  "overall_threat_level": "high",
  "total_detections": 3,
  "security_score": 35,
  "detections": [
    {
      "threat_level": "high",
      "type": "pattern",
      "description": "Shell command execution detected",
      "line": 5,
      "confidence": 0.9,
      "action": "Review and sanitize the code"
    }
  ],
  "blocked": true,
  "recommendations": [
    "⚠️ Security issues detected - review before execution",
    "• Remove shell command execution",
    "🚫 Execution blocked due to high security risk"
  ]
}
```

## Security Patterns Detected

### Critical Threats
- Remote code execution
- Keyloggers
- Ransomware
- Network backdoors
- Cryptominers

### High Risk
- Shell command injection
- System registry modification
- File deletion operations
- Data exfiltration

### Medium Risk
- Dangerous imports
- Obfuscated code
- Phishing attempts
- Suspicious URLs

## Blocking Mechanism

When `enable_blocking=True`, the system automatically blocks:
- Code with HIGH or CRITICAL threat levels
- Known malware signatures
- Dangerous execution patterns

Blocked operations are logged and reported for review.

## Reporting

Generate comprehensive security reports:

```python
detector.generate_report("security_report.json")
```

Reports include:
- Scan summary statistics
- Full detection logs
- Blocked operations
- Timestamps and metadata

## Use Cases

1. **Code Review Automation**: Scan uploaded code for security issues
2. **CI/CD Integration**: Validate code before deployment
3. **Educational Tool**: Learn about malware patterns
4. **Content Moderation**: Filter malicious user submissions
5. **Security Auditing**: Analyze codebases for vulnerabilities

## Limitations

- Pattern-based detection may have false positives
- Cannot detect all zero-day exploits
- Obfuscated code may evade some checks
- Should be used as part of layered security

## Best Practices

1. Always run untrusted code in isolated environments
2. Keep signature database updated
3. Review false positives and adjust patterns
4. Combine with other security tools
5. Regular security audits

## License

This is a demonstration/educational tool. Use responsibly and in compliance with applicable laws.

## Contributing

Feel free to extend the signature database and detection patterns for improved coverage.

## Disclaimer

This tool is for educational and defensive purposes. It demonstrates malware detection concepts and should not be used for malicious purposes. Always test in safe, isolated environments.