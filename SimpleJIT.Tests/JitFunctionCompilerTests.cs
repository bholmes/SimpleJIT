using Xunit;
using SimpleJIT.Core;

namespace SimpleJIT.Tests.Unit
{
    public class JitFunctionCompilerTests
    {
        [Fact]
        public void CompileProgram_SimpleFunctionProgram_ReturnsNonNullFunction()
        {
            // Arrange
            var program = new Program();
            var mainFunc = new Function("Main", "int");
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 42));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(mainFunc);
            
            // Act
            var result = JitCompiler.CompileProgram(program);
            
            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void CompileProgram_FunctionWithCall_CompilesSuccessfully()
        {
            // Arrange
            var program = new Program();
            
            // Main function with call
            var mainFunc = new Function("Main", "int");
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 10));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 5));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Call, "Add"));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(mainFunc);
            
            // Add function
            var addFunc = new Function("Add", "int");
            addFunc.ParameterTypes.Add("int");
            addFunc.ParameterTypes.Add("int");
            addFunc.Instructions.Add(new Instruction(InstructionType.LoadArg, 0));
            addFunc.Instructions.Add(new Instruction(InstructionType.LoadArg, 1));
            addFunc.Instructions.Add(new Instruction(InstructionType.Add));
            addFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(addFunc);
            
            // Act
            var result = JitCompiler.CompileProgram(program);
            
            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void CompileProgram_FunctionWithLoadArg_CompilesSuccessfully()
        {
            // Arrange
            var program = new Program();
            
            // Add Main function
            var mainFunc = new Function("Main", "int");
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 42));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(mainFunc);
            
            // Add test function with LoadArg
            var testFunc = new Function("Test", "int");
            testFunc.ParameterTypes.Add("int");
            testFunc.Instructions.Add(new Instruction(InstructionType.LoadArg, 0));
            testFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(testFunc);
            
            // Act
            var result = JitCompiler.CompileProgram(program);
            
            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void CompileProgram_NoMainFunction_ThrowsException()
        {
            // Arrange
            var program = new Program();
            var func = new Function("NotMain", "int");
            func.Instructions.Add(new Instruction(InstructionType.Load, 42));
            func.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(func);
            
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => JitCompiler.CompileProgram(program));
        }

        [Fact]
        public void CompileProgram_EmptyProgram_ThrowsException()
        {
            // Arrange
            var program = new Program();
            
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => JitCompiler.CompileProgram(program));
        }
    }
}
