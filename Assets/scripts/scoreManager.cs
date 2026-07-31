using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class scoreManager : MonoBehaviour
{
    [Header("score")]
    public TextMeshProUGUI scoreText;
    public int score;
    int oldScore;

    [Header("timer")]
    public TextMeshPro timerText;
    public float timer;

    [Header("UI")]
    public GameObject gameOverPanel;
    public Button again;
    public Button maneMenu;
    public TextMeshProUGUI scoreMeter;

    private void Start()
    {
        gameOverPanel.SetActive(false);
        maneMenu.onClick.AddListener(() => FEEL.gotoScene(0, this));
        again.onClick.AddListener(() => FEEL.gotoScene(SceneManager.GetActiveScene().buildIndex, this));
    }
    private void Update()
    {
        score = Mathf.Max(score, 0);
        scoreText.text = score.ToString();

        timer -= Time.deltaTime;
        timer = Mathf.Max(timer, 0);
        timerText.text = Mathf.Round(timer).ToString();

        if (score != oldScore)
        {
            int diff = score - oldScore;
            if (diff > 0) FEEL.PlaySound("scorePoint");
            if (diff < 0) FEEL.PlaySound("losePoint");

            oldScore = score;
        }

        if (timer == 0)
        {
            gameOverPanel.SetActive(true);
            scoreMeter.text = ("score : " + Mathf.Round(score)).ToString();
        }
    }
}
