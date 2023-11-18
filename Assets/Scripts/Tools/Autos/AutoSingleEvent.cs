using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class AutoSingleEvent : MonoBehaviour
{
    [SerializeField] protected bool useRange;
    [SerializeField, ShowIf("useRange")] protected FloatRange delayRange = new FloatRange(0, 0);
    [SerializeField, HideIf("useRange")] protected float delay = 0;
    [SerializeField] protected UnityEvent onEvent;
    [SerializeField] protected bool disableAfter;
    [SerializeField] protected bool unscaled;

    protected virtual void OnEnable()
    {
        if (unscaled)
        this.InvokeDelayed( new WaitForSecondsRealtime (useRange ? delayRange.Random() : delay), () => { onEvent?.Invoke(); if (disableAfter) gameObject.SetActive(false); } );
        else
        this.InvokeDelayed( new WaitForSeconds (useRange ? delayRange.Random() : delay), () => { onEvent?.Invoke(); if (disableAfter) gameObject.SetActive(false); } );
    }
    

    public void SetDelayMin(float min) => delayRange.Min = min;
    public void SetDelayMax(float max) => delayRange.Max = max;
    public void TriggerAgain() => OnEnable();
    public void TestTrigger() => Log.Message("Event Triggered after waiting for " + delay + " seconds");
}
