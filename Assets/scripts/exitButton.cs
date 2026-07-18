using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class exitButton : MonoBehaviour
{
    [Header("refrences")]
    public Button quitButton;

    private void Awake()
    {
        quitButton.onClick.AddListener(() => SceneManager.LoadScene(0));
    }
}
