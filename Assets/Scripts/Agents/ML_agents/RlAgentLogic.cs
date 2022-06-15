using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class RlAgentLogic : Agent
{
    private Rigidbody rBody;

    public Transform target;

    public float speed;

    public float bestDist;
    
    // Start is called before the first frame update
    void Start()
    {
        //rBody = GetComponent<Rigidbody>();
        bestDist = Vector3.Distance(transform.position, target.position);
    }

    public override void OnEpisodeBegin()
    {
        transform.position = Vector3.zero;
        target.localPosition = new Vector3(Random.Range(0, 200), 0, Random.Range(0,200));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(target.localPosition);
        sensor.AddObservation(transform.localPosition);
       // sensor.AddObservation(rBody.velocity);
    }



    public override void OnActionReceived(float[] vectorAction)
    {
        float moveX = vectorAction[0];
        float moveZ = vectorAction[1];
        float moveY = vectorAction[2];

        transform.position += new Vector3(moveX, moveY, moveZ) * Time.deltaTime * speed;
        float curDist = Vector3.Distance(transform.position, target.position);
        if (Vector3.Distance(transform.position, target.position) < bestDist)
        {
            bestDist = curDist;
            SetReward(1.0f);
            EndEpisode();
        }

        if (transform.position.y < -1)
        {
            SetReward(-1f);
            EndEpisode();
        }

    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Vertical");
        actionsOut[1] = Input.GetAxis("Horizontal");
    }
}
