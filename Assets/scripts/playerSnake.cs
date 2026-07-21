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
        }
    }

    void handleCamera()
    {
        Vector2 target = (Vector2)transform.position + (rb.linearVelocity.normalized * lookAhead);
        cam.transform.position = (Vector3)Vector2.SmoothDamp(cam.transform.position, target, ref vel, followTime) + new Vector3(0, 0, -10f);
    }
}
