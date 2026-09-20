using UnityEngine;

public interface ITeam
{
    public Teams Team { get; set; }

    public bool IsPlayer { get; set; }

    public GameObject accesibleObject { get; set; }

    public enum Teams
    {
        None = 0,
        UMF, // united military forces
        BFPM // better future private military
    }

    public Squad Squad { get; set; }

    void FollowLeader()
    {
        
    }

    void MoveToPos(Vector3 pos)
    {

    }

    void MoveToRandomPOI()
    {

    }
}
