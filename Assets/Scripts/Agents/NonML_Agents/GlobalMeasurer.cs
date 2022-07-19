using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;

namespace Agents
{
    [RequireComponent(typeof(MeshFilter))]
    public class GlobalMeasurer : NonMLAgent
    {
        // private MeshFilter[] meshes;
        private Mesh meshToRender;
        private NavMeshSurface surface;

        private GameObject terrain;
        //public NavMeshAgent navMeshAgent;
        public GameObject globalAgent;
        public bool hasWeighting;
        public bool usingNMax;
        public int totalXSize;
        public int totalZSize;
        public string[] weightingNames;
        

        
        //public float scoreModifier;
        //public bool generate;


        private float curMaxY = float.MinValue;
        private float curMinY = float.MaxValue;
        private int noOfObjectsSeen;
        private int nMax;
        private HashSet <String>typesSeen;

        //private AssetAreaSpawner smallAssetSpawner;

        private GameObject[] allObjects;

        //private TestPS forestSpawner;
        //private AssetAreaSpawner landmarkSpawner;




        private Vector3[] vertices;

        private Dictionary<Vector3,float> interestMeasureTable;

        private float i;

        //private List<Vector3> pathForExplorationAgentToFollow;
        //private float interestMeasure;

        private void Start()
        {
            allObjects = FindObjectsOfType<GameObject>();
            terrain = GameObject.FindGameObjectWithTag("terrain");
            totalXSize = (int) terrain.GetComponent<Renderer>().bounds.size.x;
            i = -totalXSize;
            typesSeen = new HashSet<string>();
            //Debug.Log(totalXSize);
            totalZSize = (int) terrain.GetComponent<Renderer>().bounds.size.z;
            //Debug.Log(totalZSize);
            noOfObjectsSeen = 0;
            nMax = 1;
            CheckHowManyObjectsSeen();
            //gameObject.transform.position = Vector3.zero;
            interestMeasureTable = new Dictionary<Vector3, float>();
             //reachables = GetNumberOfReachables(gameObject.transform.x, gameObject.transform.z, xStepSize, zStepSize, xSize,zSize);
             //CalculateNavMesh(totalXSize, totalZSize);
            
        }

        void Update()
        {
            if (!(i <= totalXSize)) return;
             for (int z = -totalZSize; z < totalZSize; z++)
             {
                 Vector3 target = new Vector3(i, Random.Range(-terrain.GetComponent<Renderer>().bounds.size.y, terrain.GetComponent<Renderer>().bounds.size.y), z);
                 if (SetDestination(target))
                 {
                     Debug.Log("Viable position");
            
                     gameObject.transform.position = target;

                 }
                 else
                 {
                     Debug.Log("No Viable position");
                     
                     gameObject.transform.position = RandomNavmeshLocation(10f);
                     //break;
                 }
                 
                 foreach (var t in allObjects)
                 {
                     // if (t.name == "Generator" && t.name == "GlobalAgent") continue;
                     for (var i = 0; i < 4; i++)
                     {
                         CheckHowManyObjectsSeen();
                         if (i != 3)
                         {
                             if (t.name != "Generator" || t.name != "GlobalAgent")
                                 IsInView(globalAgent, t);
                         }
            
                         //Debug.Log(i);
                         cam.transform.Rotate(0f, 90f, 0f);
                         gameObject.transform.Rotate(0, 90f, 0);
                     }
                 }
             }

            // while (interestMeasureTable.ContainsKey(gameObject.transform.position))
            // {
            //     Vector3 target = new Vector3(Random.Range(-totalXSize, totalXSize),
            //         Random.Range(-terrain.GetComponent<Renderer>().bounds.size.y,
            //             terrain.GetComponent<Renderer>().bounds.size.y), Random.Range(-totalZSize, totalZSize));
            //     if (SetDestination(target))
            //     {
            //         gameObject.transform.position = target;
            //     }
            //     else
            //     {
            //         gameObject.transform.position = RandomNavmeshLocation(2000f);
            //
            //     }
            // }
            // foreach (var t in allObjects)
            // {
            //     // if (t.name == "Generator" && t.name == "GlobalAgent") continue;
            //     for (var l = 0; l < 4; l++)
            //     {
            //         if(l !=3){
            //             if (t.name != "GlobalAgent")
            //                 IsInView(globalAgent, t);
            //         }
            //         //Debug.Log(i);
            //         cam.transform.Rotate(0f,90f,0f);
            //         gameObject.transform.Rotate(0,90f,0);
            //     }
            // }
                    
 
            i+=xStepSize;
            if (i < totalXSize) return;
            var filePath = GETPath();

            var writer = File.CreateText(filePath);
            writer.WriteLine("X;Z;Interestingness;");
            foreach (var kv in interestMeasureTable)
            {
                // Debug.Log(kv.Key);
                // Debug.Log(kv.Value);
                writer.WriteLine("{0};{1};{2};", kv.Key.x, kv.Key.z, kv.Value);
            }
            gameObject.SetActive(false);

        }


        private static string GETPath(){
#if UNITY_EDITOR
            return Application.dataPath +"/CSV/Global/"+ "GlobalMeasurer_heatmaps" + SceneManager.GetActiveScene().name + ".csv";
#endif
        }



        public  override bool IsInView(GameObject origin, GameObject toCheck)
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
                    //interestMeasureTable[position].Add(rotation,0f);
                }
                else
                {
                    interestMeasureTable.Add(position,0f);
                   // interestMeasureTable[position].Add(rotation,0f);
                }
                // Debug.Log(origin.name +" is behind: " + toCheck.name + " at point " + position );
                return false;
            }

            //Is in FOV
            if ((pointOnScreen.x < 0) || (pointOnScreen.x > Screen.width) ||
                (pointOnScreen.y < 0) || (pointOnScreen.y > Screen.height))
            {
              //  Debug.Log("OutOfBounds: " + toCheck.name + " at point " + position);
                if (interestMeasureTable.ContainsKey(position))
                {
                        interestMeasureTable[position]+= 0f;
                       // interestMeasureTable[position].Add(rotation,0f);
                }
                else
                {
                    interestMeasureTable.Add(position,0f);
                   // interestMeasureTable[position].Add(rotation,0f);
                }
                return false;
            }

            var heading = toCheck.transform.position - position;
           // var direction = heading.normalized;// / heading.magnitude;

            if (!Physics.Linecast(position, toCheck.transform.position, out var hit))
            {
                //score += 1 / allObjects.Length;
                if (interestMeasureTable.ContainsKey(position))
                {
                    interestMeasureTable[position] += scoreModifier* CalculateInterestingness(toCheck);
                        //interestMeasureTable[position].Add(rotation,calculateInterestingness(toCheck));
                }
                else
                {
                    interestMeasureTable.Add(position,scoreModifier* CalculateInterestingness(toCheck));
                    //interestMeasureTable[position].Add(rotation,calculateInterestingness(toCheck));
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
                interestMeasureTable.Add(position,0f);
            }
            //  Debug.Log(toCheck.name + " occluded by " + hit.transform.name + " at point " + position);
            return false;
        }

        public override float CalculateInterestingness(GameObject gameObject)
        {
            //var localScale = gameObject.transform.localScale;
            if (hasWeighting & !usingNMax)
            {
                if (weightingNames.Any(t => gameObject.name.Contains(t) && typesSeen.Add(t)))
            
                    return 10f * ((float)1 / allObjects.Length);
            
                if (weightingNames.Any(t => gameObject.name.Contains(t)))
                    return (10f * ((float)1 / allObjects.Length))/2;
            
                return 10f * ((float)1 / allObjects.Length);

            
            }
        
            if (hasWeighting & usingNMax)
            {
                if (weightingNames.Any(t => gameObject.name.Contains(t) && typesSeen.Add(t)))
                {
                    return 10f * ((float)1 / nMax);
                }
                if(weightingNames.Any(t => gameObject.name.Contains(t)))
                    return (10f * ((float)1 / nMax))/2;
            
                return 10f * ((float)1 / nMax);
            }
            if(usingNMax)
                return ((float)1 / nMax);
        
            return ((float)1 / allObjects.Length);
        }


        private bool SetDestination(Vector3 targetDestination)
        {
          NavMeshHit hit;
          if (NavMesh.SamplePosition(targetDestination, out hit, 1f, NavMesh.AllAreas))
          {
            navMeshAgent.SetDestination(hit.position);
            return true;
          }
          return false;
        }
        
        private Vector3 RandomNavmeshLocation(float radius) {
            Vector3 randomDirection = Random.insideUnitSphere * radius;
            randomDirection += gameObject.transform.position;
            NavMeshHit hit;
            Vector3 finalPosition = Vector3.zero;
            if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1)) {
                finalPosition = hit.position;
            }
            return finalPosition;
        }
        
        
        
        private void CheckHowManyObjectsSeen()
        {

            foreach (var obj in allObjects)
            {
                var pointOnScreen = cam.WorldToScreenPoint(obj.transform.position);
            
                //Is in FOV
                if ((pointOnScreen.x < 0) || (pointOnScreen.x > Screen.width) ||
                    (pointOnScreen.y < 0) || (pointOnScreen.y > Screen.height))
                {
                    // if (!Physics.Linecast(gameObject.transform.position, obj.transform.position, out var hit))
                    // {
                    noOfObjectsSeen++;
                    if (noOfObjectsSeen > nMax)
                    {
                        nMax = noOfObjectsSeen;
                        //Debug.Log(nMax);
                    }
                    //}
                }
            }
        }
        

    }
}
