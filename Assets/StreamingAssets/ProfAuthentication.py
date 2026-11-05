 import hashlib
import hmac
import json
import os
import sqlite3
import sys
 
SPECIAL_CHARACTERS = "!@#$%^&*()-_+=<>,.?/:;{}[]|"
 
 
def defaultDatabasePath():
    # default path when unity returns none
    return os.path.join(os.path.dirname(os.path.abspath(__file__)), "credentials.db")
 
 
def connect(databasePath):
    # creates table if none exists
    conn = sqlite3.connect(databasePath)
    conn.execute(
        """CREATE TABLE IF NOT EXISTS users
           (username TEXT PRIMARY KEY, hashedPassword TEXT NOT NULL)"""
    )
    conn.commit()
    return conn
 
 
def userValidate(username):
    return 6 <= len(username) <= 20 and " " not in username
 
 
def passValidate(password):
    return (
        len(password) >= 8
        and any(char.isupper() for char in password)
        and any(char.islower() for char in password)
        and any(char.isdigit() for char in password)
        and any(char in SPECIAL_CHARACTERS for char in password)
    )
 
 
def passHash(password):
    return hashlib.sha256(password.encode("utf-8")).hexdigest()
 
 
def saveInfo(conn, username, hashedPass):
    # the username is the primary key so a duplicate raises error
    try:
        conn.execute(
            "INSERT INTO users (username, hashedPassword) VALUES (?, ?)",
            (username, hashedPass),
        )
        conn.commit()
    except sqlite3.IntegrityError:
        return {"status": "error", "message": "username is already taken"}
 
    return {"status": "success", "message": "signup successful"}
 
 
def checkCredentials(conn, username, password):
    row = conn.execute(
        "SELECT hashedPassword FROM users WHERE username = ?", (username,)
    ).fetchone()
 
    # the same message is returned whether the username or the password was wrong, so the response cannot be used to discover which usernames exist
    
    if row is not None and hmac.compare_digest(passHash(password), row[0]):
        return {"status": "success", "message": "login successful"}
 
    return {"status": "error", "message": "invalid username or password"}
 
 
def respond(payload):
    print(json.dumps(payload))
 
 
def readRequest():
    raw = sys.stdin.read()
 
    if not raw.strip():
        return None
 
    try:
        request = json.loads(raw)
    except json.JSONDecodeError:
        return None
 
    return request if isinstance(request, dict) else None
 
 
def handleSignup(conn, username, password, confirmPassword):
    if not password or not confirmPassword:
        return {"status": "error", "message": "both password fields are required"}
 
    if password != confirmPassword:
        return {"status": "error", "message": "passwords do not match"}
 
    if not userValidate(username):
        return {
            "status": "error",
            "message": "username must be 6 to 20 characters with no spaces",
        }
 
    if not passValidate(password):
        return {
            "status": "error",
            "message": "password needs 8+ characters with an uppercase, a lowercase, a number and a symbol"
        }
 
    return saveInfo(conn, username, passHash(password))
 
 
def handleLogin(conn, username, password):
    if not username or not password:
        return {"status": "error", "message": "Enter a username and password."}
 
    return checkCredentials(conn, username, password)
 
 
def main():
    if len(sys.argv) < 2:
        respond({"status": "error", "message": "No action given."})
        return 1
 
    action = sys.argv[1]
    databasePath = sys.argv[2] if len(sys.argv) > 2 else defaultDatabasePath()
 
    request = readRequest()
    if request is None:
        respond({"status": "error", "message": "no credentials received"})
        return 1
 
    username = (request.get("username") or "").strip()
    password = request.get("password") or ""
    confirmPassword = request.get("confirmPassword") or ""
 
    try:
        conn = connect(databasePath)
    except sqlite3.Error as error:
        print("Database error: " + str(error), file=sys.stderr)
        respond({"status": "error", "message": "could not open the account database"})
        return 1
 
    try:
        if action == "signup":
            respond(handleSignup(conn, username, password, confirmPassword))
        elif action == "login":
            respond(handleLogin(conn, username, password))
        else:
            respond({"status": "error", "message": "invalid action"})
            return 1
    except sqlite3.Error as error:
        print("Database error: " + str(error), file=sys.stderr)
        respond({"status": "error", "message": "something went wrong please try again"})
        return 1
    finally:
        conn.close()
 
    return 0
 
 
if __name__ == "__main__":
    sys.exit(main())
 


