using UnityEngine;

public abstract class Item
{
    private string itemName;
    public string ItemName => itemName;

    private string itemDescription;
    public string ItemDescription => itemDescription;

    private Sprite itemIcon;
    public Sprite ItemIcon => itemIcon;

    public Item(string name, string description, Sprite icon)
    {
        itemName = name;
        itemDescription = description;
        itemIcon = icon;
    }
}