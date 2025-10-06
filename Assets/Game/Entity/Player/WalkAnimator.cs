using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkAnimator : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;

    private bool _isWalk = false;
    public bool IsWalk
    {
        get { return _isWalk; }
        set
        {
            _isWalk = value;
            _animator.SetBool("IsWalk", _isWalk);
        }
    }
}
