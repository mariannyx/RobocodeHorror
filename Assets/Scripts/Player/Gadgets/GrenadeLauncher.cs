using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Equipables))]
public class GrenadeLauncher : MonoBehaviour, IGadget, IAmmoCount
{
    public event Action<int, int, string> OnAmmoChanged;
    
    private Equipables _equipables;

    private float _timeSinceLastShot;

    private int _vaultLayer;

    private bool _isActive;

    private bool _canShoot;

    private string _name;

    public int GadgetNum { get; set;} // дуже тупо, але працює
    [Range(1, 2)]
    private int _gadgetNum = 1;
    
    [SerializeField] private GadgetScriptableObject _gadget;

    [SerializeField] private Camera _camera;
    [SerializeField] private CinemachineCamera _cameraCinema;

    private GameObject _bullet;

    private GameObject _bulletHoleContainer;

    private InputAction _shoot;

    [SerializeField] private GameObject _bulletSpawn;

    public void RefreshUI()
    {
        OnAmmoChanged.Invoke(00, 00, _name);
    }


    private void Awake()
    {
        _equipables = GetComponent<Equipables>();
    }

    void Start()
    {
        _vaultLayer = LayerMask.GetMask("VaultLayer");

        _bulletHoleContainer = GameObject.Find("BulletHoles Container");

        _shoot = _equipables.playerInputs.FindAction("Shoot");

        _bullet = _gadget.projectile;

        _name = _gadget.GadgetName;

        GadgetNum = _gadgetNum;

    }

    private void OnEnable()
    {
        if (_equipables != null)
            _equipables.OnWeaponChange += HandleWeaponChange;
    }

    private void OnDisable()
    {
        if (_equipables != null)
            _equipables.OnWeaponChange -= HandleWeaponChange;
    }

    private void Update()
    {
        _canShoot = _equipables.canShoot;

        if (_isActive)
            ActiveUpdate();
        else
            _timeSinceLastShot += Time.deltaTime;


    }

    private void HandleWeaponChange(Equipables.Equipped equipped, IAmmoCount ammoActive)
    {
        if (GadgetNum == 1)
        {
            _isActive = (equipped == Equipables.Equipped.gadget1);

            if (_isActive) OnAmmoChanged?.Invoke(00, 00, _name);

        }
        


    }

    private void ActiveUpdate()
    {
        if (_shoot.IsPressed() && _timeSinceLastShot >= _gadget.cooldown && _canShoot)
        {
            _timeSinceLastShot = 0f;
            StartCoroutine(GadgetShoot(_gadget));
            OnAmmoChanged?.Invoke(00, 00, _name);
        }


        _timeSinceLastShot += Time.deltaTime;
    }

    public void Initialize(Camera camera, CinemachineCamera cameraCinema)
    {
        _camera = camera;
        _cameraCinema = cameraCinema;
    }

    IEnumerator GadgetShoot(GadgetScriptableObject _GSO)
    {
        Transform cameraTr = _cameraCinema.transform;

        float RecoilX = UnityEngine.Random.Range(0f, 1);
        float RecoilY = UnityEngine.Random.Range(0.7f, 1);

        Player_Camera playerCam = _cameraCinema.GetComponent<Player_Camera>();

        
        float camRecoilY = 5f * RecoilY;
        float camRecoilNX = playerCam.XRotation - camRecoilY;
        

        float t = 0f;

        while (t < 0.05f)
        {
            playerCam.XRotation = Mathf.Lerp(playerCam.XRotation, camRecoilNX, 0.05f);

            t += Time.deltaTime;

            yield return null;
        }

        Vector3 targetPoint;

        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out var hit, 76, ~_vaultLayer))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(75);
        }

        Vector3 SpawnPos = _bulletSpawn.transform.position - Vector3.down * 0.01f;

        Vector3 dir = targetPoint - SpawnPos;

        GameObject tempBullet = Instantiate(_bullet, SpawnPos, Quaternion.identity);

        tempBullet.transform.SetParent(null, true);

        Vector3 dirFN = dir.normalized;

        tempBullet.transform.forward = dirFN;

        tempBullet.GetComponent<Rigidbody>().AddForce(dirFN * 7, ForceMode.Impulse);

        tempBullet.GetComponent<Bullet>().SendInfo(7, _GSO.damage, _bulletHoleContainer, _GSO.gadgetPrefab, ITeam.Teams.None,  transform.position, true);

    }
}
