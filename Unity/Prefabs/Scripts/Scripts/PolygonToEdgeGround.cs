using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(EdgeCollider2D))]
public class PolygonToEdgeGround : MonoBehaviour
{
    public bool generateOnStart = true;

    PolygonCollider2D poly;
    EdgeCollider2D edge;

    void Awake()
    {
        poly = GetComponent<PolygonCollider2D>();
        edge = GetComponent<EdgeCollider2D>();
    }

    void Start()
    {
        if (generateOnStart)
            GenerateEdgeFromPolygon();
    }

    public void GenerateEdgeFromPolygon()
    {
        Vector2[] pts = poly.points;

        Vector2[] edgePts = new Vector2[pts.Length + 1];

        for (int i = 0; i < pts.Length; i++)
            edgePts[i] = pts[i];

        edgePts[pts.Length] = pts[0];

        edge.points = edgePts;

        poly.enabled = false;

        Debug.Log("PolygonToEdgeGround: EdgeCollider is generated and enabled based on Polygon, while physics is disabled for Polygon.");
    }
}
