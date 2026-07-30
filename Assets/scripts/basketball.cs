using UnityEngine;

public class basketball : MonoBehaviour
{
    [Header("displaying movmentLine")]
    public LineRenderer line;

    [Header("movement")]
    public sentisHandTracker handTracker;

    public Rigidbody2D rb;
    public float forceMuliplier;

    public float maxDistance;

    Vector2 startingPos;
    bool wasFlexed;

    [Header("restarting")]
    public float timeToRestart;
    float restartTimer;

    Vector2 restartPos;
    float restartGravity;

    public float restartSpeed;

    [Header("scoring points")]
    public GameObject hoop;
    public scoreManager score;

    private void Start()
    {
        restartPos = transform.position;
        restartGravity = rb.gravityScale;
    }

    private void Update()
    {
        float hoverDistance = Vector2.Distance(handTracker.palmCenter(), transform.position);

        line.enabled = handTracker.isFlexed();

        Vector3 dir = Vector3.ClampMagnitude(handTracker.palmCenter() - startingPos, maxDistance);
        line.SetPosition(0, transform.position);
        line.SetPosition(1, dir + transform.position);

        if (wasFlexed != handTracker.isFlexed())
        {
            wasFlexed = handTracker.isFlexed();
            if (handTracker.isFlexed()) startingPos = handTracker.palmCenter();
            if (handTracker.isFlexed() == false && restartTimer <= 0)
            {
                rb.AddForce(-dir * forceMuliplier);
                restartTimer = timeToRestart;
            }
        }

        if (restartTimer <= 0)
        {
            transform.position = Vector2.MoveTowards(transform.position, restartPos, restartSpeed * Time.deltaTime);

            rb.gravityScale = 0;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0;
        }
        else
        {
            rb.gravityScale = restartGravity;
            restartTimer -= Time.deltaTime;
        }

        Vector3 hoopDir = (transform.position - hoop.transform.position);
        float dot = Vector3.Dot(hoopDir.normalized, hoop.transform.up);
        hoop.SetActive(dot < 0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("hoop"))
        {
            score.score += 1;
        }
    }

}
