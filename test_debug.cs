using SimpleJIT.Core;

// Test empty instructions
var instructions = new List<Instruction>();
Console.WriteLine("Testing empty instructions...");

try 
{
    var compiledFunction = JitCompiler.CompileInstructions(instructions);
    if (compiledFunction != null)
    {
        Console.WriteLine("JIT compilation succeeded");
        var result = compiledFunction();
        Console.WriteLine($"Result: {result}");
    }
    else 
    {
        Console.WriteLine("JIT compilation returned null");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Exception: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}
