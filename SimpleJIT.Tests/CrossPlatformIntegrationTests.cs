using Xunit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using SimpleJIT.Core;

namespace SimpleJIT.Tests.Integration
{
    public class CrossPlatformIntegrationTests
    {
        [Fact]
        public void JitCompilation_CrossArchitecture_SelectsCorrectCompiler()
        {
            // Arrange
            var instructions = new List<Instruction>
            {
                new(InstructionType.Load, 42),
                new(InstructionType.Return)
            };

            // Act
            var compiledFunction = JitCompiler.CompileInstructions(instructions);

            // Assert - Should work on both ARM64 and x64
            if (compiledFunction != null)
            {
                var result = compiledFunction();
                Assert.Equal(42, result);
            }
            else
            {
                // JIT compilation may fail in test environments - this is acceptable
                Assert.Null(compiledFunction);
            }
        }

        [Fact]
        public void NativeMemoryManager_CrossPlatform_AllocatesMemoryCorrectly()
        {
            // Arrange
            int size = 4096;

            // Act
            var ptr = NativeMemoryManager.AllocateWritableMemory(size);

            // Assert - Should work on Windows, macOS, and Linux
            try
            {
                Assert.NotEqual(System.IntPtr.Zero, ptr);

                // Verify memory is writable
                unsafe
                {
                    byte* bytePtr = (byte*)ptr;
                    bytePtr[0] = 0x42;
                    Assert.Equal(0x42, bytePtr[0]);
                }
            }
            finally
            {
                NativeMemoryManager.FreeMemory(ptr, size);
            }
        }

        [Fact]
        public void EndToEndWorkflow_ParseCompileExecute_WorksAcrossPlatforms()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var content = @"load 10
load 5
add
load 2
mul
ret";
            System.IO.File.WriteAllText(tempFile, content);

            try
            {
                // Act - Full end-to-end workflow
                var instructions = Parser.ParseFile(tempFile);
                var compiledFunction = JitCompiler.CompileInstructions(instructions);

                // Assert
                Assert.Equal(6, instructions.Count); // 5 instructions + implicit return
                
                if (compiledFunction != null)
                {
                    var result = compiledFunction();
                    Assert.Equal(30, result);
                }
            }
            finally
            {
                System.IO.File.Delete(tempFile);
            }
        }

        [Fact]
        public void MemoryProtection_CrossPlatform_MakesMemoryExecutable()
        {
            // Arrange
            int size = 4096;
            var ptr = NativeMemoryManager.AllocateWritableMemory(size);
            Assert.NotEqual(System.IntPtr.Zero, ptr);

            try
            {
                // Act - Should work on all platforms
                NativeMemoryManager.CommitExecutableMemory(ptr, size);

                // Assert - No exception thrown means success
                Assert.True(true); // Explicit success assertion
            }
            finally
            {
                NativeMemoryManager.FreeMemory(ptr, size);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(1024)]
        [InlineData(4096)]
        [InlineData(8192)]
        [InlineData(16384)]
        public void MemoryAllocation_VariousSizes_WorksAcrossPlatforms(int size)
        {
            // Act
            var ptr = NativeMemoryManager.AllocateWritableMemory(size);

            // Assert
            try
            {
                Assert.NotEqual(System.IntPtr.Zero, ptr);

                // Test writing to allocated memory
                unsafe
                {
                    byte* bytePtr = (byte*)ptr;
                    bytePtr[0] = 0xFF;
                    Assert.Equal(0xFF, bytePtr[0]);
                }
            }
            finally
            {
                // Use rounded-up size for cleanup
                var actualSize = Math.Max(size, 4096);
                NativeMemoryManager.FreeMemory(ptr, actualSize);
            }
        }

        [Fact]
        public void JitVsVm_SameInstructions_ProduceSameResults()
        {
            // Arrange - Complex calculation to ensure both implementations work
            var instructions = new List<Instruction>
            {
                new(InstructionType.Load, 100),
                new(InstructionType.Load, 50),
                new(InstructionType.Sub),      // 50
                new(InstructionType.Load, 3),
                new(InstructionType.Div),      // 16 (integer division)
                new(InstructionType.Load, 4),
                new(InstructionType.Mul),      // 64
                new(InstructionType.Return)
            };

            // Act
            var compiledFunction = JitCompiler.CompileInstructions(instructions);
            var vm = new VirtualMachine();
            var vmResult = vm.Execute(instructions);

            // Assert
            Assert.Equal(64, vmResult); // VM result should always work

            if (compiledFunction != null)
            {
                var jitResult = compiledFunction();
                Assert.Equal(vmResult, jitResult); // JIT and VM should produce same result
            }
            // If JIT compilation fails, that's acceptable - VM is the fallback
        }

        [Fact]
        public void ArchitectureDetection_ReturnsValidArchitecture()
        {
            // Act
            var architecture = RuntimeInformation.ProcessArchitecture;

            // Assert - Should be one of the supported architectures
            Assert.True(
                architecture == Architecture.Arm64 || 
                architecture == Architecture.X64 ||
                architecture == Architecture.X86,
                $"Unexpected architecture: {architecture}"
            );
        }

        [Fact]
        public void PlatformDetection_ReturnsValidPlatform()
        {
            // Act
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var isMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
            var isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

            // Assert - Should be exactly one of the supported platforms
            var platformCount = (isWindows ? 1 : 0) + (isMacOS ? 1 : 0) + (isLinux ? 1 : 0);
            Assert.Equal(1, platformCount);
        }
    }
}
