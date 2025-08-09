using Xunit;
using System.IO;
using SimpleJIT.Core;

namespace SimpleJIT.Tests.Unit
{
    public class FunctionVirtualMachineTests
    {
        [Fact]
        public void ExecuteProgram_SimpleFunctionCall_ReturnsCorrectResult()
        {
            // Arrange
            var program = new Program();
            
            // Main function: load 10, load 5, call Add, ret
            var mainFunc = new Function("Main", "int");
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 10));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 5));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Call, "Add"));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(mainFunc);
            
            // Add function: loadarg 0, loadarg 1, add, ret
            var addFunc = new Function("Add", "int");
            addFunc.ParameterTypes.Add("int");
            addFunc.ParameterTypes.Add("int");
            addFunc.Instructions.Add(new Instruction(InstructionType.LoadArg, 0));
            addFunc.Instructions.Add(new Instruction(InstructionType.LoadArg, 1));
            addFunc.Instructions.Add(new Instruction(InstructionType.Add));
            addFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(addFunc);
            
            var vm = new VirtualMachine();
            
            // Act
            var result = vm.ExecuteProgram(program);
            
            // Assert
            Assert.Equal(15, result); // 10 + 5 = 15
        }

        [Fact]
        public void ExecuteProgram_YourFunctionsExample_ReturnsCorrectResult()
        {
            // Arrange - Create program matching your functions.txt
            var program = new Program();
            
            // Main function
            var mainFunc = new Function("Main", "int");
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 10));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 5));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Call, "Step1"));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 2));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Mul));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Print));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(mainFunc);
            
            // Step1 function
            var step1Func = new Function("Step1", "int");
            step1Func.ParameterTypes.Add("int");
            step1Func.ParameterTypes.Add("int");
            step1Func.Instructions.Add(new Instruction(InstructionType.LoadArg, 0));
            step1Func.Instructions.Add(new Instruction(InstructionType.LoadArg, 1));
            step1Func.Instructions.Add(new Instruction(InstructionType.Add));
            step1Func.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(step1Func);
            
            var vm = new VirtualMachine();
            
            // Act
            var result = vm.ExecuteProgram(program);
            
            // Assert
            Assert.Equal(30, result); // (10 + 5) * 2 = 30
        }

        [Fact]
        public void ExecuteProgram_NestedFunctionCalls_ReturnsCorrectResult()
        {
            // Arrange - Test nested function calls
            var program = new Program();
            
            // Main: load 6, load 4, call Multiply, ret
            var mainFunc = new Function("Main", "int");
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 6));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 4));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Call, "Multiply"));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(mainFunc);
            
            // Multiply: loadarg 0, loadarg 1, call Add, loadarg 0, mul, ret
            var multiplyFunc = new Function("Multiply", "int");
            multiplyFunc.ParameterTypes.Add("int");
            multiplyFunc.ParameterTypes.Add("int");
            multiplyFunc.Instructions.Add(new Instruction(InstructionType.LoadArg, 0));
            multiplyFunc.Instructions.Add(new Instruction(InstructionType.LoadArg, 1));
            multiplyFunc.Instructions.Add(new Instruction(InstructionType.Call, "Add"));
            multiplyFunc.Instructions.Add(new Instruction(InstructionType.LoadArg, 0));
            multiplyFunc.Instructions.Add(new Instruction(InstructionType.Mul));
            multiplyFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(multiplyFunc);
            
            // Add: loadarg 0, loadarg 1, add, ret
            var addFunc = new Function("Add", "int");
            addFunc.ParameterTypes.Add("int");
            addFunc.ParameterTypes.Add("int");
            addFunc.Instructions.Add(new Instruction(InstructionType.LoadArg, 0));
            addFunc.Instructions.Add(new Instruction(InstructionType.LoadArg, 1));
            addFunc.Instructions.Add(new Instruction(InstructionType.Add));
            addFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(addFunc);
            
            var vm = new VirtualMachine();
            
            // Act
            var result = vm.ExecuteProgram(program);
            
            // Assert
            Assert.Equal(60, result); // (6 + 4) * 6 = 60
        }

        [Fact]
        public void ExecuteProgram_NoMainFunction_ThrowsException()
        {
            // Arrange
            var program = new Program();
            var addFunc = new Function("Add", "int");
            program.Functions.Add(addFunc);
            
            var vm = new VirtualMachine();
            
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => vm.ExecuteProgram(program));
        }

        [Fact]
        public void ExecuteProgram_FunctionNotFound_ThrowsException()
        {
            // Arrange
            var program = new Program();
            var mainFunc = new Function("Main", "int");
            mainFunc.Instructions.Add(new Instruction(InstructionType.Call, "NonExistent"));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(mainFunc);
            
            var vm = new VirtualMachine();
            
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => vm.ExecuteProgram(program));
        }

        [Fact]
        public void ExecuteProgram_InsufficientArguments_ThrowsException()
        {
            // Arrange
            var program = new Program();
            
            // Main function: load 10, call Add (missing second argument), ret
            var mainFunc = new Function("Main", "int");
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 10));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Call, "Add"));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(mainFunc);
            
            // Add function expects 2 arguments
            var addFunc = new Function("Add", "int");
            addFunc.ParameterTypes.Add("int");
            addFunc.ParameterTypes.Add("int");
            addFunc.Instructions.Add(new Instruction(InstructionType.LoadArg, 0));
            addFunc.Instructions.Add(new Instruction(InstructionType.LoadArg, 1));
            addFunc.Instructions.Add(new Instruction(InstructionType.Add));
            addFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(addFunc);
            
            var vm = new VirtualMachine();
            
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => vm.ExecuteProgram(program));
        }
    }
}
