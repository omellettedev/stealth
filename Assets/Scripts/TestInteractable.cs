using UnityEngine;

public class TestInteractable : ItemSpecificHeldInteractable
{
    private Color baseColor;
    private Renderer rend;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        baseColor = rend.material.GetColor("_BaseColor");
    }

    public override void TriggerInteractionEffect()
    {
        base.TriggerInteractionEffect();
        Destroy(gameObject);
    }

    public override void OnLookAway()
    {
        base.OnLookAway();
        rend.material.SetColor("_BaseColor", baseColor);
    }

    protected override void OnValidLook()
    {
        base.OnValidLook();
        rend.material.SetColor("_BaseColor", Color.white);
    }

    protected override void OnInvalidLook()
    {
        base.OnInvalidLook();
        rend.material.SetColor("_BaseColor", baseColor);
    }

    protected override bool IsItemValid(ItemBase item)
    {
        return item != null;
    }
}
