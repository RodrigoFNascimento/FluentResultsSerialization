# FluentResultsSerialization

Serializes [FluentResults](https://github.com/altmann/FluentResults) into [IResult](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.iresult?view=aspnetcore-8.0).

## About

Manually converting a `Result` to an `IResult` can result in code that's harder to read and lacks consistency. For example:

```csharp
app.MapGet("/user", (IUserService userService) =>
{
    var result = myService.GetUser();

    // You always need to check the result.
    if (result.IsFailed)
        return Results.Ok(Array.Empty<User>());

    // HTTP status codes are explicitly defined in every endpoint,
    // leading to potential inconsistency.
    return Results.Ok(result.Value);
});
```

This package simplifies and standardizes HTTP response generation from `Result` (from [FluentResults](https://github.com/altmann/FluentResults)) using a simple interface and fluent configuration.

```csharp
app.MapPost("/user", (IResultSerializer serializer, IUserService userService) =>
{
    var result = myService.GetUser();

    // The serializer handles checking and responding accordingly.
    return serializer.Serialize(result);
});
```

## Usage

Install the package:

```shell
Install-Package FluentResultsSerialization
```

or

```shell
dotnet add package FluentResultsSerialization
```

Add `IResultSerializer` to your application and use it to serialize results:

```csharp
using FluentResultsSerialization;

var builder = WebApplication.CreateBuilder(args);

// Injects an instance of IResultSerializer.
builder.Services.AddResultSerializer();

var app = builder.Build();

// Uses IResultSerializer to serialize a Result.
app.MapGet("/ping", (IResultSerializer serializer) => serializer.Serialize(Result.Ok("pong")));
```

To define how `Result` instances are serialized, configure the behavior in DI using `AddResultSerializationStrategy`. This method accepts a builder that lets you specify serialization behavior. For example:

```csharp
// Returns 400 Bad Request when the Result or Result<TValue>
// contains a ValidationError. The error details are included in the response body.
builder.Services.AddResultSerializationStrategy(builder => builder
    .Handle<ValidationError>()
    .WithStatus(System.Net.HttpStatusCode.BadRequest)
    .WithTitle("Invalid request data.")
    .WithContentType("application/problem+json")
    .ShowReasons());

// Returns 403 Forbidden when the Result or Result<TValue>
// contains AuthorizationFailError.
builder.Services.AddResultSerializationStrategy(builder => builder
    .Handle<AuthorizationFailError>()
    .WithStatus(System.Net.HttpStatusCode.Forbidden)
    .WithTitle("User authorization failed.")
    .WithContentType("application/problem+json"));

// Returns 201 Created when the Result or Result<TValue>
// contains CreatedSuccess.
builder.Services.AddResultSerializationStrategy(builder => builder
    .Handle<CreatedSuccess>()
    .WithStatus(System.Net.HttpStatusCode.Created));

// Returns 202 Accepted when a Result<SendDataRequest> is successful.
builder.Services.AddResultSerializationStrategy(builder => builder
    .Handle<SendDataRequest>(result => result.IsSuccess)
    .WithStatus(System.Net.HttpStatusCode.Accepted));

// Returns 204 No Content for successful Results.
builder.Services.AddResultSerializationStrategy(builder => builder
    .Handle(result => result.IsSuccess)
    .WithStatus(System.Net.HttpStatusCode.NoContent));

// Returns 200 OK for successful Results
// or 500 Internal Server Error if it contains errors.
builder.Services.AddResultSerializationStrategy();
```

> :warning: Strategies are executed in the order they are added.

> :warning: If no HTTP status code is specified for a configured case, `100 Continue` will be returned by default.

### Error Display

To help with debugging, responses may include details of what went wrong. To avoid leaking sensitive information, you can configure what gets displayed per case.

> :warning: All failed `Result` responses follow the [Problem Details](https://datatracker.ietf.org/doc/html/rfc7807) specification. It’s strongly recommended you understand this standard for proper usage.

By default, the `detail` field will contain the message of the first `Error` in the `Result`:

```csharp
builder.Services.AddResultSerializationStrategy(builder => builder
    // ...
    .WithStatus(System.Net.HttpStatusCode.InternalServerError));

var app = builder.Build();

app.MapGet("/ping", (IResultSerializer serializer) =>
    serializer.Serialize(
        Result.Fail("Database connection failed.")));
```

```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
  "title": "An unexpected internal error occurred.",
  "status": 500,
  "detail": "Database connection failed."
}
```

To display a specific error message, configure the strategy to handle that specific error type:

```csharp
builder.Services.AddResultSerializationStrategy(builder => builder
    .Handle<MyCustomError>()
    .WithStatus(System.Net.HttpStatusCode.InternalServerError));

var app = builder.Build();

app.MapGet("/ping", (IResultSerializer serializer) =>
    serializer.Serialize(
        Result.Fail("Database connection failed.")
            .WithError(new MyCustomError("Login failed for user 'DOMAIN\\username'."))));
```

```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
  "title": "An unexpected internal error occurred.",
  "status": 500,
  "detail": "Login failed for user 'DOMAIN\\username'."
}
```

To hide the real error message, provide a replacement message using `WithDetail`:

```csharp
builder.Services.AddResultSerializationStrategy(builder => builder
    // ...
    .WithDetail("An error occurred. Please try again later."));
```

The `detail` field will then always return this value for the configured case:

```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
  "title": "An unexpected internal error occurred.",
  "status": 500,
  "detail": "An error occurred. Please try again later."
}
```

To include all `Reasons` in the response, use `ShowReasons`:

```csharp
builder.Services.AddResultSerializationStrategy(builder => builder
    // ...
    .ShowReasons());

var app = builder.Build();

app.MapGet("/ping", (IResultSerializer serializer) =>
    serializer.Serialize(
        Result.Fail("Database connection failed.")
            .WithError("Login failed for user 'DOMAIN\\username'.")));
```

The details will be listed under the `reasons` field:

```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
  "title": "An unexpected internal error occurred.",
  "status": 500,
  "detail": "Database connection failed.",
  "reasons": [
    {
      "message": "Database connection failed.",
      "metadata": {}
    },
    {
      "message": "Login failed for user 'DOMAIN\\username'.",
      "metadata": {}
    }
  ]
}
```

To return validation errors in the response, add an `Error` to a failed `Result` with `Metadata` containing a `ValidationErrors` key:

```csharp
builder.Services.AddResultSerializationStrategy(builder => builder
    .Handle<Error>()
    .WithStatus(HttpStatusCode.BadRequest));

var app = builder.Build();

app.MapGet("/ping", (IResultSerializer serializer) =>
    serializer.Serialize(
        Result.Fail(
            new Error("Invalid request")
                .WithMetadata("ValidationErrors", new Dictionary<string, string[]>
                {
                    { "name", [ "The 'name' field cannot be empty." ] }
                }))));
```

The validation details will be shown under the `errors` field, matching ASP.NET Core's `ValidationProblemDetails` format:

```json
{
  "type": "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
  "title": "An unexpected internal error occurred.",
  "status": 400,
  "detail": "Database connection failed.",
  "errors": {
    "name": [
      "The 'name' field cannot be empty."
    ]
  }
}
```

### Response Body

According to [RFC7230, Section 3.3](https://www.rfc-editor.org/rfc/rfc7230#section-3.3), HTTP responses with status codes 1XX, 204 (No Content), or 304 (Not Modified) must not include a body. Therefore, responses generated by this package omit the body in these cases.

### Default Behaviors

Some commonly expected behaviors are included when calling `AddResultSerializationStrategy()` with no parameters. Error messages respect the app's current culture.

### Localization

Messages generated by the package follow the application's current culture. If a translation is not available, the fallback language is English.

If you're running in Docker, you might need to explicitly install culture support. For example, to install `en-US` and `pt-BR` locales in a Linux container, use this in your Dockerfile:

```dockerfile
RUN apt-get update && apt-get install -y \
    locales \
    && rm -rf /var/lib/apt/lists/* \
    && locale-gen en_US.UTF-8 \
    && locale-gen pt_BR.UTF-8 \
    && update-locale LANG=en_US.UTF-8
```

## NuGet Package Generation

This project is configured to generate a new `.nupkg` file in the `bin` folder after every successful build. You can find this configuration in the project properties.

## Compatibility

The package targets .NET 6 because the shared framework `Microsoft.AspNetCore.App` is not compatible with .NET Standard ([reference](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/target-aspnetcore?view=aspnetcore-8.0&tabs=visual-studio)). Therefore, it supports .NET 6 and newer.
