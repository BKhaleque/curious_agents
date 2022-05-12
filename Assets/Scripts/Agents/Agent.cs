using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class  Agent : MonoBehaviour
{
    //public GameObject exploratoryAgent;
    public Camera cam;
    // public int xSize;
    // public int zSize;
    public NavMeshAgent navMeshAgent;
    public int steps;
    public float xStepSize;
    public float zStepSize;
    public float scoreModifier;
    public Dictionary<GameObject,int> objectsSeen;


}
