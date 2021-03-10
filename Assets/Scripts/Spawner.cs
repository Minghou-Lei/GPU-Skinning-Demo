using UnityEngine;

public class Spawner : MonoBehaviour
{
    public int row;
    public int column;
    public Mesh bakedMesh;
    public Material bakedMaterial;
    private MaterialPropertyBlock mpb;
    private GameObject spawn;

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
    private void Start()
    {
        for (var r = 0; r < row; r++)
        for (var c = 0; c < column; c++)
        {
            var rc = Instantiate(spawn);
            var mpb = new MaterialPropertyBlock();
            rc.transform.parent = transform;
            rc.transform.position += new Vector3(r, 0, c);
            //rc.GetComponent<MeshRenderer>().material.SetFloat("_Random",Random.Range(0f,1f));
            rc.GetComponent<MeshRenderer>().GetPropertyBlock(mpb);
            mpb.SetFloat("_Offset", Random.Range(0f, 1f));
            rc.GetComponent<MeshRenderer>().SetPropertyBlock(mpb);
        }
    }
}