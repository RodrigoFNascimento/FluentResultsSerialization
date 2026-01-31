# FluentResults.HttpMapping

**FluentResults.HttpMapping** is a small, opinionated library that let's you map [FluentResults](https://github.com/altmann/FluentResults) `Result` and `Result<T>` to HTTP responses **declaratively**, **consistently**, and **without leaking HTTP concerns into your domain layer**.


## The problem this package solves

When using **FluentResults** in Web APIs, a common problem quickly appears:

- Domain code returns rich `Result` objects;
- HTTP endpoints must translate those results into status codes, headers, and bodies;
- Mapping logic ends up:
  - Repeated across endpoints;
  - Tightly coupled to ASP.NET;
  - Hard to evolve consistently;

Typical symptoms:

- `if (result.IsFailed)` blocks everywhere;
- Ad-hoc mapping of errors to status codes;
- Validation, authorization, and infrastructure errors mixed together;
- Inconsistent error responses across endpoints;

**FluentResults.HttpMapping** solves this by introducing a **centralized mapping layer**:

- Endpoints return `Result` / `Result<T>`;
- A rule-based DSL decides how results become HTTP responses;
- HTTP behavior is defined once, globally, and reused everywhere;


## Design philosophy

This package is intentionally **small and strict**.

Core principles:

- **Separation of concerns**
  - Domain logic knows nothing about HTTP;
  - HTTP mapping lives in one place;
- **Declarative rules**
  - You describe *what* should happen, not *how*;
- **First match wins**
  - Rules are evaluated in order;
- **Pure mapping**
  - No dependency injection inside rules;
  - No side effects;
- **Minimal API first**
  - Designed for `IResult` and `Results.*`;
  - Optional support for MVC controllers;

This is not a framework. It’s a thin, predictable mapping layer.


## Installation

```bash
dotnet add package FluentResults.HttpMapping
```


## Basic usage

### 1. Register the mapper

Configure your mapping rules once at startup:

```csharp
builder.Services.AddHttpResultMapping(mapper =>
{
    mapper
        .WhenFailure()
        .Problem(p => p
            .WithStatus(System.Net.HttpStatusCode.InternalServerError)
            .WithTitle("Unexpected failure"));
});
```


### 2. Inject and use in endpoints

Endpoints simply return `Result` objects:

```csharp
app.MapGet("/example", ([FromServices] IHttpResultMapper mapper) =>
{
    return mapper.Map(Result.Ok("Hello world"));
});
```

No `if`, no branching, no HTTP logic in the endpoint.


## Rule-based mapping

Rules are evaluated **in order**. The first rule that matches produces the HTTP response and no further rules are evaluated. Because of that, more specific rules must come before more generic ones.

> First match wins!

### Matching by error type

```csharp
mapper
    .WhenError<NotFoundError>()
    .Map(_ => Results.StatusCode(StatusCodes.Status404NotFound));
```


### Mapping to RFC 7807 Problem Details

```csharp
mapper
    .WhenError<ValidationError>()
    .Problem(p => p.WithValidationProblem<ValidationError>(
        e => e.Errors
    ));
```

Produces a standard **Problem Details** response with validation errors.


### Adding headers

Headers are defined **parallel to mapping**, not inside the body logic:

```csharp
mapper
    .WhenError<SecurityError>()
    .WithHeader("WWW-Authenticate", "Bearer")
    .Map(ctx => Results.Json(
        new
        {
            error = "invalid_token",
            error_description = ctx.FirstReason<SecurityError>().Message
        }
    ));
```


### Matching by metadata

Rules can match errors by metadata keys or values:

```csharp
mapper
    .WhenErrorWithMetadata<Error>("error-codes")
    .Problem(p => p
        .WithStatus(System.Net.HttpStatusCode.InternalServerError)
        .WithTitle("An internal server error occurred.")
        .WithDetail(ctx =>
            ctx.FirstReasonWithMetadata<Error>("error-codes").Message)
        .WithExtension("error-codes", ctx =>
            ctx.GetMetadata("error-codes"))
    );
```


### Fallback rule

Always define a fallback failure rule last:

```csharp
mapper
    .WhenFailure()
    .Problem(p => p
        .WithStatus(System.Net.HttpStatusCode.InternalServerError)
        .WithTitle("Unexpected failure"));
```


## Success mapping

Success handling is built in:

- `Result<T>` → **200 OK** with body
- `Result` → **204 No Content**

You can override this behavior by defining your own success rules:

```csharp
mapper
    .WhenSuccess()
    .Map(_ => Results.Ok());
```


## Example Program.cs

A complete example configuration:

```csharp
builder.Services.AddHttpResultMapping(mapper =>
{
    mapper
        .WhenError<ValidationError>()
        .Problem(p => p.WithValidationProblem<ValidationError>(e => e.Errors));

    mapper
        .WhenError<SecurityError>()
        .WithHeader("WWW-Authenticate", "Bearer")
        .Map(ctx => Results.Json(
            new
            {
                error = "invalid_token",
                error_description = ctx.FirstReason<SecurityError>().Message
            }
        ));

    mapper
        .WhenError<NotFoundError>()
        .Map(_ => Results.StatusCode(StatusCodes.Status404NotFound));

    mapper
        .WhenErrorWithMetadata<Error>("error-codes")
        .Problem(p => p
            .WithStatus(System.Net.HttpStatusCode.InternalServerError)
            .WithTitle("An internal server error occurred.")
            .WithDetail(ctx =>
                ctx.FirstReasonWithMetadata<Error>("error-codes").Message)
            .WithExtension("error-codes", ctx =>
                ctx.GetMetadata("error-codes"))
        );

    mapper
        .WhenFailure()
        .Problem(p => p
            .WithStatus(System.Net.HttpStatusCode.InternalServerError)
            .WithTitle("Unexpected failure"));
});
```

## Using with MVC Controllers

Although this package is primarily designed for Minimal APIs, it can also be used in ASP.NET Core applications that rely on MVC controllers.

The HTTP mapping system always produces an ASP.NET `IResult`. To return that result from a controller action, you can adapt it to an `IActionResult` using the provided MVC adapter.

### Example

```csharp
using FluentResults;
using FluentResults.HttpMapping.Execution;
using FluentResults.HttpMapping.MVC;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("example")]
public class ExampleController : ControllerBase
{
    private readonly IHttpResultMapper _mapper;

    public ExampleController(IHttpResultMapper mapper)
    {
        _mapper = mapper;
    }

    [HttpGet("success")]
    public IActionResult Success()
    {
        var result = Result.Ok(new { Message = "Hello from MVC" });
        return _mapper.Map(result).ToActionResult();
    }

    [HttpGet("failure")]
    public IActionResult Failure()
    {
        var result = Result.Fail("Something went wrong");
        return _mapper.Map(result).ToActionResult();
    }
}
```

### Why this works

- The HTTP mapping rules are **framework-agnostic**
- Mapping always results in an `IResult`
- MVC support is provided via a **thin adapter**, not a separate mapping system

This ensures the same rules and behavior are shared between Minimal APIs and MVC controllers without duplication or special configuration.


## What this package is not

- :x: Not an exception handler;
- :x: Not a middleware replacement;
- :x: Not a validation framework;
- :x: Not tied to MVC controllers;

It is a **mapping layer** — nothing more, nothing less.


## Summary

**FluentResults.HttpMapping** gives you:

- Clean endpoints;
- Centralized HTTP behavior;
- Predictable, testable rules;
- Zero HTTP concerns in your domain layer;

If you already use **FluentResults**, this package lets your HTTP layer finally match the same level of clarity.
