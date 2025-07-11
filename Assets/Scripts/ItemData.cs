using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    [Tooltip("Item name")]
    [SerializeField] private string itemName;
    public string ItemName => itemName;

    [Tooltip("Item description")]
    [SerializeField] private string itemDescription;
    public string ItemDescription => itemDescription;

    [Tooltip("Item icon")]
    [SerializeField] private Sprite itemIcon;
    public Sprite ItemIcon => itemIcon;

    public abstract ItemBase CreateItem();
}