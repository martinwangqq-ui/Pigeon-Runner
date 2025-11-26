using UnityEngine;

public class BackgroundZoom : MonoBehaviour
{
    [Header("Speed")]
    public float zoomSpeed = 1f;

    [Header("Min£¨1 = Original£©")]
    public float minScale = 0.8f;

    [Header("Max")]
    public float maxScale = 2.5f;

    private Vector3 startScale;

    void Start()
    {
        startScale = transform.localScale;
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.0001f)
        {

            float newScale = transform.localScale.x + scroll * zoomSpeed;


            newScale = Mathf.Clamp(newScale, minScale, maxScale);


            transform.localScale = new Vector3(newScale, newScale, 1f);
        }
    }
}
