using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.EventSystems;

public class MainMenuPlayer : MonoBehaviour
{
    public CharacterList characterList;
    public TextMeshPro nameText;
    public TextMeshPro playerNumText;
    public TextMeshPro readyText;
    private PlayerInput playerInput;
    private MasterMenuUI menuUI;
    private CharacterRegistry[] characters;
    private int characterIndex;
    private GameObject currentCharPreview;
    private bool canCycle = false;
    private string unreadyMessage; // displayed when the player hasn't said they're ready
    private ControlScheme controlScheme;
    
    public int PlayerNum { get; set; }
    public ControlScheme ControlScheme { get => controlScheme; }
    public EventSystem PlayerEventSystem { get => GetComponent<EventSystem>(); }
    public InputDevice[] InputDevices { get => playerInput.devices.ToArray(); }
    private void Awake()
    {
        
        playerInput = GetComponent<PlayerInput>();
        menuUI = FindFirstObjectByType<MasterMenuUI>();
    }

    private void Start()
    {
        transform.position += new Vector3(0, 0.36f, 0);
        canCycle = true;
        controlScheme = ((InputControlScheme)playerInput.user.controlScheme).name == "Gamepad" ? ControlScheme.Gamepad : ControlScheme.Keyboard;
        unreadyMessage = (controlScheme == ControlScheme.Gamepad ? "Ready? Press A!" : "Ready? Press Space!");
        readyText.text = unreadyMessage;
        menuUI.AddPlayer(this, controlScheme );
    }

    public void Exit()
    {
        menuUI.RemovePlayer(this);
        Destroy(this.gameObject);
    }

    // ----------------- INPUT EVENTS --------------------
    public void OnNavigate(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Vector2 vecVal = ctx.ReadValue<Vector2>();
            if (canCycle)
            {
                //Debug.Log(string.Format("x:{0}, y:{1}", vecVal.x, vecVal.y));
                menuUI.Navigate(this, vecVal);
                StartCoroutine(PreventSpeedyJoysticks());
            }
        }
    }

    public void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            menuUI.Submit(this);
        }
    }

    public void OnConfirmSelections(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            menuUI.Confirm(this);
        }
    }

    public void OnCancel(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            menuUI.Cancel(this);
        }
    }

    public void OnAnyKey(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            //Debug.Log("AnyKey");
            //menuUI.AnyKeyPressed();
        }
    }

    // ----------------- END INPUT EVENTS --------------------

    public int GetPlayerIndex()
    {
        return playerInput.playerIndex;
    }

    public bool IsPrimary()
    {
        if (PlayerNum == 0)
        {
            return true;
        }
        return menuUI.IsLowestRemainingPlayer(PlayerNum);
    }

    public RaceSettings.PlayerChoice GetPlayerChoice()
    {
        return new RaceSettings.PlayerChoice(PlayerNum, characters[characterIndex], controlScheme, playerInput.devices.ToArray());
    }

    // Joysticks on gamepads are gonna trigger CycleCharacer way too fast
    // if a buffer isn't put between each cycle
    private IEnumerator PreventSpeedyJoysticks()
    {
        canCycle = false;
        yield return new WaitForSeconds(0.18f);
        canCycle = true;
    }
}
