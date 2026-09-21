
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{

    public Camera PlayerCamera { get; private set;}

    private CapsuleCollider _collider;

    [SerializeField] private Transform _defaultCamTr;
    private Transform _currentCamTr;

    [SerializeField] private InputActionAsset _playerInput;

    private InputAction _mousePos;
    private InputAction _move;
    private InputAction _aim;
    private InputAction _crouch;

    private int _raycastIgnoreLayers;

    [SerializeField] private float _moveSpeedDefault;
    private float _moveSpeed;
    private float _speedMultiply;

    private bool _isInCamChangeTrigger = false;

    private bool _isCrouched;

    private bool _isAiming;

    private Vector2 _moveDir;

    private Vector3 _3DMoveDir;

    private Rigidbody _rb;

    private void Start()
    {
        PlayerCamera = Camera.main;

        _playerInput.FindActionMap("On foot 2nd Person").Enable();

        _mousePos = _playerInput.FindAction("MousePosition");
        _move = _playerInput.FindAction("Move");
        _aim = _playerInput.FindAction("Aim");
        _crouch = _playerInput.FindAction("Crouch");

        _raycastIgnoreLayers = LayerMask.GetMask("Player", "IgnoreRaycast");

        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<CapsuleCollider>();

        PlayerCamera.transform.SetPositionAndRotation(_defaultCamTr.position, _defaultCamTr.rotation);

        _currentCamTr = _defaultCamTr;

        _moveSpeed = _moveSpeedDefault;

    }

    private void Update()
    {

        //if (!_isInCamChangeTrigger)
        //{
        //    PlayerRotation();
        //}
        //else
        //{
        //    Vector3 velocityDir = _rb.linearVelocity.normalized;

        //    Quaternion targetRotation = Quaternion.LookRotation(velocityDir);
        //    targetRotation.z = 0f;
        //    targetRotation.x = 0f;

        //    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 4f * Time.deltaTime);
        //}

        if (_aim.IsPressed())
        {
            PlayerRotation();

            _isAiming = true;
        }
        else if (_moveDir != Vector2.zero)
        {
            Vector3 velocityDir = _rb.linearVelocity.normalized;

            Quaternion targetRotation = Quaternion.LookRotation(velocityDir);
            targetRotation.z = 0f;
            targetRotation.x = 0f;

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 4f * Time.deltaTime);

            _isAiming = false;
        }
        else
            _isAiming = false;

        if (_crouch.WasPressedThisFrame())
            Crouch();


        Move();

        CameraRotation();

        SpeedChanger();
    }

    private void Crouch()
    {
        Vector3 localScale = transform.localScale;

        if (!_isCrouched)
        {
            Vector3 crouched = new (localScale.x, 0.4f, localScale.z);
            transform.localScale = crouched;

            _collider.radius = 0.4f;

            _rb.AddForce(Vector3.down * 3, ForceMode.Impulse);

            _isCrouched = true;
        }
        else
        {
            Ray ray = new (transform.position, Vector3.up);

            if (!Physics.SphereCast(ray, 0.4f, 1f, ~_raycastIgnoreLayers))
            {
                Vector3 uncrouched = new(localScale.x, 1f, localScale.z);
                transform.localScale = uncrouched;
                
                _collider.radius = 0.5f;
                
                _rb.AddForce(Vector3.up);

                _isCrouched = false;
            }
        }
    }

    private void PlayerRotation()
    {
        Vector3 hitPoint;

        Ray ray = PlayerCamera.ScreenPointToRay(_mousePos.ReadValue<Vector2>());
        if (Physics.Raycast(ray, out RaycastHit hit, 40f, ~_raycastIgnoreLayers))
            hitPoint = hit.point;
        else
            hitPoint = ray.GetPoint(40f);

        Vector3 lookDir = hitPoint - transform.position;

        Debug.DrawRay(transform.position, lookDir, Color.red);

        Quaternion targetRotation = Quaternion.LookRotation(lookDir);
        targetRotation.z = 0f;
        targetRotation.x = 0f;

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 3f * Time.deltaTime);
    }

    private void Move()
    {
        _moveDir = _move.ReadValue<Vector2>();

        if (!_isInCamChangeTrigger)
        {
            Vector3 cameraForwardCorrect = Quaternion.Euler(0f, PlayerCamera.transform.eulerAngles.y, PlayerCamera.transform.eulerAngles.z) * Vector3.forward;

            _3DMoveDir = (cameraForwardCorrect * _moveDir.y + PlayerCamera.transform.right * _moveDir.x);
        }
        else
        {
            if (_moveDir == Vector2.zero || _aim.IsPressed())
            {
                _isInCamChangeTrigger = false;
            }
        }

        _rb.AddForce(Time.deltaTime * _moveSpeed * _3DMoveDir.normalized, ForceMode.VelocityChange);
    }

    private void CameraRotation()
    {
        Vector3 cameralookDir = transform.position - (PlayerCamera.transform.position); 

        Quaternion camTargetRotation = Quaternion.LookRotation(cameralookDir);

        PlayerCamera.transform.rotation = Quaternion.Slerp(PlayerCamera.transform.rotation, camTargetRotation, 4f * Time.deltaTime);
    }

    private void SpeedChanger()
    {
        float crouch = _isCrouched ? 0.4f : 1f;
        float aim = _isAiming ? 0.5f : 1f;

        _moveSpeed = _moveSpeedDefault * aim * crouch;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("CamPosTrigger"))
        {
            other.TryGetComponent<ChangeTrigger>(out ChangeTrigger trigger);

            Transform tr = trigger.AssociatedCameraPosition;

            if (tr != _currentCamTr)
            {
                PlayerCamera.transform.SetPositionAndRotation(tr.position, tr.rotation);

                _currentCamTr = tr;

                _isInCamChangeTrigger = true;
            }

            
        }
    }
}
