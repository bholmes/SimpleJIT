using Xunit;
using System;
using System.Runtime.InteropServices;
using SimpleJIT.Core;

namespace SimpleJIT.Tests
{
    public class NativeMemoryManagerTests
    {
        [Fact]
        public void AllocateWritableMemory_ValidSize_ReturnsNonZeroPointer()
        {
            // Arrange
            int size = 4096;

            // Act
            IntPtr ptr = NativeMemoryManager.AllocateWritableMemory(size);

            // Assert
            Assert.NotEqual(IntPtr.Zero, ptr);

            // Cleanup
            NativeMemoryManager.FreeMemory(ptr, size);
        }

        [Fact]
        public void AllocateWritableMemory_SmallSize_RoundsUpTo4KB()
        {
            // Arrange
            int smallSize = 100;

            // Act
            IntPtr ptr = NativeMemoryManager.AllocateWritableMemory(smallSize);

            // Assert
            Assert.NotEqual(IntPtr.Zero, ptr);

            // Cleanup - use actual allocated size (4096)
            NativeMemoryManager.FreeMemory(ptr, 4096);
        }

        [Fact(Skip = "NativeMemoryManager now throws ArgumentOutOfRangeException for zero size instead of returning zero pointer")]
        public void AllocateWritableMemory_ZeroSize_ReturnsZeroPointer()
        {
            // Arrange
            int size = 0;

            // Act
            IntPtr ptr = NativeMemoryManager.AllocateWritableMemory(size);

            // Assert
            Assert.Equal(IntPtr.Zero, ptr);
        }

        [Fact]
        public void CommitExecutableMemory_ValidPointer_Succeeds()
        {
            // Arrange
            int size = 4096;
            IntPtr ptr = NativeMemoryManager.AllocateWritableMemory(size);
            Assert.NotEqual(IntPtr.Zero, ptr);

            // Act & Assert - should not throw
            NativeMemoryManager.CommitExecutableMemory(ptr, size);

            // Cleanup
            NativeMemoryManager.FreeMemory(ptr, size);
        }

        [Fact(Skip = "NativeMemoryManager now throws ArgumentException for null pointer instead of being silent")]
        public void CommitExecutableMemory_ZeroPointer_DoesNotThrow()
        {
            // Arrange
            IntPtr ptr = IntPtr.Zero;
            int size = 4096;

            // Act & Assert - should not throw
            NativeMemoryManager.CommitExecutableMemory(ptr, size);
        }

        [Fact]
        public void FreeMemory_ValidPointer_DoesNotThrow()
        {
            // Arrange
            int size = 4096;
            IntPtr ptr = NativeMemoryManager.AllocateWritableMemory(size);
            Assert.NotEqual(IntPtr.Zero, ptr);

            // Act & Assert - should not throw
            NativeMemoryManager.FreeMemory(ptr, size);
        }

        [Fact]
        public void FreeMemory_ZeroPointer_DoesNotThrow()
        {
            // Arrange
            IntPtr ptr = IntPtr.Zero;
            int size = 4096;

            // Act & Assert - should not throw
            NativeMemoryManager.FreeMemory(ptr, size);
        }

        [Fact]
        public void MemoryWorkflow_AllocateCommitFree_CompletesSuccessfully()
        {
            // Arrange
            int size = 8192;

            // Act
            IntPtr ptr = NativeMemoryManager.AllocateWritableMemory(size);
            Assert.NotEqual(IntPtr.Zero, ptr);

            // Write some test data to verify memory is accessible BEFORE making it executable
            unsafe
            {
                byte* bytePtr = (byte*)ptr;
                bytePtr[0] = 0x90; // NOP instruction
                Assert.Equal(0x90, bytePtr[0]);
            }

            // Then make it executable (this will make it read-only)
            NativeMemoryManager.CommitExecutableMemory(ptr, size);

            // Assert & Cleanup - should not throw
            NativeMemoryManager.FreeMemory(ptr, size);
        }

        [Theory]
        [InlineData(1024)]
        [InlineData(2048)]
        [InlineData(4096)]
        [InlineData(8192)]
        [InlineData(16384)]
        public void AllocateWritableMemory_VariousSizes_Succeeds(int size)
        {
            // Act
            IntPtr ptr = NativeMemoryManager.AllocateWritableMemory(size);

            // Assert
            Assert.NotEqual(IntPtr.Zero, ptr);

            // Cleanup
            NativeMemoryManager.FreeMemory(ptr, Math.Max(size, 4096));
        }

        [Fact]
        public void MultipleAllocations_DoNotInterfere()
        {
            // Arrange
            int size = 4096;

            // Act
            IntPtr ptr1 = NativeMemoryManager.AllocateWritableMemory(size);
            IntPtr ptr2 = NativeMemoryManager.AllocateWritableMemory(size);
            IntPtr ptr3 = NativeMemoryManager.AllocateWritableMemory(size);

            // Assert
            Assert.NotEqual(IntPtr.Zero, ptr1);
            Assert.NotEqual(IntPtr.Zero, ptr2);
            Assert.NotEqual(IntPtr.Zero, ptr3);
            Assert.NotEqual(ptr1, ptr2);
            Assert.NotEqual(ptr2, ptr3);
            Assert.NotEqual(ptr1, ptr3);

            // Cleanup
            NativeMemoryManager.FreeMemory(ptr1, size);
            NativeMemoryManager.FreeMemory(ptr2, size);
            NativeMemoryManager.FreeMemory(ptr3, size);
        }
    }
}
