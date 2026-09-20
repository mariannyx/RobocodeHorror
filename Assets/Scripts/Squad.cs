using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Squad
{
    public ITeam.Teams squadTeam;

    public List<ITeam> members = new();

    public GameObject Leader;

    public void ElectLeader()
    {
        bool HasPlayer = false;

        foreach (var member in members)
        {
            if (member.IsPlayer)
            {
                Leader = member.accesibleObject;

                HasPlayer = true;
            }
        }

        if (!HasPlayer)
        {
            if (members.Count > 0 && members != null)
            {
                Leader = members[0].accesibleObject;
            }
                
        }


    }

    public void HandleFollowLeader()
    {
        foreach (var member in members)
        {
            if (member.accesibleObject.activeSelf)
                member.FollowLeader();
        }
    }

    public void HandleMoveToPos(Vector3 pos)
    {
        foreach (var member in members)
        {
            if (member.accesibleObject.activeSelf)
                member.MoveToPos(pos);
        }
    }

    public void HandleScatter()
    {
        foreach (var member in members)
        {
            if (member.accesibleObject.activeSelf)
                member.MoveToRandomPOI();
        }
    }
}
