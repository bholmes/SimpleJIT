using System;
using System.IO;
using SimpleJIT.Core;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: SimpleJIT <instruction_file>");
            Console.WriteLine();
            Console.WriteLine("Example instruction file content:");
            Console.WriteLine("load 10");
            Console.WriteLine("load 5");
            Console.WriteLine("add");
            Console.WriteLine("print");
            Console.WriteLine("ret");
            return;
        }

        var instructionFile = args[0];

        if (!File.Exists(instructionFile))
        {
            Console.WriteLine($"Error: File '{instructionFile}' not found.");
            return;
        }

        try
        {
            Console.WriteLine($"Parsing instructions from: {instructionFile}");
            var instructions = Parser.ParseFile(instructionFile);

            Console.WriteLine("Parsed instructions:");
            foreach (var instruction in instructions)
            {
                Console.WriteLine($"  {instruction}");
            }

            // First, execute using the virtual machine interpreter
            Console.WriteLine("\n=== Executing with Virtual Machine Interpreter ===");
            var vm = new VirtualMachine();
            var vmResult = vm.Execute(instructions);
            Console.WriteLine($"VM Execution completed. Result: {vmResult}");

            // Then try JIT compilation (may fail on some platforms due to security restrictions)
            Console.WriteLine("\n=== Attempting JIT Compilation ===");
            try
            {
                var compiledFunction = JitCompiler.CompileInstructions(instructions);
                Console.WriteLine("JIT compilation successful!");

                Console.WriteLine("Executing JIT compiled code...");
                var jitResult = compiledFunction();
                Console.WriteLine($"JIT Execution completed. Result: {jitResult}");

                if (vmResult == jitResult)
                {
                    Console.WriteLine("✓ VM and JIT results match!");
                }
                else
                {
                    Console.WriteLine($"⚠ Results differ: VM={vmResult}, JIT={jitResult}");
                }
            }
            catch (Exception jitEx)
            {
                Console.WriteLine($"JIT compilation failed: {jitEx.Message}");
                Console.WriteLine("This is expected on some platforms (like macOS) due to security restrictions.");
                Console.WriteLine("The Virtual Machine interpreter provides the same functionality safely.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
        }
    }
}
