using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Agents;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class TrueRandomAgent : NonMLAgent
{
    // Start is called before the first frame update
    
    public GameObject trueRandomAgent;
    public bool hasMemory;
    public bool hasWeighting;
    public bool usingNMax;
    
    private GameObject[] allObjects;
    private Dictionary<Vector3, float> interestMeasureTable;
    private HashSet <String>typesSeen;
    private int nMax;
    private int distance;
    private int noOfObjectsSeen;
    NavMeshPath p;

    

    void Start()
    {
        currentIters = 0;
        objectsSeen = new Dictionary<GameObject, int>();
        interestMeasureTable = new Dictionary<Vector3, float>();
        allObjects = FindObjectsOfType<GameObject>();
        typesSeen = new HashSet<String>();
        trueRandomAgent.transform.position = Vector3.zero;
        nMax = 1;
        distance = 10;
        foreach (var t in allObjects)
        {
            IsInView(trueRandomAgent,t);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (currentIters <= steps)
        {
            var interestMeasure = 0f;
            var rotTable = new Dictionary<Quaternion, float>();
            noOfObjectsSeen = 0;

            for (var i = 0; i < 4; i++)
            {
                interestMeasure += allObjects.Where(t => IsInView(trueRandomAgent, t)).Sum(t =>
                    // ReSharper disable once PossibleLossOfFraction
                    objectsSeen.Where(kv => kv.Key == t).Sum(kv => (1 / kv.Value) * calculateInterestingness(t)));
            }

            var randDirection = Random.Range(0, 4);
            switch (randDirection)
            {
                case 0:
                {
                    cam.transform.Rotate(0,90,0);
                    break;
                }
                case 1:
                {
                    cam.transform.Rotate(0,180,0);
                    break;
                }
                case 2:
                {
                    cam.transform.Rotate(0,270,0);
                    break;
                }
                case 3:
                    cam.transform.Rotate(0,0,0);
                    break;
            }
            //cam.transform.rotation = Random.rotation;

            transform.position +=  cam.transform.forward * distance;

            // if (navMeshAgent.CalculatePath(target, p) && p.status == NavMeshPathStatus.PathComplete)
            // {
            //     //move to target
            //     navMeshAgent.SetPath(p);
            //     gameObject.transform.position = target;
            //     //Debug.Log("I can get here at point (" + exploratoryAgent.gameObject.transform.position.x + stepSize + ", " + exploratoryAgent.gameObject.transform.position.z + stepSize + ") from origin with step size " + stepSize);
            //
            // }
            // else
            // {
            //     //Debug.Log("I can't get here at point (" + exploratoryAgent.gameObject.transform.position.x + xStepSize + ", " + exploratoryAgent.gameObject.transform.position.z + zStepSize + ") ");
            //     gameObject.transform.position = RandomNavmeshLocation(10f);
            //     //go to random point 10 steps away from target
            // }

            if (currentIters == steps)
            {
                var filePath = GETPath();
                var writer = File.CreateText(filePath);
                writer.WriteLine("Coord;Interestingness");
                foreach (var kv in interestMeasureTable)
                {
                    writer.WriteLine("{0};{1}", kv.Key, kv.Value);
                }
            }

            currentIters++;
        }
        else
        {
            gameObject.SetActive(false);
        }
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
            var pointOnScreen = cam.WorldToScreenPoint(toCheck.transform.position);
            var position = origin.transform.position;
            //var rotation = origin.transform.rotation;
            //Is in front
            if (pointOnScreen.z < 0)
            {
                if (interestMeasureTable.ContainsKey(position))
                {
                    interestMeasureTable[position] += 0f;
                }
                else
                {
                    interestMeasureTable.Add(position, 0f);
                }
                //Debug.Log(origin.name +" is behind: " + toCheck.name + " at point " + position );
                return false;
            }

            //Is in FOV
            if ((pointOnScreen.x < 0) || (pointOnScreen.x > Screen.width) ||
                (pointOnScreen.y < 0) || (pointOnScreen.y > Screen.height))
            {
                //Debug.Log("OutOfBounds: " + toCheck.name + " at point " + position);
                if (interestMeasureTable.ContainsKey(position))
                {
                    interestMeasureTable[position] += 0f;
                }
                else
                {
                    interestMeasureTable.Add(position, 0f);
                }
                return false;
            }

            var heading = toCheck.transform.position - position;
            var direction = heading.normalized;// / heading.magnitude;
            GameObject entryToUse = null;
            if (!Physics.Linecast(position, toCheck.transform.position, out var hit))
            {
                //score += 1 / allObjects.Length;
                var seen = false;
                foreach(KeyValuePair<GameObject, int> entry in objectsSeen.ToList())
                {
                    for (var i = 0; i < allObjects.Length; i++)
                    {
                        if (entry.Key == allObjects[i])
                        {
                            seen = true;
                            objectsSeen[entry.Key] += 1;
                            entryToUse = entry.Key;
                        }
                    }

                }
                if (interestMeasureTable.ContainsKey(position))
                {
                    if (!seen | !hasMemory)
                    {
                        interestMeasureTable[position] += calculateInterestingness(toCheck);
                        if(hasMemory)
                            objectsSeen.Add(toCheck,1);
                    }
                    else
                    {
                            //interestMeasureTable[position] += scoreModifier * (1 / objectsSeen[entryToUse]) * calculateInterestingness(toCheck);
                            interestMeasureTable[position] += (scoreModifier * calculateInterestingness(toCheck))/objectsSeen[entryToUse];
                    }
                }
                else
                {
                    noOfObjectsSeen++;
                    if (usingNMax && noOfObjectsSeen > nMax)
                    {
                        nMax = noOfObjectsSeen;
                        //Debug.Log(nMax);
                    }
                    if (!seen | !hasMemory)
                    {
                        interestMeasureTable.Add(position, scoreModifier * calculateInterestingness(toCheck));
                        if(hasMemory)
                            objectsSeen.Add(toCheck,1);
                    }else
                        //interestMeasureTable.Add(position, scoreModifier * (1 / objectsSeen[entryToUse]) * calculateInterestingness(toCheck));
                        interestMeasureTable.Add(position, (scoreModifier * calculateInterestingness(toCheck))/objectsSeen[entryToUse]);
                    

                }


                return true;
            }
            if (hit.transform.name == toCheck.name) return true;
            //Debug.DrawLine(cam.transform.position, toCheck.transform.position, Color.red);
            if (interestMeasureTable.ContainsKey(position))
            {
                interestMeasureTable[position] += 0f;
            }
            else
            {
                interestMeasureTable.Add(position, 0f);
            }
            //Debug.Log(toCheck.name + " occluded by " + hit.transform.name + " at point " + position);
            return false;
        }
 
    
 public override float calculateInterestingness(GameObject gameObject)
 {
     if (gameObject.name.Contains("House") && hasWeighting & !usingNMax){
         if(typesSeen.Add("House"))
             return 10f * ((float)1 / allObjects.Length);
         return (10f * ((float)1 / allObjects.Length))/2;
     }
        
     if (gameObject.name.Contains("House") && hasWeighting & usingNMax){
         if(typesSeen.Add("House"))
             return 10f * ((float)1 / nMax);
         return (10f * ((float)1 / nMax))/2;

     }
     if (gameObject.name.Contains("Tree") && hasWeighting && !usingNMax){
         if(typesSeen.Add("Tree"))
         {
             var localScale = gameObject.transform.localScale;
             return (localScale.x + localScale.z + localScale.y) * ((float) 1 / allObjects.Length);
         }

         var scale = gameObject.transform.localScale;
         return (scale.x + scale.z + scale.y) * ((float) 1 / allObjects.Length)/2;

     }
     if( gameObject.name.Contains("Tree") && hasWeighting && usingNMax)
     {
         var localScale = gameObject.transform.localScale;
         return (localScale.x + localScale.z + localScale.y) * ((float) 1 / nMax);
     }

     if(usingNMax)
         return ((float)1 / nMax);


     return ((float)1 / allObjects.Length);
 }
 
 private static string GETPath(){
     return Application.dataPath +"/CSV/TrueRandom/"+"heatmaps_trueRandom_agent_" + SceneManager.GetActiveScene().name +".csv";
 }

    
}
