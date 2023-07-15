using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Tester : MonoBehaviour
{
    public GameObject Prefab;
    public int Count;
    public float Range;
    public bool EnableInstancing;
    
    // Start is called before the first frame update
    void Start()
    {
        
        
        GPUSkinningPlayerResources.EnableInstancing = EnableInstancing;
        Application.targetFrameRate = 60;

        for(int i = 0; i < Count;i ++)
        {
            var go = Instantiate(Prefab, transform);
            var pos = Random.insideUnitSphere * Range;
            pos.y = 0;
            go.transform.localPosition = pos;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
