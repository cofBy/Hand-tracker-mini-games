using System.Collections.Generic;
using UnityEngine;

public class enemySnake : MonoBehaviour
{
    [Header("state machines")]
    public List<float> states; // 0 = attack, 1 = defend, 2 = eat, 3 = idle
    int currentState;

    [Header("moving")]
    public Rigidbody2D rb;
    public float speed;
    Vector2 followPoint;

    [Header("attacking")]
    public snakeLogic player;
    public LayerMask playerMask;

    private void Awake()
    {
        states = new List<float>(4) { 0, 0, 0, 0 };
    }
    private void Update()
    {
        Vector2 lookDir = (followPoint - rb.position).normalized;
        Vector2 playerDir = (player.transform.position - transform.position).normalized;
        bool lineOfSight = Physics2D.Raycast(transform.position, lookDir, 99f, playerMask);
        states[0] = Mathf.Max(Vector2.Dot(lookDir, playerDir), 0) * (lineOfSight ? 1f: 0.2f);

        currentState = states.IndexOf(Mathf.Max(states.ToArray()));

        if (currentState == 0)
        {
            followPoint = player.transform.position;
        }
    }
    private void FixedUpdate()
    {
        Vector2 dir = followPoint - rb.position;
        rb.linearVelocity = dir.normalized * speed;
        rb.SetRotation(Mathf.Atan2(rb.linearVelocityY, rb.linearVelocityX));
    }
}
