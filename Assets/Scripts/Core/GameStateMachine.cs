using System;

public enum GameState
{
    Boot,
    Tutorial,
    MainLoop,
    Pause,
    OfflineCalc
}

public class GameStateMachine
{
    public GameState State { get; private set; } = GameState.Boot;

    public event Action<GameState, GameState> OnStateChanged;

    public void TransitionTo(GameState next)
    {
        if (next == State)
        {
            return;
        }

        var previous = State;
        State = next;
        OnStateChanged?.Invoke(previous, next);
    }
}
