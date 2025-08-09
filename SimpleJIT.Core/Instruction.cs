namespace SimpleJIT.Core;

public enum InstructionType
{
    Load,
    Add,
    Sub,
    Mul,
    Div,
    Print,
    Return,
    Call,
    LoadArg
}

public class Instruction
{
    public InstructionType Type { get; set; }
    public int Value { get; set; }
    public string? FunctionName { get; set; }

    public Instruction(InstructionType type, int value = 0)
    {
        Type = type;
        Value = value;
    }

    public Instruction(InstructionType type, string functionName)
    {
        Type = type;
        FunctionName = functionName;
    }

    public override string ToString()
    {
        return Type switch
        {
            InstructionType.Load => $"{Type} {Value}",
            InstructionType.LoadArg => $"{Type} {Value}",
            InstructionType.Call => $"{Type} {FunctionName}",
            _ => Type.ToString()
        };
    }
}
