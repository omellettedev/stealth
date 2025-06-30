using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIController : MonoBehaviour
{
    private static UIController instance;
    public static UIController Instance => instance;

    [SerializeField] private Slider interactionSlider;
    private List<InventorySlotUI> inventorySlots = new List<InventorySlotUI>();
    [SerializeField] private GameObject inventoryCenter;
    [SerializeField] private float inventorySlotSpacing = 10f;
    private float inventoryCenterSpacing;
    private InventorySlotUI selectedSlot;

    private void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inventoryCenterSpacing = Manager.Instance.LoadedPrefabs.InventorySlotPrefab.GetComponent<RectTransform>().sizeDelta.x + inventorySlotSpacing;

        Player.Instance.PickedUpItemEvent += AddInventorySlot;
        Player.Instance.LostItemEvent += RemoveInventorySlot;
        Player.Instance.SelectedItemEvent += SelectInventorySlot;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetInteractionValue(float value)
    {
        interactionSlider.value = value;
        if (!interactionSlider.gameObject.activeSelf)
        {
            interactionSlider.gameObject.SetActive(true);
        }
    }

    public void CompleteInteraction()
    {
        interactionSlider.gameObject.SetActive(false);
        interactionSlider.value = 0;
    }

    public void CancelInteraction()
    {
        interactionSlider.gameObject.SetActive(false);
        interactionSlider.value = 0;
    }

    public void UpdateInventorySlotPositions()
    {
        float inventoryCount = inventorySlots.Count;
        for (int i = 0; i < inventoryCount; i++)
        {
            RectTransform slotRect = inventorySlots[i].GetComponent<RectTransform>();
            // this ensures that the slots are centered around the inventory center
            float x = (i - ((inventoryCount - 1) / 2)) * inventoryCenterSpacing;
            slotRect.anchoredPosition = new Vector2(x, slotRect.anchoredPosition.y);
        }
    }

    public void AddInventorySlot(Sprite itemIcon)
    {
        GameObject newSlot = Instantiate(Manager.Instance.LoadedPrefabs.InventorySlotPrefab, inventoryCenter.transform);
        InventorySlotUI slotUI = newSlot.GetComponent<InventorySlotUI>();
        slotUI.SetImage(itemIcon);
        inventorySlots.Add(slotUI);
        UpdateInventorySlotPositions();
    }

    public void RemoveInventorySlot(int index)
    {
        if (index < 0 || index >= inventorySlots.Count) return; // Check for valid index
        InventorySlotUI slotToRemove = inventorySlots[index];
        inventorySlots.RemoveAt(index);
        Destroy(slotToRemove.gameObject);
        UpdateInventorySlotPositions();
    }

    // TEMPORARY IMPLEMENTATION
    public void SelectInventorySlot(int prevIndex, int index)
    {
        if (prevIndex >= 0 && prevIndex < inventorySlots.Count)
        {
            InventorySlotUI prevSlot = inventorySlots[prevIndex];
            prevSlot.DeselectSlot(); // Reset previous slot color
        }
        if (index < 0 || index >= inventorySlots.Count) return; // Check for valid index
        InventorySlotUI slotToSelect = inventorySlots[index];
        slotToSelect.SelectSlot();
    }
}
