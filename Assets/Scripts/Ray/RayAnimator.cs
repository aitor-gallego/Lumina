using UnityEngine;

public class RayAnimator : MonoBehaviour
{
    public ParticleSystem diff;
    public ParticleSystem spread;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Play(Vector2 startPos, Vector2 endPos)
    {
        if (diff != null)
        {
            diff.transform.position = startPos;
            diff.Play();
        }

        if (spread != null)
        {
            spread.transform.position = endPos;
            spread.Play();
        }
    }
}
