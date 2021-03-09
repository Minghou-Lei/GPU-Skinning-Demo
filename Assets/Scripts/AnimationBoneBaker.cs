using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public enum UVChannel
{
    UV,
    UV2,
    UV3,
    UV4,
    UV5,
    UV6,
    UV7,
    UV8
}


public class AnimationBoneBaker : MonoBehaviour
{
    public Button button;
    public Mesh mesh;

    private void Start()
    {
        button = GameObject.Find("Record").GetComponent<Button>();
        button.onClick.AddListener(onClick);
    }

    private void onClick()
    {
        StartCoroutine(BakeBone());
    }

    private IEnumerator BakeBone()
    {
        var animator = GetComponent<Animator>();
        var clips = animator.runtimeAnimatorController.animationClips;
        var skin = GetComponentInChildren<SkinnedMeshRenderer>();
        var boneCount = mesh.bindposes.Length;

        animator.speed = 1;
        var textWidth = boneCount;
        foreach (var clip in clips)
        {
            var frameCount = (int) (clip.length * clip.frameRate);
            var matrixs = new List<Matrix4x4>();
            /*
            RenderTexture matrixsTexture = new RenderTexture(
                textWidth,
                frameCount,
                0,
                RenderTextureFormat.ARGBHalf
            );

            matrixsTexture.name = string.Format("{0}.{1}.BoneMatrix", name, clip.name);

            matrixsTexture.enableRandomWrite = true;
            matrixsTexture.Create();
            RenderTexture.active = matrixsTexture;
            GL.Clear(true, true, Color.clear);
            */
            yield return 0;

            /*
            for (int i = 0; i < frameCount; ++i)
            {
                animator.Play(clip.name, 0, (float) i / frameCount);
                yield return 0;
                
                Transform[] bones = skin.bones;
                foreach (var bone in bones)
                {
                    Matrix4x4 add = Matrix4x4.TRS(bone.position,bone.rotation.normalized,Vector3.one);
                    matrixs.Add(add);
                }
            }
            */
            var boneTex = CreateBoneTex(animator, skin, clip, mesh, 512, frameCount);
            Debug.Log("BoneCount:" + boneCount + "\tFrameCount:" + frameCount + "\tFrameRate:" + clip.frameRate);

            boneTex.name = string.Format("{0}.{1}.BoneMatrix", name, clip.name);
            SaveAsJPG(boneTex, Path.Combine("Assets"), boneTex.name);
            AssetDatabase.CreateAsset(boneTex, Path.Combine("Assets", boneTex.name + ".asset"));


            var bakedMesh = new Mesh();
            bakedMesh = Instantiate(mesh);
            bakedMesh.name = string.Format("{0}.{1}.mesh", name, clip.name);
            MappingBoneIndexAndWeightToMeshUV(bakedMesh, UVChannel.UV2, UVChannel.UV3);
            AssetDatabase.CreateAsset(bakedMesh, Path.Combine("Assets", bakedMesh.name + ".mesh"));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }


    public static void SaveAsJPG(Texture2D texture2D, string contents, string pngName)
    {
        var bytes = texture2D.EncodeToJPG();
        var file = File.Open(contents + "/" + pngName + ".jpg", FileMode.Create);
        var writer = new BinaryWriter(file);
        writer.Write(bytes);
        file.Close();
        writer.Close();
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
        //List<Color> colors = new List<Color>();
        // 逐帧写入矩阵
        for (var i = 0; i <= animFrameCount; i++)
        {
            clip.SampleAnimation(gameObject, i / clip.frameRate);
            

            // 写入变换后的矩阵
            for (var j = 0; j < bonesCount; j++)
            {
                Matrix4x4 matrix = transform.worldToLocalMatrix * bones[j].localToWorldMatrix * bindPoses[j];
                //Debug.Log(matrix.ToString());
                //Matrix4x4 matrix = bones[j].localToWorldMatrix * bindPoses[j];

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