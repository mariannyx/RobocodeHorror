using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;


public class DestructionManager : MonoBehaviour
{
    public IEnumerator DestroyWParticles(float duration, GameObject destructObject, GameObject particles, Vector3 offcet)
    {

        GameObject tempParticles = Instantiate(particles);
        tempParticles.transform.localPosition = destructObject.transform.TransformPoint(offcet);

        yield return new WaitForSeconds(0.1f);
        Destroy(destructObject, duration);
        
    }
}
