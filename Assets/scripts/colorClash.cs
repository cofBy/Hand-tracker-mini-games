using TMPro;
using UnityEngine;

public class colorClash : MonoBehaviour
{
    [Header("movement")]
    public sentisHandTracker handTracker;
    public float xSpeed;

    [Header("spawning doors")]
    public colorClashDoors doorsPrefab;

    public float minTimeToSpawn;
    public float maxTimeToSpawn;
    float timer;

    [Header("score")]
    public scoreManager score;

    private void Start()
    {
        timer = Random.Range(minTimeToSpawn, maxTimeToSpawn);
    }
    private void Update()
    {
        float dir = (handTracker.palmCenter().x - transform.position.x) * xSpeed * Time.deltaTime;
        transform.position = new Vector2(transform.position.x + dir, transform.position.y);

        if (score.timer > 0)
        {
            timer -= Time.deltaTime;
            if (timer < 0)
            {
                timer = Random.Range(minTimeToSpawn, maxTimeToSpawn);
                Vector2 pos = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width * 0.5f, Screen.height * 1.25f));
                colorClashDoors doorsInstance = PoolManager.SpawnObject(doorsPrefab, pos, Quaternion.identity);
                doorsInstance.score = score;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("targetDoor"))
        {
            score.score += 1;
        }
        else
        {
            score.score -= 1;
        }
    }
}
