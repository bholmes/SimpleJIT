# SimpleJIT - A Simple Just-In-Time Compiler with Virtual Machine

A C# implementation of a simple JIT compiler and virtual machine that can read instruction files and execute them using either:
1. **Virtual Machine Interpreter** - Safe, cross-platform execution 
2. **JIT Compilation** - Cross-architecture native code generation with full ARM64 and x64 support

## Project Structure

- **SimpleJIT/** - Console application (entry point)
  - `Program.cs` - Main application logic
  - `SimpleJIT.csproj` - Console app project file
- **SimpleJIT.Core/** - Reusable library containing core functionality
  - `Instruction.cs` - Instruction definitions and types
  - `Parser.cs` - Instruction file parsing with comment support
  - `VirtualMachine.cs` - Stack-based interpreter  
  - `JitCompiler.cs` - Abstract base class for JIT compilation with factory method
  - `JitCompilerArm64.cs` - ARM64-specific native code generation
  - `JitCompilerX64.cs` - x64-specific native code generation
  - `NativeMemoryManager.cs` - Cross-platform memory allocation and protection utilities
  - `SimpleJIT.Core.csproj` - Library project file
- **SimpleJIT.Tests/** - Comprehensive test suite
  - `InstructionTests.cs` - Unit tests for instruction types
  - `ParserTests.cs` - Unit tests for file parsing and error handling
  - `VirtualMachineTests.cs` - Unit tests for VM execution and stack operations
  - `JitCompilerTests.cs` - Unit tests for JIT compilation (platform-aware)
  - `IntegrationTests.cs` - End-to-end integration tests
  - `SimpleJIT.Tests.csproj` - Test project with xUnit framework
- **samples/** - Example instruction files
  - `simple.txt` - Basic arithmetic example
  - `complex.txt` - Complex expression example
  - `multi.txt` - Multiple operations with debugging
  - `example.txt` - Comprehensive demonstration
  - `divzero.txt` - Error handling test
  - `invalid.txt` - Parser error test

## Features

- **Parse simple instruction language** from text files with support for `#` and `//` comments
- **Execute using a stack-based virtual machine** interpreter (guaranteed cross-platform)
- **Cross-architecture JIT compilation** with clean separation between ARM64 and x64 code generation
- **Modular architecture** with dedicated classes for each processor architecture
- **Advanced memory management** with specialized utility class for cross-platform native memory operations
- **Two-stage memory allocation** with security-compliant executable memory management
- **Automatic architecture detection** with processor-specific code generation
- **Cross-platform compatibility** with intelligent fallback execution strategies
- **Comprehensive error handling** for stack underflow, division by zero, and file parsing errors
- **Professional testing suite** with 52 unit and integration tests using xUnit framework
- **Support for arithmetic operations** (add, sub, mul, div) and debugging prints
- **Flexible comment syntax** supporting both hash (`#`) and double-slash (`//`) comments
- **Platform-aware JIT compilation** that gracefully handles security restrictions

## Supported Instructions

- `load <value>` - Load immediate value onto stack (supports negative numbers)
- `add` - Add top two stack values  
- `sub` - Subtract top stack value from second-to-top
- `mul` - Multiply top two stack values
- `div` - Divide second-to-top by top stack value (with division by zero detection)
- `print` - Print top stack value to console (debugging aid)
- `ret` - Return from program with top stack value as result

### Comment Support
- `# comment` - Hash-style comments (can be inline or full line)
- `// comment` - C-style comments (can be inline or full line)

## Usage

### Console Application
```bash
# From the solution root (builds all projects)
dotnet build SimpleJIT.sln
dotnet run --project SimpleJIT samples/<instruction_file>

# Or build and run specific project
cd SimpleJIT
dotnet run ../samples/<instruction_file>
```

### As a Library
```csharp
using SimpleJIT.Core;

// Parse instructions from file
var instructions = Parser.ParseFile("myprogram.txt");

// Execute with VM (always works cross-platform)
var vm = new VirtualMachine();
var result = vm.Execute(instructions);

// Attempt JIT compilation (automatically selects architecture-specific compiler)
var compiledFunction = JitCompiler.CompileInstructions(instructions);
if (compiledFunction != null)
{
    var jitResult = compiledFunction();
    Console.WriteLine($"JIT result: {jitResult}");
    Console.WriteLine($"Architecture: {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}");
}
else
{
    Console.WriteLine("JIT compilation failed - using VM result");
}
```

### Running Tests
```bash
# Run all tests (via solution)
dotnet test

# Run tests via solution with detailed output
dotnet test SimpleJIT.sln --verbosity normal

# Run tests via specific project
dotnet test SimpleJIT.Tests/SimpleJIT.Tests.csproj --verbosity normal

# Run specific test category
dotnet test --filter "FullyQualifiedName~Unit"
dotnet test --filter "FullyQualifiedName~Integration"
```

## Example Results

The program will:
1. Parse the instruction file
2. Execute using the VM interpreter (always works cross-platform)
3. Perform JIT compilation with automatic architecture detection (x64 or ARM64)
4. Execute JIT-compiled native code for optimal performance
5. Compare results between VM and JIT execution to ensure correctness

## Example Instruction Files

### Simple Addition (`samples/simple.txt`)
```
// Simple addition: 7 + 3 = 10
load 7
load 3
add
ret
```

### Complex Arithmetic (`samples/complex.txt`)
```
# Complex arithmetic: ((15 - 3) * 2) / 4 = 6
load 15
load 3
sub
load 2
mul
load 4
div
print
ret
```

### Multiple Operations with Comments (`samples/multi.txt`)
```
// Multiple operations with prints
load 100
load 25
sub        # Result: 75
print
load 5
mul        # Result: 375
print
load 3
div        # Result: 125
print
ret
```

## Platform Compatibility

- **Virtual Machine**: Works on all platforms (Windows, macOS, Linux)
- **JIT Compilation**: 
  - ✅ **Windows x64**: Full support with executable memory allocation
  - ✅ **Windows ARM64**: Full support with architecture-specific code generation
  - ✅ **Linux x64**: Works on most distributions, depends on security settings
  - ✅ **Linux ARM64**: Works on most distributions with ARM64 processors
  - ✅ **macOS Intel (x64)**: Full support with two-stage memory allocation
  - ✅ **macOS Apple Silicon (ARM64)**: Full support with native ARM64 code generation
- **Architecture Detection**: Automatically detects x64 vs ARM64 and generates appropriate assembly
- **Memory Management**: Uses two-stage allocation (read/write → read/execute) to comply with modern security policies
- **Test Suite**: All 52 tests pass on all platforms with full JIT functionality

## Testing

The project includes a comprehensive test suite with **52 tests** covering:

- **Unit Tests**: Individual component testing (parser, VM, instructions, JIT compiler)
- **Integration Tests**: End-to-end workflow testing from file parsing to execution
- **Platform-Aware Testing**: All tests pass reliably across all platforms
- **JIT Testing**: JIT compilation tests are now fully functional with graceful environment handling
- **Error Condition Testing**: Division by zero, stack underflow, invalid syntax
- **Cross-Platform Validation**: VM and core functionality validated on Windows, macOS, and Linux

### Test Categories
- **Instruction Tests** (10 tests): Constructor validation and string representation
- **Parser Tests** (9 tests): File parsing, comment handling, error conditions  
- **Virtual Machine Tests** (14 tests): Stack operations, arithmetic, error handling
- **JIT Compiler Tests** (10 tests): Native compilation (works in production, limited in test environment)
- **Integration Tests** (9 tests): End-to-end scenarios with comprehensive coverage

### Test Status
- **✅ All Tests Pass**: 52/52 tests pass consistently across all platforms
- **✅ Production JIT**: 100% functional on all architectures (x64, ARM64) and platforms
- **✅ Test Environment**: All JIT tests now work reliably in test environments
- **✅ Cross-Platform**: Full validation on Windows, macOS, and Linux

## Architecture

The project demonstrates several key computer science concepts:

- **Lexical Analysis**: Robust instruction parsing with comment support and error handling
- **Virtual Machine Design**: Stack-based interpreter with comprehensive instruction set
- **Modular JIT Architecture**: Clean separation between base JIT logic and architecture-specific code generation
- **Cross-Architecture Code Generation**: Dedicated compilers for x64 and ARM64 with native instruction encoding
- **Advanced Memory Management**: Specialized utility class for cross-platform native memory operations
- **Two-Stage Memory Allocation**: Security-compliant executable memory management (RW → RX)
- **Factory Pattern**: Runtime architecture detection with automatic compiler selection
- **Platform Abstraction**: Unified memory management across Windows, macOS, and Linux
- **Error Handling**: Graceful degradation and comprehensive exception handling
- **Cross-Platform Development**: True cross-architecture compatibility with native performance
- **Test-Driven Development**: Comprehensive test coverage with xUnit framework
- **Clean Architecture**: Separation of concerns with dedicated classes for parsing, execution, compilation, and memory management

### Technical Highlights

- **Modular JIT Design**: Clean inheritance hierarchy with abstract base class and architecture-specific derived classes
- **Specialized Memory Management**: Dedicated NativeMemoryManager utility class for cross-platform memory operations
- **Architecture-Specific Code Generation**: Separate JitCompilerArm64 and JitCompilerX64 classes with processor-specific optimizations
- **Factory Pattern Implementation**: Automatic architecture detection and compiler instantiation
- **Unsafe Code**: Uses C# unsafe blocks for direct memory manipulation in JIT compiler
- **Platform Interop**: Cross-platform memory allocation (VirtualAlloc on Windows, mmap on Unix)
- **ARM64 Assembly**: Native ARM64 instruction encoding with proper function prologue/epilogue and MOVN for negative numbers
- **x64 Assembly**: Traditional x64 instruction generation for Intel/AMD processors
- **Negative Number Support**: Full support for negative immediate values with proper ARM64 MOVN instruction encoding
- **Two-Stage Allocation**: AllocateWritableMemory() → write code → CommitExecutableMemory()
- **Delegate Generation**: Dynamic function pointer creation for JIT-compiled code
- **Runtime Architecture Detection**: Uses RuntimeInformation.ProcessArchitecture for code generation
- **Stack Machine**: Classic stack-based virtual machine implementation
- **Recursive Descent**: Simple but effective parsing strategy
- **Professional Testing**: Industry-standard testing practices with comprehensive coverage
