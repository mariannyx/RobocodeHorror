using System.Collections;
using System.Runtime.CompilerServices;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using static Player_movement;

[RequireComponent(typeof(Equipables))]
public class ShootingSecondary : MonoBehaviour
{
    private Equipables _equipables;

    private float _timeSinceLastShot;

    private float _recoilReduction;
    private float _recoilBias;
    private float _sprayAdd;

    private int _vaultLayer;

    private bool _atStartPos;
    private bool _comingBack;
   
    
    [SerializeField] private WeaponStatsPreset _weaponStatsPreset;
    [SerializeField] private GameObject _weaponObject;
    [SerializeField] private GameObject _bulletSpawn;

    

    private GameObject _bulletHoleContainer;

    private GameObject _bullet;

    private Transform _weaponTr;

    private InputAction _shoot;

    private Vector3 _startPos;
    private Quaternion _startRotation;

    [SerializeField] private CinemachineCamera _cameraCinema;
    [SerializeField] private Camera _camera;

    


    private void Start()
    {
        _equipables = GetComponent<Equipables>();

        _vaultLayer = LayerMask.GetMask("VaultLayer");

        _bulletHoleContainer = GameObject.Find("BulletHoles Container");

        _shoot = _equipables.playerInputs.FindAction("Shoot");

        _bullet = _weaponStatsPreset.Bullet;

        _weaponTr = _weaponObject.transform;

        _atStartPos = true;

        
    }

    public void Initialize(Camera camera, CinemachineCamera cameraCinema)
    {
        _camera = camera;
        _cameraCinema = cameraCinema;
    }

    

    private void Update()
    {
        if (_shoot.IsPressed() && _timeSinceLastShot >= 0.15f && _atStartPos)
        {
            StartCoroutine(FirstShoot(_weaponStatsPreset));
        }
        else if (_shoot.IsPressed() && _timeSinceLastShot >= 0.15f)
        {
            StartCoroutine(Shoot(_weaponStatsPreset));
        }

        if (_timeSinceLastShot > 0.5f && !_comingBack && !_atStartPos)
        {
            StartCoroutine(BackToPos(0.5f));
        }

        _timeSinceLastShot += Time.deltaTime;
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

        StartCoroutine(Shoot(Ws));

        yield return StartCoroutine(Shoot(Ws));
    }

    IEnumerator Shoot(WeaponStatsPreset Ws)
    {
        _timeSinceLastShot = 0f;

        Debug.Log("Shoot");
        float RecoilX = UnityEngine.Random.Range(0f, Ws.RecoilX);
        float RecoilY = UnityEngine.Random.Range(0.7f, Ws.RecoilY);
        float RecoilZ = Ws.RecoilZ;

        Vector3 NextRecoilState = new (_weaponTr.localPosition.x + 0.03f * RecoilX * _recoilBias, _weaponTr.localPosition.y + 0.05f * _recoilReduction * RecoilY, _weaponTr.localPosition.z + 0.1f * RecoilZ);

        Vector3 currentEuler = _weaponTr.localRotation.eulerAngles;
        Vector3 NextRecoilR = new (currentEuler.x - 0.3f, currentEuler.y, currentEuler.z);
        Quaternion NextRecoilStateR = Quaternion.Euler(NextRecoilR);

        Player_Camera playerCam = _cameraCinema.GetComponent<Player_Camera>();

        float camRecoilX = 3f * RecoilX * _recoilBias;
        float camRecoilY = 5f * RecoilY;
        float camRecoilNX = playerCam.XRotation - camRecoilY;
        float camRecoilNY = playerCam.YRotation + camRecoilX;

        _weaponObject.GetComponent<CinemachineImpulseSource>().GenerateImpulse();

        float time = 0f;

        while (time < 0.05f)
        {
            
            
            //_weaponTr.localPosition = Vector3.Lerp(_weaponTr.localPosition, NextRecoilState, 0.05f);
            //_weaponTr.localRotation = Quaternion.Lerp(_weaponTr.localRotation, NextRecoilStateR, 0.05f);

            _weaponTr.SetLocalPositionAndRotation(Vector3.Lerp(_weaponTr.localPosition, NextRecoilState, 0.05f), Quaternion.Lerp(_weaponTr.localRotation, NextRecoilStateR, 0.05f));

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

            if (_sprayAdd < 1f)
            {
                _sprayAdd += Time.deltaTime * 0.2f;
            }

            NextRecoilState.x = Mathf.Clamp(_weaponTr.localPosition.x, -0.465f, 0.388f);
            NextRecoilState.y = Mathf.Clamp(_weaponTr.localPosition.y, _startPos.y, -0.14f);
            NextRecoilState.z = Mathf.Clamp(_weaponTr.localPosition.z, _startPos.z, 0.545f);

            yield return null;
        }
        _weaponTr.localPosition = NextRecoilState;


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

        Vector3 dir = targetPoint - _bulletSpawn.transform.position;

        float Y = UnityEngine.Random.Range(-_sprayAdd, _sprayAdd);
        float X = UnityEngine.Random.Range(-_sprayAdd, _sprayAdd);

        Vector3 dirF = dir + new Vector3(X, Y, 0f);

        GameObject tempBullet = Instantiate(_bullet, _bulletSpawn.transform.position, Quaternion.identity);

        tempBullet.transform.SetParent(null, true);

        Vector3 dirFN = dirF.normalized;

        tempBullet.transform.forward = dirFN;

        tempBullet.GetComponent<Rigidbody>().AddForce(dirFN * Ws.BulletSpeed, ForceMode.Impulse);

        tempBullet.GetComponent<Bullet>().SendInfo(Ws.BulletSpeed, Ws.Damage, _bulletHoleContainer, _weaponObject, ITeam.Teams.None, transform.position);
    }

    IEnumerator BackToPos(float duration)
    {
        float time = 0f;
        _comingBack = true;
        Debug.Log("ComingBack");
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
}
