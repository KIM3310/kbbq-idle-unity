using UnityEngine;
using UnityEngine.UI;

public class QueueControlView : MonoBehaviour
{
    [SerializeField] private Button serveButton;
    [SerializeField] private Button rushButton;

    private GameManager gameManager;

    public void Bind(GameManager manager)
    {
        gameManager = manager;

        if (serveButton != null)
        {
            serveButton.onClick.RemoveAllListeners();
            serveButton.onClick.AddListener(HandleServe);
        }

        if (rushButton != null)
        {
            rushButton.onClick.RemoveAllListeners();
            rushButton.onClick.AddListener(HandleRush);
        }
    }

    private void HandleServe()
    {
        gameManager?.ServeNextCustomer();
    }

    private void HandleRush()
    {
        gameManager?.TriggerRushService();
    }
}
