using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
public class pickupscript : MonoBehaviour
{
    public int count = 0;
    public List<GameObject> scrablistcolect = new List<GameObject>();
    [SerializeField] private InputActionAsset _playerInput;
    private InputAction _pickupAction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _playerInput.FindActionMap("On foot 2nd Person").Enable();  
        _pickupAction = _playerInput.FindAction("pickup");
    }

    // Update is called once per frame
    void Update(){}
   void OnTriggerStay(Collider other)
    {
      if(_pickupAction.WasPressedThisFrame())
        {
            if(other.gameObject.CompareTag("scrab"))
            {
                scrablistcolect.Add(other.gameObject);
                count++;
               other.gameObject.SetActive(false);
                print("scrab picked up " + count);
            }
        }
       
    }
}
