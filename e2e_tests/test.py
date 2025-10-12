import os
import uuid
import requests

BASE_URL = os.getenv("INTEGRATION_BASE_URL", "http://localhost:8080").rstrip("/")
API_KEY_HEADER = os.getenv("ApiKey__HeaderName", "")
API_KEY_VALUE = os.getenv("ApiKey__Key", "")
def api(method, path, **kwargs):
    headers = kwargs.pop("headers", {})
    headers["Accept"] = "application/json"
    if "json" in kwargs:
        headers["Content-Type"] = "application/json"
    return requests.request(method, f"{BASE_URL}{path}", headers=headers, timeout=10, **kwargs)

def test_auth_flow():
    email = f"py_it_{uuid.uuid4().hex[:12]}@mail.com"
    password = "Password1!"

    # 1) Register
    r = api("POST", "/api/auth/register", json={"email": email, "password": password})
    r.raise_for_status()
    tokens = r.json()
    assert tokens.get("accessToken") and tokens.get("refreshToken")

    # 2) Login
    r = api("POST", "/api/auth/login", json={"email": email, "password": password})
    r.raise_for_status()
    tokens = r.json()

    # 3) Validate
    r = api("POST", "/api/auth/validate", json={"token": tokens["accessToken"]})
    r.raise_for_status()

    # 4) Refresh
    r = api("POST", "/api/auth/refresh", json={"refreshToken": tokens["refreshToken"]})
    r.raise_for_status()

    # 5) UserExist
    r = api("GET", f"/api/user/UserExist?email={email}")
    r.raise_for_status()

    # 6) DeleteUser
    r = api("DELETE", f"/api/user/DeleteUser?email={email}")
    assert r.status_code == 204, f"Expected 204, got {r.status_code}"

    # 7) Ensure not exists
    r = api("GET", f"/api/user/UserExist?email={email}")
    assert r.status_code == 400

if __name__ == "__main__":
    test_auth_flow()
    print("OK")