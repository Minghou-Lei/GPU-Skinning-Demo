using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSpawner : MonoBehaviour
{
    public GameObject go;

    public int row;
    public int column;
    // Start is called before the first frame update
    void Start()
    {
        for (int r = 0; r < row; r++)
        {
            for (int c = 0; c < column; c++)
            {
                GameObject newGameObject = Instantiate(go);
                newGameObject.transform.position += new Vector3((float) r, 0, (float) c);
            }
        }
        
        Destroy(go);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
