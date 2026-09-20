using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class Equipables : MonoBehaviour
{
    public ITeam.Teams team;

    public Player_movement player;
    public Camera cam;
    public CinemachineCamera camCinema;
     

    public InputActionAsset playerInputs;
    public InputAction shoot;
    public InputAction primary;
    public InputAction secondary;
    public InputAction gadget1;
    public InputAction gadget2;
    public InputAction reload;

    public bool canShoot;

    private bool _lastInCommanderMenu;

    public Action<Equipped, IAmmoCount> OnWeaponChange;

    public Action<bool> OnCommanderMenu;

    private IAmmoCount _activeAmmo;

    public Equipped equipped;
    public enum Equipped
    {
        primary = 1,
        secondary,
        gadget1,
        gadget2
    }

    private Shooting _primary;
    private ShootingSecondary _secondary;
    private IGadget _gadget1;

    private void OnEnable()
    {
        if (player != null)
            player.OnMovementStateChange += HandleMovementStateChange;
    }

    private void OnDisable()
    {
        if (player != null)
            player.OnMovementStateChange -= HandleMovementStateChange;

        _primary.inMag = _primary.weaponStatsPreset.MaxInMag;

        _primary.reserveAmmo = 120;
        if (_activeAmmo != null)
            _activeAmmo.RefreshUI();

        
    }

    private void Start()
    {
        shoot = playerInputs.FindAction("Shoot");
        gadget1 = playerInputs.FindAction("GrenadeLauncher");
        primary = playerInputs.FindAction("primary");
        reload = playerInputs.FindAction("Reload");

        _primary = GetComponent<Shooting>();
        _secondary = GetComponent<ShootingSecondary>();

        IGadget[] gadgets = GetComponents<IGadget>();

        foreach (IGadget gadget in gadgets)
        {
            if (gadget.GadgetNum == 1)
            {
                _gadget1 = gadget;
            }
        }

        _activeAmmo = _primary;

        _activeAmmo.RefreshUI();
    }

    private void Update()
    {
        if (!player.InCommandMenu)
        {
            EquippedStateCheck();
        }

        if (player.InCommandMenu != _lastInCommanderMenu)
        {
            _lastInCommanderMenu = player.InCommandMenu;

            OnCommanderMenu?.Invoke(player.InCommandMenu);
        }
    }

    private void EquippedStateCheck()
    {
        Equipped preEquipped = equipped;
        
        if (primary.WasPressedThisFrame()) equipped = Equipped.primary;

       // if (secondary.WasPressedThisFrame()) equipped = Equipped.secondary;

        if (gadget1.WasPressedThisFrame()) equipped = Equipped.gadget1;

       // if (gadget2.WasPressedThisFrame()) equipped = Equipped.gadget2;

        if (preEquipped != equipped)
        {
            _activeAmmo = null;

            switch (equipped)
            {
                case Equipped.primary:
                    _activeAmmo = _primary;
                    break;
                //case Equipped.secondary:
                //    activeAmmo = _secondary;
                //    break;
                case Equipped.gadget1:
                    _activeAmmo = null;
                    break;

            }

            OnWeaponChange?.Invoke(equipped, _activeAmmo);
        }
    }

    private void HandleMovementStateChange(Player_movement.MovementState newState)
    {
        canShoot = (newState != Player_movement.MovementState.sprinting);
    }


    public void EquipInfo(Player_movement playerScript, CinemachineCamera cinemachineCam, Camera camera)
    {
        player = playerScript;
        camCinema = cinemachineCam;
        cam = camera;
        team = playerScript.Team;
    }

    public void ForceWeaponUI()
    {
        _activeAmmo = null;

        if (equipped == Equipped.primary)
        {
            if (_primary == null) _primary = GetComponent<Shooting>();
            _activeAmmo = _primary;
        }

        OnWeaponChange?.Invoke(equipped, _activeAmmo);
    }

}


