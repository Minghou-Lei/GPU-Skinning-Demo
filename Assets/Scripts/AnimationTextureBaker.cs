using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

struct VertexInfo{
	public Vector3 position;
	public Vector3 normal;
}

public enum UVChannel
{
	UV1,
	UV2,
	UV3,
	UV4,
	UV5,
	UV6,
	UV7,
	UV8,
}

public class AnimationTextureBaker : MonoBehaviour{
	public Button button;
	public ComputeShader computeShader;
	private GameObject finishedText;

	private void Start()
	{
		button = GameObject.Find("Record").GetComponent<Button>();
		finishedText = GameObject.Find("FinishedText");
		finishedText.SetActive(false);
		button.onClick.AddListener(onClick);
	}

	void onClick(){
		StartCoroutine(BakeAnimation());
	}

	IEnumerator BakeAnimation(){
		Animator animator = GetComponent<Animator>();
		AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
		SkinnedMeshRenderer skin = GetComponentInChildren<SkinnedMeshRenderer>();
		int vertCount = skin.sharedMesh.vertexCount;
		
		Mesh mesh = new Mesh();
		animator.speed = 1;
		int textWidth = vertCount;
		foreach (var clip in clips)
		{
			int frameCount = (int)(clip.length * clip.frameRate);
			Debug.Log("Frames:" + clip.length * clip.frameRate);
			List<VertexInfo> vertexs = new List<VertexInfo>();
			
			RenderTexture positionRenderTexture = new RenderTexture(textWidth,frameCount,0,RenderTextureFormat.ARGBHalf);
			RenderTexture normalRenderTexture = new RenderTexture(textWidth,frameCount,0,RenderTextureFormat.ARGBHalf);

			positionRenderTexture.name = string.Format("{0}.{1}.position", name, clip.name);
			normalRenderTexture.name = string.Format("{0}.{1}.normal", name, clip.name);

			foreach (RenderTexture renderTexture in new[]{positionRenderTexture,normalRenderTexture}){
				renderTexture.enableRandomWrite = true;
				renderTexture.Create();
				RenderTexture.active = renderTexture;
				GL.Clear(true,true,Color.clear);
			}
			
			animator.Play(clip.name);
			yield return 0;

			for (int i = 0; i < frameCount; ++i){
				animator.Play(clip.name,0,(float)i/frameCount);
				yield return 0;
				skin.BakeMesh(mesh);
				vertexs.AddRange(Enumerable.Range(0,vertCount).Select(idx=>new VertexInfo()
				{
					position = mesh.vertices[idx],
					normal = mesh.normals[idx]
				}));
			}
			
			ComputeBuffer buffer = new ComputeBuffer(vertexs.Count,System.Runtime.InteropServices.Marshal.SizeOf(typeof(VertexInfo)));
			buffer.SetData(vertexs);
			
			int kernelID = computeShader.FindKernel("CSMain");
			
			computeShader.SetInt("vertCount",vertCount);
			computeShader.SetBuffer(kernelID,"meshInfo",buffer);
			computeShader.SetTexture(kernelID,"OutPositionTexture",positionRenderTexture);
			computeShader.SetTexture(kernelID,"OutNormalTexture",normalRenderTexture);
			
			computeShader.Dispatch(kernelID,vertCount,frameCount,1);
			buffer.Release();

#if UNITY_EDITOR

			Texture2D pt = Convert(positionRenderTexture);
			Texture2D nt = Convert(normalRenderTexture);
			
			Graphics.CopyTexture(positionRenderTexture,pt);
			Graphics.CopyTexture(normalRenderTexture,nt);
			
			AssetDatabase.CreateAsset(pt,Path.Combine("Assets",positionRenderTexture.name+".asset"));
			AssetDatabase.CreateAsset(nt,Path.Combine("Assets",normalRenderTexture.name+".asset"));
			
			SaveAsJPG(pt,Path.Combine("Assets"),positionRenderTexture.name);
			SaveAsJPG(nt,Path.Combine("Assets"),normalRenderTexture.name);
			
			finishedText.SetActive(true);
			
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

#endif

		}
		yield return null;
	}

	public static Texture2D Convert(RenderTexture rt){
		Texture2D texture2D = new Texture2D(rt.width,rt.height,TextureFormat.RGBAHalf,false);
		RenderTexture.active = rt;
		texture2D.ReadPixels(Rect.MinMaxRect(0,0,rt.width,rt.height),0,0);
		RenderTexture.active = null;
		return texture2D;
	}

	public static void SaveAsJPG(Texture2D texture2D, string contents, string pngName){
		byte[] bytes = texture2D.EncodeToJPG();
		FileStream file = File.Open(contents + "/" + pngName + ".jpg", FileMode.Create);
		BinaryWriter writer = new BinaryWriter(file);
		writer.Write(bytes);
		file.Close();
		writer.Close();
	}
}
