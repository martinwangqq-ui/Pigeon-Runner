using UnityEngine;

public class GoalClickTest : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject goalPrefab;
    public Transform levelRoot;

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
        if (mainCamera == null || goalPrefab == null || levelRoot == null)
        {
            Debug.LogWarning("GoalClickTest: no input");
            return;
        }

        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        worldPos.z = 0f;

        if (currentGoal != null)
        {
            Destroy(currentGoal);
        }

        Vector3 goalPos = worldPos + new Vector3(2f, 0f, 0f);

        currentGoal = Instantiate(goalPrefab, goalPos, Quaternion.identity, levelRoot);

        Debug.Log($"[GoalClickTest]  {goalPos} Create a new goal point");
    }
}
