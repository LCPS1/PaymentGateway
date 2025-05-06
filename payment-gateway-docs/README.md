# Payment Gateway API Documentation

## Overview

This documentation provides instructions for testing and using the Payment Gateway API. The API allows merchants to process payments and retrieve payment status information.

## Table of Contentsa

- [Authentication](authentication.md)
- [Payments](payments.md)
- [Testing Guide](testing.md)


## Prerequisite
- Docker and Docker Compose
- A web browser or Postman (for API testing)


## Getting Started

The Payment Gateway API provides a secure and reliable way to process credit card payments. To use the API, you'll need to:

1. Obtain API credentials
2. Authenticate to get a token
3. Submit payment requests
4. Check payment statuses

### Starting API

1. Clone the repository

        git clone https://github.com/LCPS1/PaymentGateway

        cd payment-gateway

2. Start services with Docker Compose:

        docker-compose up -d --build

3. Access Swagger UI: Open your browser and navigate to:
            
        http://localhost:8080/swagger/index.html

### Troubleshooting API

1. Check logs for information/Status

        docker-compose logs -f api 

        docker-compose logs -f db

2. Stop Containers
   
        docker-compose down

For detailed instructions, please refer to the specific sections in this documentation.