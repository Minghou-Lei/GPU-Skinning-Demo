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
    public string name;
    public float frameRate;
    public float frameCount;
    public AnimationInfo(string name, float frameRate, float frameCount)
    {
        this.name = name;
        this.frameRate = frameRate;
        this.frameCount = frameCount;
    }
}
