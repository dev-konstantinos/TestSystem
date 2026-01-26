# Blazor Test System for Educational and Corporate Training

## Description

Educational test system for teachers and students, which can also be used in corporate training scenarios. This application allows teachers to create tests and manage students (colleagues) and their results; students can take tests and view their achievements.

## Tech stack

The application is built using the following technologies:

- ASP.NET Core
- Blazor for user interface
- EF Core for data access
- Identity for user management
- SQL Server as the database

This stack ensures a robust, scalable, and maintainable solution and requires no / minimum JS for UI development and support.

## Architecture overview

The application follows a layered architecture with a strong focus on clear responsibility boundaries and safe data access.

Key design decisions:

- Service-based layer with clearly separated services and DTOs for UI communication
- Direct use of EF Core DbContext inside services (Repository layer intentionally omitted in favor of simplicity and performance)
- Instead of sharing DbContext instances across services and components, the application uses IDbContextFactory<TContext> to create short-lived DbContext instances. This approach prevents concurrency exceptions and works more stable in Blazor Server.

## Roles

- Identity roles: Admin / User (clean roles for authentication and authorization)
- Business roles: Teacher / Student (used only to access business functionality)

## Main flows

The application supports the following main flows, which can be accessed based on user roles:

- Teacher adds new tests and questions, manages students
- Student passes test and views results to complete learning
- Admin manages users and adds roles to them, if necessary
- Users can use some limited functionality without roles

This functionality can be easily extended in the future thanks to the flexible layered structure of the application.

## How to run

1. Clone repository
2. Set connection string in appsettings.json
3. Apply migrations and run the application

Note: Windows Authentication is used by default for local development. You may change it to SQL authentication if needed.

### Email confirmation

Email confirmation is required for newly registered users to ensure valid email addresses (only local by default).

## Default admin account (development only)

Email: admin@example.com  
Password: Admin123!

This account is created automatically on first run.
