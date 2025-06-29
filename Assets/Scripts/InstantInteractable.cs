using UnityEngine;

public abstract class InstantInteractable : MonoBehaviour, IInteractable
{
    public void OnInteract()
    {
        TriggerInteractionEffect();
    }

    public virtual void OnLookAt() { }

    public virtual void OnLookAway() { }

    public void OnUninteract() { }

    public virtual void TriggerInteractionEffect() { }
}
