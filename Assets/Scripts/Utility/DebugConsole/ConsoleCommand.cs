using System;
using System.Collections.Generic;
using UnityEditor.ShaderKeywordFilter;

public enum CommandReturnCode
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
    public string helpText;
    public CommandArgument[] arguments;
    public Func<string[], bool> callback;
    
    public int numRequiredArgs;

    public ConsoleCommand(string name, string helpText, CommandArgument[] arguments, Func<string[], bool> callback)
    {
        this.name = name;
        this.helpText = helpText;
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

    public string GetUsage()
    {
        string usageString = name;
        foreach (CommandArgument argument in arguments)
        {
            if (argument.required)
            {
                usageString += String.Format(" <{0}>", argument.name);
            }
            else
            {
                usageString += String.Format(" [{0}]", argument.name);
            }
        }
        return usageString;
    }

    public CommandReturnCode Execute(string[] providedArgs)
    {
        if (providedArgs.Length < numRequiredArgs)
        {
            return CommandReturnCode.NotEnoughArgs;
        }
        else if (providedArgs.Length > arguments.Length)
        {
            return CommandReturnCode.TooManyArgs;
        }

        // find missing arguments and populate with defaults
        string[] evaluatedArgs;
        if (providedArgs.Length == arguments.Length)
        {
            evaluatedArgs = providedArgs;
        }
        else if (numRequiredArgs == 0)
        {
            // no args provided, assume default for everything
            evaluatedArgs = new string[arguments.Length];
            for (int i = 0; i < arguments.Length; i++)
            {
                evaluatedArgs[i] = arguments[i].defaultValue;
            }
        }
        else if (numRequiredArgs == providedArgs.Length)
        {
            // only optional args missing, assume default for optional only
            evaluatedArgs = new string[arguments.Length];
            int j = 0;
            for (int i = 0; i < arguments.Length; i++)
            {
                if (arguments[i].required)
                {
                    evaluatedArgs[i] = providedArgs[j++];
                }
                else
                {
                    evaluatedArgs[i] = arguments[i].defaultValue;
                }
            }
        }
        else
        {
            // TBD evaluate missing arguments by inferencing type of existing arguments
            return CommandReturnCode.CantEvaluateDefaultArgs;
        }

        for (int i = 0; i < arguments.Length; i++)
        {
            if (!ValidateArgument(evaluatedArgs[i], arguments[i].type))
            {
                return CommandReturnCode.InvalidArgType;
            }
        }

        bool result = callback(evaluatedArgs);
        if (!result)
        {
            return CommandReturnCode.Failed;
        }

        return CommandReturnCode.Ok;
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