public enum TutorialStep
{
    None,
    BuyMeat,
    LoadGrill,
    FlipMeat,
    CollectMeat,
    ServeCustomer,
    BuyUpgrade,
    TapBoost,
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
        step = completed ? TutorialStep.Complete : TutorialStep.BuyMeat;
    }

    public bool IsActive => active && step != TutorialStep.Complete;
    public TutorialStep CurrentStep => step;
    public string CurrentPrompt => BuildPrompt(step);

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

    public void OnBuyMeat()
    {
        if (!active || step != TutorialStep.BuyMeat)
        {
            return;
        }
        step = TutorialStep.LoadGrill;
        ShowStep();
    }

    public void OnLoadMeat()
    {
        if (!active || step != TutorialStep.LoadGrill)
        {
            return;
        }
        step = TutorialStep.FlipMeat;
        ShowStep();
    }

    public void OnFlip()
    {
        if (!active || step != TutorialStep.FlipMeat)
        {
            return;
        }
        step = TutorialStep.CollectMeat;
        ShowStep();
    }

    public void OnCollect()
    {
        if (!active || step != TutorialStep.CollectMeat)
        {
            return;
        }
        step = TutorialStep.ServeCustomer;
        ShowStep();
    }

    public void OnBoost()
    {
        if (!active || step != TutorialStep.TapBoost)
        {
            return;
        }
        Complete();
    }

    public void OnUpgrade()
    {
        if (!active || step != TutorialStep.BuyUpgrade)
        {
            return;
        }
        step = TutorialStep.TapBoost;
        ShowStep();
    }

    public void OnServe()
    {
        if (!active || step != TutorialStep.ServeCustomer)
        {
            return;
        }
        step = TutorialStep.BuyUpgrade;
        ShowStep();
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
        uiController.ShowTutorial(BuildPrompt(step));
    }

    private string BuildPrompt(TutorialStep tutorialStep)
    {
        switch (tutorialStep)
        {
            case TutorialStep.BuyMeat:
                return "1/7 오늘의 첫 손님을 맞이할 준비입니다.\nBUY +1로 고기를 먼저 채워두세요.";
            case TutorialStep.LoadGrill:
                return "2/7 빈 그릴 슬롯에 고기를 올리세요.\n기본 루프는 올리기 -> 뒤집기 -> 수거 -> 서빙입니다.";
            case TutorialStep.FlipMeat:
                return "3/7 타이밍이 오면 뒤집으세요.\n너무 이르면 육즙이 부족하고, 너무 늦으면 탑니다.";
            case TutorialStep.CollectMeat:
                return "4/7 노릇해진 고기를 수거해 cooked 재고를 만드세요.";
            case TutorialStep.ServeCustomer:
                return "5/7 손님에게 바로 서빙하세요.\n빠르게 서빙할수록 팁과 콤보가 커집니다.";
            case TutorialStep.BuyUpgrade:
                return "6/7 업그레이드로 회전율을 올리세요.\n초반엔 service/staff 계열이 체감이 큽니다.";
            case TutorialStep.TapBoost:
                return "7/7 마지막으로 지글 부스트를 써보세요.\n피크타임을 버티는 핵심 버튼입니다.";
            default:
                return string.Empty;
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
