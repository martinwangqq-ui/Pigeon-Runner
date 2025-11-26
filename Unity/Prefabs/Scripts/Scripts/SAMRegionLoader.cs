using System;
using UnityEngine;

[Serializable]
public class SAMRegionFile
{
    public int width;
    public int height;
    public SAMRegion[] regions;
}

[Serializable]
public class SAMRegion
{
    public int id;
    public float[] points;   // [x0, y0, x1, y1, ...] 像素坐标
}

public class SAMRegionLoader : MonoBehaviour
{
    [Header("输入的 JSON（SAM2 导出的）")]
    public TextAsset regionJson;           // test_region.json
    public PolygonCollider2D polygon;      // Background 的 PolygonCollider2D
    public SpriteRenderer spriteRenderer;  // Background 的 SpriteRenderer

    [Header("选项")]
    public int regionIndex = 0;            // 默认使用第 0 个区域

    void Start()
    {
        if (regionJson == null || polygon == null || spriteRenderer == null)
        {
            Debug.LogWarning("❗ SAMRegionLoader：引用没有填完整！");
            return;
        }

        ApplyRegionFromJson();
    }

    public void ApplyRegionFromJson()
    {
        Debug.Log($"[SAMRegionLoader] JSON: {regionJson.name}");
        Debug.Log($"[SAMRegionLoader] JSON length: {regionJson.text.Length}");

        SAMRegionFile data = JsonUtility.FromJson<SAMRegionFile>(regionJson.text);
        if (data == null)
        {
            Debug.LogWarning("❗ data == null，JsonUtility loading failed");
            return;
        }

        Debug.Log($"[SAMRegionLoader] width={data.width}, height={data.height}");
        Debug.Log($"[SAMRegionLoader] regions whether is null: {(data.regions == null)}");

        if (data.regions == null || data.regions.Length == 0)
        {
            Debug.LogWarning("❗ SAMRegionLoader：JSON failed or no regions。");
            return;
        }

        Debug.Log($"[SAMRegionLoader] regions.Length = {data.regions.Length}, current regionIndex = {regionIndex}");

        if (regionIndex < 0 || regionIndex >= data.regions.Length)
        {
            Debug.LogWarning("❗ SAMRegionLoader：regionIndex will be 0.");
            regionIndex = 0;
        }

        SAMRegion region = data.regions[regionIndex];
        if (region.points == null)
        {
            Debug.LogWarning("❗ SAMRegionLoader：region.points is null。");
            return;
        }

        Debug.Log($"[SAMRegionLoader] region.points.Length = {region.points.Length}");

        if (region.points.Length < 6 || region.points.Length % 2 != 0)
        {
            Debug.LogWarning("❗ SAMRegionLoader：region.points data illegal");
            return;
        }

        Sprite sprite = spriteRenderer.sprite;
        if (sprite == null)
        {
            Debug.LogWarning("❗ SAMRegionLoader：SpriteRenderer no sprite。");
            return;
        }

        float imgWidthWorld = sprite.bounds.size.x;
        float imgHeightWorld = sprite.bounds.size.y;

        int imgWidthPx = data.width;
        int imgHeightPx = data.height;

        int count = region.points.Length / 2;
        Vector2[] colliderPoints = new Vector2[count];

        for (int i = 0; i < count; i++)
        {
            float px = region.points[2 * i + 0];
            float py = region.points[2 * i + 1];

            float nx = px / imgWidthPx;
            float ny = py / imgHeightPx;
            float invNy = 1f - ny;

            float localX = (nx - 0.5f) * imgWidthWorld;
            float localY = (invNy - 0.5f) * imgHeightWorld;

            colliderPoints[i] = new Vector2(localX, localY);
        }

        polygon.pathCount = 1;
        polygon.SetPath(0, colliderPoints);

        Debug.Log($"Loading finished, found {count} vertexs. polygon.pathCount = {polygon.pathCount}");
    }

}
