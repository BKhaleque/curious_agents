using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Agents
{
    public abstract class  NonMLAgent : MonoBehaviour
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

        [HideInInspector]
        public int currentIters;


        public abstract bool IsInView(GameObject origin, GameObject toCheck);

    }
}
