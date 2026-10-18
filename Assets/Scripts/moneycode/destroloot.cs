using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class destroloot : MonoBehaviour
{

    public List<GameObject> scrablist = new List<GameObject>();
    public GameObject Esmoke;
     [SerializeField] private InputActionAsset _playerInput;
    private InputAction _destroyAction;
    [SerializeField] private int scrabAmount =1;
    private readonly HashSet<int> _processedTargets = new HashSet<int>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
         _playerInput.FindActionMap("On foot 2nd Person").Enable();  
        _destroyAction = _playerInput.FindAction("destroy");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerStay(Collider other)
    {
        if (!_destroyAction.WasPressedThisFrame() || !other.gameObject.CompareTag("destroyable"))
        {
            return;
        }

        if (!_processedTargets.Add(other.gameObject.GetInstanceID()))
        {
            return;
        }

        for (int i = 1; i <= scrabAmount; i++)
        {
            Instantiate(scrablist[Random.Range(0, scrablist.Count)], other.transform.position, Quaternion.identity);
            print("scrab spawned" + i);
            }
         
        Destroy(other.gameObject);
        Instantiate(Esmoke, other.transform.position, other.transform.rotation);
    }
}
