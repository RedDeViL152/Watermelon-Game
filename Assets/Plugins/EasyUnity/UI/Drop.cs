using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class Drop : MonoBehaviour
{
    [SerializeField] private int slot;

    public int Slot => slot;
    public GameObject AttachedGO { get; protected set; }
    public Drag AttachedDrag { get; protected set; }

    protected void OnEnable() => UpdateText();

    public abstract void UpdateText();

    public virtual void OnMouseDrop(object value)
    {
        GameObject go = value as GameObject;

        if (AttachedGO != null)
            RemoveAttached();
        Set(go);
    }

    protected virtual void Set(GameObject go)
    {
        AttachedDrag = go.GetComponent<Drag>();
        AttachedDrag.Drop = this;

        AttachedGO = go;
        AttachedGO.transform.SetParent(transform, worldPositionStays: true);
        AttachedGO.transform.position = transform.position + Vector3.back;
    }

    public virtual void RemoveAttached()
    {
        if (AttachedDrag) AttachedDrag.Reset();
        AttachedDrag = null;
        AttachedGO = null;
    }
}