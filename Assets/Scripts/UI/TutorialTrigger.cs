using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    private GameObject ui;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (transform.childCount > 0)
        {
            ui = transform.GetChild(0).gameObject;
            ui.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && ui != null)
        {
            ui.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && ui != null)
        {
            ui.SetActive(false);
        }
    }
}
