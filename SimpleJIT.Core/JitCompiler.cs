using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace SimpleJIT.Core;

public unsafe abstract class JitCompiler
{
    public delegate int CompiledFunction();

    // Legacy method for flat instruction lists
    public static CompiledFunction? CompileInstructions(List<Instruction> instructions)
    {
        if(instructions == null)
            throw new ArgumentNullException(nameof(instructions), "Instructions cannot be null");
        JitCompiler compiler = RuntimeInformation.ProcessArchitecture == Architecture.Arm64
            ? new JitCompilerArm64()
            : new JitCompilerX64();
        return compiler.Compile(instructions);
    }

    // New method for function-based programs
    public static CompiledFunction? CompileProgram(Program program)
    {
        if(program == null)
            throw new ArgumentNullException(nameof(program), "Program cannot be null");
        
        var mainFunction = program.GetMainFunction();
        if (mainFunction == null)
            throw new InvalidOperationException("No Main function found");

        JitCompiler compiler = RuntimeInformation.ProcessArchitecture == Architecture.Arm64
            ? new JitCompilerArm64()
            : new JitCompilerX64();
        return compiler.CompileProgramInternal(program);
    }

    protected CompiledFunction? Compile(List<Instruction> instructions)
    {
        try
        {
            var codeBytes = GenerateCode(instructions);
            return CreateCompiledFunction(codeBytes);
        }
        catch
        {
            // If JIT compilation fails for any reason, return null
            // This allows graceful fallback to VM execution
            return null;
        }
    }

    protected CompiledFunction? CompileProgramInternal(Program program)
    {
        try
        {
            var codeBytes = GenerateProgramCode(program);
            return CreateCompiledFunction(codeBytes);
        }
        catch
        {
            // If JIT compilation fails for any reason, return null
            // This allows graceful fallback to VM execution
            return null;
        }
    }

    private CompiledFunction? CreateCompiledFunction(byte[] codeBytes)
    {
        var writableMemory = NativeMemoryManager.AllocateWritableMemory(codeBytes.Length);
        
        // Write the code to writable memory
        fixed (byte* codePtr = codeBytes)
        {
            Buffer.MemoryCopy(codePtr, writableMemory.ToPointer(), codeBytes.Length, codeBytes.Length);
        }

        // Change memory protection to executable
        var executableMemory = NativeMemoryManager.CommitExecutableMemory(writableMemory, codeBytes.Length);

        var functionPtr = Marshal.GetDelegateForFunctionPointer<CompiledFunction>(executableMemory);
        return functionPtr;
    }

    protected abstract byte[] GenerateCode(List<Instruction> instructions);
    
    // Default implementation: compile Main function only (subclasses can override for full function support)
    protected virtual byte[] GenerateProgramCode(Program program)
    {
        var mainFunction = program.GetMainFunction();
        if (mainFunction == null)
            throw new InvalidOperationException("No Main function found");
        
        // For now, just compile the main function instructions
        // This provides basic function support without full call/return implementation
        return GenerateCode(mainFunction.Instructions);
    }
}
