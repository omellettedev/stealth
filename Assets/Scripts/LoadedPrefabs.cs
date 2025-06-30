using UnityEngine;

[CreateAssetMenu(fileName = "LoadedPrefabs", menuName = "Scriptable Objects/LoadedPrefabs")]
public class LoadedPrefabs : ScriptableObject
{
    [Header("Prefabs")]
    [Tooltip("Prefab for items")]
    [SerializeField] private GameObject itemPrefab;
    public GameObject ItemPrefab => itemPrefab;

    [Tooltip("Prefab for inventory slots")]
    [SerializeField] private GameObject inventorySlotPrefab;
    public GameObject InventorySlotPrefab => inventorySlotPrefab;
}
