using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class RlAgentLogic : Agent
{
    private Rigidbody rBody;

    private Vector3 startPosition;

    public Transform target;

    public float speed;

    public float bestDist;
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        //rBody = GetComponent<Rigidbody>();
        startPosition = transform.position;
        bestDist = Vector3.Distance(transform.position, target.position);
    }

    public override void OnEpisodeBegin()
    {
        transform.position = startPosition;
       // target.localPosition = new Vector3(Random.Range(0, 200), 0, Random.Range(0,200));
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
        if ((curDist < bestDist) && bestDist > 1f)
        {
            bestDist = curDist;
            SetReward(0.1f);
            //EndEpisode();
        }

        else if(bestDist <=1f)
        {
            SetReward(1f);
            EndEpisode();

        }
        // else if(curDist > bestDist)
        // {
        //     SetReward(-0.01f);
        //     //EndEpisode();
        //
        // }

        if (Mathf.Abs(startPosition.y - transform.position.y) < -5f || Mathf.Abs(startPosition.y - transform.position.y) > 10f)
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
