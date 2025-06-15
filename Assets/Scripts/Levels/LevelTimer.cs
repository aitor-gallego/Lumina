using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelTimer : MonoBehaviour
{
    public static LevelTimer Instance;

    private float timer;
    private bool playing = false;
    public TextMeshProUGUI text;

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

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {
        if (!playing) return;

        timer += Time.deltaTime;
        TimerText();
    }

    private void TimerText()
    {
        if (text != null)
        {
            int minutes = Mathf.FloorToInt(timer / 60f);
            int seconds = Mathf.FloorToInt(timer % 60f);
            int milliseconds = Mathf.FloorToInt(timer * 1000f % 1000f);
            text.text = $"{minutes:00}:{seconds:00}.{milliseconds / 10:00}";
        }
    }

    public void StartTimer()
    {
        timer = 0f;
        playing = true;
    }

    public void StopTimer()
    {
        playing = false;
    }

    public float GetTime()
    {
        return timer;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string name = SceneManager.GetActiveScene().name;

        if (name == "Final" || name == "Menu")
        {
            text.text = "";
            StopTimer();
        }

        else
        {
            StartTimer();
        }
    }
}

