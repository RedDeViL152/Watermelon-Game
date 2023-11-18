using NoxLibrary;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseList<T> : SingletonMB<T> where T : MonoBehaviour
{
    [SerializeField] protected int visibleCount;
    [SerializeField] protected Scrollbar scrollbar;
    [SerializeField] protected RectTransform parent;
    [SerializeField] protected GameObject template;
    [SerializeField] protected Drop[] drops;

    protected static List<GameObject> btnList;
    protected static DateTime lastRequest = DateTime.MinValue;
    protected static TimeSpan requestDelay = new TimeSpan(hours: 0, minutes: 0, seconds: 1);

    protected float VisibleHeight => parent.rect.height;

    protected float ElementHeight => VisibleHeight / visibleCount;

    protected float TotalHeight => ElementHeight * btnList.Count;

    protected float InvisibleHeight => TotalHeight - VisibleHeight;

    public void OnScrollbarChange(float value) => parent.localPosition = Vector3.up * InvisibleHeight * value;

    protected void Update()
    {
        if (Input.mouseScrollDelta.y > 0) On_Btn_Up();
        else if (Input.mouseScrollDelta.y < 0) On_Btn_Down();
    }

    public void On_Btn_Up()
    {
        float relativePos = parent.localPosition.y / ElementHeight;
        int correctedPos = Mathf.FloorToInt(relativePos);
        if (Mathn.Approximately(correctedPos, relativePos, 0.00001f) && correctedPos > 0)
            correctedPos--;
        parent.localPosition = Vector3.up * ElementHeight * correctedPos;
        scrollbar.value = parent.localPosition.y / InvisibleHeight;
    }

    public void On_Btn_Down()
    {
        float relativePos = parent.localPosition.y / ElementHeight;
        int correctedPos = Mathf.CeilToInt(relativePos);
        int maxPos = Mathn.RoundToInt(InvisibleHeight / ElementHeight);
        if (Mathn.Approximately(correctedPos, relativePos, 0.00001f) && correctedPos < maxPos)
            correctedPos++;
        parent.localPosition = Vector3.up * ElementHeight * correctedPos;
        scrollbar.value = parent.localPosition.y / InvisibleHeight;
    }

    protected virtual Transform InstantiateTemplate()
    {
        GameObject go = Instantiate(template, parent, worldPositionStays: true);
        go.SetActive(true);
        return go.transform;
    }

    protected virtual void DestroyOldList()
    {
        foreach (Drop d in drops)
            if (d.AttachedGO != null)
            {
                Destroy(d.AttachedGO);
                d.RemoveAttached();
            }

        List<GameObject> children = new List<GameObject>();
        foreach (Transform t in parent) children.Add(t.gameObject);
        foreach (GameObject go in children) Destroy(go);
    }
}
