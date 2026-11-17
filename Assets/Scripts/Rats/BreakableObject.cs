using UnityEngine;

public class BreakableObject : CarryableInteractable
{
    [SerializeField] private SpriteRenderer intactVisual;
    [SerializeField] private GameObject[] revealedContents;

    private bool broken;

    protected override void Awake()
    {
        if (revealedContents != null)
        {
            foreach (GameObject content in revealedContents)
            {
                if (content != null)
                    content.SetActive(false);
            }
        }

        base.Awake();

        if (intactVisual == null)
            intactVisual = GetComponent<SpriteRenderer>();
    }

    public override bool CanInteract(Rat rat)
    {
        return !broken && base.CanInteract(rat);
    }

    public override void Interact(Rat rat)
    {
        if (CanInteract(rat))
            rat.TryCarryObject(this);
    }

    public void Break()
    {
        if (broken || IsCarried)
            return;

        broken = true;
        DisableWorldPhysics();

        if (intactVisual != null)
            intactVisual.enabled = false;

        if (revealedContents == null)
            return;

        foreach (GameObject content in revealedContents)
        {
            if (content != null)
            {
                content.transform.SetParent(null, true);
                content.SetActive(true);
            }
        }
    }
}
