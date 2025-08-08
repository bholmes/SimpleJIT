using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace SimpleJIT;

public unsafe class JitCompiler
{
    private const uint PAGE_EXECUTE_READWRITE = 0x40;
    private const uint MEM_COMMIT = 0x1000;
    private const uint MEM_RESERVE = 0x2000;
    private const uint MEM_RELEASE = 0x8000;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool VirtualFree(IntPtr lpAddress, UIntPtr dwSize, uint dwFreeType);

    [DllImport("libc", EntryPoint = "mmap")]
    private static extern IntPtr mmap_unix(IntPtr addr, UIntPtr length, int prot, int flags, int fd, IntPtr offset);

    [DllImport("libc", EntryPoint = "munmap")]
    private static extern int munmap_unix(IntPtr addr, UIntPtr length);

    private const int PROT_READ = 1;
    private const int PROT_WRITE = 2;
    private const int PROT_EXEC = 4;
    private const int MAP_PRIVATE = 2;
    private const int MAP_ANONYMOUS = 4096; // 0x1000 on macOS

    public delegate long CompiledFunction();

    public static CompiledFunction CompileInstructions(List<Instruction> instructions)
    {
        var compiler = new JitCompiler();
        return compiler.Compile(instructions);
    }

    private CompiledFunction Compile(List<Instruction> instructions)
    {
        var codeBytes = GenerateCode(instructions);
        var executableMemory = AllocateExecutableMemory(codeBytes.Length);
        
        fixed (byte* codePtr = codeBytes)
        {
            Buffer.MemoryCopy(codePtr, executableMemory.ToPointer(), codeBytes.Length, codeBytes.Length);
        }

        var functionPtr = Marshal.GetDelegateForFunctionPointer<CompiledFunction>(executableMemory);
        return functionPtr;
    }

    private IntPtr AllocateExecutableMemory(int size)
    {
        IntPtr memory;
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            memory = VirtualAlloc(IntPtr.Zero, (UIntPtr)size, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
        }
        else
        {
            // On Unix/macOS, use mmap with -1 for anonymous mapping
            memory = mmap_unix(IntPtr.Zero, (UIntPtr)size, PROT_READ | PROT_WRITE | PROT_EXEC, 
                              MAP_PRIVATE | MAP_ANONYMOUS, -1, IntPtr.Zero);
            
            // Check for MAP_FAILED (which is -1 cast to IntPtr)
            if (memory == new IntPtr(-1))
                memory = IntPtr.Zero;
        }

        if (memory == IntPtr.Zero)
            throw new InvalidOperationException("Failed to allocate executable memory");

        return memory;
    }

    private byte[] GenerateCode(List<Instruction> instructions)
    {
        var code = new List<byte>();
        
        // Function prologue - setup stack frame
        // push rbp
        code.Add(0x55);
        // mov rbp, rsp
        code.AddRange([0x48, 0x89, 0xE5]);
        
        // Reserve space for local stack (simulating our VM stack)
        // sub rsp, 512 (space for 64 8-byte values)
        code.AddRange([0x48, 0x81, 0xEC, 0x00, 0x02, 0x00, 0x00]);
        
        // Initialize stack pointer (r12 will hold our stack top index)
        // xor r12, r12
        code.AddRange([0x4D, 0x31, 0xE4]);

        foreach (var instruction in instructions)
        {
            switch (instruction.Type)
            {
                case InstructionType.Load:
                    EmitLoad(code, instruction.Value);
                    break;
                case InstructionType.Add:
                    EmitAdd(code);
                    break;
                case InstructionType.Sub:
                    EmitSub(code);
                    break;
                case InstructionType.Mul:
                    EmitMul(code);
                    break;
                case InstructionType.Div:
                    EmitDiv(code);
                    break;
                case InstructionType.Print:
                    EmitPrint(code);
                    break;
                case InstructionType.Return:
                    break; // Handle at the end
            }
        }

        // Function epilogue - get top stack value as return value
        // mov rax, [rsp + r12*8 - 8] (get top stack value)
        code.AddRange([0x4A, 0x8B, 0x44, 0xE4, 0xF8]);
        
        // Restore stack
        // add rsp, 512
        code.AddRange([0x48, 0x81, 0xC4, 0x00, 0x02, 0x00, 0x00]);
        
        // pop rbp
        code.Add(0x5D);
        // ret
        code.Add(0xC3);

        return code.ToArray();
    }

    private void EmitLoad(List<byte> code, long value)
    {
        // mov rax, immediate value
        code.AddRange([0x48, 0xB8]);
        code.AddRange(BitConverter.GetBytes(value));
        
        // mov [rsp + r12*8], rax (push to our stack)
        code.AddRange([0x4A, 0x89, 0x04, 0xE4]);
        
        // inc r12 (increment stack pointer)
        code.AddRange([0x49, 0xFF, 0xC4]);
    }

    private void EmitAdd(List<byte> code)
    {
        // dec r12 (pop first operand)
        code.AddRange([0x49, 0xFF, 0xCC]);
        
        // mov rax, [rsp + r12*8] (get first operand)
        code.AddRange([0x4A, 0x8B, 0x04, 0xE4]);
        
        // dec r12 (pop second operand)
        code.AddRange([0x49, 0xFF, 0xCC]);
        
        // add rax, [rsp + r12*8] (add second operand)
        code.AddRange([0x4A, 0x03, 0x04, 0xE4]);
        
        // mov [rsp + r12*8], rax (push result)
        code.AddRange([0x4A, 0x89, 0x04, 0xE4]);
        
        // inc r12 (increment stack pointer)
        code.AddRange([0x49, 0xFF, 0xC4]);
    }

    private void EmitSub(List<byte> code)
    {
        // Similar to add but with sub instruction
        code.AddRange([0x49, 0xFF, 0xCC]); // dec r12
        code.AddRange([0x4A, 0x8B, 0x04, 0xE4]); // mov rax, [rsp + r12*8]
        code.AddRange([0x49, 0xFF, 0xCC]); // dec r12
        code.AddRange([0x4A, 0x2B, 0x04, 0xE4]); // sub rax, [rsp + r12*8]
        code.AddRange([0x4A, 0x89, 0x04, 0xE4]); // mov [rsp + r12*8], rax
        code.AddRange([0x49, 0xFF, 0xC4]); // inc r12
    }

    private void EmitMul(List<byte> code)
    {
        code.AddRange([0x49, 0xFF, 0xCC]); // dec r12
        code.AddRange([0x4A, 0x8B, 0x04, 0xE4]); // mov rax, [rsp + r12*8]
        code.AddRange([0x49, 0xFF, 0xCC]); // dec r12
        code.AddRange([0x4A, 0x0F, 0xAF, 0x04, 0xE4]); // imul rax, [rsp + r12*8]
        code.AddRange([0x4A, 0x89, 0x04, 0xE4]); // mov [rsp + r12*8], rax
        code.AddRange([0x49, 0xFF, 0xC4]); // inc r12
    }

    private void EmitDiv(List<byte> code)
    {
        code.AddRange([0x49, 0xFF, 0xCC]); // dec r12
        code.AddRange([0x4A, 0x8B, 0x04, 0xE4]); // mov rax, [rsp + r12*8]
        code.AddRange([0x49, 0xFF, 0xCC]); // dec r12
        code.AddRange([0x48, 0x99]); // cqo (sign extend rax to rdx:rax)
        code.AddRange([0x4A, 0xF7, 0x3C, 0xE4]); // idiv [rsp + r12*8]
        code.AddRange([0x4A, 0x89, 0x04, 0xE4]); // mov [rsp + r12*8], rax
        code.AddRange([0x49, 0xFF, 0xC4]); // inc r12
    }

    private void EmitPrint(List<byte> code)
    {
        // For simplicity, we'll just keep the value on stack without actual printing
        // In a real implementation, you'd call printf or similar
        // For now, this is a no-op that leaves the stack unchanged
    }
}
