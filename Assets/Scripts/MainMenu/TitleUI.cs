using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TitleUI : BaseMenuUI
{
    [SerializeField]
    private GameObject initialScreen;
    [SerializeField]
    private GameObject mainScreen;

    protected override void Start()
    {
        base.Start();
        initialScreen.SetActive(true);
        mainScreen.SetActive(false);
    }

    public override void AnyKeyPressed()
    {
        initialScreen.SetActive(false);
        mainScreen.SetActive(true);
        //Debug.Log("firstselected");
        if (mainMenuUI.PrimaryControlScheme == ControlScheme.Gamepad)
        {
            firstSelectable.Select();
        }
    }

    // protected override void OnEnable()
    // {
    //     // explicitly do nothing- we do NOT want first button to be selected by default
    // }

    public void OnPlayClicked()
    {
        raceSettings.mode = GameMode.Racing;
        mainMenuUI.TransitionToMode(MenuMode.CharacterSelect);
    }
    public void OnTrainingClicked()
    {
        raceSettings.mode = GameMode.Training;
        mainMenuUI.TransitionToMode(MenuMode.CharacterSelect);
    }

    public void OnSettingsClicked()
    {
        mainMenuUI.TransitionToMode(MenuMode.Settings);
    }

    public void OnPolypediaClicked()
    {
        mainMenuUI.TransitionToMode(MenuMode.Polypedia);
    }

    public void OnGalleryClicked()
    {
        mainMenuUI.TransitionToMode(MenuMode.Gallery);
    }

    public void OnCreditsClicked()
    {
        mainMenuUI.TransitionToMode(MenuMode.Credits);
    }

    public void OnQuitClicked()
    {
#if UNITY_EDITOR
        if(EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
        }
#else
        Application.Quit();
#endif
    }
}