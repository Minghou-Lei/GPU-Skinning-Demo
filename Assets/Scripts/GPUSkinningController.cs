using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GPUSkinningController : MonoBehaviour
{
    public GPUSkinningAnimConfiguration animations;
    public int activeAnimationIndex = 1;
    public List<AnimationInfo> AnimationInfos;
    public bool isRandom = false;
    public MeshRenderer mr;

    private void Awake()
    {
        AnimationInfos = animations.animationInfos.ToList();
        mr = GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        if (isRandom)
        {
            System.Random random = new System.Random();
            activeAnimationIndex = Random.Range(0, AnimationInfos.Count - 1);
        }
    }

    private void Update()
    {
        AnimationInfo info = animations.animationInfos[activeAnimationIndex-1];
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        mr.GetPropertyBlock(mpb);
        if (isRandom)
        {
            mpb.SetFloat("_FrameCount",info.frameCount);
            mpb.SetFloat("_Start",info.start);
            mpb.SetFloat("_Offset",Random.Range(0f,1f));
        }
        else
        {
            mpb.SetFloat("_FrameCount",info.frameCount);
            mpb.SetFloat("_Start",info.start);
        }
        mr.SetPropertyBlock(mpb);
    }
}
