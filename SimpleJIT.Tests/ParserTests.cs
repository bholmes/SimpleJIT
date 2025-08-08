using SimpleJIT;
using Xunit;

namespace SimpleJIT.Tests.Unit;

public class ParserTests
{

    [Fact]
    public void ParseFile_NonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentFile = "non_existent_file.txt";
        
        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => Parser.ParseFile(nonExistentFile));
    }

    [Fact]
    public void ParseFile_ValidInstructions_ReturnsCorrectInstructions()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var content = @"load 10
add
load 5
mul
print
ret";
        File.WriteAllText(tempFile, content);
        
        try
        {
            // Act
            var instructions = Parser.ParseFile(tempFile);
            
            // Assert
            Assert.Equal(6, instructions.Count);
            Assert.Equal(InstructionType.Load, instructions[0].Type);
            Assert.Equal(10, instructions[0].Value);
            Assert.Equal(InstructionType.Add, instructions[1].Type);
            Assert.Equal(InstructionType.Load, instructions[2].Type);
            Assert.Equal(5, instructions[2].Value);
            Assert.Equal(InstructionType.Mul, instructions[3].Type);
            Assert.Equal(InstructionType.Print, instructions[4].Type);
            Assert.Equal(InstructionType.Return, instructions[5].Type);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParseFile_InstructionsWithComments_IgnoresComments()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var content = @"# This is a comment
load 10  # Load value 10
# Another comment
add
# Final comment";
        File.WriteAllText(tempFile, content);
        
        try
        {
            // Act
            var instructions = Parser.ParseFile(tempFile);
            
            // Assert
            Assert.Equal(2, instructions.Count);
            Assert.Equal(InstructionType.Load, instructions[0].Type);
            Assert.Equal(10, instructions[0].Value);
            Assert.Equal(InstructionType.Add, instructions[1].Type);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParseFile_EmptyFile_ReturnsEmptyList()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "");
        
        try
        {
            // Act
            var instructions = Parser.ParseFile(tempFile);
            
            // Assert
            Assert.Empty(instructions);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParseFile_OnlyComments_ReturnsEmptyList()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var content = @"# Only comments
# in this file
# Nothing else";
        File.WriteAllText(tempFile, content);
        
        try
        {
            // Act
            var instructions = Parser.ParseFile(tempFile);
            
            // Assert
            Assert.Empty(instructions);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParseFile_InvalidInstruction_ThrowsArgumentException()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var content = "invalid_instruction 123";
        File.WriteAllText(tempFile, content);
        
        try
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => Parser.ParseFile(tempFile));
            Assert.Contains("Unknown instruction", exception.Message);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParseFile_LoadWithoutValue_ThrowsArgumentException()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var content = "load";
        File.WriteAllText(tempFile, content);
        
        try
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => Parser.ParseFile(tempFile));
            Assert.Contains("Load instruction requires a value", exception.Message);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParseFile_LoadWithInvalidValue_ThrowsArgumentException()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var content = "load not_a_number";
        File.WriteAllText(tempFile, content);
        
        try
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => Parser.ParseFile(tempFile));
            Assert.Contains("Invalid value for load instruction", exception.Message);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParseFile_NegativeValues_ParsesCorrectly()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var content = "load -42";
        File.WriteAllText(tempFile, content);
        
        try
        {
            // Act
            var instructions = Parser.ParseFile(tempFile);
            
            // Assert
            Assert.Single(instructions);
            Assert.Equal(InstructionType.Load, instructions[0].Type);
            Assert.Equal(-42, instructions[0].Value);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
