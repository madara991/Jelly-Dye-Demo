using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testFlowfluid : MonoBehaviour
{

    public GameObject InsPref;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Instantiate(InsPref, pos, Quaternion.identity);
        }
    }
}
