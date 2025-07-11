using UnityEngine;

[CreateAssetMenu(fileName = "BasicItemData", menuName = "Scriptable Objects/BasicItemData")]
public class BasicItemData : ItemData
{
    public override ItemBase CreateItem()
    {
        return new BasicItem(this);
    }
}