using TMPro;
using UnityEngine;

public class scoreManager : MonoBehaviour
{
    [Header("score")]
    public TextMeshProUGUI scoreText;
    public int score;

    private void Update()
    {
        score = Mathf.Max(score, 0);
        scoreText.text = score.ToString();
    }
}
