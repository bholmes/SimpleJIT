using Xunit;
using System;
using System.Collections.Generic;
using SimpleJIT.Core;

namespace SimpleJIT.Tests.Unit
{
    public class ErrorHandlingTests
    {
        [Fact]
        public void VirtualMachine_DivisionByZero_ThrowsException()
        {
            // Arrange
            var instructions = new List<Instruction>
            {
                new(InstructionType.Load, 10),
                new(InstructionType.Load, 0),
                new(InstructionType.Div),
                new(InstructionType.Return)
            };
            var vm = new VirtualMachine();

            // Act & Assert
            Assert.Throws<DivideByZeroException>(() => vm.Execute(instructions));
        }

        [Fact]
        public void VirtualMachine_StackUnderflow_ThrowsException()
        {
            // Arrange - Try to add without enough operands
            var instructions = new List<Instruction>
            {
                new(InstructionType.Load, 10),
                new(InstructionType.Add), // Only one operand on stack
                new(InstructionType.Return)
            };
            var vm = new VirtualMachine();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => vm.Execute(instructions));
        }

        [Fact]
        public void VirtualMachine_EmptyStackReturn_ReturnsZero()
        {
            // Arrange
            var instructions = new List<Instruction>
            {
                new(InstructionType.Return) // Return with empty stack
            };
            var vm = new VirtualMachine();

            // Act
            var result = vm.Execute(instructions);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void VirtualMachine_NoReturnInstruction_ImplicitReturn()
        {
            // Arrange
            var instructions = new List<Instruction>
            {
                new(InstructionType.Load, 42)
                // No explicit return
            };
            var vm = new VirtualMachine();

            // Act
            var result = vm.Execute(instructions);

            // Assert
            Assert.Equal(42, result);
        }

        [Fact]
        public void VirtualMachine_MultipleOperationsStackUnderflow_ThrowsException()
        {
            // Arrange
            var instructions = new List<Instruction>
            {
                new(InstructionType.Mul), // No operands
                new(InstructionType.Return)
            };
            var vm = new VirtualMachine();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => vm.Execute(instructions));
        }

        [Fact]
        public void VirtualMachine_SubtractionStackUnderflow_ThrowsException()
        {
            // Arrange
            var instructions = new List<Instruction>
            {
                new(InstructionType.Load, 5),
                new(InstructionType.Sub), // Only one operand
                new(InstructionType.Return)
            };
            var vm = new VirtualMachine();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => vm.Execute(instructions));
        }

        [Fact]
        public void VirtualMachine_PrintWithEmptyStack_DoesNotThrow()
        {
            // Arrange
            var instructions = new List<Instruction>
            {
                new(InstructionType.Print), // Print with empty stack
                new(InstructionType.Return)
            };
            var vm = new VirtualMachine();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => vm.Execute(instructions));
        }

        [Fact]
        public void VirtualMachine_LargeNumbers_HandledCorrectly()
        {
            // Arrange
            var instructions = new List<Instruction>
            {
                new(InstructionType.Load, long.MaxValue),
                new(InstructionType.Load, 1),
                new(InstructionType.Add), // This will overflow
                new(InstructionType.Return)
            };
            var vm = new VirtualMachine();

            // Act
            var result = vm.Execute(instructions);

            // Assert - Should handle overflow (wrapping to negative)
            Assert.Equal(long.MinValue, result);
        }

        [Fact]
        public void VirtualMachine_NegativeNumbers_HandledCorrectly()
        {
            // Arrange
            var instructions = new List<Instruction>
            {
                new(InstructionType.Load, -100),
                new(InstructionType.Load, 50),
                new(InstructionType.Add),
                new(InstructionType.Return)
            };
            var vm = new VirtualMachine();

            // Act
            var result = vm.Execute(instructions);

            // Assert
            Assert.Equal(-50, result);
        }

        [Fact]
        public void JitCompiler_NullInstructions_ReturnsNull()
        {
            // Act & Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.Throws<ArgumentNullException>(() => JitCompiler.CompileInstructions(null));
#pragma warning restore CS8625
        }

        [Fact]
        public void JitCompiler_EmptyInstructions_ReturnsNotNull()
        {
            // Arrange
            var instructions = new List<Instruction>();

            // Act
            var result = JitCompiler.CompileInstructions(instructions);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void Parser_EmptyString_ReturnsEmptyList()
        {
            // Arrange - Create temp file with empty content
            var tempFile = System.IO.Path.GetTempFileName();
            System.IO.File.WriteAllText(tempFile, "");

            try
            {
                // Act
                var instructions = Parser.ParseFile(tempFile);

                // Assert
                Assert.Empty(instructions);
            }
            finally
            {
                System.IO.File.Delete(tempFile);
            }
        }

        [Fact]
        public void Parser_OnlyWhitespace_ReturnsEmptyList()
        {
            // Arrange
            var tempFile = System.IO.Path.GetTempFileName();
            System.IO.File.WriteAllText(tempFile, "   \n\t\r\n   ");

            try
            {
                // Act
                var instructions = Parser.ParseFile(tempFile);

                // Assert
                Assert.Empty(instructions);
            }
            finally
            {
                System.IO.File.Delete(tempFile);
            }
        }

        [Fact]
        public void JitCompiler_NullInstructions_ThrowsArgumentOutOfRangeException()
        {
            // Act & Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.Throws<ArgumentNullException>(() => JitCompiler.CompileInstructions(null));
#pragma warning restore CS8625
        }

        [Fact]
        public void NativeMemoryManager_FreeNullPointer_DoesNotThrow()
        {
            // Act & Assert - Should not throw
            NativeMemoryManager.FreeMemory(IntPtr.Zero, 4096);
        }

        [Fact]
        public void JitCompiler_NullInstructions_ThrowsArgumentException()
        {
            // Act & Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Assert.Throws<ArgumentNullException>(() => JitCompiler.CompileInstructions(null));
#pragma warning restore CS8625
        }

        [Fact]
        public void VirtualMachine_VeryLargeStack_HandledCorrectly()
        {
            // Arrange - Build a large stack
            var instructions = new List<Instruction>();
            
            // Load 1000 values
            for (int i = 0; i < 1000; i++)
            {
                instructions.Add(new(InstructionType.Load, 1));
            }
            
            // Sum them all (999 add operations)
            for (int i = 0; i < 999; i++)
            {
                instructions.Add(new(InstructionType.Add));
            }
            
            instructions.Add(new(InstructionType.Return));

            var vm = new VirtualMachine();

            // Act
            var result = vm.Execute(instructions);

            // Assert
            Assert.Equal(1000, result);
        }

        [Fact]
        public void VirtualMachine_ConsecutiveDivisions_HandledCorrectly()
        {
            // Arrange
            var instructions = new List<Instruction>
            {
                new(InstructionType.Load, 1000),
                new(InstructionType.Load, 10),
                new(InstructionType.Div),      // 100
                new(InstructionType.Load, 5),
                new(InstructionType.Div),      // 20
                new(InstructionType.Load, 2),
                new(InstructionType.Div),      // 10
                new(InstructionType.Return)
            };
            var vm = new VirtualMachine();

            // Act
            var result = vm.Execute(instructions);

            // Assert
            Assert.Equal(10, result);
        }
    }
}
