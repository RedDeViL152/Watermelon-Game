using UnityEngine;

public abstract class Drag : MonoBehaviour
{
    [SerializeField] protected Transform originalParent;
    [SerializeField] protected Transform dragParent;
    [SerializeField] protected LayerMask dropLayers;
    protected bool dragging = false;

    public Transform OriginalParent => originalParent;
    public Drop Drop { get; set; }

    protected void Awake()
    {
        if (dragParent is null)
            dragParent = GameObject.Find("DragObjects").transform;
    }

    protected abstract int SideWindowID { get; }

    protected virtual void OnMouseDown()
    {
        if (Drop != null)
        {
            Drop.RemoveAttached();
            Drop = null;
        }
        dragging = true;
        transform.SetParent(dragParent, worldPositionStays: true);
    }

    protected void OnMouseDrag()
    {
        float z = transform.position.z;
        Vector3 newPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        newPos.z = z;
        transform.position = newPos;
    }

    protected void OnMouseUp()
    {
        dragging = false;
        Collider2D collider2D = Physics2D.OverlapPoint(transform.position, dropLayers);
        if (collider2D != null)
            collider2D.gameObject.SendMessage("OnMouseDrop", gameObject, SendMessageOptions.RequireReceiver);
        else Reset();
    }

    /// <summary>
    /// Returns the object to it's original position and breaks the DropSkill reference
    /// </summary>
    public void Reset()
    {
        Drop = null;
        transform.SetParent(originalParent, worldPositionStays: true);
        if (!dragging) transform.localPosition = Vector3.zero;
    }
}
