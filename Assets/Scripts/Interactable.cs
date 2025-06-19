using UnityEngine;

public interface IInteractable
{
    void OnInteract();
    void OnUninteract();
    void OnLookAt();
    void OnLookAway();
    void TriggerInteractionEffect();
}