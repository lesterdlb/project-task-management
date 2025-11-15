# Project & Task Management System

A full-stack project management application built with ASP.NET Core and React. This is a portfolio project where I'm
exploring Clean Architecture patterns, CQRS, and modern web development practices.

## What's This About?

I built this system to demonstrate how I approach software architecture and development. The focus has been on clean
code, proper separation of concerns, and maintainable patterns rather than just getting features out the door.

The backend uses a vertical slice architecture with a custom mediator implementation (not MediatR), and the frontend is
a React app with TypeScript.

## Current Status

This is a work in progress. The core authentication and project management features are done, with some error handling,
validation, and security, but I'm still working on task management, labels, and comments.

**What's Working:**

- User registration and authentication with email confirmation workflow
- JWT-based authorization with fine-grained permissions
- Full CRUD operations for projects
- Project member management with role-based access
- Pagination, sorting, filtering, and field selection on collection endpoints
- HATEOAS support for API discoverability
- React frontend with authentication and layout components

**What's Coming:**

- Task management (create, assign, track progress)
- Labels and categories
- Comments and discussions
- Password reset functionality
- More comprehensive frontend features

## Technical Highlights

**Architecture Decisions:**

- Vertical Slice Architecture instead of traditional layered architecture
- Custom mediator pattern for CQRS (commands/queries separation)
- Result pattern for error handling instead of exceptions
- ASP.NET Core Identity for user management
- Claims-based authorization with permission system

**Backend Stack:**

- ASP.NET Core 10 Web API
- PostgreSQL with Entity Framework Core
- FluentValidation for request validation
- Serilog for structured logging
- xUnit, FluentAssertions, and Moq for testing

**Frontend Stack:**

- React with TypeScript
- Vite for build tooling
- Tailwind CSS v4
- shadcn/ui component library
- React Router for navigation
- Axios with interceptors for API calls

## Code Organization

The solution follows Clean Architecture principles with vertical slices. Each feature (like "Create Project" or "Add
Member") lives in its own file with the endpoint definition, command/query, handler, validator, and DTOs all together.
This makes features easy to find and modify without jumping between layers. Some pragmatic decisions were made to keep
the code organized.

The frontend follows a standard React structure with contexts for state management, hooks for reusable logic, and a
clear separation between UI components and business logic.

## Running the Project

## API Examples

The API uses standard REST conventions with additional HATEOAS support. Here are a few examples:

**Register a new user:**

```http
POST /api/auth/register
Content-Type: application/json

{
  "userName": "johndoe",
  "email": "john@example.com",
  "fullName": "John Doe",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!"
}
```

**Create a project:**

```http
POST /api/projects
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Website Redesign",
  "description": "Modernize the company website",
  "startDate": "2025-01-15",
  "endDate": "2025-03-30",
  "priority": "High"
}
```

**Get projects with filtering:**

```http
GET /api/projects?page=1&pageSize=10&sort=-startDate&search=redesign
Authorization: Bearer {token}
Accept: application/vnd.projectmanagement.hateoas+json
```

For HATEOAS responses, include the custom Accept header and you'll get hypermedia links for navigation.

## Design Patterns Used

**Result Pattern:**
Commands return `Result<T>` instead of throwing exceptions, making error handling explicit and predictable.

**Mediator Pattern:**
Custom implementation that separates commands (writes) from queries (reads) and supports pipeline behaviors for
cross-cutting concerns like validation. This was a fun part of the project and I'm glad I was able to implement it from
scratch.

**Vertical Slices:**
Features are self-contained slices rather than spread across layers. This reduces coupling and makes features easier to
modify or remove.

**Repository Pattern:**
Entity Framework DbContext acts as a repository with specific query methods in handlers.

## Security Considerations

- Passwords are hashed using ASP.NET Core Identity's default hasher
- JWT tokens include role and permission claims for stateless authorization
- Email confirmation required before login
- Input validation using FluentValidation
- SQL injection protection via parameterized queries (EF Core)
- CORS configured for development (needs production configuration)
- Optimistic concurrency control prevents lost updates

## What I Learned

This project has been a great way to deepen my understanding of:

- When vertical slices work better than traditional layering
- Building a mediator pattern from scratch vs. using libraries
- Implementing proper authorization at both endpoint and resource levels
- Balancing REST conventions with modern patterns like HATEOAS
- Managing React state without heavy state management libraries