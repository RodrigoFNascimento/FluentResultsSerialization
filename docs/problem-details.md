# Problem Details (RFC 7807)

FluentResults.HttpMapping provides first-class support for producing
[RFC 7807](https://datatracker.ietf.org/doc/html/rfc7807) **Problem Details**
responses for failed results.

This document explains **what Problem Details are**, **when to use them**, and
**how this package models them**.


## What Are Problem Details?

Problem Details is a standardized JSON format for HTTP error responses.

A typical Problem Details response looks like this:

```json
{
  "type": "about:blank",
  "title": "Invalid request data.",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "errors": {
    "email": ["The email field is required."]
  }
}
```

Key goals of the format:

- Machine-readable error responses
- Consistent structure across APIs
- Extensible via custom members
- Works across clients and frameworks

ASP.NET already supports this format via `Results.Problem(...)`.


## How Problem Details Fit This Package

FluentResults.HttpMapping treats **Problem Details as the canonical
representation of failures**.

The idea is simple:

- **Domain errors live in FluentResults**
- **HTTP concerns live in mapping rules**
- **Problem Details bridge the two**

Instead of throwing exceptions or manually creating error responses,
you define rules that map failures into Problem Details responses.


## Using `Problem(...)` in the DSL

You create Problem Details responses using the `Problem(...)` method
on a rule builder:

```csharp
mapper
    .WhenFailure()
    .Problem(p => p
        .WithStatus(HttpStatusCode.InternalServerError)
        .WithTitle("Unexpected failure")
    );
```

This rule says:

> When a result fails, return a Problem Details response with status 500.


## Status, Title and Detail

The most common Problem Details members are:

- **status** – HTTP status code
- **title** – short, human-readable summary
- **detail** – detailed explanation

They can be static or computed dynamically:

```csharp
.Problem(p => p
    .WithStatus(HttpStatusCode.BadRequest)
    .WithTitle("Invalid input")
    .WithDetail(ctx => ctx.FirstReason<IError>().Message)
);
```

This keeps HTTP formatting logic close to the mapping layer, not the domain.


## Extensions

Problem Details allows arbitrary extension members.

This package exposes that explicitly via `WithExtension`:

```csharp
.WithExtension("error-codes", ctx => ctx.GetMetadata("error-codes"))
```

Extensions are ideal for:

- Error codes
- Correlation IDs
- Validation details
- Client-specific metadata

They are **not** intended for headers or transport-level concerns.


## Validation Errors

Validation failures are common enough to deserve helpers.

### Metadata-based validation

If validation errors are stored as metadata:

```csharp
.Problem(p => p.WithValidationProblem());
```

This produces a `400 Bad Request` with an `errors` extension.

### Typed validation errors

If validation errors are part of the error itself:

```csharp
.Problem(p => p.WithValidationProblem<ValidationError>(e => e.Errors));
```

This avoids forcing validation data into metadata when it already exists
as structured data.


## Headers vs Problem Details

Problem Details describe the **response body**, not headers.

This package enforces that separation:

- Headers are defined **on the rule**
- Problem Details define **only the body**

Example:

```csharp
mapper
    .WhenError<SecurityError>()
    .WithHeader("WWW-Authenticate", "Bearer")
    .Problem(p => p
        .WithStatus(HttpStatusCode.Unauthorized)
        .WithTitle("Authentication failed")
    );
```

This keeps HTTP semantics clear and predictable.


## When *Not* to Use Problem Details

Not every error response should use Problem Details.

Examples:

- `401 Unauthorized` with OAuth error bodies
- `404 Not Found` without a body
- File downloads
- Redirects

For these cases, use `Map(...)` instead:

```csharp
.WhenError<NotFoundError>()
.Map(_ => Results.NotFound());
```

The DSL intentionally allows both styles.


## Design Philosophy

Problem Details support in this package is:

- Explicit
- Opt-in per rule
- RFC-aligned
- Minimal

It avoids:

- Automatically wrapping all failures
- Inferring HTTP behavior from error types
- Hidden conventions

If you choose Problem Details, you do so deliberately — and you control
exactly what is sent to the client.


## Summary

- Problem Details are the default failure representation
- Use `Problem(...)` to define them
- Status, title, detail, and extensions are explicit
- Validation helpers exist for common cases
- Headers are handled separately
- `Map(...)` remains available for non-Problem responses

Problem Details are a tool — not a requirement — and FluentResults.HttpMapping
treats them that way.
