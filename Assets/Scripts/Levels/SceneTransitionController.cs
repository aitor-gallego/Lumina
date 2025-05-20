using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionController : MonoBehaviour
{
    public static SceneTransitionController Instance;
    public CanvasGroup fade;
    private bool transitioning = false;
    public static float duration = 0.8f;
    public static float resetDuration = 0.25f;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FadeIn(duration));
    }

    public void LoadScene(int index, float duration)
    {
        if (!transitioning)
            StartCoroutine(FadeOutAndLoad(index, duration));
    }

    public void ResetScene()
    {
        if (!transitioning)
            StartCoroutine(FadeOutAndLoad(SceneManager.GetActiveScene().buildIndex, resetDuration));
    }

    private IEnumerator FadeIn(float duration)
    {
        transitioning = true;
        float i = duration;
        while (i > 0f)
        {
            i -= Time.unscaledDeltaTime;
            fade.alpha = i / duration;
            yield return null;
        }
        fade.alpha = 0f;
        transitioning = false;
    }

    private IEnumerator FadeOutAndLoad(int index, float duration)
    {
        transitioning = true;
        float i = 0f;
        while (i < duration)
        {
            i += Time.unscaledDeltaTime;
            fade.alpha = i / duration;
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.1f);
        SceneManager.LoadScene(index);
    }
}
