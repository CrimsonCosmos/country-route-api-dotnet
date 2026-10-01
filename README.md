# Country Route API: C# .NET + Azure

C# / ASP.NET Core version of the C.H. Robinson interview project (Options 1 and 2
combined), designed for C.H. Robinson's own stack: .NET for the API, React for the UI,
Azure for hosting. A JavaScript/Vercel version of the same project lives in a separate repo,
`country-route-api`.

Given a three-letter North American country code, it returns the ordered list of countries
a driver passes through going from the USA to that destination.

```
GET /PAN  ->  {"destination":"PAN","list":["USA","MEX","GTM","HND","NIC","CRI","PAN"]}
GET /BLZ  ->  {"destination":"BLZ","list":["USA","MEX","BLZ"]}
```

Live URL: _add after deploying_

- `/` is the React UI: type a code, see the route as a list.
- `/PAN` (any three-letter code) is the JSON API. The UI calls the same endpoint at `/api/PAN`.
- `/api` returns usage info and the supported codes.

## Design

| Path | Role |
| --- | --- |
| `src/CountryRoute.Api/CountryGraph.cs` | Border map (adjacency list) and `FindRoute`, a breadth-first search. |
| `src/CountryRoute.Api/Program.cs` | ASP.NET Core minimal API: routes, validation, JSON responses, static UI hosting. |
| `tests/CountryRoute.Tests/` | xUnit unit tests for the graph plus integration tests of the HTTP endpoints through `WebApplicationFactory`. |
| `web/` | React + Vite UI. `vite build` writes into the API's `wwwroot`, so one app serves both. |
| `deploy.sh` | Builds everything and deploys to Azure App Service with the Azure CLI. |

BFS gives the route with the fewest border crossings and keeps working if the map gains loops
or new countries; only the `Borders` table needs to change. The API has no third-party
runtime dependencies.

### Why one App Service

The UI and API ship as one deployable: ASP.NET Core serves the built React files from
`wwwroot`, so there is no CORS setup and nothing extra to host. App Service on the **Free (F1)**
tier costs nothing, which is plenty for a demo. The trade-off is that F1 has a daily CPU quota and
cold starts after idling; move to `SKU=B1` (a few dollars a month) if that matters.

## Assumptions

1. **"Route" means the fewest border crossings.** The map is nearly a line; `SLV` goes
   `USA, MEX, GTM, SLV` rather than via `HND`.
2. **The list includes both endpoints** (`USA` first, destination last), as in the examples.
3. **Input is case-insensitive** (`/pan` works); responses use upper case.
4. **`/USA` returns `["USA"]`** (no border crossed). **`/CAN` returns `["USA","CAN"]`.**
5. **Errors:** a malformed code sent to `/api/{code}` gives `400`; a well-formed code outside
   the map (e.g. `FRA`) gives `404`. Both return a JSON `error`. At the top-level path
   only three-letter codes reach the API, so anything else is a plain 404.
6. **The map is land borders only**, exactly as given in the brief.
7. **Both options are delivered.** The API is the primary deliverable; the UI is a thin client over it.

## Run locally

Requires the .NET 10 SDK and Node 20+.

```bash
dotnet test                                   # unit + integration tests
(cd web && npm ci && npm run build)           # build UI into wwwroot
dotnet run --project src/CountryRoute.Api     # http://localhost:5000 (or the port it prints)
```

For UI hot reload, run the API with `ASPNETCORE_URLS=http://localhost:5080` and
`npm run dev` in `web/`; Vite proxies `/api` to it.

## Deploy to Azure

```bash
az login
./deploy.sh my-unique-app-name
```

The script creates a resource group and Linux App Service plan, creates the web app on the
`DOTNETCORE:10.0` runtime, enforces HTTPS, and zip-deploys the published app. Override
`RG`, `LOCATION`, `PLAN`, `SKU`, or `RUNTIME` through environment variables. To remove everything:
`az group delete -n country-route-rg`.
