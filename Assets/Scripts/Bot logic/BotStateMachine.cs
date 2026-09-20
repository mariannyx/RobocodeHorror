using UnityEngine;

public class BotStateMachine
{
    public BotState CurrentState { get; set; }

    public void Initialize(BotState startState)
    {
        CurrentState = startState;
        CurrentState.EnterState();
    }

    public void ChangeState(BotState newState)
    {
        CurrentState.ExitState();
        CurrentState = newState;
        CurrentState.EnterState();
    }

}
