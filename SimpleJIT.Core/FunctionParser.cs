using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SimpleJIT.Core;

public static class FunctionParser
{
    private static readonly Regex FunctionHeaderRegex = new Regex(
        @"^\s*(\w+)\s+(\w+)\s*\(\s*(.*?)\s*\)\s*$", 
        RegexOptions.Compiled);

    public static Program ParseProgram(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        var lines = File.ReadAllLines(filePath);
        var program = new Program();
        
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            
            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//") || line.StartsWith("#"))
                continue;

            // Check for function header
            var match = FunctionHeaderRegex.Match(line);
            if (match.Success)
            {
                var function = ParseFunction(match, lines, ref i);
                program.Functions.Add(function);
            }
        }

        return program;
    }

    private static Function ParseFunction(Match headerMatch, string[] lines, ref int currentIndex)
    {
        var returnType = headerMatch.Groups[1].Value;
        var functionName = headerMatch.Groups[2].Value;
        var parametersString = headerMatch.Groups[3].Value;

        var function = new Function(functionName, returnType);

        // Parse parameters
        if (!string.IsNullOrWhiteSpace(parametersString))
        {
            var paramParts = parametersString.Split(',');
            foreach (var param in paramParts)
            {
                var trimmedParam = param.Trim();
                if (!string.IsNullOrWhiteSpace(trimmedParam))
                {
                    // For now, just store the type (e.g., "int")
                    function.ParameterTypes.Add(trimmedParam);
                }
            }
        }

        // Find opening brace
        currentIndex++;
        while (currentIndex < lines.Length && !lines[currentIndex].Trim().StartsWith("{"))
        {
            currentIndex++;
        }

        if (currentIndex >= lines.Length)
            throw new ArgumentException($"No opening brace found for function {functionName}");

        // Parse function body
        currentIndex++; // Skip opening brace
        while (currentIndex < lines.Length)
        {
            var line = lines[currentIndex].Trim();
            
            if (line.StartsWith("}"))
                break;

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//") || line.StartsWith("#"))
            {
                currentIndex++;
                continue;
            }

            // Parse instruction
            var instruction = ParseInstruction(line);
            if (instruction != null)
                function.Instructions.Add(instruction);

            currentIndex++;
        }

        return function;
    }

    private static Instruction? ParseInstruction(string line)
    {
        var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return null;

        var instructionName = parts[0].ToLower();

        return instructionName switch
        {
            "load" => parts.Length >= 2 && int.TryParse(parts[1], out int value) 
                ? new Instruction(InstructionType.Load, value) 
                : throw new ArgumentException($"Invalid load instruction: {line}"),
            "add" => new Instruction(InstructionType.Add),
            "sub" => new Instruction(InstructionType.Sub),
            "mul" => new Instruction(InstructionType.Mul),
            "div" => new Instruction(InstructionType.Div),
            "print" => new Instruction(InstructionType.Print),
            "ret" or "return" => new Instruction(InstructionType.Return),
            "call" => parts.Length >= 2 
                ? new Instruction(InstructionType.Call, parts[1]) 
                : throw new ArgumentException($"Invalid call instruction: {line}"),
            "loadarg" => parts.Length >= 2 && int.TryParse(parts[1], out int argIndex) 
                ? new Instruction(InstructionType.LoadArg, argIndex) 
                : throw new ArgumentException($"Invalid loadarg instruction: {line}"),
            _ => throw new ArgumentException($"Unknown instruction: {instructionName}")
        };
    }
}
