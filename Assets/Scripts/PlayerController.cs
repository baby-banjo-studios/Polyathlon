using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerController : Racer
{
    public CameraController cameraController;
    public float gamepadLookSensititvity = 20;
    public float mouseLookSensisitivity = 15;
    public float arrowKeysLookSensitivity = 20;
    private float keyboardSchemeSensitivity;
    private Vector2 look; 

    private InputActions inputActions;
    private PlayerInput playerInput;
    private ControlScheme controlScheme;
    public ControlScheme ControlScheme { get => controlScheme; }
    private UI ui;
    private VFX vfx;
    private bool canMove;
    private bool canLook = true;
    private int playerNumber = -1;

    private PhotoModeController photoModeController;
    private InputActionMap prevActionMap;

    /* -------------- INPUT EVENTS ---------------- */

    // Running
    public void OnRunningMovement(InputAction.CallbackContext ctx)
    {
        if (canMove && !RaceManager.IsPaused)
        {
            if (ctx.performed)
                move = ctx.ReadValue<Vector2>();
            else if (ctx.canceled)
                move = Vector2.zero;
        }
    }

    public void OnRunningLook(InputAction.CallbackContext ctx)
    {
        if (canLook && !RaceManager.IsPaused)
        {
            if (ctx.performed)
                look = ctx.ReadValue<Vector2>() * (ctx.control.device is Gamepad ? Time.deltaTime * gamepadLookSensititvity : keyboardSchemeSensitivity);
            else if (ctx.canceled)
                look = Vector2.zero;
        }
    }

    public void OnRunningJump(InputAction.CallbackContext ctx)
    {
        if (canMove && !RaceManager.IsPaused)
        {
            if (ctx.performed)
                movement.Jump(true);
            else if (ctx.canceled)
                movement.Jump(false);
        }
    }

    public void OnUseItem(InputAction.CallbackContext ctx)
    {
        if (!RaceManager.IsPaused)
        {
            if (RaceManager.RespawnOnUse)
            {
                RaceManager.RespawnPlayer(this);
            }
            else if (!dead && canMove && item != null)
            {
                if (ctx.performed)
                {
                    item.Use(this);
                }
            }
        }
    }

    // Swimming
    public void OnSwimmingMovement(InputAction.CallbackContext ctx)
    {
        if (canMove && !RaceManager.IsPaused)
        {
            if (ctx.performed)
                move = ctx.ReadValue<Vector2>();
            else if (ctx.canceled)
                move = Vector2.zero;
        }
    }

    public void OnSwimmingLook(InputAction.CallbackContext ctx)
    {
        if (canLook && !RaceManager.IsPaused)
        {
            if (ctx.performed)
                look = ctx.ReadValue<Vector2>() * (ctx.control.device is Gamepad ? Time.deltaTime * gamepadLookSensititvity : keyboardSchemeSensitivity);
            else if (ctx.canceled)
                look = Vector2.zero;
        }
    }

    public void OnSwimmingJump(InputAction.CallbackContext ctx)
    {
        if (canMove)
        {
            if (ctx.performed)
                movement.Jump(true);
            else if (ctx.canceled)
                movement.Jump(false);
        }
    }

    // Biking
    public void OnBikingMovement(InputAction.CallbackContext ctx)
    {
        if (canMove && !RaceManager.IsPaused)
        {
            if (ctx.performed)
                move = ctx.ReadValue<Vector2>();
            else if (ctx.canceled)
                move = Vector2.zero;
        }
    }

    public void OnBikingLook(InputAction.CallbackContext ctx)
    {
        if (canLook && !RaceManager.IsPaused)
        {
            if (ctx.performed)
                look = ctx.ReadValue<Vector2>() * (ctx.control.device is Gamepad ? Time.deltaTime * gamepadLookSensititvity : keyboardSchemeSensitivity);
            else if (ctx.canceled)
                look = Vector2.zero;
        }
    }

    public void OnBikingJump(InputAction.CallbackContext ctx)
    {
        if (canMove)
        {
            if (ctx.performed)
                movement.Jump(true);
            else if (ctx.canceled)
                movement.Jump(false);
        }
    }

    // Jetpacking
    public void OnJetpackingMovement(InputAction.CallbackContext ctx)
    {
        if (canMove && !RaceManager.IsPaused)
        {
            if (ctx.performed)
                move = ctx.ReadValue<Vector2>();
            else if (ctx.canceled)
                move = Vector2.zero;
        }
    }

    public void OnJetpackingLook(InputAction.CallbackContext ctx)
    {
        if (canLook && !RaceManager.IsPaused)
        {
            if (ctx.performed)
                look = ctx.ReadValue<Vector2>() * (ctx.control.device is Gamepad ? Time.deltaTime * gamepadLookSensititvity : keyboardSchemeSensitivity);
            else if (ctx.canceled)
                look = Vector2.zero;
        }
    }

    public void OnJetpackingJump(InputAction.CallbackContext ctx)
    {
        if (canMove && !RaceManager.IsPaused)
        {
            if (ctx.performed)
                movement.Jump(true);
            else if (ctx.canceled)
                movement.Jump(false);
        }
    }

    // Gliding
    public void OnGlidingMovement(InputAction.CallbackContext ctx)
    {
        if (canMove && !RaceManager.IsPaused)
        {
            if (ctx.performed)
                move = ctx.ReadValue<Vector2>();
            else if (ctx.canceled)
                move = Vector2.zero;
        }
    }

    public void OnGlidingLook(InputAction.CallbackContext ctx)
    {
        if (canLook && !RaceManager.IsPaused)
        {
            if (ctx.performed)
                look = ctx.ReadValue<Vector2>() * (ctx.control.device is Gamepad ? Time.deltaTime * gamepadLookSensititvity : keyboardSchemeSensitivity);
            else if (ctx.canceled)
                look = Vector2.zero;
        }
    }

    public void OnGlidingJump(InputAction.CallbackContext ctx)
    {
        if (canMove && !RaceManager.IsPaused)
        {
            if (ctx.performed)
                movement.Jump(true);
            else if (ctx.canceled)
                movement.Jump(false);
        }
    }

    // Wheeling
    public void OnWheelingMovement(InputAction.CallbackContext ctx)
    {
        if (canMove)
        {
            if (ctx.performed)
                move = ctx.ReadValue<Vector2>();
            else if (ctx.canceled)
                move = Vector2.zero;
        }
    }

    public void OnWheelingLook(InputAction.CallbackContext ctx)
    {
        if (canLook)
        {
            if (ctx.performed)
                look = ctx.ReadValue<Vector2>() * (ctx.control.device is Gamepad ? Time.deltaTime * gamepadLookSensititvity : keyboardSchemeSensitivity);
            else if (ctx.canceled)
                look = Vector2.zero;
        }
    }

    public void OnWheelingJump(InputAction.CallbackContext ctx)
    {
        if (canMove)
        {
            if (ctx.performed)
                movement.Jump(true);
            else if (ctx.canceled)
                movement.Jump(false);
        }
    }
    
    public void OnFinishAnyKeyPressed(InputAction.CallbackContext ctx)
    {
        if (!RaceManager.IsPaused)
        {
            Debug.Log("OnFinishAnyKeyPressed now!");
            RaceManager.ReturnToMenu();
        }
    }

    // This is only for exiting training while using a gamepad
    public void OnPause(InputAction.CallbackContext ctx)
    {
        if (ctx.performed == true && !isFinished)
        {
            // if (RaceManager.IsTrainingCourse)
            //     RaceManager.ReturnToMenu();
            RaceManager.TogglePause(this);
        }
    }

    public void OnPhotoModeMovement(InputAction.CallbackContext ctx)
    {
        if (photoModeController != null)
        {            
            if (ctx.performed)
            {
                photoModeController.MoveXZ = ctx.ReadValue<Vector2>();
            }
            else if (ctx.canceled)
            {
                photoModeController.MoveXZ = Vector2.zero;
            }
        }
    }

    public void OnPhotoModeUp(InputAction.CallbackContext ctx)
    {
        if (photoModeController != null)
        {            
            if (ctx.performed)
            {
                photoModeController.MoveUp = ctx.ReadValue<float>();
            }
            else if (ctx.canceled)
            {
                photoModeController.MoveUp = 0f;
            }
        }
    }

    public void OnPhotoModeDown(InputAction.CallbackContext ctx)
    {
        if (photoModeController != null)
        {            
            if (ctx.performed)
            {
                photoModeController.MoveDown = ctx.ReadValue<float>();
            }
            else if (ctx.canceled)
            {
                photoModeController.MoveDown = 0f;
            }
        }
    }

    public void OnPhotoModeLook(InputAction.CallbackContext ctx)
    {
        if (photoModeController != null)
        {            
            if (ctx.performed)
            {
                Vector2 s = ctx.ReadValue<Vector2>();
                photoModeController.Look = s * (ctx.control.device is Gamepad ? Time.unscaledDeltaTime * gamepadLookSensititvity : keyboardSchemeSensitivity);
            }
            else if (ctx.canceled)
            {
                photoModeController.Look = Vector2.zero;
            }
        }
    }

    public void OnPhotoModeTakePhoto(InputAction.CallbackContext ctx)
    {
        if (photoModeController != null)
        {
            if (ctx.performed)
            {
                photoModeController.TakeSnapshot();
            }        
        }
    }

    public void OnPhotoModeCycleResolution(InputAction.CallbackContext ctx)
    {
        if (photoModeController != null)
        {
            if (ctx.performed)
            {
                photoModeController.CycleResolution();
            }        
        }
    }

    public void OnPhotoModeCycleAspectRatio(InputAction.CallbackContext ctx)
    {
        if (photoModeController != null)
        {
            if (ctx.performed)
            {
                photoModeController.CycleAspectRatio();
            }        
        }
    }

    public void OnPhotoModeHideUI(InputAction.CallbackContext ctx)
    {
        if (photoModeController != null)
        {
            if (ctx.performed)
            {
                photoModeController.HideUI();
            }        
        }
    }

    public void OnPausedNavigate(InputAction.CallbackContext ctx)
    {
        if (RaceManager.IsPaused)
        {
            if (ctx.performed)
            {
                ui.OnNavigate(ctx.ReadValue<Vector2>());
            }
        }
    }
    public void OnPausedSubmit(InputAction.CallbackContext ctx)
    {
        if (RaceManager.IsPaused)
        {
            if (ctx.performed)
            {
                ui.OnSubmit();
            }
        }
    }

    public void OnPausedCancel(InputAction.CallbackContext ctx)
    {
        if (RaceManager.IsPaused)
        {
            if (ctx.performed)
            {
                ui.OnCancel();
            }
        }
    }

    

    protected override void Awake() 
    {
        base.Awake();

        playerInput = GetComponent<PlayerInput>();
        vfx = GetComponentInChildren<VFX>();
        ui = transform.GetComponentInChildren<UI>();
        //inputActions = new InputActions();
        //inputActions.Debug.Exit.performed += ctx => Application.Quit();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        keyboardSchemeSensitivity = mouseLookSensisitivity;
        controlScheme = ((InputControlScheme)playerInput.user.controlScheme).name == "Gamepad" ? ControlScheme.Gamepad : ControlScheme.Keyboard;
    }

    protected override void Start() 
    {
        base.Start();
        
       // inputActions.Disable();
        canMove = false;
        foreach(Movement m in movementOptions)
        {
            m.enabled = false;
            m.CameraController = cameraController;
        }

        base.Start();
        movement.enabled = false;
    }

    protected override void Update()
    {
        // when we press an arrow key, change the sensitivity to expect looking with arrow keys
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) ||
            Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            keyboardSchemeSensitivity = arrowKeysLookSensitivity;
        }
        // when we click the mouse, change the sensitivity to expect looking with mouse movement
        else if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            keyboardSchemeSensitivity = mouseLookSensisitivity;
        }
        base.Update();
        if (!RaceManager.IsPaused)
        {
            movement.RotateCamera(look.x, look.y);
        }
    }

    public void SetPlayerNumber(int newNum)
    {
        playerNumber = newNum;
    }

    public int GetPlayerNumber()
    {
        return playerNumber;
    }

    protected override void ReviveText()
    {
        if (!isFinished)
        {
            ui.ReviveText(true);
        }
    }

    public override void EquipItem(Item item)
    {
        base.EquipItem(item);
        ui.SetItemImage(item != null ? item.icon : null);
    }

    /*  called when the race officially starts after the countdown 
        enables movement */
    public override void StartRace()
    {
        canMove = true;
        movement.enabled = true;
    }

    /*  called when the racer crosses the finish line 
        disables movement controls, displays text */
    public override void FinishRace(bool force)
    {
        base.FinishRace(force);
        // disable all movement controls
        canMove = false;
        move = Vector2.zero;
        movement.Jump(false);
        // display text
        StartCoroutine(ui.FinishRace());
    }

    public override void RaceIsOver()
    {
        // Enable FinishLine action map
        playerInput.currentActionMap = playerInput.actions.actionMaps[8];
    }

    /*  updates player's movement mode and maxSpeed/locomotion accordingly */
    public override void SetMovementMode(Movement.Mode mode, bool initial = false)
    {
        Vector2 movePreserve = move;
        base.SetMovementMode(mode, initial);
        playerInput.currentActionMap = playerInput.actions.actionMaps[(int)mode];
        Debug.Log("currentActionMap " + playerInput.currentActionMap);
        move = movePreserve;
    }

    public void EnableDebugControls()
    {
        playerInput.actions.FindActionMap("Debug").Enable();
    }

    public override void SetTarget(Transform target)
    {
        vfx.SetTarget(target);
    }

    public void SetPlayerIndex(int playerIndex, int maxPlayers)
    {
        ui.SetScale(playerIndex, maxPlayers);
        cameraController.SetScale(playerIndex, maxPlayers);
    }

    public void OnGameStateChanged(GameState prevState, bool isPlayerInControl)
    {
        switch (RaceManager.CurrentGameState)
        {
            case GameState.Normal:
                {
                    if (prevActionMap != null)
                    {
                        playerInput.currentActionMap = prevActionMap;
                        prevActionMap = null;
                    }
                    ui.SetGameHUD(true);
                    ui.SetPauseMenu(false);
                    EnablePhotoMode(false);
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                break;
            case GameState.Paused:
                {
                    if (prevState == GameState.Normal)
                    {
                        prevActionMap = playerInput.currentActionMap;
                    }
                    if (isPlayerInControl)
                    {
                        playerInput.currentActionMap = playerInput.actions.actionMaps[11];  // Paused
                    }
                    ui.SetGameHUD(true);
                    EnablePhotoMode(false);
                    ui.SetPauseMenu(isPlayerInControl);
                    if (controlScheme == ControlScheme.Keyboard)
                    {
                        if (isPlayerInControl)
                        {
                            Cursor.visible = true;
                            Cursor.lockState = CursorLockMode.Confined;
                            Cursor.visible = true;
                        }
                        else
                        {
                            Cursor.visible = false;
                            Cursor.lockState = CursorLockMode.Locked;
                            Cursor.visible = false;
                        }
                    }
                }
                break;
            case GameState.PhotoMode:
                {
                    ui.SetGameHUD(false);
                    ui.SetPauseMenu(false);
                    EnablePhotoMode(isPlayerInControl);
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                break;
        }
    }

    public void EnablePhotoMode(bool enable)
    {
        if (enable)
        {
            photoModeController = RaceManager.PhotoModeController;
            photoModeController.SetActive(true);
            photoModeController.SetStartingPosition(cameraController);
            photoModeController.SetControlScheme(playerInput);
            playerInput.currentActionMap = playerInput.actions.actionMaps[10];  // Greenscreen
            cameraController.gameObject.SetActive(false);
        }
        else
        {
            if (photoModeController != null)
            {
                photoModeController.SetActive(false);
                photoModeController = null;
            }
            cameraController.gameObject.SetActive(true);
        }
    }

    protected override IEnumerator SpeedBoost(float magnitude, float duration)
    {
        vfx.SetSpeedLines(true);
        cameraController.SetZoom(true);
        yield return base.SpeedBoost(magnitude, duration);
        cameraController.SetZoom(false);
        vfx.SetSpeedLines(false);
    }

    public override void Die(bool emphasizeTorso, Vector3 newMomentum = default(Vector3))
    {
        base.Die(emphasizeTorso, newMomentum);
        Debug.Log("die");
        // Have the camera start following the ragdoll
        StartCoroutine(cameraController.FollowRagdoll());
        vfx.ShowDamage();
    }

    public override void Revive(bool forceRevive = false)
    {
        if (dead && (canRevive || forceRevive))
        {
            // have the camera stop following the ragdoll
            StartCoroutine(cameraController.FollowRagdoll());
        }
        ui.ReviveText(false);
        base.Revive(forceRevive);
    }

    protected override void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer != 8) // ignore collisions with projectiles
        {
            base.OnCollisionEnter(other);
            float mag;
            if (other.rigidbody != null)    // if the other thing is moving
            {
                mag = (velocityBeforePhysicsUpdate - other.rigidbody.linearVelocity).magnitude;
            }
            else    // if the other thing is stationary
            {
                mag = (velocityBeforePhysicsUpdate).magnitude;
            }
            if (mag > dieThreshold)
            {
                Debug.Log(gameObject.name + " hit " + other.gameObject.name + " at " + mag + " m/s and died");
                Die(false);
            }
        }
    }

    /*  debug method
        toggles time between 20% and 100% */
    private void SlowTime()
    {
        if (Time.timeScale == 1.0f)
        {
            Time.timeScale = 0.2f;
        }
        else
        {
            Time.timeScale = 1.0f;
        }
    }
}