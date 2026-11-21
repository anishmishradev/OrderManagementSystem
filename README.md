
Order Management System
An ASP.NET Core 8 Web API for managing customers, products, orders, and inventory using SQL Server, Entity Framework Core Code First, and JWT authentication.

1. Features
  Customer and Product Management
  Order Creation and Order Details
  Order Status Management
  Stock Validation and Inventory Management
  Automatic Stock Restoration on Order Cancellation
  JWT Authentication and Authorization
  Global Exception Handling Middleware
  Request and Response Logging Middleware
  Structured Logging using Serilog
  Swagger API Documentation
  Entity Framework Core Code First Migrations
  Standardized API Responses

2. Prerequisites
Windows
================================
Visual Studio 2022 with ASP.NET and Web Development workload
.NET 8 SDK
SQL Server Developer Edition or SQL Server Express
SQL Server Management Studio (SSMS)
Docker Desktop (optional)
Git
Postman
=============================================
macOS
Visual Studio Code
.NET 8 SDK
Docker Desktop
Git
Postman
============================================
4. Clone and Open the Project
Clone the GitHub repository:

git clone https://github.com/anishmishradev/OrderManagementSystem.git

Navigate to the project directory:

cd OrderManagementSystem

Visual Studio (Windows)

Open OrderManagementSystem.sln and set OrderManagement.API as the startup project.

Visual Studio Code (macOS)

Open the project folder:

code .
============================
4. Database Configuration
Choose either of the following SQL Server setup options.

Option A — Using Docker (Windows/macOS)
Start Docker Desktop.

macOS Terminal:

docker run --name sqlserver --platform linux/amd64 \
  -e ACCEPT_EULA=Y \
  -e MSSQL_SA_PASSWORD='YOUR_SQL_SERVER_PASSWORD' \
  -p 1433:1433 \
  -v sqlserver_data:/var/opt/mssql \
  -d mcr.microsoft.com/mssql/server:2022-latest

Windows PowerShell:

docker run --name sqlserver -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD="Password@12345" -p 1433:1433 -v sqlserver_data:/var/opt/mssql -d mcr.microsoft.com/mssql/server:2022-latest

Use the SQL Server password configured in the existing appsettings.json when creating the container.

If the container already exists, start it using:

docker start sqlserver

Verify that SQL Server is running:

docker ps
===================================================
Option B — Without Docker (Windows Only)
Install SQL Server Developer Edition or SQL Server Express and SQL Server Management Studio (SSMS).

Start the SQL Server instance and update the connection string in:
================================================
OrderManagement.API/appsettings.json

SQL Server Authentication:

"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=OrderManagementDb;User Id=sa;Password=Password@12345;TrustServerCertificate=True;"
}


Application Configuration
The connection string, admin credentials, JWT configuration, and logging settings are already configured in OrderManagement.API/appsettings.json.

Use the existing configuration. Update only the database connection string if your SQL Server instance or credentials are different.

The default application login credentials are:

"Auth": {
  "Username": "admin",
  "Password": "Admin@12345"
}

The JWT configuration is already available in appsettings.json with a token expiry of 30 minutes. No additional JWT configuration is required.

5. Restore NuGet Packages and Build
Visual Studio (Windows)
Open OrderManagementSystem.sln.

Right-click the solution in Solution Explorer.

Select Restore NuGet Packages.

Select Build → Build Solution, or press Ctrl + Shift + B.

Visual Studio Code (macOS)
Execute the following commands from the project root:

dotnet restore
dotnet build

Ensure the solution builds successfully before proceeding.

6. Entity Framework Core Database Migrations
The EF Core Code First migration files are included in the repository under:

OrderManagement.Infrastructure/Migrations/
These migrations create the required database schema and tables automatically.

Visual Studio (Windows)
Open:

Tools → NuGet Package Manager → Package Manager Console

Select OrderManagement.Infrastructure as the default project.

If EF Core Tools is not available, install it using the version matching the project's EF Core packages:

Install-Package Microsoft.EntityFrameworkCore.Tools -Version 8.0.11 -ProjectName OrderManagement.Infrastructure

Apply the existing migrations:

Update-Database -Project OrderManagement.Infrastructure -StartupProject OrderManagement.API -Context OrderManagementDbContext

Visual Studio Code / Terminal
Install the EF Core CLI tool if it is not already installed:

dotnet tool install --global dotnet-ef --version 8.0.11

Verify the installation:

dotnet ef --version

Apply the existing database migrations:

dotnet ef database update \
  --project OrderManagement.Infrastructure \
  --startup-project OrderManagement.API \
  --context OrderManagementDbContext

The database and required tables will be created automatically when the configured SQL Server account has sufficient permissions. Manual table creation is not required.

The existing migrations should be used; creating a new initial migration is not required.
================================================
7. Run the Application
Visual Studio (Windows)
Set OrderManagement.API as the startup project.

Press F5 or click the Run button.

Swagger will be available in the Development environment.
=========================================================
Visual Studio Code (macOS)
Run the following command:

dotnet run --project OrderManagement.API

The application URL will be displayed in the terminal.

Open Swagger using the application URL followed by /swagger.

The application port is configured in:

OrderManagement.API/Properties/launchSettings.json
=======================================================
8. Test APIs Using Postman
The Postman collection is included in the GitHub repository for testing all available APIs.

Step 1 — Import the Postman Collection
After the application builds and runs successfully:

Navigate to the postman folder in the repository.

Locate the Postman collection JSON file.

Open Postman and click Import.

Select the collection JSON file and import it.

All configured API requests will be available in Postman.

Verify that the collection base URL matches the running application URL.

Step 2 — Generate JWT Access Token
First, execute the Authentication API.

Endpoint:

POST /api/auth/getoken
Request Body:

{
  "username": "admin",
  "password": "Admin@12345"
}

Response:

{
  "accessToken": "GENERATED_JWT_TOKEN",
  "tokenType": "Bearer"
}

Copy the accessToken from the response.

Step 3 — Configure Authorization
In Postman:

Select the imported collection.

Open the Authorization tab.

Select Bearer Token as the authorization type.

Paste the generated JWT access token.

Save the collection.

Ensure protected API requests use Inherit auth from parent.

The token can now be used to access all protected Customer, Product, and Order APIs.

Step 4 — Execute the APIs
Execute the APIs in the following order:

Create Customer

Get All Customers

Get Customer By ID

Create Product with available stock

Get All Products

Create Order using the Customer ID and Product ID

Get Order By ID

Update Order Status

Verify Product Stock

If the token expires, execute the Authentication API again and update the Bearer Token.

## 9. API Endpoints

| Method |     Endpoint |         Description |
|---|---|---|
| POST | `/api/auth/getoken` | Generate JWT token |
| POST | `/api/customers` | Create customer |
| GET | `/api/customers` | Get all customers |
| GET | `/api/customers/{id}` | Get customer by ID |
| POST | `/api/products` | Create product |
| GET | `/api/products` | Get all products |
| POST | `/api/orders` | Create order |
| GET | `/api/orders/{id}` | Get order details |
| PATCH | `/api/orders/{id}/status` | Update order status |

Authentication: All APIs except `/api/auth/getoken` require a valid JWT token.

API Testing: Import the included Postman collection for request bodies and endpoint testing..

10. Project Structure
OrderManagementSystem/
│
├── OrderManagement.API/
│   ├── Controllers/
│   ├── Middleware/
│   ├── Properties/
│   ├── Program.cs
│   └── appsettings.json
│
├── OrderManagement.Application/
│   ├── Auth/
│   ├── Common/
│   ├── Customers/
│   ├── Products/
│   ├── Orders/
│   └── Interfaces/
│
├── OrderManagement.Domain/
│   ├── Entities/
│   └── Enums/
│
├── OrderManagement.Infrastructure/
│   ├── Data/
│   ├── Repositories/
│   └── Migrations/
│
├── postman/
├── OrderManagementSystem.sln
├── .gitignore
└── README.md
#11. Architecture and Design Decisions
   The application follows Clean Architecture principles to ensure separation of concerns, maintainability, scalability, and testability.

Layer

Responsibility

Domain-->Business, entities and enums

Application-->Business logic, services, DTOs, and interfaces

Infrastructure-->Database access, repositories, and EF Core migrations

API ---> Controllers, authentication, middleware, and dependency injection

Design Decisions ---> Clean Architecture: Separation of business logic, database operations, and presentation layers.

SOLID Principles: Maintainable, loosely coupled, and reusable components.

Repository Pattern: Abstracts database operations from business logic.

Dependency Injection: Manages service and repository dependencies.

Entity Framework Core Code First: Manages database schema through migrations.

Asynchronous Programming: Uses async/await for database and API operations.

CancellationToken: Supports cancellation of asynchronous operations.

JWT Authentication: Secures Customer, Product, and Order APIs.

In-Memory Caching: Reuses generated JWT tokens until expiration.

Database Transactions: Maintains consistency between order creation and inventory updates.

Concurrency Handling: Conditional stock updates help prevent overselling.

Global Exception Handling: Centralized handling of application exceptions.

Request/Response Middleware: Logs API requests, responses, status codes, and execution time.

Structured Logging: Uses Serilog and ILogger.

Standardized API Responses: Maintains a consistent success and error response structure.

#12. Assumptions and Business Rules

  Admin authentication uses appsettings.json.
  Orders require valid customers, products, and sufficient stock.
  Stock decreases on purchase and is restored on cancellation.
  Order flow: Pending → Confirmed → Shipped; Pending/Confirmed → Cancelled.
  Shipped and Cancelled orders cannot be modified.

13. Open-Source Libraries
  EF Core / SQL Server: Database and migrations
  JWT Bearer: Authentication
  Serilog: Logging
  Swagger: API documentation
  MemoryCache: Token caching

All NuGet dependencies are defined in .csproj files and restored automatically.

14. Logging

    Serilog logs API requests, responses, execution times, and exceptions.

15. HTTP Status Codes

    200 Success | 201 Created | 400 Bad Request | 401 Unauthorized | 403 Forbidden | 404 Not Found | 500 Server Error

16. Build and Run

  dotnet restore
  dotnet build
  dotnet run --project OrderManagement.API
  
  Test APIs using the included Postman collection or Swagger.

17. GitHub Repository

https://github.com/anishmishradev/OrderManagementSystem

Tech Stack: .NET 8, C#, SQL Server, EF Core, JWT, Serilog, Swagger, Git.
