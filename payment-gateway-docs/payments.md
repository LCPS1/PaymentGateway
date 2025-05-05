# Payments

## Processing Payments

### Creating a Payment
bash
curl -X 'POST' \
'http://localhost:8080/api/v1/payments' \
-H 'accept: /' \
-H 'Authorization: Bearer YOUR_TOKEN_HERE' \
-H 'Content-Type: application/json' \
-d '{
"amount": 99.99,
"currency": "USD",
"cardNumber": "4111111111111111",
"cardHolderName": "John Doe",
"expiryMonth": 12,
"expiryYear": 2025,
"cvv": "123",
"idempotencyKey": "test-payment-123456"
}'
**Successful Response:**
json
{
"id": "6f3c9a2d-eb1c-4c9d-b49c-b940c1a8ac88",
"status": "Successful",
"amount": 99.99,
"currency": "USD",
"cardLastFour": "1111",
"cardBrand": "Visa",
"timestamp": "2023-06-01T10:15:30Z"
}
**Failed Response:**
json
{
"id": "79e2c534-f9a2-4a0d-8fe3-28d7a8e91c2d",
"status": "Failed",
"amount": 99.99,
"currency": "USD",
"cardLastFour": "1111",
"cardBrand": "Visa",
"timestamp": "2023-06-01T10:15:30Z",
"errorMessage": "Insufficient funds"
}
### Retrieving Payment Status
bash
curl -X 'GET' \
'http://localhost:8080/api/v1/payments/PAYMENT_ID_HERE' \
-H 'accept: /' \
-H 'Authorization: Bearer YOUR_TOKEN_HERE'

**Response:**
json
{
"id": "6f3c9a2d-eb1c-4c9d-b49c-b940c1a8ac88",
"status": "Successful",
"amount": 99.99,
"currency": "USD",
"cardLastFour": "1111",
"cardBrand": "Visa",
"timestamp": "2023-06-01T10:15:30Z"
}