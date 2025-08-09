using SimpleJIT.Core;
using Xunit;

namespace SimpleJIT.Tests.Unit;

public class InstructionTests
{
    [Fact]
    public void Instruction_Constructor_SetsTypeCorrectly()
    {
        // Arrange & Act
        var instruction = new Instruction(InstructionType.Add);
        
        // Assert
        Assert.Equal(InstructionType.Add, instruction.Type);
        Assert.Equal(0, instruction.Value);
    }

    [Fact]
    public void Instruction_ConstructorWithValue_SetsTypeAndValueCorrectly()
    {
        // Arrange & Act
        var instruction = new Instruction(InstructionType.Load, 42);
        
        // Assert
        Assert.Equal(InstructionType.Load, instruction.Type);
        Assert.Equal(42, instruction.Value);
    }

    [Theory]
    [InlineData(InstructionType.Load, 10, "Load 10")]
    [InlineData(InstructionType.Load, -5, "Load -5")]
    [InlineData(InstructionType.Add, 0, "Add")]
    [InlineData(InstructionType.Sub, 0, "Sub")]
    [InlineData(InstructionType.Mul, 0, "Mul")]
    [InlineData(InstructionType.Div, 0, "Div")]
    [InlineData(InstructionType.Print, 0, "Print")]
    [InlineData(InstructionType.Return, 0, "Return")]
    public void Instruction_ToString_ReturnsExpectedFormat(InstructionType type, int value, string expected)
    {
        // Arrange
        var instruction = new Instruction(type, value);
        
        // Act
        var result = instruction.ToString();
        
        // Assert
        Assert.Equal(expected, result);
    }
}
