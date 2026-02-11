using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Threading.Tasks;

public class GalleryUI : BaseMenuUI
{    
    protected struct GridRow
    {
        Vector2 verticalBounds;
        bool isVisible;
    }

    protected enum GalleryState
    {
        GridView,
        DetailsView
    }
    [SerializeField]
    protected Button backButton, deleteButton, yesButton, noButton;

    [SerializeField]
    protected GameObject gridView, detailsView;
    [SerializeField]
    protected GameObject emptyGalleryDisplayObj;

    [SerializeField]
    protected Transform gridParent;
    protected GridLayoutGroup gridLayoutGroup;
    protected ScrollRect gridScrollRect;
    protected int numGridRows;
    protected float topPadding, bottomPadding, verticalSpacing;
    [SerializeField] protected int selectedGridRow, topGridRow, bottomGridRow;
    protected List<Vector2> rowBounds;

    [SerializeField]
    protected GridEntry gridEntryTemplate;
    [SerializeField]
    private Selector selector;
    [SerializeField]
    protected TextMeshProUGUI detailsText;
    [SerializeField]
    protected Image detailsIcon;
    protected GalleryState currState;    
    protected List<GridEntry> gridEntriesOrdered;
    protected Dictionary<GridEntry, Tuple<Sprite, FileInfo>> entriesLookup;
    protected GridEntry currentlyDisplayingGridEntry;
    [SerializeField]
    protected GameObject loadingPanel;
    [SerializeField]
    protected TextMeshProUGUI loadingText;
    [SerializeField]
    protected float loadingTextDelay = 1f;
    protected bool allSnapshotsLoaded = false;
    protected bool isDeleting = false;
    [SerializeField]
    protected GameObject deleteConfirmPanel;
    [SerializeField]
    protected TextMeshProUGUI deleteConfirmText;

    protected int currentDetailIdx = -1;
    protected override void Awake()
    {
        base.Awake();
        currState = GalleryState.GridView;
        gridEntriesOrdered = new List<GridEntry>();
        entriesLookup = new Dictionary<GridEntry, Tuple<Sprite, FileInfo>>();
        currentlyDisplayingGridEntry = null;
        selector.Initialize(0, "");
        gridLayoutGroup = gridParent.GetComponentInChildren<GridLayoutGroup>();
        gridScrollRect = gridParent.GetComponentInParent<ScrollRect>(true);
        topPadding = gridLayoutGroup.padding.top;
        bottomPadding = gridLayoutGroup.padding.bottom;
        verticalSpacing = gridLayoutGroup.spacing.y;

        rowBounds = new List<Vector2>();

        allSnapshotsLoaded = false;
    }

    protected override void Start()
    {
        base.Start();
    }    
    
    // void Update()
    // {
    //     Rect r = UIUtility.GetVisibleRegion(gridScrollRect);

    //     currViewportBounds = new Vector2(r.yMax, r.yMin);
    // }

    public override void Reset()
    {
        base.Reset();  
        currentlyDisplayingGridEntry = null;
        emptyGalleryDisplayObj.SetActive(false);
        gridView.SetActive(true);   
        detailsView.SetActive(false);  
        SetDeleteMode(false);
        if (!allSnapshotsLoaded)
        {
            StartCoroutine(LoadSnapshotCoroutine());
        }
        if (gridEntriesOrdered.Count == 0)
        {
            emptyGalleryDisplayObj.SetActive(true);
            selector.gameObject.SetActive(false);
        }
        //LoadSnapshots();
    }

    protected void SetState(GalleryState newState)
    {        
        gridView.SetActive(false);   
        detailsView.SetActive(false);   
        switch (newState)
        {
            case GalleryState.GridView:
                {
                    currentlyDisplayingGridEntry = null;
                    gridView.SetActive(true);   
                    SetDeleteMode(false);
                }
                break;
            case GalleryState.DetailsView:
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
            case GalleryState.GridView:
                {
                    if (isDeleting)
                    {
                        SetDeleteMode(false);
                    }
                    else
                    {
                        mainMenuUI.TransitionToPreviousMode();
                    }
                }
                break;
            case GalleryState.DetailsView:
                {
                    SetState(currState - 1);
                }
                break;
        } 
    }    
    public void NavigateButtonPressed(int dir)
    {
        switch (currState)
        {
            case GalleryState.GridView:
                {
                }
                break;
            case GalleryState.DetailsView:
                {
                    NavigateDetail(dir);
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
                case GalleryState.GridView:
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
                                    selectedGridRow = nextEntry.RowIdx;
                                    ScrollToRow(selectedGridRow);
                                }    
                            }
                        }
                    }
                    break;
                case GalleryState.DetailsView:
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
            // TBD there has gotta be a better way than this...
            if (EventSystem.current.currentSelectedGameObject != backButton.gameObject &&
                EventSystem.current.currentSelectedGameObject != deleteButton.gameObject &&
                EventSystem.current.currentSelectedGameObject != yesButton.gameObject &&
                EventSystem.current.currentSelectedGameObject != noButton.gameObject &&
                EventSystem.current.currentSelectedGameObject != gridScrollRect.verticalScrollbar.gameObject)
            {
                switch (currState)
                {
                    case GalleryState.GridView:
                        {
                            if (isDeleting)
                            {
                                selector.selectedEntry.ToggleRadioButton();
                            }
                            else
                            {
                                PopulateDetailsView(selector.selectedEntry);
                                currentDetailIdx = selector.selectedEntry.transform.GetSiblingIndex();
                            }
                        }
                        break;
                    case GalleryState.DetailsView:
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

    protected void LoadSnapshots()
    {

    }

    protected void NavigateDetail(int dir)
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
            PopulateDetailsView(nextEntry);
        }
    }

    protected IEnumerator LoadSnapshotCoroutine()
    {
        loadingPanel.SetActive(true);

        StartCoroutine(ChangeLoadingText());

        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }

        gridEntriesOrdered.Clear();
        entriesLookup.Clear();

        string snapshotFolderPath = string.Format("{0}/Snapshots/", Application.dataPath);
        
        List<GridEntry> firstEntriesPerRow = new List<GridEntry>();
        rowBounds.Clear();
        int photoCount = 0;
        DirectoryInfo di = new DirectoryInfo(snapshotFolderPath);
        di.Create();
        FileInfo[] files = di.GetFiles().OrderByDescending(f => f.CreationTime).ToArray();
        foreach (FileInfo f in files)
        {
            Texture2D texture;
            string filepath = Path.Combine(snapshotFolderPath, f.FullName);
            if (File.Exists(filepath) && Path.GetExtension(filepath) == ".png")
            {
                byte[] fileData = File.ReadAllBytes(filepath);
                texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                
                AsyncInstantiateOperation<GridEntry> asyncResult = InstantiateAsync(gridEntryTemplate, gridParent);
                yield return asyncResult;
                //GridEntry gridEntry = Instantiate(gridEntryTemplate, gridParent);
                GridEntry gridEntry = asyncResult.Result[0];
                gridEntry.Initialize(null, "", sprite, 1, photoCount / gridLayoutGroup.constraintCount);
                if (mainMenuUI.PrimaryControlScheme == ControlScheme.Keyboard)
                {
                    gridEntry.SetMouseSelector(selector);
                }

                if (photoCount % gridLayoutGroup.constraintCount == 0)
                {
                    firstEntriesPerRow.Add(gridEntry);
                }

                photoCount++;

                gridEntriesOrdered.Add(gridEntry);
                entriesLookup[gridEntry] = Tuple.Create(sprite, f);
            }
        }

        // force grid entries to size themselves
        yield return null;
        Canvas.ForceUpdateCanvases(); 

        if (gridEntriesOrdered.Count > 0)
        {
            selector.selectedEntry = firstEntriesPerRow[0];
            selector.selectedEntry.AddSelector(selector, false);    // TODO warp=true is broken
        
            
            numGridRows = firstEntriesPerRow.Count;
            foreach (GridEntry entry in firstEntriesPerRow)
            {
                if (entry.TryGetComponent(out RectTransform rt))
                {
                    rowBounds.Add(new Vector2(rt.offsetMax.y, rt.offsetMin.y));
                }
            }

            // scale the content manually 
            // this is required since we cannot put the GridLayoutGroup directly, as the selector must be outside a grid
            float gridHeight = gridLayoutGroup.GetComponent<RectTransform>().sizeDelta.y;
            gridScrollRect.content.sizeDelta = new Vector2(gridScrollRect.content.sizeDelta.x, gridHeight);

            // now, find the row indices that are visible
            // RectTransform viewportRT = gridScrollRect.viewport;
            // Vector3[] corners = new Vector3[4];
            // viewportRT.GetWorldCorners(corners);
            // float viewportMinY = corners[1].y;
            // float viewportMaxY = corners[0].y;

            RecalculateYBounds();

            numGridRows = (photoCount + gridLayoutGroup.constraintCount - 1) / gridLayoutGroup.constraintCount;

        }
        allSnapshotsLoaded = true;
        loadingPanel.SetActive(false);

    }

    protected IEnumerator ChangeLoadingText()
    {
        while (!allSnapshotsLoaded)
        {
            loadingText.text = "Loading";
            yield return new WaitForSeconds(loadingTextDelay);
            if (allSnapshotsLoaded)
            {
                break;
            }
            loadingText.text = "Loading.";
            yield return new WaitForSeconds(loadingTextDelay);
            if (allSnapshotsLoaded)
            {
                break;
            }
            loadingText.text = "Loading..";
            yield return new WaitForSeconds(loadingTextDelay);
            if (allSnapshotsLoaded)
            {
                break;
            }
            loadingText.text = "Loading...";
            yield return new WaitForSeconds(loadingTextDelay);
            if (allSnapshotsLoaded)
            {
                break;
            }
        }
    }

    protected void PopulateDetailsView(GridEntry entry)
    {
        Tuple<Sprite, FileInfo> tup = entriesLookup[entry];
        detailsIcon.sprite = tup.Item1;
        detailsIcon.preserveAspect = true;
        detailsText.text = tup.Item2.CreationTime.ToString();

        currentlyDisplayingGridEntry = entry;
        SetState(GalleryState.DetailsView);
    }

    public void OnScrollbarUpdated(float newVal)
    {
        RecalculateYBounds();
    }

    protected void RecalculateYBounds()
    {
        Rect viewportBounds = UIUtility.GetVisibleRegion(gridScrollRect);

        topGridRow = -1;
        for (int i = 0; i < numGridRows; i++)
        {
            if (topGridRow < 0)
            {
                if (rowBounds[i].x <= viewportBounds.yMax)
                {
                    topGridRow = i;
                }
            }
            else
            {
                if (rowBounds[i].x <= viewportBounds.yMax && rowBounds[i].y >= viewportBounds.yMin)
                {
                    bottomGridRow = i;
                }
                else
                {
                    break;
                }
            }
        }
    }
    
    protected void ScrollToRow(int rowIndex)
    {
        if (topGridRow > 0 && topGridRow > rowIndex)
        {
            // scrolling up
            Canvas.ForceUpdateCanvases(); 
            topGridRow--;
            bottomGridRow--;

            float ceil = rowBounds[rowIndex].x + verticalSpacing;
            gridScrollRect.verticalNormalizedPosition = UIUtility.GetNormalizedScrollAmountForYValToBeVisible(ceil, true, gridScrollRect);
        }
        else if (bottomGridRow < numGridRows - 1 && bottomGridRow < rowIndex)
        {
            // scrolling down
            Canvas.ForceUpdateCanvases(); 
            topGridRow++;
            bottomGridRow++;
            
            float floor = rowBounds[rowIndex].y -verticalSpacing;
            gridScrollRect.verticalNormalizedPosition = UIUtility.GetNormalizedScrollAmountForYValToBeVisible(floor, false, gridScrollRect);
        }
        else
        {
            // no scrolling, can exit early
            return;
        }
    }

    public void OnDeletePressed()
    {
        switch (currState)
        {
            case GalleryState.GridView:
                {
                    if (!isDeleting)
                    {
                        SetDeleteMode(true);
                    }
                    else
                    {
                        EnableDeleteConfirmation(true, true);
                    }
                }
                break;
            case GalleryState.DetailsView:
                {
                    EnableDeleteConfirmation(true, false);
                }
                break;
        }
    }

    public void OnDeleteConfirmed()
    {
        DeleteSelectedPhotos();
        SetDeleteMode(false);
    }

    public void OnDeleteCanceled()
    {
        SetDeleteMode(false);
    }

    void SetDeleteMode(bool enabled)
    {
        isDeleting = enabled;
        foreach (GridEntry ge in gridEntriesOrdered)
        {
            ge.EnableRadioButton(enabled);
        }
        if (!enabled)
        {
            EnableDeleteConfirmation(false, false);
        }
    }

    void EnableDeleteConfirmation(bool enable, bool grid)
    {   
        if (enable)
        {
            if (grid)
            {
                int numToDelete = 0;
                foreach (GridEntry ge in gridEntriesOrdered)
                {
                    if (ge.IsChecked)
                    {
                        numToDelete++;
                    }
                }
                deleteConfirmText.text = String.Format("Are you sure you want to delete {0} photos?", numToDelete);
            }
            else
            {
                deleteConfirmText.text = String.Format("Are you sure you want to delete this photo?");
            }
        }
        deleteConfirmPanel.SetActive(enable);
    }

    void DeleteSelectedPhotos()
    {
        List<GridEntry> entriesToDelete = new List<GridEntry>();
        bool deleteSelectedEntry = false;                    
        int indexOfSelectedEntry = gridEntriesOrdered.IndexOf(selector.selectedEntry);

        switch (currState)
        {
            case GalleryState.GridView:
                {
                    foreach (GridEntry ge in gridEntriesOrdered)
                    {
                        if (ge.IsChecked)
                        {
                            if (selector.selectedEntry == ge)
                            {
                                deleteSelectedEntry = true;
                            }
                            entriesToDelete.Add(ge);
                        }
                    }
                }
                break;
            case GalleryState.DetailsView:
                {
                    if (currentlyDisplayingGridEntry == selector.selectedEntry)
                    {
                        deleteSelectedEntry = true;
                    }
                    entriesToDelete.Add(currentlyDisplayingGridEntry);
                    NavigateDetail(1);
                }
                break;
        }
        
        foreach (GridEntry ge in entriesToDelete)
        {
            Tuple<Sprite, FileInfo> t = entriesLookup[ge];
#if UNITY_EDITOR
            File.Delete(t.Item2.FullName + ".meta");
#endif
            t.Item2.Delete();
            gridEntriesOrdered.Remove(ge);
            entriesLookup.Remove(ge);
            Destroy(ge.gameObject);
        }

        // update selector if its entry was deleted
        if (deleteSelectedEntry)
        {
            GridEntry newSelectedEntry = gridEntriesOrdered[Mathf.Min(gridEntriesOrdered.Count - 1, indexOfSelectedEntry)];
            selector.selectedEntry = newSelectedEntry;
            selector.selectedEntry.AddSelector(selector, false);    // TODO warp=true is broken
        }

    }
}