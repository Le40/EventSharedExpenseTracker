# Event Shared Expense Tracker

A web application for managing shared trip expenses. The application supports multi-currency expense tracking and AI-assisted features such as automatic expense categorization and receipt parsing.

The project was built primarily as a personal/learning project to explore ASP.NET Core, Entity Framework Core, application architecture, testing, authentication, deployment, and cloud services and others.

https://eventsharedexpensetracker.azurewebsites.net/
(if trying out, its deployed on the free server, and it takes quite a while until the site warms up, until then it seems like it doesnt work, but its just parked.)

## Features

* Create and manage trips with multiple participants
* Track shared expenses
* Flexible expense splitting between participants
* Expense categories and filtering
* Support for multiple currencies with automatic conversion to trip base currency
* AI-assisted expense categorization based on expense name
* AI-assisted receipt parsing to prefill expense forms from uploaded receipt photos
* HTMX-powered partial updates for a responsive user experience
* Image upload and compression
* ASP.NET Core Identity authentication
* Authorization rules for trips and expenses
* Structured error/result handling
* Automated tests
* Azure deployment with Azure SQL Database and Key Vault integration

## Technology Stack

* ASP.NET Core MVC
* Entity Framework Core
* SQL Server / Azure SQL Database
* ASP.NET Core Identity
* HTMX
* Bootstrap
* Mapster
* xUnit
* OpenAI API
* Azure App Service
* Azure Key Vault

## Project Structure

The application is organized into several layers:

* Domain

  * Entities
  * Value Objects
  * Business rules

* Application

  * Commands
  * Queries
  * Services

* Infrastructure

  * Entity Framework Core
  * Repositories
  * External integrations

* Presentation

  * MVC Controllers
  * Razor Views
  * HTMX interactions

## Testing

The solution includes:

* Unit tests
* Integration tests
* GitHub Actions CI workflow

## Running Locally

Requirements:

* .NET 8 SDK
* SQL Server (or Azure SQL)

Clone the repository:

```bash
git clone https://github.com/Le40/EventSharedExpenseTracker.git
```

Apply migrations:

```bash
dotnet ef database update
```

Run the application:

```bash
dotnet run
```

### Planned Features

- Mobile application
- REST API
- Offline expense entry
- Vertical Slice Architecture

## Notes

This project is still evolving and is used as a place to experiment with new ideas and technologies while improving development practices.


