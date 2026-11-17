using UnityEngine;

public class CarryableInteractable : Interactable
{
    [SerializeField] private Vector2 backOffset = new Vector2(0.45f, 0.45f);
    [SerializeField] private Vector2 dropOffset = new Vector2(0.8f, 0.1f);

    private Collider2D[] itemColliders;
    private bool[] colliderWasEnabled;
    private Rigidbody2D itemBody;
    private bool bodyWasSimulated;
    private SpriteRenderer itemRenderer;
    private int originalSortingOrder;
    private Rat carrier;

    public bool IsCarried => carrier != null;

    protected virtual void Awake()
    {
        itemColliders = GetComponentsInChildren<Collider2D>();
        colliderWasEnabled = new bool[itemColliders.Length];
        itemBody = GetComponent<Rigidbody2D>();
        itemRenderer = GetComponentInChildren<SpriteRenderer>();
        if (itemRenderer != null)
            originalSortingOrder = itemRenderer.sortingOrder;
        if (itemBody != null)
            bodyWasSimulated = itemBody.simulated;
    }

    protected virtual void LateUpdate()
    {
        if (carrier != null)
            transform.localPosition = carrier.GetBackOffset(backOffset);
    }

    public override bool CanInteract(Rat rat)
    {
        return !IsCarried && rat != null && rat.PodeCarregarObjeto;
    }

    public override void Interact(Rat rat)
    {
        if (CanInteract(rat))
            rat.TryCarryObject(this);
    }

    internal bool AttachToRat(Rat rat)
    {
        if (!CanInteract(rat))
            return false;

        carrier = rat;
        for (int i = 0; i < itemColliders.Length; i++)
        {
            colliderWasEnabled[i] = itemColliders[i].enabled;
            itemColliders[i].enabled = false;
        }

        if (itemBody != null)
        {
            bodyWasSimulated = itemBody.simulated;
            itemBody.velocity = Vector2.zero;
            itemBody.simulated = false;
        }

        transform.SetParent(rat.transform, false);
        transform.localPosition = rat.GetBackOffset(backOffset);
        transform.localRotation = Quaternion.identity;

        SpriteRenderer ratRenderer = rat.GetComponent<SpriteRenderer>();
        if (itemRenderer != null && ratRenderer != null)
            itemRenderer.sortingOrder = ratRenderer.sortingOrder + 1;

        return true;
    }

    internal void DropFromRat(Rat rat)
    {
        if (carrier != rat)
            return;

        carrier = null;
        transform.SetParent(null, true);
        transform.position = rat.transform.position + new Vector3(
            rat.FacingDirection * dropOffset.x,
            dropOffset.y,
            0f);

        for (int i = 0; i < itemColliders.Length; i++)
            itemColliders[i].enabled = colliderWasEnabled[i];

        if (itemBody != null)
        {
            itemBody.simulated = bodyWasSimulated;
            if (bodyWasSimulated)
                itemBody.velocity = rat.CurrentVelocity;
        }

        if (itemRenderer != null)
            itemRenderer.sortingOrder = originalSortingOrder;
    }

    protected void DisableWorldPhysics()
    {
        foreach (Collider2D itemCollider in itemColliders)
            itemCollider.enabled = false;

        if (itemBody != null)
        {
            itemBody.velocity = Vector2.zero;
            itemBody.simulated = false;
        }
    }
}
