# Rules and Ordering

FluentResults.HttpMapping is **rule-based and order-dependent**.

This document explains how rules are evaluated, why ordering matters, and how to reason about rule precedence when configuring mappings.


## Core Principle

**Rules are evaluated in the order they are defined.**  
**The first rule that matches wins.**

Once a rule matches and produces an HTTP response, **no further rules are evaluated**.

This behavior is intentional and fundamental to the design of the package.


## Why Ordering Matters

Because rules are evaluated sequentially, **more specific rules must come before more generic ones**.

Consider the following example:

```csharp
mapper
    .WhenFailure()
    .Problem(p => p.WithTitle("Generic failure"));

mapper
    .WhenError<SecurityError>()
    .WithHeader("WWW-Authenticate", "Bearer")
    .Map(ctx => Results.Unauthorized());
```

In this case, the `WhenError<SecurityError>()` rule will **never execute**, because:

- `SecurityError` is a failure
- `WhenFailure()` matches first
- Rule evaluation stops immediately

### Correct ordering

```csharp
mapper
    .WhenError<SecurityError>()
    .WithHeader("WWW-Authenticate", "Bearer")
    .Map(ctx => Results.Unauthorized());

mapper
    .WhenFailure()
    .Problem(p => p.WithTitle("Generic failure"));
```


## Recommended Ordering Strategy

A good mental model is to define rules from **most specific** to **most general**.

Typical ordering:

1. **Specific error types**
   - `WhenError<ValidationError>()`
   - `WhenError<SecurityError>()`
2. **Errors with metadata**
   - `WhenErrorWithMetadata<Error>("error-codes")`
3. **Other specific failures**
   - `WhenError<NotFoundError>()`
4. **Catch-all failure**
   - `WhenFailure()`
5. **Success**
   - Handled automatically by the default success rule


## Default Success Rule

FluentResults.HttpMapping always appends a **default success rule** internally.

This rule:

- Matches `Result<T>` → `200 OK` with body
- Matches `Result` → `204 No Content`

Because it is added **after all user-defined rules**, it only applies if no other rule matches.

You do **not** need to define a success rule unless you want to override this behavior.


## Headers and Ordering

Headers are associated with **rules**, not with the response body.

This means:

- Headers defined on a rule are applied only if that rule matches
- Headers from non-matching rules are ignored
- Headers are not merged across multiple rules

Example:

```csharp
mapper
    .WhenError<SecurityError>()
    .WithHeader("WWW-Authenticate", "Bearer")
    .Map(ctx => Results.Unauthorized());
```

If this rule matches, the header is applied. If another rule matches first, the header is never added.


## No Fallback Rule Match = Error

If **no rule matches**, including the default success rule, an exception is thrown.

This is a deliberate design choice to ensure:

- Misconfigurations fail fast
- Missing mappings are visible immediately
- Behavior is always explicit

In practice, this only happens if:
- You remove or override the default success behavior incorrectly
- You build a custom rule set without a success rule


## Design Rationale

This rule system is designed to be:

- Explicit
- Deterministic
- Easy to reason about
- Free of hidden behavior

It intentionally avoids:
- Attribute-based magic
- Exception-driven flow control
- Implicit fallbacks
- Global side effects

Once you internalize the ordering model, rule behavior becomes predictable and transparent.


## Summary

- Rules are evaluated **in order**
- **First match wins**
- More specific rules go first
- `WhenFailure()` is a catch-all
- Success is handled automatically at the end

Understanding rule ordering is the key to using FluentResults.HttpMapping effectively.
