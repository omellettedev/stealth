using System;
using UnityEngine;

public abstract class HeldInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] protected float interactionTime;
    protected float interactionTimeLeft = 0;

    public event Action<float> InteractionUpdateEvent;

    private void Update()
    {
        Tick();
    }

    protected virtual void Tick()
    {
        if (interactionTimeLeft > 0)
        {
            interactionTimeLeft -= Time.deltaTime;
            InteractionUpdateEvent?.Invoke((interactionTime - interactionTimeLeft) / interactionTime);
            if (interactionTimeLeft < 0)
            {
                interactionTimeLeft = 0;
                TriggerInteractionEffect();
            }
        }
    }

    public virtual void OnInteract()
    {
        interactionTimeLeft = interactionTime;
        UIController.Instance.SetInteractionValue(0);
        InteractionUpdateEvent += UIController.Instance.SetInteractionValue;
    }
    
    public virtual void OnUninteract()
    {
        if (interactionTimeLeft > 0)
        {
            interactionTimeLeft = 0;
        }
        UIController.Instance.CancelInteraction();
        InteractionUpdateEvent -= UIController.Instance.SetInteractionValue;
    }

    public virtual void OnLookAt() { }
    public virtual void OnLookAway()
    { 
        OnUninteract(); 
    }

    public virtual void TriggerInteractionEffect()
    {
        UIController.Instance.CompleteInteraction();
        InteractionUpdateEvent -= UIController.Instance.SetInteractionValue;
    }
}