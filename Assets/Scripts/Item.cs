using UnityEngine;

public class Item
{
    private string itemName;
    public string ItemName => itemName;

    public Item(string name)
    {
        itemName = name;
    }
}