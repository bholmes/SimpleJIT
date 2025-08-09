using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace SimpleJIT.Core;

public unsafe abstract class JitCompiler
{
    public delegate long CompiledFunction();

    public static CompiledFunction? CompileInstructions(List<Instruction> instructions)
    {
        JitCompiler compiler = RuntimeInformation.ProcessArchitecture == Architecture.Arm64
            ? new JitCompilerArm64()
            : new JitCompilerX64();
        return compiler.Compile(instructions);
    }

    protected CompiledFunction? Compile(List<Instruction> instructions)
    {
        try
        {
            var codeBytes = GenerateCode(instructions);
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
        catch
        {
            // If JIT compilation fails for any reason, return null
            // This allows graceful fallback to VM execution
            return null;
        }
    }

    protected abstract byte[] GenerateCode(List<Instruction> instructions);
}
