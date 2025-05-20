using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public CanvasGroup fade;
    public CanvasGroup canvas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Play()
    {
        SceneTransitionController.Instance.LoadScene(SceneManager.GetActiveScene().buildIndex + 1, SceneTransitionController.duration);
    }

    public void Settings()
    {
        Debug.Log("settings");
    }

    public void Credits()
    {
        Debug.Log("credits");
    }
}
