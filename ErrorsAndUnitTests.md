# Errors

### #1 – `Delete` uses the Id as a list index (High)

```csharp
public bool Delete(int id)
{
    if (id < _vehicles.Count)
    {
        _vehicles.RemoveAt(id);   // <-- id is used as an INDEX
        return true;
    }
    return false;
}
```

`Id` is a business identifier (the seed data starts at `1`), while `RemoveAt` expects a zero-based index. This causes several wrong behaviors with the seed data (`Id` 1 and 2):

| Request | Expected | Actual |
|---------|----------|--------|
| `DELETE /api/vehicles/1` | Removes Ford (Id 1) | Removes **Volvo (Id 2)**, the wrong vehicle, and returns 204 |
| `DELETE /api/vehicles/2` | Removes Volvo (Id 2) | `2 < 2` is false, so it returns **404** even though the vehicle exists |
| `DELETE /api/vehicles/0` | 404 (no such Id) | Removes **Ford (Id 1)** and returns 204 |
| `DELETE /api/vehicles/-1` | 404 | `RemoveAt(-1)` throws `ArgumentOutOfRangeException` → **500** |
| `DELETE /api/vehicles/999` | 404 | 404 (correct, but only by accident) |

The bug gets worse after deletions, because Ids and indexes drift further apart.

**Fix:** look the vehicle up by `Id`.

```csharp
public bool Delete(int id)
{
    var vehicle = GetById(id);
    if (vehicle == null) return false;
    return _vehicles.Remove(vehicle);
}
```

---

### #2 – `GetByStatus` is not implemented (High)

```csharp
public IEnumerable<Vehicle> GetByStatus(string status)
{
    throw new NotImplementedException("Filtering by status is not yet implemented.");
}
```

The controller exposes the endpoint, but it always crashes with a 500. See [Unimplemented Features](#unimplemented-features) (U1).

---

### #3 – `GetAverageFleetMileage` returns `0.0` (Medium)

```csharp
public double GetAverageFleetMileage()
{
    // TODO: Calculate and return the average mileage across all fleet vehicles.
    return 0.0;
}
```

The endpoint responds with `200 OK` and a plausible-looking but wrong value. The expected result for the seed data is `(45000 + 120000) / 2 = 82500`. Note that LINQ's `Average()` on an empty sequence throws `InvalidOperationException`, so the empty fleet case needs explicit handling. See U2.

---

### #4 – `CleanLicensePlate` is lossy and inconsistent (Medium)

```csharp
return plate.Replace("-", "").Trim().ToLower();
```

Problems:

- **Inconsistent data.** Seed data is stored as `FLT-101` (hyphen, uppercase), but any plate created or updated through the API becomes `flt103`. The same collection mixes two formats.
- **Comment does not match the behavior.** It says "sanitizes hyphenation and whitespace", but it also lowercases the string. Plates are conventionally displayed in uppercase.
- **Whitespace is only partially handled.** `Trim()` removes leading and trailing spaces only. `"FLT 103"` keeps its inner space. In addition, `Trim()` runs after `Replace`, so the order is harmless here but easy to misread.
- **No uniqueness check.** `FLT-103` and `flt103` normalize to the same value, but nothing prevents duplicates.
- **Lossy.** Hyphens and original formatting are discarded permanently. This is a problem in countries where the hyphen or space is part of the official plate format (e.g. German plates like `H-AB 1234`).

**Fix (example):** pick one canonical format, apply it to the seed data as well, and enforce uniqueness.

```csharp
public static string CleanLicensePlate(string plate)
{
    if (string.IsNullOrWhiteSpace(plate)) return string.Empty;
    return new string(plate.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
}
```

---

### #5 – `IsValidYear` is dead code (Medium)

`FleetHelper.IsValidYear` exists but is never called from `Create`, `Update` or the controller. A vehicle with `Year = -5` or `Year = 3000` is accepted without complaint. Also, `DateTime.Now` depends on the server's local time zone, and `DateTime.UtcNow` is usually preferred.

**Fix:** call it during `Create` / `Update` (or use data annotations) and return `400 Bad Request` for invalid values.

---

### #6 – Missing input validation (Low)

- `Status` is documented as `Active | Maintenance | Retired` (comment only), but any string is accepted, e.g. `"banana"`.
- `Mileage` can be negative. `Update` can silently lower mileage.
- `Make`, `Model` and `LicensePlate` can be empty strings (the defaults are `string.Empty`).
- `Update` does not check that the license plate is unique.
- `PUT` ignores any `Id` in the request body. This is fine, but it is not documented.

**Fix:** use an `enum` for `Status` (with `JsonStringEnumConverter`) and data annotations (`[Required]`, `[Range]`) on the model. `[ApiController]` then returns 400 automatically.

---

### #7 – Not thread-safe (Low)

`VehicleService` is a singleton that mutates a plain `List<Vehicle>`. Concurrent requests can corrupt the list or throw. The Id generation (`Max(v => v.Id) + 1`) is also a race condition: two simultaneous `POST` requests can receive the **same Id**.

**Fix:** protect the list with a `lock`, or use a `ConcurrentDictionary<int, Vehicle>` and an `Interlocked` counter. For anything beyond a demo, use a real database (e.g. EF Core).

---

### #8 – Controller details (Low)

- `[HttpGet("{id}")]` has no route constraint. `GET /api/vehicles/abc` goes to `GetById`, fails model binding and returns 400. Use `{id:int}`.
- `if (vehicle == null) return BadRequest();` in `Create` is redundant. With `[ApiController]`, a missing or invalid body already produces an automatic 400.
- `Update` has no such check, so the behavior is inconsistent between the two methods.
- `GetAll()` returns the internal list instance. Callers inside the process could mutate service state, so return a copy (`.ToList()`) or a DTO.
- Entities are exposed directly as API contracts. A separate DTO or request model would avoid over-posting (e.g. a client sending its own `Id`).
- Status route values are case-sensitive unless the service handles them (see #2).

---

### #9 – `Program.cs` comment is misleading (Low)

```csharp
c.RoutePrefix = "swagger"; // Opens Swagger at root URL or /swagger
```

With `RoutePrefix = "swagger"`, Swagger UI is only served at `/swagger`. To serve it at the root, the prefix must be `string.Empty`.

---

## Additional Code-Quality Observations

- No logging, no global exception handling (`UseExceptionHandler` / `ProblemDetails`), so unhandled exceptions return raw 500s.
- Service methods are synchronous. This is fine for an in-memory store, but real persistence would need `async`/`Task`.
- No unit or integration tests exist. The bugs above would be caught by simple tests.
- `UseAuthorization()` is configured, but there is no authentication and no `[Authorize]` attributes, so every endpoint is public.
- `Year` / `IsValidYear` upper bound of `currentYear + 1` allows next year's models. That is plausible, but should be documented.

---

## Suggested Fixes

Reference implementation for the two unimplemented methods:

```csharp
public IEnumerable<Vehicle> GetByStatus(string status)
{
    return _vehicles.Where(v =>
        string.Equals(v.Status, status, StringComparison.OrdinalIgnoreCase));
}

public double GetAverageFleetMileage()
{
    return _vehicles.Count == 0 ? 0.0 : _vehicles.Average(v => v.Mileage);
}
```

## Manual Test Plan

Run these against a fresh start (seed data only). The "after fix" column shows the expected behavior once everything is repaired.

| # | Request | Current behavior | After fix |
|---|---------|------------------|-----------|
| 1 | `GET /api/vehicles` | 200, 2 vehicles | same |
| 2 | `GET /api/vehicles/1` | 200, Ford | same |
| 3 | `GET /api/vehicles/99` | 404 | same |
| 4 | `GET /api/vehicles/status/Active` | **500** | 200, `[Ford]` |
| 5 | `GET /api/vehicles/analytics/average-mileage` | 200, `0` | 200, `82500` |
| 6 | `DELETE /api/vehicles/1` | 204, but **Volvo is removed** | 204, Ford removed |
| 7 | `DELETE /api/vehicles/2` (fresh start) | **404** | 204, Volvo removed |
| 8 | `DELETE /api/vehicles/-1` | **500** | 404 |
| 9 | `POST` with plate `FLT-103` | 201, plate stored as `flt103` | 201, consistent format |
| 10 | `POST` with `year: 3000` | 201 (accepted) | 400 |
| 11 | `POST` with `status: "banana"` | 201 (accepted) | 400 |
| 12 | Two parallel `POST` requests | Possible duplicate Id | Unique Ids |

## Suggested Next Steps

1. Fix `Delete` (#1) first, since it can delete the wrong data.
2. Implement `GetByStatus` and `GetAverageFleetMileage`.
3. Decide on one license plate format and apply it consistently, including the seed data.
4. Wire up `IsValidYear` and add model validation.
5. Add thread safety, or replace the list with EF Core.
6. Add xUnit tests for the service layer and `WebApplicationFactory` integration tests for the endpoints.
