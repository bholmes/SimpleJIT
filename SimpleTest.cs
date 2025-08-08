using System;
using System.Collections.Generic;
using SimpleJIT.Core;

class SimpleTest
{
    static void Main()
    {
        Console.WriteLine($"Running on: {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}");
        
        try
        {
            var instructions = new List<Instruction>
            {
                new() { Type = InstructionType.Load, Value = 10 },
                new() { Type = InstructionType.Load, Value = 5 },
                new() { Type = InstructionType.Add },
                new() { Type = InstructionType.Return }
            };

            Console.WriteLine("Compiling JIT code...");
            var compiledFunction = JitCompiler.CompileInstructions(instructions);
            
            Console.WriteLine("JIT compilation successful! Executing...");
            var result = compiledFunction();
            Console.WriteLine($"Result: {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex}");
        }
    }
}
