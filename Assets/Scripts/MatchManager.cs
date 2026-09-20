
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MatchManager : MonoBehaviour
{
    public ExpectedMatchState matchState = ExpectedMatchState.PreStart;
    
    public enum ExpectedMatchState
    {
        PreStart = 1,
        InProgress,
        PostMatch
    }



    public int UMF_Score = 0;
    public int BFPM_Score = 0;

    private bool _postStart;

    public Transform UMF_Spawn;
    public Transform BFPM_Spawn;

    public List<Squad> UMF_Squads = new();
    public List<Squad> BFPM_Squads = new();

    [SerializeField] public int UMF_Agents = 0;
    [SerializeField] public int BFPM_Agents = 0;


    [SerializeField] private float _preStartDuration = 10f;
    private float _timePassed = 0;

    public HashSet<GameObject> agents = new();

    [SerializeField] private List<GameObject> _interestP = new();

    public List<Vector3> interestPoints = new();

    

    private void Awake()
    {
        foreach (GameObject point in _interestP)
        {
            interestPoints.Add(point.transform.position);
        }

        UMF_Squads.Add(new Squad());
        BFPM_Squads.Add(new Squad());
    }

    private void Update()
    {
        _timePassed += Time.deltaTime;

        
        if (UMF_Score >= 50 || BFPM_Score >= 50)
        {
            matchState = ExpectedMatchState.PostMatch;
        }

        if (matchState == ExpectedMatchState.PostMatch && !_postStart)
        {
            _timePassed = 0;
            Time.timeScale = 0.3f;

            _postStart = true;
        }

        if (matchState == ExpectedMatchState.PostMatch && _timePassed > 1f)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Time.timeScale = 1f;

            SceneManager.LoadScene(0);
        }

        if (matchState == ExpectedMatchState.PreStart)
        {
            PreStart();
        }
        else if (matchState == ExpectedMatchState.InProgress)
        {
            InProgress();
        }
        else
        {
            
        }

    }

    private void PreStart()
    {
        if (_timePassed > 10f)
        {
            _timePassed = 0;

            matchState = ExpectedMatchState.InProgress;
        }
    }

    private void InProgress()
    {
        if (UMF_Score > 50f)
        {
            matchState = ExpectedMatchState.PostMatch;
        }

        if (BFPM_Score > 50f)
        {
            matchState = ExpectedMatchState.PostMatch;
        }
    }

    public void RegisterDeath(ITeam.Teams team)
    {
        if (team == ITeam.Teams.UMF)
        {
            BFPM_Score++;
        }
        else if (team == ITeam.Teams.BFPM)
        {
            UMF_Score++;
        }
    }

    

    public void RegisterAgent(GameObject agent, ITeam interf)
    {
        Debug.Log("3");

        float xU = UnityEngine.Random.Range(0f, 4f);
        float zU = UnityEngine.Random.Range(0f, 4f);

        //if (!agents.Contains(agent))
        //{
        agents.Add(agent);
        //}

        if (interf.Team == ITeam.Teams.None)
        {
            if (UMF_Agents > BFPM_Agents)
            {
                interf.Team = ITeam.Teams.BFPM;

                interf.accesibleObject.transform.position = BFPM_Spawn.position + new Vector3(xU, 0, zU);

                BFPM_Agents++;

                Debug.Log("2");
            }
            else if (UMF_Agents == BFPM_Agents)
            {
                interf.Team = ITeam.Teams.UMF;

                interf.accesibleObject.transform.position = UMF_Spawn.position + new Vector3(xU, 0, zU);

                UMF_Agents++;

                Debug.Log("1");
            }
            else
            {
                interf.Team = ITeam.Teams.UMF;

                interf.accesibleObject.transform.position = UMF_Spawn.position + new Vector3(xU, 0, zU);

                UMF_Agents++;

                Debug.Log("1");
            }
        }

        if (interf.Team == ITeam.Teams.UMF)
        {
            Squad UMF_Squad = UMF_Squads[^1];

            if (UMF_Squad.members.Count < 4)
            {
                UMF_Squad.members.Add(interf);

                interf.Squad = UMF_Squad;
            }
            else
            {
                UMF_Squads.Add(new Squad());
                UMF_Squads[^1].members.Add(interf);
                UMF_Squads[^1].squadTeam = ITeam.Teams.UMF;

                interf.Squad = UMF_Squads[^1];
            }

            UMF_Squads[^1].ElectLeader();
        }
        else if (interf.Team == ITeam.Teams.BFPM)
        {
            Squad BFPM_Squad = BFPM_Squads[^1];

            if (BFPM_Squad.members.Count < 4)
            {
                BFPM_Squad.members.Add(interf);

                interf.Squad = BFPM_Squad;
            }
            else
            {
                BFPM_Squads.Add(new Squad());
                BFPM_Squads[^1].members.Add(interf);

                BFPM_Squads[^1].squadTeam = ITeam.Teams.BFPM;

                interf.Squad = BFPM_Squads[^1];

                
            }

            BFPM_Squads[^1].ElectLeader();
        }
    }
    
    public IEnumerator SpawnOnTime(float timetoSpawn, ITeam.Teams team, GameObject toSpawn)
    {
        yield return new WaitForSeconds(timetoSpawn);

        if (team == ITeam.Teams.UMF)
        {
            toSpawn.transform.position = UMF_Spawn.position;
        }
        else if (team == ITeam.Teams.BFPM)
        {
            toSpawn.transform.position = BFPM_Spawn.position;
        }

        toSpawn.SetActive(true);

        if (TryGetComponent(out BotController bot))
        {
            ResetBot(bot);
        }

    }

    private void ResetBot(BotController bot)
    {
        bot.AttackExpected = false;
        bot.visibleTargets.Clear();

        bot.StateMachine.ChangeState(bot.Idle);
    }

}
