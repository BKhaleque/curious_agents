using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Agents;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class AnticipatoryAgent : NonMLAgent
{

    public int numberOfRays; //Determines how many rays are cast
    public float radius; //Determines length of raycast 
    private Dictionary<Vector3, float> anticipatoryContextMap;
    private Dictionary<Vector3, float> nonAnticipatoryContextMap;
    private float turnFraction = (float)(1 + Math.Sqrt(5)) / 2;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        anticipatoryContextMap = new Dictionary<Vector3, float>();
        nonAnticipatoryContextMap = new Dictionary<Vector3, float>();
        buildContextMap();
        if (anticipatoryContextMap.Count == 0)
        {
            transform.position  += RandomNavmeshLocation(10) * Time.deltaTime;
            Debug.Log("No anticipation found");
        }
        else
        {
            Debug.Log("Anticipation found");
        
            float noHitsummation = 0f;
            float hitSummation = 0f;
            foreach (var kv in anticipatoryContextMap)
            {
                noHitsummation = kv.Value;
            }
            foreach (var kv in nonAnticipatoryContextMap)
            {
                hitSummation = kv.Value;
            }
        
            if (hitSummation > noHitsummation)
                transform.position -= nonAnticipatoryContextMap.ElementAt(0).Key * radius *Time.deltaTime ;
            else if (noHitsummation > hitSummation)
                transform.position += anticipatoryContextMap.ElementAt(0).Key * radius *Time.deltaTime;
            else
                transform.position  += RandomNavmeshLocation(10) * Time.deltaTime;
        }

                
    }

    void buildContextMap()
    {
        float angle = 0;
        int numOfRayCastsHit = 0;
        List<Vector3> hitDirs = new List<Vector3>();
        List<Vector3> noHitDirs = new List<Vector3>();
        for (int i = 0; i < numberOfRays; i++)
        {
            float x = radius * Mathf.Cos (angle);
            float y = radius * Mathf.Sin (angle);
            //float z = Mathf.Cos(angle);
            angle +=  2* Mathf.PI *turnFraction ;
            Vector3 dir = new Vector3 (transform.position.x + x,transform.position.y + y  , transform.position.z + radius );
            RaycastHit hit;
            Debug.DrawLine (transform.position, dir, Color.red);
            if (Physics.Raycast (transform.position, dir, out hit, radius))
            {
                numOfRayCastsHit++;
                hitDirs.Add(dir);
                //for the raycasts that did hit add them to the non anticipatory context map
                //nonAnticipatoryContextMap.Add(dir, 1f-(1/numberOfRays));

            }
            else
            {
                //for the raycasts that didn't hit, add them to the anticipatory context map
                noHitDirs.Add(dir);
                //anticipatoryContextMap.Add(dir,1f-(1/numberOfRays));
            }
        }

        if (numOfRayCastsHit ==0||numOfRayCastsHit == numberOfRays)
        {
            //transform.position = RandomNavmeshLocation(radius);
            return;
        }

        for (int i = 0; i < hitDirs.Count; i++)
            nonAnticipatoryContextMap.Add(hitDirs[i], 1f-(1/numOfRayCastsHit));   
        

        for (int i = 0; i < noHitDirs.Count; i++)
            anticipatoryContextMap.Add(noHitDirs[i], 1f-(1/numOfRayCastsHit));

    }
    
    private Vector3 RandomNavmeshLocation(float radius) {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;
        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1)) {
            finalPosition = hit.position;
        }
        return finalPosition;
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
