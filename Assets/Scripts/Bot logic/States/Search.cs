using UnityEngine;
using UnityEngine.AI;

public class Search : BotState
{
    private NavMeshAgent _mAgent;

    public Search(BotController bot, BotStateMachine stateMachine) : base(bot, stateMachine)
    {
        _mAgent = bot.MAgent;
    }

    public override void EnterState()
    {
        base.EnterState();

        _mAgent.updateRotation = true;

        float x = UnityEngine.Random.Range(bot.hostileTargetPos.x - 5f, bot.hostileTargetPos.x + 5f);
        float z = UnityEngine.Random.Range(bot.hostileTargetPos.z - 5f, bot.hostileTargetPos.z + 5f);

        NavMesh.SamplePosition(new Vector3(x, bot.hostileTargetPos.y, z), out NavMeshHit hit, 50f, NavMesh.AllAreas);

        bot.MoveToDestination(hit.position);

    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    public override void Tick()
    {
        base.Tick();

        if (bot.TimeSinceLastSeenTarget > 15f)
        {
            stateMachine.ChangeState(bot.Idle);

            bot.AttackExpected = false;
        }
        else if (bot.TimeSinceLastSeenTarget <= 0.3f)
        {
            stateMachine.ChangeState(bot.ExpectContact);
        }
    }
}
