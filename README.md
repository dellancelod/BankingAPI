# BankingAPI
This is small banking WEB API created as a test task for my job application.

## Stack:
### Language & Framework

• C# – main programming language

• .NET 7 / .NET 8 – framework for building the REST API

### Web & API

• ASP.NET Core Web API – for building RESTful endpoints

• Controllers & Routing – for mapping HTTP requests to actions

### Data & Database

• Entity Framework Core (EF Core) – ORM for database access

• InMemory Database (for testing) – allows unit tests without a real DB

• Optional SQLite / SQL Server – for production or integration tests

### Dependency Injection & Helpers

• IAccountService / AccountService – service layer for business logic

• IAccountNumberGenerator / AccountNumberGenerator – helper for account numbers

• DTOs (Data Transfer Objects) – for safe data transfer to clients

### Testing

• xUnit – unit testing framework

• Moq – mocking framework for interfaces (like the generator)

• EF Core InMemory – to test database operations without a real DB

### Tools & Environment

• Visual Studio / Visual Studio Code – IDE for coding

• Git / GitHub – version control and repository hosting

• Postman / curl – for testing REST endpoints manually

• Swagger / OpenAPI – for API documentation and testing

## How to run:
```
git clone https://github.com/dellancelod/BankingAPI
cd .\BankingAPI\
dotnet restore
dotnet tool install --global dotnet-ef
dotnet ef --project Banking.Api database update
dotnet test
dotnet run --project Banking.Api
```

## Example curl commands:
Get all accounts:
```
curl -X GET http://localhost:5288/api/accounts
```
Create account:
```
curl -X POST http://localhost:5288/api/accounts ^
-H "Content-Type: application/json" ^
-d "{\"ownerName\":\"Alice\",\"initialBalance\":100.50}"
```
