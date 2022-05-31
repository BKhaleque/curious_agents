using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class PathfinderAgent : Agent
{
    private GameObject focusedObject;
    //private Dictionary<GameObject,int> objectsSeen;
    private GameObject[] allObjects;
    private Dictionary<Vector3, float> interestMeasureTable;
    public GameObject pathfinderAgent;


    void Start()
    {
        currentIters = 0;
        objectsSeen = new Dictionary<GameObject, int>();
        allObjects = FindObjectsOfType<GameObject>();
        focusedObject = allObjects[Random.Range(0, allObjects.Length)];// pick rando object for now
        interestMeasureTable = new Dictionary<Vector3, float>();
        //FindPathToObject();
    }

    void FindPathToObject()
    {

            var interestMeasure = 0f;

            var ultimateTarget =  focusedObject.gameObject.transform.rotation * new Vector3(gameObject.transform.position.x + xStepSize,gameObject.transform.position.y,gameObject.transform.position.z + zStepSize) ;
            var p = new NavMeshPath();
            var rotTable = new Dictionary<Quaternion, float>();
            //Debug.Log("I'm here");

            //if (!navMeshAgent.CalculatePath(ultimateTarget, p) || p.status != NavMeshPathStatus.PathComplete)
            //{
            //    focusedObject = allObjects[Random.Range(0, allObjects.Length)];
          //  }
            while (Vector3.Distance(gameObject.transform.position, focusedObject.gameObject.transform.position) > 3f)
            {
                //move towards object
                rotTable = new Dictionary<Quaternion, float>();
                transform.position = Vector3.MoveTowards(transform.position,
                    focusedObject.gameObject.transform.position, xStepSize);
                    //Debug.Log("I'm here");
                for (var j = 0; j < 4; j++)
                {
                    interestMeasure += allObjects.Where(t => IsInView(pathfinderAgent, t)).Sum(t =>
                        // ReSharper disable once PossibleLossOfFraction
                        objectsSeen.Where(kv => kv.Key == t).Sum(kv => (1 / kv.Value) * calculateInterestingness(t)));
                    rotTable.Add(pathfinderAgent.transform.rotation,interestMeasure);
                    pathfinderAgent.transform.Rotate(0f,90f,0f);

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
                        interestMeasureTable[position] += calculateInterestingness(toCheck);
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
                    if (!seen)
                    {
                        objectsSeen.Add(toCheck,1);
                        interestMeasureTable.Add(position, scoreModifier * calculateInterestingness(toCheck));
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

    private float calculateInterestingness(GameObject gameObject)
    {
        if (gameObject.name.Contains("House"))
            return 10f * ((float)1 / allObjects.Length);
        if (gameObject.name.Contains("Tree"))
            return (gameObject.transform.localScale.x + gameObject.transform.localScale.z + gameObject.transform.localScale.y) * ((float) 1 / allObjects.Length);
        return ((float)1 / allObjects.Length);
    }

    void Update(){
      if(currentIters <= steps){
        FindPathToObject();
        currentIters++;
      }else{
        gameObject.SetActive(false);
      }
    }


    private static string GETPath(){
#if UNITY_EDITOR
        return Application.dataPath +"/CSV/Pathfinder/"+"heatmaps_pathfinder_agent_" + SceneManager.GetActiveScene().name +".csv";
#elif UNITY_ANDROID
        return Application.persistentDataPath+"Saved_data.csv";
#elif UNITY_IPHONE
        return Application.persistentDataPath+"/"+"Saved_data.csv";
#else
        return Application.dataPath +"/"+"Saved_data.csv";
#endif
    }

}
