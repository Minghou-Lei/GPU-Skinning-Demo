using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorRandomPlayer : MonoBehaviour
{
    private Animator _animator;

    public int random;
    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        random = Random.Range(0, _animator.runtimeAnimatorController.animationClips.Length);
        _animator.Play(_animator.runtimeAnimatorController.animationClips[random].name);
    }

}
