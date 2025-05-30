using UnityEngine;
using UnityEngine.EventSystems;

public class MenuSlider : MonoBehaviour
{
    public RectTransform button;
    public AudioClip hoverSound;

    private Vector3 originalbutton;
    private bool selected = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (button == null)
            button = GetComponent<RectTransform>();

        originalbutton = button.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        GameObject selected = EventSystem.current.currentSelectedGameObject;

        bool isSelected = selected != null && (selected == gameObject || selected.transform.IsChildOf(transform));

        Vector3 targetScale = isSelected ? originalbutton * 1.1f : originalbutton;
        button.localScale = Vector3.Lerp(button.localScale, targetScale, Time.deltaTime * 10f);

        if (isSelected && !this.selected && hoverSound != null)
        {
            AudioController.Instance.PlaySFX(hoverSound);
        }

        this.selected = isSelected;
    }
}
