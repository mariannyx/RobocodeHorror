using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HudHandler : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;

    private GameObject _outcomePanel;

    private GameObject _criticalStateOutline;
    private GameObject _deathScreen;

    private TextMeshProUGUI _timeToRespawnText;

    private TextMeshProUGUI _wonByText;

    private TextMeshProUGUI _ammoText;
    private TextMeshProUGUI _weaponNameText;

    private TextMeshProUGUI _UMFTxt;
    private TextMeshProUGUI _BFPMTxt;

    private int _lastUMFScore = -1;
    private int _lastBFPMScore = -1;

    private float _timeToRespawn;

    private Equipables _equipables;

    private Player_movement _player;

    private MatchManager _matchManager;

    private IAmmoCount _currentTrackedAmmo;

    private GameObject _commanderPanel;

    private void OnEnable()
    {
        if (_equipables != null)
        {
            _equipables.OnWeaponChange += HandleWeaponChange;
            _equipables.OnCommanderMenu += HandleCommanderMenu;
        }
    }

    private void OnDisable()
    {
        if (_equipables != null)
        {
            _equipables.OnWeaponChange -= HandleWeaponChange;
            _equipables.OnCommanderMenu -= HandleCommanderMenu;
        }
            
    }

    private void Start()
    {
        _player = _equipables.player;

        Transform HudPanel = _canvas.transform.Find("HudHolder");

        Transform textTr = HudPanel.transform.Find("AmmoCount");
        Transform nameTxtTr = HudPanel.transform.Find("WeaponName");
        Transform UMFTxtTr = HudPanel.transform.Find("UMF");
        Transform BFPMTxtTr = HudPanel.transform.Find("BFPM");

        _outcomePanel = _canvas.transform.Find("OutcomePanel").gameObject;

        _wonByText = _outcomePanel.transform.Find("won by team").GetComponent<TextMeshProUGUI>();

        _criticalStateOutline = _canvas.transform.Find("CriticalState").gameObject;

        _deathScreen = _canvas.transform.Find("DeathScreen").gameObject;

        _timeToRespawnText = _deathScreen.transform.Find("CountDown").GetComponent<TextMeshProUGUI>();


        if (textTr != null)
        {
            _ammoText = textTr.GetComponent<TextMeshProUGUI>();
        }

        if (nameTxtTr != null)
        {
            _weaponNameText = nameTxtTr.GetComponent<TextMeshProUGUI>();
        }

        if (UMFTxtTr != null)
        {
            _UMFTxt = UMFTxtTr.GetComponent<TextMeshProUGUI>();
        }

        if (BFPMTxtTr != null)
        {
            _BFPMTxt = BFPMTxtTr.GetComponent<TextMeshProUGUI>();
        }

        if (_equipables != null)
        {
            _equipables.ForceWeaponUI();
        }

        _commanderPanel = _canvas.transform.Find("SquadCommanderPanel").gameObject;


    }

    private void Awake()
    {
        _equipables = GetComponentInChildren<Equipables>();

        if (_matchManager == null)
        {
            _matchManager = GameObject.FindGameObjectWithTag("MatchManager").GetComponent<MatchManager>();
        }
    }


    private void HandleWeaponChange(Equipables.Equipped equipped, IAmmoCount handler)
    {
        Debug.Log("Handling Weapon Change");
        if (_currentTrackedAmmo != null)
        {
            _currentTrackedAmmo.OnAmmoChanged -= HandleAmmo;
        }

        _currentTrackedAmmo = handler;

        if (_currentTrackedAmmo != null)
        {
            _currentTrackedAmmo.OnAmmoChanged += HandleAmmo;
            Debug.Log("TrackedAmmo != null");

            _currentTrackedAmmo.RefreshUI();
        }
        else
        {
            _ammoText.text = "---";

            Debug.Log("TrackedAmmo is null");
        }
    }

    private void HandleCommanderMenu(bool menu)
    {
        _commanderPanel.SetActive(menu);
    }

    private void HandleAmmo(int inMag, int reserveAmmo, string tname)
    {
        Debug.Log("Handling Ammo");
        _ammoText.text = $"{inMag} | {reserveAmmo} Ammo";
        _weaponNameText.text = tname;

    }

    private void Update()
    {
        if (_matchManager.UMF_Score != _lastUMFScore)
        {
            _lastUMFScore = _matchManager.UMF_Score;
            _UMFTxt.text = $"UMF : {_lastUMFScore} | 50";
        }

        if (_matchManager.BFPM_Score != _lastBFPMScore)
        {
            _lastBFPMScore = _matchManager.BFPM_Score;
            _BFPMTxt.text = $"BFPM : {_lastBFPMScore} | 50";
        }

        if (_matchManager.matchState == MatchManager.ExpectedMatchState.PostMatch)
        {
            _outcomePanel.SetActive(true);

            if (_matchManager.BFPM_Score > _matchManager.UMF_Score)
            {
                if (_wonByText.text != "BFPM")
                    _wonByText.text = "BFPM";
            }
            else
            {
                if (_wonByText.text != "UMF")
                    _wonByText.text = "UMF";
            }
        }

        if (_player.Health <= 55 && !_criticalStateOutline.activeSelf)
        {
            _criticalStateOutline.SetActive(true);
        }
        else if (_player.Health > 55 && _criticalStateOutline.activeSelf)
        {
            _criticalStateOutline.SetActive(false);
        }

        if (!_player.gameObject.activeInHierarchy && !_deathScreen.activeSelf)
        {
            _deathScreen.SetActive(true);

            _timeToRespawn = 6f;
        }

        if (_deathScreen.activeSelf)
        {
            _timeToRespawn -= Time.deltaTime;

            _timeToRespawnText.text = _timeToRespawn.ToString();

            if (_timeToRespawn <= 0f || _player.gameObject.activeInHierarchy)
            {
                _deathScreen.SetActive(false);
            }
        }
    }
}
