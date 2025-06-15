using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelFinished : MonoBehaviour
{
    private bool flag = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (flag)
            return;

        if (!collider.CompareTag("Player"))
            return;

        flag = true;
        PlayerController.inputBlocked = true;

        float time = LevelTimer.Instance.GetTime();
        LevelTimer.Instance.StopTimer();

        string name = SceneManager.GetActiveScene().name;
        int index = SceneManager.GetActiveScene().buildIndex;

        DataController.Instance.SaveLevel(name, time, index);
        SceneTransitionController.Instance.LoadScene(index + 1, SceneTransitionController.duration);
    }
}