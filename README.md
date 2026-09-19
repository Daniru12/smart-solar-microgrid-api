# Smart Solar Microgrid - Web API

This is the central C# .NET Web API for the Smart Solar Microgrid project. It handles all backend business rules, coordinates interactions between the Web Application and the Android Mobile Application, and connects to MongoDB.

## Architecture

This project is built using a **Component-wise / Feature-based Architecture**. It includes four main components, each designed to be independently developed by a specific team member:

- **Identity & Prosumer Management** (`Components/Identity/`)
  - Handles authentication, backoffice/grid operator management, and prosumer registration via NIC.
- **Microgrid Station & Energy Slots** (`Components/Microgrid/`)
  - Manages solar stations, their locations, capacities, and the available energy booking slots.
- **Reservations & Booking Management** (`Components/Reservations/`)
  - Handles the 7-day future booking rules, 12-hour modification rules, and the complete reservation lifecycle.
- **Operations (QR & Maps)** (`Components/Operations/`)
  - Generates secure transaction data for QR codes, validates them during energy transfer, and manages map data.

## Getting Started

### Prerequisites
- [.NET 8 SDK (or newer)](https://dotnet.microsoft.com/download)
- [MongoDB](https://www.mongodb.com/try/download/community) (Running locally on `mongodb://localhost:27017` or configured in `appsettings.json`)

### Running the API

1. Open your terminal in this directory.
2. Build the project to resolve dependencies:
   ```bash
   dotnet build
   ```
3. Run the project:
   ```bash
   dotnet run
   ```
4. Access the Swagger UI for testing endpoints by navigating to the generated `localhost` URL (usually `https://localhost:5001/swagger` or via the OpenAPI endpoint).

## Contribution Rules
- Always keep important business logic in the **Services**, not the Controllers.
- All database operations should be funneled through the **Repositories**.
- Before creating a pull request, ensure `dotnet build` succeeds with 0 warnings.