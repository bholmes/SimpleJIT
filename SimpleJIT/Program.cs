using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SimpleJIT.Core;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: SimpleJIT <instruction_file>");
            Console.WriteLine();
            Console.WriteLine("Supports two formats:");
            Console.WriteLine();
            Console.WriteLine("1. Flat instructions (legacy):");
            Console.WriteLine("   load 10");
            Console.WriteLine("   load 5");
            Console.WriteLine("   add");
            Console.WriteLine("   print");
            Console.WriteLine("   ret");
            Console.WriteLine();
            Console.WriteLine("2. Function-based (new):");
            Console.WriteLine("   int Main() {");
            Console.WriteLine("       load 10");
            Console.WriteLine("       load 5");
            Console.WriteLine("       call Add");
            Console.WriteLine("       ret");
            Console.WriteLine("   }");
            Console.WriteLine("   int Add(int, int) {");
            Console.WriteLine("       loadarg 0");
            Console.WriteLine("       loadarg 1");
            Console.WriteLine("       add");
            Console.WriteLine("       ret");
            Console.WriteLine("   }");
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
            
            // Detect format by checking if file contains function definitions
            var fileContent = File.ReadAllText(instructionFile);
            // Use regex to detect function signature like 'int Main(' or 'int FunctionName('
            bool isFunctionFormat = Regex.IsMatch(fileContent, @"^\s*int\s+\w+\s*\(", RegexOptions.Multiline);
            
            if (isFunctionFormat)
            {
                Console.WriteLine("Detected function-based format");
                ExecuteFunctionProgram(instructionFile);
            }
            else
            {
                Console.WriteLine("Detected flat instruction format");
                ExecuteFlatInstructions(instructionFile);
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

    static void ExecuteFunctionProgram(string filePath)
    {
        var program = FunctionParser.ParseProgram(filePath);

        Console.WriteLine("Parsed functions:");
        foreach (var function in program.Functions)
        {
            Console.WriteLine($"  {function.ReturnType} {function.Name}({string.Join(", ", function.ParameterTypes)})");
            Console.WriteLine($"    Instructions: {function.Instructions.Count}");
            foreach (var instruction in function.Instructions)
            {
                Console.WriteLine($"      {instruction}");
            }
        }

        // Execute using the virtual machine
        Console.WriteLine("\n=== Executing with Virtual Machine ===");
        var vm = new VirtualMachine();
        var vmResult = vm.ExecuteProgram(program);
        Console.WriteLine($"VM Execution completed. Result: {vmResult}");

        // Try JIT compilation for function programs
        Console.WriteLine("\n=== Attempting JIT Compilation ===");
        try
        {
            var compiledFunction = JitCompiler.CompileProgram(program);
            if (compiledFunction != null)
            {
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
                    Console.WriteLine("Note: JIT function support is currently basic - full implementation coming soon!");
                }
            }
            else
            {
                Console.WriteLine("JIT compilation failed - using VM result only");
            }
        }
        catch (Exception jitEx)
        {
            Console.WriteLine($"JIT compilation failed: {jitEx.Message}");
            Console.WriteLine("This is expected on some platforms due to security restrictions.");
            Console.WriteLine("The Virtual Machine interpreter provides the same functionality safely.");
        }
    }

    static void ExecuteFlatInstructions(string filePath)
    {
        var instructions = Parser.ParseFile(filePath);

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
            if (compiledFunction != null)
            {
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
            else
            {
                Console.WriteLine("JIT compilation failed - using VM result only");
            }
        }
        catch (Exception jitEx)
        {
            Console.WriteLine($"JIT compilation failed: {jitEx.Message}");
            Console.WriteLine("This is expected on some platforms (like macOS) due to security restrictions.");
            Console.WriteLine("The Virtual Machine interpreter provides the same functionality safely.");
        }
    }
}
