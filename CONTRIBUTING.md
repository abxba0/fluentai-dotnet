# Contributing to FluentAI.NET

Thank you for your interest in contributing to FluentAI.NET! This document provides guidelines and information for contributors.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Contribution Guidelines](#contribution-guidelines)
- [Pull Request Process](#pull-request-process)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Documentation](#documentation)
- [Release Process](#release-process)

## Code of Conduct

This project and everyone participating in it is governed by our Code of Conduct. By participating, you are expected to uphold this code.

### Our Standards

- **Be respectful** and inclusive in all interactions
- **Be constructive** when providing feedback
- **Focus on the technical merits** of contributions
- **Help maintain a welcoming environment** for all contributors

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Git
- An IDE (Visual Studio, VS Code, or JetBrains Rider)
- API keys for testing (optional but recommended)

### Setting Up Your Environment

1. **Fork the repository**
   ```bash
   # Clone your fork
   git clone https://github.com/YOUR_USERNAME/fluentai-dotnet.git
   cd fluentai-dotnet
   
   # Add upstream remote
   git remote add upstream https://github.com/abxba0/fluentai-dotnet.git
   ```

2. **Build the project**
   ```bash
   dotnet restore
   dotnet build
   ```

3. **Run the tests**
   ```bash
   dotnet test
   ```

4. **Set up API keys for integration testing** (optional)
   ```bash
   export OPENAI_API_KEY="your-test-key"
   export ANTHROPIC_API_KEY="your-test-key"
   ```

## Development Setup

### Project Structure

```
FluentAI.NET/
├── Abstractions/          # Core interfaces and models
│   ├── IChatModel.cs      # Main chat interface
│   ├── Models/            # Data models
│   ├── Exceptions/        # Custom exceptions
│   ├── Security/          # Security features
│   └── Performance/       # Performance monitoring
├── Providers/             # AI provider implementations
│   ├── OpenAI/           # OpenAI provider
│   ├── Anthropic/        # Anthropic provider
│   └── Google/           # Google AI provider
├── Configuration/         # Configuration models
├── Extensions/           # Service registration extensions
├── Examples/             # Example applications
│   └── ConsoleApp/       # Comprehensive demo app
├── FluentAI.NET.Tests/   # Test suite
└── docs/                 # Documentation
```

### Building and Testing

```bash
# Build all projects
dotnet build

# Run unit tests
dotnet test --logger trx --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test FluentAI.NET.Tests/

# Run with verbose output
dotnet test --verbosity normal

# Run integration tests (requires API keys)
dotnet test --filter Category=Integration
```

## Contribution Guidelines

### What to Contribute

**We welcome contributions for:**

- 🐛 **Bug fixes** - Fix issues in existing functionality
- ✨ **New features** - Add new AI providers, enhance existing features
- 📖 **Documentation** - Improve guides, examples, API docs
- 🧪 **Tests** - Improve test coverage, add integration tests
- 🔧 **Performance** - Optimize existing code, reduce memory usage
- 🛡️ **Security** - Enhance security features, fix vulnerabilities

**Please discuss before contributing:**

- Major architectural changes
- Breaking changes to public APIs
- New dependencies
- Significant performance modifications

### Issue Guidelines

**Before creating an issue:**

1. Search existing issues to avoid duplicates
2. Use the appropriate issue template
3. Provide clear reproduction steps for bugs
4. Include environment details (OS, .NET version, etc.)

**Issue Labels:**

- `bug` - Something isn't working
- `enhancement` - New feature or request
- `documentation` - Improvements or additions to documentation
- `good first issue` - Good for newcomers
- `help wanted` - Extra attention is needed
- `provider:openai` - OpenAI provider specific
- `provider:anthropic` - Anthropic provider specific

## Pull Request Process

### Before Submitting

1. **Create a feature branch**
   ```bash
   git checkout -b feature/my-new-feature
   # or
   git checkout -b fix/bug-description
   ```

2. **Make your changes**
   - Follow coding standards
   - Write/update tests
   - Update documentation
   - Ensure builds pass

3. **Test thoroughly**
   ```bash
   # Run all tests
   dotnet test
   
   # Check code coverage
   dotnet test --collect:"XPlat Code Coverage"
   
   # Test the example app
   cd Examples/ConsoleApp
   dotnet run
   ```

### Submitting the Pull Request

1. **Update from upstream**
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

2. **Create descriptive commits**
   ```bash
   # Use conventional commit format
   git commit -m "feat: add support for new OpenAI GPT-4o model"
   git commit -m "fix: resolve rate limiting issue in Anthropic provider"
   git commit -m "docs: add Azure Functions integration guide"
   ```

3. **Push and create PR**
   ```bash
   git push origin feature/my-new-feature
   ```

4. **Fill out the PR template**
   - Describe what you changed and why
   - Link related issues
   - Note any breaking changes
   - Include testing details

### PR Review Process

1. **Automated checks** must pass:
   - Build succeeds
   - All tests pass
   - Code coverage meets threshold
   - No security vulnerabilities

2. **Code review** by maintainers:
   - Code quality and style
   - Test coverage
   - Documentation updates
   - Breaking change considerations

3. **Integration testing** in staging environment

4. **Approval and merge** by maintainers

## Coding Standards

### C# Coding Conventions

Follow [Microsoft's C# coding conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions):

```csharp
// Use PascalCase for public members
public class ChatService
{
    // Use camelCase for private fields with underscore prefix
    private readonly IChatModel _chatModel;
    
    // Use PascalCase for properties
    public string ModelId { get; set; }
    
    // Use camelCase for parameters and local variables
    public async Task<string> GetResponseAsync(string userMessage)
    {
        var messages = new List<ChatMessage>();
        // ...
    }
}
```

### Code Quality Guidelines

1. **SOLID Principles**
   - Single Responsibility: Classes should have one reason to change
   - Open/Closed: Open for extension, closed for modification
   - Liskov Substitution: Derived classes must be substitutable
   - Interface Segregation: Many client-specific interfaces
   - Dependency Inversion: Depend on abstractions, not concretions

2. **Error Handling**
   ```csharp
   // Use specific exceptions
   throw new AiSdkConfigurationException("OpenAI API key is required");
   
   // Log appropriately
   _logger.LogError(ex, "Failed to process chat request for user {UserId}", userId);
   
   // Use nullable reference types
   public string? ProcessMessage(string? input)
   {
       if (input is null)
           return null;
           
       // ...
   }
   ```

3. **Async/Await Best Practices**
   ```csharp
   // Use ConfigureAwait(false) in libraries
   var response = await httpClient.GetAsync(url).ConfigureAwait(false);
   
   // Cancel operations appropriately
   public async Task<ChatResponse> GetResponseAsync(
       IEnumerable<ChatMessage> messages,
       CancellationToken cancellationToken = default)
   {
       // Pass cancellation token through
       return await _provider.GetResponseAsync(messages, cancellationToken);
   }
   ```

### Documentation Standards

1. **XML Documentation Comments**
   ```csharp
   /// <summary>
   /// Gets a response from the AI model for the given messages.
   /// </summary>
   /// <param name="messages">The conversation messages.</param>
   /// <param name="options">Optional request configuration.</param>
   /// <param name="cancellationToken">Cancellation token.</param>
   /// <returns>The AI response with content and metadata.</returns>
   /// <exception cref="AiSdkException">Thrown when the AI service returns an error.</exception>
   public async Task<ChatResponse> GetResponseAsync(
       IEnumerable<ChatMessage> messages,
       ChatRequestOptions? options = null,
       CancellationToken cancellationToken = default)
   ```

2. **README and Markdown**
   - Use clear headings and structure
   - Include code examples that actually work
   - Add links to related documentation
   - Use proper markdown syntax

## Testing Guidelines

### Test Categories

1. **Unit Tests** (`FluentAI.NET.Tests/UnitTests/`)
   - Test individual classes and methods
   - Use mocks for dependencies
   - Should be fast and isolated

2. **Integration Tests** (`FluentAI.NET.Tests/Integration/`)
   - Test component interactions
   - May use real external services
   - Mark with `[Category("Integration")]`

3. **Performance Tests** (`FluentAI.NET.Tests/Performance/`)
   - Test performance characteristics
   - Memory usage, throughput, latency
   - Mark with `[Category("Performance")]`

### Test Structure

```csharp
[TestFixture]
public class ChatModelTests
{
    [Test]
    public async Task GetResponseAsync_ValidInput_ReturnsExpectedResponse()
    {
        // Arrange
        var mockProvider = new Mock<IChatProvider>();
        var chatModel = new ChatModel(mockProvider.Object);
        var messages = new[] { new ChatMessage(ChatRole.User, "Hello") };
        
        mockProvider.Setup(x => x.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>()))
            .ReturnsAsync(new ChatResponse { Content = "Hi there!" });
        
        // Act
        var result = await chatModel.GetResponseAsync(messages);
        
        // Assert
        Assert.That(result.Content, Is.EqualTo("Hi there!"));
        mockProvider.Verify(x => x.GetResponseAsync(messages), Times.Once);
    }
    
    [Test]
    public void GetResponseAsync_NullMessages_ThrowsArgumentNullException()
    {
        // Arrange
        var chatModel = new ChatModel(Mock.Of<IChatProvider>());
        
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(() => 
            chatModel.GetResponseAsync(null));
    }
}
```

### Coverage Requirements

- **Minimum 80% code coverage** for new code
- **100% coverage** for public APIs
- **Critical paths** must be thoroughly tested
- **Error scenarios** must be covered

## Documentation

### Types of Documentation

1. **API Documentation** - XML comments in code
2. **Integration Guides** - How to use in different project types
3. **Examples** - Working code samples
4. **Troubleshooting** - Common issues and solutions

### Documentation Standards

- **Keep it up to date** - Update docs with code changes
- **Use examples** - Show don't just tell
- **Be clear and concise** - Avoid unnecessary complexity
- **Link appropriately** - Reference related content

### Building Documentation

```bash
# Generate API documentation
dotnet tool install -g docfx
docfx init -q
docfx build

# Serve documentation locally
docfx serve _site
```

## Release Process

### Versioning

We follow [Semantic Versioning](https://semver.org/):

- **MAJOR** version for incompatible API changes
- **MINOR** version for backwards-compatible functionality
- **PATCH** version for backwards-compatible bug fixes

### Release Checklist

1. **Update version numbers**
   - `FluentAI.NET.csproj`
   - Package release notes

2. **Update documentation**
   - CHANGELOG.md
   - README.md if needed
   - API documentation

3. **Create release branch**
   ```bash
   git checkout -b release/v1.2.0
   ```

4. **Run full test suite**
   ```bash
   dotnet test --configuration Release
   ```

5. **Create GitHub release**
   - Tag the release
   - Upload NuGet packages
   - Include release notes

## Getting Help

### Communication Channels

- **GitHub Issues** - Bug reports and feature requests
- **GitHub Discussions** - Questions and general discussion
- **Discord** (if available) - Real-time chat

### Maintainer Response Times

- **Bug reports** - Within 48 hours
- **Feature requests** - Within 1 week
- **Pull requests** - Within 1 week
- **Security issues** - Within 24 hours

### Code of Conduct Violations

Report violations to the maintainers through:
- Direct message to maintainers
- Email to project administrators
- GitHub's reporting features

---

Thank you for contributing to FluentAI.NET! 🚀

Your contributions help make AI integration easier for .NET developers worldwide.