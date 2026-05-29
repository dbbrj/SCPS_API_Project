# SCPS Weather API Project

ASP.NET Core 8 MVC application that fetches weather snapshots from the [IBM Weather Company API](https://developer.weather.com) and stores them in a local SQL Server database. The UI shows the full history of snapshots and allows manual fetches.

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

Then open `https://localhost:7105` in your browser and navigate to **Weather** to see snapshots.

## Configuration

All other settings are in `appsettings.json`:

| Key | Default | Description |
|---|---|---|
| `WeatherApi:Latitude` | `55.673291` | Location latitude (Stigs Bjergby, 4440 Mørkøv) |
| `WeatherApi:Longitude` | `11.478659` | Location longitude (Stigs Bjergby, 4440 Mørkøv) |
| `WeatherApi:Units` | `m` | Unit system — `m` = metric, `e` = imperial |
| `WeatherApi:FetchIntervalMinutes` | `10` | How often a snapshot is auto-fetched |

## How it works

- On startup the app fetches one snapshot immediately, then repeats on the configured interval (`WeatherFetchService` — Singleton/BackgroundService).
- The **Weather** page shows the 50 most recent snapshots. Click **Fetch New Snapshot** to trigger a manual fetch.
- Data is stored in a LocalDB SQL Server database via Entity Framework Core.

## Known issues

None currently known.
