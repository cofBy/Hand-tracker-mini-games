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

    [Header("moving the hoop")]
    public Transform hoopObject;
    Vector2 hoopTarget;

    [Header("scoring points")]
    public GameObject hoopCollider;
    public scoreManager score;

    private void Start()
    {
        restartPos = transform.position;
        restartGravity = rb.gravityScale;
        hoopTarget = new Vector2(hoopObject.position.x, Random.Range(8f, -7.5f));
    }

    private void Update()
    {
        if (score.timer < 0) return;
        float hoverDistance = Vector2.Distance(handTracker.palmCenter(), transform.position);

        line.enabled = handTracker.isFlexed() && restartTimer <= 0;

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

                hoopTarget = new Vector2(hoopObject.position.x, Random.Range(8f, -7.5f));
                restartTimer = timeToRestart;
            }
        }

        if (restartTimer <= 0)
        {
            transform.position = Vector2.MoveTowards(transform.position, restartPos, restartSpeed * Time.deltaTime);

            hoopObject.position = Vector2.MoveTowards(hoopObject.position, hoopTarget, restartSpeed * Time.deltaTime);

            rb.gravityScale = 0;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0;
        }
        else
        {
            rb.gravityScale = restartGravity;
            restartTimer -= Time.deltaTime;
        }

        Vector3 hoopDir = (transform.position - hoopCollider.transform.position);
        float dot = Vector3.Dot(hoopDir.normalized, hoopCollider.transform.up);
        hoopCollider.SetActive(dot < 0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("hoop"))
        {
            score.score += 1;
        }
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("bounds"))
        {
            FEEL.PlaySound("lightImpactBasic");
        }
    }

}
