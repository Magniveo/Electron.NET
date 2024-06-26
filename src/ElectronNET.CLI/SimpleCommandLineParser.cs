﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ElectronNET.CLI;

public class SimpleCommandLineParser
{
    public SimpleCommandLineParser()
    {
        Arguments = new Dictionary<string, string[]>();
    }

    public IDictionary<string, string[]> Arguments { get; }

    public void Parse(string[] args)
    {
        var currentName = "";
        var values = new List<string>();
        foreach (var arg in args)
            if (arg.StartsWith("/"))
            {
                if (currentName != "")
                    Arguments[currentName] = values.ToArray();
                values.Clear();
                currentName = arg.Substring(1);
            }
            else if (currentName == "")
            {
                Arguments[arg] = new string[0];
            }
            else
            {
                values.Add(arg);
            }

        if (currentName != "")
            Arguments[currentName] = values.ToArray();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(
            $"Arguments: \n\t{string.Join("\n\t", Arguments.Keys.Select(i => $"{i} = {string.Join(" ", Arguments[i])}"))}");
        Console.ResetColor();
    }

    public bool Contains(string name)
    {
        return Arguments.ContainsKey(name);
    }

    internal bool TryGet(string key, out string[] value)
    {
        value = null;
        if (!Contains(key)) return false;
        value = Arguments[key];
        return true;
    }
}