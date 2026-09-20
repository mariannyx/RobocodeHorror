using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.Cinemachine;
using System;

[RequireComponent(typeof(Rigidbody))]
public class Player_movement : MonoBehaviour, ITeam
{
    [SerializeField] private ITeam.Teams _team;

    public ITeam.Teams Team { get; set; }

    public Squad Squad { get; set; }

    public bool IsPlayer { get; set; } = true;

    public GameObject accesibleObject { get; set; }

    [SerializeField] private MatchManager _matchManager;

    [Header("Input Action Settings")]
    [SerializeField] private InputActionAsset PlayerInputs;


    #region InputAction
    private InputAction _move;
    private InputAction _look;
    private InputAction _jump;
    private InputAction _sprint;
    private InputAction _shoot;
    private InputAction _crouch;
    private InputAction _primary, _gadgetButton1, _secondary/*, _gadgetButton2, _throwableButton*/;
    private InputAction _commandButton;
    #endregion

    private Transform _tr;

    private Transform _weaponTr;

    private Rigidbody _rb;

    private Vector2 _moveDir;
    public Vector2 LookDir { get; private set; }
    public Vector3 NextMoveRotState { get; private set; }

    private Vector3 _startPos;
    private Quaternion _startRotation;

    private int _vaultLayer;

    public event Action<MovementState> OnMovementStateChange;


    [Header("Movement Settings")]
    [SerializeField] private float _walkSpeed;
    [SerializeField] private float _sprintSpeed;
    [SerializeField] private float _jumpPower;

    [Header("Heights")]
    [SerializeField] private float _height, _vaultHalfHeight;


    [Header("Body positions")]
    [SerializeField] private GameObject _vaultLow;
    [SerializeField] private GameObject _head;

    [Header("Cameras")]
    [SerializeField] private Camera _camera;
    [SerializeField] private CinemachineCamera _cameraCinema;



    [Header("EquipablesSO")]
    [SerializeField] private WeaponStatsPreset _weaponStatsPreset;
    [SerializeField] private GadgetScriptableObject _gadget1;
    [SerializeField] private GadgetScriptableObject _gadget2;

    private float _timeSinceLastShot, _moveSpeed, _yScaleStart;

    public float Health { get; private set; } = 100f;


    private bool _onGround, _rotEfMove, _isCrouched, _canMove = true;

    public bool InCommandMenu { get; private set; }

    private GameObject _bulletSpawn;

    private ViewModelAnimController _viewModelAnim;

    [Header("States")]
    public MovementState MoveState;
    public enum MovementState
    {
        walking,
        sprinting,
        air,
        crouched
    }

    public EquipState equipped;
    public enum EquipState
    {
        primary,
        secondary,
        gadget1,
        gadget2,
        throwable
    }

    void OnEnable()
    {
        PlayerInputs.FindActionMap("OnFoot").Enable();



        Health = 100f;
    }

    void OnDisable()
    {
        _viewModelAnim.SetBoolAll("IsRunning", false);
        _viewModelAnim.SetBoolAll("IsMoving", false);

    }

    void Awake()
    {
        if (_matchManager == null)
        {
            _matchManager = GameObject.FindGameObjectWithTag("MatchManager").GetComponent<MatchManager>();

            
        }

        

        accesibleObject = gameObject;

        Team = _team;
    }

    void Start()
    {
        _move = PlayerInputs.FindAction("Move");
        _look = PlayerInputs.FindAction("Rotation");
        _jump = PlayerInputs.FindAction("Jump/Vault");
        _sprint = PlayerInputs.FindAction("Sprint");
        _shoot = PlayerInputs.FindAction("Shoot");
        _crouch = PlayerInputs.FindAction("Crouch");
        _gadgetButton1 = PlayerInputs.FindAction("GrenadeLauncher");
        _primary = PlayerInputs.FindAction("primary");
        _commandButton = PlayerInputs.FindAction("CommandMenu");
        _secondary = PlayerInputs.FindAction("Secondary");


         _matchManager.RegisterAgent(gameObject, this);

        _rb = GetComponent<Rigidbody>();

        _vaultLayer = LayerMask.GetMask("VaultLayer");
        //_vaultLayer = ~_vaultLayer;

        _yScaleStart = transform.localScale.y;

        #pragma warning disable UNT0039, UNT0026
        Equipables equipables = GetComponent<Equipables>();
        
        if (equipables != null)
        {
            equipables.EquipInfo(this, _cameraCinema, _camera);
        }
        
        Shooting shooting = GetComponent<Shooting>();

        

        IGadget[] gadgets = GetComponents<IGadget>();

        foreach (var gadget in gadgets)
        {
            gadget.Initialize(_camera, _cameraCinema);
        }
#pragma warning restore UNT0039, UNT0026


        _viewModelAnim = _camera.GetComponentInChildren<ViewModelAnimController>();

        if (shooting != null)
        {
            shooting.Initialize(_camera, _cameraCinema, _viewModelAnim);
        }

        //StartCoroutine(RotEffect());
        



        _move.performed += ctx => _moveDir = ctx.ReadValue<Vector2>();
        _move.canceled += ctx => _moveDir = Vector2.zero;

        _look.performed += ctx => LookDir = ctx.ReadValue<Vector2>();
        _look.canceled += ctx => LookDir = Vector2.zero;

        if (Squad != null && Squad.Leader == gameObject)
            Squad.HandleFollowLeader();
    }

    void Update()
    {
        _onGround = Physics.Raycast(transform.position, Vector3.down, _height * 0.5f + 0.7f);

        
        if (_onGround)
        {
            _rb.linearDamping = 5f;
            _rb.angularDamping = 5f;
        }
        else
        {
            _rb.linearDamping = 0f;
            _rb.angularDamping = 0f;
        }


        //_moveDir = _move.ReadValue<Vector2>();
        //LookDir = _look.ReadValue<Vector2>();


        if(_moveDir.x != 0f || _moveDir.y != 0f)
        {
            _rotEfMove = true;
        }
        else
        {
            _rotEfMove = false;
        }

        if(!_isCrouched && _crouch.WasPressedThisFrame())
        {
            transform.localScale = new Vector3(transform.localScale.x, _yScaleStart * 0.7f, transform.localScale.z);
            _rb.AddForce(Vector3.down * 5f);
            _isCrouched = true;
        }
        else if(_isCrouched && _crouch.WasPressedThisFrame())
        {
            transform.localScale = new Vector3(transform.localScale.x, _yScaleStart, transform.localScale.z);
            _isCrouched = false;
            _rb.AddForce(Vector3.up * 0.2f);
        }

        if (MoveState != MovementState.crouched)
        {
            if (_jump.WasPressedThisFrame() && _onGround && _canMove)
            {
                Jump();
            }

            if (_jump.WasPressedThisFrame())
            {
                VaultCheck();
            }
        }

        MoveStateChange();

        _timeSinceLastShot += Time.deltaTime;

        if (!InCommandMenu)
        {
            

            if (_gadgetButton1.WasPressedThisFrame())
            {
                equipped = EquipState.gadget1;
            }
            if (_primary.WasPressedThisFrame())
            {
                equipped = EquipState.primary;
            }
        }

        if (_commandButton.IsPressed())
        {
            if (Squad == null)
            {
                Debug.LogError("Squad is null in Player");
            }

            if (Squad.Leader == null)
            {
                Debug.LogError("Squad Leader is null");
            }

            if (Squad.Leader == gameObject)
            {
                InCommandMenu = true;
            }
        }
        else
        {
            InCommandMenu = false;
        }

        if (InCommandMenu)
        {
            if (_primary.WasPressedThisFrame())
            {
                Squad.HandleFollowLeader();
            }

            if (_secondary.WasPressedThisFrame())
            {
                Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

                if (Physics.Raycast(ray,  out RaycastHit hit, 60f))
                {
                    Squad.HandleMoveToPos(hit.point);
                }
            }

            if (_gadgetButton1.WasPressedThisFrame())
            {
                Squad.HandleScatter();
            }
        }

        if (Health <= 0f)
        {
            _matchManager.RegisterDeath(Team);

            

            _matchManager.StartCoroutine(_matchManager.SpawnOnTime(6f, Team, gameObject));

            gameObject.SetActive(false);
        }

        RotEffect();
    }

   
        

    private void FixedUpdate()
    {
        if (_canMove)
        {
            Walk();
        }
        SpeedLimit();
    }

    

    private void Jump()
    {
        _rb.AddForce(transform.up * _jumpPower, ForceMode.Impulse);
    }

    private void Walk()
    {
        Vector3 _moveDirection = (transform.forward * _moveDir.y + transform.right * _moveDir.x); 

        _rb.AddForce(_moveDirection.normalized * _moveSpeed, ForceMode.VelocityChange); 


        //_rigidBody.MovePosition(_rigidBody.position + _moveDirection);
    }

    private void MoveStateChange()
    {
        MovementState preState = MoveState;

        if (_isCrouched && _sprint.IsPressed())
        {
            MoveState = MovementState.sprinting;

           

            _moveSpeed = _sprintSpeed * 0.7f;

        }
        else if (_isCrouched)
        {
            MoveState = MovementState.crouched;
            _moveSpeed = _walkSpeed * 0.5f;

        }

        
        if(!_isCrouched)
        {
            if (_onGround && _sprint.IsPressed())
            {
                MoveState = MovementState.sprinting;
                _moveSpeed = _sprintSpeed;

                
            }
            else if (_onGround)
            {
                MoveState = MovementState.walking;
                _moveSpeed = _walkSpeed;
            }
            else
            {
                MoveState = MovementState.air;
                _moveSpeed = 2f;
            }
        }
        else if(!_onGround)
        {
            MoveState = MovementState.air;

            _viewModelAnim.SetBoolAll("IsRunning", false);
            _viewModelAnim.SetBoolAll("IsMoving", false);
        }

        if(_onGround)
        {
            _viewModelAnim.SetBoolAll("IsMoving", _moveDir != Vector2.zero);
            _viewModelAnim.SetBoolAll("IsRunning", MoveState == MovementState.sprinting);
        }

        if (preState != MoveState)
        {
            OnMovementStateChange?.Invoke(MoveState);
        }
        
            
    }

    private void SpeedLimit()
    {
        Vector3 _rgVel = new (_rb.linearVelocity.x, 0, _rb.linearVelocity.z);

        if (_rgVel.magnitude > _moveSpeed)
        {
            Vector3 _limitedVel = _rgVel.normalized * _moveSpeed;
            _rb.linearVelocity = new Vector3(_limitedVel.x, _rb.linearVelocity.y, _limitedVel.z);
        }
    }

    private void VaultCheck() // hippity hoppity your code is now my property!
    {

        Transform headTr = _head.transform;
        Transform vLTr = _vaultLow.transform;
        if (Physics.Raycast(headTr.position, headTr.forward, out var firstHit, 1f, _vaultLayer))
        {
            if (Physics.Raycast(firstHit.point + (headTr.forward * 0.3f) + (0.6f * 2 * Vector3.up ), Vector3.down, out var secondHit))
            {
                _viewModelAnim.SetTriggerAll("VaultHigh");
                StartCoroutine(Vault(secondHit.point, 1f));
            }
        }
        else if (Physics.Raycast(vLTr.position, vLTr.forward, out var firstHitL, 1f, _vaultLayer))
        {
            if (Physics.Raycast(firstHitL.point + (vLTr.forward * 0.3f) + (0.6f * 2 * Vector3.up), Vector3.down, out var secondHitL))
            {
                _viewModelAnim.SetTriggerAll("Vault");
                StartCoroutine(Vault(secondHitL.point, 0.6f));
            }
                
        }
   
    }

    IEnumerator Vault(Vector3 point, float duration)
    {
        float time = 0;
        Vector3 _startPos = transform.position;
        _canMove = false;

        while (time < duration)
        {
            transform.position = Vector3.Lerp(_startPos, new Vector3(point.x, point.y + _vaultHalfHeight, point.z), time/duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = new Vector3(point.x, point.y + _vaultHalfHeight, point.z);
        _canMove = true;
    }

    void RotEffect()
    {
        float time = 0f;
        float duration = 0.2f;

        
        
            time += Time.deltaTime;

            if (_rotEfMove)
            {
                if (time >= duration)
                {
                    time = duration;
                }


                float t = time / duration;

                if (MoveState == MovementState.sprinting)
                {
                    NextMoveRotState = Vector3.Lerp(NextMoveRotState, new Vector3(_moveDir.x * -1.2f, 0f, _moveDir.y * 1.2f), t);
                }
                else
                {
                    NextMoveRotState = Vector3.Lerp(NextMoveRotState, new Vector3(_moveDir.x * -0.7f, 0f, _moveDir.y * 0.7f), t);
                }
            }
            else
            {
                time = 0f;
                NextMoveRotState = Vector3.Lerp(NextMoveRotState, Vector3.zero, 0.1f);
            }
        
    }

    private GameObject FindChildByName(String ChildName, GameObject Parent)
    {
        GameObject[] allChildren;
        allChildren = new GameObject[Parent.transform.childCount];

        for (int i = 0; i < allChildren.Length; i++)
        {
            if (Parent.transform.GetChild(i).name == ChildName)
            {
                return Parent.transform.GetChild(i).gameObject;
            }

        }

        return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.TryGetComponent(out Bullet bullet))
        {
            if (bullet.team != Team)
            {
                Health -= bullet.Damage;
           
                
            }
        }
    }






}



