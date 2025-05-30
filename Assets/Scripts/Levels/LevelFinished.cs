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
        if (flag) return;

        if (collider.CompareTag("Player"))
        {
            flag = true;
            int currentIndex = SceneManager.GetActiveScene().buildIndex;
            PlayerController.inputBlocked = true;
            SceneTransitionController.Instance.LoadScene(SceneManager.GetActiveScene().buildIndex + 1, SceneTransitionController.duration);
        }
    }
}