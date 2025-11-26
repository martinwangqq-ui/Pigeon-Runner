using UnityEngine;

public class LevelRotator : MonoBehaviour
{
    [Header("Rotate Speed")]
    public float rotateSpeed = 40f;

    [Header("Rotate Angle")]
    public float maxAngle = 45f;

    private float currentAngle = 0f;

    void Update()
    {
        float input = 0f;

        if (Input.GetKey(KeyCode.Q))
            input += 1f;

        if (Input.GetKey(KeyCode.E))
            input -= 1f;

        if (Mathf.Abs(input) > 0f)
        {
            currentAngle += input * rotateSpeed * Time.deltaTime;
            currentAngle = Mathf.Clamp(currentAngle, -maxAngle, maxAngle);

            transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
        }
    }
}
