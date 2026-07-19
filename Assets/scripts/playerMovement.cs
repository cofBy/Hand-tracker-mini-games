using UnityEngine;

public class playerMovement : MonoBehaviour
{
    [Header("steering")]
    public float linearSpeed;
    public float angularSpeed;

    public float maxSpeed;

    public Rigidbody2D rb;

    [Header("getting steering target")]
    public sentisHandTracker handTracker;

    private void Update()
    {
        Vector2 dir = rb.position - handTracker.palmCenter();

        float breakMultiplier = Mathf.Clamp(dir.magnitude, -0.25f, 1f);
        rb.linearVelocity += (Vector2)transform.right * linearSpeed;
        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed) * breakMultiplier;

        if (breakMultiplier > 0.1f)
        {
            rb.angularVelocity = Vector2.SignedAngle(dir.normalized, transform.right) * angularSpeed * breakMultiplier;
        }
    }
}