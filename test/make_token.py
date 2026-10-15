"""Mint an HS256 dev token (same claims as SyntheticTokenFactory.java).

    python3 make_token.py                  # print a token for support@quillingartist.com
    python3 make_token.py --email x@y.com  # different user
    python3 make_token.py --update         # also write it into postman.txt

The secret defaults to the Development key in appsettings.Development.json; set
JWT_SIGNING_KEY to sign for another environment.
"""
import argparse
import base64
import hashlib
import hmac
import json
import os
import re
import time

POSTMAN_FILE = "postman.txt"
DEV_SECRET = "local-dev-secret-at-least-32-bytes!"


def b64url(data: bytes) -> str:
    return base64.urlsafe_b64encode(data).rstrip(b"=").decode()


def make_token(email, subject, secret, ttl):
    now = int(time.time())
    header = {"alg": "HS256"}
    claims = {
        "aud": "quillingartist",
        "sub": subject,
        "iss": "https://auth.local/quillingartist",
        "exp": now + ttl,
        "iat": now,
        "email": email,
    }
    signing_input = f"{b64url(json.dumps(header).encode())}.{b64url(json.dumps(claims).encode())}"
    signature = hmac.new(secret.encode(), signing_input.encode(), hashlib.sha256).digest()
    return f"{signing_input}.{b64url(signature)}"


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--email", default="support@quillingartist.com")
    parser.add_argument("--subject", default="ADMIN")
    parser.add_argument("--ttl", type=int, default=3600, help="lifetime in seconds")
    parser.add_argument("--update", action="store_true", help=f"replace the tokens in {POSTMAN_FILE}")
    args = parser.parse_args()

    token = make_token(args.email, args.subject, os.environ.get("JWT_SIGNING_KEY", DEV_SECRET), args.ttl)
    print(token)

    if args.update:
        with open(POSTMAN_FILE) as f:
            content = f.read()
        content = re.sub(r'(Authorization:\s+Bearer\s+)\S+(")', rf"\g<1>{token}\g<2>", content)
        with open(POSTMAN_FILE, "w") as f:
            f.write(content)
