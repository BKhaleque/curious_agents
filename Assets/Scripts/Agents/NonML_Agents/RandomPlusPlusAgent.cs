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

public class RandomPlusPlusAgent : NonMLAgent
{
    public string[] weightingNames;

    private GameObject focusedObject;
    //private Dictionary<GameObject,int> objectsSeen;
    private GameObject[] allObjects;
    private Dictionary<Vector3, float> interestMeasureTable;
    public GameObject pathfinderAgent;
    public bool usingNMax;
    //public bool hasMemory;
    public bool hasWeighting;
    private int nMax;
    private int noOfObjectsSeen;

    void Start()
    {
        currentIters = 0;
        objectsSeen = new Dictionary<GameObject, int>();
        allObjects = FindObjectsOfType<GameObject>();
        focusedObject = allObjects[Random.Range(0, allObjects.Length)];// pick rando object for now
        interestMeasureTable = new Dictionary<Vector3, float>();
        nMax = 1;
        foreach (var t in allObjects)
        {
            IsInView(pathfinderAgent,t);
        }
        //FindPathToObject();
    }

    void FindPathToObject()
    {

            var interestMeasure = 0f;
           // var ultimateTarget =  focusedObject.gameObject.transform.rotation * new Vector3(gameObject.transform.position.x + xStepSize,gameObject.transform.position.y,gameObject.transform.position.z + zStepSize) ;
           // var p = new NavMeshPath();
            var rotTable = new Dictionary<Quaternion, float>();
            //Debug.Log("I'm here");

            //if (!navMeshAgent.CalculatePath(ultimateTarget, p) || p.status != NavMeshPathStatus.PathComplete)
            //{
            //    focusedObject = allObjects[Random.Range(0, allObjects.Length)];
          //  }
            while (Vector3.Distance(gameObject.transform.position, focusedObject.gameObject.transform.position) > 8f)
            {
                //move towards object
                rotTable = new Dictionary<Quaternion, float>();
                transform.position = Vector3.MoveTowards(transform.position,
                    focusedObject.gameObject.transform.position, xStepSize);
                    //Debug.Log("I'm here");
                for (var j = 0; j < 4; j++)
                {
                    noOfObjectsSeen = 0;
                    interestMeasure += allObjects.Where(t => IsInView(pathfinderAgent, t)).Sum(t =>
                        // ReSharper disable once PossibleLossOfFraction
                        objectsSeen.Where(kv => kv.Key == t).Sum(kv => (1 / kv.Value) * CalculateInterestingness(t)));
                    rotTable.Add(cam.transform.rotation,interestMeasure);
                    cam.transform.Rotate(0f,90f,0f);

                }
            }

            focusedObject = allObjects[Random.Range(0, allObjects.Length)];

            if(currentIters == steps){
              var filePath = GETPath();
              var writer = File.CreateText(filePath);
              writer.WriteLine("Coord;Interestingness");
              foreach (var kv in interestMeasureTable)
              {
                  writer.WriteLine("{0};{1}", kv.Key, kv.Value);
              }
            }

    }

     public override bool IsInView(GameObject origin, GameObject toCheck)
        {
            var pointOnScreen = cam.WorldToScreenPoint(toCheck.transform.position);
            var position = origin.transform.position;
            var rotation = origin.transform.rotation;
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
                    if (!seen)
                    {
                        interestMeasureTable[position] += CalculateInterestingness(toCheck);
                        objectsSeen.Add(toCheck,1);
                    }
                    else
                    {
                            //interestMeasureTable[position] += scoreModifier * (1 / objectsSeen[entryToUse]) * calculateInterestingness(toCheck);
                            interestMeasureTable[position] += (scoreModifier * CalculateInterestingness(toCheck))/objectsSeen[entryToUse];
                    }
                }
                else
                {
                    if (!seen)
                    {
                        objectsSeen.Add(toCheck,1);
                        interestMeasureTable.Add(position, scoreModifier * CalculateInterestingness(toCheck));
                    }else
                    //interestMeasureTable.Add(position, scoreModifier * (1 / objectsSeen[entryToUse]) * calculateInterestingness(toCheck));
                        interestMeasureTable.Add(position, (scoreModifier * CalculateInterestingness(toCheck))/objectsSeen[entryToUse]);
                }
                
                noOfObjectsSeen++;
                if (usingNMax && noOfObjectsSeen > nMax)
                {
                    nMax = noOfObjectsSeen;
                    //Debug.Log(nMax);
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

     public override float CalculateInterestingness(GameObject gameObject)
     {
         if (hasWeighting & !usingNMax)
         {

             if (weightingNames.Any(t => gameObject.name.Contains(t)))
                 return (10f * ((float)1 / allObjects.Length))/2;
            
             return 10f * ((float)1 / allObjects.Length);

            
         }
        
         if (hasWeighting & usingNMax)
         {
             if(weightingNames.Any(t => gameObject.name.Contains(t)))
                 return (10f * ((float)1 / nMax))/2;
            
             return 10f * ((float)1 / nMax);
         }

         if(usingNMax)
             return ((float)1 / nMax);


         return ((float)1 / allObjects.Length);
     }

    void Update(){
      if(currentIters < steps){
        FindPathToObject();
        currentIters++;
      }else{
        gameObject.SetActive(false);
      }
    }


    private static string GETPath(){
        return Application.dataPath +"/CSV/Pathfinder/"+"random_plus_plus_eval" + SceneManager.GetActiveScene().name +".csv";
    }

}
