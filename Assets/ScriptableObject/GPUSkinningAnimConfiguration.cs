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

    public AnimationInfo(string name, int start, int end)
    {
        this.name = name;
        this.start = start;
        this.end = end;
    }
}
