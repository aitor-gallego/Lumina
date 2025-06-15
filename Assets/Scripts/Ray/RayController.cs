using UnityEngine;
using System.Collections.Generic;

public class RayController : MonoBehaviour
{
    // publicas
    [Header("config")]
    public LayerMask interactLayer;
    public int maxBounces;
    public LineRenderer ray;

    [Header("efectos")]
    public GameObject rayAnimatorTemplate;
    public AudioClip sfx;

    // privadas
    private Vector2 origin;
    private Vector2 direction;
    private List<LineRenderer> lines;
    private LineRenderer line;
    private bool teleported = false;
    private ActivateDoor lastTarget = null;
    private Vector2? lastImpactPoint = null;
    private GameObject currentSpread = null;
    private float minDistance = 0.1f;
    private GameObject lastParticles = null;
    private bool targeting = false;
    private bool sfxplayed = false;

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

        targeting = false;

        CreateSegment(origin);
        CastRay();

        if (!targeting && lastParticles != null)
        {
            lastParticles.SetActive(false);
            lastParticles = null;
        }

        if (!targeting && sfxplayed)
        {
            sfxplayed = false;
        }
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

    void CastRay()
    {
        Vector2 currentOrigin = origin;
        Vector2 currentDirection = direction;
        int bounces = 0;

        while (bounces < maxBounces)
        {
            RaycastHit2D hit = Physics2D.Raycast(currentOrigin, currentDirection, Mathf.Infinity, interactLayer);

            if (hit.collider == null)
            {
                line.positionCount++;
                Vector2 endPoint = currentOrigin + currentDirection * 25f;
                line.SetPosition(line.positionCount - 1, endPoint);
                ClearSpread();
                break;
            }

            line.positionCount++;
            line.SetPosition(line.positionCount - 1, hit.point);

            if (hit.collider.CompareTag("Target"))
            {
                targeting = true;
                Transform targetTransform = hit.collider.transform;
                Transform particleTransform = targetTransform.Find("ParticleSystem");

                if (particleTransform != null)
                {
                    GameObject particles = particleTransform.gameObject;

                    if (lastParticles != null && lastParticles != particles)
                    {
                        lastParticles.SetActive(false);
                    }

                    particles.SetActive(true);
                    lastParticles = particles;
                }

                if (!sfxplayed)
                {
                    AudioController.Instance.PlaySFX(sfx);
                    sfxplayed = true;
                }

                Transform parent = hit.collider.transform.parent;
                if (parent != null)
                {
                    ClearSpread();
                    ActivateDoor door = parent.GetComponentInChildren<ActivateDoor>();
                    if (door != null)
                    {
                        Target(door);
                        break;
                    }
                }

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
                break;
            }

            CreateSegment(currentOrigin);
        }
    }

    void DrawRay()
    {
        ray.positionCount = 1;
        ray.SetPosition(0, transform.position);
        ray.startWidth = 0.1f;
        ray.endWidth = 0.1f;
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
        if (portal == null || portal.linked == null) return;

        Transform portal2 = portal.linked.transform;
        Vector2 position = portal2.position;
        Vector2 direction = (-(Vector2)portal2.up).normalized;

        currentOrigin = position;
        currentDirection = direction;
    }

    void Spread(Vector2 position)
    {
        if (currentSpread == null)
        {
            currentSpread = Instantiate(rayAnimatorTemplate, position, Quaternion.identity);
            RayAnimator animator = currentSpread.GetComponent<RayAnimator>();
            if (animator != null)
                animator.Play(origin, position);

            lastImpactPoint = position;
            return;
        }
        if (Vector2.Distance(position, lastImpactPoint.Value) > minDistance)
        {
            Destroy(currentSpread);
            currentSpread = Instantiate(rayAnimatorTemplate, position, Quaternion.identity);
            RayAnimator animator = currentSpread.GetComponent<RayAnimator>();
            if (animator != null)
                animator.Play(origin, position);

            lastImpactPoint = position;
        }
        else
        {
            currentSpread.transform.position = position;

            RayAnimator animator = currentSpread.GetComponent<RayAnimator>();
            if (animator != null)
                animator.Play(origin, position);

            lastImpactPoint = position;
        }
    }

    void ClearSpread()
    {
        if (currentSpread != null)
        {
            Destroy(currentSpread);
            currentSpread = null;
        }
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
