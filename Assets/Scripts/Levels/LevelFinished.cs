using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelFinished : MonoBehaviour
{
    public string playertag = "Player";
    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

        if (other.CompareTag(playertag))
        {
            triggered = true;
            int currentIndex = SceneManager.GetActiveScene().buildIndex;
            PlayerController.inputBlocked = true;
            LevelProgress.UnlockLevel(currentIndex + 1);
            SceneTransitionController.Instance.LoadScene(SceneManager.GetActiveScene().buildIndex + 1, SceneTransitionController.duration);
        }
    }
}