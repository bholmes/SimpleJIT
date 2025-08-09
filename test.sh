#!/bin/bash

echo "=== SimpleJIT Comprehensive Test Suite ==="
echo

echo "1. Testing simple arithmetic..."
dotnet run --project SimpleJIT/SimpleJIT.csproj samples/simple.txt
echo

echo "2. Testing complex expression..."
dotnet run --project SimpleJIT/SimpleJIT.csproj samples/complex.txt
echo

echo "3. Testing multiple operations..."
dotnet run --project SimpleJIT/SimpleJIT.csproj samples/multi.txt
echo

echo "4. Testing comprehensive example..."
dotnet run --project SimpleJIT/SimpleJIT.csproj samples/example.txt
echo

echo "5. Testing error handling - division by zero..."
dotnet run --project SimpleJIT/SimpleJIT.csproj samples/divzero.txt
echo

echo "6. Testing error handling - invalid instruction..."
dotnet run --project SimpleJIT/SimpleJIT.csproj samples/invalid.txt
echo

echo "7. Testing function-based programs..."
dotnet run --project SimpleJIT/SimpleJIT.csproj samples/functions.txt
echo

echo "8. Testing usage help..."
dotnet run --project SimpleJIT/SimpleJIT.csproj
echo

echo "9. Running all unit tests..."
dotnet test --verbosity normal
echo

echo "=== All tests completed ==="
