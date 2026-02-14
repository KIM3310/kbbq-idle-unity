public enum TutorialStep
{
    None,
    TapBoost,
    BuyUpgrade,
    ServeCustomer,
    Complete
}

public class TutorialSystem
{
    private readonly GameManager gameManager;
    private readonly UIController uiController;
    private TutorialStep step;
    private bool active;

    public TutorialSystem(GameManager gameManager, UIController uiController, bool completed)
    {
        this.gameManager = gameManager;
        this.uiController = uiController;
        step = completed ? TutorialStep.Complete : TutorialStep.TapBoost;
    }

    public void Start()
    {
        if (step == TutorialStep.Complete)
        {
            uiController?.HideTutorial();
            active = false;
            return;
        }

        active = true;
        ShowStep();
    }

    public void OnBoost()
    {
        if (!active || step != TutorialStep.TapBoost)
        {
            return;
        }
        step = TutorialStep.BuyUpgrade;
        ShowStep();
    }

    public void OnUpgrade()
    {
        if (!active || step != TutorialStep.BuyUpgrade)
        {
            return;
        }
        step = TutorialStep.ServeCustomer;
        ShowStep();
    }

    public void OnServe()
    {
        if (!active || step != TutorialStep.ServeCustomer)
        {
            return;
        }
        Complete();
    }

    public void Skip()
    {
        Complete();
    }

    private void ShowStep()
    {
        if (uiController == null)
        {
            return;
        }

        switch (step)
        {
            case TutorialStep.TapBoost:
                uiController.ShowTutorial("지글 부스트 버튼을 눌러보세요!");
                break;
            case TutorialStep.BuyUpgrade:
                uiController.ShowTutorial("업그레이드를 하나 구매하세요!");
                break;
            case TutorialStep.ServeCustomer:
                uiController.ShowTutorial("서빙 버튼을 눌러 손님을 서빙하고 콤보를 쌓아보세요!");
                break;
        }
    }

    private void Complete()
    {
        active = false;
        step = TutorialStep.Complete;
        uiController?.HideTutorial();
        gameManager?.CompleteTutorial();
    }
}
