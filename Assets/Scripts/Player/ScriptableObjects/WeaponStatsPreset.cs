using UnityEngine;

[CreateAssetMenu(fileName = "WeaponStatsPreset", menuName = "Scriptable Objects/WeaponStatsPreset")]
public class WeaponStatsPreset : ScriptableObject
{
    public GameObject Bullet;
    public float BulletSpeed, MovementSpeedMultiply, RecoilX, RecoilY, RecoilZ, AimDuration, Damage;
    public int MaxInMag;
    public bool IsExplosive;
    public bool SingleFire, BurstFire, AutoFire;
    public string WeaponName;
}
