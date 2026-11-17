using UnityEngine;

public class CheeseInteractable : Interactable
{
    [SerializeField] private Vector2 backOffset = new Vector2(0.45f, 0.45f);

    private Collider2D[] cheeseColliders;
    private Rigidbody2D cheeseBody;
    private SpriteRenderer cheeseRenderer;
    private Rat carrier;
    private bool collected;

    private void Awake()
    {
        cheeseColliders = GetComponentsInChildren<Collider2D>();
        cheeseBody = GetComponent<Rigidbody2D>();
        cheeseRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        if (carrier != null)
            transform.localPosition = carrier.GetBackOffset(backOffset);
    }

    public override bool CanInteract(Rat rat)
    {
        return !collected && rat != null && !rat.CarregandoQueijo;
    }

    public override void Interact(Rat rat)
    {
        if (!CanInteract(rat) || !rat.TryCarryCheese())
            return;

        collected = true;
        carrier = rat;

        foreach (Collider2D cheeseCollider in cheeseColliders)
            cheeseCollider.enabled = false;

        if (cheeseBody != null)
        {
            cheeseBody.velocity = Vector2.zero;
            cheeseBody.simulated = false;
        }

        transform.SetParent(rat.transform, false);
        transform.localPosition = rat.GetBackOffset(backOffset);
        transform.localRotation = Quaternion.identity;

        if (cheeseRenderer != null && rat.GetComponent<SpriteRenderer>() != null)
            cheeseRenderer.sortingOrder = rat.GetComponent<SpriteRenderer>().sortingOrder + 1;
    }
}
