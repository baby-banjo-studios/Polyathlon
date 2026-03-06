using System;
using UnityEngine;

public class GreenscreenController : MonoBehaviour
{
    private ControlMode controlMode;
    private TargetMode targetMode;

    private InputActions inputActions;
    [SerializeField]
    private SnapshotCamera snapCam;
    [SerializeField]
    private GreenScreenUI ui;
    [SerializeField]
    private GreenScreenAnimationController animationController;

    private Vector3 moveInput;

    public float movementSpeed = 1f, rotationSpeed = 30f;
    [SerializeField]
    private Transform cameraTransform, targetTransformParent;
    private Transform targetTransform;
    private Vector3 startingCameraPos, startingTargetPos;
    private Quaternion startingCameraRot, startingTargetRot;
    public int ImageWidth { get; set; } 
    public int ImageHeight { get; set; }

    private void Awake()
    {
        inputActions = new InputActions();

        inputActions.Greeenscreen.Move.performed += ctx => moveInput = ctx.ReadValue<Vector3>();
        inputActions.Greeenscreen.Move.canceled += ctx => moveInput = Vector3.zero;

        inputActions.Greeenscreen.Scrub.performed += ctx => ui.Scrub(ctx.ReadValue<float>() > 0);

        inputActions.Greeenscreen.TakeSnapshot.performed += ctx => TakeSnapshot(ImageWidth, ImageHeight);
        
        inputActions.Greeenscreen.SwapControl.performed += ctx => ToggleMode();
        inputActions.Greeenscreen.SwapTarget.performed += ctx => ToggleTarget();

        foreach (Transform t in targetTransformParent)
        {
            if (t.gameObject.activeInHierarchy)
            {
                targetTransform = t;
                break;
            }
        }

        startingCameraPos = cameraTransform.position;
        startingCameraRot = cameraTransform.rotation;
        startingTargetPos = targetTransform.position;
        startingTargetRot = targetTransform.rotation;

        controlMode = ControlMode.Position;
        targetMode = TargetMode.Camera;
        ui.SetMode(controlMode);
        ui.SetTargetMode(targetMode);
    }

    private void Start()
    {
        if (targetTransform.TryGetComponent(out Animator targetAnimator))
        {
            animationController.Initialize(targetAnimator);
        }
    }

    private void OnEnable()
    {
        inputActions.Greeenscreen.Enable();
    }

    private void OnDisable()
    {
        inputActions.Greeenscreen.Disable();
    }

    private void Update()
    {
        Transform target;
        if (targetMode == TargetMode.Camera)
        {
            target = cameraTransform;
        }
        else // if (targetMode == TargetMode.Target)
        {
            target = targetTransform;
        }

        if (controlMode == ControlMode.Position)
        {        
            Vector3 movement = (cameraTransform.right * moveInput.x) + (cameraTransform.up * moveInput.y) + (cameraTransform.forward * moveInput.z);
            target.Translate(movement * movementSpeed * Time.deltaTime, Space.World);
        }
        else if (controlMode == ControlMode.Rotation)
        {
            Vector3 rotation = new Vector3(moveInput.x, moveInput.y, moveInput.z);

            if (rotation.x != 0)
            {
                target.Rotate(Vector3.up, rotation.x * rotationSpeed * Time.deltaTime);
            }
            else if (rotation.y != 0)
            {
                target.Rotate(Vector3.back, rotation.y * rotationSpeed * Time.deltaTime);
            }
            else if (rotation.z != 0)
            {
                target.Rotate(Vector3.left, rotation.z * rotationSpeed * Time.deltaTime);
            }

            // rotation.x = 0;
            // rotation.y = 0;
            // rotation.z = 0;

            Debug.Log(moveInput);
        }
    }

    public void TakeSnapshot(int height, int width)
    {
        snapCam.SetResolution(width, height);
        snapCam.TakeSnapshot(false);
    }

    public void ToggleMode()
    {
        controlMode = EnumUtility.NextValue(controlMode);
        ui.SetMode(controlMode);
    }
    public void ToggleTarget()
    {
        targetMode = EnumUtility.NextValue(targetMode);
        ui.SetTargetMode(targetMode);
    }

    public void ResetPosition()
    {
        if (targetMode == TargetMode.Camera)
        {
            if (controlMode == ControlMode.Position)
            {
                cameraTransform.position = startingCameraPos;
            }
            else if (controlMode == ControlMode.Rotation)
            {
                cameraTransform.rotation = startingCameraRot;
            }
        }
        else if (targetMode == TargetMode.Target)
        {
            if (controlMode == ControlMode.Position)
            {
                targetTransform.position = startingTargetPos;
            }
            else if (controlMode == ControlMode.Rotation)
            {
                targetTransform.rotation = startingTargetRot;
            }
        }

    }
}