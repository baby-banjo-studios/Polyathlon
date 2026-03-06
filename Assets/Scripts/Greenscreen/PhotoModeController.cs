using UnityEngine;
using UnityEngine.InputSystem;

public class PhotoModeController : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField]
    private CameraController cameraController;
    [SerializeField]
    private SnapshotCamera snapshotCamera;
    [SerializeField]
    private PhotoModeUI ui;

    [SerializeField]
    private float speed = 5f;

    public Vector2 MoveXZ { get; set; }
    public float MoveUp { get; set; }
    public float MoveDown { get; set; }
    public Vector2 Look { get; set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        cameraController.gameObject.SetActive(false);
        ui.gameObject.SetActive(false);
    }

    private void Update()
    {
        AddMovement(MoveXZ.x, MoveXZ.y, MoveUp - MoveDown);
        cameraController.Rotate(Look.x, Look.y);
    }

    public void SetActive(bool enable)
    {
        cameraController.gameObject.SetActive(enable);
        ui.gameObject.SetActive(enable);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void AddMovement(float forward, float right, float up)
    {
        Vector3 translation = Vector3.zero;
        translation += right * cameraController.transform.forward;
        translation += forward * cameraController.transform.right;
        translation += up * cameraController.transform.up;

        transform.Translate(translation * speed * Time.unscaledDeltaTime, Space.World);
    }

    public void SetControlScheme(PlayerInput playerInput)
    {
        ui.UpdateControlsText(playerInput);
    }

    public void SetStartingPosition(CameraController originController)
    {
        transform.position = originController.ActualCameraPosition;
        cameraController.SetRotation(originController.transform.rotation);
    }

    public void TakeSnapshot()
    {
        Vector2Int widthHeight = ui.GetPhotoDimensions();
        snapshotCamera.SetResolution(widthHeight.x, widthHeight.y);
        snapshotCamera.TakeSnapshot(true);
        ui.TakeSnapshot();
    }

    public void CycleResolution()
    {
        ui.CycleResolution();
    }

    public void CycleAspectRatio()
    {
        ui.CycleAspectRatio();
    }

    public void HideUI()
    {
        ui.ToggleHideUI();
    }
}