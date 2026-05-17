# AsyncSpark.Web

Modern .NET 10 web application demonstrating async programming patterns with interactive API documentation.

Part of [Make Bold Spark](https://makeboldspark.com) — built by [Mark Hazleton](https://markhazleton.com), [Make Bold Solutions](https://makeboldsolutions.com).

**Live Site**: [https://async.makeboldspark.com](https://async.makeboldspark.com)

## Features

- **Beautiful API Documentation** - Powered by [Scalar](https://github.com/scalar/scalar)
- **Async/Await Patterns** - Real-world examples of asynchronous programming
- **Polly Resilience** - Retry policies, circuit breakers, and fallback strategies
- **Weather API Integration** - Live OpenWeatherMap data
- **Clean Architecture** - Decorator pattern and dependency injection

## API Documentation with Scalar

Explore the interactive API documentation at:

- **Scalar UI**: `/scalar/v1`
- **OpenAPI Spec**: `/openapi/v1.json`
- **Live Demo**: [https://async.makeboldspark.com/scalar/v1](https://async.makeboldspark.com/scalar/v1)

### Why Scalar?

- Modern, beautiful interface
- Multi-language code generation (C#, JS, Python, cURL, etc.)
- Fast and responsive
- Dark mode support
- Mobile-friendly

## Technologies

- **.NET 10** - Latest framework
- **ASP.NET Core** - Web framework
- **Scalar** - API documentation UI
- **Microsoft.AspNetCore.OpenApi** - OpenAPI generation
- **Polly** - Resilience patterns
- **Bootstrap 5** - UI framework
- **[WebSpark.Bootswatch](https://www.nuget.org/packages/WebSpark.Bootswatch)** - 25+ Bootstrap themes by Mark Hazleton
- **[WebSpark.HttpClientUtility](https://www.nuget.org/packages/WebSpark.HttpClientUtility)** - Structured HTTP client by Mark Hazleton

## Learning Resources

### Async Programming — articles by [Mark Hazleton](https://markhazleton.com)

- [CancellationToken for Async Programming](https://markhazleton.com/blog/cancellation-token)
- [Decorator Pattern — Adding Telemetry to HttpClient](https://markhazleton.com/blog/decorator-pattern-http-client)
- [Concurrent Processing in C# — SemaphoreSlim & Task Parallelism](https://markhazleton.com/blog/concurrent-processing)
- [Fire and Forget for Enhanced Performance](https://markhazleton.com/blog/fire-and-forget-for-enhanced-performance)
- [RESTRunner — Building a DIY API Load Testing Tool](https://markhazleton.com/blog/rest-runner-building-your-own-api-load-tester)
- [TaskListProcessor — Enterprise Async Orchestration for .NET](https://markhazleton.com/blog/task-list-processor)
- [Cancel Asynchronous Operations in C#](https://johnthiriet.com/cancel-asynchronous-operation-in-csharp/)
- [Await, UI, and Deadlocks](https://devblogs.microsoft.com/pfxteam/await-and-ui-and-deadlocks-oh-my/)
- [Stop Calling .Result](https://montemagno.com/c-sharp-developers-stop-calling-dot-result/)

### Polly Resilience

- [The Polly Project](http://www.thepollyproject.org/) - .NET resilience library
- [Retry Count Diagnostics](https://www.stevejgordon.co.uk/polly-using-context-to-obtain-retry-count-diagnostics)
- [Retry & Circuit Breaker Patterns](https://medium.com/@therealjordanlee/retry-circuit-breaker-patterns-in-c-with-polly-9aa24c5fe23a)

---

**AsyncSpark** — Part of [Make Bold Spark](https://makeboldspark.com) | Built by [Mark Hazleton](https://markhazleton.com), [Make Bold Solutions](https://makeboldsolutions.com) | API Documentation powered by Scalar
