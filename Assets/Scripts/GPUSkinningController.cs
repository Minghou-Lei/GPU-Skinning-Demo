using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GPUSkinningController : MonoBehaviour
{
    public GPUSkinningAnimConfiguration animations;
    public int activeAnimationIndex = 1;
    public List<AnimationInfo> AnimationInfos;

    private void Awake()
    {
        AnimationInfos = animations.animationInfos.ToList();
    }

    private void Update()
    {
        AnimationInfo info = animations.animationInfos[activeAnimationIndex-1];
        MeshRenderer mr = transform.GetComponent<MeshRenderer>();
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        mr.GetPropertyBlock(mpb);
        mpb.SetFloat("_FrameCount",info.frameCount);
        mpb.SetFloat("_Start",info.start);
        mr.SetPropertyBlock(mpb);
    }
}
