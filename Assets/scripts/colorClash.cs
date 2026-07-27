using UnityEngine;

public class colorClash : MonoBehaviour
{
    [Header("movement")]
    public sentisHandTracker handTracker;
    public float xSpeed;

    [Header("spawning doors")]
    public GameObject doorsPrefab;
    public float minTimeToSpawn;
    public float maxTimeToSpawn;
    float timer;

    private void Start()
    {
        timer = Random.Range(minTimeToSpawn, maxTimeToSpawn);
    }
    private void Update()
    {
        float dir = (handTracker.palmCenter().x - transform.position.x) * xSpeed * Time.deltaTime;
        transform.position = new Vector2(transform.position.x + dir, transform.position.y);

        timer -= Time.deltaTime;
        if (timer < 0)
        {
            timer = Random.Range(minTimeToSpawn, maxTimeToSpawn);
            Vector2 pos = new Vector2(0, Screen.height * 1.25f);
            PoolManager.spawnObject(doorsPrefab, pos, Quaternion.identity);
        }
    }
}
