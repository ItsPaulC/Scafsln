namespace Scafsln.Cli;

/// <summary>
/// Contains static content for various configuration files
/// </summary>
public static class FileContents
{
    /// <summary>
    /// Default .gitignore content for .NET projects
    /// </summary>
    public const string GitIgnoreContent = """
                                           ## Visual Studio
                                           .vs/
                                           [Bb]in/
                                           [Oo]bj/
                                           [Dd]ebug/
                                           [Rr]elease/
                                           *.user
                                           *.userosscache
                                           *.suo
                                           *.userprefs
                                           *.dbmdl
                                           *.jfm
                                           *.pfx
                                           *.publishsettings

                                           ## Visual Studio Code
                                           .vscode/*
                                           !.vscode/settings.json
                                           !.vscode/tasks.json
                                           !.vscode/launch.json
                                           !.vscode/extensions.json
                                           *.code-workspace

                                           ## Rider
                                           .idea/
                                           *.sln.iml
                                           .idea/**/workspace.xml
                                           .idea/**/tasks.xml
                                           .idea/**/usage.statistics.xml
                                           .idea/**/dictionaries
                                           .idea/**/shelf

                                           ## .NET Core
                                           project.lock.json
                                           project.fragment.lock.json
                                           artifacts/

                                           ## NuGet
                                           *.nupkg
                                           **/packages/*
                                           !**/packages/build/
                                           *.nuget.props
                                           *.nuget.targets

                                           ## Build results
                                           [Dd]ist/
                                           [Ll]og/
                                           [Ll]ogs/
                                           msbuild.log
                                           msbuild.err
                                           msbuild.wrn

                                           ## Other
                                           *.swp
                                           *.*~
                                           *.bak
                                           """;

    /// <summary>
    /// Default .editorconfig content for .NET projects
    /// </summary>
    public const string EditorConfigContent = """
                                              # editorconfig.org

                                              # top-most EditorConfig file
                                              root = true

                                              # Default settings:
                                              # A newline ending every file
                                              # Use 4 spaces as indentation
                                              [*]
                                              insert_final_newline = true
                                              indent_style = space
                                              indent_size = 4
                                              trim_trailing_whitespace = true

                                              # Generated code
                                              [*{_AssemblyInfo.cs,.notsupported.cs,AsmOffsets.cs}]
                                              generated_code = true

                                              # C# files
                                              [*.cs]
                                              # New line preferences
                                              csharp_new_line_before_open_brace = all
                                              csharp_new_line_before_else = true
                                              csharp_new_line_before_catch = true
                                              csharp_new_line_before_finally = true
                                              csharp_new_line_before_members_in_object_initializers = true
                                              csharp_new_line_before_members_in_anonymous_types = true
                                              csharp_new_line_between_query_expression_clauses = true

                                              # Indentation preferences
                                              csharp_indent_block_contents = true
                                              csharp_indent_braces = false
                                              csharp_indent_case_contents = true
                                              csharp_indent_case_contents_when_block = false
                                              csharp_indent_switch_labels = true
                                              csharp_indent_labels = one_less_than_current

                                              # Modifier preferences
                                              csharp_preferred_modifier_order = public,private,protected,internal,file,static,extern,new,virtual,abstract,sealed,override,readonly,unsafe,required,volatile,async:suggestion

                                              # avoid this. unless absolutely necessary
                                              dotnet_style_qualification_for_field = false:suggestion
                                              dotnet_style_qualification_for_property = false:suggestion
                                              dotnet_style_qualification_for_method = false:suggestion
                                              dotnet_style_qualification_for_event = false:suggestion

                                              # Types: use keywords instead of BCL types, and permit var only when the type is clear
                                              csharp_style_var_for_built_in_types = false:suggestion
                                              csharp_style_var_when_type_is_apparent = false:none
                                              csharp_style_var_elsewhere = false:suggestion
                                              dotnet_style_predefined_type_for_locals_parameters_members = true:suggestion
                                              dotnet_style_predefined_type_for_member_access = true:suggestion

                                              # name all constant fields using PascalCase
                                              dotnet_naming_rule.constant_fields_should_be_pascal_case.severity = suggestion
                                              dotnet_naming_rule.constant_fields_should_be_pascal_case.symbols  = constant_fields
                                              dotnet_naming_rule.constant_fields_should_be_pascal_case.style    = pascal_case_style
                                              dotnet_naming_symbols.constant_fields.applicable_kinds   = field
                                              dotnet_naming_symbols.constant_fields.required_modifiers = const
                                              dotnet_naming_style.pascal_case_style.capitalization = pascal_case

                                              # static fields should have s_ prefix
                                              dotnet_naming_rule.static_fields_should_have_prefix.severity = suggestion
                                              dotnet_naming_rule.static_fields_should_have_prefix.symbols  = static_fields
                                              dotnet_naming_rule.static_fields_should_have_prefix.style    = static_prefix_style
                                              dotnet_naming_symbols.static_fields.applicable_kinds   = field
                                              dotnet_naming_symbols.static_fields.required_modifiers = static
                                              dotnet_naming_symbols.static_fields.applicable_accessibilities = private, internal, private_protected
                                              dotnet_naming_style.static_prefix_style.required_prefix = s_
                                              dotnet_naming_style.static_prefix_style.capitalization = camel_case

                                              # internal and private fields should be _camelCase
                                              dotnet_naming_rule.camel_case_for_private_internal_fields.severity = suggestion
                                              dotnet_naming_rule.camel_case_for_private_internal_fields.symbols  = private_internal_fields
                                              dotnet_naming_rule.camel_case_for_private_internal_fields.style    = camel_case_underscore_style
                                              dotnet_naming_symbols.private_internal_fields.applicable_kinds = field
                                              dotnet_naming_symbols.private_internal_fields.applicable_accessibilities = private, internal
                                              dotnet_naming_style.camel_case_underscore_style.required_prefix = _
                                              dotnet_naming_style.camel_case_underscore_style.capitalization = camel_case

                                              # Code style defaults
                                              csharp_using_directive_placement = outside_namespace:suggestion
                                              dotnet_sort_system_directives_first = true
                                              csharp_prefer_braces = true:silent
                                              csharp_preserve_single_line_blocks = true:none
                                              csharp_preserve_single_line_statements = false:none
                                              csharp_prefer_static_local_function = true:suggestion
                                              csharp_prefer_simple_using_statement = false:none
                                              csharp_style_prefer_switch_expression = true:suggestion
                                              dotnet_style_readonly_field = true:suggestion

                                              # Expression-level preferences
                                              dotnet_style_object_initializer = true:suggestion
                                              dotnet_style_collection_initializer = true:suggestion
                                              dotnet_style_prefer_collection_expression = when_types_exactly_match
                                              dotnet_style_explicit_tuple_names = true:suggestion
                                              dotnet_style_coalesce_expression = true:suggestion
                                              dotnet_style_null_propagation = true:suggestion
                                              dotnet_style_prefer_is_null_check_over_reference_equality_method = true:suggestion
                                              dotnet_style_prefer_inferred_tuple_names = true:suggestion
                                              dotnet_style_prefer_inferred_anonymous_type_member_names = true:suggestion
                                              dotnet_style_prefer_auto_properties = true:suggestion
                                              dotnet_style_prefer_conditional_expression_over_assignment = true:silent
                                              dotnet_style_prefer_conditional_expression_over_return = true:silent
                                              csharp_prefer_simple_default_expression = true:suggestion

                                              # Expression-bodied members
                                              csharp_style_expression_bodied_methods = true:silent
                                              csharp_style_expression_bodied_constructors = true:silent
                                              csharp_style_expression_bodied_operators = true:silent
                                              csharp_style_expression_bodied_properties = true:silent
                                              csharp_style_expression_bodied_indexers = true:silent
                                              csharp_style_expression_bodied_accessors = true:silent
                                              csharp_style_expression_bodied_lambdas = true:silent
                                              csharp_style_expression_bodied_local_functions = true:silent

                                              # Pattern matching
                                              csharp_style_pattern_matching_over_is_with_cast_check = true:suggestion
                                              csharp_style_pattern_matching_over_as_with_null_check = true:suggestion
                                              csharp_style_inlined_variable_declaration = true:suggestion

                                              # Null checking preferences
                                              csharp_style_throw_expression = true:suggestion
                                              csharp_style_conditional_delegate_call = true:suggestion

                                              # Other features
                                              csharp_style_prefer_index_operator = false:none
                                              csharp_style_prefer_range_operator = false:none
                                              csharp_style_pattern_local_over_anonymous_function = false:none

                                              # Space preferences
                                              csharp_space_after_cast = false
                                              csharp_space_after_colon_in_inheritance_clause = true
                                              csharp_space_after_comma = true
                                              csharp_space_after_dot = false
                                              csharp_space_after_keywords_in_control_flow_statements = true
                                              csharp_space_after_semicolon_in_for_statement = true
                                              csharp_space_around_binary_operators = before_and_after
                                              csharp_space_around_declaration_statements = do_not_ignore
                                              csharp_space_before_colon_in_inheritance_clause = true
                                              csharp_space_before_comma = false
                                              csharp_space_before_dot = false
                                              csharp_space_before_open_square_brackets = false
                                              csharp_space_before_semicolon_in_for_statement = false
                                              csharp_space_between_empty_square_brackets = false
                                              csharp_space_between_method_call_empty_parameter_list_parentheses = false
                                              csharp_space_between_method_call_name_and_opening_parenthesis = false
                                              csharp_space_between_method_call_parameter_list_parentheses = false
                                              csharp_space_between_method_declaration_empty_parameter_list_parentheses = false
                                              csharp_space_between_method_declaration_name_and_open_parenthesis = false
                                              csharp_space_between_method_declaration_parameter_list_parentheses = false
                                              csharp_space_between_parentheses = false
                                              csharp_space_between_square_brackets = false

                                              # License header
                                              file_header_template = Licensed to the .NET Foundation under one or more agreements.\nThe .NET Foundation licenses this file to you under the MIT license.

                                              # C++ Files
                                              [*.{cpp,h,in}]
                                              curly_bracket_next_line = true
                                              indent_brace_style = Allman

                                              # Xml project files
                                              [*.{csproj,vbproj,vcxproj,vcxproj.filters,proj,nativeproj,locproj}]
                                              indent_size = 2

                                              [*.{csproj,vbproj,proj,nativeproj,locproj}]
                                              charset = utf-8

                                              # Xml build files
                                              [*.builds]
                                              indent_size = 2

                                              # Xml files
                                              [*.{xml,stylecop,resx,ruleset}]
                                              indent_size = 2

                                              # Xml config files
                                              [*.{props,targets,config,nuspec}]
                                              indent_size = 2

                                              # YAML config files
                                              [*.{yml,yaml}]
                                              indent_size = 2

                                              # Shell scripts
                                              [*.sh]
                                              end_of_line = lf
                                              [*.{cmd,bat}]
                                              end_of_line = crlf
                                              """;

    public const string CopilotInstructions = """
                                              # GitHub Copilot Instructions for C# 12 & .NET 8 Development

                                              This guide directs GitHub Copilot to generate clean, modern, performant, and secure C# code that aligns with .NET 8 standards and industry best practices.

                                              ---

                                              ## 🎯 Core Principles

                                              * **Clarity and Simplicity**: Write code that is easy to read and understand. Prioritize simple, straightforward solutions over overly complex ones.
                                              * **Single Responsibility Principle (SRP)**: Every class and method should have one, and only one, reason to change.
                                              * **Composition over Inheritance**: Favor has-a relationships (composition) over is-a relationships (inheritance) to increase flexibility and reduce tight coupling.
                                              * **Immutability**: Use immutable data structures (`record`, `readonly struct`, `init`-only properties) whenever possible to prevent unintended side effects and improve thread safety.

                                              ---

                                              ## ✨ Modern C# 12 / .NET 8 Idioms

                                              Always prefer modern C# syntax to write more concise and readable code.

                                              * **Primary Constructors**: Use primary constructors for classes and structs, especially for dependency injection and simple data models.
                                                  ```csharp
                                                  // Use this
                                                  public class ProductService(ILogger<ProductService> logger, IProductRepository repository)
                                                  {
                                                      // ...
                                                  }
                                                  ```
                                              * **Collection Expressions**: Initialize collections using the shortest possible syntax.
                                                  ```csharp
                                                  // Use this
                                                  List<string> names = ["Alice", "Bob", "Charlie"];
                                                  int[] numbers = [1, 2, 3, 4, 5];
                                                  ```
                                              * **Target-Typed `new` Expressions**: Omit the type name when the compiler can infer it.
                                                  ```csharp
                                                  // Use this
                                                  System.Text.StringBuilder sb = new();
                                                  ```
                                              * **File-Scoped Namespaces**: Reduce indentation by declaring the namespace for the entire file.
                                                  ```csharp
                                                  // Use this
                                                  namespace MyCompany.MyAwesomeApp.Business;

                                                  public class MyClass { /* ... */ }
                                                  ```
                                              * **`using` Statements**: Place `using` directives outside the namespace declaration.
                                              * **Top-Level Statements**: Use for simple programs like console utilities and scripts. For larger applications (APIs, web apps), use the standard `Program.cs` with a `WebApplication` builder.

                                              ---

                                              ## 🔧 Code Quality & Maintainability

                                              * **Meaningful Naming**: Use descriptive names for variables, methods, and classes that clearly communicate their purpose.
                                              * **Constants over Magic Values**: Avoid "magic strings" and numbers. Define them as `const` or `static readonly` fields.
                                              * **XML Documentation**: Document all public methods and properties with XML comments (`<summary>`, `<param>`, `<returns>`, `<exception>`).
                                              * **Null Handling**:
                                                  * Assume **`Nullable` is enabled** for all projects (`<Nullable>enable</Nullable>`).
                                                  * Avoid the null-forgiving operator (`!`) unless you are certain a value cannot be `null` and the analyzer cannot prove it.
                                              * **Error Handling**: Use `try-catch` blocks for exceptions you can handle. Let unhandled exceptions propagate up to a global handler.

                                              ---

                                              ## 🚀 Performance

                                              * **Asynchronous Operations**: Use `async` and `await` for all I/O-bound operations (e.g., database calls, HTTP requests, file access) to keep the application responsive.
                                              * **Use `Span<T>` and `Memory<T>`**: When processing large amounts of data, arrays, or strings, use `Span<T>` to avoid unnecessary memory allocations and copying.
                                              * **LINQ Awareness**:
                                                  * Be mindful of multiple enumerations of `IEnumerable`. If a LINQ query is used more than once, cache the results with `.ToList()` or `.ToArray()`.
                                                  * For performance-critical paths, a standard `for` or `foreach` loop can be more efficient than LINQ.
                                              * **String Manipulation**: For building strings in loops or complex logic, always use `System.Text.StringBuilder`.

                                              ---

                                              ## 📈 Logging & Instrumentation

                                              * **Structured Logging**: Use `Microsoft.Extensions.Logging.ILogger` for all logging. Write log messages with templates, not string interpolation.
                                                  ```csharp
                                                  // Good: Structured logging
                                                  _logger.LogInformation("Processing order {OrderId} for customer {CustomerId}", order.Id, customer.Id);

                                                  // Bad: String interpolation
                                                  _logger.LogInformation($"Processing order {order.Id} for customer {customer.Id}");
                                                  ```
                                              * **OpenTelemetry**: When generating code that needs tracing or metrics, assume **OpenTelemetry** is the standard. Create `ActivitySource` and `Meter` instances to produce telemetry data.

                                              ---

                                              ## 🔒 Security

                                              * **Parameterize Database Queries**: Always use parameterized queries (e.g., with Dapper or EF Core) to prevent SQL injection. Never use string concatenation to build queries with user input.
                                              * **Validate and Sanitize Input**: Treat all external input (from users, APIs, files) as untrusted. Validate it and sanitize it to prevent security vulnerabilities.
                                              * **Keep Dependencies Updated**: When adding NuGet packages, use the latest stable version. Add comments reminding to regularly check for vulnerabilities using `dotnet list package --vulnerable`.

                                              ---

                                              ## 🤖 Custom Commands

                                              * When I ask: **"who dat?"** you reply: **"you know who."**

                                              ---

                                              ## 🧪 Unit Testing

                                              Write clear, concise, and meaningful unit tests. Focus on testing public behavior, not private implementation details.

                                              * **Frameworks**:
                                                  * **Test Runner**: **XUnit v2**
                                                  * **Mocking**: **NSubstitute**
                                                  * **Logger Testing**: **MELT** (`Microsoft.Extensions.Logging.Testing`)
                                                  * **Assertions**: **XUnit's built-in `Assert` class** (`Assert.Equal`, `Assert.ThrowsAsync`, etc.).
                                              * **Test Structure**: All tests **must** follow the **Arrange-Act-Assert (AAA)** pattern using comments to separate the sections.
                                                  ```csharp
                                                  // Arrange

                                                  // Act

                                                  // Assert
                                                  ```
                                              * **Test Naming Convention**: Use the `MethodName_Should_ExpectedBehavior_When_Condition` format.
                                                  * **Example**: `CreateUser_Should_ReturnNewUser_When_UserDataIsValid`
                                              * **Test Coverage**: Write tests for the "happy path," edge cases (e.g., null arguments, empty lists), and expected exceptions.

                                              * **Example Test Implementation**:

                                                  ```csharp
                                                  using MELT;
                                                  using Microsoft.Extensions.Logging;
                                                  using NSubstitute;
                                                  using Xunit;

                                                  // Class being tested
                                                  public class GreeterService(ILogger<GreeterService> logger, INameRepository repository)
                                                  {
                                                      public string GenerateGreeting(int id)
                                                      {
                                                          logger.LogInformation("Generating greeting for user {UserId}", id);
                                                          var name = repository.GetName(id);
                                                          return $"Hello, {name}!";
                                                      }
                                                  }

                                                  public interface INameRepository
                                                  {
                                                      string GetName(int id);
                                                  }

                                                  // Test Class
                                                  public class GreeterServiceTests
                                                  {
                                                      private readonly ITestSink _testSink;
                                                      private readonly ILogger<GreeterService> _logger;
                                                      private readonly INameRepository _mockRepo;
                                                      private readonly GreeterService _sut;

                                                      public GreeterServiceTests()
                                                      {
                                                          // MELT setup for ILogger testing
                                                          _testSink = TestSink.Create();
                                                          _logger = _testSink.CreateLogger<GreeterService>();

                                                          // NSubstitute setup for mocking dependencies
                                                          _mockRepo = Substitute.For<INameRepository>();

                                                          // System Under Test (SUT)
                                                          _sut = new GreeterService(_logger, _mockRepo);
                                                      }

                                                      [Fact]
                                                      public void GenerateGreeting_Should_ReturnCorrectGreeting_When_UserExists()
                                                      {
                                                          // Arrange
                                                          var userId = 1;
                                                          var expectedName = "Alice";
                                                          _mockRepo.GetName(userId).Returns(expectedName);

                                                          // Act
                                                          var result = _sut.GenerateGreeting(userId);

                                                          // Assert
                                                          // Assert the return value is correct (XUnit)
                                                          Assert.Equal("Hello, Alice!", result);
                                                          // Assert that a dependency was called (NSubstitute)
                                                          _mockRepo.Received(1).GetName(userId);
                                                          // Assert that a log was written with the correct state (MELT)
                                                          var log = Assert.Single(_testSink.LogEntries);
                                                          Assert.Equal("Generating greeting for user {UserId}", log.Message);
                                                          Assert.Collection(log.Properties, kvp => Assert.Equal(userId, kvp.Value));
                                                      }
                                                  }
                                                  ```

                                              ---

                                              ## 🤖 Custom Commands
                                              """;
}