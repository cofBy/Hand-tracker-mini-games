using UnityEngine;

public class playerMovement : MonoBehaviour
{
    [Header("steering")]
    public float linearSpeed;
    public float angularSpeed;

    public float maxSpeed;

    public Rigidbody2D rb;

    [Header("bumping")]
    public CapsuleCollider2D col;
    public float checkLength;
    public float strength;

    [Header("scoring points")]
    public scoreManager scoringSystem;

    public float resetTime;
    float resetTimer;

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

        resetTimer = Mathf.Max(resetTimer - Time.deltaTime, 0);

        handlBumping();
    }

    void handlBumping()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position + col.size.x * transform.right.normalized, transform.right.normalized, checkLength);

        if (hit)
        {
            if (resetTimer <= 0 && hit.collider.gameObject.CompareTag("car"))
            {
                resetTimer = resetTime;

                scoringSystem.score += 2;
            }

            Vector2 normal = new Vector2(-hit.normal.y, hit.normal.x).normalized;
            Vector2 bounceDir = Vector2.Reflect(-transform.right.normalized, normal);

            Debug.DrawRay(hit.point, normal * 50, Color.blue);
            Debug.DrawRay(transform.position, transform.right.normalized * 50, Color.yellow);
            Debug.DrawRay(transform.position, bounceDir * 50, Color.green);

            rb.linearVelocity = bounceDir.normalized * strength;
            hit.collider.attachedRigidbody.linearVelocity = Vector2.zero;
        }
    }
}