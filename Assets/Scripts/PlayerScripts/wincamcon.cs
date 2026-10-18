using UnityEngine;
using UnityEngine.InputSystem;
public class wincamcon : MonoBehaviour
{
    [SerializeField] private InputActionAsset _playerInput;
    private InputAction moveAction;
    [SerializeField] private Camera PlayerCamera;
[SerializeField] private Transform qwer;
 [SerializeField] private GameObject player;
 public bool incam = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
         _playerInput.FindActionMap("On foot 2nd Person").Enable();  
        moveAction = _playerInput.FindAction("exit");
    }

    // Update is called once per frame
    void Update()
    {
        if(moveAction.WasPressedThisFrame())
        {
             

            if(player != null)
            {
                player.SetActive(true);
                incam = false;
             PlayerCamera.transform.SetPositionAndRotation(qwer.position, qwer.rotation);
             gameObject.SetActive(false);
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {

        Debug.Log("hit");
        if(other.gameObject.CompareTag("Player"))
        {
            incam = true;
            Debug.Log("player entered");
            player = other.gameObject;
            other.gameObject.SetActive(false);

        }
    }
//    private void OntriggerExit(Collider collision)
//     {
//         if(collision.gameObject.CompareTag("Player"))
//         {
//             if(player != null)
//             {
//             player = null;}

//         }
//     }
}
