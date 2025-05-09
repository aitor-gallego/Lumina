using UnityEngine;
using System.Collections.Generic;

public class RayController : MonoBehaviour
{
    // publicas
    public LayerMask interactLayer;
    public int maxBounces;
    public LineRenderer ray;
    public GameObject spreadPref;

    // privadas
    private Vector2 origin;
    private Vector2 direction;
    private GameObject target;
    private GameObject spread;
    private List<LineRenderer> lines;
    private LineRenderer line;
    private bool teleported = false;
    private ActivateDoor currentTarget = null;
    private ActivateDoor lastTarget = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ray = GetComponent<LineRenderer>();
        DrawRay();

        origin = transform.position;
        direction = transform.right;

        lines = new List<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var segment in lines)
        {
            Destroy(segment.gameObject);
        }
        lines.Clear();
        teleported = false;

        origin = transform.position;
        direction = transform.right;
        Target(null);

        CreateSegment(origin);
        CastRay();
    }

    void CastRay()
    {
        Vector2 currentOrigin = origin;
        Vector2 currentDirection = direction;
        int bounces = 0;
        bool didSpread = false;

        while (bounces < maxBounces)
        {
            RaycastHit2D hit = Physics2D.Raycast(currentOrigin, currentDirection, Mathf.Infinity, interactLayer);

            if (hit.collider == null)
            {
                line.positionCount++;
                Vector2 endPoint = currentOrigin + currentDirection * 25f;
                line.SetPosition(line.positionCount - 1, endPoint);
                break;
            }

            line.positionCount++;
            line.SetPosition(line.positionCount - 1, hit.point);

            if (hit.collider.CompareTag("Target"))
            {
                ActivateDoor door = hit.collider.GetComponentInChildren<ActivateDoor>();
                Target(door);
                break;
            }
            else if (hit.collider.CompareTag("Reflect"))
            {
                Reflection(ref currentOrigin, ref currentDirection, hit.point, hit.normal);
                teleported = false;
                bounces++;
            }
            else if (hit.collider.CompareTag("Teleport") && !teleported)
            {
                teleported = true;
                Teleport(hit.collider.gameObject, ref currentOrigin, ref currentDirection);
                CreateSegment(currentOrigin);
                bounces++;
            }
            else
            {
                Spread(hit.point);
                didSpread = true;
                break;
            }

            CreateSegment(currentOrigin);
        }

        if (!didSpread && spread != null)
        {
            Destroy(spread);
            spread = null;
        }
    }

    void DrawRay()
    {
        ray.positionCount = 1;
        ray.SetPosition(0, transform.position);
        ray.startWidth = 0.15f;
        ray.endWidth = 0.15f;
        ray.startColor = Color.yellow;
        ray.sortingLayerName = "Ray";
    }

    void Reflection(ref Vector2 currentOrigin, ref Vector2 currentDirection, Vector2 point, Vector2 normal)
    {
        Vector2 nd = Vector2.Reflect(currentDirection, normal).normalized;
        currentOrigin = point + nd * 0.00001f;
        currentDirection = nd;
    }

    void Teleport(GameObject collider, ref Vector2 currentOrigin, ref Vector2 currentDirection)
    {
        Portal portal = collider.GetComponent<Portal>();
        if (portal == null || portal.linkedPortal == null) return;

        Transform portal2 = portal.linkedPortal.transform;
        Vector2 position = portal2.position;
        Vector2 direction = (-(Vector2)portal2.up).normalized;

        currentOrigin = position;
        currentDirection = direction;
    }

    void CreateSegment(Vector2 startPos)
    {
        var go = new GameObject("RaySegment");
        go.transform.parent = transform;
        var lr = go.AddComponent<LineRenderer>();

        lr.startWidth = ray.startWidth;
        lr.endWidth = ray.endWidth;
        lr.material = ray.material;
        lr.positionCount = 1;
        lr.SetPosition(0, startPos);
        lr.sortingLayerName = "Ray";

        lines.Add(lr);
        line = lr;
    }

    void Spread(Vector2 position)
    {
        if (spread == null)
            spread = Instantiate(spreadPref, position, Quaternion.identity);
        else
            spread.transform.position = new(position.x, position.y, -1);
    }

    void Target(ActivateDoor door)
    {
        if (door != null)
        {
            door.Open();
        }

        if (lastTarget != null && lastTarget != door)
        {
            lastTarget.Close();
        }

        lastTarget = door;
    }
}
