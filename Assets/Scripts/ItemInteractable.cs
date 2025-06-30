using UnityEngine;

public class ItemInteractable : InstantInteractable
{
    private Color baseColor;
    private Renderer rend;
    [SerializeField] private ItemData startingData;
    private Item item;

    public override void TriggerInteractionEffect()
    {
        base.TriggerInteractionEffect();
        Player.Instance.TryPickupItem(item);
        Destroy(gameObject);
    }

    private void Start()
    {
        rend = GetComponent<Renderer>();
        baseColor = rend.material.GetColor("_BaseColor");
        if (startingData != null)
        {
            item = startingData.CreateItem();
        }
    }

    public override void OnLookAt()
    {
        base.OnLookAt();
        rend.material.SetColor("_BaseColor", Color.white);
    }

    public override void OnLookAway()
    {
        base.OnLookAway();
        rend.material.SetColor("_BaseColor", baseColor);
    }
}
