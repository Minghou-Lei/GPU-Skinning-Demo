using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

public class Baker : MonoBehaviour
{
    public enum BAKEMODE
    {
        bone,
        vertex
    }

    public enum UVCHANNEL
    {
        UV,
        UV2,
        UV3
    }

    [HideInInspector] public BAKEMODE Bakemode;

    [HideInInspector] public UVCHANNEL outW;

    [HideInInspector] public UVCHANNEL outI;

    [HideInInspector] public Mesh mesh;
    [HideInInspector] public ComputeShader computeShader;
    [HideInInspector] public Button button;

    private void Awake()
    {
        button = GameObject.Find("Record").GetComponent<Button>();
        button.onClick.AddListener(onClick);
    }

    private void onClick()
    {
        if (Bakemode == BAKEMODE.bone)
        {
            if (mesh == null) throw new Exception("请添加Mesh");

            if (outI == outW) throw new Exception("频道冲突，请重选");

            BakeBoneTexture();
        }

        if (Bakemode == BAKEMODE.vertex) StartCoroutine(BakeVertexTexture());
    }

    private void BakeBoneTexture()
    {
        
        var animator = GetComponent<Animator>();
        var clips = animator.runtimeAnimatorController.animationClips;
        var skin = GetComponentInChildren<SkinnedMeshRenderer>();
        var boneCount = mesh.bindposes.Length;
        GPUSkinningAnimConfiguration sac = ScriptableObject.CreateInstance<GPUSkinningAnimConfiguration>();
        sac.boneCount = boneCount;
        sac.modelName = name;

        animator.speed = 1;
        var textWidth = boneCount;
        List<AnimationInfo> animationInfos = new List<AnimationInfo>();
        List<Texture2D> textures = new List<Texture2D>();
        List<int> frameCounts = new List<int>();
        List<int> frameRates = new List<int>();
        
        foreach (var clip in clips)
        {
            Debug.Log(clip.name);
            var frameCount = (int)(clip.frameRate * clip.length);
            var boneTex = CreateBoneTex(animator, skin, clip, mesh, 512, frameCount);
            /*
            var colors = new Color[boneCount * frameCount * 36];
            */
            boneTex.name = string.Format("{0}.{1}.BoneMatrix", name, clip.name);
            textures.Add(boneTex);
            frameCounts.Add(frameCount);
            frameRates.Add((int)clip.frameRate);
            SaveAsJPG(boneTex, Path.Combine("Assets/BakedDemoImgs"), boneTex.name);
            AssetDatabase.CreateAsset(boneTex, Path.Combine("Assets/BakedMatrixs", boneTex.name + ".asset"));
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        var bakedMesh = new Mesh();
        bakedMesh = Instantiate(mesh);
        bakedMesh.name = string.Format("{0}.mesh", name);
        MappingBoneIndexAndWeightToMeshUV(bakedMesh, UVChannel.UV2, UVChannel.UV3);
        
        sac.bakedMesh = bakedMesh;
        /*
        var start = 0;
        var height = 0;
        foreach (var texture in textures)
        {
            start = height;
            height += texture.height;
            AnimationInfo info = new AnimationInfo(texture.name, start, height-1);
            animationInfos.Add(info);
        }
        */
        
        var start = 0;
        var height = 0;
        for (int i = 0; i < textures.Count; ++i)
        {
            start = height;
            height += textures[i].height;
            AnimationInfo info = new AnimationInfo(textures[i].name, start, height-1,frameCounts[i],frameRates[i]);
            animationInfos.Add(info);
        }
        
        sac.animationInfos = animationInfos.ToArray();

        var combined = combineTextures(textures);
        combined.name = name;
        AssetDatabase.CreateAsset(combined, Path.Combine("Assets/BakedMatrixs", combined.name + ".ALL.asset"));
        
        AssetDatabase.CreateAsset(bakedMesh, Path.Combine("Assets/BakedMesh", bakedMesh.name+".BakedMesh" + ".mesh"));
        AssetDatabase.CreateAsset(sac, Path.Combine("Assets/ScriptableObject", name + ".asset"));
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    // 2021年3月11日 - 待完成
    private Texture2D combineTextures(List<Texture2D> textures)
    {
        var height = 0;
        var start = 0;
        foreach (var texture in textures)
        {
            start = height;
            height += texture.height;
        }
        
        var result = new Texture2D(512, height, TextureFormat.RGBA32, false);
        height = 0;
        foreach (var texture in textures)
        {
            result.SetPixels(0,height,texture.width,texture.height,texture.GetPixels());
            height += texture.height;
        }
        return result;
    }

    private IEnumerator BakeVertexTexture()
    {
        var animator = GetComponent<Animator>();
        var clips = animator.runtimeAnimatorController.animationClips;
        var skin = GetComponentInChildren<SkinnedMeshRenderer>();
        var vertCount = skin.sharedMesh.vertexCount;

        var mesh = new Mesh();
        animator.speed = 1;
        var textWidth = vertCount;
        foreach (var clip in clips)
        {
            var frameCount = (int) (clip.length * clip.frameRate);
            Debug.Log("Frames:" + clip.length * clip.frameRate);
            var vertexs = new List<VertexInfo>();

            var positionRenderTexture =
                new RenderTexture(textWidth, frameCount, 0, RenderTextureFormat.ARGBHalf);
            var normalRenderTexture =
                new RenderTexture(textWidth, frameCount, 0, RenderTextureFormat.ARGBHalf);

            positionRenderTexture.name = string.Format("{0}.{1}.position", name, clip.name);
            normalRenderTexture.name = string.Format("{0}.{1}.normal", name, clip.name);

            foreach (var renderTexture in new[] {positionRenderTexture, normalRenderTexture})
            {
                renderTexture.enableRandomWrite = true;
                renderTexture.Create();
                RenderTexture.active = renderTexture;
                GL.Clear(true, true, Color.clear);
            }

            animator.Play(clip.name);
            yield return 0;

            for (var i = 0; i < frameCount; ++i)
            {
                animator.Play(clip.name, 0, (float) i / frameCount);
                yield return 0;
                skin.BakeMesh(mesh);
                vertexs.AddRange(Enumerable.Range(0, vertCount).Select(idx => new VertexInfo
                {
                    position = mesh.vertices[idx],
                    normal = mesh.normals[idx]
                }));
            }

            var buffer = new ComputeBuffer(vertexs.Count,
                Marshal.SizeOf(typeof(VertexInfo)));
            buffer.SetData(vertexs);

            var kernelID = computeShader.FindKernel("CSMain");

            computeShader.SetInt("vertCount", vertCount);
            computeShader.SetBuffer(kernelID, "meshInfo", buffer);
            computeShader.SetTexture(kernelID, "OutPositionTexture", positionRenderTexture);
            computeShader.SetTexture(kernelID, "OutNormalTexture", normalRenderTexture);

            computeShader.Dispatch(kernelID, vertCount, frameCount, 1);
            buffer.Release();

#if UNITY_EDITOR

            var pt = Convert(positionRenderTexture);
            var nt = Convert(normalRenderTexture);

            Graphics.CopyTexture(positionRenderTexture, pt);
            Graphics.CopyTexture(normalRenderTexture, nt);

            AssetDatabase.CreateAsset(pt, Path.Combine("Assets", positionRenderTexture.name + ".asset"));
            AssetDatabase.CreateAsset(nt, Path.Combine("Assets", normalRenderTexture.name + ".asset"));

            SaveAsJPG(pt, Path.Combine("Assets"), positionRenderTexture.name);
            SaveAsJPG(nt, Path.Combine("Assets"), normalRenderTexture.name);


            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }

        yield return null;
    }


    public void SaveAsJPG(Texture2D texture2D, string contents, string pngName)
    {
        var bytes = texture2D.EncodeToJPG();
        var file = File.Open(contents + "/" + pngName + ".jpg", FileMode.Create);
        var writer = new BinaryWriter(file);
        writer.Write(bytes);
        file.Close();
        writer.Close();
    }

    public static Texture2D Convert(RenderTexture rt)
    {
        var texture2D = new Texture2D(rt.width, rt.height, TextureFormat.RGBAHalf, false);
        RenderTexture.active = rt;
        texture2D.ReadPixels(Rect.MinMaxRect(0, 0, rt.width, rt.height), 0, 0);
        RenderTexture.active = null;
        return texture2D;
    }

    private Texture2D CreateBoneTex(Animator animator, SkinnedMeshRenderer skinnedMeshRenderer, AnimationClip clip,
        Mesh mesh,
        int width, int animFrameCount)
    {
        var bindPoses = mesh.bindposes;
        var bones = skinnedMeshRenderer.bones;

        var bonesCount = bones.Length;
        if (bindPoses.Length != bones.Length)
            return null;
        animator.applyRootMotion = false;
        animator.Play(clip.name);

        // 开始采样
        var lines = Mathf.CeilToInt((float) bones.Length * animFrameCount * 12 / width);
        var result = new Texture2D(width, lines, TextureFormat.RGBA32, false);
        result.filterMode = FilterMode.Point;
        result.wrapMode = TextureWrapMode.Clamp;
        var colors = new Color[width * lines * 3];
        // 逐帧写入矩阵
        for (var i = 0; i <= animFrameCount; i++)
        {
            clip.SampleAnimation(gameObject, i / clip.frameRate);


            // 写入变换后的矩阵
            for (var j = 0; j < bonesCount; j++)
            {
                var matrix = transform.worldToLocalMatrix * bones[j].localToWorldMatrix * bindPoses[j];

                colors[(i * bonesCount + j) * 12 + 0] = EncodeFloatRGBA(matrix.m00);
                colors[(i * bonesCount + j) * 12 + 1] = EncodeFloatRGBA(matrix.m01);
                colors[(i * bonesCount + j) * 12 + 2] = EncodeFloatRGBA(matrix.m02);
                colors[(i * bonesCount + j) * 12 + 3] = EncodeFloatRGBA(matrix.m03);

                colors[(i * bonesCount + j) * 12 + 4] = EncodeFloatRGBA(matrix.m10);
                colors[(i * bonesCount + j) * 12 + 5] = EncodeFloatRGBA(matrix.m11);
                colors[(i * bonesCount + j) * 12 + 6] = EncodeFloatRGBA(matrix.m12);
                colors[(i * bonesCount + j) * 12 + 7] = EncodeFloatRGBA(matrix.m13);

                colors[(i * bonesCount + j) * 12 + 8] = EncodeFloatRGBA(matrix.m20);
                colors[(i * bonesCount + j) * 12 + 9] = EncodeFloatRGBA(matrix.m21);
                colors[(i * bonesCount + j) * 12 + 10] = EncodeFloatRGBA(matrix.m22);
                colors[(i * bonesCount + j) * 12 + 11] = EncodeFloatRGBA(matrix.m23);
            }
        }

        //从左到右，从下到上
        result.SetPixels(colors);
        result.Apply();

        return result;
    }

    private static Vector4 EncodeFloatRGBA(float v)
    {
        v = v * 0.01f + 0.5f;
        var kEncodeMul = new Vector4(1.0f, 255.0f, 65025.0f, 160581375.0f);
        var kEncodeBit = 1.0f / 255.0f;
        var enc = kEncodeMul * v;
        for (var i = 0; i < 4; i++)
            enc[i] = enc[i] - Mathf.Floor(enc[i]);
        enc = enc - new Vector4(enc.y, enc.z, enc.w, enc.w) * kEncodeBit;
        return enc;
    }

    public static bool MappingBoneIndexAndWeightToMeshUV(Mesh mesh, UVChannel indexChannel, UVChannel weightChannel)
    {
        var boneWeights = mesh.boneWeights;
        var weightUV = new List<Vector2>();
        var indexUV = new List<Vector2>();

        mesh.GetUVs((int) weightChannel, weightUV);
        mesh.GetUVs((int) indexChannel, indexUV);

        weightUV = new List<Vector2>();
        indexUV = new List<Vector2>();

        for (var i = 0; i < boneWeights.Length; i++)
        {
            var bw = boneWeights[i];
            indexUV.Add(new Vector2(bw.boneIndex0, bw.boneIndex1));
            weightUV.Add(new Vector2(bw.weight0, bw.weight1));
        }

        mesh.SetUVs((int) weightChannel, weightUV);
        mesh.SetUVs((int) indexChannel, indexUV);
        return true;
    }
}