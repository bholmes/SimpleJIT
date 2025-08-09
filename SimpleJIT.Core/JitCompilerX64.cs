using System;
using System.Collections.Generic;

namespace SimpleJIT.Core;

public unsafe class JitCompilerX64 : JitCompiler
{
    protected override byte[] GenerateCode(List<Instruction> instructions)
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

    private void EmitLoad(List<byte> code, int value)
    {
        // mov rax, immediate value (32-bit immediate, zero-extended to 64-bit)
        code.AddRange([0x48, 0xC7, 0xC0]);
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

    private void EmitPrint(List<byte> code)
    {
        // For simplicity, we'll just keep the value on stack without actual printing
        // In a real implementation, you'd call printf or similar
        // For now, this is a no-op that leaves the stack unchanged
    }
}
