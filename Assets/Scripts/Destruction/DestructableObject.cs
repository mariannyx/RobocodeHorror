using TMPro;
using UnityEngine;

public class DestructableObject : MonoBehaviour
{
    [SerializeField] private GameObject _manager;
    private DestructionManager _managerF;
    public GameObject _coverUpParticle;
    [SerializeField] private Vector3 ParticleOffcet;
    public float _health = 100;
    private bool _dead = false;

    void Start()
    {
        _managerF = _manager.GetComponent<DestructionManager>();
    }

    private void Update()
    {
        if (_health <= 0 && !_dead)
        {
            gameObject.SetActive(false);
            _dead = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Bullet"))
        {
            _health -= collision.gameObject.GetComponent<Bullet>().Damage;
            
        }
    }
}
