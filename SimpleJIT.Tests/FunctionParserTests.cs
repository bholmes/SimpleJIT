using Xunit;
using System.IO;
using SimpleJIT.Core;

namespace SimpleJIT.Tests.Unit
{
    public class FunctionParserTests
    {
        [Fact]
        public void ParseProgram_SimpleFunctionDefinition_ParsesCorrectly()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var content = @"int Main ()
{
    load 42
    ret
}";
            File.WriteAllText(tempFile, content);
            
            try
            {
                // Act
                var program = FunctionParser.ParseProgram(tempFile);
                
                // Assert
                Assert.Single(program.Functions);
                Assert.Equal("Main", program.Functions[0].Name);
                Assert.Equal("int", program.Functions[0].ReturnType);
                Assert.Empty(program.Functions[0].ParameterTypes);
                Assert.Equal(2, program.Functions[0].Instructions.Count);
                Assert.Equal(InstructionType.Load, program.Functions[0].Instructions[0].Type);
                Assert.Equal(42, program.Functions[0].Instructions[0].Value);
                Assert.Equal(InstructionType.Return, program.Functions[0].Instructions[1].Type);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
        
        [Fact]
        public void ParseProgram_FunctionWithParameters_ParsesCorrectly()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var content = @"int Add(int, int)
{
    loadarg 0
    loadarg 1
    add
    ret
}";
            File.WriteAllText(tempFile, content);
            
            try
            {
                // Act
                var program = FunctionParser.ParseProgram(tempFile);
                
                // Assert
                Assert.Single(program.Functions);
                var function = program.Functions[0];
                Assert.Equal("Add", function.Name);
                Assert.Equal("int", function.ReturnType);
                Assert.Equal(2, function.ParameterTypes.Count);
                Assert.Equal("int", function.ParameterTypes[0]);
                Assert.Equal("int", function.ParameterTypes[1]);
                Assert.Equal(4, function.Instructions.Count);
                Assert.Equal(InstructionType.LoadArg, function.Instructions[0].Type);
                Assert.Equal(0, function.Instructions[0].Value);
                Assert.Equal(InstructionType.LoadArg, function.Instructions[1].Type);
                Assert.Equal(1, function.Instructions[1].Value);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
        
        [Fact]
        public void ParseProgram_FunctionWithCall_ParsesCorrectly()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var content = @"int Main()
{
    load 10
    load 5
    call Add
    ret
}";
            File.WriteAllText(tempFile, content);
            
            try
            {
                // Act
                var program = FunctionParser.ParseProgram(tempFile);
                
                // Assert
                Assert.Single(program.Functions);
                var function = program.Functions[0];
                Assert.Equal(4, function.Instructions.Count);
                Assert.Equal(InstructionType.Call, function.Instructions[2].Type);
                Assert.Equal("Add", function.Instructions[2].FunctionName);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void ParseProgram_YourFunctionsExample_ParsesCorrectly()
        {
            // Arrange - Test with your actual functions.txt example
            var tempFile = Path.GetTempFileName();
            var content = @"int Main ()
{
    // Simple arithmetic: (10 + 5) * 2 = 30
    load 10
    load 5
    call Step1
    load 2
    mul
    print
    ret
}

int Step1(int, int)
{
    loadarg 0
    loadarg 1
    add
    ret
}";
            File.WriteAllText(tempFile, content);
            
            try
            {
                // Act
                var program = FunctionParser.ParseProgram(tempFile);
                
                // Assert
                Assert.Equal(2, program.Functions.Count);
                
                // Check Main function
                var mainFunc = program.GetMainFunction();
                Assert.NotNull(mainFunc);
                Assert.Equal("Main", mainFunc.Name);
                Assert.Equal("int", mainFunc.ReturnType);
                Assert.Empty(mainFunc.ParameterTypes);
                Assert.Equal(7, mainFunc.Instructions.Count); // load, load, call, load, mul, print, ret
                
                // Check Step1 function
                var step1Func = program.GetFunction("Step1");
                Assert.NotNull(step1Func);
                Assert.Equal("Step1", step1Func.Name);
                Assert.Equal("int", step1Func.ReturnType);
                Assert.Equal(2, step1Func.ParameterTypes.Count);
                Assert.Equal(4, step1Func.Instructions.Count); // loadarg, loadarg, add, ret
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
