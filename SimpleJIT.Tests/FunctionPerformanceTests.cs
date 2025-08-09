using Xunit;
using System.IO;
using System.Diagnostics;
using SimpleJIT.Core;

namespace SimpleJIT.Tests.Performance
{
    public class FunctionPerformanceTests
    {
        [Fact]
        public void ExecuteProgram_ManyFunctionCalls_CompletesInReasonableTime()
        {
            // Arrange - Create a program that makes many sequential function calls
            var program = new Program();
            
            var mainFunc = new Function("Main", "int");
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 1));
            
            // Chain 100 function calls
            for (int i = 0; i < 100; i++)
            {
                mainFunc.Instructions.Add(new Instruction(InstructionType.Call, "Increment"));
            }
            
            mainFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(mainFunc);
            
            // Increment function: add 1 to input
            var incrementFunc = new Function("Increment", "int");
            incrementFunc.ParameterTypes.Add("int");
            incrementFunc.Instructions.Add(new Instruction(InstructionType.LoadArg, 0));
            incrementFunc.Instructions.Add(new Instruction(InstructionType.Load, 1));
            incrementFunc.Instructions.Add(new Instruction(InstructionType.Add));
            incrementFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(incrementFunc);
            
            var vm = new VirtualMachine();
            var stopwatch = Stopwatch.StartNew();
            
            // Act
            var result = vm.ExecuteProgram(program);
            stopwatch.Stop();
            
            // Assert
            Assert.Equal(101, result); // 1 + 100 increments = 101
            Assert.True(stopwatch.ElapsedMilliseconds < 1000, 
                $"Execution took {stopwatch.ElapsedMilliseconds}ms, expected < 1000ms");
        }

        [Fact]
        public void ExecuteProgram_ManyFunctions_CompletesSuccessfully()
        {
            // Arrange - Create a program with many functions (testing function lookup performance)
            var program = new Program();
            
            var mainFunc = new Function("Main", "int");
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 0));
            
            // Call 50 different functions
            for (int i = 0; i < 50; i++)
            {
                mainFunc.Instructions.Add(new Instruction(InstructionType.Call, $"Func{i}"));
                mainFunc.Instructions.Add(new Instruction(InstructionType.Add));
            }
            
            mainFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(mainFunc);
            
            // Create 50 functions, each returns its index
            for (int i = 0; i < 50; i++)
            {
                var func = new Function($"Func{i}", "int");
                func.Instructions.Add(new Instruction(InstructionType.Load, i));
                func.Instructions.Add(new Instruction(InstructionType.Return));
                program.Functions.Add(func);
            }
            
            var vm = new VirtualMachine();
            var stopwatch = Stopwatch.StartNew();
            
            // Act
            var result = vm.ExecuteProgram(program);
            stopwatch.Stop();
            
            // Assert
            var expectedSum = 0 + (49 * 50 / 2); // Sum of 0 to 49
            Assert.Equal(expectedSum, result);
            Assert.True(stopwatch.ElapsedMilliseconds < 1000, 
                $"Execution took {stopwatch.ElapsedMilliseconds}ms, expected < 1000ms");
        }

        [Fact]
        public void ExecuteProgram_DeepCallStack_CompletesSuccessfully()
        {
            // Arrange - Test deep call stack (50 levels)
            var program = new Program();
            
            var mainFunc = new Function("Main", "int");
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 0));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Call, "Level0"));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(mainFunc);
            
            // Create a chain of 50 functions, each calling the next
            for (int i = 0; i < 50; i++)
            {
                var func = new Function($"Level{i}", "int");
                func.ParameterTypes.Add("int");
                func.Instructions.Add(new Instruction(InstructionType.LoadArg, 0));
                func.Instructions.Add(new Instruction(InstructionType.Load, 1));
                func.Instructions.Add(new Instruction(InstructionType.Add));
                
                if (i < 49)
                {
                    func.Instructions.Add(new Instruction(InstructionType.Call, $"Level{i + 1}"));
                }
                
                func.Instructions.Add(new Instruction(InstructionType.Return));
                program.Functions.Add(func);
            }
            
            var vm = new VirtualMachine();
            var stopwatch = Stopwatch.StartNew();
            
            // Act
            var result = vm.ExecuteProgram(program);
            stopwatch.Stop();
            
            // Assert
            Assert.Equal(50, result); // Each level adds 1, so 0 + 50 = 50
            Assert.True(stopwatch.ElapsedMilliseconds < 1000, 
                $"Execution took {stopwatch.ElapsedMilliseconds}ms, expected < 1000ms");
        }

        [Fact]
        public void ExecuteProgram_FunctionWithManyParameters_CompletesSuccessfully()
        {
            // Arrange - Test function with many parameters (stress test argument handling)
            var program = new Program();
            
            var mainFunc = new Function("Main", "int");
            
            // Push 20 values onto stack
            for (int i = 1; i <= 20; i++)
            {
                mainFunc.Instructions.Add(new Instruction(InstructionType.Load, i));
            }
            
            mainFunc.Instructions.Add(new Instruction(InstructionType.Call, "SumMany"));
            mainFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(mainFunc);
            
            // Function that sums 20 parameters
            var sumFunc = new Function("SumMany", "int");
            for (int i = 0; i < 20; i++)
            {
                sumFunc.ParameterTypes.Add("int");
            }
            
            // Load first parameter
            sumFunc.Instructions.Add(new Instruction(InstructionType.LoadArg, 0));
            
            // Add all other parameters
            for (int i = 1; i < 20; i++)
            {
                sumFunc.Instructions.Add(new Instruction(InstructionType.LoadArg, i));
                sumFunc.Instructions.Add(new Instruction(InstructionType.Add));
            }
            
            sumFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(sumFunc);
            
            var vm = new VirtualMachine();
            var stopwatch = Stopwatch.StartNew();
            
            // Act
            var result = vm.ExecuteProgram(program);
            stopwatch.Stop();
            
            // Assert
            var expectedSum = 20 * 21 / 2; // Sum of 1 to 20 = 210
            Assert.Equal(expectedSum, result);
            Assert.True(stopwatch.ElapsedMilliseconds < 1000, 
                $"Execution took {stopwatch.ElapsedMilliseconds}ms, expected < 1000ms");
        }

        [Fact]
        public void ParseProgram_LargeFile_CompletesInReasonableTime()
        {
            // Arrange - Create a simple test first
            var tempFile = Path.GetTempFileName();
            var content = @"int Main()
{
    load 10
    ret
}

int Add(int a, int b)
{
    loadarg 0
    loadarg 1
    add
    ret
}";
            
            File.WriteAllText(tempFile, content);
            
            try
            {
                var stopwatch = Stopwatch.StartNew();
                
                // Act
                var program = FunctionParser.ParseProgram(tempFile);
                stopwatch.Stop();
                
                // Assert - expect 2 functions (Main and Add)
                Assert.Equal(2, program.Functions.Count);
                Assert.Contains(program.Functions, f => f.Name == "Main");
                Assert.Contains(program.Functions, f => f.Name == "Add");
                Assert.True(stopwatch.ElapsedMilliseconds < 1000, 
                    $"Parsing took {stopwatch.ElapsedMilliseconds}ms, expected < 1000ms");
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void ExecuteProgram_MemoryIntensiveOperations_DoesNotLeakMemory()
        {
            // Arrange - Program that should stress garbage collection
            var program = new Program();
            
            var mainFunc = new Function("Main", "int");
            mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 0));
            
            // Perform many operations that create temporary objects
            for (int i = 0; i < 1000; i++)
            {
                mainFunc.Instructions.Add(new Instruction(InstructionType.Load, 1));
                mainFunc.Instructions.Add(new Instruction(InstructionType.Add));
                mainFunc.Instructions.Add(new Instruction(InstructionType.Call, "Identity"));
            }
            
            mainFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(mainFunc);
            
            // Identity function - just returns its input
            var identityFunc = new Function("Identity", "int");
            identityFunc.ParameterTypes.Add("int");
            identityFunc.Instructions.Add(new Instruction(InstructionType.LoadArg, 0));
            identityFunc.Instructions.Add(new Instruction(InstructionType.Return));
            program.Functions.Add(identityFunc);
            
            var vm = new VirtualMachine();
            
            // Measure memory before
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var memoryBefore = GC.GetTotalMemory(false);
            
            // Act
            var result = vm.ExecuteProgram(program);
            
            // Force garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var memoryAfter = GC.GetTotalMemory(false);
            
            // Assert
            Assert.Equal(1000, result);
            
            // Memory usage shouldn't grow dramatically (allow for some variance)
            var memoryGrowth = memoryAfter - memoryBefore;
            Assert.True(memoryGrowth < 1024 * 1024, // Less than 1MB growth
                $"Memory grew by {memoryGrowth} bytes, expected < 1MB");
        }
    }
}
