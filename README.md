# 🧾 Customer Order Management API

This is a full-featured Customer Order Management API built with **.NET 8**, designed with **Clean Architecture** principles and supports modern features like **CQRS with MediatR**, **JWT Authentication**, **FluentValidation**, **Redis Caching**, **RabbitMQ Messaging**, and **structured logging with Serilog**.

---

## Features

- User authentication & JWT-based authorization
- Customer, Product, and Order management
- CQRS (Command & Query Separation) with MediatR
- API-level request logging into PostgreSQL
- Redis integration for product caching
- RabbitMQ integration for order-related messaging
- Health checks & Swagger documentation
- Structured, rolling log files with Serilog

---

## Tech Stack

- **Backend**: ASP.NET Core 8
- **Database**: PostgreSQL + Entity Framework Core
- **Caching**: Redis (via StackExchange.Redis)
- **Message Queue**: RabbitMQ
- **Logging**: Serilog
- **Validation**: FluentValidation
- **Mapping**: AutoMapper
- **Testing**: xUnit (planned)
- **API Docs**: Swagger (Swashbuckle)

---

## Project Structure

- `CustomerOrders.Core` - Domain entities & interfaces
- `CustomerOrders.Application` - Business logic, DTOs, CQRS handlers
- `CustomerOrders.Infrastructure` - Data access, external integrations
- `CustomerOrderApi` - API controllers & configurations
- `OrderManagementApplication.Tests` - Unit test layer

---

## API Endpoints

•	POST /api/auth/register: Register a new user
•	POST /api/auth/login: Login and receive JWT token
•	PUT /api/auth/update-password: Change password for the logged-in user
•	GET /api/customers: List all active customers
•	GET /api/customers/{id}: Get customer by ID
•	PUT /api/customers/update-info: Update name, email, or address
•	DELETE /api/customers/delete-account: Delete the currently logged-in user’s account
•	GET /api/products: List products
•	POST /api/products: Add one or more products
•	PUT /api/products: Update a product
•	GET /api/products/{id}: Get product by ID
•	DELETE /api/products/{id}: Delete a product by ID
•	GET /api/customerorders/{id}: Get a specific order
•	POST /api/customerorders: Create a new order
•	DELETE /api/customerorders/{id}: Delete an order
•	POST /api/customerorders/{orderId}/products: Add product(s) to an order
•	DELETE /api/customerorders/{orderId}/products/{productId}: Remove a product from an order
•	PUT /api/customerorders/{orderId}/address: Update shipping address
•	PATCH /api/customerorders/{orderId}/products/{productId}/quantity: Update quantity of a product in the order

---

## Getting Started

1. Clone the repository
2. Configure PostgreSQL, Redis, and RabbitMQ
3. Run the project with Docker Compose or manually
4. Use Swagger 

---

## Future Improvements

- Role-based access control
- Continue Test coverage with xUnit & Moq

---

## Full API Documentation

For complete details including request/response formats, usage scenarios, architecture, and diagrams:

**[Read Full Project Documentation](./Customer-Order-Management-Documentation.docx)**

---

## Developed By

**Eda Belge**  
Date: March 23, 2025

---

