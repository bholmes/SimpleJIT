namespace SimpleJIT.Core;

public enum InstructionType
{
    Load,
    Add,
    Sub,
    Mul,
    Div,
    Print,
    Return
}

public class Instruction
{
    public InstructionType Type { get; set; }
    public int Value { get; set; }

    public Instruction(InstructionType type, int value = 0)
    {
        Type = type;
        Value = value;
    }

    public override string ToString()
    {
        return Type == InstructionType.Load ? $"{Type} {Value}" : Type.ToString();
    }
}
