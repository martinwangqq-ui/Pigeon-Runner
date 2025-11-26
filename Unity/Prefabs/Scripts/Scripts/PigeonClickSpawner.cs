using UnityEngine;

public class PigeonClickSpawner : MonoBehaviour
{
    [Header("Quotes")]
    public Camera mainCamera;                 
    public PolygonCollider2D regionCollider;  
    public GameObject pigeonPrefab;          
    public GameObject goalPrefab;            
    public Transform levelRoot;

    [Header("Level Setting")]
    public LayerMask groundLayer;             

    [Header("Delivery Parameters")]
    public float pigeonSpawnOffsetUp = 2f;  
    public float goalOffsetUp = 0.3f;        

    [Header("Random Parameters")]
    public float minGoalRadius = 3f;      
    public float maxGoalRadius = 6f;       
    public int maxRandomTries = 50;      

    private GameObject currentPigeon;
    private GameObject currentGoal;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    void HandleClick()
    {
        if (mainCamera == null || regionCollider == null ||
            pigeonPrefab == null || goalPrefab == null || levelRoot == null)
        {
            Debug.LogWarning("PigeonClickSpawner: not input！");
            return;
        }

        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        worldPos.z = 0f;

        if (!IsPointInRegion(worldPos))
        {
            Debug.Log("Ignore");
            return;
        }

        Vector3 pigeonSpawnPos = worldPos + Vector3.up * pigeonSpawnOffsetUp;

        if (currentPigeon == null)
        {
            currentPigeon = Instantiate(pigeonPrefab, pigeonSpawnPos, Quaternion.identity, levelRoot);
        }
        else
        {
            currentPigeon.transform.SetPositionAndRotation(pigeonSpawnPos, Quaternion.identity);
        }

        Rigidbody2D rb = currentPigeon.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (currentGoal != null)
        {
            return;
        }

        Vector3 goalPos;
        if (!TryGetRandomGoalPositionAroundClick(worldPos, out goalPos))
        {
            goalPos = worldPos + new Vector3(minGoalRadius, 0f, 0f);
            Debug.LogWarning("Keep current goal point");
        }

        currentGoal = Instantiate(goalPrefab, goalPos, Quaternion.identity, regionCollider.transform);
        Debug.Log($"{goalPos} Randomly create the portal");
    }


    bool TryGetRandomGoalPositionAroundClick(Vector3 clickWorldPos, out Vector3 goalPos)
    {
        Bounds b = regionCollider.bounds;

        for (int t = 0; t < maxRandomTries; t++)
        {
            float radius = Random.Range(minGoalRadius, maxGoalRadius);
            float angle = Random.Range(0f, Mathf.PI * 2f);

            Vector3 offset = new Vector3(
                Mathf.Cos(angle),
                Mathf.Sin(angle),
                0f
            ) * radius;

            Vector3 sample = clickWorldPos + offset;

            if (!IsPointInRegion(sample))
                continue;

            Vector2 rayOrigin = new Vector2(sample.x, b.max.y + 5f);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, 100f, groundLayer);

            Vector3 basePos = sample;

            if (hit)
            {
                basePos = hit.point;
            }

            goalPos = basePos + Vector3.up * goalOffsetUp;
            return true;
        }

        goalPos = Vector3.zero;
        return false;
    }

    bool IsPointInRegion(Vector3 worldPoint)
    {
        Vector2 localPoint = regionCollider.transform.InverseTransformPoint(worldPoint);
        Vector2[] pts = regionCollider.points;

        bool inside = false;
        int j = pts.Length - 1;

        for (int i = 0; i < pts.Length; i++)
        {
            bool condY = (pts[i].y > localPoint.y) != (pts[j].y > localPoint.y);
            if (condY)
            {
                float xIntersection = pts[j].x + (localPoint.y - pts[j].y) * (pts[i].x - pts[j].x) / (pts[i].y - pts[j].y);
                if (localPoint.x < xIntersection)
                {
                    inside = !inside;
                }
            }
            j = i;
        }

        return inside;
    }
}
