using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class mainMenuManager : MonoBehaviour
{
    [Header("mini games")]
    public Transform gamesParent;
    public Button playButtonPrefab;

    public List<miniGame> miniGames;
    [System.Serializable] public struct miniGame
    {
        public string name;
        public string description;
        public Sprite icon;
        [HideInInspector] public Button playButton;
    }

    [Header("starting mini games")]
    public GameObject gamePanel;
    public TextMeshProUGUI gameName, gameDesc;
    public Button playButton;
    public Button exitButton;

    private void Awake()
    {
        gamePanel.SetActive(false);
        for (int i = 0; i < miniGames.Count; i++)
        {
            Button buttonInstance = Instantiate(playButtonPrefab, gamesParent);
            miniGame m = miniGames[i];
            miniGames[i] = new miniGame {name = m.name, description = m.description, icon = m.icon, playButton = buttonInstance};

            int gameIndex = i;

            miniGames[i].playButton.image.sprite = m.icon;
            miniGames[i].playButton.onClick.AddListener(() => setGamePanel(gameIndex));
        }

        exitButton.onClick.AddListener(() => gamePanel.SetActive(false));
    }

    void setGamePanel(int index)
    {
        gamePanel.SetActive(true);

        gameName.text = miniGames[index].name;
        gameDesc.text = miniGames[index].description;
        playButton.onClick.AddListener(() => playGame(index));
    }
    void playGame(int index)
    {
        SceneManager.LoadScene(index + 1);
    }
}
