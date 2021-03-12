using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="GPU Skinning/Create GPUSkinningAnimConfiguration")]
public class GPUSkinningAnimConfiguration : ScriptableObject
{
    public string modelName = null;
    public int boneCount = 0;
    public AnimationInfo[] animationInfos = null;
    public Mesh bakedMesh;
}

[System.Serializable]
public class AnimationInfo
{
    [HideInInspector]
    public string name;
    public int start;
    public int end;
    public int frameCount;
    public int frameRate;

    public AnimationInfo(string name, int start, int end)
    {
        this.name = name;
        this.start = start;
        this.end = end;
    }

    public AnimationInfo(string name, int start, int end, int frameCount, int frameRate)
    {
        this.name = name;
        this.start = start;
        this.end = end;
        this.frameCount = frameCount;
        this.frameRate = frameRate;
    }

    public string Name
    {
        get => name;
        set => name = value;
    }

    public int Start
    {
        get => start;
        set => start = value;
    }

    public int End
    {
        get => end;
        set => end = value;
    }

    public int FrameCount
    {
        get => frameCount;
        set => frameCount = value;
    }

    public int FrameRate
    {
        get => frameRate;
        set => frameRate = value;
    }
}
