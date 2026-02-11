using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BaseMenuUI : MonoBehaviour
{
    protected MasterMenuUI mainMenuUI;
    [SerializeField]
    protected Selectable firstSelectable;
    protected RaceSettings raceSettings;
    protected bool usingKeyboardMouse;
    
    private bool receivedFirstNavEvent;

    protected virtual void Awake()
    {
        mainMenuUI = GetComponentInParent<MasterMenuUI>();
        raceSettings = FindFirstObjectByType<RaceSettings>();
    }
    protected virtual void Start()
    {
        
    }

    public virtual void AnyKeyPressed()
    {

    }

    protected virtual void OnEnable()
    {
        receivedFirstNavEvent = false;
        if (mainMenuUI.PrimaryControlScheme == ControlScheme.Gamepad && firstSelectable != null)
        {
            firstSelectable.Select();
        }
        else
        {
            EventSystem.current.sendNavigationEvents = false;
            EventSystem.current.SetSelectedGameObject(null);
            //Debug.Log("NO nav events");
        }
    }

    protected virtual void OnDisable()
    {

    }

    public virtual void Reset()
    {
    }

    public virtual void Navigate(MainMenuPlayer player, Vector2 input)
    {
        // explanation: navigation event will be sent 2x and we only want to enable selections on the 2nd one
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            firstSelectable.Select();
            EventSystem.current.sendNavigationEvents = false;
        }
        else if (!EventSystem.current.sendNavigationEvents)
        {
            EventSystem.current.sendNavigationEvents = true;
        }
    }

    

    public virtual void Submit(MainMenuPlayer player)
    {

    }

    public virtual void Cancel(MainMenuPlayer player)
    {
        mainMenuUI.TransitionToPreviousMode();
    }

    public virtual void Confirm(MainMenuPlayer player)
    {

    }
}