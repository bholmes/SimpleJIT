using System;
using System.Collections.Generic;
using SimpleJIT.Core;

class TestArm64JIT
{
    static void Main()
    {
        Console.WriteLine($"Current Architecture: {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}");
        
        // Test simple arithmetic: 10 + 5 = 15
        var instructions = new List<Instruction>
        {
            new Instruction { Type = InstructionType.Load, Value = 10 },
            new Instruction { Type = InstructionType.Load, Value = 5 },
            new Instruction { Type = InstructionType.Add },
            new Instruction { Type = InstructionType.Return }
        };

        try
        {
            var compiledFunction = JitCompiler.CompileInstructions(instructions);
            var result = compiledFunction();
            Console.WriteLine($"JIT compilation and execution successful! Result: {result}");
            Console.WriteLine("Expected: 15");
            Console.WriteLine(result == 15 ? "✅ Test PASSED" : "❌ Test FAILED");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JIT compilation failed: {ex}");
        }
    }
}
