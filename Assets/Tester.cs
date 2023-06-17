using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
    public GameObject Prefab;
    public int Count;
    public float Range;

    
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        
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
