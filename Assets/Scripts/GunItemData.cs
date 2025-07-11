using UnityEngine;

[CreateAssetMenu(fileName = "GunData", menuName = "Scriptable Objects/GunData")]
public class GunItemData : ItemData
{
    [Tooltip("Bullets per second that the gun fires while held down")]
    [SerializeField] private float fireRate = 2f;

    [Tooltip("Random angle of bullet displacement")]
    [SerializeField] private float spreadAngle = 5f;

    public override ItemBase CreateItem()
    {
        return new GunItem(this);
    }
}