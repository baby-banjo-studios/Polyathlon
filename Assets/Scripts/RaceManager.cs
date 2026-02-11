using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public enum GameState
{
    Normal,
    Paused,
    PhotoMode
}

public class RaceManager : MonoBehaviour
{
    public static RaceManager instance;

    public Transform startingPositionsParent;
    private Transform[] startingPositions;
    private RaceSettings raceSettings;

    private List<(Racer, int, float)> positions;
    private List<(Racer, float)> finalPositions;
    private float elapsedTime;
    private GameState gameState;
    private PlayerController playerWhoPausedTheGame = null;
    private PhotoModeController photoModeController;
    private bool isRaceActive = false;
    private static bool canLoadMenu = true;

    private Racer[] racers;
    private List<PlayerController> playerControllers;
    private int racersAlreadyFinished;
    private int realPlayersInRace;

    public CheckpointChain chain;

    public GameObject resultsText;
    public TextMeshProUGUI placeText;
    public TextMeshProUGUI timeText;
    public Image panel;
    public TextMeshProUGUI menuText;

    public GameObject dummyUI;

    private GameObject contingencyItems;

    public static float ElapsedTime { get => instance.elapsedTime; }
    public static bool IsRaceActive { get => instance.isRaceActive; }
    public static GameState CurrentGameState { get => instance.gameState; }
    public static bool IsPaused { get => instance.gameState == GameState.Paused; }
    public static bool IsPhotoMode { get => instance.gameState == GameState.PhotoMode; }
    public static PhotoModeController PhotoModeController { get => instance.photoModeController; }
    public GameObject raceCourseTester;
    public bool dontAddRacers = false; // use this for non-racing test scenes
    [SerializeField]
    private bool isTrainingCourse = false;
    [SerializeField]
    private bool respawnOnUse = false;
    public static bool IsTrainingCourse { get => instance.isTrainingCourse; } // use this for training scenes
    public static bool RespawnOnUse { get => instance.respawnOnUse; } // use this for training scenes
    public static LootTable CurrLootTable
    {
        get
        {
            if (instance.raceSettings != null)
            {
                return instance.raceSettings.lootTable;   
            }
            return null;
        }
    }

    private bool testRun = false;

    private void Awake()
    {
        instance = this;
        
        photoModeController = FindAnyObjectByType<PhotoModeController>();
        if (photoModeController == null)
        {
            Debug.LogWarning("Photo Mode Controller not found, players will not be able to enter photo mode");
        }

        if (!dontAddRacers)
        {
            // race starter code
            raceSettings = FindAnyObjectByType<RaceSettings>();
            if (raceSettings == null)
            {
                Debug.Log("RaceSettings not found, instantiating a race settings for testing!");
                raceSettings = Instantiate(raceCourseTester).GetComponentInChildren<RaceSettings>();
            }

            testRun = raceSettings.testSettings;

            // get all the starting positions
            startingPositions = new Transform[startingPositionsParent.childCount];
            for (int i = 0; i < startingPositionsParent.childCount; i++)
            {
                startingPositions[i] = startingPositionsParent.GetChild(i);
            }

            // Get the racers
            // First instantiate the NPCs
            List<CharacterRegistry> npcChoices = new List<CharacterRegistry>();
            if (!isTrainingCourse)
            {
                npcChoices = raceSettings.GetNPCChoices();
                for (int i = 0; i < npcChoices.Count; i++)
                {
                    Racer racer = Instantiate(npcChoices[i].npcObj, startingPositions[i].position, startingPositions[i].rotation).GetComponent<Racer>();
                    // Change movement mode of NPCs if necessary
                    if (SceneManager.GetActiveScene().name == "Course 2")
                    {
                        racer.movementMode = Movement.Mode.GetOffTheBoat;
                    }
                    racer.name = npcChoices[i].displayName;
                }
            }
            // Next instantiate the players
            if (!testRun)
            {
                List<RaceSettings.PlayerChoice> playerChoices = raceSettings.PlayerChoices;
                for (int i = 0; i < playerChoices.Count; i++)
                {
                    PlayerInput newPlayer = PlayerInput.Instantiate(playerChoices[i].character.playerObj, playerChoices[i].playerNumber,
                                                                    playerChoices[i].controlScheme.ToString(), -1, playerChoices[i].inputDevices);
                    newPlayer.transform.position = startingPositions[i + npcChoices.Count].position;
                    newPlayer.transform.rotation = startingPositions[i + npcChoices.Count].rotation;

                    PlayerController playerRacer = newPlayer.GetComponent<PlayerController>();
                    playerRacer.name = playerChoices[i].character.displayName + " (P" + (playerChoices[i].playerNumber + 1) + ")";
                    playerRacer.SetPlayerNumber(playerChoices[i].playerNumber);
                    playerRacer.SetPlayerIndex(i, playerChoices.Count);
                    //newPlayer.GetComponentInChildren<UI>().SetScale(i, playerChoices.Count);

                    // remove excess audio listeners
                    if (i > 0)
                    {
                        Destroy(newPlayer.GetComponentInChildren<AudioListener>());
                    }
                }
                // activate dummy camera for 3 player splitscreen
                if (playerChoices.Count == 3)
                {
                    dummyUI.SetActive(true);
                }
                realPlayersInRace = playerChoices.Count;
            }
            else
            {
                Transform testPlayer = raceSettings.testCharacterGameObject.transform;
                testPlayer.position = startingPositions[npcChoices.Count].position;
                testPlayer.rotation = startingPositions[npcChoices.Count].rotation;
                realPlayersInRace = 1;

            }
            //Destroy(raceSettings.gameObject);
        }
    }

    private void Start() 
    {
        racers = FindObjectsByType<Racer>(FindObjectsSortMode.None);
        positions = new List<(Racer, int, float)>(racers.Length);
        finalPositions = new List<(Racer, float)>(racers.Length);
        
        playerControllers = new List<PlayerController>();
        foreach (Racer racer in racers)
        {
            if (racer is PlayerController player)
            {
                playerControllers.Add(player);
            }
        }

        if (!isTrainingCourse)
        {
            foreach(Racer r in racers)
            {
                r.nextCheckpoint = chain.GetFirstCheckpoint();
            }
        }
        if (resultsText != null)
            resultsText.SetActive(false);
        if (timeText != null)
            timeText.gameObject.SetActive(false);
        if (placeText != null)
            placeText.gameObject.SetActive(false);
        if (panel != null)
            panel.enabled = false;
        if (menuText != null)
            menuText.gameObject.SetActive(false);
        canLoadMenu = true;

        
        // see if there are contingency items in this scene and disable them if there are
        // for example, jetpacks
        contingencyItems = GameObject.FindWithTag("ContingencyItems");
        ActivateContingencyItems(false);
    }

    private void Update()
    {
        // if (!Application.isEditor && Input.GetKey(KeyCode.Escape))
        // {
        //     ReturnToMenu();
        // }
        // update positions of racers (1st, 2nd...)
        if (isRaceActive && !isTrainingCourse)
        {
            positions.Clear();
            foreach(Racer r in racers)
            {
                if (!r.isFinished)
                {
                    positions.Add((r, r.nextCheckpoint.seq, Vector3.SqrMagnitude(r.nextCheckpoint.transform.position - r.transform.position)));
                }
            }
            positions.Sort((a, b) => (b.Item2.CompareTo(a.Item2) == 0 ? a.Item3.CompareTo(b.Item3) : b.Item2.CompareTo(a.Item2)));

            // update time
            elapsedTime += UnityEngine.Time.deltaTime;
        }
    }

    public static void RespawnPlayer(PlayerController player)
    {
        int playerNumber = player.GetPlayerNumber();
        if (playerNumber == -1)
        {
            playerNumber = 0;
        }
        player.transform.position = instance.startingPositions[playerNumber].position;
        player.Land();
        player.SetMovementMode(Movement.Mode.Running);
        // Remove velocity
        player.Revive(true);
        Rigidbody playerRb = player.transform.GetComponent<Rigidbody>();
        playerRb.isKinematic = true;
        playerRb.isKinematic = false;   
    }

    public static void StartRace()
    {
        instance.isRaceActive = true;
        foreach (Racer r in instance.racers)
        {
            r.StartRace();
        }
    }

    public static void FinishRace(Racer racer, float extraTime = 0f)
    {
        Debug.Log("racer " + racer);
        instance.racersAlreadyFinished++;
        instance.finalPositions.Add((racer, instance.elapsedTime + extraTime));
        if (racer is PlayerController)
        {
            instance.realPlayersInRace--;
            if (instance.realPlayersInRace == 0)
            {
                foreach(Racer r in instance.racers)
                {
                    if (!r.isFinished && r is NPC)
                    {
                        r.FinishRace(true);
                    }
                }
                instance.StartCoroutine(instance.DisplayScores());
            }
        }
    }

    public static void SetGameState(GameState newState)
    {
        GameState prevState = instance.gameState;
        instance.gameState = newState;
        switch (newState)
        {
            case GameState.Normal:
                {
                    Time.timeScale = 1f;
                }
                break;
            case GameState.Paused:
                {
                    Time.timeScale = 0f;
                }
                break;
            case GameState.PhotoMode:
                {
                    Time.timeScale = 0f;
                }
                break;
        }
        foreach (PlayerController pc in instance.playerControllers)
        {
            pc.OnGameStateChanged(prevState, pc == instance.playerWhoPausedTheGame);
        }
    }

    public static bool TogglePause(PlayerController playerAttemptingToTogglePause)
    {
        if (instance.playerWhoPausedTheGame != null && playerAttemptingToTogglePause != instance.playerWhoPausedTheGame)
        {
            return false;
        }    

        switch (instance.gameState)
        {
            case GameState.Normal:
                {
                    instance.playerWhoPausedTheGame = playerAttemptingToTogglePause;
                    SetGameState(GameState.Paused);
                }
                break;
            case GameState.Paused:
                {
                    instance.playerWhoPausedTheGame = null;
                    SetGameState(GameState.Normal);
                }
                break;
            case GameState.PhotoMode:
                {
                    SetGameState(GameState.Paused);
                }
                break;
        }
        return true;
    }

    public static bool TogglePhotoMode()
    {
        if (IsPaused && instance.photoModeController != null)
        {
            // instance.isPhotoMode = !instance.isPhotoMode;
            // instance.playerWhoPausedTheGame.EnablePhotoMode(IsPhotoMode, instance.photoModeController);
            SetGameState(GameState.PhotoMode);
            return true;
        }
        return false;
    }

    public static int GetPosition(Racer racer)
    {
        for (int i = 0; i < instance.positions.Count; i++)
        {
            if (instance.positions[i].Item1 == racer)
            {
                return i + 1 + instance.racersAlreadyFinished;
            }
        }
        return 0;
    }

    private IEnumerator DisplayScores()
    {
        finalPositions.Sort((a, b) => (a.Item2.CompareTo(b.Item2)));
        yield return new WaitForSeconds(2f);
        string names = "";
        string times = "";
        for (int i = 0; i < racers.Length; i++)
        {
            names += (i+1) + (i < 9 ? "    " : "   ") + finalPositions[i].Item1.name;
          /*
            if (racers[i] is PlayerController)
            {
                names += "(P" + (((PlayerController)racers[i]).GetPlayerNumber() + 1) + ")";
            }
            */
            names += '\n';
            times += UI.FormatTime(finalPositions[i].Item2) + '\n';
        }
        placeText.text = names;
        timeText.text = times;
        resultsText.SetActive(true);
        timeText.gameObject.SetActive(true);
        placeText.gameObject.SetActive(true);
        panel.enabled = true;

        yield return new WaitForSeconds(2f);
        
        menuText.gameObject.SetActive(true);
        if (raceSettings.HasNextRace)
        {
            menuText.text = "Press any button to continue to next race";
        }
        else
        {
            menuText.text = "Press any button to return to menu";
        }
        
        foreach (Racer r in racers)
        {
            r.RaceIsOver();
        }
    }

    public static void ReturnToMenu()
    {
        if (IsRaceActive)
        {
            instance.isRaceActive = false;
            if (instance.raceSettings.HasNextRace)
            {
                instance.raceSettings.StartNextRace();
            }
            else
            {
                instance.raceSettings.EndRace();
                Debug.Log("return to main menu please!");
                if (canLoadMenu)
                {
                    canLoadMenu = false;
                    Debug.Log("Okay loading main menu!");
                    SceneManager.LoadScene("Main Menu");
                }
            }
        }
    }

    public static void ActivateContingencyItems(bool value)
    {
        // activate contingency items as soon as someone is targeted
        if (instance.contingencyItems != null)
        {
            instance.contingencyItems.SetActive(value);
        }
    }

    /*  if it looks stupid but it works, it ain't stupid */
    public static IList<Racer> GetListOfRacers()
    {
        return instance.racers;
    }
    public static Racer GetRacerOtherThanThisOne(Racer racer)
    {
        Racer chosen = racer;
        while (chosen == racer)
        {
            chosen = instance.racers[Random.Range(0, instance.racers.Length)];
        }
        return chosen;
    }

    public static Racer GetClosestRacerAheadOfThisOne(Racer racer)
    {
        Racer closestRacer = null;
        float closestDist = float.MaxValue;
        foreach (Racer otherRacer in instance.racers)
        {
            int myPlace = GetPosition(racer);
            int otherPlace = GetPosition(otherRacer);
            if (otherRacer != racer && (IsTrainingCourse || otherPlace < myPlace))
            {
                float dist = Vector3.Distance(racer.transform.position, otherRacer.transform.position);
                if (dist < closestDist)
                {
                    closestRacer = otherRacer;
                    closestDist = dist;
                }
            }
        }
        return closestRacer;
    }

    public static Racer GetClosestRacer(Vector3 position)
    {
        Racer closestRacer = null;
        float closestDist = float.MaxValue;
        foreach (Racer otherRacer in instance.racers)
        {
            float dist = Vector3.Distance(position, otherRacer.transform.position);
            if (dist < closestDist)
            {
                closestRacer = otherRacer;
                closestDist = dist;
            }
        }
        return closestRacer;
    }

    public static Racer GetHighestRacerOtherThanThisOne(Racer racer)
    {
        Racer highestRacer = null;
        int highestPlace = int.MinValue;
        foreach (Racer otherRacer in instance.racers)
        {
            int place = GetPosition(otherRacer);
            if (otherRacer != racer && place < highestPlace)
            {
                highestRacer = otherRacer;
                highestPlace = place;
            }
        }
        return highestRacer;
    }

    public static IEnumerable<Racer> GetRacersSortedByDistance(Vector3 refPosition)
    {
        // Dictionary<float, Racer> distancesAndRacers = new Dictionary<float, Racer>();
        // foreach (Racer racer in instance.racers)
        // {
        //     float dist = Vector3.Distance(racer.transform.position, refPosition);

        // }
        List<Racer> sortedRacers = new List<Racer>();
        foreach (Racer racer in instance.racers)
        {
            sortedRacers.Add(racer);
        }
        sortedRacers.Sort((x, y) => Vector3.Distance(x.transform.position, refPosition).CompareTo(Vector3.Distance(y.transform.position, refPosition)));
        
        return sortedRacers;
    }
}