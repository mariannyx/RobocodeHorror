using UnityEngine;

public class BotShooting : MonoBehaviour
{


    [SerializeField] private WeaponStatsPreset _ws;

    private GameObject _bulletSpawn;

    private Vector3 hostilePos;

    private BotController bot;

    private GameObject _bulletHoleContainer;

    private void OnEnable()
    {
        if (bot != null)
            bot.ShootEvent += Bot_ShootEvent;
    }

    private void OnDisable()
    {
        if (bot != null)
            bot.ShootEvent -= Bot_ShootEvent;
    }

    private void Awake()
    {
        bot = GetComponent<BotController>();

        _bulletSpawn = GameObject.Find($"{gameObject.name}/BulletSpawn");

        _bulletHoleContainer = GameObject.Find("BulletHoles Container");
    }

    private void Bot_ShootEvent(int inMag, float shootCool, Vector3 hostilePosit, ITeam.Teams team)
    {
        hostilePos = hostilePosit;

        Shooting();
    }

    void Start()
    {
        
    }

    private void Shooting()
    {
        Vector3 targetPoint = hostilePos;



        Vector3 dir = targetPoint - _bulletSpawn.transform.position;

        float Y = UnityEngine.Random.Range(-0.2f, 0.2f);
        float X = UnityEngine.Random.Range(-0.5f, 0.5f);

        Vector3 dirF = dir + new Vector3(X, Y, 0f);

        GameObject tempBullet = Instantiate(_ws.Bullet, _bulletSpawn.transform.position, Quaternion.identity);

        Vector3 dirFN = dirF.normalized;

        tempBullet.transform.forward = dirFN;

        tempBullet.transform.SetParent(null, true);

        tempBullet.GetComponent<Rigidbody>().AddForce(dirFN * _ws.BulletSpeed, ForceMode.Impulse);

        tempBullet.GetComponent<Bullet>().SendInfo(_ws.BulletSpeed, _ws.Damage, _bulletHoleContainer, _bulletSpawn, bot.Team, transform.position);
    }
}
