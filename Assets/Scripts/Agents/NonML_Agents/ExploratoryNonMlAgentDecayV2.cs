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

public class ExploratoryNonMlAgentDecayV2 : NonMLAgent
{
    public GameObject exploratoryAgent;
    public GameObject player;
    public bool hasMemory;
    public bool hasWeighting;
    
    private Dictionary<GameObject,int> objectsSeen;
    private Dictionary<Vector3, float> interestMeasureTable;
    private HashSet <String>typesSeen;
    private GameObject[] allObjects;
    int maxIters;
    NavMeshPath p;

    private void Awake()
    {
        //maxIters = steps;
        currentIters = 0;
        objectsSeen = new Dictionary<GameObject, int>();
        interestMeasureTable = new Dictionary<Vector3, float>();
        allObjects = FindObjectsOfType<GameObject>();
        typesSeen = new HashSet<String>();
        //player = GameObject.Find("Player");
        player.gameObject.SetActive(false);
        exploratoryAgent.transform.position = Vector3.zero;
    }

    private void Explore()
    {
          p = new NavMeshPath();

          var interestMeasure = 0f;
          var travelled = false;
          var rotTable = new Dictionary<Quaternion, float>();

          for (var j = 0; j < 4; j++)
          {
                interestMeasure += allObjects.Where(t => IsInView(exploratoryAgent, t)).Sum(t =>
                    // ReSharper disable once PossibleLossOfFraction
                    objectsSeen.Where(kv => kv.Key == t).Sum(kv => (1 / kv.Value) * calculateInterestingness(t)));
               rotTable.Add(exploratoryAgent.transform.rotation,interestMeasure);
               exploratoryAgent.transform.Rotate(0f,90f,0f);

          }

            var allMeasuresSame = new bool[4];
            var prevMeasure = 0f;
            var m = 0;
            var bestRot = gameObject.transform.rotation;
            foreach (var kv in rotTable)
            {
                var highestInterest = kv.Value;
                if (kv.Value > highestInterest)
                    bestRot = kv.Key;
                if (Math.Abs(prevMeasure - kv.Value) < 0.01f)
                    allMeasuresSame[m] = true;
                else
                    allMeasuresSame[m] = false;

                prevMeasure = kv.Value;
                exploratoryAgent.transform.rotation = bestRot;
                Debug.Log(bestRot);

            }

            var allTrue = true;
            for (var j = 1; j < allMeasuresSame.Length; j++)
            {
                if (allMeasuresSame[j] != allMeasuresSame[j - 1])
                    allTrue = false;
            }
            var position = gameObject.transform.position;
            var target =  gameObject.transform.rotation * new Vector3(position.x + xStepSize,position.y,position.z + zStepSize) ;

            if (navMeshAgent.CalculatePath(target, p) && p.status == NavMeshPathStatus.PathComplete && !allTrue)
            {
                //move to target
                navMeshAgent.SetPath(p);
                gameObject.transform.position = target;
                //Debug.Log("I can get here at point (" + exploratoryAgent.gameObject.transform.position.x + stepSize + ", " + exploratoryAgent.gameObject.transform.position.z + stepSize + ") from origin with step size " + stepSize);

            }
            else
            {
                //Debug.Log("I can't get here at point (" + exploratoryAgent.gameObject.transform.position.x + xStepSize + ", " + exploratoryAgent.gameObject.transform.position.z + zStepSize + ") ");
                gameObject.transform.position = RandomNavmeshLocation(10f);
                //do nothing
            }
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

        void Update(){
          if(currentIters <= steps){
            Explore();
            StartCoroutine(Wait());
            currentIters++;

          }else{
            gameObject.SetActive(false);
            player.gameObject.SetActive(true);
          }

        }

      IEnumerator Wait(){
                yield return new WaitForSecondsRealtime(2f);
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

     private float calculateInterestingness(GameObject gameObject)
    {
        if (gameObject.name.Contains("House") && hasWeighting){
          if(typesSeen.Add("House"))
            return 10f * ((float)1 / allObjects.Length);
          else
            return (10f * ((float)1 / allObjects.Length))/2;

        }
        if (gameObject.name.Contains("Tree") && hasWeighting){
            if(typesSeen.Add("Tree"))
                return (gameObject.transform.localScale.x + gameObject.transform.localScale.z + gameObject.transform.localScale.y) * ((float) 1 / allObjects.Length);
            else
                return (gameObject.transform.localScale.x + gameObject.transform.localScale.z + gameObject.transform.localScale.y) * ((float) 1 / allObjects.Length)/2;

        }
        return ((float)1 / allObjects.Length);
    }

    private static string GETPath(){
#if UNITY_EDITOR
        return Application.dataPath +"/CSV/Exploratory/"+"heatmaps_decay_agent_" + SceneManager.GetActiveScene().name +".csv";
#elif UNITY_ANDROID
        return Application.persistentDataPath+"Saved_data.csv";
#elif UNITY_IPHONE
        return Application.persistentDataPath+"/"+"Saved_data.csv";
#else
        return Application.dataPath +"/"+"Saved_data.csv";
#endif
    }
}