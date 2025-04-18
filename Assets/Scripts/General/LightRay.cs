using UnityEngine;

public class LightRay : MonoBehaviour
{
    // publicas
    public LayerMask bounceLayer;
    public int maxBounces;
    public LineRenderer ray;
    public LayerMask targetLayer;

    // privadas
    private Vector2 origin;
    private Vector2 direction;
    private GameObject currentTarget;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ray = GetComponent<LineRenderer>();

        ray.positionCount = 1;
        ray.SetPosition(0, transform.position);
        ray.startWidth = 0.1f;
        ray.endWidth = 0.1f;

        origin = transform.position;
        direction = transform.right;
    }

    // Update is called once per frame
    void Update()
    {
        CastRay();
    }

    void CastRay()
    {
        ray.positionCount = 1;
        ray.SetPosition(0, origin);

        Vector2 currentOrigin = origin;
        Vector2 currentDirection = direction;
        int bounceCount = 0;

        GameObject target = null;

        while (bounceCount < maxBounces)
        {
            RaycastHit2D hit = Physics2D.Raycast(currentOrigin, currentDirection, Mathf.Infinity, bounceLayer);

            if (hit.collider != null)
            {
                ray.positionCount += 1;
                ray.SetPosition(ray.positionCount - 1, hit.point);

                Vector2 hitNormal = hit.normal;

                //normal de la dirección del rebote y el origen
                Debug.DrawRay(hit.point, hitNormal * 2f, Color.blue, 1f);

                //rebote
                Vector2 newDir = Vector2.Reflect(currentDirection, hitNormal);
                Debug.DrawRay(hit.point, newDir * 2f, Color.red, 1f);

                currentOrigin = hit.point + hit.normal * 0.01f; //desplazar origen del rebote para quie no se buguee
                currentDirection = newDir.normalized; //direcciorebot con vector ya normalizado

                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Target"))
                {
                    SpriteRenderer sr = hit.collider.GetComponent<SpriteRenderer>();
                    if (sr != null)
                        sr.color = Color.green;

                    target = hit.collider.gameObject;
                    break;
                }

                bounceCount++;
            }
            else
            {
                ray.positionCount += 1;
                ray.SetPosition(ray.positionCount - 1, currentOrigin + currentDirection * 25f);
                break;
            }
        }

        if (currentTarget != null && target != currentTarget)
        {
            SpriteRenderer sr = currentTarget.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = Color.black;

            currentTarget = null;
        }

        if (target != null)
            currentTarget = target;
    }

}
