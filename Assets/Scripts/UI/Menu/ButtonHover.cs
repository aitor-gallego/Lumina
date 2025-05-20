using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour
{
    public RectTransform button;
    public GameObject indicator;
    public float offset = 10f;
    public float speed = 10f;
    private Vector2 origin;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        origin = button.anchoredPosition;
        indicator.SetActive(false);
    }

    void Update()
    {
        bool isSelected = EventSystem.current.currentSelectedGameObject == gameObject;

        Vector2 targetPosition = isSelected ? origin + new Vector2(offset, 0) : origin;
        button.anchoredPosition = Vector2.Lerp(button.anchoredPosition, targetPosition, Time.deltaTime * speed);

        indicator.SetActive(isSelected);
    }
}
