using System.IO;
using System;
using System.Collections.Generic;

namespace SimpleJIT.Core;

public class Parser
{
    public static List<Instruction> ParseFile(string filePath)
    {
        var instructions = new List<Instruction>();
        var lines = File.ReadAllLines(filePath);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("//") || trimmedLine.StartsWith("#"))
                continue;

            var instruction = ParseLine(trimmedLine);
            if (instruction != null)
                instructions.Add(instruction);
        }

        return instructions;
    }

    private static Instruction? ParseLine(string line)
    {
        // Handle inline comments by removing everything after # or //
        int commentIndex = line.IndexOf('#');
        if (commentIndex == -1)
            commentIndex = line.IndexOf("//");
        
        if (commentIndex >= 0)
            line = line.Substring(0, commentIndex);
        
        var parts = line.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return null;

        var command = parts[0].ToLowerInvariant();

        return command switch
        {
            "load" => ParseLoad(parts),
            "add" => ParseSimpleInstruction(parts, InstructionType.Add, "add"),
            "sub" => ParseSimpleInstruction(parts, InstructionType.Sub, "sub"),
            "mul" => ParseSimpleInstruction(parts, InstructionType.Mul, "mul"),
            "div" => ParseSimpleInstruction(parts, InstructionType.Div, "div"),
            "print" => ParseSimpleInstruction(parts, InstructionType.Print, "print"),
            "ret" or "return" => ParseSimpleInstruction(parts, InstructionType.Return, command),
            _ => throw new ArgumentException($"Unknown instruction: {command}")
        };
    }

    private static Instruction ParseLoad(string[] parts)
    {
        if (parts.Length < 2)
            throw new ArgumentException("Load instruction requires a value");
        
        if (parts.Length > 2)
            throw new ArgumentException("Load instruction should have exactly one argument");

        if (!long.TryParse(parts[1], out long value))
            throw new ArgumentException($"Invalid value for load instruction: {parts[1]}");

        return new Instruction(InstructionType.Load, value);
    }

    private static Instruction ParseSimpleInstruction(string[] parts, InstructionType instructionType, string instructionName)
    {
        if (parts.Length > 1)
            throw new ArgumentException($"{char.ToUpper(instructionName[0]) + instructionName.Substring(1)} instruction should not have arguments");

        return new Instruction(instructionType);
    }
}
