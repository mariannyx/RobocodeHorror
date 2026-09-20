using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
public class BotController : MonoBehaviour, ITeam
{
    [SerializeField] private ITeam.Teams _team;

    public ITeam.Teams Team { get; set; }

    public bool IsPlayer { get; set; } = false;

    public GameObject accesibleObject {  get; set; }

    public MatchManager matchManager;

    public Squad Squad { get; set; }

    public event Action<int /* in mag */, float /* shoot cooldown */, Vector3, ITeam.Teams> ShootEvent;

    public NavMeshAgent MAgent { get; private set; }

    [SerializeField] private float _speed;

    private float _timeSinceLastTick;
    public float TimeSinceLastSeenTarget {  get; private set; }
    public float TimeSinceLastShot { get; set; } // shoot

    public float ShootCooldown { get; private set; } = 0.3f;

    private float _panic;

    private float _timeSinceLastCover;
    [SerializeField] private float _health = 100f;

    private int _mask;
    private int _plAndBotMask;

    private int _inMag = 30; // shoot

    //private bool _moveToDestination = true;

    //private bool _coverCalculationRun;

    private bool _canSeeTarget;

    public bool LeaderOverrideActive {  get; set; }

    public bool AttackExpected { get; set; } = false;

    [SerializeField] private bool _cover;

    public Vector3 destination;

    public Vector3 hostileTargetPos;

    private Vector3 _averageTargetsPos;


    [SerializeField] private GameObject _debugDestination;

    [SerializeField] private GameObject _bulletProximity;

    private Collider[] _bulletColl = new Collider[10];
    //private Collider[] _rangeCheck = new Collider[30];

    public List<Transform> visibleTargets = new();

    private Collider[] _rangeChecks;
    private Collider[] _proximityChecks;

    #region StateMachine Variables

    public BotStateMachine StateMachine {  get; private set; }

    public Idle Idle { get; private set; }

    public Search Search { get; private set; }

    public MoveToCover MoveToCover { get; private set; }

    public ExpectContact ExpectContact { get; private set; }

    #endregion

    public void FollowLeader()
    {
        if (Squad.Leader != gameObject)
        {
            if (StateMachine.CurrentState != Idle)
                StateMachine.ChangeState(Idle);

            TimeSinceLastSeenTarget = 25f;

            LeaderOverrideActive = true;
        }
    }

    public void MoveToPos(Vector3 pos)
    {
        LeaderOverrideActive = false;

        MoveToDestination(pos);
    }

    public void MoveToRandomPOI()
    {
        StateMachine.ChangeState(Idle);
    }

    private void Awake()
    {
        MAgent = GetComponent<NavMeshAgent>();

        StateMachine = new BotStateMachine();

        //Team = _team;

        Idle = new Idle(this, StateMachine);
        Search = new Search(this, StateMachine);
        MoveToCover = new MoveToCover(this, StateMachine);
        ExpectContact = new ExpectContact(this, StateMachine);

        if (matchManager == null)
        {
            matchManager = GameObject.FindGameObjectWithTag("MatchManager").GetComponent<MatchManager>();

            
        }
        accesibleObject = gameObject;

        Team = _team;

        _rangeChecks = new Collider[20];
        _proximityChecks = new Collider[10];
    }

    private void Start()
    {
        
        
        _mask = LayerMask.GetMask("Bots", "Player", "FirstPerson");
        _plAndBotMask = LayerMask.GetMask("Player", "Bots");

        matchManager.RegisterAgent(gameObject, this);

        StateMachine.Initialize(Idle);

        if (Team == ITeam.Teams.UMF)
        {
            GetComponent<MeshRenderer>().material.color = Color.lightBlue;
        }

        if (Team == ITeam.Teams.BFPM)
        {
            GetComponent<MeshRenderer>().material.color = Color.softRed;
        }

    }

    private void Update()
    {
        _panic -= Time.deltaTime * 1.5f;
        _timeSinceLastTick += Time.deltaTime;
        if (_timeSinceLastTick > 0.1f)
        {
            Tick();
        }

        Debug.DrawLine(transform.position, hostileTargetPos);

        StateMachine.CurrentState.FrameUpdate();

        TimeSinceLastSeenTarget += Time.deltaTime;

        _timeSinceLastCover += Time.deltaTime;

        TimeSinceLastShot += Time.deltaTime;

        if (_health <= 0)
        {
            matchManager.RegisterDeath(Team);

            matchManager.StartCoroutine(matchManager.SpawnOnTime(6f, Team, gameObject));

            _health = 100f;
            _panic = 0f;

            gameObject.SetActive(false);
        }
        
    }

    private void Tick()
    {
        _timeSinceLastTick = 0f;

        

        int bulletProx = Physics.OverlapSphereNonAlloc(transform.position, 5f, _bulletColl);
        for (int i = 0; i < bulletProx; i++)
        {
            if (_bulletColl[i].gameObject.CompareTag("Bullet"))
            {
                Debug.Log("Bullet in proximity " + _bulletColl[i].name);
                SupressionProximity();
            }
        }

        //_hostileTargetPos = _debugDestination.transform.position; // debug

        //if (_panic > 100f)
        //{
        //panicked logic
        //}

        FieldOfViewCheck();

        StateMachine.CurrentState.Tick();

        if (LeaderOverrideActive)
        {
            Vector3 dirToLeader = (Squad.Leader.transform.position - transform.position).normalized;

            NavMesh.SamplePosition(Squad.Leader.transform.position - dirToLeader * 2f, out NavMeshHit hit, 5f, NavMesh.AllAreas);

            MoveToDestination(hit.position);
        }
    }

    public void MoveToDestination(Vector3 destination)
    {
        MAgent.speed = _speed;
        MAgent.destination = destination;


    }

    private void FieldOfViewCheck()
    {
        visibleTargets.Clear();

        int rangeNum = Physics.OverlapSphereNonAlloc(transform.position, 30f, _rangeChecks, _plAndBotMask);
        
        if (_rangeChecks.Length != 0)
        {
            for (int i = 0; i < rangeNum; i++ )
            {
                if (_rangeChecks[i].gameObject != gameObject)
                {
                    Transform target = _rangeChecks[i].transform;
                    Vector3 dirToTarget = (target.position - transform.position).normalized;

                    if (Vector3.Angle(transform.forward, dirToTarget) < 90 / 2)
                    {
                        float distanceToTarget = (target.position - transform.position).magnitude;
                        ITeam.Teams team = ITeam.Teams.None;

                        if (_rangeChecks[i].TryGetComponent(out BotController bot))
                        {
                            team = bot.Team;
                        }
                        else if (_rangeChecks[i].TryGetComponent(out Player_movement player))
                        {
                            team = player.Team;
                        }

                        if (!Physics.Raycast(transform.position, dirToTarget, distanceToTarget, ~_mask) && team != Team)
                        {
                            visibleTargets.Add(target);
                            TimeSinceLastSeenTarget = 0f;
                        }
                    }
                }
                
            }


            int proxNum = Physics.OverlapSphereNonAlloc(transform.position, 7f, _proximityChecks,  _plAndBotMask);


            for (int i = 0; i < proxNum; i++)
            {
                Transform target = _proximityChecks[i].transform;
                
                ITeam.Teams team = ITeam.Teams.None;

                if (_proximityChecks[i].TryGetComponent(out BotController bot))
                {
                    team = bot.Team;
                }
                else if (_proximityChecks[i].TryGetComponent(out Player_movement player))
                {
                    team = player.Team;
                }

                if (!visibleTargets.Contains(target) && target != transform && team != Team)
                {
                    visibleTargets.Add(target);
                }
            }

        }



        if (visibleTargets != null && visibleTargets.Count > 0)
        {
            hostileTargetPos = visibleTargets[0].transform.position;

            TimeSinceLastSeenTarget = 0f;

            if (!AttackExpected)
            {
                _timeSinceLastCover = 0f;
                StateMachine.ChangeState(MoveToCover);
            }

            if ((hostileTargetPos - transform.position).sqrMagnitude < 25f)
            {
                if (StateMachine.CurrentState != MoveToCover)
                {
                    _timeSinceLastCover = 0f;
                    StateMachine.ChangeState(MoveToCover);
                }
            }

            AttackExpected = true;

            _canSeeTarget = true;

        }
        else
        {
            _canSeeTarget = false;
        }
    }

    public void SupressionProximity()
    {
        if (AttackExpected)
        {
            _panic += 2f;
            //logic
        }
        else
        {
            _panic += 15f;
            AttackExpected = true;
        }

        if (!AttackExpected || _timeSinceLastCover > 15f)
        {
            _timeSinceLastCover = 0f;
            
            StateMachine.ChangeState(MoveToCover);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.TryGetComponent(out Bullet bullet))
        {
            _health -= bullet.Damage;
            _panic += 20f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.TryGetComponent(out Bullet bullet))
        {
            if (bullet.team != Team)
            {
                _health -= bullet.Damage;
                _panic += 20f;
            }
        }
    }

    public void SendShootEvent()
    {
        ShootEvent?.Invoke(_inMag, ShootCooldown, hostileTargetPos, Team);
    }
}
