using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PhotoModeUI : MonoBehaviour
{
    [SerializeField]
    private GameObject controlsPanel;
    [SerializeField]
    private TextMeshProUGUI lookText, moveText, upText, downText, photoText, resolutionText, aspectRatioText, hideText, exitText;

    [SerializeField]
    private Image topShutter, bottomShutter;
    [SerializeField]
    private float shutterCloseTime, shutterFadeTime;
    [SerializeField]
    private AudioClip shutterClip;
    private Color transparent, opaque;
    private Vector2 maxScreenSize;
    private AudioSource audioSource;
    private bool displayUI = true;

    private PhotoModeResolution currentResolution;
    private PhotoModeAspectRatio currentAspectRatio;    
    [SerializeField]
    private TextMeshProUGUI resolutionDisplayText, aspectRatioDisplayText;
    [SerializeField]
    private RectTransform leftBorder, rightBorder, topBorder, bottomBorder;
    private GameObject borderParent;


    private void Awake()
    {
        CanvasScaler canvasScaler = GetComponentInParent<CanvasScaler>();
        if (canvasScaler != null)
        {
            maxScreenSize = canvasScaler.referenceResolution;
        }
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("PhotoModeUI does not have an AudioSource");
        }

        borderParent = leftBorder.parent.gameObject;

        currentResolution = PhotoModeResolution.Native;
        currentAspectRatio = PhotoModeAspectRatio.Free;
        SetResolutionText(currentResolution);
        SetAspectRatioText(currentAspectRatio);

        transparent = new Color(1f, 1f, 1f, 0f);
        opaque = new Color(1f, 1f, 1f, 1f);
    }

    private void OnEnable()
    {
        displayUI = true;
        controlsPanel.SetActive(true);

        topShutter.color = transparent;
        bottomShutter.color = transparent;        
        topShutter.rectTransform.sizeDelta = new Vector2(topShutter.rectTransform.sizeDelta.x, 0f);
        bottomShutter.rectTransform.sizeDelta = new Vector2(bottomShutter.rectTransform.sizeDelta.x, 0f);
        RedrawBorders();
    }

    public void UpdateControlsText(PlayerInput playerInput)
    {
        InputActionMap actionMap = playerInput.actions.FindActionMap("PhotoMode");
        string lookButton =         GamepadUtility.GetButtonFromInput(actionMap.FindAction("Look"), playerInput.currentControlScheme);
        string moveButton =         GamepadUtility.GetButtonFromInput(actionMap.FindAction("Movement"), playerInput.currentControlScheme);
        string upButton =           GamepadUtility.GetButtonFromInput(actionMap.FindAction("Up"), playerInput.currentControlScheme);
        string downButton =         GamepadUtility.GetButtonFromInput(actionMap.FindAction("Down"), playerInput.currentControlScheme);
        string photoButton =        GamepadUtility.GetButtonFromInput(actionMap.FindAction("TakePhoto"), playerInput.currentControlScheme);
        string resolutionButton =   GamepadUtility.GetButtonFromInput(actionMap.FindAction("CycleResolution"), playerInput.currentControlScheme);
        string aspectRatioButton =  GamepadUtility.GetButtonFromInput(actionMap.FindAction("CycleAspectRatio"), playerInput.currentControlScheme);
        string hidebutton =         GamepadUtility.GetButtonFromInput(actionMap.FindAction("HideUI"), playerInput.currentControlScheme);
        string exitButton =         GamepadUtility.GetButtonFromInput(actionMap.FindAction("Pause"), playerInput.currentControlScheme);

        lookText.text =         string.Format("[{0}] Look", lookButton);
        moveText.text =         string.Format("[{0}] Move", moveButton);
        upText.text =           string.Format("[{0}] Up", upButton);
        downText.text =         string.Format("[{0}] Down", downButton);
        photoText.text =        string.Format("[{0}] Take Photo", photoButton);
        resolutionText.text =   string.Format("[{0}] Cycle Resolution", resolutionButton);
        aspectRatioText.text =  string.Format("[{0}] Cycle Aspect Ratio", aspectRatioButton);
        hideText.text =         string.Format("[{0}] Hide UI", hidebutton);
        exitText.text =         string.Format("[{0}] Exit Photo Mode", exitButton);
    }

    public void ToggleHideUI()
    {
        displayUI = !displayUI;
        controlsPanel.SetActive(displayUI);
        borderParent.SetActive(displayUI);
    }

    public void TakeSnapshot()
    {
        StartCoroutine(ShuttersCoroutine());
    }

    private IEnumerator ShuttersCoroutine()
    {
        topShutter.color = opaque;
        bottomShutter.color = opaque;

        float elapsedTime = 0f;
        while (elapsedTime < shutterCloseTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float height = Mathf.Lerp(0f, maxScreenSize.y / 2f, elapsedTime / shutterCloseTime);
            topShutter.rectTransform.sizeDelta = new Vector2(topShutter.rectTransform.sizeDelta.x, height);
            bottomShutter.rectTransform.sizeDelta = new Vector2(bottomShutter.rectTransform.sizeDelta.x, height);
            yield return null;
        }

        topShutter.rectTransform.sizeDelta = new Vector2(topShutter.rectTransform.sizeDelta.x, maxScreenSize.y);
        bottomShutter.rectTransform.sizeDelta = new Vector2(bottomShutter.rectTransform.sizeDelta.x, maxScreenSize.y);

        audioSource.clip = shutterClip;
        audioSource.Play();

        elapsedTime = 0f;
        while (elapsedTime < shutterFadeTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            Color col = Color.Lerp(opaque, transparent, elapsedTime / shutterFadeTime);
            topShutter.color = col;
            bottomShutter.color = col;
            yield return null;
        }
        topShutter.color = transparent;
        bottomShutter.color = transparent;
    }

    public void CycleResolution()
    {
        currentResolution++;
        if ((int)currentResolution >= Enum.GetNames(typeof(PhotoModeResolution)).Length)
        {
            currentResolution = 0;
        }
        SetResolutionText(currentResolution);
    }

    private void SetResolutionText(PhotoModeResolution resolution)
    {
        string resolutionStr = "";
        switch (resolution)
        {
            case PhotoModeResolution.Native:
                {
                    resolutionStr = "Native";
                }
                break;
            case PhotoModeResolution.Ten_Eighty_P:
                {
                    resolutionStr = "1080p";
                }
                break;
            case PhotoModeResolution.Four_K:
                {
                    resolutionStr = "4k";
                }
                break;
        }
        resolutionDisplayText.text = String.Format("Resolution: {0}", resolutionStr);
    }

    public void CycleAspectRatio()
    {
        currentAspectRatio++;
        if ((int)currentAspectRatio >= Enum.GetNames(typeof(PhotoModeAspectRatio)).Length)
        {
            currentAspectRatio = 0;
        }
        RedrawBorders();
        SetAspectRatioText(currentAspectRatio);
    }

    private void SetAspectRatioText(PhotoModeAspectRatio aspectRatio)
    {
        string aspectRatioStr = "";
        switch (aspectRatio)
        {
            case PhotoModeAspectRatio.Free:
                {
                    aspectRatioStr = "Free";
                }
                break;
            case PhotoModeAspectRatio.Square:
                {
                    
                    aspectRatioStr = "Square";
                }
                break;
            case PhotoModeAspectRatio.Four_Three:
                {
                    aspectRatioStr = "4:3";
                }
                break;
            case PhotoModeAspectRatio.Sixteen_Nine:
                {
                    aspectRatioStr = "16:9";
                }
                break;
            case PhotoModeAspectRatio.Sixteen_Ten:
                {
                    aspectRatioStr = "16:10";
                }
                break;
            case PhotoModeAspectRatio.Three_Four:
                {
                    aspectRatioStr = "3:4";
                }
                break;
            case PhotoModeAspectRatio.Nine_Sixteen:
                {
                    aspectRatioStr = "9:16";
                }
                break;
            case PhotoModeAspectRatio.Ten_Sixteen:
                {
                    aspectRatioStr = "10:16";
                }
                break;
        }
        aspectRatioDisplayText.text = String.Format("Aspect Ratio: {0}", aspectRatioStr);
    }

    public Vector2Int GetPhotoDimensions()
    {
        int nominalHeight = 0;
        int nominalWidth = 0;
        switch (currentResolution)
        {
            case PhotoModeResolution.Native:
                {
                    nominalHeight = Screen.height;
                    nominalWidth = Screen.width;
                }
                break;
            case PhotoModeResolution.Ten_Eighty_P:
                {
                    nominalHeight = 1080;
                    nominalWidth = 1920;
                }
                break;
            case PhotoModeResolution.Four_K:
                {
                    nominalHeight = 2160;
                    nominalWidth = 3840;
                }
                break;
        }

        int imageHeight = 0;
        int imageWidth = 0;
        switch (currentAspectRatio)
        {
            case PhotoModeAspectRatio.Free:
                {
                    if (currentResolution == PhotoModeResolution.Native)
                    {
                        imageHeight = Screen.height;
                        imageWidth = Screen.width;
                    }
                    else
                    {
                        double screenHWRatio = (double)Screen.height / (double)Screen.width;
                        if (screenHWRatio < 0)
                        {
                            // oriented landscape
                            imageHeight = (int)(nominalWidth / screenHWRatio);
                            imageWidth = nominalWidth;
                        }
                        else
                        {
                            // oriented portrait
                            imageHeight = nominalHeight;
                            imageWidth = (int)(nominalHeight / screenHWRatio);
                        }
                    }
                }
                break;
            case PhotoModeAspectRatio.Square:
                {
                    int minDimension = Math.Min(nominalHeight, nominalWidth);
                    imageHeight = minDimension;
                    imageWidth = minDimension;
                }
                break;
            case PhotoModeAspectRatio.Four_Three:
                {
                    imageHeight = nominalHeight;
                    imageWidth = (int)(nominalHeight * (4.0 / 3.0));
                }
                break;
            case PhotoModeAspectRatio.Sixteen_Nine:
                {
                    imageHeight = nominalHeight;
                    imageWidth = (int)(nominalHeight * (16.0 / 9.0));
                }
                break;
            case PhotoModeAspectRatio.Sixteen_Ten:
                {
                    imageHeight = nominalHeight;
                    imageWidth = (int)(nominalHeight * (16.0 / 10.0));
                }
                break;
            case PhotoModeAspectRatio.Three_Four:
                {
                    imageHeight = (int)(nominalHeight * (4.0 / 3.0));
                    imageWidth = nominalHeight;
                }
                break;
            case PhotoModeAspectRatio.Nine_Sixteen:
                {
                    imageHeight = (int)(nominalHeight * (16.0 / 9.0));
                    imageWidth = nominalHeight;
                }
                break;
            case PhotoModeAspectRatio.Ten_Sixteen:
                {
                    imageHeight = (int)(nominalHeight * (16.0 / 10.0));
                    imageWidth = nominalHeight;
                }
                break;
        }
        return new Vector2Int(imageWidth, imageHeight);
    }

    public void RedrawBorders()
    {
        Vector2Int widthHeight = GetPhotoDimensions();
        
        float photoRatio = widthHeight.x / (float)widthHeight.y;

        RectTransform parentRect = borderParent.GetComponent<RectTransform>();
        float parentWidth  = parentRect.rect.width;
        float parentHeight = parentRect.rect.height;

        float parentRatio = parentWidth / parentHeight;

        float frameWidth;
        float frameHeight;

        if (photoRatio > parentRatio)
        {
            // Fit to width
            frameWidth  = parentWidth;
            frameHeight = parentWidth / photoRatio;
        }
        else
        {
            // Fit to height
            frameHeight = parentHeight;
            frameWidth  = parentHeight * photoRatio;
        }

        float halfW = frameWidth * 0.5f;
        float halfH = frameHeight * 0.5f;

        topBorder.anchoredPosition = new Vector2(0f, halfH);
        topBorder.sizeDelta = new Vector2(frameWidth, 5f);
        bottomBorder.anchoredPosition = new Vector2(0f, -halfH);
        bottomBorder.sizeDelta = new Vector2(frameWidth, 5f);

        leftBorder.anchoredPosition = new Vector2(-halfW, 0f);
        leftBorder.sizeDelta = new Vector2(5f, frameHeight);
        rightBorder.anchoredPosition = new Vector2(halfW, 0f);
        rightBorder.sizeDelta = new Vector2(5f, frameHeight);
    }
}