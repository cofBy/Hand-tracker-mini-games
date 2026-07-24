using UnityEngine;

public class playerSnake : MonoBehaviour
{
    [Header("movement")]
    public sentisHandTracker handTracker;
    public Rigidbody2D rb;
    public float speed;

    [Header("camera movement")]
    public Camera cam;
    public float followTime;
    Vector2 vel;

    public float lookAhead;

    [Header("making the snake")]
    public GameObject snakeBodyPrefab;
    public Point animator;

    public int snakeLength;
    int oldSnakeLength = 0;

    [Header("shooting lazers")]
    public LineRenderer lazer;
    public int maxReflections;

    public float lazerUpTime;
    public float timeBetweenLazers;
    float lazerTimer;
    bool shootingLazer;

    public LayerMask wallMask;
    public LayerMask snakeMask;

    private void Update()
    {
        Vector2 dir = handTracker.palmCenter() - rb.position;
        rb.linearVelocity = dir.normalized * speed;

        handleCamera();

        if (oldSnakeLength != snakeLength)
        {
            int differance = snakeLength - oldSnakeLength;

            if (differance > 0)
            {
                oldSnakeLength += 1;
                GameObject segmentInstance = PoolManager.spawnObject(snakeBodyPrefab, Vector3.zero, Quaternion.identity);
                Point.segment bodySegment = new Point.segment { transform = segmentInstance.transform, distance = 1f };
                animator.Segments.Add(bodySegment);
            }
            else
            {
                oldSnakeLength -= 1;
                animator.Segments[animator.Segments.Count - 1].transform.gameObject.tag = "snake";
                animator.Segments.RemoveAt(animator.Segments.Count - 1);
            }
        }
        handleLazer();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("snake"))
        {
            other.gameObject.tag = "Untagged";
            PoolManager.ReturnToPool(other.gameObject);
            snakeLength += 1;
        }
    }

    void handleLazer()
    {
        lazerTimer += Time.deltaTime;
        if (lazerTimer >= timeBetweenLazers)
        {
            shootingLazer = lazerTimer <= timeBetweenLazers + lazerUpTime;
            if (lazerTimer > timeBetweenLazers + lazerUpTime)
            {
                shootingLazer = false;
                lazerTimer -= timeBetweenLazers + lazerUpTime;
            }
        }

        lazer.gameObject.SetActive(shootingLazer);
        if (shootingLazer == false) return;

        RaycastHit2D[] hits = new RaycastHit2D[maxReflections];
        for (int i = 0; i < maxReflections; i++)
        {
            if (i == 0)
            {
                hits[i] = Physics2D.Raycast((Vector2)transform.position + rb.linearVelocity.normalized * 0.5f, rb.linearVelocity.normalized, float.MaxValue);
                lazer.SetPosition(i, transform.position);

            }
            else if (i == 1)
            {
                Vector2 bounceDir = Vector2.Reflect(rb.linearVelocity.normalized, hits[i - 1].normal);
                hits[i] = Physics2D.Raycast(hits[i - 1].point - rb.linearVelocity.normalized * 0.1f, bounceDir, float.MaxValue);
            }
            else
            {
                Vector2 incomeDir = (hits[i - 1].point - hits[i - 2].point).normalized;
                Vector2 reflectDir = Vector2.Reflect(incomeDir, hits[i - 1].normal);

                hits[i] = Physics2D.Raycast(hits[i - 1].point - incomeDir * 0.1f, reflectDir, float.MaxValue);
            }

            if (hits[i] == true)
            {
                lazer.positionCount = i + 2;
                lazer.SetPosition(i + 1, hits[i].point);

                if ((snakeMask.value & (1 << hits[i].collider.gameObject.layer)) != 0)
                {
                    Point.segment burntSegment = new Point.segment { transform = hits[i].collider.gameObject.transform, distance = 1f };
                    if (animator.Segments.IndexOf(burntSegment) > 1)
                    {
                        snakeLength = animator.Segments.IndexOf(burntSegment) + 1;
                    }

                    break;
                }
            }
        }
    }
    void handleCamera()
    {
        Vector2 target = (Vector2)transform.position + (rb.linearVelocity.normalized * lookAhead);
        cam.transform.position = (Vector3)Vector2.SmoothDamp(cam.transform.position, target, ref vel, followTime) + new Vector3(0, 0, -10f);
    }
}
