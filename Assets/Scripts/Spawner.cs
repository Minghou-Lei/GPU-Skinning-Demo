using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    
    public int row;
    public int column;
    public Mesh bakedMesh;
    public Material bakedMaterial;
    private GameObject spawn;
    private MaterialPropertyBlock mpb;

    private void Awake()
    {
        spawn = new GameObject();
        spawn.name = "spawned";
        spawn.transform.parent = transform;
        
        spawn.AddComponent<MeshFilter>().sharedMesh = bakedMesh;
        spawn.AddComponent<MeshRenderer>().material = bakedMaterial;

        mpb = new MaterialPropertyBlock();
        spawn.GetComponent<MeshRenderer>().GetPropertyBlock(mpb);
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int r = 0; r < row; r++)
        {
            for (int c = 0; c < column; c++)
            {
                GameObject rc = Instantiate(spawn);
                rc.transform.parent = transform;
                rc.transform.position += new Vector3(r, 0, c);

            }
        }
    }
}
