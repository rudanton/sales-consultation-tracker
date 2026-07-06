# AI Development Rules

## General

Always read the documentation inside the docs folder before generating code.

---

## Priority

Follow documents in this order.

1. SRS
2. Requirements
3. Database
4. Architecture
5. Coding Guidelines

---

## Code Style

- Use Clean Architecture.
- Use Repository Pattern.
- Use Dependency Injection.
- Use Entity Framework Core.
- Use SQLite.

---

## API

- RESTful API only.
- DTO must be separated from Entity.
- Validation required.

---

## Database

Do not modify schema without explicit instruction.

Do not store binary files in SQLite.

Only save file paths.

---

## Frontend

Keep components small.

One responsibility per component.

Avoid duplicated UI.

---

## General Rules

Readable code is preferred over clever code.

Avoid unnecessary third-party packages.

Do not generate dead code.

Do not create duplicate functionality.

Always keep documentation synchronized.

Update TODO.md and CHANGELOG.md after completing a feature.