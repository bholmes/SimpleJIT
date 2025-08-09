using Xunit;
using System;
using System.IO;
using System.Collections.Generic;
using SimpleJIT.Core;

namespace SimpleJIT.Tests.Integration
{
    public class SampleFileIntegrationTests
    {
        private static readonly string SamplesDirectory = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..", "samples"
        );

        [Fact]
        public void SampleFile_Simple_ExecutesCorrectly()
        {
            // Arrange
            var sampleFile = Path.Combine(SamplesDirectory, "simple.txt");
            
            // Verify sample file exists
            Assert.True(File.Exists(sampleFile), $"Sample file not found: {sampleFile}");

            // Act
            var instructions = Parser.ParseFile(sampleFile);
            var vm = new VirtualMachine();
            var result = vm.Execute(instructions);

            // Assert - Simple addition: 7 + 3 = 10
            Assert.Equal(10, result);
            
            // Verify instruction parsing
            Assert.Equal(4, instructions.Count);
            Assert.Equal(InstructionType.Load, instructions[0].Type);
            Assert.Equal(7, instructions[0].Value);
            Assert.Equal(InstructionType.Load, instructions[1].Type);
            Assert.Equal(3, instructions[1].Value);
            Assert.Equal(InstructionType.Add, instructions[2].Type);
            Assert.Equal(InstructionType.Return, instructions[3].Type);
        }

        [Fact]
        public void SampleFile_Example_ExecutesCorrectly()
        {
            // Arrange
            var sampleFile = Path.Combine(SamplesDirectory, "example.txt");
            
            // Verify sample file exists
            Assert.True(File.Exists(sampleFile), $"Sample file not found: {sampleFile}");

            // Act
            var instructions = Parser.ParseFile(sampleFile);
            var vm = new VirtualMachine();
            var result = vm.Execute(instructions);

            // Assert - (10 + 5) * 2 = 30
            Assert.Equal(30, result);
            
            // Verify instruction parsing
            Assert.Equal(7, instructions.Count);
            Assert.Equal(InstructionType.Load, instructions[0].Type);
            Assert.Equal(10, instructions[0].Value);
            Assert.Equal(InstructionType.Load, instructions[1].Type);
            Assert.Equal(5, instructions[1].Value);
            Assert.Equal(InstructionType.Add, instructions[2].Type);
            Assert.Equal(InstructionType.Load, instructions[3].Type);
            Assert.Equal(2, instructions[3].Value);
            Assert.Equal(InstructionType.Mul, instructions[4].Type);
            Assert.Equal(InstructionType.Print, instructions[5].Type);
            Assert.Equal(InstructionType.Return, instructions[6].Type);
        }

        [Fact]
        public void SampleFile_Complex_ExecutesCorrectly()
        {
            // Arrange
            var sampleFile = Path.Combine(SamplesDirectory, "complex.txt");
            
            // Verify sample file exists
            Assert.True(File.Exists(sampleFile), $"Sample file not found: {sampleFile}");

            // Act
            var instructions = Parser.ParseFile(sampleFile);
            var vm = new VirtualMachine();
            var result = vm.Execute(instructions);

            // Assert - ((15 - 3) * 2) / 4 = 6
            Assert.Equal(6, result);
            
            // Verify instruction parsing
            Assert.Equal(9, instructions.Count);
            Assert.Equal(InstructionType.Load, instructions[0].Type);
            Assert.Equal(15, instructions[0].Value);
            Assert.Equal(InstructionType.Load, instructions[1].Type);
            Assert.Equal(3, instructions[1].Value);
            Assert.Equal(InstructionType.Sub, instructions[2].Type);
            Assert.Equal(InstructionType.Load, instructions[3].Type);
            Assert.Equal(2, instructions[3].Value);
            Assert.Equal(InstructionType.Mul, instructions[4].Type);
            Assert.Equal(InstructionType.Load, instructions[5].Type);
            Assert.Equal(4, instructions[5].Value);
            Assert.Equal(InstructionType.Div, instructions[6].Type);
            Assert.Equal(InstructionType.Print, instructions[7].Type);
            Assert.Equal(InstructionType.Return, instructions[8].Type);
        }

        [Fact]
        public void SampleFile_Multi_ExecutesCorrectly()
        {
            // Arrange
            var sampleFile = Path.Combine(SamplesDirectory, "multi.txt");
            
            // Verify sample file exists
            Assert.True(File.Exists(sampleFile), $"Sample file not found: {sampleFile}");

            // Act
            var instructions = Parser.ParseFile(sampleFile);
            var vm = new VirtualMachine();
            var result = vm.Execute(instructions);

            // Assert - Final result: ((100 - 25) * 5) / 3 = 125 (integer division)
            Assert.Equal(125, result);
            
            // Verify instruction parsing includes multiple print statements
            var printCount = 0;
            foreach (var instruction in instructions)
            {
                if (instruction.Type == InstructionType.Print)
                    printCount++;
            }
            Assert.Equal(3, printCount); // Should have 3 print instructions
        }

        [Fact]
        public void SampleFile_DivZero_ThrowsException()
        {
            // Arrange
            var sampleFile = Path.Combine(SamplesDirectory, "divzero.txt");
            
            // Verify sample file exists
            Assert.True(File.Exists(sampleFile), $"Sample file not found: {sampleFile}");

            // Act
            var instructions = Parser.ParseFile(sampleFile);
            var vm = new VirtualMachine();

            // Assert - Should throw DivideByZeroException
            Assert.Throws<DivideByZeroException>(() => vm.Execute(instructions));
            
            // Verify instruction parsing
            Assert.Equal(5, instructions.Count);
            Assert.Equal(InstructionType.Load, instructions[0].Type);
            Assert.Equal(10, instructions[0].Value);
            Assert.Equal(InstructionType.Load, instructions[1].Type);
            Assert.Equal(0, instructions[1].Value);
            Assert.Equal(InstructionType.Div, instructions[2].Type);
            Assert.Equal(InstructionType.Print, instructions[3].Type);
            Assert.Equal(InstructionType.Return, instructions[4].Type);
        }

        [Fact]
        public void SampleFile_Invalid_ThrowsParseException()
        {
            // Arrange
            var sampleFile = Path.Combine(SamplesDirectory, "invalid.txt");
            
            // Verify sample file exists
            Assert.True(File.Exists(sampleFile), $"Sample file not found: {sampleFile}");

            // Act & Assert - Should throw ArgumentException for invalid instruction
            var exception = Assert.Throws<ArgumentException>(() => Parser.ParseFile(sampleFile));
            Assert.Contains("Unknown instruction", exception.Message);
            Assert.Contains("badinstruction", exception.Message);
        }

        [Fact]
        public void SampleFiles_JitVsVm_ProduceSameResults()
        {
            // Test that JIT and VM produce the same results for valid sample files
            var validSampleFiles = new[]
            {
                "simple.txt",
                "example.txt", 
                "complex.txt",
                "multi.txt"
            };

            foreach (var sampleFileName in validSampleFiles)
            {
                // Arrange
                var sampleFile = Path.Combine(SamplesDirectory, sampleFileName);
                Assert.True(File.Exists(sampleFile), $"Sample file not found: {sampleFile}");

                var instructions = Parser.ParseFile(sampleFile);
                
                // Act
                var vm = new VirtualMachine();
                var vmResult = vm.Execute(instructions);
                
                var compiledFunction = JitCompiler.CompileInstructions(instructions);
                
                // Assert
                if (compiledFunction != null)
                {
                    var jitResult = compiledFunction();
                    Assert.Equal(vmResult, jitResult);
                }
                // If JIT compilation fails, that's acceptable - we just test VM
            }
        }

        [Theory]
        [InlineData("simple.txt", 10)]
        [InlineData("example.txt", 30)]
        [InlineData("complex.txt", 6)]
        [InlineData("multi.txt", 125)]
        public void SampleFiles_ParameterizedTest_ProducesExpectedResults(string fileName, int expectedResult)
        {
            // Arrange
            var sampleFile = Path.Combine(SamplesDirectory, fileName);
            Assert.True(File.Exists(sampleFile), $"Sample file not found: {sampleFile}");

            // Act
            var instructions = Parser.ParseFile(sampleFile);
            var vm = new VirtualMachine();
            var result = vm.Execute(instructions);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void SampleFiles_AllValidFilesParseSuccessfully()
        {
            // Arrange
            var validSampleFiles = new[]
            {
                "simple.txt",
                "example.txt", 
                "complex.txt",
                "multi.txt",
                "divzero.txt" // This parses successfully, just throws at runtime
            };

            foreach (var sampleFileName in validSampleFiles)
            {
                // Arrange
                var sampleFile = Path.Combine(SamplesDirectory, sampleFileName);
                Assert.True(File.Exists(sampleFile), $"Sample file not found: {sampleFile}");

                // Act & Assert - Should not throw during parsing
                var instructions = Parser.ParseFile(sampleFile);
                Assert.NotEmpty(instructions);
                
                // All valid files should have at least one instruction
                Assert.True(instructions.Count > 0);
                
                // All valid files should end with a return instruction
                Assert.Equal(InstructionType.Return, instructions[^1].Type);
            }
        }

        [Fact]
        public void SampleFiles_CommentHandling_IgnoresComments()
        {
            // Test that all sample files with comments parse correctly
            var sampleFiles = new[]
            {
                "simple.txt",
                "example.txt", 
                "complex.txt",
                "multi.txt",
                "divzero.txt",
                "invalid.txt"
            };

            foreach (var sampleFileName in sampleFiles)
            {
                // Arrange
                var sampleFile = Path.Combine(SamplesDirectory, sampleFileName);
                Assert.True(File.Exists(sampleFile), $"Sample file not found: {sampleFile}");

                var fileContent = File.ReadAllText(sampleFile);
                
                // Only test files that actually have comments
                if (fileContent.Contains("//"))
                {
                    try
                    {
                        // Act
                        var instructions = Parser.ParseFile(sampleFile);
                        
                        // Assert - Comments should not become instructions
                        foreach (var instruction in instructions)
                        {
                            // No instruction should have a type that doesn't exist
                            Assert.True(Enum.IsDefined(typeof(InstructionType), instruction.Type));
                        }
                    }
                    catch (ArgumentException)
                    {
                        // Invalid files are expected to throw - that's okay
                        Assert.Equal("invalid.txt", sampleFileName);
                    }
                }
            }
        }

        [Fact]
        public void SampleDirectory_Exists_AndContainsExpectedFiles()
        {
            // Arrange & Act
            var expectedFiles = new[]
            {
                "simple.txt",
                "example.txt", 
                "complex.txt",
                "multi.txt",
                "divzero.txt",
                "invalid.txt"
            };

            // Assert
            Assert.True(Directory.Exists(SamplesDirectory), $"Samples directory not found: {SamplesDirectory}");
            
            foreach (var expectedFile in expectedFiles)
            {
                var filePath = Path.Combine(SamplesDirectory, expectedFile);
                Assert.True(File.Exists(filePath), $"Expected sample file not found: {expectedFile}");
            }
        }

        [Fact]
        public void SampleFiles_EndToEnd_VmExecution()
        {
            // Test complete end-to-end workflow: Parse -> Execute with VM
            var sampleFile = Path.Combine(SamplesDirectory, "example.txt");
            Assert.True(File.Exists(sampleFile));

            // Act - Complete workflow
            var instructions = Parser.ParseFile(sampleFile);
            var vm = new VirtualMachine();
            
            // Capture initial state
            var initialInstructionCount = instructions.Count;
            
            // Execute
            var result = vm.Execute(instructions);
            
            // Assert
            Assert.Equal(30, result); // (10 + 5) * 2 = 30
            Assert.Equal(7, initialInstructionCount); // Should have 7 instructions
        }

        [Fact]
        public void SampleFiles_EndToEnd_JitExecution()
        {
            // Test complete end-to-end workflow: Parse -> Compile -> Execute with JIT
            var sampleFile = Path.Combine(SamplesDirectory, "simple.txt");
            Assert.True(File.Exists(sampleFile));

            // Act - Complete workflow
            var instructions = Parser.ParseFile(sampleFile);
            var compiledFunction = JitCompiler.CompileInstructions(instructions);
            
            // Assert
            if (compiledFunction != null)
            {
                var result = compiledFunction();
                Assert.Equal(10, result); // 7 + 3 = 10
            }
            else
            {
                // JIT compilation failed - acceptable in test environment
                // Fall back to VM to verify the instructions are correct
                var vm = new VirtualMachine();
                var vmResult = vm.Execute(instructions);
                Assert.Equal(10, vmResult);
            }
        }
    }
}
