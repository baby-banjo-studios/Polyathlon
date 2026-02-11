using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PolypediaUI : BaseMenuUI
{
    protected enum PolypediaState
    {
        TypeView,
        GridView,
        DetailsView,
    }

    [SerializeField]
    protected CharacterList characterList;
    [SerializeField]
    protected StageList stageList;
    [SerializeField]
    protected LootTable itemList;

    [SerializeField]
    protected Button backButton;

    [SerializeField]
    protected GameObject typeSelectView;
    [SerializeField]
    protected GameObject gridSelectView;
    [SerializeField]
    protected TextMeshProUGUI gridTitleText;
    [SerializeField]
    protected Transform gridParent;
    [SerializeField]
    protected GridEntry gridEntryTemplate;
    [SerializeField]
    private Selector selector;
    [SerializeField]
    protected GameObject detailsView;
    [SerializeField]
    protected TextMeshProUGUI detailsTitleText, detailsDescriptionText;
    [SerializeField]
    protected Image detailsIcon, detailsIconFade;
    protected PolypediaState currState;

    protected List<GridEntry> currentEntries;

    protected int currentDetailIdx = -1;
    [SerializeField]
    protected float timePerSlide = 5f;
    [SerializeField]
    protected float slideTransitionTime = 0.5f;
    protected float timeUntilSwitch = 0f;
    protected int currDetailSlideIdx = 0;
    protected List<Sprite> detailSlides;

    protected override void Awake()
    {
        base.Awake();
        currState = PolypediaState.TypeView;
        selector.Initialize(0, "");
        currentEntries = new List<GridEntry>();
        detailSlides = new List<Sprite>();
    }

    protected void Update()
    {
        if (currState == PolypediaState.DetailsView)
        {
            timeUntilSwitch -= Time.deltaTime;
            if (timeUntilSwitch <= 0)
            {
                StartCoroutine(NextDetailSlide());
                timeUntilSwitch = timePerSlide;
            }
        }
    }

    public override void Reset()
    {
        base.Reset();
        typeSelectView.SetActive(true);   
        gridSelectView.SetActive(false);   
        detailsView.SetActive(false);   
    }

    protected void SetState(PolypediaState newState)
    {        
        typeSelectView.SetActive(false);   
        gridSelectView.SetActive(false);   
        detailsView.SetActive(false);   
        switch (newState)
        {
            case PolypediaState.TypeView:
                {
                    typeSelectView.SetActive(true);   
                }
                break;
            case PolypediaState.GridView:
                {
                    gridSelectView.SetActive(true);   
                }
                break;
            case PolypediaState.DetailsView:
                {
                    detailsView.SetActive(true);   
                }
                break;

        }
        currState = newState;
    }

    public void BackButtonClicked()
    {
        switch (currState)
        {
            case PolypediaState.TypeView:
                {
                    mainMenuUI.TransitionToPreviousMode();
                }
                break;
            case PolypediaState.GridView:
            case PolypediaState.DetailsView:
                {
                    SetState(currState - 1);
                }
                break;
        } 
    }

    public override void Navigate(MainMenuPlayer player, Vector2 input)
    {
        if (player.IsPrimary())
        {
            switch (currState)
            {
                case PolypediaState.TypeView:
                    {
                        base.Navigate(player, input);
                    }
                    break;
                case PolypediaState.GridView:
                    {
                        if (selector.selectedEntry != null && !selector.Locked)
                        {
                            UnityEngine.UI.Selectable nextButton = null;
                            if (input.x > 0)
                            {
                                nextButton = selector.selectedEntry.Button.FindSelectableOnRight();
                            }
                            else if (input.x < 0)
                            {
                                nextButton = selector.selectedEntry.Button.FindSelectableOnLeft();
                            }
                            else if (input.y > 0)
                            {
                                nextButton = selector.selectedEntry.Button.FindSelectableOnUp();
                            }
                            else if (input.y < 0)
                            {
                                nextButton = selector.selectedEntry.Button.FindSelectableOnDown();
                            }

                            if (nextButton != null)
                            {
                                GridEntry nextEntry = nextButton.GetComponent<GridEntry>();
                                if (nextEntry != null)
                                {
                                    selector.selectedEntry.RemoveSelector(selector);
                                    selector.selectedEntry = nextEntry;
                                    selector.selectedEntry.AddSelector(selector, false);
                                }    
                            }
                        }
                    }
                    break;
                case PolypediaState.DetailsView:
                    {
                        if (input.x != 0)
                        {
                            NavigateDetail((int)input.x);
                        }  
                    }
                    break;
            }
            
        }
    }

    public override void Submit(MainMenuPlayer player)
    {
        if (player.IsPrimary())
        {
            if (EventSystem.current.currentSelectedGameObject != backButton.gameObject)
                {
                switch (currState)
                {
                    case PolypediaState.TypeView:
                        {
                        }
                        break;
                    case PolypediaState.GridView:
                        {
                            PopulateDetailsView((IPolypediaEntry)selector.selectedEntry.Registry);
                            currentDetailIdx = selector.selectedEntry.transform.GetSiblingIndex();
                        }
                        break;
                    case PolypediaState.DetailsView:
                        {
                        }
                        break;

                }
            }
        }
    }

    public override void Cancel(MainMenuPlayer player)
    {
        if (player.IsPrimary())
        {
            BackButtonClicked();
        }
    }    
    
    public override void Confirm(MainMenuPlayer player)
    {
        // no difference here but want to allow same button press
        Submit(player);
    }    

    public void OnRacersClicked()
    {
        PopulateGridView(characterList.GetCharacters());
        gridTitleText.text = "Select a Character";
    }
    public void OnCoursesClicked()
    {
        PopulateGridView(stageList.stages);
        gridTitleText.text = "Select a Course";
    }
    public void OnItemsClicked()
    {
        PopulateGridView(itemList.GetAllItems());
        gridTitleText.text = "Select an Item";
    }

    protected void PopulateGridView<T>(IEnumerable<T> entries) where T : ScriptableObject, IPolypediaEntry
    {        
        SetState(PolypediaState.GridView);
        currentEntries.Clear();

        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }

        foreach (T entry in entries)
        {
            GridEntry gridEntry = Instantiate(gridEntryTemplate, gridParent);
            gridEntry.Initialize(entry, entry.DisplayName, entry.Thumbnail, 1, 0); // TODO get row index
            if (mainMenuUI.PrimaryControlScheme == ControlScheme.Keyboard)
            {
                gridEntry.SetMouseSelector(selector);
            }
            currentEntries.Add(gridEntry);
        }

        if (currentEntries.Count > 0)
        {
            Canvas.ForceUpdateCanvases();
            selector.selectedEntry = currentEntries[0];
            selector.selectedEntry.AddSelector(selector, false);    // TODO warp=true is broken
        }
    }

    protected void PopulateDetailsView(IPolypediaEntry entry)
    {
        SetState(PolypediaState.DetailsView);  

        detailSlides.Clear();
        currDetailSlideIdx = 0;

        detailsTitleText.text = entry.DisplayName;
        detailsDescriptionText.text = entry.Description;
        if (entry.Slides.Count > 0)
        {
            foreach(Sprite slide in entry.Slides)
            {
                detailSlides.Add(slide);
            }
        }
        else
        {
            detailsIcon.sprite = entry.Thumbnail;
            detailSlides.Add(entry.Thumbnail);
        }
        detailsIcon.sprite = detailSlides[0];
        detailsIcon.preserveAspect = true;
        detailsIconFade.preserveAspect = true;
        detailsIconFade.color = new Color(detailsIconFade.color.r, detailsIconFade.color.g, detailsIconFade.color.b, 0f);
        timeUntilSwitch = timePerSlide;
    }

    public void NavigateDetail(int dir)
    {
        currentDetailIdx = currentDetailIdx + dir;
        if (currentDetailIdx < 0)
        {
            currentDetailIdx = gridParent.childCount - 1;
        }
        if (currentDetailIdx >= gridParent.childCount)
        {
            currentDetailIdx = 0;
        }
        GridEntry nextEntry = gridParent.GetChild(currentDetailIdx).GetComponent<GridEntry>();
        if (nextEntry != null)
        {
            PopulateDetailsView((IPolypediaEntry)nextEntry.Registry);
        }
    }

    protected IEnumerator NextDetailSlide()
    {
        detailsIconFade.sprite = detailSlides[currDetailSlideIdx];
        currDetailSlideIdx = (currDetailSlideIdx + 1) % detailSlides.Count;
        detailsIcon.sprite = detailSlides[currDetailSlideIdx];
        //detailsIconFade.color

        Color opaque = new Color(detailsIconFade.color.r, detailsIconFade.color.g, detailsIconFade.color.b, 1f);
        Color transparent = new Color(detailsIconFade.color.r, detailsIconFade.color.g, detailsIconFade.color.b, 0f);
        float elapsedTime = 0f;
        while (elapsedTime <= slideTransitionTime)
        {
            elapsedTime += Time.deltaTime;
            detailsIconFade.color = Color.Lerp(opaque, transparent, elapsedTime / slideTransitionTime);
            yield return null;
        }
        detailsIconFade.color = transparent;
    }

}