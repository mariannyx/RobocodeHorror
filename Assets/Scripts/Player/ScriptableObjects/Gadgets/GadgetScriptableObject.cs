using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GadgetScriptableObject", menuName = "Scriptable Objects/GadgetScriptableObject")]

public class GadgetScriptableObject : ScriptableObject
{
    public GameObject parentPrefab, gadgetPrefab, projectile;
    public string childName;
    public string GadgetName;

    public float cooldown;
    public float useTime;
    public float damage;

    public int maxUses;
    public int maxInMagazine;

    public GadgetType gadgetType;


    

    public enum GadgetType
    {
        Attachment,
        Usable,
        Weaponry,
        Placeable
    }

    private void OnEnable()
    {
        if (gadgetPrefab == null && childName != null)
        {
            gadgetPrefab = GameObject.Find(childName);
        }
    }
}