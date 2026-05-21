# Event Shared Expense Tracker

Web application for tracking shared expenses during trips, events, and group activities.

Users can create trips, manage participants, add expenses, and track who paid and who owes money.

Live demo: https://eventsharedexpensetracker.azurewebsites.net

## Features

- ASP.NET Core Identity authentication
- Trip and participant management
- Shared expense tracking
- Equal-share expense calculation
- Dummy/non-registered participants
- Image upload and compression
- Authorization rules for trips and expenses
- Structured error/result handling
- Logging
- Automated tests
- Azure deployment

## Tech Stack

- C#
- ASP.NET Core MVC
- Entity Framework Core
- SQL Server / SQLite
- Bootstrap
- htmx
- Mapster
- Azure App Service

## Architecture Overview

The solution is separated into multiple layers/projects:

```text
Domain
Application
Infrastructure
MVC
Tests
```

The project uses:

- layered architecture inspired by Clean Architecture concepts
- `ServiceResult` / `DomainResult` patterns for structured error handling
- domain-level payment processing and validation
- repository + unit of work pattern
- DTO/query/view model mapping
- authorization rules separated from controllers

One notable modeling concept is separating unfinished payment input from persisted payment entities:

```text
PaymentInput -> ExpenseProcessor -> Payment
```

which allows validation and equal-share calculation before creating final payment entities.

## Purpose

This project is mainly a portfolio and learning project focused on:

- backend architecture
- ASP.NET Core MVC
- domain modeling
- validation and error handling
- authentication and authorization
- deployment and logging
- building a real-world CRUD/business application
