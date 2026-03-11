using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridEntry : Selectable, IPointerEnterHandler
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
    protected Action<GridEntry, BaseEventData> selectedCallback;

    [SerializeField]
    protected GameObject radioButtonParent, radioButtonCheck;

    public Selectable Button { get => button; }
    public int SelectorCount { get => currentSelectors.Count; }
    public ScriptableObject Registry { get => registry; }
    public int RowIdx { get; private set; }
    public bool IsChecked { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        button = GetComponent<Selectable>();

        currentSelectors = new List<Selector>();
        targets = new List<RectTransform>();
    }

    protected override void Start()
    {
        base.Start();
        foreach (Transform target in targetParent)
        {
            target.gameObject.SetActive(false);
        }
        EnableRadioButton(false);
    }

    public virtual void Initialize(ScriptableObject registry, string name, Sprite image, int maxTargets, int rowIndex, Action<GridEntry, BaseEventData> selectedCallback)
    {
        this.registry = registry;
        nameText.text = name;
        thumbnail.sprite = image;
        RowIdx = rowIndex;
        this.selectedCallback = selectedCallback;

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

    public new void Reset()
    {
        //base.Reset();
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

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (mouseSelector != null)
        {
            base.OnPointerEnter(eventData);
            mouseSelector.InterruptMove();

            if (mouseSelector.selectedEntry != null && mouseSelector.selectedEntry != this && !mouseSelector.Locked)
            {
                mouseSelector.selectedEntry.RemoveSelector(mouseSelector);
                mouseSelector.selectedEntry = this;
                AddSelector(mouseSelector, false);
            }
        }
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        if (selectedCallback != null)
        {
            selectedCallback(this, eventData);
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