# Git Commit Message for SimpleJIT Project

## Summary
feat: Implement SimpleJIT - A dual-mode JIT compiler and virtual machine

## Detailed Description

### What was built:
A comprehensive C# application demonstrating both interpretation and JIT compilation techniques for a simple stack-based instruction language.

### Core Components Implemented:

#### 1. Instruction Parser (Parser.cs)
- Parses simple instruction files with support for arithmetic operations
- Handles comments (// syntax) and error conditions
- Supports: load, add, sub, mul, div, print, ret instructions
- Robust error handling for malformed instructions

#### 2. Virtual Machine (VirtualMachine.cs)
- Stack-based interpreter providing safe, cross-platform execution
- Implements all arithmetic operations with proper error handling
- Division by zero protection and stack underflow detection
- Always functional regardless of platform security restrictions

#### 3. JIT Compiler (JitCompiler.cs)
- Generates native x64 assembly instructions
- Platform-specific executable memory allocation (VirtualAlloc/mmap)
- Handles Windows, Linux, and macOS memory management differences
- Graceful fallback when platform security prevents JIT execution

#### 4. Main Program (Program.cs)
- Dual execution mode: VM interpreter + JIT compiler
- Comprehensive error handling and user feedback
- Command-line interface with usage help
- Comparison between VM and JIT results when both work

### Test Coverage:
- simple.txt: Basic arithmetic (7 + 3 = 10)
- complex.txt: Complex expressions ((15-3)*2)/4 = 6)
- multi.txt: Multiple operations with intermediate debugging
- example.txt: Comprehensive demonstration
- divzero.txt: Error handling validation
- invalid.txt: Parser error testing
- test.sh: Automated test suite

### Platform Compatibility:
- Virtual Machine: Works on all platforms (Windows, macOS, Linux)
- JIT Compilation: Platform-dependent due to modern security restrictions
- Graceful degradation with informative error messages

### Educational Value:
Demonstrates key compiler/interpreter concepts:
- Lexical analysis and parsing
- Stack-based virtual machine architecture
- Just-in-time compilation to native code
- Platform-specific memory management
- Error handling and recovery strategies
- Cross-platform development considerations

### Build System:
- .NET 8.0 project with unsafe code support
- Cross-platform compatibility
- Comprehensive documentation in README.md
- Automated testing infrastructure

This implementation serves as both a functional tool and educational resource
for understanding the fundamentals of language implementation, from parsing
through interpretation to native code generation.
