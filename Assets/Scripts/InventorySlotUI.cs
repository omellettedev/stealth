using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image slotBorderImage;
    private Color baseColor;
    [SerializeField] private Image itemImage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        baseColor = slotBorderImage.color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelectSlot()
    {
        slotBorderImage.color = Color.black;
    }

    public void DeselectSlot()
    {
        slotBorderImage.color = baseColor;
    }

    public void SetImage(Sprite sprite)
    {
        itemImage.sprite = sprite;
    }
}
