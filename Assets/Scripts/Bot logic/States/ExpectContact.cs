using UnityEngine;
using UnityEngine.AI;

public class ExpectContact : BotState
{

    private NavMeshAgent _mAgent;

    private Transform _bTr;

    private bool _seeTarget;

    private int _mask;

    public ExpectContact(BotController bot, BotStateMachine stateMachine) : base(bot, stateMachine)
    {
        _mAgent = bot.MAgent;

        _bTr = bot.transform;

        _mAgent.speed = 2f;

        _mask = LayerMask.GetMask("Bots", "Player", "FirstPerson");
    }

    public override void EnterState()
    {
        base.EnterState();

        _mAgent.updateRotation = false;

        //bot.MoveToDestination(_bTr.position);
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        if (bot.TimeSinceLastSeenTarget < 3f)
        {
            Vector3 direction = (bot.hostileTargetPos - _bTr.position).normalized;
            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                _seeTarget = true;

                _bTr.rotation = Quaternion.Slerp(_bTr.rotation, Quaternion.LookRotation(direction), Time.deltaTime * _mAgent.angularSpeed * 1.5f);
            }
        }
        else
        {
            stateMachine.ChangeState(bot.Search);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    public override void Tick()
    {
        base.Tick();

        Vector3 dirToTarget = (bot.hostileTargetPos - _bTr.position).normalized;
        Vector3 dirToTargetUp = (bot.hostileTargetPos + Vector3.up * 0.5f - _bTr.position).normalized;
        float distanceToTarget = (bot.hostileTargetPos - _bTr.position).magnitude;
        if (bot.TimeSinceLastShot > bot.ShootCooldown && (!Physics.Raycast(_bTr.position, dirToTarget, distanceToTarget, ~_mask) || !Physics.Raycast(_bTr.position, dirToTargetUp, distanceToTarget, ~_mask)))
        {
            bot.TimeSinceLastShot = 0f;
            bot.SendShootEvent();
        }
    }
}
