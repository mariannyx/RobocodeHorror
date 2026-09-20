using UnityEngine;

public class BotState
{
    protected BotController bot;
    protected BotStateMachine stateMachine;

    public BotState(BotController bot, BotStateMachine stateMachine)
    {
        this.bot = bot;
        this.stateMachine = stateMachine;
    }

    public virtual void EnterState() { }
    public virtual void ExitState() { }
    public virtual void FrameUpdate() { }
    public virtual void PhysicsUpdate() { }
    public virtual void Tick() { }

}
