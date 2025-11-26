using UnityEngine;

public class PigeonController_New : MonoBehaviour
{
    public Rigidbody2D rb;
    public LayerMask groundLayer;

    public float normalGravity = 1f;
    public float fallingGravity = 0.4f;
    public float groundCheckDistance = 0.1f;
    public Vector2 groundCheckOffset = new Vector2(0, -0.1f);

    bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = normalGravity;
    }

    void Update()
    {
        CheckGround();

        if (isGrounded)
        {
            rb.gravityScale = normalGravity;
        }
        else
        {
            rb.gravityScale = fallingGravity;
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Goal"))
        {
            Debug.Log("You win！");



            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.simulated = false; 
            }

            PigeonClickSpawner spawner = FindObjectOfType<PigeonClickSpawner>();
            if (spawner != null)
            {
                spawner.enabled = false;
            }

        }
    }
    void CheckGround()
    {
        Vector2 origin = (Vector2)transform.position + groundCheckOffset;

        Vector2 downDir = -(Vector2)transform.up;

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            downDir,
            groundCheckDistance,
            groundLayer
        );

        Debug.DrawRay(origin, downDir * groundCheckDistance,
            hit ? Color.green : Color.red);

        isGrounded = hit.collider != null;
    }

}
