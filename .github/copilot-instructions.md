# Copilot Instructions for C# .NET 8 Development

## Purpose

This document provides instructions for GitHub Copilot to assist in writing C# code targeting the .NET 8 framework. The goal is to ensure Copilot generates accurate, efficient, and well-documented code that aligns with best practices.

## Project Context

* **Framework:** .NET 8
* **Language:** C#
* **Style Guide:** Adhere to Microsoft's C# coding conventions.
* **Focus:** Prioritize clean, maintainable, and testable code.

## General Guidelines

1.  **Code Generation:**
    * Generate complete, functional code snippets.
    * Use appropriate data structures and algorithms for the task.
    * Implement error handling and input validation where necessary.
    * Use asynchronous programming (`async`/`await`) when appropriate.
2.  **Documentation:**
    * Include XML documentation comments for all public members.
    * Add inline comments to explain complex logic.
    * Provide clear explanations of the code's functionality and usage.
3.  **Testing:**
    * Suggest or generate unit tests using xUnit or NUnit.
    * Focus on testing edge cases and boundary conditions.
4.  **Dependencies:**
    * Use NuGet packages for external dependencies.
    * Prefer built-in .NET 8 libraries when possible.
5.  **Performance:**
    * Optimize code for performance, considering memory usage and execution time.
    * Use `Span<T>` and `Memory<T>` for efficient memory management when applicable.
    * Consider using the System.Text.Json source generators.
6.  **Specific Areas:**
    * **Web API Development:**
        * Use minimal APIs or ASP.NET Core MVC as appropriate.
        * Implement RESTful principles.
        * Use dependency injection.
        * When using minimal APIs, consider using the new features in .NET 8, like form binding, and native AOT.
    * **Console Applications:**
        * Use `System.CommandLine` for command-line argument parsing.
        * Provide clear and concise output.
    * **Desktop Applications:**
        * When using MAUI, use model view viewmodel (MVVM) patterns.
        * Ensure platform specific code is handled correctly.
6.  **Error Handling:**
    * Use try-catch blocks to handle exceptions.
    * Log errors using `Microsoft.Extensions.Logging`.
    * Provide informative error messages.
7.  **Asynchronous Programming:**
    * Use `async` and `await` keywords appropriately.
    * Avoid blocking calls in asynchronous methods.
    * Handle `Task` exceptions.

## Example Scenario

**User Request:** "Create a .NET 8 Web API endpoint that returns a list of products from a database."

**Copilot Expectations:**

1.  Generate a minimal API endpoint.
2.  Implement a data access layer using Entity Framework Core.
3.  Include error handling and logging.
4.  Provide unit tests for the endpoint.
5.  Document the code with XML comments.
6.  Use asynchronous programming for database operations.
7.  Follow RESTful API design principles.
8.  Optimize the code for performance.
9.  Use dependency injection for services.
10. Include clear and concise comments to explain the logic.
11. Use the latest C# features and .NET 8 libraries.
12. Ensure the code is well-structured and maintainable.
13. Consider scalability and security aspects.
14. Suggest best practices for testing and deployment.
15. give PowerShell commands for CLI operations.

## Interaction Style

* Assume a basic understanding of C# and .NET 8.
* Provide clear and concise explanations.
* Offer alternative solutions and best practices.
* Prioritize .net 8 specific features.