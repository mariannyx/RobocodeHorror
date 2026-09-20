using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

[RequireComponent(typeof(Equipables))]
public class Shooting : MonoBehaviour, IAmmoCount
{
    private Equipables _equipables;
    private ViewModelAnimController _FPController;

    private float _timeSinceLastShot;

    private float _recoilReduction;
    private float _recoilBias;
    private float _sprayAdd;

    private float _ogFOV;

    private int _vaultLayer;

    public int inMag;
    public int reserveAmmo;

    private bool _atStartPos;
    private bool _comingBack;
    
    private bool _isActive;

    private bool _canShoot;

    private bool _aboutToAimIn;
    private bool _aimedIn;

    private bool _canReload = true;

    private string _name;
   
    public WeaponStatsPreset weaponStatsPreset;
    [SerializeField] private GameObject _weaponObject;
    [SerializeField] private GameObject _bulletSpawn;



    private GameObject _bulletHoleContainer;

    private GameObject _bullet;

    private Transform _weaponTr;

    private InputAction _shoot;
    private InputAction _reload;
    private InputAction _ADS;

    private Vector3 _startPos;
    private Quaternion _startRotation;

    [SerializeField] private CinemachineCamera _cameraCinema;
    [SerializeField] private Camera _camera;

    private WaitForSeconds _waitForReloadFull = new(4f);
    private WaitForSeconds _waitForReload = new(3.7f);



    public event Action<int, int, string> OnAmmoChanged;

    public void RefreshUI()
    {
        OnAmmoChanged?.Invoke(inMag, reserveAmmo, _name);
    }
    
    private void Awake()
    {
        _equipables = GetComponent<Equipables>();
    }

    private void OnEnable()
    {
        if (_equipables != null)
            _equipables.OnWeaponChange += HandleWeaponChange;

        _canReload = true;
    }

    private void OnDisable()
    {
        if (_equipables != null)
            _equipables.OnWeaponChange -= HandleWeaponChange;
    }

    private void Start()
    {
        _vaultLayer = LayerMask.GetMask("VaultLayer");

        _bulletHoleContainer = GameObject.Find("BulletHoles Container");

        _shoot = _equipables.playerInputs.FindAction("Shoot");

        _reload = _equipables.playerInputs.FindAction("Reload");

        _ADS = _equipables.playerInputs.FindAction("ADS");

        _bullet = weaponStatsPreset.Bullet;

        _weaponTr = _weaponObject.transform;

        _atStartPos = true;

        _startPos = _weaponTr.position;

        _name = weaponStatsPreset.WeaponName;

        inMag = weaponStatsPreset.MaxInMag;
        reserveAmmo = 120;

        
    }

    public void Initialize(Camera camera, CinemachineCamera cameraCinema, ViewModelAnimController viewModelAnim)
    {
        _camera = camera;

        _ogFOV = _camera.fieldOfView;

        _cameraCinema = cameraCinema;
        _FPController = viewModelAnim;
    }


    private void Update()
    {
        _canShoot = _equipables.canShoot;
        if (_timeSinceLastShot > 0.2f && !_comingBack && !_atStartPos)
            StartCoroutine(BackToPos(0.5f));

        if (_isActive && _canShoot)
            ActiveUpdate();
        else
            ADS(true);

        _timeSinceLastShot += Time.deltaTime;

        if (_reload.WasPressedThisFrame() && _canReload && _isActive)
            StartCoroutine(Reload());

        if (_ADS.WasPressedThisFrame())
            _aboutToAimIn = false;
        
        if (_ADS.WasReleasedThisFrame())
            _aboutToAimIn = true;
    }

    private void HandleWeaponChange(Equipables.Equipped newState, IAmmoCount ammoEquip)
    {

        _isActive = newState == Equipables.Equipped.primary;

        if (_isActive) OnAmmoChanged?.Invoke(inMag, reserveAmmo, _name);
    }

    private void ActiveUpdate()
    {
        
        if (inMag > 0)
        {
            if (_shoot.IsPressed() && _timeSinceLastShot >= 0.1f && _atStartPos)
            {
                StartCoroutine(FirstShoot(weaponStatsPreset));
            }
            else if (_shoot.IsPressed() && _timeSinceLastShot >= 0.1f)
            {
                StartCoroutine(Shoot(weaponStatsPreset));
            }
        }

        

        if (inMag < 0)
            inMag = 0;

        if(reserveAmmo == 0)
            _canReload = false;

        if ((_ADS.WasPressedThisFrame() || _ADS.WasReleasedThisFrame()))
            _aimedIn = ADS();

        _cameraCinema.Lens.FieldOfView = _aimedIn ? Mathf.Lerp(_cameraCinema.Lens.FieldOfView, 50f, 0.1f) : Mathf.Lerp(_cameraCinema.Lens.FieldOfView, _ogFOV, 0.1f);
    }

    private bool ADS(bool forceIntoState = false, bool forcedState = false)
    {
        if (!forceIntoState)
        {
            if (!_aboutToAimIn)
            {
                _FPController.SetBoolAll("Aimed in", false);

                

                

                //_bulletSpawn.transform.localPosition = new Vector3(-0.0025f, -0.005f, 0.0045f);
                return false;
            }
            else
            {
                _FPController.SetBoolAll("Aimed in", true);

                //_bulletSpawn.transform.localPosition = new Vector3(-0.0046f, -0.003f, 0.0055f);
                return true;
            }
        }
        else
        {
            if (forcedState)
            {
                _FPController.SetBoolAll("Aimed in", true);

                //_bulletSpawn.transform.localPosition = new Vector3(-0.0046f, -0.003f, 0.0055f);
                return true;
            }
            else
            {
                _FPController.SetBoolAll("Aimed in", false);

                //_bulletSpawn.transform.localPosition = new Vector3(-0.0025f, -0.005f, 0.0045f);
                return false;
            }
        }


    }

    



    IEnumerator FirstShoot(WeaponStatsPreset Ws)
    {
        _timeSinceLastShot = 0f;
        
        _recoilBias = UnityEngine.Random.Range(-1f, 1f);

        _startPos = _weaponTr.localPosition;
        _startRotation = _weaponTr.localRotation;



        _recoilReduction = 1f;
        _sprayAdd = 0f;

        _atStartPos = false;
        _startPos = _weaponTr.localPosition;
        _startRotation = _weaponTr.localRotation;

        yield return StartCoroutine(Shoot(Ws));
    }

    IEnumerator Shoot(WeaponStatsPreset Ws)
    {
        _FPController.SetTriggerAll("Shoot");
        
        inMag -= 1;

        _timeSinceLastShot = 0f;

        Debug.Log("Shoot");
        float RecoilX = UnityEngine.Random.Range(0f, Ws.RecoilX);
        float RecoilY = UnityEngine.Random.Range(0.7f, Ws.RecoilY);
        float RecoilZ = (_aimedIn) ? Ws.RecoilZ * 0.2f : Ws.RecoilZ;

        Vector3 NextRecoilState = new (_weaponTr.localPosition.x + 0.01f * RecoilX * _recoilBias, _weaponTr.localPosition.y + 0.003f * _recoilReduction * RecoilY, _weaponTr.localPosition.z + -0.01f * RecoilZ);

        //Vector3 currentEuler = _weaponTr.localRotation.eulerAngles;
        //Vector3 NextRecoilR = new (currentEuler.x - 1f, currentEuler.y, currentEuler.z);
        //Quaternion NextRecoilStateR = Quaternion.Euler(NextRecoilR);

        Player_Camera playerCam = _cameraCinema.GetComponent<Player_Camera>();

        float camRecoilX = 3f * RecoilX * _recoilBias;
        float camRecoilY = 5f * RecoilY;
        float camRecoilNX = playerCam.XRotation - camRecoilY;
        float camRecoilNY = playerCam.YRotation + camRecoilX;

        _weaponObject.GetComponent<CinemachineImpulseSource>().GenerateImpulse();

        float time = 0f;

        while (time < 0.05f)
        {
            
            
            _weaponTr.localPosition = (_aimedIn) ? Vector3.Lerp(_weaponTr.localPosition, _startPos, 0.05f) : Vector3.Lerp(_weaponTr.localPosition, NextRecoilState, 0.05f);
            //_weaponTr.localRotation = Quaternion.Lerp(_weaponTr.localRotation, NextRecoilStateR, 0.05f);

            playerCam.XRotation = Mathf.Lerp(playerCam.XRotation, camRecoilNX, 0.05f);
            playerCam.YRotation = Mathf.Lerp(playerCam.YRotation, camRecoilNY, 0.05f);


            time += Time.deltaTime;
            if (_recoilReduction <= 0f)
            {
                _recoilReduction = 0f;
            }
            else
            {
                _recoilReduction -= Time.deltaTime * 0.5f;
            }

            if (_sprayAdd < 1f && !_aimedIn)
            {
                _sprayAdd += Time.deltaTime * 0.2f;
            }
            else if (_aimedIn)
            {
                _sprayAdd = 0f;
            }

            //NextRecoilState.x = Mathf.Clamp(_weaponTr.localPosition.x, -0.465f, 0.388f);
            //NextRecoilState.y = Mathf.Clamp(_weaponTr.localPosition.y, _startPos.y, -0.14f);
            //NextRecoilState.z = Mathf.Clamp(_weaponTr.localPosition.z, _startPos.z, 0.545f);

            yield return null;
        }
        _weaponTr.localPosition = (_aimedIn) ? _startPos : NextRecoilState;


        Vector3 targetPoint;

        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out var hit, 76, ~LayerMask.GetMask("VaultLayer", "Ignore Raycast")))
        {
            if (hit.distance >= 1f) targetPoint = hit.point;
            else targetPoint = ray.GetPoint(75);

        }
        else
        {
            targetPoint = ray.GetPoint(75);
        }

        Vector3 dir = targetPoint - _bulletSpawn.transform.position;

        float Y = UnityEngine.Random.Range(-_sprayAdd, _sprayAdd);
        float X = UnityEngine.Random.Range(-_sprayAdd, _sprayAdd);

        Vector3 dirF = dir + new Vector3(X, Y, 0f);

        GameObject tempBullet = Instantiate(_bullet, _bulletSpawn.transform.position, Quaternion.identity);

        tempBullet.transform.SetParent(null, true);

        Vector3 dirFN = dirF.normalized;

        tempBullet.transform.forward = dirFN;

        tempBullet.GetComponent<Rigidbody>().AddForce(dirFN * Ws.BulletSpeed, ForceMode.Impulse);

        tempBullet.GetComponent<Bullet>().SendInfo(Ws.BulletSpeed, Ws.Damage, _bulletHoleContainer, _weaponObject, _equipables.team, transform.position);

        OnAmmoChanged?.Invoke(inMag, reserveAmmo, _name);
    }

    IEnumerator BackToPos(float duration)
    {
        float time = 0f;
        _comingBack = true;
        while (time < duration && _timeSinceLastShot > 0.5f)
        {
            //_weaponTr.localPosition = Vector3.Lerp(_weaponTr.localPosition, _startPos, time / duration);
            //_weaponTr.localRotation = Quaternion.Lerp(_weaponTr.localRotation, _startRotation, time / duration);

            _weaponTr.SetLocalPositionAndRotation(Vector3.Lerp(_weaponTr.localPosition, _startPos, time / duration), Quaternion.Lerp(_weaponTr.localRotation, _startRotation, time / duration));

            time += Time.deltaTime;
            yield return null;
        }

        if (_timeSinceLastShot < 0.5f)
        {
            _comingBack = false;
            yield break;
        }
        _weaponTr.localPosition = _startPos;
        _atStartPos = true;
        _comingBack = false;
        Debug.Log("CameBack");
    }

    IEnumerator Reload()
    {
        _canReload = false;
        _canShoot = false;
        bool emptyReload = inMag == 0;
        reserveAmmo += inMag;
        inMag = 0;

        OnAmmoChanged?.Invoke(inMag, reserveAmmo, _name);

        if (emptyReload)
        {
            _FPController.SetTriggerAll("ReloadFull");
            yield return _waitForReloadFull;
        }
        else
        {
            _FPController.SetTriggerAll("Reload");
            yield return _waitForReload;
        }
            

        

        if (reserveAmmo >= weaponStatsPreset.MaxInMag)
        {
            inMag += weaponStatsPreset.MaxInMag;
            reserveAmmo -= weaponStatsPreset.MaxInMag;
        }
        else
        {
            inMag = reserveAmmo;
            reserveAmmo = 0;
        }

        _canReload = true;
        _canShoot = true;

        OnAmmoChanged?.Invoke(inMag, reserveAmmo, _name);

        yield return null;
    }
}
