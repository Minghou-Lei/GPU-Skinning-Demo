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
            activeAnimationIndex = Random.Range(0, AnimationInfos.Count);
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
