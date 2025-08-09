using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace SimpleJIT.Core;

public unsafe class JitCompiler
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
        var compiler = new JitCompiler();
        return compiler.Compile(instructions);
    }

    private CompiledFunction? Compile(List<Instruction> instructions)
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

    private IntPtr AllocateWritableMemory(int size)
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

    private IntPtr CommitExecutableMemory(IntPtr memory, int size)
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

    private byte[] GenerateCode(List<Instruction> instructions)
    {
        if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
        {
            return GenerateArm64Code(instructions);
        }
        else
        {
            return GenerateX64Code(instructions);
        }
    }

    private byte[] GenerateArm64Code(List<Instruction> instructions)
    {
        var code = new List<byte>();
        
        // ARM64 function prologue
        // stp x29, x30, [sp, #-16]!    // Save frame pointer and link register
        code.AddRange([0xFD, 0x7B, 0xBF, 0xA9]);
        // mov x29, sp                   // Set up frame pointer
        code.AddRange([0xFD, 0x03, 0x00, 0x91]);
        
        // Handle empty instructions case early
        if (instructions.Count == 0)
        {
            // mov x0, #0                    // Return 0 for empty instructions
            code.AddRange([0x00, 0x00, 0x80, 0xD2]);
            
            // ldp x29, x30, [sp], #16       // Restore frame pointer and link register
            code.AddRange([0xFD, 0x7B, 0xC1, 0xA8]);
            // ret                           // Return
            code.AddRange([0xC0, 0x03, 0x5F, 0xD6]);
            
            return code.ToArray();
        }
        
        // Reserve space for local stack (simulating our VM stack)
        // sub sp, sp, #512              // Reserve 512 bytes for stack
        code.AddRange([0xFF, 0x03, 0x08, 0xD1]);
        
        // Initialize stack pointer (x19 will hold our stack top index)
        // mov x19, #0                   // x19 = stack pointer index
        code.AddRange([0x13, 0x00, 0x80, 0xD2]);

        foreach (var instruction in instructions)
        {
            switch (instruction.Type)
            {
                case InstructionType.Load:
                    EmitLoadArm64(code, instruction.Value);
                    break;
                case InstructionType.Add:
                    EmitAddArm64(code);
                    break;
                case InstructionType.Sub:
                    EmitSubArm64(code);
                    break;
                case InstructionType.Mul:
                    EmitMulArm64(code);
                    break;
                case InstructionType.Div:
                    EmitDivArm64(code);
                    break;
                case InstructionType.Print:
                    EmitPrintArm64(code);
                    break;
                case InstructionType.Return:
                    break; // Handle at the end
            }
        }

        // Function epilogue - get top stack value as return value
        // Get the top value from stack (x19 points to next slot, so x19-1 is the top)
        // sub x19, x19, #1              // Decrement to get last pushed value index
        code.AddRange([0x73, 0x06, 0x00, 0xD1]);
        // ldr x0, [sp, x19, lsl #3]     // Load value at stack[x19] into x0
        code.AddRange([0xE0, 0x7B, 0x73, 0xF8]);
        
        // Restore stack
        // add sp, sp, #512              // Restore stack pointer
        code.AddRange([0xFF, 0x03, 0x08, 0x91]);
        
        // ldp x29, x30, [sp], #16       // Restore frame pointer and link register
        code.AddRange([0xFD, 0x7B, 0xC1, 0xA8]);
        // ret                           // Return
        code.AddRange([0xC0, 0x03, 0x5F, 0xD6]);

        return code.ToArray();
    }

    private byte[] GenerateX64Code(List<Instruction> instructions)
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
                    EmitLoadX64(code, instruction.Value);
                    break;
                case InstructionType.Add:
                    EmitAddX64(code);
                    break;
                case InstructionType.Sub:
                    EmitSubX64(code);
                    break;
                case InstructionType.Mul:
                    EmitMulX64(code);
                    break;
                case InstructionType.Div:
                    EmitDivX64(code);
                    break;
                case InstructionType.Print:
                    EmitPrintX64(code);
                    break;
                case InstructionType.Return:
                    break; // Handle at the end
            }
        }

        // Function epilogue - get top stack value as return value
        // Default to 0 if no instructions were executed
        // xor rax, rax (set rax = 0)
        code.AddRange([0x48, 0x31, 0xC0]);
        
        // If stack has values (r12 > 0), get the top value
        // test r12, r12
        code.AddRange([0x4D, 0x85, 0xE4]);
        // je skip_load (if r12 == 0, skip loading from stack)
        code.AddRange([0x74, 0x05]);
        // mov rax, [rsp + r12*8 - 8] (get top stack value)
        code.AddRange([0x4A, 0x8B, 0x44, 0xE4, 0xF8]);
        // skip_load:
        
        // Restore stack
        // add rsp, 512
        code.AddRange([0x48, 0x81, 0xC4, 0x00, 0x02, 0x00, 0x00]);
        
        // pop rbp
        code.Add(0x5D);
        // ret
        code.Add(0xC3);

        return code.ToArray();
    }

    // ARM64 instruction emitters
    private void EmitLoadArm64(List<byte> code, long value)
    {
        // Load the immediate value into x0 using correct ARM64 encoding
        // For negative values, we need to handle them as 64-bit signed values
        
        if (value >= 0 && value <= 0xFFFF)
        {
            // movz x0, #value (move zero with 16-bit immediate)
            var imm16 = (ushort)value;
            var instruction = 0xD2800000u | ((uint)imm16 << 5) | 0u; // target register x0
            code.AddRange(BitConverter.GetBytes(instruction));
        }
        else if (value < 0)
        {
            // For negative values, use movn (move NOT) instruction
            // movn x0, #(~value & 0xFFFF)  - Move NOT of the complement
            var complement = (ushort)(~value & 0xFFFF);
            var instruction = 0x92800000u | ((uint)complement << 5) | 0u; // MOVN x0, #complement
            code.AddRange(BitConverter.GetBytes(instruction));
        }
        else
        {
            // For larger positive values, use movz + movk sequence
            // movz x0, #(value & 0xFFFF)
            var low16 = (ushort)(value & 0xFFFF);
            var instruction1 = 0xD2800000u | ((uint)low16 << 5) | 0u;
            code.AddRange(BitConverter.GetBytes(instruction1));
            
            // movk x0, #((value >> 16) & 0xFFFF), lsl #16
            var mid16 = (ushort)((value >> 16) & 0xFFFF);
            if (mid16 != 0)
            {
                var instruction2 = 0xF2A00000u | ((uint)mid16 << 5) | 0u;
                code.AddRange(BitConverter.GetBytes(instruction2));
            }
        }
        
        // Store x0 to stack at position [sp + x19*8]
        // str x0, [sp, x19, lsl #3]     
        code.AddRange([0xE0, 0x7B, 0x33, 0xF8]);
        
        // Increment stack pointer: add x19, x19, #1
        code.AddRange([0x73, 0x06, 0x00, 0x91]);
    }

    private void EmitAddArm64(List<byte> code)
    {
        // Pop first operand (most recent)
        // sub x19, x19, #1              // Decrement stack pointer
        code.AddRange([0x73, 0x06, 0x00, 0xD1]);
        // ldr x0, [sp, x19, lsl #3]     // Load first operand into x0
        code.AddRange([0xE0, 0x7B, 0x73, 0xF8]);
        
        // Pop second operand  
        // sub x19, x19, #1              // Decrement stack pointer  
        code.AddRange([0x73, 0x06, 0x00, 0xD1]);
        // ldr x1, [sp, x19, lsl #3]     // Load second operand into x1
        code.AddRange([0xE1, 0x7B, 0x73, 0xF8]);
        
        // add x0, x1, x0               // Add operands (x1 + x0)
        code.AddRange([0x20, 0x00, 0x00, 0x8B]);
        
        // Push result back to stack
        // str x0, [sp, x19, lsl #3]     // Store result at current stack position
        code.AddRange([0xE0, 0x7B, 0x33, 0xF8]);
        
        // add x19, x19, #1              // Increment stack pointer
        code.AddRange([0x73, 0x06, 0x00, 0x91]);
    }

    private void EmitSubArm64(List<byte> code)
    {
        // sub x19, x19, #1              // Decrement stack pointer
        code.AddRange([0x73, 0x06, 0x00, 0xD1]);
        // ldr x0, [sp, x19, lsl #3]     // Load first operand
        code.AddRange([0xE0, 0x7B, 0x73, 0xF8]);
        
        // sub x19, x19, #1              // Decrement stack pointer  
        code.AddRange([0x73, 0x06, 0x00, 0xD1]);
        // ldr x1, [sp, x19, lsl #3]     // Load second operand
        code.AddRange([0xE1, 0x7B, 0x73, 0xF8]);
        
        // sub x0, x1, x0               // Subtract operands (x1 - x0)
        code.AddRange([0x20, 0x00, 0x00, 0xCB]);
        
        // str x0, [sp, x19, lsl #3]     // Store result
        code.AddRange([0xE0, 0x7B, 0x33, 0xF8]);
        
        // add x19, x19, #1              // Increment stack pointer
        code.AddRange([0x73, 0x06, 0x00, 0x91]);
    }

    private void EmitMulArm64(List<byte> code)
    {
        // sub x19, x19, #1              // Decrement stack pointer
        code.AddRange([0x73, 0x06, 0x00, 0xD1]);
        // ldr x0, [sp, x19, lsl #3]     // Load first operand
        code.AddRange([0xE0, 0x7B, 0x73, 0xF8]);
        
        // sub x19, x19, #1              // Decrement stack pointer  
        code.AddRange([0x73, 0x06, 0x00, 0xD1]);
        // ldr x1, [sp, x19, lsl #3]     // Load second operand
        code.AddRange([0xE1, 0x7B, 0x73, 0xF8]);
        
        // mul x0, x1, x0               // Multiply operands
        code.AddRange([0x20, 0x7C, 0x00, 0x9B]);
        
        // str x0, [sp, x19, lsl #3]     // Store result
        code.AddRange([0xE0, 0x7B, 0x33, 0xF8]);
        
        // add x19, x19, #1              // Increment stack pointer
        code.AddRange([0x73, 0x06, 0x00, 0x91]);
    }

    private void EmitDivArm64(List<byte> code)
    {
        // sub x19, x19, #1              // Decrement stack pointer
        code.AddRange([0x73, 0x06, 0x00, 0xD1]);
        // ldr x0, [sp, x19, lsl #3]     // Load first operand (divisor)
        code.AddRange([0xE0, 0x7B, 0x73, 0xF8]);
        
        // sub x19, x19, #1              // Decrement stack pointer  
        code.AddRange([0x73, 0x06, 0x00, 0xD1]);
        // ldr x1, [sp, x19, lsl #3]     // Load second operand (dividend)
        code.AddRange([0xE1, 0x7B, 0x73, 0xF8]);
        
        // sdiv x0, x1, x0              // Signed divide operands (x1 / x0)
        code.AddRange([0x20, 0x0C, 0xC0, 0x9A]);
        
        // str x0, [sp, x19, lsl #3]     // Store result
        code.AddRange([0xE0, 0x7B, 0x33, 0xF8]);
        
        // add x19, x19, #1              // Increment stack pointer
        code.AddRange([0x73, 0x06, 0x00, 0x91]);
    }

    private void EmitPrintArm64(List<byte> code)
    {
        // For simplicity, this is a no-op that leaves the stack unchanged
        // In a real implementation, you'd call printf or similar
    }

    // X64 instruction emitters (renamed from the original methods)
    private void EmitLoadX64(List<byte> code, long value)
    {
        // mov rax, immediate value
        code.AddRange([0x48, 0xB8]);
        code.AddRange(BitConverter.GetBytes(value));
        
        // mov [rsp + r12*8], rax (push to our stack)
        code.AddRange([0x4A, 0x89, 0x04, 0xE4]);
        
        // inc r12 (increment stack pointer)
        code.AddRange([0x49, 0xFF, 0xC4]);
    }

    private void EmitAddX64(List<byte> code)
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

    private void EmitSubX64(List<byte> code)
    {
        // dec r12 (pop first operand - the subtrahend)
        code.AddRange([0x49, 0xFF, 0xCC]); 
        // mov rax, [rsp + r12*8] (get first operand - subtrahend)
        code.AddRange([0x4A, 0x8B, 0x04, 0xE4]); 
        // dec r12 (pop second operand - the minuend)
        code.AddRange([0x49, 0xFF, 0xCC]); 
        // neg rax (negate the subtrahend)
        code.AddRange([0x48, 0xF7, 0xD8]); 
        // add rax, [rsp + r12*8] (add minuend + (-subtrahend) = minuend - subtrahend)
        code.AddRange([0x4A, 0x03, 0x04, 0xE4]); 
        // mov [rsp + r12*8], rax (push result)
        code.AddRange([0x4A, 0x89, 0x04, 0xE4]); 
        // inc r12 (increment stack pointer)
        code.AddRange([0x49, 0xFF, 0xC4]); 
    }

    private void EmitMulX64(List<byte> code)
    {
        code.AddRange([0x49, 0xFF, 0xCC]); // dec r12
        code.AddRange([0x4A, 0x8B, 0x04, 0xE4]); // mov rax, [rsp + r12*8]
        code.AddRange([0x49, 0xFF, 0xCC]); // dec r12
        code.AddRange([0x4A, 0x0F, 0xAF, 0x04, 0xE4]); // imul rax, [rsp + r12*8]
        code.AddRange([0x4A, 0x89, 0x04, 0xE4]); // mov [rsp + r12*8], rax
        code.AddRange([0x49, 0xFF, 0xC4]); // inc r12
    }

    private void EmitDivX64(List<byte> code)
    {
        // dec r12 (pop first operand - the divisor)
        code.AddRange([0x49, 0xFF, 0xCC]); 
        // mov rcx, [rsp + r12*8] (get divisor into rcx)
        code.AddRange([0x4A, 0x8B, 0x0C, 0xE4]); 
        // dec r12 (pop second operand - the dividend)
        code.AddRange([0x49, 0xFF, 0xCC]); 
        // mov rax, [rsp + r12*8] (get dividend into rax)
        code.AddRange([0x4A, 0x8B, 0x04, 0xE4]); 
        // cqo (sign extend rax to rdx:rax)
        code.AddRange([0x48, 0x99]); 
        // idiv rcx (divide rdx:rax by rcx, quotient in rax)
        code.AddRange([0x48, 0xF7, 0xF9]); 
        // mov [rsp + r12*8], rax (push result)
        code.AddRange([0x4A, 0x89, 0x04, 0xE4]); 
        // inc r12 (increment stack pointer)
        code.AddRange([0x49, 0xFF, 0xC4]); 
    }

    private void EmitPrintX64(List<byte> code)
    {
        // For simplicity, we'll just keep the value on stack without actual printing
        // In a real implementation, you'd call printf or similar
        // For now, this is a no-op that leaves the stack unchanged
    }
}
