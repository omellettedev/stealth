using UnityEngine;

public abstract class ItemSpecificHeldInteractable : HeldInteractable
{
    public override void OnInteract()
    {
        if (Player.Instance.HeldItem != null && IsItemValid(Player.Instance.HeldItem))
        {
            base.OnInteract();
        }
    }

    public override void OnLookAt()
    {
        base.OnLookAt();
        CheckItem();
        Player.Instance.SelectedItemEvent += ItemChangeHandler;
    }

    public override void OnLookAway()
    {
        base.OnLookAway();
        Player.Instance.SelectedItemEvent -= ItemChangeHandler;
    }

    private void ItemChangeHandler(int prev, int curr) => CheckItem();

    private void CheckItem()
    {
        if (IsItemValid(Player.Instance.HeldItem))
        {
            OnValidLook();
        }
        else
        {
            OnInvalidLook();
        }
    }

    protected virtual void OnValidLook()
    {

    }

    protected virtual void OnInvalidLook()
    {
        OnUninteract();
    }

    protected abstract bool IsItemValid(ItemBase item);
}
