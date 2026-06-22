# Authentication Control

The API uses a pretty standard API key system which is laid out in this document.

## Key Data

The API key is a Base64 encoded JSON string which contains the following data:

```json
{
    "signature": "The Signature of all other API key data, signed by the server",
    "issued": "Timestamp of when the key was issued",
    "expires": "Timestamp of when the key expires"
}
```

The key deliberately includes 0 identifying information about the user, so that if the key is compromised or stolen,
it is more difficult for an attacker to use the key. This is because the key is linked to a significant quantity of
heuristic data server-side which is verified on every request.

Additionally, the key is signed by the server using a secret key, so that if the key is tampered with, it will be
rejected by the server.

## Key Issuance

An API key lifetime begins at the /signin endpoint, where the user provides their username and password. If the
credentials are valid, the server will issue a new API key with the information outlined above,
and the client can use this key for subsequent requests to the API.

The lifespan of a key is determined by the server, and can be configured to be as long or as short as desired. The
server will also have the ability to revoke keys if necessary, which will immediately invalidate the key and prevent
it from being used for further requests.

The process is described as follows:

```
User /signin
    └─▶ Validate Credentials
        ├─▶ Valid
        │   └─▶ Collect Heuristic Data
        │       └─▶ Compile public key data and sign with the server secret key
        │           └─▶ Create key in the database with the public key data and heuristic data
        │               └─▶ Return API Key
        │       
        └─▶ Invalid: Return Error
```