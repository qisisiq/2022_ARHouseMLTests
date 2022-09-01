using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlacer : MonoBehaviour
{
    private float horizontalRandom = 1f;
    private float verticalRandom = 0.55f;

    // Start is called before the first frame update
    void Start()
    {
        var origPosition = this.transform.position;
        
        /*
        // approximating a sphere of where my current object is 
        Collider[] hitColliders = Physics.OverlapSphere(this.transform.position - Vector3.down * -4.52f, 0.3f);
        
        for(int i = 0; i < 7; i++)
        {
            transform.position = origPosition + new Vector3(
                Random.Range(-horizontalRandom, horizontalRandom),
                Random.Range(-verticalRandom, verticalRandom),
                Random.Range(-horizontalRandom, horizontalRandom));
            
            hitColliders = Physics.OverlapSphere(this.transform.position - Vector3.down * -4.52f, 0.3f);
            if (hitColliders.Length == 0)
            {
                return;
            }

        }
    */
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
