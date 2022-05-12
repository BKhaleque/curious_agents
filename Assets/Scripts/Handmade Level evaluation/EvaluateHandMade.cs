using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

public class EvaluateHandMade : MonoBehaviour
{
    // Start is called before the first frame update
    public NavMeshData navMeshData;
    public NavMeshAgent navMeshAgent;
    public GameObject validatorAgent;
    public GameObject terrain;
    public float walkRadius;
    private Dictionary<Vector3,float> interestMeasureTable;
    private float curMaxY = float.MinValue;
    private float curMinY = float.MaxValue;
    private GameObject[] allObjects;
    

    public Camera cam;

    private void Awake()
    {
        interestMeasureTable = new Dictionary<Vector3,float>();
        allObjects = FindObjectsOfType<GameObject>() ;
        CheckStuff();
    }

    // Update is called once per frame

    void CheckStuff()
    {
        for (var i = 0; i < 300; i++)
        {
            if (!navMeshAgent.SetDestination(RandomNavmeshLocation(walkRadius))) continue;
            validatorAgent.transform.position = RandomNavmeshLocation(walkRadius);
            foreach (var t in allObjects)
            {
                for (var j = 0; j < 3; j++)
                {
                    IsInView(validatorAgent, t);
                    validatorAgent.transform.Rotate(0.0f,90.0f,0.0f);
                }
            }
        }
        var filePath = GETPath();

        var writer = File.CreateText(filePath);
        writer.WriteLine("X;Z;Interestingness;");
        foreach (var kv in interestMeasureTable)
        {
            //print(kv.Key + " key ");
            
                writer.WriteLine("{0};{1};{2}", kv.Key.x, kv.Key.z, kv.Value);
            

        }
    }

    private static string GETPath(){
#if UNITY_EDITOR
        return Application.dataPath +"/CSV/"+"heatmaps_handmade.csv";
#elif UNITY_ANDROID
        return Application.persistentDataPath+"Saved_data.csv";
#elif UNITY_IPHONE
        return Application.persistentDataPath+"/"+"Saved_data.csv";
#else
        return Application.dataPath +"/"+"Saved_data.csv";
#endif
    }
     private bool IsInView(GameObject origin, GameObject toCheck)
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
               // score += 1 / allObjects.Length;
                if (interestMeasureTable.ContainsKey(position))
                {
          
                        interestMeasureTable[position] += CalculateInterestingness(toCheck);

                        //interestMeasureTable[position].Add(rotation,calculateInterestingness(toCheck));
                    
                    
                }
                else
                {
                    interestMeasureTable.Add(position,CalculateInterestingness(toCheck));
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

     private static float CalculateInterestingness(GameObject gameObject)
     {
         const float interestMeasure = 0.001f;
         if (gameObject.name.Contains("House"))
             return 10f * interestMeasure;
         if (gameObject.name.Contains("Tree"))
             return gameObject.transform.localScale.x * interestMeasure;
         return interestMeasure;

     }
    
    public Vector3 RandomNavmeshLocation(float radius) {
        var randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;
        var finalPosition = Vector3.zero;
        if (NavMesh.SamplePosition(randomDirection, out var hit, radius, 1)) {
            finalPosition = hit.position;            
        }
        return finalPosition;
    }
}
