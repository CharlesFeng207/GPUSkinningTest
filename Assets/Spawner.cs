using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject Prefab;
    public int Count;
    public float Range;
    
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < Count;i ++)
        {
            var go = Instantiate(Prefab, transform); 
            go.transform.localPosition = Random.insideUnitSphere * Range;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
