using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public abstract bool CanInteract(Rat rat);
    public abstract void Interact(Rat rat);
}
