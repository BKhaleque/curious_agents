using System.Collections;
using System.Collections.Generic;
using Agents;
using UnityEngine;

public class AnticipatoryAgent : NonMLAgent
{

    public int numberOfRays; //Determines how many rays are cast
    public float radius; //Determines length of raycast 
    private Dictionary<Vector3, float> anticipatoryContextMap;
    private Dictionary<Vector3, float> nonAnticipatoryContextMap;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float angle = 0;
        for (int i = 0; i < numberOfRays; i++)
        {
            float x = Mathf.Sin (angle);
            float z = Mathf.Cos (angle);
            //float z = Mathf.Cos(angle);
            angle +=  Mathf.PI / numberOfRays;
            Vector3 dir = new Vector3 (transform.position.x + x,transform.position.y  , transform.position.z + z);
            RaycastHit hit;
            Debug.DrawLine (transform.position, dir, Color.red);
            if (Physics.Raycast (transform.position, dir, out hit, radius)) {
                //Do things
            }
        }
    }

    public override bool IsInView(GameObject origin, GameObject toCheck)
    {
        throw new System.NotImplementedException();
    }

    public override float CalculateInterestingness(GameObject gameObject)
    {
        throw new System.NotImplementedException();
    }
}
