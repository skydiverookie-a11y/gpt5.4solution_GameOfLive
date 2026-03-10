# Game of Life — Coding Challenge

You are being evaluated. This is a benchmark comparing different AI models on the same task. Your output will be directly compared against other models in terms of code quality, architecture, correctness, and user experience. Treat this as your portfolio piece — show what you're capable of. Make smart architectural decisions, write clean code, and deliver a polished result.

You are tasked with implementing **Conway's Game of Life** as a full-stack web application using the provided project skeleton.

## Tech Stack

- **Backend:** .NET 10 Web API (C#) — the skeleton is already set up with controllers, CORS, and Swagger
- **Frontend:** Angular 21 (zone-less, no zone.js) — the skeleton is already set up with routing, HttpClient, and a proxy to the backend API. **Important:** Angular 21 runs without zone.js. Change detection does not trigger automatically on async operations. You must handle this explicitly (e.g. signals, markForCheck, or other appropriate mechanisms).

## Architecture Constraint

**All game logic MUST run server-side.** The frontend is a pure UI client — it renders the grid, captures user input, and calls the API. No game rules, no grid computation, no simulation logic in the frontend.

---

## Functional Requirements

### 1. Grid Management

- Configurable grid dimensions (minimum 10×10, maximum 100×100)
- Default grid size: 30×30
- Toggle individual cells (alive/dead) via click
- Clear the entire grid (all cells dead)
- Randomize the grid with a configurable density (0–100%)

### 2. Game Rules (Conway's Rules — must be exact)

- Any live cell with **fewer than 2** live neighbours → dies (underpopulation)
- Any live cell with **2 or 3** live neighbours → survives
- Any live cell with **more than 3** live neighbours → dies (overpopulation)
- Any dead cell with **exactly 3** live neighbours → becomes alive (reproduction)
- **Grid edges: wrapping (toroidal)** — cells on the edge treat the opposite edge as their neighbour

### 3. Simulation Control

- **Step:** Advance exactly one generation
- **Play/Pause:** Auto-advance with configurable speed (1–30 generations per second)
- **Generation counter:** Display the current generation number
- **Reset:** Return to generation 0 with the initial pattern (the pattern that was set before the first step/play)

### 4. Predefined Patterns

The following patterns must be loadable via a UI control (dropdown, button group, or similar):

| Pattern | Type |
|---|---|
| **Glider** | Spaceship |
| **Blinker** | Period 2 oscillator |
| **Pulsar** | Period 3 oscillator |
| **Gosper Glider Gun** | Gun (creates gliders) |
| **Lightweight Spaceship (LWSS)** | Spaceship |

Loading a pattern should place it in the center of the grid (or a reasonable position) and reset the generation counter.

### 5. API Requirements

- **REST API** — design the endpoints as you see fit
- The API should handle:
  - Getting/setting grid state
  - Stepping the simulation
  - Starting/stopping auto-play
  - Loading patterns
  - Configuring grid size and speed

### 6. UI Requirements

- Visual grid rendering (Canvas, SVG, or HTML table/div-based)
- Alive cells must be visually distinct from dead cells
- Controls for: Step, Play/Pause, Speed slider/input, Clear, Randomize
- Pattern selector (dropdown or similar)
- Grid size configuration (width × height inputs)
- Generation counter display
- Responsive layout — usable on 1920×1080 and 1366×768 screens

---

## Non-Functional Requirements

These are not explicitly tested but will be evaluated:

- **Clean Architecture** — proper separation of concerns (controllers, services, models)
- **Error Handling** — meaningful HTTP status codes and error messages
- **Performance** — a 30×30 grid should animate smoothly at 30 generations/second
- **Scalability** — Design the backend with future growth in mind. Grid sizes may increase significantly in upcoming iterations (e.g. 10,000×10,000). Choose data structures and algorithms that do not degrade catastrophically at scale. You are not required to support this now, but your architecture should not preclude it.
- **Code Quality** — readable code, consistent naming, no unnecessary duplication

---

## Getting Started

1. Backend: `cd backend && dotnet run --project GameOfLife.Api`
   - API runs on `http://localhost:5000`
   - Swagger UI: `http://localhost:5000/swagger`
2. Frontend: `cd frontend && npm start`
   - App runs on `http://localhost:4200`
   - API calls are proxied to `http://localhost:5000`

## What NOT to Change

- Do not change the backend port (5000) or frontend port (4200)
- Do not remove the health endpoint (`GET /api/health`)
- Do not remove the proxy configuration
- Do not add game logic to the frontend
- **Do not access, read, or modify any files outside this project directory.** Your working directory is the project root — stay within it.

Good luck!
