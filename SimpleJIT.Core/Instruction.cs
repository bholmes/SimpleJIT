namespace SimpleJIT;

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
    public long Value { get; set; }

    public Instruction(InstructionType type, long value = 0)
    {
        Type = type;
        Value = value;
    }

    public override string ToString()
    {
        return Type == InstructionType.Load ? $"{Type} {Value}" : Type.ToString();
    }
}
