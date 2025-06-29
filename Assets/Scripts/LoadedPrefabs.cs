using UnityEngine;

public class LoadedPrefabs : MonoBehaviour
{
    private static LoadedPrefabs instance;
    public static LoadedPrefabs Instance => instance;
    private void Awake()
    {
        instance = this;
    }

    [Header("Prefabs")]
    [Tooltip("Prefab for items")]
    [SerializeField] private GameObject itemPrefab;
    public GameObject ItemPrefab => itemPrefab;

    [Tooltip("Prefab for inventory slots")]
    [SerializeField] private GameObject inventorySlotPrefab;
    public GameObject InventorySlotPrefab => inventorySlotPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
