using SimpleJIT;
using Xunit;

namespace SimpleJIT.Tests.Unit;

public class VirtualMachineTests
{
    private readonly VirtualMachine _vm = new();

    [Fact]
    public void Execute_EmptyInstructions_ReturnsZero()
    {
        // Arrange
        var instructions = new List<Instruction>();
        
        // Act
        var result = _vm.Execute(instructions);
        
        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Execute_LoadAndReturn_ReturnsLoadedValue()
    {
        // Arrange
        var instructions = new List<Instruction>
        {
            new(InstructionType.Load, 42),
            new(InstructionType.Return)
        };
        
        // Act
        var result = _vm.Execute(instructions);
        
        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void Execute_Addition_ReturnsCorrectSum()
    {
        // Arrange
        var instructions = new List<Instruction>
        {
            new(InstructionType.Load, 10),
            new(InstructionType.Load, 32),
            new(InstructionType.Add),
            new(InstructionType.Return)
        };
        
        // Act
        var result = _vm.Execute(instructions);
        
        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void Execute_Subtraction_ReturnsCorrectDifference()
    {
        // Arrange
        var instructions = new List<Instruction>
        {
            new(InstructionType.Load, 50),
            new(InstructionType.Load, 8),
            new(InstructionType.Sub),
            new(InstructionType.Return)
        };
        
        // Act
        var result = _vm.Execute(instructions);
        
        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void Execute_Multiplication_ReturnsCorrectProduct()
    {
        // Arrange
        var instructions = new List<Instruction>
        {
            new(InstructionType.Load, 6),
            new(InstructionType.Load, 7),
            new(InstructionType.Mul),
            new(InstructionType.Return)
        };
        
        // Act
        var result = _vm.Execute(instructions);
        
        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void Execute_Division_ReturnsCorrectQuotient()
    {
        // Arrange
        var instructions = new List<Instruction>
        {
            new(InstructionType.Load, 84),
            new(InstructionType.Load, 2),
            new(InstructionType.Div),
            new(InstructionType.Return)
        };
        
        // Act
        var result = _vm.Execute(instructions);
        
        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void Execute_DivisionByZero_ThrowsDivideByZeroException()
    {
        // Arrange
        var instructions = new List<Instruction>
        {
            new(InstructionType.Load, 42),
            new(InstructionType.Load, 0),
            new(InstructionType.Div)
        };
        
        // Act & Assert
        Assert.Throws<DivideByZeroException>(() => _vm.Execute(instructions));
    }

    [Fact]
    public void Execute_AddWithInsufficientStack_ThrowsInvalidOperationException()
    {
        // Arrange
        var instructions = new List<Instruction>
        {
            new(InstructionType.Load, 42),
            new(InstructionType.Add) // Only one value on stack, need two
        };
        
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _vm.Execute(instructions));
        Assert.Contains("Add requires two operands on stack", exception.Message);
    }

    [Fact]
    public void Execute_SubWithInsufficientStack_ThrowsInvalidOperationException()
    {
        // Arrange
        var instructions = new List<Instruction>
        {
            new(InstructionType.Sub) // No values on stack
        };
        
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _vm.Execute(instructions));
        Assert.Contains("Sub requires two operands on stack", exception.Message);
    }

    [Fact]
    public void Execute_ComplexExpression_ReturnsCorrectResult()
    {
        // Arrange - Calculate (10 + 5) * 3 - 1 = 44
        var instructions = new List<Instruction>
        {
            new(InstructionType.Load, 10),
            new(InstructionType.Load, 5),
            new(InstructionType.Add),      // Stack: [15]
            new(InstructionType.Load, 3),
            new(InstructionType.Mul),      // Stack: [45]
            new(InstructionType.Load, 1),
            new(InstructionType.Sub),      // Stack: [44]
            new(InstructionType.Return)
        };
        
        // Act
        var result = _vm.Execute(instructions);
        
        // Assert
        Assert.Equal(44, result);
    }

    [Fact]
    public void Execute_PrintInstruction_DoesNotCrash()
    {
        // Arrange
        var instructions = new List<Instruction>
        {
            new(InstructionType.Load, 42),
            new(InstructionType.Print),
            new(InstructionType.Return)
        };
        
        // Act & Assert - Should not throw
        var result = _vm.Execute(instructions);
        Assert.Equal(42, result);
    }

    [Fact]
    public void Execute_PrintWithEmptyStack_ThrowsInvalidOperationException()
    {
        // Arrange
        var instructions = new List<Instruction>
        {
            new(InstructionType.Print) // No value to print
        };
        
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _vm.Execute(instructions));
        Assert.Contains("Print requires one operand on stack", exception.Message);
    }

    [Fact]
    public void Execute_ReturnWithEmptyStack_ReturnsZero()
    {
        // Arrange
        var instructions = new List<Instruction>
        {
            new(InstructionType.Return)
        };
        
        // Act
        var result = _vm.Execute(instructions);
        
        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Execute_NegativeNumbers_HandledCorrectly()
    {
        // Arrange
        var instructions = new List<Instruction>
        {
            new(InstructionType.Load, -10),
            new(InstructionType.Load, 5),
            new(InstructionType.Add),
            new(InstructionType.Return)
        };
        
        // Act
        var result = _vm.Execute(instructions);
        
        // Assert
        Assert.Equal(-5, result);
    }
}
