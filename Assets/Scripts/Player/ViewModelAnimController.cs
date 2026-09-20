using UnityEngine;

public class ViewModelAnimController : MonoBehaviour
{
    private Animator[] _animators;

    private void Awake()
    {
        _animators = GetComponentsInChildren<Animator>();
    }

    public void SetBoolAll(string boolName, bool state = true)
    {
        foreach (var anim in  _animators)
        {
            anim.SetBool(boolName, state);
        }
    }


    public void SetTriggerAll(string triggerName)
    {
        foreach (var anim in _animators)
        {
            anim.SetTrigger(triggerName);
        }
    }

    public float AnimLength()
    {
        float length = 0;
        foreach (var anim in _animators)
        {
            length = anim.GetCurrentAnimatorClipInfo(0).Length;
        }
        
        if (length != 0)
        {
            return length;
        }
        else
        {
            return 0;
        }
    }



}
