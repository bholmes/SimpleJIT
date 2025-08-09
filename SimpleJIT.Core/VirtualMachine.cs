using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleJIT.Core;

public class CallFrame
{
    public string FunctionName { get; set; }
    public List<int> Arguments { get; set; }
    public int ReturnAddress { get; set; }

    public CallFrame(string functionName, List<int> arguments, int returnAddress)
    {
        FunctionName = functionName;
        Arguments = arguments;
        ReturnAddress = returnAddress;
    }
}

public class VirtualMachine
{
    private Stack<int> stack = new Stack<int>();
    private Stack<CallFrame> callStack = new Stack<CallFrame>();

    // Execute a single function (legacy method for backward compatibility)
    public int Execute(List<Instruction> instructions)
    {
        stack.Clear();
        callStack.Clear();

        return ExecuteInstructions(instructions, new List<int>());
    }

    // Execute a program with functions
    public int ExecuteProgram(Program program)
    {
        stack.Clear();
        callStack.Clear();

        var mainFunction = program.GetMainFunction();
        if (mainFunction == null)
            throw new InvalidOperationException("No Main function found");

        return ExecuteFunction(program, mainFunction, new List<int>());
    }

    private int ExecuteFunction(Program program, Function function, List<int> arguments)
    {
        // Push call frame for argument access
        var callFrame = new CallFrame(function.Name, arguments, -1);
        callStack.Push(callFrame);

        try
        {
            return ExecuteInstructions(function.Instructions, arguments, program);
        }
        finally
        {
            callStack.Pop();
        }
    }

    private int ExecuteInstructions(List<Instruction> instructions, List<int>? arguments = null, Program? program = null)
    {
        arguments ??= new List<int>();

        for (int pc = 0; pc < instructions.Count; pc++)
        {
            var instruction = instructions[pc];

            switch (instruction.Type)
            {
                case InstructionType.Load:
                    stack.Push(instruction.Value);
                    break;

                case InstructionType.LoadArg:
                    if (instruction.Value < 0 || instruction.Value >= arguments.Count)
                        throw new ArgumentOutOfRangeException($"Argument index {instruction.Value} out of range");
                    stack.Push(arguments[instruction.Value]);
                    break;

                case InstructionType.Call:
                    if (program == null)
                        throw new InvalidOperationException("Cannot call functions without program context");
                    
                    var targetFunction = program.GetFunction(instruction.FunctionName!);
                    if (targetFunction == null)
                        throw new InvalidOperationException($"Function '{instruction.FunctionName}' not found");

                    // Pop arguments from stack (in reverse order)
                    var callArgs = new List<int>();
                    for (int i = 0; i < targetFunction.ParameterTypes.Count; i++)
                    {
                        if (stack.Count == 0)
                            throw new InvalidOperationException($"Insufficient arguments for function '{instruction.FunctionName}'");
                        callArgs.Insert(0, stack.Pop()); // Insert at beginning to reverse order
                    }

                    // Execute the function and push result
                    var result = ExecuteFunction(program, targetFunction, callArgs);
                    stack.Push(result);
                    break;

                case InstructionType.Add:
                    if (stack.Count < 2)
                        throw new InvalidOperationException("Add requires two operands on stack");
                    var b = stack.Pop();
                    var a = stack.Pop();
                    stack.Push(a + b);
                    break;

                case InstructionType.Sub:
                    if (stack.Count < 2)
                        throw new InvalidOperationException("Sub requires two operands on stack");
                    var sub_b = stack.Pop();
                    var sub_a = stack.Pop();
                    stack.Push(sub_a - sub_b);
                    break;

                case InstructionType.Mul:
                    if (stack.Count < 2)
                        throw new InvalidOperationException("Mul requires two operands on stack");
                    var mul_b = stack.Pop();
                    var mul_a = stack.Pop();
                    stack.Push(mul_a * mul_b);
                    break;

                case InstructionType.Div:
                    if (stack.Count < 2)
                        throw new InvalidOperationException("Div requires two operands on stack");
                    var div_b = stack.Pop();
                    var div_a = stack.Pop();
                    if (div_b == 0)
                        throw new DivideByZeroException("Division by zero");
                    stack.Push(div_a / div_b);
                    break;

                case InstructionType.Print:
                    if (stack.Count == 0)
                        throw new InvalidOperationException("Print requires one operand on stack");
                    Console.WriteLine($"Print: {stack.Peek()}");
                    break;

                case InstructionType.Return:
                    return stack.Count > 0 ? stack.Pop() : 0;
            }
        }

        return stack.Count > 0 ? stack.Peek() : 0;
    }
}
