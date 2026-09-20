using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    public float Damage { get; private set; }
    private float _dmgU;
    //private float _velocity;
    private Rigidbody _rb;
    [SerializeField] private float _health = 3;
    //private float _startHealth;
    private bool _alreadyHit, _explosive;
    private GameObject _weapon;
    private float _time;
    private int _mask;
    public Vector3 lastPosOfParent;
    public ITeam.Teams team;


    [SerializeField] private GameObject _bulletHole, _bulletHoleContainer;






    private void Start()
    {
        //_startHealth = _health;
    }
    public void SendInfo(float Vel, float Dmg, GameObject BHC, GameObject Weapon, ITeam.Teams Team, Vector3 LastPosOfParent, bool IsExplosive = false)
    {
        _rb = GetComponent<Rigidbody>();
        _rb.AddForce(transform.forward * Vel, ForceMode.Impulse);
        _dmgU = Dmg;
        //_velocity = Vel;
        _bulletHoleContainer = BHC;
        _mask = LayerMask.GetMask("VaultLayer", "FirstPerson", "Player", "Ignore Raycast");
        _mask = ~_mask;
        _explosive = IsExplosive;
        _weapon = Weapon;
        lastPosOfParent = LastPosOfParent;
        team = Team;

        Damage = _dmgU;

    }

    private void Update()
    {
        //Damage = _rb.linearVelocity.magnitude / _velocity * _dmgU;

        

        if (_health <= 0f)
        {
            Destroy(gameObject, 0.1f);
        }

        _health -= Time.deltaTime * 0.2f;

        _time += Time.deltaTime;

        Ray ray = new (transform.position, transform.forward);
        Ray rayDown = new (transform.position, transform.up * -1);
        Debug.DrawRay(ray.origin, ray.direction * 0.8f, Color.red);
        Debug.DrawRay(rayDown.origin, rayDown.direction * 0.8f, Color.red);

        if (Physics.Raycast(ray, out var hit, 0.8f, _mask) && !_alreadyHit)
        {
            GameObject g = hit.transform.gameObject;
            if (g.name != "Bullet(Clone)" && g != _weapon && !g.CompareTag("WallHitEffect") && !g.CompareTag("Bullet"))
            {
                GameObject tempBulletHole = Instantiate(_bulletHole, hit.point, Quaternion.identity);
                Quaternion targetRotation = Quaternion.LookRotation(ray.direction * 180f);

                tempBulletHole.TryGetComponent<CinemachineImpulseSource>(out CinemachineImpulseSource component);
                if (component != null) component.GenerateImpulse();


                tempBulletHole.transform.rotation = targetRotation;
                tempBulletHole.transform.SetParent(_bulletHoleContainer.transform);
                tempBulletHole.transform.Rotate(Vector3.forward, Random.Range(0f, 360f));

                _bulletHoleContainer.GetComponent<DecalDeleter>().DecalDelete(tempBulletHole);

                _alreadyHit = true;
            }

        }

        if (_explosive)
        {
            if (Physics.Raycast(rayDown, out var hitDown, 0.3f, _mask) && !_alreadyHit  && _time > 0.017f)
            {
                GameObject g = hitDown.transform.gameObject;
                if (g.name != "Bullet(Clone)" && g != _weapon && !g.CompareTag("WallHitEffect"))
                {
                    GameObject tempBulletHole = Instantiate(_bulletHole, hitDown.point, Quaternion.identity);
                    Quaternion targetRotation = Quaternion.LookRotation(rayDown.direction * 180f);

                    tempBulletHole.TryGetComponent<CinemachineImpulseSource>(out CinemachineImpulseSource component);
                    if (component != null) component.GenerateImpulse();

                    tempBulletHole.transform.rotation = targetRotation;
                    tempBulletHole.transform.SetParent(_bulletHoleContainer.transform);
                    tempBulletHole.transform.Rotate(Vector3.forward, Random.Range(0f, 360f));

                    

                    _bulletHoleContainer.GetComponent<DecalDeleter>().DecalDelete(tempBulletHole);

                    _alreadyHit = true;
                }

            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Bullet"))
        {
            _health -= 0.7f;
        }
            
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Bullet"))
        {
            _health -= 0.5f;
        }

        if (collision.gameObject.CompareTag("Destructable"))
        {
            DestructableObject destrObj = collision.gameObject.GetComponent<DestructableObject>();

            destrObj._health -= Damage;
            if (destrObj._health <= 0f)
            {
                GameObject tempCoverUp = Instantiate(destrObj._coverUpParticle, transform.position, Quaternion.identity);
                tempCoverUp.TryGetComponent<CinemachineImpulseSource>(out CinemachineImpulseSource component);
                if (component != null) component.GenerateImpulse();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Bullet"))
        {
            _health -= 0.2f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("WallHitEffect"))
        { 
            _alreadyHit = false; 
        }
    }

}
