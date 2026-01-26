using FluentResults;
using FluentResults.HttpMapping;
using FluentResults.HttpMapping.Execution;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using TestApi.Reasons;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

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
                Error = "invalid_token",
                Error_description = ctx.FirstReason<SecurityError>().Message
            })
        );

    mapper
        .WhenError<NotFoundError>()
        .Map(p => Results.StatusCode(StatusCodes.Status404NotFound));

    mapper
        .WhenErrorWithMetadata<Error>("error-codes")
        .Problem(p => p
            .WithStatus(System.Net.HttpStatusCode.InternalServerError)
            .WithTitle("An internal server error occured.")
            .WithDetail(ctx => ctx.FirstReasonWithMetadata<Error>("error-codes").Message)
            .WithExtension("error-codes", ctx => ctx.GetMetadata("error-codes"))
        );

    mapper
        .WhenFailure()
        .Problem(p => p
            .WithStatus(System.Net.HttpStatusCode.InternalServerError)
            .WithTitle("Unexpected failure")
        );
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapGet("success/value", ([FromServices] IHttpResultMapper mapper) =>
{
    return mapper.Map(
        Result.Ok(new { Message = "Hello world" }));
});

app.MapGet("success", ([FromServices] IHttpResultMapper mapper) =>
{
    return mapper.Map(
        Result.Ok());
});

app.MapGet("failure/validation-problem", ([FromServices] IHttpResultMapper mapper) =>
{
    var errors = new Dictionary<string, string[]>()
    {
        { "name", ["The field 'name' is required."] },
        { "email", ["The field 'email' is required."] }
    };

    return mapper.Map(
        Result.Fail(new ValidationError(errors)));
});

app.MapGet("failure/security", ([FromServices] IHttpResultMapper mapper) =>
{
    return mapper.Map(
        Result.Fail(new SecurityError("Invalid username or password")));
});

app.MapGet("failure/error-codes", ([FromServices] IHttpResultMapper mapper) =>
{
    string[] errorCodes = ["1234", "4321"];
    return mapper.Map(
        Result.Fail(
            new Error("Checkout the error codes for more details.")
                .WithMetadata("error-codes", errorCodes)));
});

app.MapGet("failure/error-fallback", ([FromServices] IHttpResultMapper mapper) =>
{
    return mapper.Map(
        Result.Fail(
            new Error("This error was handled by the fallback rule.")));
});

app.Run();
