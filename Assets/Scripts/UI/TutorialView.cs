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
        ApplyPolish();
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

    private void ApplyPolish()
    {
        if (messageText != null)
        {
            messageText.supportRichText = true;
            messageText.fontStyle = FontStyle.Bold;
            messageText.lineSpacing = 1.08f;
            messageText.alignment = TextAnchor.MiddleCenter;
        }

        if (skipButton != null)
        {
            var buttonImage = skipButton.targetGraphic as Image;
            if (buttonImage != null)
            {
                buttonImage.color = new Color(0.40f, 0.18f, 0.12f, 0.96f);
            }

            var colors = skipButton.colors;
            colors.normalColor = new Color(0.40f, 0.18f, 0.12f, 0.96f);
            colors.highlightedColor = new Color(0.54f, 0.24f, 0.16f, 1f);
            colors.pressedColor = new Color(0.31f, 0.13f, 0.09f, 1f);
            skipButton.colors = colors;

            var skipText = skipButton.GetComponentInChildren<Text>(true);
            if (skipText != null)
            {
                skipText.fontStyle = FontStyle.Bold;
                skipText.color = new Color(0.98f, 0.94f, 0.84f, 1f);
            }
        }
    }
}
