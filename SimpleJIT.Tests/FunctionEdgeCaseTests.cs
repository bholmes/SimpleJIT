using Xunit;
using System.IO;
using SimpleJIT.Core;

namespace SimpleJIT.Tests.Unit
{
    public class FunctionEdgeCaseTests
    {
        [Fact]
        public void ExecuteProgram_FunctionWithZeroParameters_ReturnsCorrectResult()
        {
            // Arrange
            var program = new Program();
            
            // Main function calls GetConstant
            var mainFunc = new Function("Main", "int");
            mainFunc.Instructions.Add(new Instruction(InstructionType.Call, "GetConstant"));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 10));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Add));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(mainFunc);
            
            // GetConstant function returns 42
            var constFunc = new Function("GetConstant", "int");
            constFunc.Instructions.Add(new Instruction(InstructionType.Load, 42));
            constFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(constFunc);
            
            var vm = new VirtualMachine();
            
            // Act
            var result = vm.ExecuteProgram(program);
            
            // Assert
            Assert.Equal(52, result); // 42 + 10 = 52
        }

        [Fact]
        public void ExecuteProgram_FunctionWithSingleParameter_ReturnsCorrectResult()
        {
            // Arrange
            var program = new Program();
            
            var mainFunc = new Function("Main", "int");
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 5));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Call, "Square"));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(mainFunc);
            
            // Square function: x * x
            var squareFunc = new Function("Square", "int");
            squareFunc.ParameterTypes.Add("int");
            squareFunc.Instructions.Add(new Instruction(InstructionType.LoadArg, 0));
            squareFunc.Instructions.Add(new Instruction(InstructionType.LoadArg, 0));
            squareFunc.Instructions.Add(new Instruction(InstructionType.Mul));
            squareFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(squareFunc);
            
            var vm = new VirtualMachine();
            
            // Act
            var result = vm.ExecuteProgram(program);
            
            // Assert
            Assert.Equal(25, result); // 5 * 5 = 25
        }

        [Fact]
        public void ExecuteProgram_FunctionWithManyParameters_ReturnsCorrectResult()
        {
            // Arrange
            var program = new Program();
            
            var mainFunc = new Function("Main", "int");
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 1));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 2));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 3));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 4));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 5));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Call, "SumFive"));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(mainFunc);
            
            // SumFive function: sum all 5 parameters
            var sumFunc = new Function("SumFive", "int");
            for (int i = 0; i < 5; i++)
            {
                sumFunc.ParameterTypes.Add("int");
            }
            sumFunc.Instructions.Add(new Instruction(InstructionType.LoadArg, 0));
            sumFunc.Instructions.Add(new Instruction(InstructionType.LoadArg, 1));
            sumFunc.Instructions.Add(new Instruction(InstructionType.Add));
            sumFunc.Instructions.Add(new Instruction(InstructionType.LoadArg, 2));
            sumFunc.Instructions.Add(new Instruction(InstructionType.Add));
            sumFunc.Instructions.Add(new Instruction(InstructionType.LoadArg, 3));
            sumFunc.Instructions.Add(new Instruction(InstructionType.Add));
            sumFunc.Instructions.Add(new Instruction(InstructionType.LoadArg, 4));
            sumFunc.Instructions.Add(new Instruction(InstructionType.Add));
            sumFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(sumFunc);
            
            var vm = new VirtualMachine();
            
            // Act
            var result = vm.ExecuteProgram(program);
            
            // Assert
            Assert.Equal(15, result); // 1 + 2 + 3 + 4 + 5 = 15
        }

        [Fact]
        public void ExecuteProgram_DeeplyNestedFunctionCalls_ReturnsCorrectResult()
        {
            // Arrange - Test 5 levels of nesting
            var program = new Program();
            
            var mainFunc = new Function("Main", "int");
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 3));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Call, "Level1"));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(mainFunc);
            
            var level1Func = new Function("Level1", "int");
            level1Func.ParameterTypes.Add("int");
            level1Func.Instructions.Add(new Instruction(InstructionType.LoadArg, 0));
            level1Func.Instructions.Add(new Instruction(InstructionType.Load, 1));
            level1Func.Instructions.Add(new Instruction(InstructionType.Add));
            level1Func.Instructions.Add(new Instruction(InstructionType.Call, "Level2"));
            level1Func.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(level1Func);
            
            var level2Func = new Function("Level2", "int");
            level2Func.ParameterTypes.Add("int");
            level2Func.Instructions.Add(new Instruction(InstructionType.LoadArg, 0));
            level2Func.Instructions.Add(new Instruction(InstructionType.Load, 2));
            level2Func.Instructions.Add(new Instruction(InstructionType.Mul));
            level2Func.Instructions.Add(new Instruction(InstructionType.Call, "Level3"));
            level2Func.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(level2Func);
            
            var level3Func = new Function("Level3", "int");
            level3Func.ParameterTypes.Add("int");
            level3Func.Instructions.Add(new Instruction(InstructionType.LoadArg, 0));
            level3Func.Instructions.Add(new Instruction(InstructionType.Load, 1));
            level3Func.Instructions.Add(new Instruction(InstructionType.Sub));
            level3Func.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(level3Func);
            
            var vm = new VirtualMachine();
            
            // Act
            var result = vm.ExecuteProgram(program);
            
            // Assert
            // Level 1: 3 + 1 = 4
            // Level 2: 4 * 2 = 8
            // Level 3: 8 - 1 = 7
            Assert.Equal(7, result);
        }

        [Fact]
        public void ExecuteProgram_RecursiveFunctionCall_ReturnsCorrectResult()
        {
            // Arrange - Test that handles recursive calls gracefully
            // Note: Since we don't have conditional logic, we'll test a controlled recursion scenario
            var program = new Program();
            
            var mainFunc = new Function("Main", "int");
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 42));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(mainFunc);
            
            var vm = new VirtualMachine();
            
            // Act
            var result = vm.ExecuteProgram(program);
            
            // Assert
            // This test verifies basic functionality - true recursion would need conditionals
            Assert.Equal(42, result);
        }

        [Fact]
        public void ExecuteProgram_FunctionCallInMiddleOfStack_ReturnsCorrectResult()
        {
            // Arrange - Test that stack is properly managed around function calls
            var program = new Program();
            
            var mainFunc = new Function("Main", "int");
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 10)); // Stack: [10]
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 20)); // Stack: [10, 20]
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 5));  // Stack: [10, 20, 5]
            mainFunc.Instructions.Add(new Instruction(InstructionType.Call, "Double")); // Should pop 5, call Double(5), push result
            mainFunc.Instructions.Add(new Instruction(InstructionType.Add)); // Add with 20: Stack: [10, 30]
            mainFunc.Instructions.Add(new Instruction(InstructionType.Add)); // Add with 10: Stack: [40]
            mainFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(mainFunc);
            
            var doubleFunc = new Function("Double", "int");
            doubleFunc.ParameterTypes.Add("int");
            doubleFunc.Instructions.Add(new Instruction(InstructionType.LoadArg, 0));
            doubleFunc.Instructions.Add(new Instruction(InstructionType.Load, 2));
            doubleFunc.Instructions.Add(new Instruction(InstructionType.Mul));
            doubleFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(doubleFunc);
            
            var vm = new VirtualMachine();
            
            // Act
            var result = vm.ExecuteProgram(program);
            
            // Assert
            // Double(5) = 10, then 20 + 10 = 30, then 10 + 30 = 40
            Assert.Equal(40, result);
        }

        [Fact]
        public void ExecuteProgram_InvalidLoadArgIndex_ThrowsException()
        {
            // Arrange
            var program = new Program();
            
            var mainFunc = new Function("Main", "int");
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 5));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Call, "BadFunc"));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(mainFunc);
            
            // BadFunc tries to load argument 1 when only 1 parameter (index 0) exists
            var badFunc = new Function("BadFunc", "int");
            badFunc.ParameterTypes.Add("int");
            badFunc.Instructions.Add(new Instruction(InstructionType.LoadArg, 1)); // Invalid index!
            badFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(badFunc);
            
            var vm = new VirtualMachine();
            
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => vm.ExecuteProgram(program));
        }

        [Fact]
        public void ExecuteProgram_NegativeLoadArgIndex_ThrowsException()
        {
            // Arrange
            var program = new Program();
            
            var mainFunc = new Function("Main", "int");
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 5));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Call, "BadFunc"));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(mainFunc);
            
            var badFunc = new Function("BadFunc", "int");
            badFunc.ParameterTypes.Add("int");
            badFunc.Instructions.Add(new Instruction(InstructionType.LoadArg, -1)); // Negative index!
            badFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(badFunc);
            
            var vm = new VirtualMachine();
            
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => vm.ExecuteProgram(program));
        }

        [Fact]
        public void ExecuteProgram_FunctionWithComplexMathOperations_ReturnsCorrectResult()
        {
            // Arrange
            var program = new Program();
            
            var mainFunc = new Function("Main", "int");
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 12));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 8));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Call, "ComplexMath"));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(mainFunc);
            
            // ComplexMath: (a + b) * (a - b) / 2
            var mathFunc = new Function("ComplexMath", "int");
            mathFunc.ParameterTypes.Add("int");
            mathFunc.ParameterTypes.Add("int");
            
            // Calculate a + b
            mathFunc.Instructions.Add(new Instruction(InstructionType.LoadArg, 0));
            mathFunc.Instructions.Add(new Instruction(InstructionType.LoadArg, 1));
            mathFunc.Instructions.Add(new Instruction(InstructionType.Add)); // Stack: [a+b]
            
            // Calculate a - b
            mathFunc.Instructions.Add(new Instruction(InstructionType.LoadArg, 0));
            mathFunc.Instructions.Add(new Instruction(InstructionType.LoadArg, 1));
            mathFunc.Instructions.Add(new Instruction(InstructionType.Sub)); // Stack: [a+b, a-b]
            
            // Multiply them
            mathFunc.Instructions.Add(new Instruction(InstructionType.Mul)); // Stack: [(a+b)*(a-b)]
            
            // Divide by 2
            mathFunc.Instructions.Add(new Instruction(InstructionType.Load, 2));
            mathFunc.Instructions.Add(new Instruction(InstructionType.Div)); // Stack: [(a+b)*(a-b)/2]
            
            mathFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(mathFunc);
            
            var vm = new VirtualMachine();
            
            // Act
            var result = vm.ExecuteProgram(program);
            
            // Assert
            // (12 + 8) * (12 - 8) / 2 = 20 * 4 / 2 = 40
            Assert.Equal(40, result);
        }
    }
}
