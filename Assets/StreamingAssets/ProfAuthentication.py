import hashlib
import hmac
import json
import os
import secrets
import sqlite3
import sys

SPECIAL_CHARACTERS = "!@#$%^&*()-_+=<>,.?/:;{}[]|"

# scrypt settings. n is the work factor, so a higher n makes every guess slower for an attacker
SCRYPT_N = 2 ** 14
SCRYPT_R = 8
SCRYPT_P = 1
SALT_BYTES = 16
HASH_PREFIX = "scrypt"


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


def passHash(password, salt=None):
    # a new random salt per user means two users with the same password get different hashes
    if salt is None:
        salt = secrets.token_bytes(SALT_BYTES)

    # scrypt is deliberately slow and memory hungry, unlike plain sha256, so guessing passwords in bulk is expensive
    hashed = hashlib.scrypt(
        password.encode("utf-8"), salt=salt, n=SCRYPT_N, r=SCRYPT_R, p=SCRYPT_P
    )

    # stored as scrypt$salt$hash so the salt is kept next to the hash it belongs to
    return HASH_PREFIX + "$" + salt.hex() + "$" + hashed.hex()


def legacyHash(password):
    # the unsalted sha256 hash older accounts were saved with
    return hashlib.sha256(password.encode("utf-8")).hexdigest()


def passMatches(password, storedHash):
    parts = storedHash.split("$")

    # current format. hash the attempt with the stored salt and compare in constant time
    if len(parts) == 3 and parts[0] == HASH_PREFIX:
        try:
            salt = bytes.fromhex(parts[1])
        except ValueError:
            return False
        return hmac.compare_digest(passHash(password, salt), storedHash)

    # anything else is an account made before scrypt was added
    return hmac.compare_digest(legacyHash(password), storedHash)


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

    if row is None:
        # hash anyway so an unknown username takes about as long as a wrong password
        passHash(password)
        return {"status": "error", "message": "invalid username or password"}

    if not passMatches(password, row[0]):
        return {"status": "error", "message": "invalid username or password"}

    # an old sha256 account that just logged in correctly is upgraded to scrypt
    if not row[0].startswith(HASH_PREFIX + "$"):
        conn.execute(
            "UPDATE users SET hashedPassword = ? WHERE username = ?",
            (passHash(password), username),
        )
        conn.commit()

    return {"status": "success", "message": "login successful"}


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
