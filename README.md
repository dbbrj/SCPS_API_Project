# SCPS Weather API Project

ASP.NET Core 8 MVC application that fetches current weather snapshots from the [IBM Weather Company API](https://developer.weather.com) (Currents on Demand endpoint) and stores them in a local SQL Server database. The UI shows the full history as a chart and table.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server LocalDB (included with Visual Studio)
- `dotnet-ef` CLI tool:
  ```
  dotnet tool install --global dotnet-ef
  ```

## Setup

### 1. Clone and restore

```bash
git clone <repo-url>
cd SCPS_API_Project/SCPS_API_Project
dotnet restore
```

### 2. Set your API key

The API key is stored in user secrets and never committed to the repo:

```bash
dotnet user-secrets set "WeatherApi:ApiKey" "YOUR_API_KEY_HERE"
```

### 3. Create the database

```bash
dotnet ef database update
```

### 4. Run

```bash
dotnet run
```

Then open `https://localhost:7105` in your browser and navigate to **Weather**.

## Configuration

All settings are in `appsettings.json`:

| Key | Default | Description |
|---|---|---|
| `WeatherApi:Latitude` | `55.6722880` | Location latitude (Stigs Bjergby) |
| `WeatherApi:Longitude` | `11.4797220` | Location longitude (Stigs Bjergby) |
| `WeatherApi:FetchIntervalMinutes` | `10` | How often a snapshot is auto-fetched (minutes) |

## How it works

- On startup the app immediately fetches one snapshot, then repeats on the configured interval (`WeatherFetchService` — Singleton BackgroundService pattern).
- Each fetch calls `GET /v3/wx/observations/current` (Currents on Demand) and appends one row to the `WeatherModel` table. The accumulating rows are the historical data.
- The **Weather** page loads the 100 most recent snapshots and displays them as a line chart (temperature °C on the left axis, wind km/h on the right axis) and a table below.
- Click **Fetch New Snapshot** to trigger a manual fetch, then refresh the page to see the new data point.

## Known issues

None currently known.
