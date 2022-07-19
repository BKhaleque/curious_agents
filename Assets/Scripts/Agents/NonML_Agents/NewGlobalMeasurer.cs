using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Agents;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class NewGlobalMeasurer : NonMLAgent
{
    
    private float curMaxY = float.MinValue;
    private float curMinY = float.MaxValue;
    private GameObject terrain;
    public GameObject validatorAgent;
    //public NavMeshAgent navMeshAgent;
    private NavMeshSurface surface;
    private GameObject[] allObjects;
    private Dictionary<Vector3,float> interestMeasureTable;
    public string[] weightingNames;
    private HashSet <String>typesSeen;
    private int nMax;
    private int noOfObjectsSeen;

    public bool hasWeighting;
    public bool usingNMax;
    // Start is called before the first frame update
    void Start()
    {
        terrain = GameObject.FindGameObjectWithTag("terrain");
        allObjects = FindObjectsOfType<GameObject>();
        curMaxY = terrain.GetComponent<Renderer>().bounds.size.y;
        interestMeasureTable = new Dictionary<Vector3, float>();
        typesSeen = new HashSet<string>();
        nMax = 1;
        CalculateNavMesh((int)terrain.GetComponent<Renderer>().bounds.size.x, (int)terrain.GetComponent<Renderer>().bounds.size.z);
        var filePath = GETPath();
        var writer = File.CreateText(filePath);
        writer.WriteLine("X;Z;Interestingness;");
        foreach (var kv in interestMeasureTable)
        {
           // Debug.Log(kv.Key);
            //Debug.Log(kv.Value);
            writer.WriteLine("{0};{1};{2};", kv.Key.x, kv.Key.z, kv.Value);
        }
        //gameObject.SetActive(false);
    }
    
    private static string GETPath(){
#if UNITY_EDITOR
        return Application.dataPath +"/CSV/Global/"+ "GlobalMeasurerNew_heatmaps" + SceneManager.GetActiveScene().name + ".csv";
#endif
    }



    private float CalculateNavMesh( int xSize, int zSize, float validationNavProportion = 0.03f)
    {
            
        var zStep = zSize * validationNavProportion;
        var xStep = xSize * validationNavProportion;
        var xMax = xSize - xStep;
        var zMax = zSize - zStep;

        var totalReached = 0f;
        var totalPoints = 0f;

        //look in a grid fron (bottom left + validationNavProportion) to top (right - validationNavProportion)
        for (var z = zStep; z <= zMax; z += zStep)
        {
            for (var x = xStep; x <= xMax; x += xStep)
            {
                totalReached += GetNumberOfReachables(x, z, xStep, zStep, xMax, zMax);
                if (x+xStep <= xMax)
                {
                    totalPoints++;
                }
                if (z+zStep <= zMax)
                {
                    totalPoints++;
                }
            }
        }
        return totalReached/totalPoints;
    }
    
    private float GetNumberOfReachables(float x, float z, float xStep, float zStep, float xMax, float zMax)
    {
        //used for calculating paths, can be ignored
        var p = new NavMeshPath();

        float canReach = 0;
        //calculate current y
        RaycastHit hit;
        var origin = new Vector3(x, curMaxY + 1, z);
        var ray = new Ray(origin, Vector3.down);
        //shoot a raycast down to get the mesh location at this coordinate
        if (!Physics.Raycast(ray, out hit)) return canReach;
        var y = hit.point.y;
        if (z + zStep <= zMax)
        {

            if (CanAgentReach(x, y, z, x, z + zStep, p))
            {
                canReach++;
            }
        }

        if (!(x + xStep <= xMax)) return canReach;
        if (CanAgentReach(x, y, z, x + xStep, z, p))
        {
            canReach++;
        }

        return canReach;
    }
    
    private bool CanAgentReach(float fromX, float fromY, float fromZ, float toX, float toZ, NavMeshPath p)
    {
        var origin = new Vector3(toX, curMaxY + 1, toZ);
        var ray = new Ray(origin, Vector3.down);
        if (!Physics.Raycast(ray, out var hit)) return false;
        navMeshAgent.transform.position = new Vector3(fromX, fromY, fromZ);
        foreach (var t in allObjects)
        {
            for (var i = 0; i < 4; i++)
            {
                Debug.Log(validatorAgent.transform.position);
                if (i != 3)
                {
                    CheckHowManyObjectsSeen();
                    IsInView(validatorAgent, t);
                }


                cam.transform.Rotate(0.0f,90.0f,0.0f);
                gameObject.transform.Rotate(0, 90f, 0);

            }
        }
        return navMeshAgent.CalculatePath(hit.point, p);
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
    // Update is called once per frame
    void Update()
    {
        
    }
}
