using System;
using System.Collections.Generic;
using UnityEditor.ShaderKeywordFilter;

public enum CommandReturnType
{
    Ok,
    Failed,
    NotEnoughArgs,
    TooManyArgs,
    CantEvaluateDefaultArgs,
    InvalidArgType,
};

public class CommandArgument
{
    public string name;
    public Type type;
    public bool required;
    public string defaultValue;
}

public class CommandArgument<T> : CommandArgument
{
    public CommandArgument(string name)
    {
        this.name = name;
        this.type = typeof(T);
        this.required = true;
    }

    public CommandArgument(string name, T defaultValue)
    {
        this.name = name;
        this.type = typeof(T);
        this.required = false;
        this.defaultValue = defaultValue.ToString();
    }
}

public class ConsoleCommand
{
    public string name;
    public CommandArgument[] arguments;
    public Func<string[], bool> callback;
    
    protected int numRequiredArgs;

    public ConsoleCommand(string name, CommandArgument[] arguments, Func<string[], bool> callback)
    {
        this.name = name;
        this.arguments = arguments;
        this.callback = callback;
        foreach (CommandArgument arg in arguments)
        {
            if (arg.required)
            {
                numRequiredArgs++;
            }
        }
    }

    public CommandReturnType Execute(string[] providedArgs)
    {
        if (providedArgs.Length < numRequiredArgs)
        {
            return CommandReturnType.NotEnoughArgs;
        }
        else if (providedArgs.Length > arguments.Length)
        {
            return CommandReturnType.TooManyArgs;
        }

        // find missing arguments and populate with defaults
        string[] evaluatedArgs;
        if (providedArgs.Length == arguments.Length)
        {
            evaluatedArgs = providedArgs;
        }
        else if (numRequiredArgs == 0)
        {
            evaluatedArgs = new string[arguments.Length];
            for (int i = 0; i < arguments.Length; i++)
            {
                evaluatedArgs[i] = arguments[i].defaultValue;
            }
        }
        else
        {
            // TBD evaluate missing arguments by inferencing type of existing arguments
            return CommandReturnType.CantEvaluateDefaultArgs;
        }

        for (int i = 0; i < arguments.Length; i++)
        {
            if (!ValidateArgument(evaluatedArgs[i], arguments[i].type))
            {
                return CommandReturnType.InvalidArgType;
            }
        }

        bool result = callback(evaluatedArgs);
        if (!result)
        {
            return CommandReturnType.Failed;
        }

        return CommandReturnType.Ok;
    }

    protected bool ValidateArgument(string arg, Type expectedType)
    {
        if (expectedType == typeof(string))
        {
            return true;
        }
        if (expectedType == typeof(int))
        {
            return int.TryParse(arg, out _);
        }
        if (expectedType == typeof(float))
        {
            return float.TryParse(arg, out _);
        }
        return false;
    }

}