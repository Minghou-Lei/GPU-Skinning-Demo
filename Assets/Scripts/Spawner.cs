using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public int row;
    public int column;
    public Mesh bakedMesh;
    public Material bakedMaterial;
    private MaterialPropertyBlock mpb;
    private GameObject spawn;
    public List<Material> materials;
    private System.Random random;
    
    private void Awake()
    {
        random = new System.Random();
        spawn = new GameObject();
        spawn.name = "spawned";
        spawn.transform.parent = transform;

        spawn.AddComponent<MeshFilter>().sharedMesh = bakedMesh;
        spawn.AddComponent<MeshRenderer>().material = bakedMaterial;

        mpb = new MaterialPropertyBlock();
        spawn.GetComponent<MeshRenderer>().GetPropertyBlock(mpb);
    }

    // Start is called before the first frame update
    private void Start()
    {
        for (var r = 0; r < row; r++)
        for (var c = 0; c < column; c++)
        {
            var rc = Instantiate(spawn);
            MeshRenderer mr = rc.GetComponent<MeshRenderer>();
            int i = random.Next(materials.Count);
            mr.material = materials[i];
            var mpb = new MaterialPropertyBlock();
            rc.transform.parent = transform;
            rc.transform.position += new Vector3(r, 0, c);
            rc.gameObject.name = mr.name;
            
            mr.GetPropertyBlock(mpb);
            mpb.SetFloat("_Offset", Random.Range(0f, 1f));
            mr.SetPropertyBlock(mpb);
        }
    }
}