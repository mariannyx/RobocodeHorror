using UnityEngine;
using UnityEngine.InputSystem;


public class Player_Camera : MonoBehaviour
{
    public float XRotation, YRotation;
    [SerializeField] private float _sensX, _sensY;

    [SerializeField] private Player_movement _player;

    [SerializeField] private GameObject _head;

    //[SerializeField] private GameObject _fpCam;


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
    }

    void Update()
    {
        XRotation -= _player.LookDir.y * _sensX;
        YRotation += _player.LookDir.x * _sensY;
        XRotation = Mathf.Clamp(XRotation, -90f, 90f);

        //if (XRotation < 0)
        //{
        //    _fpCam.transform.rotation = Quaternion.Euler(XRotation / 45f, transform.rotation.y, transform.rotation.z);
        //}
        //else if (XRotation > 0)
        //{
        //    _fpCam.transform.rotation = Quaternion.Euler(XRotation / 18f, 0, 0);
        //}

        transform.rotation = Quaternion.Euler(XRotation + _player.NextMoveRotState.z, YRotation, _player.NextMoveRotState.x);
        _player.transform.rotation = Quaternion.Euler(0f, YRotation, 0f);
    }

    
}
