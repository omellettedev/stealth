using UnityEngine;

public class TestInteractable : HeldInteractable
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
