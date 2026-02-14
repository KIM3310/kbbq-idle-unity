using UnityEngine;
using UnityEngine.UI;

public class PrestigeView : MonoBehaviour
{
    [SerializeField] private Text prestigeInfoText;
    [SerializeField] private Button prestigeButton;

    private GameManager gameManager;

    public void Bind(GameManager manager)
    {
        gameManager = manager;
    }

    public void Refresh(int level, int points)
    {
        if (prestigeInfoText != null)
        {
            prestigeInfoText.text = "Prestige " + level + " (+" + points + ")";
        }

        if (prestigeButton != null)
        {
            prestigeButton.interactable = gameManager != null && gameManager.CanPrestige();
        }
    }

    public void OnPrestigeClicked()
    {
        gameManager?.TryPrestige();
    }
}
