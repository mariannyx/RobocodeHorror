using UnityEngine;

public class DecalDeleter : MonoBehaviour
{
    public void DecalDelete(GameObject decal)
    {
        Destroy(decal, 10f);
    }
}
