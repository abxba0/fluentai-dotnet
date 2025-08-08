# FluentAI.NET - AI Provider SDK

FluentAI.NET is a production-ready, extensible .NET SDK for interacting with multiple AI providers (OpenAI and Anthropic) through a unified interface. The project targets .NET 9.0 and is currently in active development with incomplete provider implementations.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### Initial Setup
- Install .NET 9.0 SDK (REQUIRED):
  ```bash
  curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version latest --channel 9.0
  export PATH="$HOME/.dotnet:$PATH"
  dotnet --version  # Should show 9.0.x (tested: shows 9.0.303)
  ```
- NEVER use the system .NET SDK (8.0.x) - it will fail with targeting errors
- ALWAYS export the PATH with `export PATH="$HOME/.dotnet:$PATH"` before any dotnet commands
- **VALIDATION**: Verify installation with `which dotnet` (should show `/home/runner/.dotnet/dotnet`)

### Build Commands and Timing
- `dotnet clean` -- takes 1-2 seconds. NEVER CANCEL.
- `dotnet restore` -- takes 1-2 seconds. NEVER CANCEL.
- `dotnet build --no-restore` -- takes 1-2 seconds but WILL FAIL due to incomplete implementations.
- `dotnet build` -- takes 2-3 seconds but WILL FAIL due to incomplete implementations.

**CRITICAL**: Set timeouts to 30+ seconds for all build commands. Even though they complete quickly, allow buffer time.

### Current Build Status
- **BUILD FAILS**: The codebase has incomplete implementations with 11+ compilation errors
- **MISSING METHODS**: `ValidateConfiguration`, `ProcessResponse`, `PrepareRequest`, `SendRequestAsync` are referenced but not implemented in provider classes
- **INCOMPLETE PROVIDERS**: Both OpenAI and Anthropic providers have stub implementations
- **NO TESTS**: No test projects exist in the repository currently

## Project Structure

### Key Directories
```
/home/runner/work/fluentai-dotnet/fluentai-dotnet/
├── Abstractions/           # Core interfaces and base classes
│   ├── IChatModel.cs      # Main interface for AI providers
│   ├── ChatModelBase.cs   # Base implementation with retry logic
│   ├── Models/            # Data models (ChatMessage, ChatResponse, etc.)
│   └── Exceptions/        # Custom exception types
├── Configuration/         # Options and configuration classes
├── Extensions/            # Dependency injection extensions
├── Providers/             # AI provider implementations
│   ├── OpenAI/            # OpenAI provider (INCOMPLETE)
│   └── Anthropic/         # Anthropic provider (INCOMPLETE)
└── FluentAI.NET.csproj    # Main project file
```

### Core Files to Know
- `IChatModel.cs` - Main interface defining `GetResponseAsync` and `StreamResponseAsync`
- `ChatModelBase.cs` - Base class with retry logic and message validation
- `ServiceCollectionExtensions.cs` - Dependency injection setup
- Provider implementations in `Providers/OpenAI/` and `Providers/Anthropic/`

## Development Guidelines

### Working with Incomplete Code
- **DO NOT** expect `dotnet build` to succeed initially
- **IMPLEMENT** missing methods before attempting to build:
  - `ValidateConfiguration()` - validates provider options
  - `ProcessResponse()` - converts provider response to ChatResponse
  - `PrepareRequest()` - creates provider-specific request objects
  - `SendRequestAsync()` - handles HTTP requests (Anthropic only)
- **EXPECT** compilation errors until all methods are implemented

### Making Changes
- ALWAYS run `export PATH="$HOME/.dotnet:$PATH"` before dotnet commands
- Test compilation frequently: `dotnet build --no-restore`
- Reference base classes and interfaces for expected method signatures
- Follow established patterns in `ChatModelBase.cs` for error handling and logging

### Package Dependencies
```xml
<PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Http" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Options" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version="8.0.0" />
<PackageReference Include="Azure.AI.OpenAI" Version="1.0.0-beta.17" />
```

## Validation and Testing

### Current Testing Status
- **NO UNIT TESTS**: No test projects exist
- **NO INTEGRATION TESTS**: No test infrastructure present
- **MANUAL TESTING**: Must create console app to test functionality

### Manual Validation Steps
1. **PREREQUISITE**: Complete the missing method implementations first (build will fail otherwise)
2. Create a simple console application to test:
   ```bash
   cd /tmp && mkdir fluentai-test && cd fluentai-test
   export PATH="$HOME/.dotnet:$PATH"
   dotnet new console -n TestFluentAI
   cd TestFluentAI
   dotnet add reference /home/runner/work/fluentai-dotnet/fluentai-dotnet/FluentAI.NET.csproj
   dotnet build  # Will fail until FluentAI.NET implementations are complete
   ```
3. **Expected Result**: Build will fail with 11+ compilation errors from FluentAI.NET
4. Test basic functionality (after implementations complete):
   - Configuration setup
   - Provider instantiation  
   - Basic chat completion (with valid API keys)

### Common Validation Scenarios
Since this is a library SDK, always test:
- **Configuration Loading**: Verify options are loaded correctly from configuration
- **Provider Selection**: Test both OpenAI and Anthropic providers
- **Error Handling**: Test invalid configurations and network failures
- **Dependency Injection**: Verify service registration works correctly

## Common Tasks

### Repository Root Contents
```
ls -la /home/runner/work/fluentai-dotnet/fluentai-dotnet/
.git/
.gitattributes
.gitignore
Abstractions/
Class1.cs (unused)
Configuration/
Extensions/
FluentAI.NET.csproj
FluentAI.NET.sln
Providers/
```

### Essential Commands
```bash
# Setup (run once per session)
export PATH="$HOME/.dotnet:$PATH"

# Quick validation
dotnet --version  # Should show 9.0.x
dotnet restore
dotnet build --no-restore  # Will fail until implementations complete

# File exploration
find . -name "*.cs" | head -20
grep -r "interface\|abstract" --include="*.cs" .
```

### Configuration Structure
The SDK expects configuration like:
```json
{
  "AiSdk": {
    "DefaultProvider": "openai"
  },
  "OpenAI": {
    "ApiKey": "sk-...",
    "Model": "gpt-4"
  },
  "Anthropic": {
    "ApiKey": "sk-ant-...",
    "Model": "claude-3-sonnet-20240229"
  }
}
```

## CRITICAL WARNINGS

- **NEVER CANCEL**: Always allow build commands to complete, even if fast
- **NEVER USE .NET 8**: Project requires .NET 9.0 SDK specifically
- **EXPECT BUILD FAILURES**: Codebase is incomplete and will not compile until missing methods are implemented
- **NO SHORTCUTS**: Cannot test functionality until implementation is complete
- **PATH REQUIRED**: Always export the .NET 9.0 PATH before any dotnet commands

## Build Failure Quick Reference

Current compilation errors to expect:
- `CS0103: The name 'ValidateConfiguration' does not exist`
- `CS0103: The name 'ProcessResponse' does not exist`
- `CS0103: The name 'PrepareRequest' does not exist`
- `CS0103: The name 'SendRequestAsync' does not exist`
- `CS0161: not all code paths return a value`
- `CS0117: 'ChatCompletionsOptions' does not contain a definition for 'TopP'`

These must be resolved before the library can be used or tested.