using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class sortTheBoxes : MonoBehaviour
{
    [Header("getting info")]
    public Transform target;
    public sentisHandTracker handTracker;

    [Header("moving the target")]
    public float maxSpeed;
    public float maxDistance;

    [Header("boxes")]
    public SpriteRenderer boxPrefab;
    public Color blue;
    public Color red;

    public float TimeBetweenBoxes;
    float timer;

    public float range;

    List<SpriteRenderer> boxInstances = new List<SpriteRenderer>();

    [Header("score")]
    public scoreManager scoreLogic;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > TimeBetweenBoxes)
        {
            spawnBox();
            timer = Random.Range(0.5f, TimeBetweenBoxes - 0.5f);
        }

        if (handTracker == null || handTracker.handLandmarks == null || handTracker.handLandmarks.Length < 1) return;

        Vector3 dir = Vector3.ClampMagnitude((Vector3)handTracker.palmCenter() - target.position, maxSpeed) * maxSpeed;
        target.position = Vector3.ClampMagnitude(target.position + dir * Time.deltaTime, maxDistance);

        for (int i = 0; i < boxInstances.Count; i++)
        {
            float maxX = Camera.main.orthographicSize * ((float)Screen.width / (float)Screen.height) + 1;
            float maxY = Camera.main.orthographicSize + 1;

            if (boxInstances[i].transform.position.y < -maxY)
            {
                PoolManager.ReturnToPool(boxInstances[i].gameObject);
                boxInstances.RemoveAt(i);
            }
            if (boxInstances[i].transform.position.x > maxX)
            {
                bool isBlue = boxInstances[i].color == blue;
                scoreLogic.score = isBlue ? scoreLogic.score + 1 : Mathf.Max(scoreLogic.score - 1, 0);

                PoolManager.ReturnToPool(boxInstances[i].gameObject);
                boxInstances.RemoveAt(i);
            }
            else if (boxInstances[i].transform.position.x < -maxX)
            {
                bool isRed = boxInstances[i].color == red;
                scoreLogic.score = isRed ? scoreLogic.score + 1 : Mathf.Max(scoreLogic.score - 1, 0);

                PoolManager.ReturnToPool(boxInstances[i].gameObject);
                boxInstances.RemoveAt(i);
            }
        }
    }

    void spawnBox()
    {
        Vector2 randomPos = Camera.main.ScreenToWorldPoint(new Vector2(Random.Range(range, Screen.width - range), Screen.height + range));

        SpriteRenderer boxInstance = PoolManager.SpawnObject(boxPrefab, randomPos, Quaternion.identity);
        boxInstances.Add(boxInstance);

        int rnd = Random.Range(0, 2);
        boxInstance.color = rnd == 0 ? blue : red;
    }
}
