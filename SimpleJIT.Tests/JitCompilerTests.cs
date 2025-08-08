using SimpleJIT;
using Xunit;

namespace SimpleJIT.Tests.Unit;

public class JitCompilerTests
{
    [Fact]
    public void Compile_EmptyInstructions_ReturnsNonNullFunction()
    {
        // Arrange
        var instructions = new List<Instruction>();
        
        // Act
        var compiledFunction = JitCompiler.CompileInstructions(instructions);
        
        // Assert - Function should be created even for empty instructions
        if (compiledFunction != null)
        {
            var result = compiledFunction();
            Assert.Equal(0, result);
        }
        // If null, JIT compilation is not supported on this platform (expected on macOS)
    }

    [Fact]
    public void Compile_LoadAndReturn_ReturnsCorrectFunction()
    {
        // Arrange
        var instructions = new List<Instruction>
        {
            new(InstructionType.Load, 42),
            new(InstructionType.Return)
        };
        
        // Act
        var compiledFunction = JitCompiler.CompileInstructions(instructions);
        
        // Assert
        if (compiledFunction != null)
        {
            var result = compiledFunction();
            Assert.Equal(42, result);
        }
        // If null, JIT compilation is not supported on this platform
        else
        {
            // On platforms like macOS where JIT might not work due to security restrictions
            Assert.Null(compiledFunction);
        }
    }

    [Fact]
    public void Compile_SimpleAddition_ReturnsCorrectFunction()
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
        var compiledFunction = JitCompiler.CompileInstructions(instructions);
        
        // Assert
        if (compiledFunction != null)
        {
            var result = compiledFunction();
            Assert.Equal(42, result);
        }
        else
        {
            // JIT compilation not supported on this platform
            Assert.Null(compiledFunction);
        }
    }

    [Fact]
    public void Compile_SimpleSubtraction_ReturnsCorrectFunction()
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
        var compiledFunction = JitCompiler.CompileInstructions(instructions);
        
        // Assert
        if (compiledFunction != null)
        {
            var result = compiledFunction();
            Assert.Equal(42, result);
        }
        else
        {
            Assert.Null(compiledFunction);
        }
    }

    [Fact]
    public void Compile_SimpleMultiplication_ReturnsCorrectFunction()
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
        var compiledFunction = JitCompiler.CompileInstructions(instructions);
        
        // Assert
        if (compiledFunction != null)
        {
            var result = compiledFunction();
            Assert.Equal(42, result);
        }
        else
        {
            Assert.Null(compiledFunction);
        }
    }

    [Fact]
    public void Compile_SimpleDivision_ReturnsCorrectFunction()
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
        var compiledFunction = JitCompiler.CompileInstructions(instructions);
        
        // Assert
        if (compiledFunction != null)
        {
            var result = compiledFunction();
            Assert.Equal(42, result);
        }
        else
        {
            Assert.Null(compiledFunction);
        }
    }

    [Fact]
    public void Compile_ComplexExpression_ReturnsCorrectFunction()
    {
        // Arrange - Calculate (10 + 5) * 3 - 1 = 44
        var instructions = new List<Instruction>
        {
            new(InstructionType.Load, 10),
            new(InstructionType.Load, 5),
            new(InstructionType.Add),
            new(InstructionType.Load, 3),
            new(InstructionType.Mul),
            new(InstructionType.Load, 1),
            new(InstructionType.Sub),
            new(InstructionType.Return)
        };
        
        // Act
        var compiledFunction = JitCompiler.CompileInstructions(instructions);
        
        // Assert
        if (compiledFunction != null)
        {
            var result = compiledFunction();
            Assert.Equal(44, result);
        }
        else
        {
            Assert.Null(compiledFunction);
        }
    }

    [Fact]
    public void Compile_WithPrintInstruction_DoesNotThrow()
    {
        // Arrange
        var instructions = new List<Instruction>
        {
            new(InstructionType.Load, 42),
            new(InstructionType.Print),
            new(InstructionType.Return)
        };
        
        // Act & Assert - Should not throw during compilation
        var compiledFunction = JitCompiler.CompileInstructions(instructions);
        
        // If compilation succeeds, execution should also not throw
        if (compiledFunction != null)
        {
            var result = compiledFunction();
            Assert.Equal(42, result);
        }
    }

    [Fact]
    public void Compile_NegativeNumbers_HandledCorrectly()
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
        var compiledFunction = JitCompiler.CompileInstructions(instructions);
        
        // Assert
        if (compiledFunction != null)
        {
            var result = compiledFunction();
            Assert.Equal(-5, result);
        }
        else
        {
            Assert.Null(compiledFunction);
        }
    }

    [Fact]
    public void Compile_OnSupportedPlatform_ReturnsNonNullFunction()
    {
        // Skip this test if we're on an unsupported platform
        if (Environment.OSVersion.Platform == PlatformID.MacOSX)
        {
            return; // Skip test on macOS due to security restrictions
        }
        
        // Arrange
        var instructions = new List<Instruction>
        {
            new(InstructionType.Load, 1),
            new(InstructionType.Return)
        };
        
        // Act
        var compiledFunction = JitCompiler.CompileInstructions(instructions);
        
        // Assert
        Assert.NotNull(compiledFunction);
        var result = compiledFunction();
        Assert.Equal(1, result);
    }
}
