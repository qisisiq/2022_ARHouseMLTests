using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dynamite3D.RealIvy;
using MagicLeap.Examples;

public class IvySeed : MonoBehaviour
{
    public LayerMask layerMask;
    //private IvyMeshing ivyMeshing;
    
    // Start is called before the first frame update
    void Start()
    {
        //ivyMeshing = FindObjectOfType<IvyMeshing>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("collision entered");

        if (true)
        {
            IvyMeshing.Instance.CastIvy(transform.position, Quaternion.Euler(other.contacts[0].normal));
            Debug.Log("cast ivy:" + transform.position);
        }
        
        Destroy(this);
    }
}
