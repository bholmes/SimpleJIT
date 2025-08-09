using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace SimpleJIT.Core;

public unsafe abstract class JitCompiler
{
    private const uint PAGE_READWRITE = 0x04;
    private const uint PAGE_EXECUTE_READ = 0x20;
    private const uint MEM_COMMIT = 0x1000;
    private const uint MEM_RESERVE = 0x2000;
    private const uint MEM_RELEASE = 0x8000;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool VirtualFree(IntPtr lpAddress, UIntPtr dwSize, uint dwFreeType);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

    [DllImport("libc", EntryPoint = "mmap")]
    private static extern IntPtr mmap_unix(IntPtr addr, UIntPtr length, int prot, int flags, int fd, IntPtr offset);

    [DllImport("libc", EntryPoint = "munmap")]
    private static extern int munmap_unix(IntPtr addr, UIntPtr length);

    [DllImport("libc", EntryPoint = "mprotect")]
    private static extern int mprotect_unix(IntPtr addr, UIntPtr len, int prot);

    private const int PROT_READ = 1;
    private const int PROT_WRITE = 2;
    private const int PROT_EXEC = 4;
    private const int MAP_PRIVATE = 2;
    private const int MAP_ANONYMOUS = 4096; // 0x1000 on macOS
    private const int MAP_JIT = 0x800; // Only used on macOS, ignored elsewhere

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
            var writableMemory = AllocateWritableMemory(codeBytes.Length);
            
            // Write the code to writable memory
            fixed (byte* codePtr = codeBytes)
            {
                Buffer.MemoryCopy(codePtr, writableMemory.ToPointer(), codeBytes.Length, codeBytes.Length);
            }

            // Change memory protection to executable
            var executableMemory = CommitExecutableMemory(writableMemory, codeBytes.Length);

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

    protected IntPtr AllocateWritableMemory(int size)
    {
        IntPtr memory;
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            memory = VirtualAlloc(IntPtr.Zero, (UIntPtr)size, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
        }
        else
        {
            int flags = MAP_PRIVATE | MAP_ANONYMOUS;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                flags |= MAP_JIT;
            }
            memory = mmap_unix(IntPtr.Zero, (UIntPtr)size, PROT_READ | PROT_WRITE, 
                              flags, -1, IntPtr.Zero);
            
            // Check for MAP_FAILED (which is -1 cast to IntPtr)
            if (memory == new IntPtr(-1))
                memory = IntPtr.Zero;
        }

        if (memory == IntPtr.Zero)
            throw new InvalidOperationException("Failed to allocate writable memory");

        return memory;
    }

    protected IntPtr CommitExecutableMemory(IntPtr memory, int size)
    {
        bool success;
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Change protection from PAGE_READWRITE to PAGE_EXECUTE_READ
            success = VirtualProtect(memory, (UIntPtr)size, PAGE_EXECUTE_READ, out _);
        }
        else
        {
            // On Unix/macOS, use mprotect to change from RW to RX
            var result = mprotect_unix(memory, (UIntPtr)size, PROT_READ | PROT_EXEC);
            success = result == 0;
        }

        if (!success)
            throw new InvalidOperationException("Failed to commit executable memory");

        return memory;
    }
}
