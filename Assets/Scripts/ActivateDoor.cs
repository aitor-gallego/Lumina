using UnityEngine;

public class ActivateDoor : MonoBehaviour
{
    private Vector3 origin;
    private Vector3 end;
    private BoxCollider2D boxcollider;
    private bool closing = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        origin = transform.position;
        end = origin;
        boxcollider = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, end, 2f * Time.deltaTime);

        if (closing && Vector3.Distance(transform.position, origin) < 0.01f)
        {
            if (boxcollider != null)
                boxcollider.enabled = true;

            closing = false;
        }
    }

    public void Open()
    {
        end = origin + Vector3.up * 2f;
        if (boxcollider != null)
            boxcollider.enabled = false;
    }

    public void Close()
    {
        end = origin;
        closing = true;
    }
}
