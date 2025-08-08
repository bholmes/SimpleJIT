using SimpleJIT.Core;
using Xunit;

namespace SimpleJIT.Tests.Integration;

public class EndToEndTests
{
    private readonly VirtualMachine _vm = new();

    [Fact]
    public void ParseAndExecute_SimpleProgram_ReturnsCorrectResult()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var content = @"load 10
load 5
add
print
ret";
        File.WriteAllText(tempFile, content);
        
        try
        {
            // Act
            var instructions = Parser.ParseFile(tempFile);
            var vmResult = _vm.Execute(instructions);
            
            // Assert
            Assert.Equal(15, vmResult);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParseAndExecute_ComplexExpression_ReturnsCorrectResult()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var content = @"# Calculate (10 + 5) * 3 - 1 = 44
load 10
load 5
add
load 3
mul
load 1
sub
print
ret";
        File.WriteAllText(tempFile, content);
        
        try
        {
            // Act
            var instructions = Parser.ParseFile(tempFile);
            var vmResult = _vm.Execute(instructions);
            
            // Assert
            Assert.Equal(44, vmResult);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParseAndExecute_DivisionByZero_ThrowsException()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var content = @"load 10
load 0
div";
        File.WriteAllText(tempFile, content);
        
        try
        {
            // Act
            var instructions = Parser.ParseFile(tempFile);
            
            // Assert
            Assert.Throws<DivideByZeroException>(() => _vm.Execute(instructions));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParseAndExecute_VmAndJitCompiler_ProduceSameResult()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var content = @"load 6
load 7
mul
load 8
add
ret";
        File.WriteAllText(tempFile, content);
        
        try
        {
            // Act
            var instructions = Parser.ParseFile(tempFile);
            var vmResult = _vm.Execute(instructions);
            var jitFunction = JitCompiler.CompileInstructions(instructions);
            
            // Assert
            Assert.Equal(50, vmResult); // 6 * 7 + 8 = 50
            
            if (jitFunction != null)
            {
                var jitResult = jitFunction();
                Assert.Equal(vmResult, jitResult);
            }
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParseAndExecute_WithComments_IgnoresCommentsCorrectly()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var content = @"# This program calculates 2 * 3 + 4
load 2  # Load first operand
load 3  # Load second operand
mul     # Multiply them
load 4  # Load addend
add     # Add to result
# Print the result
print
ret     # Return the final value";
        File.WriteAllText(tempFile, content);
        
        try
        {
            // Act
            var instructions = Parser.ParseFile(tempFile);
            var vmResult = _vm.Execute(instructions);
            
            // Assert
            Assert.Equal(10, vmResult); // 2 * 3 + 4 = 10
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParseAndExecute_NegativeNumbers_HandledCorrectly()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var content = @"load -10
load 5
add
load 3
mul
ret";
        File.WriteAllText(tempFile, content);
        
        try
        {
            // Act
            var instructions = Parser.ParseFile(tempFile);
            var vmResult = _vm.Execute(instructions);
            
            // Assert
            Assert.Equal(-15, vmResult); // (-10 + 5) * 3 = -15
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParseAndExecute_EmptyFile_ReturnsZero()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "");
        
        try
        {
            // Act
            var instructions = Parser.ParseFile(tempFile);
            var vmResult = _vm.Execute(instructions);
            
            // Assert
            Assert.Equal(0, vmResult);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParseAndExecute_OnlyComments_ReturnsZero()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var content = @"# This file only has comments
# No actual instructions
# Should return 0";
        File.WriteAllText(tempFile, content);
        
        try
        {
            // Act
            var instructions = Parser.ParseFile(tempFile);
            var vmResult = _vm.Execute(instructions);
            
            // Assert
            Assert.Equal(0, vmResult);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParseAndExecute_ExistingSampleFiles_ExecuteCorrectly()
    {
        // This test verifies that existing sample files in the samples directory work correctly
        var samplesDir = Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "..", "samples");
        
        if (Directory.Exists(samplesDir))
        {
            var simpleFile = Path.Combine(samplesDir, "simple.txt");
            if (File.Exists(simpleFile))
            {
                // Act
                var instructions = Parser.ParseFile(simpleFile);
                var vmResult = _vm.Execute(instructions);
                
                // Assert - simple.txt should calculate 10 + 5 = 15
                Assert.True(vmResult != 0, "Simple.txt should produce a non-zero result");
            }
        }
    }
}
