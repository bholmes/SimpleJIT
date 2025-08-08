#!/bin/bash

echo "=== SimpleJIT Comprehensive Test Suite ==="
echo

echo "1. Testing simple arithmetic..."
dotnet run --project SimpleJIT simple.txt
echo

echo "2. Testing complex expression..."
dotnet run --project SimpleJIT complex.txt
echo

echo "3. Testing multiple operations..."
dotnet run --project SimpleJIT multi.txt
echo

echo "4. Testing comprehensive example..."
dotnet run --project SimpleJIT example.txt
echo

echo "5. Testing error handling - division by zero..."
dotnet run --project SimpleJIT divzero.txt
echo

echo "6. Testing error handling - invalid instruction..."
dotnet run --project SimpleJIT invalid.txt
echo

echo "7. Testing usage help..."
dotnet run --project SimpleJIT
echo

echo "=== All tests completed ==="
