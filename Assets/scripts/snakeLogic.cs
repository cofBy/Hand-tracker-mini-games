using UnityEngine;

public class snakeLogic : MonoBehaviour
{
    [Header("making the snake")]
    public GameObject snakeBodyPrefab;
    public GameObject foodPrefab;
    public Point animator;
    public LineRenderer bodyRenderer;

    public int snakeLength;
    int oldSnakeLength = 0;

    [Header("shooting lazers")]
    public Rigidbody2D rb;
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
        bodyRenderer.positionCount = animator.Segments.Count;
        for (int i = 0; i < animator.Segments.Count; i++)
        {
            bodyRenderer.SetPosition(i, animator.Segments[i].transform.position);
        }

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
                Point.segment old = animator.Segments[animator.Segments.Count - 1];
                PoolManager.spawnObject(foodPrefab, old.transform.position, Quaternion.identity);
                PoolManager.ReturnToPool(old.transform.gameObject);
                animator.Segments.Remove(old);
            }
        }
        handleLazer();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("food"))
        {
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
}
