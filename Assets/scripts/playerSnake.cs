using UnityEngine;

public class playerSnake : MonoBehaviour
{
    [Header("movement")]
    public Rigidbody2D rb;

    [Header("making the snake")]
    public GameObject snakeBodyPrefab;
    public Point animator;

    public int snakeLength;
    int oldSnakeLength = 0;

    private void Update()
    {
        if (oldSnakeLength != snakeLength)
        {
            int differance = snakeLength - oldSnakeLength;

            if (differance > 0)
            {
                oldSnakeLength += 1;
                GameObject segmentInstance = PoolManager.spawnObject(snakeBodyPrefab, Vector3.zero, Quaternion.identity);
                Point.segment bodySegment = new Point.segment { transform = segmentInstance.transform, distance = 1 };
                animator.Segments.Add(bodySegment);
            }
        }
    }
}
