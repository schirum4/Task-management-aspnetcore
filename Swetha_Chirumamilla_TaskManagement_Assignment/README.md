#Task Management Take-Home (ASP.NET Core 6)

This solution implements a simple RESTful Task API that supports:
- Creating a new Task
- Updating an existing Task

All required business rules are enforced in the service layer and covered by unit tests.

## Tech Stack
- .NET 6 / ASP.NET Core 6 Web API (C#)
- xUnit for unit tests
- In-memory repository (no database required)

## Prerequisites
- Visual Studio 2019 or later (Windows), or Visual Studio for Mac 2019 or later (macOS)
- .NET 6 SDK installed

## How to run the API
1. Open `TaskManagement.sln` in Visual Studio.
2. Set `TaskManagement.Api` as the startup project.
3. Run the project (F5). Swagger will open automatically.

By default the API runs on HTTPS and HTTP on the ports selected by Visual Studio.

## How to run unit tests
- In Visual Studio: **Test** → **Run All Tests**
- Or from terminal at the repository root:
  ```bash
  dotnet test
  ```

## API Endpoints
### Create a task
- `POST /api/tasks`

Request body example:
```json
{
  "name": "Pay invoices",
  "description": "Pay vendor invoices for December",
  "dueDate": "2026-01-15",
  "startDate": "2025-12-16",
  "endDate": null,
  "priority": "High",
  "status": "New"
}
```

Responses:
- `201 Created` with created task payload
- `400 Bad Request` if a business rule is violated

### Update a task
- `PUT /api/tasks/{id}`

Request body example:
```json
{
  "name": "Pay invoices (updated)",
  "description": "Pay vendor invoices",
  "dueDate": "2026-01-16",
  "startDate": "2025-12-16",
  "endDate": null,
  "priority": "Medium",
  "status": "InProgress"
}
```

Responses:
- `200 OK` with updated task payload
- `404 Not Found` if the task does not exist
- `400 Bad Request` if a business rule is violated

## Business rules implemented
- Due date cannot be in the past
- Due date cannot fall on a weekend
- Due date cannot be on a holiday (**US federal holidays only**, including observed days when a holiday falls on a weekend)
- The system cannot have more than 100 **High** priority tasks with the same due date where the tasks are **not Finished**

## Design decisions (for interview)
- **DDD-ish layering:** domain model + service layer encapsulates rules; controllers stay thin.
- **DI:** repository, holiday provider, and clock are injected.
- **Testability:** business rules are tested via unit tests with a fake clock and in-memory repository.
- **No DB:** an in-memory repository is used to keep the assignment focused on correctness and testing.

