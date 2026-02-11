using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridEntry : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField]
    protected TextMeshProUGUI nameText;
    [SerializeField]
    protected Image thumbnail;
    protected ScriptableObject registry;
    protected List<RectTransform> targets;
    [SerializeField]
    protected Transform targetParent;
    protected Selectable button;
    protected List<Selector> currentSelectors; // TODO change to dict w/ max cap???
    protected Selector mouseSelector;   // special selector for the player who is using a mouse

    [SerializeField]
    protected GameObject radioButtonParent, radioButtonCheck;

    public Selectable Button { get => button; }
    public int SelectorCount { get => currentSelectors.Count; }
    public ScriptableObject Registry { get => registry; }
    public int RowIdx { get; private set; }
    public bool IsChecked { get; private set; }

    protected virtual void Awake()
    {
        button = GetComponent<Selectable>();   

        currentSelectors = new List<Selector>();
        targets = new List<RectTransform>();
    }

    protected virtual void Start()
    {
        foreach (Transform target in targetParent)
        {
            target.gameObject.SetActive(false);
        }
        EnableRadioButton(false);
    }

    public virtual void Initialize(ScriptableObject registry, string name, Sprite image, int maxTargets, int rowIndex)
    {
        this.registry = registry;
        nameText.text = name;
        thumbnail.sprite = image;
        RowIdx = rowIndex;

        for (int i = 0; i < maxTargets; i++)
        {
            GameObject newTarget = new GameObject(string.Format("Target {0}", i), typeof(RectTransform));
            //newTarget.transform.parent = targetParent;
            newTarget.transform.SetParent(targetParent, false);
            if (newTarget.TryGetComponent(out RectTransform rt))
            {
                targets.Add(rt);
            }
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)targetParent);
    }

    public void Reset()
    {
        currentSelectors.Clear();
        Redraw(true);
    }

    public virtual void AddSelector(Selector selector, bool warp)
    {
        currentSelectors.Add(selector);
        currentSelectors.Sort((a, b) => a.SelectorIndex.CompareTo(b.SelectorIndex));
        Redraw(warp);
    }

    public virtual void RemoveSelector(Selector selector)
    {
        currentSelectors.Remove(selector);
        Redraw(false);
    }

    public void SetMouseSelector(Selector selector)
    {
        mouseSelector = selector;
    }

    public void OnPointerEnter(PointerEventData _)
    {
        if (mouseSelector != null)
        {
            mouseSelector.InterruptMove();

            if (mouseSelector.selectedEntry != null && mouseSelector.selectedEntry != this && !mouseSelector.Locked)
            {
                mouseSelector.selectedEntry.RemoveSelector(mouseSelector);
                mouseSelector.selectedEntry = this;
                AddSelector(mouseSelector, false);
            }
        }
    }

    protected void Redraw(bool warp)
    {
        foreach (Transform target in targetParent)
        {
            target.gameObject.SetActive(false);
        }

        for (int i = 0; i < currentSelectors.Count; i++)
        {
            targets[i].gameObject.SetActive(true);
            if (warp)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)targetParent);
            }
            currentSelectors[i].MoveToTarget(targets[i], warp);
        }
    }

    public void EnableRadioButton(bool enabled)
    {
        IsChecked = false;
        radioButtonCheck.SetActive(false);
        radioButtonParent.SetActive(enabled);
    }

    public void ToggleRadioButton()
    {
        IsChecked = !IsChecked;
        radioButtonCheck.SetActive(IsChecked);
    }

}