using UnityEngine;
using UnityEngine.AI;


public class Idle : BotState
{
    private Vector3 _destination;

    private NavMeshAgent _mAgent;

    private float _timePassed;
    public Idle(BotController bot, BotStateMachine stateMachine) : base(bot, stateMachine)
    {
        _mAgent = bot.MAgent;
    }

    public override void EnterState()
    {
        base.EnterState();

        MatchManager matchManager = bot.matchManager;

        if(matchManager.interestPoints != null && matchManager.interestPoints.Count > 0 )
        {
            int count = UnityEngine.Random.Range(0, matchManager.interestPoints.Count);

            float x = UnityEngine.Random.Range(matchManager.interestPoints[count].x - 5f, matchManager.interestPoints[count].x + 5f);
            float z = UnityEngine.Random.Range(matchManager.interestPoints[count].z - 5f, matchManager.interestPoints[count].z + 5f);

            NavMesh.SamplePosition(new Vector3(x, matchManager.interestPoints[count].y, z), out NavMeshHit hit, 5f, NavMesh.AllAreas);

            _destination = hit.position;

            if (bot.Squad.Leader == bot.gameObject)
            {
                bot.Squad.HandleFollowLeader();


            }

            bot.MoveToDestination(_destination);
        }

        _timePassed = 0f;

        _mAgent.updateRotation = true;
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        _timePassed += Time.deltaTime;
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    public override void Tick()
    {
        base.Tick();

        if (bot.AttackExpected)
        {
            stateMachine.ChangeState(bot.ExpectContact);
        }

        if (_timePassed >= 10f && !bot.LeaderOverrideActive)
        {
            stateMachine.ChangeState(bot.Idle);
        }
    }
}
