using UnityEngine;

public class RayAnimator : MonoBehaviour
{
    public ParticleSystem startEffect;
    public ParticleSystem endEffect;

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
        if (startEffect != null)
        {
            startEffect.transform.position = startPos;
            startEffect.Play();
        }

        if (endEffect != null)
        {
            endEffect.transform.position = endPos;
            endEffect.Play();
        }
    }
}
