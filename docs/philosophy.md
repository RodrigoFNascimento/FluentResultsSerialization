# Design Philosophy

This document explains why this library exists, what problem it solves, and the design decisions behind it.
It is written primarily for future maintainers (including future-you).

## The problem

ASP.NET Minimal APIs are designed around IResult, while domain and application layers often use `Result` or `Result<T>`.

In real applications, this mismatch usually leads to:

- HTTP concerns leaking into endpoints;
- Repeated if/else or switch logic in controllers;
- Ad-hoc mapping scattered across the codebase;
- Filters or middleware that are difficult to reason about;

The result is code that is harder to maintain, harder to test, and harder to evolve.

## What this library does

This library introduces an explicit mapping layer whose sole responsibility is converting FluentResults results into HTTP responses.

Endpoints return domain results.

HTTP mapping happens afterward.

In exactly one place.

In a predictable way.

 ## Core principles

### 1. Endpoints return domain results, not HTTP results

Endpoints should express business outcomes, not transport decisions.

Instead of returning HTTP responses directly, endpoints return FluentResults results.

The mapping from result to HTTP response happens outside the endpoint.

This keeps business logic free of HTTP concerns.

### 2. HTTP mapping is centralized and declarative

All HTTP behavior is defined in one place during application startup.

Rules are defined declaratively and read top-to-bottom.

This makes HTTP behavior:

- Easy to discover;
- Easy to change;
- Easy to reason about;
- Easy to audit;

There is no hidden logic inside endpoints.

### 3. Rules are explicit and order-dependent

Rules are evaluated in the order they are registered.

The first matching rule wins.

There is no implicit fallback rule for failures.

There is no rule merging.

There is no magic.

This mirrors how developers naturally think about error handling.

### 4. Rules are pure and deterministic

Rules do not:

- Depend on HttpContext;
- Use dependency injection;
- Mutate external state;

Given the same input result, the same rule will always produce the same HTTP response.

This keeps the mapping layer predictable and testable.

### 5. Headers are transport concerns, not body concerns

Headers are defined alongside rules, not inside response bodies.

This keeps responsibilities clear:

- Bodies describe representations;
- Headers describe protocol behavior;

### 6. The library is intentionally opinionated

This library does not attempt to:

- Replace middleware;
- Replace filters;
- Abstract ASP.NET internals;
- Handle every HTTP edge case;

It solves one specific problem well:

Mapping FluentResults results to HTTP responses.

## Design goal summary

- Explicit over implicit;
- Declarative over imperative;
- Centralized over scattered;
- Boring success paths;
- Intentional failure handling;

If something feels strict, it is probably deliberate.