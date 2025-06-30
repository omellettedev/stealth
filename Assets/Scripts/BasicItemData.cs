using UnityEngine;

[CreateAssetMenu(fileName = "BasicItemData", menuName = "Scriptable Objects/BasicItemData")]
public class BasicItemData : ItemData
{
    public override Item CreateItem()
    {
        return new BasicItem(ItemName, ItemDescription, ItemIcon);
    }
}
