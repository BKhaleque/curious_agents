using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class RlAgentLogic : Agent
{
    private Rigidbody rBody;
    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        
    }
}
