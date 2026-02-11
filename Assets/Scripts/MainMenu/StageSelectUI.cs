using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StageSelectUI : BaseMenuUI
{
    
    public StageList stages;
    public GridEntry entryTemplate;
    private List<GridEntry> entries;
    public Transform entryParent;
    [SerializeField]
    private Selector selector;
    [SerializeField]
    private AllReadyOverlay allReadyOverlay;

    protected override void Awake()
    {
        base.Awake();

        entries = new List<GridEntry>();

        selector.Initialize(0, "");

        // clear out any children left in scene??
        foreach (Transform child in entryParent)
        {
            Destroy(child.gameObject);
        }

        foreach (StageRegistry stage in stages.stages)
        {
            AddStage(stage);
        }

    }

    protected override void OnEnable()
    {
        if (mainMenuUI.PrimaryControlScheme == ControlScheme.Keyboard)
        {
            foreach (GridEntry entry in entries)
            {
                entry.SetMouseSelector(selector);
            }
        }

    }

    public override void Reset()
    {
        base.Reset();
        

        if (stages.stages.Count > 0)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)entryParent.transform);
            Canvas.ForceUpdateCanvases();
            selector.selectedEntry = entries[0];
            selector.selectedEntry.AddSelector(selector, false);    // TODO warp=true is broken
        }
        allReadyOverlay.SetActive(false);
    }

    public override void Navigate(MainMenuPlayer player, Vector2 input)
    {
        if (player.IsPrimary())
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
    }

    public override void Submit(MainMenuPlayer player)
    {
        if (player.IsPrimary())
        {
            if (!selector.Locked)
            {
                selector.Lock();
                                
                UpdateReadyOverlay();
            }
        }
    }

    public override void Cancel(MainMenuPlayer player)
    {
        if (player.IsPrimary())
        {
            if (selector.Locked)
            {
                selector.Unlock();
                UpdateReadyOverlay();
            }
            else
            {
                mainMenuUI.TransitionToPreviousMode();
            }
        }
    }

    public override void Confirm(MainMenuPlayer player)
    {
        if (player.IsPrimary())
        {
            if (selector.Locked)
            {
                raceSettings.SetSelectedStage((StageRegistry)selector.selectedEntry.Registry);
                raceSettings.StartRace();
            }
        }
    }    
    
    private void UpdateReadyOverlay()
    {
        
        if (selector.Locked)
        {
            allReadyOverlay.SetControlScheme(mainMenuUI.PrimaryControlScheme);         
            allReadyOverlay.SetActive(true);
        }
        else
        {
            allReadyOverlay.SetActive(false);
        }
    }

    private void AddStage(StageRegistry stage)
    {
        GridEntry entry = Instantiate(entryTemplate, entryParent);
        entry.Initialize(stage, stage.displayName, stage.icon, 1, 0); // TODO get row index
        entries.Add(entry);
        if (firstSelectable == null)
        {
            firstSelectable = entry.Button;
        }
    }

    public List<StageRegistry> GetNStages(int n, bool random)
    {
        List<StageRegistry> registries = new List<StageRegistry>();
        List<StageRegistry> tempRegistries = new List<StageRegistry>();
        while (registries.Count < n)
        {
            tempRegistries.Clear();
            foreach (StageRegistry registry in stages.stages)
            {
                tempRegistries.Add(registry);
            }
            if (random)
            {
                tempRegistries = ListUtility.FisherYatesShuffle(tempRegistries);
            }
            foreach (StageRegistry registry in tempRegistries)
            {
                registries.Add(registry);
                if (registries.Count == n)
                {
                    break;
                }
            }
        }
        return registries;
    }
}