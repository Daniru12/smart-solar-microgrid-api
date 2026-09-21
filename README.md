# Smart Solar Microgrid Trading System

## Implementation Specification & Development Context

> **Purpose:** This README provides the technical implementation context for AI-assisted development tools such as Cursor.
> Before modifying or generating code, understand and follow the architecture, technologies, business rules, data flow, and project constraints described below.

---

# 1. Project Overview

The **Smart Solar Microgrid Trading System** is an enterprise client-server application developed for the SE4040 – Enterprise Application Development module.

The system manages:

* Solar prosumers
* Microgrid nodes
* Energy slots
* Energy reservations
* Grid operators
* Backoffice users
* Energy transfer transactions

The application consists of three major parts:

```text
┌───────────────────────────────────────────────┐
│              Client Applications              │
│                                               │
│  ┌─────────────────┐    ┌─────────────────┐  │
│  │ Web Application │    │ Android Mobile  │  │
│  │                 │    │ Application     │  │
│  └────────┬────────┘    └────────┬────────┘  │
│           │                      │            │
└───────────┼──────────────────────┼────────────┘
            │      REST API        │
            └──────────┬───────────┘
                       ▼
             ┌───────────────────┐
             │    C# Web API     │
             │                   │
             │ Business Logic    │
             │ Authentication    │
             │ Validation        │
             │ Authorization     │
             └─────────┬─────────┘
                       │
                       ▼
             ┌───────────────────┐
             │     MongoDB       │
             │   NoSQL Database  │
             └───────────────────┘
```

The Web and Android applications act as client/UI layers. The central Web API handles business logic and communicates with MongoDB. The assignment explicitly requires both clients to interact with the service through RESTful API calls rather than directly accessing the database.

---

# 2. Core Architecture

## 2.1 Architecture Pattern

The backend follows the **FAT Service Pattern**.

This means:

```text
Client
   │
   │ Request
   ▼
Web API
   │
   ├── Authentication
   ├── Authorization
   ├── Validation
   ├── Business Rules
   ├── Data Processing
   └── Database Operations
   │
   ▼
MongoDB
```

The clients should remain primarily responsible for:

* UI rendering
* User interaction
* Input collection
* API communication
* Displaying API responses
* Local Android persistence where required

The Web API should remain responsible for:

* Business logic
* Validation
* Authorization
* Reservation rules
* Transaction verification
* Database operations
* State changes

---

# 3. Technology Stack

## Backend

```text
Language:       C#
Framework:      ASP.NET Web API
Database:       MongoDB
Server:         Windows IIS
Architecture:   FAT Service
Communication:  REST API
```

The assignment specifies a C# Web API deployed on Windows IIS with a NoSQL server-side database such as MongoDB.

---

## Web Application

The Web Application is a client-side UI.

Allowed UI technologies according to the assignment include:

```text
Bootstrap 5
Tailwind CSS
React.js
```

The actual repository technology must be preserved once selected.

The Web Application must not directly connect to MongoDB.

```text
Web UI
   │
   ▼
REST API
   │
   ▼
MongoDB
```

---

## Android Application

The mobile application must be:

```text
Pure Native Android
SQLite
Google Maps API
QR Scanner
REST API
```

### Important

Do NOT introduce:

```text
React Native
Flutter
Ionic
Xamarin
.NET MAUI
Other cross-platform frameworks
```

The assignment explicitly requires a pure native Android application and SQLite local persistence.

---

# 4. User Roles

The system contains three main user categories.

```text
                 Users
                   │
        ┌──────────┼──────────┐
        ▼          ▼          ▼
   Backoffice   Operator   Prosumer
```

---

## 4.1 Backoffice

Backoffice users manage administrative operations.

Responsibilities:

* User management
* Prosumer management
* Microgrid node management
* Account activation/deactivation
* Operational schedule management
* Monitoring system information

Only Backoffice users should have access to administration-specific functionality.

---

## 4.2 Grid Operator

Grid Operators handle operational activities.

Responsibilities:

* Monitor reservations
* View booking information
* Manage energy transfer operations
* Scan QR codes
* Verify transactions
* Finalize energy transfers
* View nearby microgrid nodes

Grid Operators can use both Web and Android clients.

---

## 4.3 Solar Prosumer

A Solar Prosumer represents a property owner with a solar panel array.

Responsibilities:

* Register
* Login
* Manage profile
* Request account deactivation
* View energy slots
* Create reservations
* Modify reservations
* Cancel reservations
* View booking history
* View pending bookings
* Generate transaction QR code
* View nearby microgrid nodes

---

# 5. Backend Project Structure

Recommended backend organization:

```text
WebAPI/
│
├── Controllers/
│   ├── AuthController.cs
│   ├── UserController.cs
│   ├── ProsumerController.cs
│   ├── MicrogridController.cs
│   ├── BookingController.cs
│   └── TransactionController.cs
│
├── Models/
│   ├── User.cs
│   ├── Prosumer.cs
│   ├── SolarStation.cs
│   ├── EnergyBookingSlot.cs
│   └── EnergyReservation.cs
│
├── DTOs/
│   ├── LoginRequest.cs
│   ├── RegisterProsumerRequest.cs
│   ├── BookingRequest.cs
│   └── BookingResponse.cs
│
├── Services/
│   ├── AuthService.cs
│   ├── UserService.cs
│   ├── ProsumerService.cs
│   ├── MicrogridService.cs
│   ├── BookingService.cs
│   └── TransactionService.cs
│
├── Repositories/
│   ├── UserRepository.cs
│   ├── ProsumerRepository.cs
│   ├── MicrogridRepository.cs
│   └── BookingRepository.cs
│
├── Middleware/
│   └── ...
│
├── Configuration/
│   └── MongoDbSettings.cs
│
├── Program.cs
└── appsettings.json
```

> Adapt this structure to the actual repository. Do not create unnecessary layers if the existing project follows a different structure.

---

# 6. Backend Responsibilities

The backend is the central source of truth.

Every important operation should follow:

```text
Request
   ↓
Controller
   ↓
Service / Business Logic
   ↓
Repository / Database Access
   ↓
MongoDB
   ↓
Response
```

Example:

```text
Create Reservation
       │
       ▼
BookingController
       │
       ▼
BookingService
       │
       ├── Validate User
       ├── Validate Slot
       ├── Validate Date
       ├── Validate Availability
       ├── Apply Business Rules
       └── Create Reservation
       │
       ▼
MongoDB
       │
       ▼
API Response
```

---

# 7. Authentication

Authentication should be centralized in the Web API.

General flow:

```text
User
 │
 │ Login
 ▼
Web / Android
 │
 │ POST /api/auth/login
 ▼
Web API
 │
 ├── Validate credentials
 ├── Identify role
 └── Generate authentication response
 │
 ▼
Client
 │
 └── Store required authentication information
```

The client must not independently implement critical authorization rules.

---

# 8. Authorization

Role-based access must be enforced by the API.

Example:

```text
Backoffice
 ├── User Management
 ├── Prosumer Management
 └── Microgrid Management

Grid Operator
 ├── Booking Monitoring
 ├── QR Verification
 └── Energy Transfer

Prosumer
 ├── Profile
 ├── Reservations
 ├── Booking History
 └── QR Transaction
```

The API must verify the user's role before executing protected operations.

---

# 9. MongoDB Database

The assignment identifies four primary data areas:

```text
User Details
Solar Station Information
Energy Booking Slots
Energy Reservation
```

These correspond to the required database collections described in the marking scheme.

Recommended conceptual structure:

```text
MongoDB
│
├── UserDetails
│
├── SolarStationInfo
│
├── EnergyBookingSlots
│
└── EnergyReservation
```

---

# 10. UserDetails Collection

Purpose:

Store application users and their roles.

Conceptual fields:

```text
UserDetails
├── _id
├── nic
├── name
├── email
├── password / password hash
├── role
├── status
├── createdAt
└── updatedAt
```

Possible roles:

```text
BACKOFFICE
GRID_OPERATOR
PROSUMER
```

Do not expose password hashes or sensitive authentication information through API responses.

---

# 11. SolarStationInfo Collection

Stores microgrid node information.

Conceptual fields:

```text
SolarStationInfo
├── _id
├── stationId
├── stationName
├── latitude
├── longitude
├── capacity
├── batterySlots
├── availableSlots
├── schedule
├── status
├── createdAt
└── updatedAt
```

GPS coordinates are required so the mobile application can display nearby nodes on Google Maps.

---

# 12. EnergyBookingSlots Collection

Represents available energy trading slots.

Conceptual fields:

```text
EnergyBookingSlots
├── _id
├── stationId
├── slotDate
├── startTime
├── endTime
├── capacity
├── availableCapacity
└── status
```

The actual schema must match the implementation and database design used by the team.

---

# 13. EnergyReservation Collection

Stores user reservations.

Conceptual fields:

```text
EnergyReservation
├── _id
├── reservationId
├── prosumerNIC
├── stationId
├── slotId
├── reservationDate
├── status
├── qrCode
├── createdAt
├── updatedAt
└── completedAt
```

Possible statuses:

```text
PENDING
APPROVED
CANCELLED
COMPLETED
REJECTED
```

Use the actual status values implemented in the project.

---

# 14. Reservation Business Logic

Reservation creation must be validated by the backend.

```text
Create Reservation
       │
       ▼
Is User Valid?
       │
       ├── NO → Reject
       │
       ▼
Is Slot Available?
       │
       ├── NO → Reject
       │
       ▼
Is Date Within Allowed Period?
       │
       ├── NO → Reject
       │
       ▼
Create Reservation
       │
       ▼
Return Confirmation
```

The assignment specifies that reservations must be scheduled within 7 days. Updates and cancellations require at least 12 hours' notice.

These rules should be enforced server-side.

---

# 15. Reservation Update

Flow:

```text
User
 │
 ▼
Select Existing Reservation
 │
 ▼
API
 │
 ├── Verify ownership / permission
 ├── Check reservation status
 ├── Check current time
 ├── Check 12-hour rule
 ├── Validate requested slot
 └── Update reservation
 │
 ▼
MongoDB
```

The client should not be trusted to enforce the 12-hour restriction by itself.

---

# 16. Reservation Cancellation

Flow:

```text
Cancel Request
      │
      ▼
Web API
      │
      ├── Verify reservation
      ├── Check authorization
      ├── Check 12-hour restriction
      └── Update status
      │
      ▼
MongoDB
```

Prefer status changes where appropriate instead of permanently deleting transaction history.

---

# 17. Microgrid Node Deactivation

A node cannot be deactivated when active energy reservations exist.

```text
Deactivate Node
      │
      ▼
Find Active Reservations
      │
      ├── Found
      │     ↓
      │   Reject
      │
      └── None
            ↓
       Deactivate Node
```

This validation belongs in the Web API.

---

# 18. QR Transaction Workflow

After an energy reservation is approved:

```text
Reservation Approved
        │
        ▼
Generate Transaction Identifier
        │
        ▼
Generate QR Code
        │
        ▼
Display QR to Prosumer
```

Grid Operator workflow:

```text
Grid Operator
      │
      ▼
Scan QR
      │
      ▼
Extract Transaction Information
      │
      ▼
Call Web API
      │
      ▼
Verify Transaction
      │
      ├── Invalid → Reject
      │
      └── Valid
           │
           ▼
      Finalize Transfer
           │
           ▼
      Mark Job Completed
```

The assignment specifically requires the operator to scan the prosumer QR code, verify it against server data, and finalize the energy transfer.

---

# 19. Google Maps Integration

The mobile application uses stored station coordinates.

```text
MongoDB
   │
   │ latitude / longitude
   ▼
Web API
   │
   ▼
Android Application
   │
   ▼
Google Maps
   │
   ├── Station Marker
   └── Station Details
```

The assignment requires nearby grid nodes to be displayed using Google Maps API.

---

# 20. Android SQLite

SQLite is used for local persistence in the native Android application.

The assignment specifies local SQLite usage for mobile user management and persistence.

Conceptual flow:

```text
Android
   │
   ├── API Data
   │
   └── SQLite
          │
          ├── Local user/reference data
          └── Required local persistence
```

Do not use SQLite as a replacement for the central MongoDB database.

MongoDB remains the server-side source of truth.

---

# 21. API Endpoint Structure

Use RESTful endpoint naming.

Example structure:

```text
/api/auth/login

/api/users
/api/users/{id}

/api/prosumers
/api/prosumers/{nic}

/api/stations
/api/stations/{id}

/api/slots
/api/slots/{id}

/api/reservations
/api/reservations/{id}

/api/transactions/{id}/verify
/api/transactions/{id}/complete
```

These are conceptual examples.

**Do not create endpoints automatically if they do not correspond to an actual implemented requirement.**

Before adding an endpoint:

1. Check existing controllers.
2. Check existing service methods.
3. Check existing models.
4. Check existing routes.
5. Preserve existing API conventions.

---

# 22. HTTP Methods

Use HTTP methods consistently.

```text
GET
    Retrieve data

POST
    Create new data

PUT / PATCH
    Update existing data

DELETE
    Delete data only where appropriate
```

Example:

```text
GET    /api/stations
GET    /api/stations/{id}

POST   /api/stations

PUT    /api/stations/{id}

DELETE /api/stations/{id}
```

For reservation cancellation, a status update may be preferable to physical deletion when historical records need to be preserved.

---

# 23. API Response Handling

API responses should be predictable.

Example success response:

```json
{
  "success": true,
  "message": "Reservation created successfully",
  "data": {}
}
```

Example error:

```json
{
  "success": false,
  "message": "Reservation cannot be cancelled within 12 hours of the scheduled time"
}
```

The exact response structure should follow the existing implementation.

Do not introduce a new response format across the entire application without checking existing code.

---

# 24. Validation

Validation should occur at multiple levels.

### Client Validation

Used for:

* Required fields
* Basic formatting
* User-friendly error messages

### Server Validation

Used for:

* Authentication
* Authorization
* Business rules
* Reservation availability
* Date restrictions
* User permissions
* Transaction verification

### Database Validation

Used for:

* Data structure
* Required values
* Consistency
* Unique identifiers where applicable

The server must remain the final authority for business rules.

---

# 25. Error Handling

The API should handle:

```text
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
500 Internal Server Error
```

Example:

```text
Invalid reservation request
        ↓
400 Bad Request

Unauthenticated user
        ↓
401 Unauthorized

Insufficient permissions
        ↓
403 Forbidden

Reservation not found
        ↓
404 Not Found

Slot already reserved
        ↓
409 Conflict
```

Do not expose internal exceptions, database credentials, stack traces, or sensitive information to clients.

---

# 26. Client Responsibilities

## Web Application

The Web Application should:

* Display UI
* Collect user input
* Call API
* Display API results
* Handle loading states
* Handle errors
* Provide navigation
* Maintain client-side UI state

It should not:

* Connect directly to MongoDB
* Implement authoritative business rules
* Modify server database directly

---

## Android Application

The Android application should:

* Display UI
* Manage user interaction
* Call REST APIs
* Manage required SQLite data
* Display maps
* Scan QR codes
* Display API responses

It should not:

* Connect directly to MongoDB
* Bypass the API
* Implement authoritative business rules locally

---

# 27. Development Rules for Cursor

When modifying this repository, follow these rules.

## Rule 1 — Inspect Before Editing

Before changing code:

```text
1. Inspect project structure
2. Find related controller
3. Find related service
4. Find related model
5. Find database interaction
6. Find the client consuming the API
7. Understand existing flow
8. Then make the smallest required change
```

---

## Rule 2 — Do Not Rewrite Existing Architecture

Do not automatically introduce:

```text
Microservices
CQRS
Event Sourcing
Clean Architecture
Repository Pattern
MediatR
GraphQL
Redis
Kafka
RabbitMQ
```

unless they already exist or are explicitly required.

The assignment requires a centralized FAT Web Service architecture.

---

## Rule 3 — Business Logic Belongs in API

Do not move important business rules into:

```text
React
Web JavaScript
Android UI
SQLite
```

Business rules should be enforced by the Web API.

---

## Rule 4 — No Direct Database Access from Clients

Never implement:

```text
Web → MongoDB

Android → MongoDB
```

Correct:

```text
Web → Web API → MongoDB

Android → Web API → MongoDB
```

---

## Rule 5 — Preserve Existing Code

When implementing a new feature:

```text
Do not rewrite unrelated files.
Do not rename existing APIs unnecessarily.
Do not change database fields without checking dependencies.
Do not remove working functionality.
```

---

## Rule 6 — Follow Existing Naming

Before creating:

```text
Controller
Service
Model
DTO
Endpoint
Database field
```

inspect existing naming conventions and follow them.

---

# 28. Implementation Workflow

For every new feature, use this workflow:

```text
Requirement
     │
     ▼
Identify Business Rule
     │
     ▼
Database Model
     │
     ▼
API Endpoint
     │
     ▼
Service / Business Logic
     │
     ▼
Database Operation
     │
     ▼
API Response
     │
     ▼
Web / Android Client
     │
     ▼
UI Testing
```

Example:

```text
"Create Energy Reservation"

        ↓

Reservation Requirements

        ↓

EnergyReservation Model

        ↓

POST /api/reservations

        ↓

ReservationService

        ↓

Validate:
- User
- Slot
- Availability
- Date
- Business rules

        ↓

MongoDB

        ↓

Response

        ↓

Android UI
```

---

# 29. Testing Workflow

Every feature should be tested through the complete system.

Example:

```text
Android UI
    ↓
API Request
    ↓
Controller
    ↓
Business Logic
    ↓
MongoDB
    ↓
API Response
    ↓
Android UI
```

Do not test only the frontend.

Test:

* Valid input
* Invalid input
* Unauthorized requests
* Incorrect roles
* Missing records
* Duplicate records
* Business-rule violations
* Database failures
* API failures

---

# 30. Git Development

Use meaningful commits.

Good:

```text
feat: implement prosumer registration API

feat: add reservation cancellation validation

fix: prevent station deactivation with active bookings

feat: add QR transaction verification

fix: handle unavailable booking slots
```

Avoid:

```text
update
changes
final
final2
test
asdf
```

The assignment expects development under version control with meaningful, descriptive commits.

---

# 31. Code Quality

When writing C#:

* Use meaningful names.
* Keep methods focused.
* Avoid duplicated business logic.
* Validate inputs.
* Handle exceptions.
* Use asynchronous database/API operations where appropriate.
* Keep controllers lightweight.
* Keep business rules in services.
* Avoid hard-coded configuration values.

When writing Android code:

* Follow the existing native Android architecture.
* Keep UI logic separate from API/database operations where the existing project allows.
* Handle network failures.
* Handle lifecycle issues correctly.
* Avoid blocking the UI thread.
* Validate user input.

When writing Web code:

* Follow the existing framework conventions.
* Keep API calls centralized where possible.
* Handle loading and error states.
* Avoid duplicating backend business logic.

---

# 32. Configuration

Do not hard-code sensitive information.

Examples of values that should be configurable:

```text
MongoDB Connection String
API Base URL
JWT Secret
Google Maps API Key
IIS Configuration
Environment-specific settings
```

Use appropriate configuration files/environment variables according to the existing project setup.

Never commit private credentials to GitHub.

---

# 33. Important Assignment Constraints

The implementation must satisfy the following constraints:

### Backend

```text
C# Web API
MongoDB
IIS
FAT Service Pattern
```

### Web

```text
Web UI
REST API communication
No direct MongoDB access
```

### Android

```text
Pure Native Android
SQLite
REST API
Google Maps
QR scanning
```

### Architecture

```text
Client
   ↓
REST API
   ↓
Business Logic
   ↓
MongoDB
```

## The assignment's marking scheme specifically evaluates the Web API/IIS/MongoDB integration, the FAT-service business logic, native Android with SQLite, the Web UI, Google Maps, QR scanning, and API integration.

# 34. Current Implementation Checklist

Update this section as development progresses.

## Backend

* [ ] C# Web API created
* [ ] MongoDB connection configured
* [ ] Authentication implemented
* [ ] Authorization implemented
* [ ] User management implemented
* [ ] Prosumer management implemented
* [ ] Microgrid node management implemented
* [ ] Energy slot management implemented
* [ ] Reservation management implemented
* [ ] Reservation business rules implemented
* [ ] QR verification implemented
* [ ] Energy transfer completion implemented
* [ ] API error handling implemented
* [ ] IIS deployment completed

## Web Application

* [ ] Login
* [ ] Role-based navigation
* [ ] User management
* [ ] Prosumer management
* [ ] Microgrid management
* [ ] Booking management
* [ ] Dashboard
* [ ] API integration
* [ ] Error handling
* [ ] Responsive UI

## Android

* [ ] Native Android project
* [ ] SQLite
* [ ] Login
* [ ] Registration
* [ ] Profile management
* [ ] Reservation creation
* [ ] Reservation update
* [ ] Reservation cancellation
* [ ] Booking history
* [ ] Pending bookings
* [ ] QR generation
* [ ] QR scanning
* [ ] Server verification
* [ ] Google Maps
* [ ] Nearby stations
* [ ] Operator mode

---

# 35. Final Rule for AI Coding Tools

When working on this project:

> **Do not invent requirements.**

Always use the existing project code, assignment requirements, database design, API contracts, and implementation decisions as the source of truth.

Before implementing a feature:

```text
Understand
    ↓
Inspect
    ↓
Plan
    ↓
Implement
    ↓
Test
    ↓
Verify
```

If an implementation decision is ambiguous, inspect the existing codebase first rather than introducing a new architecture.

The final implementation must remain understandable to the students who developed it, since the assignment includes a supervised viva where students must explain and justify their implementation and development decisions.
