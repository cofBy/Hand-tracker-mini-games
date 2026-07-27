using System.Collections.Generic;
using System.Linq;
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

    [Header("states logic")]
    public float minDistnace;
    public snakeLogic player;
    public LayerMask playerMask;

    public LayerMask foodMask;

    public float lazyness;
    public float timeTargetChange;
    float randomTimer;

    private void Awake()
    {
        states = new List<float>(4) { 0, 0, 0, 0 };
    }
    private void Update()
    {
        Vector2 lookDir = rb.linearVelocity;
        Vector2 playerDir = (player.transform.position - transform.position);
        Vector2 playerLookDir = player.rb.linearVelocity;

        Debug.DrawRay(Vector2.zero, playerDir.normalized, Color.red);
        Debug.DrawRay(Vector2.zero, -playerLookDir.normalized, Color.green);

        bool lineOfSight = Physics2D.Raycast(transform.position, lookDir, 99f, playerMask);
        states[0] = Mathf.Max(Vector2.Dot(lookDir.normalized, playerDir.normalized), 0) * (lineOfSight ? 1f: 0.2f) * (playerDir.magnitude < minDistnace ? 0f : 1f);

        states[1] = Mathf.Max(Vector2.Dot(playerLookDir.normalized, -playerDir.normalized), 0);

        Collider2D[] food = Physics2D.OverlapCircleAll(transform.position, minDistnace, foodMask);
        Collider2D closestFood = food.OrderBy(f => Vector2.Distance(transform.position, f.transform.position)).FirstOrDefault();
        if (food.Length > 0)
        {
            states[2] = Mathf.Abs(1 - Vector2.Distance(transform.position, closestFood.transform.position) / minDistnace);
        }
        else
        {
            states[2] = 0;
        }

        states[3] = lazyness;

        currentState = states.IndexOf(Mathf.Max(states.ToArray()));

        if (currentState == 0)
        {
            randomTimer -= Time.deltaTime;
            if (randomTimer < 0)
            {
                randomTimer = timeTargetChange;
                followPoint = player.transform.position + new Vector3(Random.Range(2f, 2f), Random.Range(-2f, 2f));
            }
        }
        else if (currentState == 1)
        {
            followPoint = -player.transform.position;
        }
        else if (currentState == 2)
        {
            followPoint = closestFood.transform.position;
        }
        else
        {
            randomTimer -= Time.deltaTime;
            if (randomTimer < 0)
            {
                randomTimer = timeTargetChange;
                followPoint = new Vector2(Random.Range(-10f, 10f), Random.Range(-10f, 10f));
            }
        }
    }
    private void FixedUpdate()
    {
        Vector2 dir = followPoint - rb.position;
        rb.linearVelocity = dir.normalized * speed;
        rb.SetRotation(Mathf.Atan2(rb.linearVelocityY, rb.linearVelocityX));
    }
}
