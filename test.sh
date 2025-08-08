#!/bin/bash

echo "=== SimpleJIT Comprehensive Test Suite ==="
echo

echo "1. Testing simple arithmetic..."
dotnet run simple.txt
echo

echo "2. Testing complex expression..."
dotnet run complex.txt
echo

echo "3. Testing multiple operations..."
dotnet run multi.txt
echo

echo "4. Testing comprehensive example..."
dotnet run example.txt
echo

echo "5. Testing error handling - division by zero..."
dotnet run divzero.txt
echo

echo "6. Testing error handling - invalid instruction..."
dotnet run invalid.txt
echo

echo "7. Testing usage help..."
dotnet run
echo

echo "=== All tests completed ==="
