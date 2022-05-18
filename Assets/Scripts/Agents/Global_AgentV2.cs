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
    public class Global_AgentV2 : Agent
    {
        // private MeshFilter[] meshes;
        private Mesh meshToRender;
        private NavMeshSurface surface;
        //public NavMeshAgent navMeshAgent;
        public GameObject globalAgent;
      //  public GameObject landmarkSpawnerObj;
      //  public GameObject treeSpawnerObj;
      //  public GameObject smallAssetSpawnerObj;
      //  public GameObject exploratoryAgent;
        public int totalXSize;
        public int totalZSize;
        //public float scoreModifier;
        //public bool generate;


        private float curMaxY = float.MinValue;
        private float curMinY = float.MaxValue;

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
            gameObject.transform.position = Vector3.zero;
            interestMeasureTable = new Dictionary<Vector3, float>();
             //reachables = GetNumberOfReachables(gameObject.transform.x, gameObject.transform.z, xStepSize, zStepSize, xSize,zSize);
             //CalculateNavMesh(totalXSize, totalZSize);

        }

        void Update()
        {
          if(i<=totalXSize){
            for(float z = 0; z<totalZSize; z+=zStepSize){
              Vector3 target = new Vector3(i,1,z);
              if(SetDestination(target)){
                Debug.Log("Viable position");

                gameObject.transform.position = target;
                foreach (var t in allObjects)
                {
                    if (t.name == "Generator" && t.name == "GlobalAgent") continue;
                    for (var i = 0; i < 4; i++)
                    {
                      if(i !=3){
                        IsInView(globalAgent, t);
                      }
                      cam.transform.Rotate(0f,90f,0f);
                    }
                }
              }else{
                Debug.Log("No Viable position");
                continue;
              }
            }
            i+=xStepSize;
            if(i==totalXSize){
              var filePath = GETPath();

              var writer = File.CreateText(filePath);
              writer.WriteLine("X;Z;Interestingness;");
              foreach (var kv in interestMeasureTable)
              {
                   Debug.Log(kv.Key);
                   Debug.Log(kv.Value);

                   writer.WriteLine("{0};{1};{2};", kv.Key.x, kv.Key.z, kv.Value);
              }
              gameObject.SetActive(false);
            }
          }

        }


        private static string GETPath(){
#if UNITY_EDITOR
            return Application.dataPath +"/CSV/Global/"+ "GlobalAgent_heatmaps" + SceneManager.GetActiveScene().name + ".csv";
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
                Debug.Log(origin.name +" is behind: " + toCheck.name + " at point " + position );
                return false;
            }

            //Is in FOV
            if ((pointOnScreen.x < 0) || (pointOnScreen.x > Screen.width) ||
                (pointOnScreen.y < 0) || (pointOnScreen.y > Screen.height))
            {
                Debug.Log("OutOfBounds: " + toCheck.name + " at point " + position);
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
                    interestMeasureTable[position] += scoreModifier* calculateInterestingness(toCheck);
                        //interestMeasureTable[position].Add(rotation,calculateInterestingness(toCheck));
                }
                else
                {
                    interestMeasureTable.Add(position,scoreModifier* calculateInterestingness(toCheck));
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
            Debug.Log(toCheck.name + " occluded by " + hit.transform.name + " at point " + position);
            return false;
        }

        private float calculateInterestingness(GameObject gameObject)
        {
            if (gameObject.name.Contains("House"))
                return 10f * ((float)1 / allObjects.Length);
            if (gameObject.name.Contains("Tree"))
                return gameObject.transform.localScale.x * ((float) 1 / allObjects.Length);
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

    }
}
