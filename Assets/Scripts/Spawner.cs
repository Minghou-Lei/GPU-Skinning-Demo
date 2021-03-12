using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using Random = UnityEngine.Random;

public enum GPUINSTANCINGMODE
{
    Auto,
    Manual
}

public class RenderObjectData
{
    public Vector3 pos;
    public Vector3 scale;
    public Quaternion rot;
    
    public RenderObjectData(Vector3 pos, Vector3 scale, Quaternion rot)
    {
        this.pos = pos;
        this.scale = scale;
        this.rot = rot;
    }

    public Matrix4x4 matrix
    {
        get
        {
            return Matrix4x4.TRS(pos, rot, scale);
        }
        
        
    }
}

public class BatchData
{
    public List<RenderObjectData> RenderObjectDatas;
    public MaterialPropertyBlock mpb;

    public BatchData(List<RenderObjectData> renderObjectDatas, MaterialPropertyBlock mpb)
    {
        RenderObjectDatas = renderObjectDatas;
        this.mpb = mpb;
    }
}

public class Spawner : MonoBehaviour
{
    public int row;
    public int column;
    public GPUINSTANCINGMODE GPUInstancingMode;
    public int batchSize = 1000;

    public Mesh bakedMesh;
    public Material bakedMaterial;
    private MaterialPropertyBlock mpb;
    private GameObject spawn;
    private System.Random random;
    private static List<BatchData> batches = new List<BatchData>();


    private void Awake()
    {
        random = new System.Random();

        if (GPUInstancingMode == GPUINSTANCINGMODE.Auto)
        {
            spawn = new GameObject();
            spawn.name = "spawned";
            spawn.transform.parent = transform;

            spawn.AddComponent<MeshFilter>().sharedMesh = bakedMesh;
            spawn.AddComponent<MeshRenderer>().material = bakedMaterial;
        }
        
    }

    // Start is called before the first frame update
    private void Start()
    {
        if (GPUInstancingMode == GPUINSTANCINGMODE.Auto)
        {
            for (var r = 0; r < row; r++)
            for (var c = 0; c < column; c++)
            {
                var rc = Instantiate(spawn);
                MeshRenderer mr = rc.GetComponent<MeshRenderer>();
                mr.material = bakedMaterial;
                
                rc.transform.parent = transform;
                rc.transform.position += new Vector3(r, 0, c);
                rc.gameObject.name = mr.name;
                mpb = new MaterialPropertyBlock();
                mr.GetPropertyBlock(mpb);
                mpb.SetFloat("_Offset", Random.Range(0f, 1f));
                mr.SetPropertyBlock(mpb);
            
            }
        }

        if (GPUInstancingMode == GPUINSTANCINGMODE.Manual)
        {
            int batchIndexNum = 0;
            List<RenderObjectData> currBatch = new List<RenderObjectData>();
            for (int r = 0; r < row; r++)
            {
                for (int c = 0; c < column; c++)
                {
                    Vector3 position = new Vector3(r, 0, c);
                    RenderObjectData renderObjectData =
                        new RenderObjectData(position, new Vector3(1, 1, 1), Quaternion.identity);
                    currBatch.Add(renderObjectData);
                    ++batchIndexNum;
                    if (batchIndexNum >= batchSize)
                    {
                        MaterialPropertyBlock m = new MaterialPropertyBlock();
                        m.SetFloat("_Offset",Random.Range(0f,1f));
                        BatchData bd = new BatchData(currBatch.GetRange(0, currBatch.Count),
                            m);
                        batches.Add(bd);
                        currBatch.Clear();
                        batchIndexNum = 0;
                    }
                }
            }
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            mpb.SetFloat("_Offset",Random.Range(0f,1f));
            BatchData last = new BatchData(currBatch.GetRange(0, currBatch.Count), mpb);
            batches.Add(last);
            currBatch.Clear();
        }
    }

    void RenderBatches()
    {
        foreach (var batch in batches)
        {
            Graphics.DrawMeshInstanced(bakedMesh,0,bakedMaterial,batch.RenderObjectDatas.Select((a)=>a.matrix).ToList(),batch.mpb );
        }
    }

    private void Update()
    {
        if(GPUInstancingMode == GPUINSTANCINGMODE.Manual)
            RenderBatches();
    }
}