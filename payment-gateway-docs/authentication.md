# Authentication

All API endpoints (except authentication) require a JWT token obtained via the login endpoint.

## Obtaining a Token
bash
curl -X 'POST' \
'http://localhost:8080/api/v1/auth/login' \
-H 'accept: /' \
-H 'Content-Type: application/json' \
-d '{
"apiKey": "test_shop_key_456",
"apiSecret": "test_shop_secret_abc"
}'
**Response:**
json
{
"token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
"expiresAt": "2023-06-01T12:00:00Z"
}

## Test Accounts

The system is pre-seeded with the following test merchant accounts:

| Merchant Name | API Key | API Secret |
|---------------|---------|------------|
| Admin | admin_api_key_123 | admin_api_secret_xyz |
| Test Shop | test_shop_key_456 | test_shop_secret_abc |
| Demo Store | demo_store_key_789 | demo_store_secret_def |

Save the token for subsequent requests.