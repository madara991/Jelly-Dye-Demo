using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControler : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            // start injection color
            // animtion
            return;
        }
        else
        if (Input.GetMouseButtonUp(1))
        {
            // end injection color
            // animation
        }

        Vector3 mousePOsition = Camera.main.ScreenToWorldPoint(Input.mousePosition); 
		transform.position = new Vector3(mousePOsition.x, transform.position.y, mousePOsition.z);

    }



}
