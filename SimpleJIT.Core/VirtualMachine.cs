using System;
using System.Collections.Generic;

namespace SimpleJIT.Core;

public class VirtualMachine
{
    private Stack<int> stack = new Stack<int>();

    public int Execute(List<Instruction> instructions)
    {
        stack.Clear();

        foreach (var instruction in instructions)
        {
            switch (instruction.Type)
            {
                case InstructionType.Load:
                    stack.Push(instruction.Value);
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
                    break; // Return will be handled after the loop
            }
        }

        return stack.Count > 0 ? stack.Peek() : 0;
    }
}
