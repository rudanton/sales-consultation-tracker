# Architecture

Frontend

- React
- TypeScript
- Tailwind

Backend

- ASP.NET Core

Database

- SQLite

Storage

/files

Architecture

Presentation

↓

Service

↓

Repository

↓

SQLite

No business logic inside Controller.

Repository only accesses database.

Service handles business logic.