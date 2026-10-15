import re
import urllib.request
import urllib.error
from datetime import datetime

POSTMAN_FILE = "postman.txt"
RESULT_FILE = "postman_result.txt"

curl_pattern = re.compile(
    r'#\s*(\S+)\s*\ncurl\s+-H\s+"Authorization:\s+Bearer\s+(\S+)"\s+(http\S+)'
)

with open(POSTMAN_FILE) as f:
    content = f.read()

tests = curl_pattern.findall(content)

results = []
results.append(f"Test run: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
results.append(f"Total tests: {len(tests)}")
results.append("")

passed = 0
failed = 0

for name, token, url in tests:
    req = urllib.request.Request(url, headers={"Authorization": f"Bearer {token}"})
    try:
        with urllib.request.urlopen(req) as resp:
            status = resp.status
    except urllib.error.HTTPError as e:
        status = e.code
    except urllib.error.URLError as e:
        status = f"ERROR: {e.reason}"

    if status == 200:
        result = "PASSED"
        passed += 1
    else:
        result = "FAILED"
        failed += 1

    line = f"{result} [{status}] {name}: {url}"
    results.append(line)
    print(line)

results.append("")
results.append(f"Passed: {passed}  Failed: {failed}")

with open(RESULT_FILE, "w") as f:
    f.write("\n".join(results) + "\n")

print(f"\nPassed: {passed}  Failed: {failed}")
print(f"Results saved to {RESULT_FILE}")
