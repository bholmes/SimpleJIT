using System.Collections.Generic;
using System.Linq;

namespace SimpleJIT.Core;

public class Function
{
    public string Name { get; set; }
    public string ReturnType { get; set; }
    public List<string> ParameterTypes { get; set; }
    public List<Instruction> Instructions { get; set; }

    public Function(string name, string returnType)
    {
        Name = name;
        ReturnType = returnType;
        ParameterTypes = new List<string>();
        Instructions = new List<Instruction>();
    }
}

public class Program
{
    public List<Function> Functions { get; set; }

    public Program()
    {
        Functions = new List<Function>();
    }

    public Function? GetFunction(string name)
    {
        return Functions.FirstOrDefault(f => f.Name == name);
    }

    public Function? GetMainFunction()
    {
        return GetFunction("Main");
    }
}
