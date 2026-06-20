using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text;
using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

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

    private Racer racer;    // only used to keep track of who opened console

    public LootTable allItems;

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
            new ConsoleCommand("noclip",    new CommandArgument[] { new CommandArgument<int>("playerID", 1) },  HandleNoclipCommand),
            new ConsoleCommand("god",       new CommandArgument[] { new CommandArgument<int>("playerID", 1) },  HandleGodCommand),
            new ConsoleCommand("setspeed",  new CommandArgument[] { new CommandArgument<int>("racerID", 1),
                                                                    new CommandArgument<float>("speedScale")},  HandleSetspeedCommand),
            new ConsoleCommand("equipitem", new CommandArgument[] { new CommandArgument<int>("racerID", 1),
                                                                    new CommandArgument<string>("item")},       HandleEquipItemCommand),
            new ConsoleCommand("useitem",   new CommandArgument[] { new CommandArgument<int>("racerID", 1),
                                                                    new CommandArgument<string>("item")},       HandleUseItemCommand),
            new ConsoleCommand("addplayer", new CommandArgument[] { },                                          HandleAddPlayer),
        };

        availableCommandsLookup = new Dictionary<string, ConsoleCommand>();
        foreach (ConsoleCommand command in availableCommands)
        {
            availableCommandsLookup[command.name] = command;
        }

        racer = GetComponentInParent<Racer>();
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
                }
                break;
            case KeyCode.Backspace:
                {
                    // delete character before cursor
                    if (currentCommand.Length > 0)
                    {
                        currentCommand.Remove(cursorPos - 1, 1);
                        cursorPos--;
                        cursorElapsedTime = 0f;
                        cursorVisible = true;
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
                    }
                }
                break;
            case KeyCode.Tab:
                {
                    // autocomplete
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
                    }
                }
                break;
            default:
                {
                    if (e.character != '\0' && e.character != '`' && !char.IsControl(e.character))
                    {
                        e.Use();
                        currentCommand.Append(e.character);
                        cursorPos++;
                        cursorElapsedTime = 0f;
                        cursorVisible = true;
                    }
                }
                break;
        }
    }

    private void UpdateDisplay()
    {

    }

    private void SubmitCommand(string commandString)
    {
        string[] commandToks = commandString.Split(' ');
        if (commandToks.Length > 0)
        {
            string commandName = commandToks[0];
            if (availableCommandsLookup.TryGetValue(commandName, out ConsoleCommand command))
            {
                command.Execute(commandToks[1..]);
            }
        }
    }

    private bool HandleNoclipCommand(string[] args)
    {
        int playerIndex = Int32.Parse(args[0]) - 1;
        PlayerController player = RaceManager.GetPlayerByIndex(playerIndex);
        if (player == null)
        {
            return false;
        }

        if (player.movementMode == Movement.Mode.Noclip)
        {
            player.SetMovementMode(player.prevMovementMode);
        }
        else
        {
            player.SetMovementMode(Movement.Mode.Noclip);
        }
        return true;
    }

    
    private bool HandleGodCommand(string[] args)
    {
        int playerIndex = Int32.Parse(args[0]) - 1;
        PlayerController player = RaceManager.GetPlayerByIndex(playerIndex);
        if (player == null)
        {
            return false;
        }
        player.invincible = !player.invincible;
        return true;
    }
    private bool HandleSetspeedCommand(string[] args)
    {
        int racerIndex = Int32.Parse(args[0]) - 1;
        float speedMultiplier = Single.Parse(args[1]);
        Racer target = RaceManager.GetRacerByIndex(racerIndex);
        if (target == null)
        {
            return false;
        }
        target.SetPermanentSpeedScale(speedMultiplier);
        return true;
    }
    private bool HandleEquipItemCommand(string[] args)
    {
        int racerIndex = Int32.Parse(args[0]) - 1;
        string itemName = args[1].ToLower();
        Racer target = RaceManager.GetRacerByIndex(racerIndex);
        if (target == null)
        {
            return false;
        }
                
        return EquipItemHelper(target, itemName, out _);
    }

    private bool HandleUseItemCommand(string[] args)
    {
        int racerIndex = Int32.Parse(args[0]) - 1;
        string itemName = args[1].ToLower();
        Racer target = RaceManager.GetRacerByIndex(racerIndex);
        if (target == null)
        {
            return false;
        }

        if (!EquipItemHelper(target, itemName, out Item equippedItem))
        {
            return false;
        }   

        equippedItem.Use(target);
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
            equippedItem = null;
            return false;
        }

        racer.EquipItem(item);

        equippedItem = item;
        return true;
    }

    private bool HandleAddPlayer(string[] args)
    {
        // no args
        RaceManager.AddDummyPlayer();
        return true;
    }

}