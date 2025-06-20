using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private static UIController instance;
    public static UIController Instance => instance;

    [SerializeField] private Slider interactionSlider;

    private void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

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
}
