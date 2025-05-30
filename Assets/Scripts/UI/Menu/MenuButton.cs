using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour
{
    public RectTransform button;
    public GameObject indicator;
    public float offset = 10f;
    public float speed = 10f;

    public AudioClip hoverSound;

    private Vector2 origin;
    private bool selected = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        origin = button.anchoredPosition;
        indicator.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        bool hover = EventSystem.current.currentSelectedGameObject == gameObject;

        Vector2 targetPosition = hover ? origin + new Vector2(offset, 0) : origin;
        button.anchoredPosition = Vector2.Lerp(button.anchoredPosition, targetPosition, Time.deltaTime * speed);

        indicator.SetActive(hover);

        if (hover && !selected && hoverSound != null)
        {
            AudioController.Instance.PlaySFX(hoverSound);
        }

        selected = hover;
    }
}
