using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GPUSkinningController : MonoBehaviour
{
    public GPUSkinningAnimConfiguration animations;
    public int activeAnimationIndex = 0;
    public List<AnimationInfo> AnimationInfos;
    public bool isRandom = false;
    private MeshRenderer mr;
    private float r,c;

    private void Awake()
    {
        AnimationInfos = animations.animationInfos.ToList();
        mr = GetComponent<MeshRenderer>();
        System.Random random = new System.Random();
        r = Random.Range(0f, 1f);
        c = Random.Range(0, AnimationInfos.Count);
    }

    private void Start()
    {
        if (isRandom)
        {
            activeAnimationIndex = (int)c;
        }
    }

    private void Update()
    {
        AnimationInfo info = animations.animationInfos[activeAnimationIndex];
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        mr.GetPropertyBlock(mpb);
        if (isRandom)
        {
            mpb.SetFloat("_FrameCount",info.frameCount);
            mpb.SetFloat("_Start",info.start);
            mpb.SetFloat("_Offset",r);
        }
        else
        {
            mpb.SetFloat("_FrameCount",info.frameCount);
            mpb.SetFloat("_Start",info.start);
        }
        mr.SetPropertyBlock(mpb);
    }
}
