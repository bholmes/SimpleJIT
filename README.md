# SimpleJIT - A Simple Just-In-Time Compiler with Virtual Machine

A C# implementation of a simple JIT compiler and virtual machine that can read instruction files and execute them using either:
1. **Virtual Machine Interpreter** - Safe, cross-platform execution 
2. **JIT Compilation** - Direct native code generation (platform dependent)

## Features

- Parse simple instruction language from text files
- Execute using a stack-based virtual machine interpreter
- Attempt JIT compilation to native x64 assembly (where supported)
- Cross-platform compatibility with fallback execution
- Support for arithmetic operations and debugging prints

## Supported Instructions

- `load <value>` - Load immediate value onto stack
- `add` - Add top two stack values
- `sub` - Subtract top stack value from second-to-top
- `mul` - Multiply top two stack values
- `div` - Divide second-to-top by top stack value
- `print` - Print top stack value (debugging)
- `ret` - Return from program

## Usage

```bash
dotnet run <instruction_file>
```

## Example Results

The program will:
1. Parse the instruction file
2. Execute using the VM interpreter (always works)
3. Attempt JIT compilation (may fail on some platforms due to security restrictions)

## Example Instruction Files

### Simple Addition (`simple.txt`)
```
// Simple addition: 7 + 3 = 10
load 7
load 3
add
ret
```

### Complex Arithmetic (`complex.txt`)
```
// Complex arithmetic: ((15 - 3) * 2) / 4 = 6
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

### Multiple Operations (`multi.txt`)
```
// Multiple operations with prints
load 100
load 25
sub        // 75
print
load 5
mul        // 375
print
load 3
div        // 125
print
ret
```

## Platform Compatibility

- **Virtual Machine**: Works on all platforms (Windows, macOS, Linux)
- **JIT Compilation**: May work on Windows and some Linux configurations, but is restricted on modern macOS due to security policies

## Architecture

The project demonstrates:
- **Lexical Analysis**: Simple instruction parsing
- **Virtual Machine**: Stack-based interpreter
- **JIT Compilation**: Direct x64 assembly generation
- **Memory Management**: Platform-specific executable memory allocation
- **Error Handling**: Graceful fallback between execution methods
