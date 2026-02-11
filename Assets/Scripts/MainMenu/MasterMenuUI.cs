using System.Collections.Generic;
using UnityEngine;

public class MasterMenuUI : MonoBehaviour
{
    private MenuMode currentMode;
    private BaseMenuUI currentMenu;
    private Stack<MenuMode> previousMenus;
    
    [SerializeField]
    private BaseMenuUI titleUI;
    [SerializeField]
    private BaseMenuUI charSelectUI;
    [SerializeField]
    private BaseMenuUI raceSettingsUI;
    [SerializeField]
    private BaseMenuUI stageSelectUI;
    [SerializeField]
    private BaseMenuUI trainingSelectUI;
    [SerializeField]
    private BaseMenuUI settingsUI;
    [SerializeField]
    private BaseMenuUI polypediaUI;
    [SerializeField]
    private BaseMenuUI galleryUI;
    [SerializeField]
    private BaseMenuUI creditsUI;
    
    private bool inputSchemeRegistered = false;
    public List<MainMenuPlayer> players;

    private RaceSettings raceSettings;

    public ControlScheme PrimaryControlScheme { get; private set; }

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        previousMenus = new Stack<MenuMode>();
        raceSettings = FindAnyObjectByType<RaceSettings>();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        currentMode = MenuMode.Invalid;
    }

    private void Start()
    {
        if (RaceSettings.instance.IsMidRace)
        {
            TransitionToMode(MenuMode.StageSelect);
        }
        else
        {
            TransitionToMode(MenuMode.Title);
        }
    }

    public void TransitionToMode(MenuMode newMode)
    {
        titleUI.gameObject.SetActive(false);
        charSelectUI.gameObject.SetActive(false);
        raceSettingsUI.gameObject.SetActive(false);
        stageSelectUI.gameObject.SetActive(false);
        trainingSelectUI.gameObject.SetActive(false);
        settingsUI.gameObject.SetActive(false);
        polypediaUI.gameObject.SetActive(false);
        galleryUI.gameObject.SetActive(false);
        creditsUI.gameObject.SetActive(false);

        if (currentMode != MenuMode.Invalid)
        {
            previousMenus.Push(currentMode);
        }

        currentMode = newMode;
        switch (newMode)
        {
            case MenuMode.Title:
            {
                currentMenu = titleUI;
            }
            break;
            case MenuMode.CharacterSelect:
            {
                currentMenu = charSelectUI;
            }
            break;
            case MenuMode.RaceSettings:
            {
                currentMenu = raceSettingsUI;
            }
            break;
            case MenuMode.StageSelect:
            {
                currentMenu = stageSelectUI;
            }
            break;
            case MenuMode.TrainingSelect:
            {
                currentMenu = trainingSelectUI;
            }
            break;
            case MenuMode.Settings:
            {
                currentMenu = settingsUI;
            }
            break;
            case MenuMode.Polypedia:
            {
                currentMenu = polypediaUI;
            }
            break;
            case MenuMode.Gallery:
            {
                currentMenu = galleryUI;
            }
            break;
            case MenuMode.Credits:
            {
                currentMenu = creditsUI;
            }
            break;
        }

        currentMenu.gameObject.SetActive(true);
        currentMenu.Reset();
    }

    public void TransitionToPreviousMode()
    {
        if (previousMenus.Count > 0)
        {
            MenuMode prevMode = previousMenus.Pop();
            currentMode = MenuMode.Invalid;
            TransitionToMode(prevMode);
        }
    }

    public virtual void AnyKeyPressed(ControlScheme scheme)
    {
        if (!inputSchemeRegistered)
        {
            PrimaryControlScheme = scheme;
            inputSchemeRegistered = true;
        }

        currentMenu.AnyKeyPressed();
    }

    public void AddPlayer(MainMenuPlayer player, ControlScheme scheme)
    {
        player.PlayerNum = GetNextPlayerNum();

        AnyKeyPressed(scheme);
        players.Add(player);
        if (currentMode == MenuMode.CharacterSelect)
        {
            ((CharSelectUI)charSelectUI).AddPlayer(player);
        }
    }

    public void RemovePlayer(MainMenuPlayer player)
    {
        players.Remove(player);
    }

    private int GetNextPlayerNum()
    {
        HashSet<int> takenNums = new HashSet<int>();
        foreach (MainMenuPlayer player in players)
        {
            takenNums.Add(player.PlayerNum);
        }
        for (int i = 0; i < takenNums.Count + 1; i++)
        {
            if (!takenNums.Contains(i))
            {
                return i;
            }
        }
        // should never hit this
        Debug.Log("Somehow ended up missing valid player num lol");
        return players.Count + 1;
    }

    public bool IsLowestRemainingPlayer(int playerNum)
    {
        foreach (MainMenuPlayer player in players)
        {
            if (player.PlayerNum < playerNum)
            {
                return false;
            }
        }
        return true;
    }

    public void Navigate(MainMenuPlayer player, Vector2 input)
    {
        // make stick input discretely vertical or horizontal
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            input.y = 0;
        }
        else
        {
            input.x = 0;
        }
        
        currentMenu.Navigate(player, input);
    }

    public void Submit(MainMenuPlayer player)
    {
        currentMenu.Submit(player);
    }

    public void Cancel(MainMenuPlayer player)
    {
        currentMenu.Cancel(player);
    }

    public void Confirm(MainMenuPlayer player)
    {
        currentMenu.Confirm(player);
    }

}