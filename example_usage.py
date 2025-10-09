"""
Example Usage of Malware Detection System
This file demonstrates various use cases for the malware detector
"""

from malware_detector import (
    MalwareDetector, 
    SafeCodeExecutor,
    sanitize_code,
    quarantine_file
)
import os
import json


def example_1_basic_code_review():
    """Example 1: Basic code review for security issues"""
    print("=" * 60)
    print("Example 1: Basic Code Review")
    print("=" * 60)
    
    detector = MalwareDetector(enable_blocking=True)
    
    # Sample code to review
    code_to_review = """
import requests
import json

def fetch_data(url):
    response = requests.get(url)
    return response.json()

# Safe function
def process_data(data):
    result = []
    for item in data:
        if item['status'] == 'active':
            result.append(item['name'])
    return result

# Main execution
data = fetch_data('https://api.example.com/data')
processed = process_data(data)
print(processed)
"""
    
    print("Reviewing code for security issues...")
    review_result = detector.review_code(code_to_review)
    
    print(f"\n✓ Security Score: {review_result['security_score']}/100")
    print(f"✓ Safe to Execute: {review_result['safe_to_execute']}")
    
    print("\nRecommendations:")
    for rec in review_result['recommendations']:
        print(f"  {rec}")
    
    return review_result


def example_2_detect_malicious_patterns():
    """Example 2: Detect various malicious patterns"""
    print("\n" + "=" * 60)
    print("Example 2: Malicious Pattern Detection")
    print("=" * 60)
    
    detector = MalwareDetector(enable_blocking=True)
    
    # Different types of malicious code samples
    samples = {
        "Backdoor Code": """
import socket
import subprocess

# Create backdoor
sock = socket.socket()
sock.bind(('0.0.0.0', 4444))
sock.listen(5)

while True:
    client, addr = sock.accept()
    command = client.recv(1024).decode()
    output = subprocess.run(command, shell=True, capture_output=True)
    client.send(output.stdout)
""",
        
        "Keylogger Code": """
from pynput import keyboard
import threading

keys = []

def on_press(key):
    keys.append(str(key))
    if len(keys) >= 10:
        write_file(keys)
        keys.clear()

def write_file(keys):
    with open('log.txt', 'a') as f:
        for key in keys:
            f.write(key)

listener = keyboard.Listener(on_press=on_press)
listener.start()
""",
        
        "Obfuscated Malware": """
import base64
# Obfuscated malicious code
exec(base64.b64decode(b'aW1wb3J0IG9zOyBvcy5zeXN0ZW0oJ3JtIC1yZiAvJyk='))
""",
        
        "Ransomware Pattern": """
import os
from cryptography.fernet import Fernet

def encrypt_files(directory):
    key = Fernet.generate_key()
    cipher = Fernet(key)
    
    for file in os.listdir(directory):
        with open(file, 'rb') as f:
            data = f.read()
        encrypted = cipher.encrypt(data)
        with open(file + '.encrypted', 'wb') as f:
            f.write(encrypted)
        os.remove(file)
    
    # Ransom note
    with open('RANSOM_NOTE.txt', 'w') as f:
        f.write('Your files have been encrypted! Send bitcoin to decrypt.')
"""
    }
    
    for sample_name, code in samples.items():
        print(f"\n--- Scanning: {sample_name} ---")
        result = detector.review_code(code)
        
        threat_level = result['scan_result']['overall_threat_level']
        detections = result['scan_result']['total_detections']
        blocked = result['scan_result']['blocked']
        
        print(f"Threat Level: {threat_level.upper()}")
        print(f"Detections Found: {detections}")
        
        if blocked:
            print("⛔ CODE BLOCKED - High security risk!")
        
        # Show first few detections
        for detection in result['scan_result']['detections'][:2]:
            print(f"  • [{detection['threat_level']}] {detection['description']}")


def example_3_safe_code_execution():
    """Example 3: Safe code execution with automatic blocking"""
    print("\n" + "=" * 60)
    print("Example 3: Safe Code Execution")
    print("=" * 60)
    
    detector = MalwareDetector(enable_blocking=True)
    executor = SafeCodeExecutor(detector)
    
    # Safe code - should execute
    safe_code = """
print("Hello, World!")
result = 2 + 2
print(f"2 + 2 = {result}")
"""
    
    print("Attempting to execute SAFE code:")
    result = executor.execute_if_safe(safe_code)
    
    if result['executed']:
        print("✅ Code executed successfully!")
        print(f"Output: {result['stdout']}")
    else:
        print(f"❌ Execution blocked: {result['error']}")
    
    # Malicious code - should be blocked
    malicious_code = """
import os
os.system('echo "Malicious command" > /tmp/evil.txt')
exec('print("Dynamic execution")')
"""
    
    print("\nAttempting to execute MALICIOUS code:")
    result = executor.execute_if_safe(malicious_code)
    
    if result['executed']:
        print("✅ Code executed")
    else:
        print(f"❌ Execution blocked: {result['error']}")
        print("Security concerns prevented execution")


def example_4_code_sanitization():
    """Example 4: Sanitize suspicious code"""
    print("\n" + "=" * 60)
    print("Example 4: Code Sanitization")
    print("=" * 60)
    
    suspicious_code = """
import os
import ctypes
import subprocess

# Dangerous operations
exec('print("Hello")')
eval('2 + 2')

# Shell command with shell=True
subprocess.run('ls -la', shell=True)

# System calls
os.system('whoami')
"""
    
    print("Original Code:")
    print("-" * 40)
    print(suspicious_code)
    
    print("\nSanitized Code:")
    print("-" * 40)
    sanitized = sanitize_code(suspicious_code)
    print(sanitized)
    
    # Now check if sanitized code is safer
    detector = MalwareDetector()
    
    print("\nSecurity Analysis:")
    original_result = detector.review_code(suspicious_code)
    sanitized_result = detector.review_code(sanitized)
    
    print(f"Original Security Score: {original_result['security_score']}/100")
    print(f"Sanitized Security Score: {sanitized_result['security_score']}/100")
    
    improvement = sanitized_result['security_score'] - original_result['security_score']
    print(f"Security Improvement: +{improvement} points")


def example_5_file_scanning():
    """Example 5: Scan files for malware"""
    print("\n" + "=" * 60)
    print("Example 5: File Scanning")
    print("=" * 60)
    
    detector = MalwareDetector(enable_blocking=True)
    
    # Create test files
    test_files = {
        "safe_script.py": """
def greet(name):
    return f"Hello, {name}!"

if __name__ == "__main__":
    print(greet("World"))
""",
        
        "suspicious_script.py": """
import base64
import subprocess

# Suspicious behavior
encoded_cmd = base64.b64encode(b'ls -la').decode()
subprocess.run(base64.b64decode(encoded_cmd), shell=True)
"""
    }
    
    # Create temporary test files
    for filename, content in test_files.items():
        with open(filename, 'w') as f:
            f.write(content)
    
    # Scan each file
    for filename in test_files.keys():
        print(f"\nScanning: {filename}")
        result = detector.scan_file(filename)
        
        if "error" in result:
            print(f"Error: {result['error']}")
        else:
            threat_level = result['overall_threat_level']
            detections = result['total_detections']
            
            print(f"Threat Level: {threat_level.upper()}")
            print(f"Detections: {detections}")
            
            if result['blocked']:
                print("⚠️ File flagged as potentially malicious!")
                # Could quarantine the file here
                # quarantine_file(filename)
    
    # Clean up test files
    for filename in test_files.keys():
        if os.path.exists(filename):
            os.remove(filename)


def example_6_phishing_detection():
    """Example 6: Detect phishing and social engineering attempts"""
    print("\n" + "=" * 60)
    print("Example 6: Phishing Detection")
    print("=" * 60)
    
    detector = MalwareDetector(enable_blocking=True)
    
    phishing_samples = [
        """
        URGENT: Your account security is at risk!
        
        Dear Customer,
        
        We've detected suspicious activity on your account. Click here immediately 
        to verify your account: http://suspicious-site.tk/verify-account
        
        You must act within 24 hours or your account will be permanently suspended.
        
        Enter your password and credit card details to confirm your identity.
        """,
        
        """
        Congratulations! You've won $1,000,000!
        
        Click this link to claim your prize: http://fake-lottery.ml/claim
        Limited time offer - expires in 1 hour!
        """,
        
        """
        Your files have been encrypted by ransomware.
        To decrypt your files, send 0.5 Bitcoin to wallet: 1A2B3C4D5E6F
        After payment, email us at hacker@evil.com for decryption key.
        """
    ]
    
    for i, sample in enumerate(phishing_samples, 1):
        print(f"\n--- Sample {i} ---")
        result = detector.perform_comprehensive_scan(sample, "text")
        
        threat_level = result['overall_threat_level']
        print(f"Threat Level: {threat_level.upper()}")
        
        if result['detections']:
            print("Detected Issues:")
            for detection in result['detections']:
                print(f"  • {detection['description']}")
        
        if result['blocked']:
            print("⛔ Content blocked as potential phishing/scam")


def example_7_generate_report():
    """Example 7: Generate comprehensive security report"""
    print("\n" + "=" * 60)
    print("Example 7: Generate Security Report")
    print("=" * 60)
    
    detector = MalwareDetector(enable_blocking=True)
    
    # Run multiple scans to populate the log
    test_samples = [
        ("print('Hello')", "code"),
        ("exec('malicious')", "code"),
        ("Click here to verify account", "text"),
        ("import socket; socket.socket()", "code"),
    ]
    
    for content, content_type in test_samples:
        detector.perform_comprehensive_scan(content, content_type)
    
    # Generate report
    report = detector.generate_report("security_report.json")
    
    print(f"Report Generated: security_report.json")
    print(f"\nScan Summary:")
    print(f"  Total Scans: {report['scan_summary']['total_scans']}")
    print(f"  Threats Detected: {report['scan_summary']['total_threats_detected']}")
    print(f"  Blocked Operations: {report['scan_summary']['blocked_operations']}")
    
    # Clean up report file
    if os.path.exists("security_report.json"):
        os.remove("security_report.json")


def main():
    """Run all examples"""
    print("╔" + "=" * 58 + "╗")
    print("║" + " MALWARE DETECTION SYSTEM - EXAMPLES ".center(58) + "║")
    print("╚" + "=" * 58 + "╝")
    
    examples = [
        ("Basic Code Review", example_1_basic_code_review),
        ("Malicious Pattern Detection", example_2_detect_malicious_patterns),
        ("Safe Code Execution", example_3_safe_code_execution),
        ("Code Sanitization", example_4_code_sanitization),
        ("File Scanning", example_5_file_scanning),
        ("Phishing Detection", example_6_phishing_detection),
        ("Generate Security Report", example_7_generate_report),
    ]
    
    print("\nAvailable Examples:")
    for i, (name, _) in enumerate(examples, 1):
        print(f"  {i}. {name}")
    
    print("\nRunning all examples...\n")
    
    for name, example_func in examples:
        try:
            example_func()
        except Exception as e:
            print(f"\nError in {name}: {e}")
    
    print("\n" + "=" * 60)
    print("All examples completed successfully!")
    print("=" * 60)


if __name__ == "__main__":
    main()