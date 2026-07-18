using UnityEngine;
using UnityEngine.UI;

public class exitButton : MonoBehaviour
{
    [Header("refrences")]
    public Button quitButton;

    private void Awake()
    {
        quitButton.onClick.AddListener(() => FEEL.gotoScene(0, this));
    }
}
