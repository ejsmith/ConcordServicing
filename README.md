# Concord Servicing

## ASP.NET Backend

How to run entire application backend and frontend:

- Run `docker compose up`
- Run the `https` launch profile in Visual Studio or Rider

## Vite / SvelteKit Frontend

- Run `npm run dev`

## Features

- [x] ASP.NET Core backend API
- [x] Swagger documentation
- [x] Mediator pattern for message handling
  - [x] CSS handlers in separate project and can be disabled for local dev
  - [x] Retry policy for transient errors
  - [x] Multiple handlers executed sequentially (CSS, SQL)
- [x] Ability to easily publish and listen to events
  - [ ] Scale out support (all web instances get the events)
- [x] SignalR for real-time event forwarding to the UI
  - [ ] Scale out support (events can be sent to clients connected to other web instances)
- [x] Docker compose for service dependencies (SQL Server, Redis, RabbitMQ)
- [ ] Redis for caching
- [x] EF Core persistence
- [x] Editorconfig for ensuring code consistency
- [x] Dockerfile for building docker image
- [x] API problem details standard error handling response
- [x] Health checks
- [x] OpenTelemetry logs, metrics, traces
  - [x] OpenTelemetry Prometheus metrics scraping endpoint
- [ ] Integration tests for talking to the API
  - [ ] Ability to inspect messages
- [ ] Authentication
  - [ ] Ability for customer to login
  - [ ] ActiveDirectory login for internal users
- [ ] Authorization
- [ ] Multi-tennancy support
  - [ ] Automatically enforced on messages based on current user
- [x] Ability to easily register startup tasks to run at the start of the app
- [x] Sample data created locally when no data exists
- [ ] Service bus pattern for handling messages out of process
  - [ ] Queue job handling
  - [ ] Ability to run queues in process locally
  - [ ] Persisted to SQL Server for durability
  - [ ] Can scale out to multiple app instances while running in proc
  - [ ] Can be run out of process and be scaled separately
- [x] SPA frontend
  - [x] Static site that can be built and hosted by ASPNET Core in the same domain to avoid CORS requests
  - [x] Ability to just work on frontend and point to a hosted API
  - [x] Vite for dev
  - [x] Proxy to API which can be hosted or local
