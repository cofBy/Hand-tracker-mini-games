using UnityEngine;

public class fruitSalad : MonoBehaviour
{
    [Header("slashing")]
    public sentisHandTracker handTracker;
    public TrailRenderer target;
    public float speed;
    float maxTrailTime;
    public float trailLengthSpeed;

    [Header("spawning boxes")]
    public SpriteRenderer boxPrefab;
    public Gradient color;

    public Vector2 range;

    public float timeBetweenBoxes;
    float timer;

    public Vector2 minForce;
    public Vector2 maxForce;

    [Header("checking for cuts")]
    public float minVel;
    public float radius;
    public LayerMask boxMask;

    [Header("scoring system")]
    public scoreManager scoringManager;

    private void Awake()
    {
        maxTrailTime = target.time;
    }
    private void Update()
    {
        if (handTracker == null || handTracker.handLandmarks == null || handTracker.handLandmarks.Length < 1) return;

        target.transform.position = Vector3.MoveTowards(target.transform.position, handTracker.palmCenter(), speed * Time.deltaTime);
        if (Vector2.Distance(target.transform.position, handTracker.palmCenter()) > minVel)
        {
            target.time = Mathf.MoveTowards(target.time, maxTrailTime, trailLengthSpeed * Time.deltaTime);

            Collider2D[] slashs = Physics2D.OverlapCircleAll(target.transform.position, radius, boxMask);
            foreach (Collider2D box in slashs)
            {
                Vector2 dir = handTracker.palmCenter() - (Vector2)target.transform.position;
                float angle = Mathf.Atan2(-dir.x, dir.y) * Mathf.Rad2Deg;
                box.GetComponent<fruiteBox>().cut(angle);
                scoringManager.score += 1;
            }
        }
        else
        {
            target.time = Mathf.MoveTowards(target.time, 0, trailLengthSpeed * Time.deltaTime);
        }

        timer += Time.deltaTime;
        if (timer > timeBetweenBoxes)
        {
            timer = Random.Range(0.5f, timeBetweenBoxes - 1f);

            Vector2 randomPos = Camera.main.ScreenToWorldPoint(new Vector2(Random.Range(Screen.width / 2 - range.x, Screen.width / 2 + range.x), range.y));
            SpriteRenderer boxInstance = PoolManager.SpawnObject(boxPrefab, randomPos, Quaternion.identity);
            boxInstance.color = color.Evaluate(Random.Range(0f, 1f));

            Vector2 dir = new Vector2(Random.Range(minForce.x, maxForce.x), Random.Range(minForce.y, maxForce.y));

            Rigidbody2D rb = boxInstance.GetComponent<Rigidbody2D>();
            rb.AddForce(dir);
            rb.angularVelocity = Random.Range(-90f, 90f);
        }
    }
}
