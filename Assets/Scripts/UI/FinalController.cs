using UnityEngine;

public class FinalController : MonoBehaviour
{
    private const float seconds = 5.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        StartCoroutine(WaitAndReturnMenu());
    }

    // Update is called once per frame
    void Update()
    {

    }


    private System.Collections.IEnumerator WaitAndReturnMenu()
    {
        yield return new WaitForSecondsRealtime(seconds);
        SceneTransitionController.Instance.LoadScene(0, SceneTransitionController.duration);
    }
}
