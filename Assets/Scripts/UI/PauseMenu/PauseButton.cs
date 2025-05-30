using UnityEngine;
using UnityEngine.EventSystems;

public class PauseButton : MonoBehaviour
{
    public AudioClip selectSFX;
    public RectTransform button;

    private float multiplier = 1.1f;
    private float scaleSpeed = 10f;
    private Vector3 startSize;
    private Vector3 finalSize;
    private bool selected = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startSize = button.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        bool isSelected = EventSystem.current.currentSelectedGameObject == gameObject;
        finalSize = isSelected ? startSize * multiplier : startSize;
        button.localScale = Vector3.Lerp(button.localScale, finalSize, Time.unscaledDeltaTime * scaleSpeed);

        if (isSelected && !selected && selectSFX != null)
        {
            AudioController.Instance.PlaySFX(selectSFX);
        }

        selected = isSelected;
    }
}
