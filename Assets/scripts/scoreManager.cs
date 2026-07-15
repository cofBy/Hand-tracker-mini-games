using TMPro;
using UnityEngine;

public class scoreManager : MonoBehaviour
{
    [Header("score")]
    public TextMeshProUGUI scoreText;
    public int score;

    private void Update()
    {
        scoreText.text = score.ToString();
    }
}
