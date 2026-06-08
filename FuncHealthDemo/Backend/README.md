# FuncHealth Backend

A healthcare appointment booking API. Users can register, book appointments with doctors, and manage their profiles. Built with .NET 10 and Firebase Authentication.

## How to Run This

**Requirements:**
- .NET 10 SDK
- PostgreSQL
- A Firebase project (optional for dev testing)

**Setup:**
```bash
# 1. Clone and restore packages
git clone <repo-url>
cd FuncHealthDemo
dotnet restore

# 2. Set your database connection in appsettings.json
# ConnectionStrings:DefaultConnection should point to your PostgreSQL instance

# 3. Create the database and seed test data
dotnet ef database update

# 4. Run it
dotnet run --project FuncHealthDemo
```

The API runs on `http://localhost:5000`. Swagger docs are at `/swagger`.

## Test Users (Already Seeded)

After running migrations, you have two test users:

- **John Doe**: Firebase UID `mock-fb-uid-john-doe`, Email: john.doe@example.com
- **Jane Smith**: Firebase UID `mock-fb-uid-jane-smith`, Email: jane.smith@example.com

There are also ~15 seeded lab exams across different medical categories.

## Authentication: Two Modes

**Development mode** (what I use for testing):
- Set `app.UseDevAuth()` in Program.cs
- No Firebase token needed
- Send `X-User-Id: mock-fb-uid-john-doe` header instead
- Example: `curl -H "X-User-Id: mock-fb-uid-john-doe" http://localhost:5000/api/appointments`

**Production mode** (what should be deployed):
- Set `app.UseFirebaseAuth()` in Program.cs
- Requires valid Firebase ID token
- Send `Authorization: Bearer <firebase-token>` header
- Returns 401 if token is expired, invalid, or user doesn't exist in DB

Currently set to **dev mode** by default because it's easier to test.

## What I Built

**User Management:**
- Register users with Firebase UID, name, email, phone, date of birth
- Update profile (phone number and DOB only)
- Phone numbers must have exactly 11 digits (any format accepted)
- Each Firebase UID maps to one internal database user ID

**Appointments:**
- Book appointments for lab exams at specific locations
- System auto-assigns a doctor based on the exam category
- Prevents double-booking: no overlapping appointments within 1 hour
- Users can only see and modify their own appointments
- Appointments must be between now and 1 year from now

**Security (the main focus):**
- Firebase UID → internal user ID mapping is 1:1 and isolated
- User 1's Firebase token cannot access User 2's data
- Middleware sets `HttpContext.Items["UserId"]` after auth
- Controllers validate that authenticated user ID matches requested resource
- 40 unit tests covering this isolation (UserService, AppointmentService, middleware)

**Data Validation:**
- Phone: exactly 11 digits
- Appointments: future dates only, within 1 year, no conflicts
- Locations: must be from a predefined list of 5 clinics
- DOB: must be in the past

## What I Deliberately Left Out

**No soft deletes.** Appointments and users are hard-deleted. I would add a `DeletedAt` timestamp and filter queries, but ran out of time.

**No appointment cancellation endpoint.** You can update an appointment but not cancel it. Would add `DELETE /api/appointments/{id}` that sets status to Cancelled.

**No pagination.** `GET /api/appointments` returns all appointments for a user. Works fine for a demo, breaks with 1000+ appointments. Would add `?page=1&pageSize=20` params.

**No rate limiting.** Any user can spam the API. Would add rate limiting middleware per user ID or IP.

**No logging.** Console output only. Production needs Serilog with structured logs and correlation IDs.

**No health checks.** No `/health` endpoint for monitoring or load balancers.

**No admin endpoints.** Admins can't view all users or manage appointments. Would need role-based authorization.

## What I'd Do With Another Day

**Production deployment:**
- Dockerfile and docker-compose.yml
- GitHub Actions CI/CD pipeline
- Environment-based config (dev/staging/prod)
- Secrets management (not hardcoded in appsettings.json)

**Security hardening:**
- Add security headers (HSTS, CSP, X-Frame-Options)
- CORS policy configuration
- Input sanitization audit
- Proper error messages (hide stack traces in prod)

**Monitoring:**
- Application Insights or similar
- Health check endpoints
- Performance metrics

**Integration tests:**
- Test full API flows with a real database
- Currently only have unit tests with in-memory DB

## Tests

```bash
dotnet test
```

**40 tests total across 3 test classes:**
- `UserServiceTests` (21 tests): Profile updates, phone validation, Firebase UID mapping
- `AppointmentServiceTests` (7 tests): Appointment isolation, conflict detection
- `FirebaseAuthMiddlewareTests` (12 tests): The critical ones - ensures User 1's Firebase token never maps to User 2's internal ID

All tests use in-memory databases and are isolated. FluentAssertions for readability.

## Known Issues

**DevAuthMiddleware is enabled by default.** This bypasses Firebase authentication entirely. Change to `app.UseFirebaseAuth()` before deploying anywhere.

**No HTTPS enforcement.** Running on HTTP locally. Production needs HTTPS redirects and HSTS headers.

**Database connection string in appsettings.json.** Should be in environment variables or a secrets manager.

**Seeded data has mock Firebase UIDs.** These don't exist in any Firebase project. Real users need real Firebase accounts.

## API Endpoints

**Public:**
- `POST /api/users` - Register new user

**Protected (requires auth):**
- `GET /api/users/{id}` - Get user profile
- `PUT /api/users/{id}/profile` - Update phone/DOB
- `GET /api/appointments` - Get my appointments
- `POST /api/appointments` - Book appointment (requires labExamId, appointmentDate, location)
- `PUT /api/appointments/{id}` - Update appointment date or location
- `GET /api/labexams` - List all available lab exams

See `/swagger` for full API docs and request/response examples.

## Project Structure

```
FuncHealthDemo/
├── Controllers/          # HTTP endpoints
├── Services/             # Business logic (UserService, AppointmentService)
├── Middleware/           # Auth (DevAuthMiddleware, FirebaseAuthMiddleware)
├── Entities/             # Database models (User, Appointment, LabExam)
├── DataBase/             # EF Core DbContext
├── Migrations/           # Database migrations
└── DTO/                  # Request/response models

FuncHealthTests/          # xUnit tests with FluentAssertions
```

## Dependencies

- ASP.NET Core 10 Web API
- Entity Framework Core 10 (PostgreSQL provider)
- FirebaseAdmin SDK (token verification)
- xUnit, FluentAssertions, Moq (testing)

That's it. No unnecessary packages.
