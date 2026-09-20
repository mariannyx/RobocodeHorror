using UnityEngine;
using UnityEngine.AI;


public class MoveToCover : BotState
{
    private bool _coverCalculationRun;

    private NavMeshAgent _mAgent;

    private int _mask;

    private Vector3 _destination;

    private Transform _bTr;

    private float _timeInState;

    private float _distanceToDest;

    private NavMeshPath _path;


    public MoveToCover(BotController bot, BotStateMachine stateMachine) : base(bot, stateMachine)
    {
        _bTr = bot.transform;

        _mAgent = bot.MAgent;

        _mask = LayerMask.GetMask("Bots", "Player", "FirstPerson");

        _path = new NavMeshPath();
    }

    public override void EnterState()
    {
        base.EnterState();

        _mAgent.updateRotation = true;

        bot.MoveToDestination(BestCoverFinder());

    }

    public override void ExitState()
    {
        base.ExitState();

        _timeInState = 0f;
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        _timeInState += Time.deltaTime;

        _distanceToDest = (_destination - _bTr.position).magnitude;
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    public override void Tick()
    {
        base.Tick();

        

        if (_destination != Vector3.zero)
        {
            const float timeoutMax = 20f;

            if (_distanceToDest < 0.5f || _timeInState > timeoutMax)
            {
                stateMachine.ChangeState(bot.ExpectContact);
            }
        }
        else
        {
            stateMachine.ChangeState(bot.ExpectContact);
        }
    }

    private Vector3 BestCoverFinder()
    {
        float bestScore = -Mathf.Infinity;
        Vector3 bestCoverPos = _bTr.position;

        _coverCalculationRun = true;

        if (_coverCalculationRun)
        {
            for (float dist = 1f; dist <= 25f; dist += 4f)
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = i * 45f * Mathf.Deg2Rad;

                    Vector3 angleDir = new(Mathf.Cos(angle), 0, Mathf.Sin(angle));

                    if (NavMesh.SamplePosition(_bTr.position + angleDir * dist, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                    {
                        if (NavMesh.FindClosestEdge(hit.position, out NavMeshHit hitEdge, NavMesh.AllAreas))
                        {
                            float currentScore = ComputeScore(_bTr.position, hitEdge);

                            if (currentScore > bestScore)
                            {
                                bestScore = currentScore;
                                bestCoverPos = hitEdge.position;
                            }
                        }
                    }
                }
            }
        }

        _destination = bestCoverPos;


        //Debug.Log("Hide Score: " + bestScore + " At: " + bestCoverPos);
        return bestCoverPos;
    }



    private float ComputeScore(Vector3 currentPos, NavMeshHit hit)
    {
        float score = 0f;

        Vector3 dirToHostile = (bot.hostileTargetPos - hit.position).normalized;

        float dot = Vector3.Dot(hit.normal, dirToHostile);

        float distance = (hit.position - currentPos).magnitude;

        if (dot < 0f)
        {
            score += Mathf.Abs(dot) * 10f;
        }
        else
        {
            return -Mathf.Infinity;
        }



        if (Physics.Raycast(hit.position + Vector3.up, dirToHostile, (bot.hostileTargetPos - hit.position).magnitude, ~_mask))
        {
            score += 50f;
        }
        else
        {
            return -Mathf.Infinity;
        }

        float distToHostile = (bot.hostileTargetPos - hit.position).magnitude;


        if (distToHostile > 10f)
        {
            score += 10f;
        }


        if (_mAgent.CalculatePath(hit.position, _path))
        {
            if (_path.status != NavMeshPathStatus.PathComplete)
            {
                 return -Mathf.Infinity;
            }
        }


        score -= distance * 1.5f;


        return score;

    }
}
