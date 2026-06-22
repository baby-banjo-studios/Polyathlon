using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class DebugConsole : MonoBehaviour
{
    public TextMeshProUGUI commandInputText;

    private List<ConsoleCommand> availableCommands;
    private Dictionary<string, ConsoleCommand> availableCommandsLookup;

    public int maxCommandHistorySize = 100;
    private LinkedList<string> commandHistory;
    private LinkedListNode<string> currentCommandHistorySelection = null;
    private StringBuilder currentCommand;
    private int cursorPos;
    public float cursorBlinkTime = 1.0f;
    private float cursorElapsedTime = 0.0f;
    private bool cursorVisible = true;
    private bool autocompleteNeedsToRun = true;
    private List<string> autocompleteOptions = new List<string>();
    private List<string> autocompleteFinalizedTokens = new List<string>();
    private ConsoleCommand autocompleteSelectedCommand = null;
    private int autoCompleteIndex = 0;
    public GameObject feedbackLinePrefab;
    public RectTransform feedbackLineParent;
    public float feedbackDisplayTime = 10f;
    public float feedbackFadeTime = 1f;
    private List<TextMeshProUGUI> feedbackInstances = new List<TextMeshProUGUI>();

    private Racer racer;    // only used to keep track of who opened console

    public LootTable allItems;
    private Vector3 startingGravity;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        commandHistory = new LinkedList<string>();
        currentCommand = new StringBuilder();

        // create command definitions
        availableCommands = new List<ConsoleCommand>
        {
            new ConsoleCommand("help",          "displays useful info about a command",     new CommandArgument[] { new CommandArgument<string>("command", "") }, HandleHelpCommand),
            new ConsoleCommand("list",          "lists all available commands",             new CommandArgument[] { },                                              HandleListCommand),
            new ConsoleCommand("noclip",        "disables all collision on a player",       new CommandArgument[] { new CommandArgument<int>("playerID", 1) },      HandleNoclipCommand),
            new ConsoleCommand("god",           "makes a player invincible",                new CommandArgument[] { new CommandArgument<int>("playerID", 1) },      HandleGodCommand),
            new ConsoleCommand("kill",          "instantly kills a racer",                  new CommandArgument[] { new CommandArgument<int>("racerID") },          HandleKillCommand),
            new ConsoleCommand("setspeed",      "multiplies a racer's movement speed",      new CommandArgument[] { new CommandArgument<int>("racerID", 1),
                                                                                            new CommandArgument<float>("speedScale")},                              HandleSetspeedCommand),
            new ConsoleCommand("equipitem",     "equips and item on a racer",               new CommandArgument[] { new CommandArgument<int>("racerID", 1),
                                                                                            new CommandArgument<string>("itemName")},                               HandleEquipItemCommand),
            new ConsoleCommand("useitem",       "uses an item on a racer immediately",      new CommandArgument[] { new CommandArgument<int>("racerID", 1),
                                                                                            new CommandArgument<string>("itemName")},                               HandleUseItemCommand),
            new ConsoleCommand("setmovement",   "changes a racer's movement mode",          new CommandArgument[] { new CommandArgument<int>("racerID", 1),
                                                                                            new CommandArgument<string>("movementMode")},                           HandleSetMovement),
            new ConsoleCommand("addplayer",     "adds a dummy player with its own screen",  new CommandArgument[] { },                                              HandleAddPlayer),
            new ConsoleCommand("setgravity",    "scales gravity by a multiplier",           new CommandArgument[] { new CommandArgument<float>("gravityScale") },   HandleSetGravity),
            new ConsoleCommand("reload",        "reloads the scene",                        new CommandArgument[] { },                                              HandleReload),

        };

        availableCommandsLookup = new Dictionary<string, ConsoleCommand>();
        foreach (ConsoleCommand command in availableCommands)
        {
            availableCommandsLookup[command.name] = command;
        }

        racer = GetComponentInParent<Racer>();

        startingGravity = Physics.gravity;
    }

    private void OnDisable()
    {
        foreach (TextMeshProUGUI feedbackLine in feedbackInstances)
        {
            Destroy(feedbackLine.gameObject);
        }
        feedbackInstances.Clear();
    }

    private void Update()
    {
        cursorElapsedTime += Time.unscaledDeltaTime;
        
        if (cursorElapsedTime > cursorBlinkTime)
        {
            cursorVisible = !cursorVisible;
            cursorElapsedTime = 0f;
        }

        string commandStr = currentCommand.ToString();
        string renderedStr = commandStr;    // if no cursor to render, display exact command input
        if (cursorVisible)
        {
            if (cursorPos < commandStr.Length)
            {
                string leftCommand = commandStr.Substring(0, cursorPos);
                string rightCommand = commandStr.Substring(cursorPos + 1, commandStr.Length - cursorPos - 1);
                renderedStr = leftCommand + "<u>" + commandStr[cursorPos] + "</u>" + rightCommand; 
            }
            else
            {
                renderedStr = commandStr + "<u><color=#00000000>_</color></u>"; 
            }
        }
        commandInputText.text = ">" + renderedStr;
    }

    private void OnGUI()
    {
        Event e = Event.current;
        if (e == null || e.type != EventType.KeyDown)
        {
            return;
        }

        switch (e.keyCode)
        {
            case KeyCode.Escape:
                {
                    e.Use();
                    RaceManager.ToggleDebugConsole((PlayerController)racer);
                }
                break;
            case KeyCode.BackQuote:
                {
                    // consume the `
                    e.Use();
                }
                break;
            case KeyCode.Return:
            case KeyCode.KeypadEnter:
                {
                    // submit command
                    string command = currentCommand.ToString();
                    SubmitCommand(command);
                    commandHistory.AddFirst(new LinkedListNode<string>(command));
                    if (commandHistory.Count > maxCommandHistorySize)
                    {
                        commandHistory.RemoveLast();
                    }
                    currentCommandHistorySelection = null;
                    currentCommand.Clear();
                    cursorPos = 0;
                    cursorElapsedTime = 0f;
                    cursorVisible = true;
                    autocompleteNeedsToRun = true;
                }
                break;
            case KeyCode.Backspace:
                {
                    // delete character before cursor
                    if (cursorPos > 0)
                    {
                        currentCommand.Remove(cursorPos - 1, 1);
                        cursorPos--;
                        cursorElapsedTime = 0f;
                        cursorVisible = true;
                        autocompleteNeedsToRun = true;
                    }
                }
                break;
            case KeyCode.Delete:
                {
                    // delete character after cursor
                    if (currentCommand.Length > 0 && cursorPos < currentCommand.Length)
                    {
                        currentCommand.Remove(cursorPos, 1);
                        cursorElapsedTime = 0f;
                        cursorVisible = true;
                        autocompleteNeedsToRun = true;
                    }
                }
                break;
            case KeyCode.Tab:
                {
                    // autocomplete
                    if (autocompleteNeedsToRun)
                    {
                        // populate autocomplete options if first time hitting tab
                        autocompleteNeedsToRun = false;
                        autocompleteOptions.Clear();
                        autocompleteFinalizedTokens.Clear();
                        autoCompleteIndex = -1;
                        string[] commandToks = currentCommand.ToString().Split(' ');
                        if (commandToks.Length == 1)
                        {
                            string partialCommmand = commandToks[0];
                            // prune list of commands
                            foreach (ConsoleCommand command in availableCommands)
                            {
                                if (command.name.StartsWith(partialCommmand, StringComparison.OrdinalIgnoreCase))
                                {
                                    autocompleteOptions.Add(command.name);
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < commandToks.Length - 1; i++)
                            {
                                autocompleteFinalizedTokens.Add(commandToks[i]);
                            }
                            string partialArgument = commandToks[commandToks.Length - 1];
                            // based on command, cycle through predetermined lists of acceptable arguments
                            string commandName = autocompleteFinalizedTokens[0];
                            if (availableCommandsLookup.TryGetValue(commandName, out ConsoleCommand command))
                            {
                                int argumentIndex = commandToks.Length - 2;
                                if (command.arguments.Length > argumentIndex)
                                {
                                    CommandArgument argument = command.arguments[argumentIndex];
                                    if (argument.name == "itemName")
                                    {
                                        foreach (ItemRegistry registry in allItems.GetAllItems())
                                        {
                                            string itemCommandName = registry.name.Replace(" ", "");
                                            if (itemCommandName.StartsWith(partialArgument, StringComparison.OrdinalIgnoreCase))
                                            {
                                                autocompleteOptions.Add(itemCommandName);
                                            }
                                        }
                                    }
                                    if (argument.name == "movementMode")
                                    {
                                        foreach (Movement.Mode mode in Enum.GetValues(typeof(Movement.Mode)))
                                        {
                                            if (mode < Movement.Mode.Noclip)
                                            {
                                                string modeName = mode.ToString().ToLower();
                                                if (modeName.StartsWith(partialArgument, StringComparison.OrdinalIgnoreCase))
                                                {
                                                    autocompleteOptions.Add(modeName);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    // cycle through options if not first time hitting tab
                    if (autocompleteOptions.Count > 0)
                    {
                        autoCompleteIndex++;
                        autoCompleteIndex %= autocompleteOptions.Count;
                        currentCommand.Clear();
                        foreach (string tok in autocompleteFinalizedTokens)
                        {
                            currentCommand.Append(tok);
                            currentCommand.Append(" ");
                        }
                        currentCommand.Append(autocompleteOptions[autoCompleteIndex]);
                        cursorElapsedTime = 0f;
                        cursorPos = currentCommand.Length;
                        cursorVisible = true;
                    }
                }
                break;
            case KeyCode.LeftArrow:
                {
                    // move cursor left
                    if (cursorPos > 0)
                    {
                        cursorPos--;
                        cursorElapsedTime = 0f;
                        cursorVisible = true;
                    }
                }
                break;
            case KeyCode.RightArrow:
                {
                    // move cursor right
                    if (cursorPos < currentCommand.Length)
                    {
                        cursorPos++;
                        cursorElapsedTime = 0f;
                        cursorVisible = true;
                    }
                }
                break;
            case KeyCode.UpArrow:
                {
                    // select previous command
                    if (commandHistory.Count > 0)
                    {
                        if (currentCommandHistorySelection == null)
                        {
                            currentCommandHistorySelection = commandHistory.First;
                        }
                        else if (currentCommandHistorySelection.Next != null)
                        {
                            currentCommandHistorySelection = currentCommandHistorySelection.Next;
                        }
                        currentCommand.Clear();
                        currentCommand.Append(currentCommandHistorySelection.Value);
                        cursorElapsedTime = 0f;
                        cursorPos = currentCommand.Length;
                        cursorVisible = true;
                        autocompleteNeedsToRun = true;
                    }
                }
                break;
            case KeyCode.DownArrow:
                {
                    // select next command
                    if (currentCommandHistorySelection != null)
                    {
                        currentCommandHistorySelection = currentCommandHistorySelection.Previous;
                        currentCommand.Clear();
                        if (currentCommandHistorySelection != null)
                        {
                            currentCommand.Append(currentCommandHistorySelection.Value);
                        }
                        cursorElapsedTime = 0f;
                        cursorPos = currentCommand.Length;
                        cursorVisible = true;
                        autocompleteNeedsToRun = true;
                    }
                }
                break;
            default:
                {
                    if (e.character != '\0' && e.character != '`' && !char.IsControl(e.character))
                    {
                        e.Use();
                        currentCommand.Insert(cursorPos, e.character);
                        cursorPos++;
                        cursorElapsedTime = 0f;
                        cursorVisible = true;
                        autocompleteNeedsToRun = true;
                    }
                }
                break;
        }
    }

    private void SubmitCommand(string commandString)
    {
        string[] commandToks = commandString.Split(' ');
        if (commandToks.Length > 0)
        {
            string commandName = commandToks[0];
            if (availableCommandsLookup.TryGetValue(commandName, out ConsoleCommand command))
            {
                CommandReturnCode returnCode = command.Execute(commandToks[1..]);
                switch (returnCode)
                {
                    case CommandReturnCode.Ok:
                    case CommandReturnCode.Failed:
                        {
                            // do nothing, individual commands print feedback
                        }
                        break;
                    case CommandReturnCode.NotEnoughArgs:
                    case CommandReturnCode.TooManyArgs:
                    case CommandReturnCode.InvalidArgType:
                        {
                            // display usage string to instruct user on how to better use command next time
                            DisplayFeedback(String.Format("Usage: {0}", command.GetUsage()));
                        }
                        break;
                    case CommandReturnCode.CantEvaluateDefaultArgs:
                        {
                            // this should really not be possible if commands are written with non-ambiguous parameters
                            DisplayFeedback(String.Format("Oops! Try specifying more arguments", command.name, command.arguments.Length));
                        }
                        break;
                }
            }
            else
            {
                DisplayFeedback(String.Format("Unrecognized command \"{0}\"", commandName));
            }
        }
    }

    private void DisplayFeedback(string message)
    {
        TextMeshProUGUI feedbackText = Instantiate(feedbackLinePrefab, feedbackLineParent).GetComponent<TextMeshProUGUI>();
        feedbackText.text = message;
        feedbackInstances.Add(feedbackText);
        LayoutRebuilder.ForceRebuildLayoutImmediate(feedbackLineParent);
        StartCoroutine(FadeFeedbackLine(feedbackText));
    }

    private IEnumerator FadeFeedbackLine(TextMeshProUGUI feedbackLine)
    {
        yield return new WaitForSecondsRealtime(feedbackDisplayTime);
        float elapsedTime = 0f;
        while (elapsedTime < feedbackFadeTime)
        {
            feedbackLine.alpha = 1.0f - (elapsedTime / feedbackFadeTime);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        feedbackLine.alpha = 0f;
        feedbackInstances.Remove(feedbackLine);
        Destroy(feedbackLine.gameObject);
    }

    private bool HandleHelpCommand(string[] args)
    {
        if (args[0] == "")
        {
            // general help about the command line
            DisplayFeedback("This command line can be used to execute commands to assist with debugging.\nIt also may be fun to play around with!\nTry running the \"list\" command to see all available commands or type \"help [command]\" to get help about a specific command");
        }
        else
        {
            string commandName = args[0];
            if (availableCommandsLookup.TryGetValue(commandName, out ConsoleCommand command))
            {
                DisplayFeedback(command.helpText);
                DisplayFeedback(String.Format("Usage: {0}", command.GetUsage()));
            }
            else
            {
                DisplayFeedback(String.Format("Unrecognized command \"{0}\"", commandName));
            }
        }
        return true;
    }

    private bool HandleListCommand(string[] args)
    {
        DisplayFeedback("Available commands:");
        foreach (ConsoleCommand command in availableCommands)
        {
            DisplayFeedback(String.Format("- {0}", command.name));
        }
        return true;
    }

    private bool HandleNoclipCommand(string[] args)
    {
        int playerIndex = Int32.Parse(args[0]) - 1;
        PlayerController player = RaceManager.GetPlayerByIndex(playerIndex);
        if (player == null)
        {
            DisplayFeedback(String.Format("Invalid player ID {0}", playerIndex + 1));
            return false;
        }

        if (player.movementMode == Movement.Mode.Noclip)
        {
            player.SetMovementMode(player.prevMovementMode);
            DisplayFeedback(String.Format("Enabled noclip for player {0}", playerIndex + 1));
        }
        else
        {
            player.SetMovementMode(Movement.Mode.Noclip);
            DisplayFeedback(String.Format("Disabled noclip for player {0}", playerIndex + 1));
        }
        return true;
    }

    
    private bool HandleGodCommand(string[] args)
    {
        int playerIndex = Int32.Parse(args[0]) - 1;
        PlayerController player = RaceManager.GetPlayerByIndex(playerIndex);
        if (player == null)
        {
            DisplayFeedback(String.Format("Invalid player ID {0}", playerIndex + 1));
            return false;
        }
        player.invincible = !player.invincible;
        if (player.invincible)
        {
            DisplayFeedback(String.Format("Enabled god mode for player {0}", playerIndex + 1));
        }
        else
        {
            DisplayFeedback(String.Format("Disabled god mode for player {0}", playerIndex + 1));
        }
        return true;
    }

    private bool HandleKillCommand(string[] args)
    {
        int racerIndex = Int32.Parse(args[0]) - 1;
        Racer target = RaceManager.GetRacerByIndex(racerIndex);
        if (target == null)
        {
            DisplayFeedback(String.Format("Invalid racer ID {0}", racerIndex + 1));
            return false;
        }
        target.Die(false);
        DisplayFeedback(String.Format("Killed racer ID {0}", racerIndex + 1));
        return true;
    }

    private bool HandleSetspeedCommand(string[] args)
    {
        int racerIndex = Int32.Parse(args[0]) - 1;
        float speedMultiplier = Single.Parse(args[1]);
        Racer target = RaceManager.GetRacerByIndex(racerIndex);
        if (target == null)
        {
            DisplayFeedback(String.Format("Invalid racer ID {0}", racerIndex + 1));
            return false;
        }
        target.SetPermanentSpeedScale(speedMultiplier);
        DisplayFeedback(String.Format("Set racer {0}'s speed to {1}x", racerIndex + 1, speedMultiplier));
        return true;
    }
    private bool HandleEquipItemCommand(string[] args)
    {
        int racerIndex = Int32.Parse(args[0]) - 1;
        string itemName = args[1].ToLower();
        Racer target = RaceManager.GetRacerByIndex(racerIndex);
        if (target == null)
        {
            DisplayFeedback(String.Format("Invalid racer ID {0}", racerIndex + 1));
            return false;
        }
                
        if (!EquipItemHelper(target, itemName, out _))
        {
            return false;
        }
        DisplayFeedback(String.Format("Equipped racer {0} with {1}", racerIndex + 1, itemName));
        return true;
    }

    private bool HandleUseItemCommand(string[] args)
    {
        int racerIndex = Int32.Parse(args[0]) - 1;
        string itemName = args[1].ToLower();
        Racer target = RaceManager.GetRacerByIndex(racerIndex);
        if (target == null)
        {
            DisplayFeedback(String.Format("Invalid racer ID {0}", racerIndex + 1));
            return false;
        }

        if (!EquipItemHelper(target, itemName, out Item equippedItem))
        {
            return false;
        }   

        equippedItem.Use(target);

        DisplayFeedback(String.Format("Used {0} on racer {1}", itemName, racerIndex + 1));
        return true;
    }

    private bool EquipItemHelper(Racer racer, string itemName, out Item equippedItem)
    {        

        Item item = null;
        foreach (ItemRegistry registry in allItems.GetAllItems())
        {
            if (registry.name.ToLower() == itemName ||
                registry.displayName.Replace(" ", "").ToLower() == itemName)
            {
                item = registry.itemPrefab;
                break;
            }
        }
        if (item == null)
        {
            DisplayFeedback(String.Format("Invalid item {0}", itemName));
            equippedItem = null;
            return false;
        }

        racer.EquipItem(item);

        equippedItem = item;
        return true;
    }

    private bool HandleSetMovement(string[] args)
    {
        int racerIndex = Int32.Parse(args[0]) - 1;
        string submittedModeName = args[1].ToLower();
        Racer target = RaceManager.GetRacerByIndex(racerIndex);
        if (target == null)
        {
            DisplayFeedback(String.Format("Invalid racer ID {0}", racerIndex + 1));
            return false;
        }
        Movement.Mode resolvedMovementMode = Movement.Mode.None;
        foreach (Movement.Mode mode in Enum.GetValues(typeof(Movement.Mode)))
        {
            if (mode < Movement.Mode.Noclip)
            {
                string modeName = mode.ToString().ToLower();
                if (submittedModeName == modeName)
                {
                    resolvedMovementMode = mode;
                    break;
                }
            }
        }
        if (resolvedMovementMode == Movement.Mode.None)
        {
            DisplayFeedback(String.Format("Unrecognized movement mode {0}", submittedModeName));
            return false;
        }
        target.SetMovementMode(resolvedMovementMode);
        DisplayFeedback(String.Format("Set racer {0}'s movement mode to {1}", racerIndex + 1, resolvedMovementMode.ToString()));
        return true;
    }

    private bool HandleAddPlayer(string[] args)
    {
        // no args
        if (!RaceManager.AddDummyPlayer())
        {
            DisplayFeedback("Failed to add player");
        }
        DisplayFeedback("Added dummy player");
        return true;
    }

    private bool HandleSetGravity(string[] args)
    {
        float gravityScale = Single.Parse(args[0]);
        Physics.gravity = startingGravity * gravityScale;
        DisplayFeedback(String.Format("Set gravity to {0} m/s<sup>2</sup>", Physics.gravity.y));
        return true;
    }

    private bool HandleReload(string[] args)
    {
        // no args
        Time.timeScale = 1f;
        Physics.gravity = startingGravity;
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
        return true;
    }

}