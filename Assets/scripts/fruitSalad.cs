using UnityEngine;

public class fruitSalad : MonoBehaviour
{
    [Header("slashing")]
    public sentisHandTracker handTracker;
    public TrailRenderer target;
    public float speed;
    float maxTrailTime;
    public float trailLengthSpeed;

    [Header("spawning fruit")]
    public SpriteRenderer fruitPrefab;
    public Sprite[] fruitSprites;

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

    [Header("spawning bombs")]
    public SpriteRenderer bombPrefab;
    [Range(0, 1)] public float chanceToSpawnBomb;

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

                if (box.gameObject.CompareTag("bomb"))
                {
                    scoringManager.score -= 1;
                }
                else
                {
                    scoringManager.score += 1;
                }
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

            SpriteRenderer spawnPerfab = Random.Range(0f, 1f) <= chanceToSpawnBomb ? bombPrefab : fruitPrefab; 

            Vector2 randomPos = Camera.main.ScreenToWorldPoint(new Vector2(Random.Range(Screen.width / 2 - range.x, Screen.width / 2 + range.x), range.y));
            SpriteRenderer instance = PoolManager.SpawnObject(spawnPerfab, randomPos, Quaternion.identity);

            if (spawnPerfab == fruitPrefab)
            {
                instance.sprite = fruitSprites[Random.Range(0, 3)];
            }

            Vector2 dir = new Vector2(Random.Range(minForce.x, maxForce.x), Random.Range(minForce.y, maxForce.y));

            Rigidbody2D rb = instance.GetComponent<Rigidbody2D>();
            rb.AddForce(dir);
            rb.angularVelocity = Random.Range(-90f, 90f);
        }
    }
}
