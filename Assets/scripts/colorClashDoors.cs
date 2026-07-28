using TMPro;
using UnityEngine;

public class colorClashDoors : MonoBehaviour
{
    [Header("moving")]
    public float speed;

    [Header("coloring")]
    public SpriteRenderer[] doors;

    public doorColor[] doorColors;
    public float alpha;

    public TextMeshPro colorName;

    [Header("despawning")]
    public scoreManager score;

    [System.Serializable] public struct doorColor
    {
        public string name;
        public Color color;
    }

    private void OnEnable()
    {
        int index = Random.Range(0, doorColors.Length);
        for (int i = 0; i < doors.Length; i++)
        {
            doors[i].color = doorColors[(index + i) % doorColors.Length].color * new Color(1, 1, 1, alpha);
            doors[i].gameObject.tag = "Untagged";
        }

        doorColor targetColor = doorColors[(index + Random.Range(4, 6)) % doorColors.Length];

        int targetIndex = Random.Range(0, doors.Length);
        doors[targetIndex].color = targetColor.color * new Color(1, 1, 1, alpha);
        doors[targetIndex].gameObject.tag = "targetDoor";

        colorName.text = targetColor.name;
        colorName.color = doorColors[index].color;
    }
    private void Update()
    {
        transform.position += Vector3.down * speed * Time.deltaTime;
        if (transform.position.y < -Camera.main.orthographicSize - 5f || score.timer < 0)
        {
            PoolManager.ReturnToPool(gameObject);
        }
    }
}
