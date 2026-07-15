using TMPro;
using UnityEngine;

public class scoreManager : MonoBehaviour
{
    [Header("score")]
    public TextMeshProUGUI scoreText;
    public int score;

    [Header("timer")]
    public TextMeshPro timerText;
    public float timer;
    private void Update()
    {
        score = Mathf.Max(score, 0);
        scoreText.text = score.ToString();

        timer -= Time.deltaTime;
        timer = Mathf.Max(timer, 0);
        timerText.text = Mathf.Round(timer).ToString();
    }
}
