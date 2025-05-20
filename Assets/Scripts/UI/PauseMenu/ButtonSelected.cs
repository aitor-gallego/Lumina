using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSelected : MonoBehaviour
{
    public RectTransform button;
    public float scaleMultiplier = 1.1f;
    public float scaleSpeed = 10f;
    private Vector3 originalScale;
    private Vector3 targetScale;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalScale = button.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        bool isSelected = EventSystem.current.currentSelectedGameObject == gameObject;
        targetScale = isSelected ? originalScale * scaleMultiplier : originalScale;
        button.localScale = Vector3.Lerp(button.localScale, targetScale, Time.unscaledDeltaTime * scaleSpeed);
    }
}
