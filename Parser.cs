using System.IO;
using System;
using System.Collections.Generic;

namespace SimpleJIT;

public class Parser
{
    public static List<Instruction> ParseFile(string filePath)
    {
        var instructions = new List<Instruction>();
        var lines = File.ReadAllLines(filePath);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("//"))
                continue;

            var instruction = ParseLine(trimmedLine);
            if (instruction != null)
                instructions.Add(instruction);
        }

        return instructions;
    }

    private static Instruction? ParseLine(string line)
    {
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return null;

        var command = parts[0].ToLowerInvariant();

        return command switch
        {
            "load" => ParseLoad(parts),
            "add" => new Instruction(InstructionType.Add),
            "sub" => new Instruction(InstructionType.Sub),
            "mul" => new Instruction(InstructionType.Mul),
            "div" => new Instruction(InstructionType.Div),
            "print" => new Instruction(InstructionType.Print),
            "ret" or "return" => new Instruction(InstructionType.Return),
            _ => throw new ArgumentException($"Unknown instruction: {command}")
        };
    }

    private static Instruction ParseLoad(string[] parts)
    {
        if (parts.Length != 2)
            throw new ArgumentException("Load instruction requires a value");

        if (!long.TryParse(parts[1], out long value))
            throw new ArgumentException($"Invalid value for load instruction: {parts[1]}");

        return new Instruction(InstructionType.Load, value);
    }
}
