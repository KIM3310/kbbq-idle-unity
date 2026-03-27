using UnityEngine;
using UnityEngine.UI;

public class TutorialView : MonoBehaviour
{
    [SerializeField] private Text messageText;
    [SerializeField] private Button skipButton;

    private GameManager gameManager;

    public void Bind(GameManager manager)
    {
        gameManager = manager;
        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(SkipTutorial);
        }
    }

    public void Show(string message)
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        if (messageText != null)
        {
            messageText.text = message;
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void SkipTutorial()
    {
        gameManager?.SkipTutorial();
    }
}
