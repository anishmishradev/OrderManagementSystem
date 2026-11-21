# Order Management System

ASP.NET Core 8 Web API for customer, product, order, and inventory management using SQL Server, Entity Framework Core Code First, and JWT authentication.

## 1. Features

- Customer and product management
- Order creation, retrieval, and status updates
- Stock validation, deduction, and restoration on cancellation
- JWT authentication and authorization
- Global exception handling and request/response logging with Serilog
- Swagger documentation and EF Core Code First migrations
- Standardized API responses

## 2. Prerequisites

| Windows | macOS |
|---|---|
| Visual Studio 2022 (ASP.NET and web development workload) | Visual Studio Code |
| .NET 8 SDK | .NET 8 SDK |
| SQL Server Developer/Express **or** Docker Desktop | Docker Desktop (or access to a remote SQL Server) |
| SQL Server Management Studio (SSMS), if using local SQL Server | — |
| Git and Postman | Git and Postman |

## 3. Clone and Open the Project

```bash
git clone https://github.com/anishmishradev/OrderManagementSystem.git
cd OrderManagementSystem
```

- **Windows / Visual Studio:** Open `OrderManagementSystem.sln` and set `OrderManagement.API` as the startup project.
- **macOS / VS Code:** Open the repository folder with `code .`.

## 4. Database Configuration

Choose **one** option.

### Option A — Docker (Windows/macOS)

Start Docker Desktop and create a SQL Server 2022 container. Use the **same SQL Server password** as in your local `OrderManagement.API/appsettings.json` connection string.

**macOS terminal:**

```bash
docker run --name sqlserver --platform linux/amd64 \
  -e ACCEPT_EULA=Y \
  -e MSSQL_SA_PASSWORD='YOUR_SQL_SERVER_PASSWORD' \
  -p 1433:1433 \
  -v sqlserver_data:/var/opt/mssql \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

**Windows PowerShell:**

```powershell
docker run --name sqlserver -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD="YOUR_SQL_SERVER_PASSWORD" -p 1433:1433 -v sqlserver_data:/var/opt/mssql -d mcr.microsoft.com/mssql/server:2022-latest
```

If the container already exists, run `docker start sqlserver`. Check it using `docker ps`.

**Docker connection string** (`ConnectionStrings:DefaultConnection` in `OrderManagement.API/appsettings.json`):

```text
Server=localhost,1433;Database=OrderManagementDb;User Id=sa;Password=YOUR_SQL_SERVER_PASSWORD;TrustServerCertificate=True;
```

### Option B — Without Docker (Windows Only)

Install **SQL Server Developer or Express** and **SQL Server Management Studio (SSMS)**. Start your SQL Server instance and use the corresponding connection string in `OrderManagement.API/appsettings.json`.

**SQL Server Express (Windows authentication):**

```text
Server=localhost\SQLEXPRESS;Database=OrderManagementDb;Trusted_Connection=True;TrustServerCertificate=True;
```

**SQL Server (SQL authentication):**

```text
Server=localhost;Database=OrderManagementDb;User Id=sa;Password=YOUR_SQL_SERVER_PASSWORD;TrustServerCertificate=True;
```

### Application Configuration

`OrderManagement.API/appsettings.json` already contains the connection string, JWT settings, and admin account configuration. Change the connection string only if your database setup differs.

**Default admin credentials:**

```json
{
  "Auth": {
    "Username": "admin",
    "Password": "Admin@12345"
  }
}
```

JWT token expiry is configured as **30 minutes**. Use the existing JWT settings in the application configuration. The sample admin credentials are intended for this assessment only.

## 5. Restore NuGet Packages and Build

**Visual Studio (Windows):** Right-click the solution → **Restore NuGet Packages**; then **Build → Build Solution** (`Ctrl + Shift + B`).

**Terminal (Windows/macOS):**

```bash
dotnet restore
dotnet build
```

Proceed after the solution builds successfully.

## 6. Apply EF Core Migrations

Existing migration files are stored in `OrderManagement.Infrastructure/Migrations/`. Apply these files; **do not create a new initial migration**.

**Visual Studio (Windows):** Open **Tools → NuGet Package Manager → Package Manager Console** and run:

```powershell
Update-Database -Project OrderManagement.Infrastructure -StartupProject OrderManagement.API -Context OrderManagementDbContext
```

If the EF Core Package Manager Console commands are unavailable, install the `Microsoft.EntityFrameworkCore.Tools` package with the version matching the project's EF Core packages.

**VS Code / terminal:** If `dotnet ef` is not installed, run:

```bash
dotnet tool install --global dotnet-ef --version 8.0.11
```

Then apply migrations:

```bash
dotnet ef database update --project OrderManagement.Infrastructure --startup-project OrderManagement.API --context OrderManagementDbContext
```

Migrations create/update the database tables automatically, provided the configured database account has sufficient permissions.

## 7. Run the Application

- **Visual Studio:** Set `OrderManagement.API` as the startup project and press **F5**.
- **VS Code / terminal:**

```bash
dotnet run --project OrderManagement.API
```

Open the application's Swagger page at `<application-url>/swagger`. Use the actual URL printed in the terminal or configured in `OrderManagement.API/Properties/launchSettings.json`.

## 8. Test APIs Using Postman

After the application builds and starts:

1. Open the repository's **`postman`** folder, find the Postman collection `.json` file, and import it using **Postman → Import → File**.
2. Set the collection's base URL to the running API address.
3. **First call** `POST /api/auth/getoken` with this JSON body:

   ```json
   {
     "username": "admin",
     "password": "Admin@12345"
   }
   ```

4. Copy the `accessToken` from the authentication response.
5. In Postman, select the collection → **Authorization → Bearer Token**, paste the token, and save. Protected requests should use **Inherit auth from parent**.
6. Test Customer APIs, then Product APIs, then Order APIs. Create a customer and product before creating an order.

If the token expires, call the Auth API again and replace the token.

## 9. API Endpoints

| Method | Endpoint | Description |
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

All endpoints except the Auth API require a valid JWT token. Refer to the included Postman collection for request bodies and additional routes.

## 10. Project Structure

```text
OrderManagementSystem/
├── OrderManagement.API/             # Controllers, middleware, Program.cs
├── OrderManagement.Application/     # Services, DTOs, interfaces
├── OrderManagement.Domain/          # Entities and enums
├── OrderManagement.Infrastructure/  # DbContext, repositories, migrations
├── postman/                         # API collection JSON
├── OrderManagementSystem.sln
└── README.md
```

## 11. Architecture and Design Decisions

| Layer | Responsibility |
|---|---|
| Domain | Entities, enums, business rules |
| Application | Services, DTOs, interfaces |
| Infrastructure | Database, repositories, migrations |
| API | Controllers, authentication, middleware, dependency injection |

- **Clean Architecture, SOLID, and Repository Pattern:** Separation of concerns and maintainable code.
- **EF Core Code First:** Database schema managed through migrations.
- **Async/await and CancellationToken:** Asynchronous operations.
- **JWT and MemoryCache:** Protected APIs and token caching.
- **Transactions and concurrency checks:** Consistent inventory updates.
- **Middleware and Serilog:** Error handling and request/response logging.
- **Standardized responses:** Consistent API response structure.

## 12. Assumptions and Business Rules

- Admin authentication uses credentials configured in `appsettings.json`; user registration is out of scope.
- Orders require existing customers/products, positive quantities, and sufficient stock.
- Creating an order deducts stock; cancelling restores it.
- Status flow: **Pending → Confirmed → Shipped**; Pending/Confirmed can also become **Cancelled**.
- Shipped and Cancelled orders cannot change status.
- SQL Server generates entity IDs.

## 13. Open-Source Libraries

| Library | Purpose |
|---|---|
| `Microsoft.EntityFrameworkCore.SqlServer` / `Microsoft.EntityFrameworkCore.Design` | Database and migrations |
| `Microsoft.AspNetCore.Authentication.JwtBearer` / `System.IdentityModel.Tokens.Jwt` | JWT authentication and token generation |
| `Serilog.AspNetCore` | Structured logging |
| `Swashbuckle.AspNetCore` | Swagger / OpenAPI |
| `Microsoft.Extensions.Caching.Memory` | Token caching |
| `Microsoft.Extensions.Configuration.Binder` | Configuration binding |
| `Microsoft.Extensions.Logging.Abstractions` | Logging interfaces |

Package versions and dependencies are specified in the `.csproj` files and restored with `dotnet restore`. Visual Studio can restore NuGet packages automatically.

## 14. Logging

Serilog and middleware log API requests, responses, HTTP status codes, execution times, business events, and exceptions. Logging settings are in `appsettings.json`.

## 15. HTTP Status Codes

| Code | Meaning |
|---|---|
| 200 / 201 | Success / Created |
| 400 | Bad Request |
| 401 / 403 | Unauthorized / Forbidden |
| 404 | Not Found |
| 500 | Internal Server Error |

## 16. Build and Verification

```bash
dotnet restore
dotnet build --configuration Release
dotnet run --project OrderManagement.API
```

Verify authentication, customer/product creation, order creation, status changes, and stock restoration using Swagger or Postman.

## 17. Version Control

**GitHub:** https://github.com/anishmishradev/OrderManagementSystem

Git is used for source control. The repository should include application source, EF Core migrations, Postman collection, and README.

**Tech Stack:** .NET 8, C#, SQL Server, EF Core, JWT, Serilog, Swagger, Postman, Git, Clean Architecture.
