using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ButtonHover : MonoBehaviour
{
    public AudioClip sfx;
    private Color32 origincolor = new(170, 170, 170, 255);
    private Color32 hovercolor = new(255, 255, 255, 255);
    private Vector3 originscale;
    private bool selected = false;
    private TextMeshProUGUI text;
    private Slider slider;
    private Image background;
    private Image fill;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originscale = transform.localScale;

        text = GetComponentInChildren<TextMeshProUGUI>(true);
        if (text != null)
            text.color = origincolor;

        slider = GetComponentInChildren<Slider>(true);
        if (slider != null)
        {
            var cb = slider.colors;
            cb.normalColor = origincolor;
            slider.colors = cb;

            Transform bgTransform = slider.transform.Find("Background");
            if (bgTransform != null)
                background = bgTransform.GetComponent<Image>();
            if (background != null)
                background.color = origincolor;

            Transform fillTransform = slider.transform.Find("Fill Area/Fill");
            if (fillTransform != null)
                fill = fillTransform.GetComponent<Image>();
            if (fill != null)
                fill.color = origincolor;
        }
    }

    // Update is called once per frame
    void Update()
    {
        GameObject currentselected = EventSystem.current.currentSelectedGameObject;
        bool hover = false;

        if (currentselected != null)
        {
            if (currentselected == gameObject)
                hover = true;
            else if (currentselected.transform.IsChildOf(transform))
                hover = true;
        }

        if (hover && !selected && sfx != null)
            AudioController.Instance.PlaySFX(sfx);

        float lerpSpeed = Time.unscaledDeltaTime * 10f;

        Vector3 targetscale = hover ? originscale * 1.1f : originscale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetscale, lerpSpeed);

        if (text != null)
        {
            Color targetcolor = hover ? hovercolor : origincolor;
            text.color = Color.Lerp(text.color, targetcolor, lerpSpeed);
        }

        if (slider != null)
        {
            Color targetBlockColor = hover ? hovercolor : origincolor;
            var cb = slider.colors;
            if (cb.normalColor != targetBlockColor)
            {
                cb.normalColor = targetBlockColor;
                slider.colors = cb;
            }

            if (background != null)
            {
                Color targetBgColor = hover ? hovercolor : origincolor;
                background.color = Color.Lerp(background.color, targetBgColor, lerpSpeed);
            }

            if (fill != null)
            {
                Color targetFillColor = hover ? hovercolor : origincolor;
                fill.color = Color.Lerp(fill.color, targetFillColor, lerpSpeed);
            }
        }

        selected = hover;
    }

}
