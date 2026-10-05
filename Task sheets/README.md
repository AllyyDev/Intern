# FleetApi – Fleet Management REST API

A small ASP.NET Core Web API for managing a fleet of vehicles. It uses an in-memory data store, so there is no database to set up. Data is reset every time the app restarts.

> **Note:** This project is an intern/training exercise. It contains **intentionally unimplemented methods** and **intentionally planted bugs**. They are documented in [Known Issues](#known-issues-intended-bugs) and [Unimplemented Features](#unimplemented-features) below.

---

## Table of Contents

1. [Tech Stack](#tech-stack)
2. [Project Structure](#project-structure)
3. [Getting Started](#getting-started)
4. [Data Model](#data-model)
5. [API Reference](#api-reference)
6. [Architecture Notes](#architecture-notes)
7. [Unimplemented Features](#unimplemented-features)
8. [Known Issues (Intended Bugs)](#known-issues-intended-bugs)

---

## Tech Stack

- C# / ASP.NET Core Web API (controller-based, .NET 8 or newer, nullable reference types enabled)
- Swagger / OpenAPI via Swashbuckle (`AddSwaggerGen`, `UseSwaggerUI`)
- Dependency injection (built-in container)
- In-memory storage (`List<Vehicle>`) registered as a singleton

## Project Structure

```
Intern (solution)
└── FleetApi
    ├── Connected Services
    ├── Dependencies
    ├── Properties
    ├── Controllers
    │   └── VehiclesController.cs   # HTTP endpoints
    ├── Helpers
    │   └── FleetHelper.cs          # License plate cleaning, year validation
    ├── Models
    │   └── Vehicle.cs              # Domain model
    ├── Services
    │   ├── IVehicleService.cs      # Service contract
    │   └── VehicleService.cs       # In-memory implementation
    ├── appsettings.json
    ├── FleetApi.http               # Sample requests for the VS HTTP editor
    └── Program.cs             
```

## Getting Started

**Prerequisites:** .NET SDK 8.0 or newer, Visual Studio 2022+

```bash
dotnet restore
dotnet run --project FleetApi
```

In the Development environment, Swagger UI is available at:

```
https://localhost:<port>/swagger
```

The port is defined in `Properties/launchSettings.json`. You can also use `FleetApi.http` directly in Visual Studio to send requests.

## Data Model

`Models/Vehicle.cs`

| Property       | Type     | Description                                                  |
|----------------|----------|--------------------------------------------------------------|
| `Id`           | `int`    | Unique identifier, assigned by the service on creation       |
| `Make`         | `string` | Manufacturer (e.g. Ford)                                     |
| `Model`        | `string` | Model name (e.g. Transit)                                    |
| `LicensePlate` | `string` | License plate, normalized on create/update (see issue #4)    |
| `Year`         | `int`    | Year of manufacture                                          |
| `Mileage`      | `int`    | Odometer reading                                             |
| `Status`       | `string` | `Active` (default), `Maintenance` or `Retired` (not enforced) |

**Seed data**

| Id | Make  | Model   | License Plate | Year | Mileage | Status      |
|----|-------|---------|---------------|------|---------|-------------|
| 1  | Ford  | Transit | FLT-101       | 2021 | 45000   | Active      |
| 2  | Volvo | FH16    | FLT-102       | 2019 | 120000  | Maintenance |

## API Reference

Base route: `/api/vehicles`

| Method | Route                                 | Description                    | Success      | Errors                     | Status            |
|--------|---------------------------------------|--------------------------------|--------------|----------------------------|-------------------|
| GET    | `/api/vehicles`                       | List all vehicles              | 200          | –                          | Works             |
| GET    | `/api/vehicles/{id}`                  | Get one vehicle                | 200          | 404                        | Works             |
| POST   | `/api/vehicles`                       | Create a vehicle               | 201 + Location | 400                      | Works (no validation) |
| PUT    | `/api/vehicles/{id}`                  | Update a vehicle               | 204          | 404                        | Works (no validation) |
| DELETE | `/api/vehicles/{id}`                  | Delete a vehicle               | 204          | 404                        | **Buggy** (#1)    |
| GET    | `/api/vehicles/status/{status}`       | Filter by status               | 200          | –                          | **Not implemented** (#2) |
| GET    | `/api/vehicles/analytics/average-mileage` | Average mileage of the fleet | 200        | –                          | **Stub** (#3)     |

### Example request

```http
POST /api/vehicles
Content-Type: application/json

{
  "make": "Mercedes",
  "model": "Sprinter",
  "licensePlate": "FLT-103",
  "year": 2022,
  "mileage": 15000,
  "status": "Active"
}
```

## Architecture Notes

- **Controller → Service → in-memory list.** The controller only deals with HTTP concerns. All data logic lives in `IVehicleService` / `VehicleService`.
- **Singleton lifetime.** `VehicleService` is registered with `AddSingleton`, which is required for the in-memory list to survive between requests. It also means shared mutable state (see issue #7).
- **Helpers.** `FleetHelper` contains static utility methods used by the service layer.
- **Swagger** is only enabled when `ASPNETCORE_ENVIRONMENT=Development`.

---

## Unimplemented Features

These are deliberately left open in the source code.

| # | Location | Method | Current behavior | Expected behavior |
|---|----------|--------|------------------|-------------------|
| U1 | `Services/VehicleService.cs` | `GetByStatus(string status)` | Throws `NotImplementedException`, so the endpoint returns **HTTP 500** | Return all vehicles whose `Status` matches the given value (case-insensitive) |
| U2 | `Services/VehicleService.cs` | `GetAverageFleetMileage()` | Contains a `TODO`, always returns `0.0` | Return the average `Mileage` across all vehicles (and define behavior for an empty fleet) |

Affected endpoints:

- `GET /api/vehicles/status/{status}` → 500 Internal Server Error
- `GET /api/vehicles/analytics/average-mileage` → always returns `0` (a silent wrong answer, which is arguably worse than an error)

---

## Known Issues (Intended Bugs)

### Summary

| #  | Severity | File | Issue |
|----|----------|------|-------|
| 1  | 🔴 High   | `VehicleService.cs` | `Delete` treats the vehicle `Id` as a list index |
| 2  | 🔴 High   | `VehicleService.cs` | `GetByStatus` throws `NotImplementedException` (see U1) |
| 3  | 🟠 Medium | `VehicleService.cs` | `GetAverageFleetMileage` returns a hardcoded `0.0` (see U2) |
| 4  | 🟠 Medium | `FleetHelper.cs` | `CleanLicensePlate` changes data in a lossy, inconsistent way |
| 5  | 🟠 Medium | `FleetHelper.cs` / Service | `IsValidYear` is never called, so there is no year validation |
| 6  | 🟡 Low    | Service / Controller | No validation of `Status`, `Mileage`, duplicates, or required fields |
| 7  | 🟡 Low    | `VehicleService.cs` | Not thread-safe (shared `List<T>` in a singleton) |
| 8  | 🟡 Low    | `VehiclesController.cs` | Route `{id}` has no `:int` constraint; redundant null checks |
| 9  | 🟡 Low    | `Program.cs` | Misleading comment about the Swagger route |
