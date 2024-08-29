using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;


// Ball movement controls and ML-Agents integration
public class RollerBallAgent : Agent
{
    public GameObject Target;
    public GameObject ViewCamera = null;
    public AudioClip JumpSound = null;
    public AudioClip HitSound = null;
    public AudioClip CoinSound = null;

    private Rigidbody mRigidBody;
    private AudioSource mAudioSource;
    private bool mFloorTouched = false;

    public override void Initialize()
    {
        mRigidBody = GetComponent<Rigidbody>();
        mAudioSource = GetComponent<AudioSource>();
    }

    public override void OnEpisodeBegin()
    {
      
        if (this.transform.localPosition.y < 0)
        {
            this.transform.localPosition = new Vector3(0, 0.5f, 0);
            this.mRigidBody.velocity = Vector3.zero;
            this.mRigidBody.angularVelocity = Vector3.zero;
        }

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        
        sensor.AddObservation(this.transform.localPosition);

        
        sensor.AddObservation(Target.transform.localPosition);

        
        sensor.AddObservation(mRigidBody.velocity);

        
        sensor.AddObservation(mFloorTouched);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];

        mRigidBody.AddForce(controlSignal * 10);

      
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.transform.localPosition);


        if (distanceToTarget < 1.42f)
        {
            SetReward(1.0f);
            EndEpisode();
        }

       
        if (this.transform.localPosition.y < 0)
        {
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    void FixedUpdate()
    {
        HandleCamera();
    }

    private void HandleCamera()
    {
        if (ViewCamera == null) return;

        Vector3 direction = (Vector3.up * 2 + Vector3.back) * 2;
        RaycastHit hit;
        Debug.DrawLine(transform.position, transform.position + direction, Color.red);

        if (Physics.Linecast(transform.position, transform.position + direction, out hit))
        {
            ViewCamera.transform.position = hit.point;
        }
        else
        {
            ViewCamera.transform.position = transform.position + direction;
        }

        ViewCamera.transform.LookAt(transform.position);
    }

    void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.CompareTag("Floor"))
        {
            mFloorTouched = true;
            PlayHitSound(coll);
        }
        else
        {
            PlayHitSound(coll);
        }
    }

    void OnCollisionExit(Collision coll)
    {
        if (coll.gameObject.CompareTag("Floor"))
        {
            mFloorTouched = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Coin"))
        {
            if (mAudioSource != null && CoinSound != null)
            {
                mAudioSource.PlayOneShot(CoinSound);
            }
            Destroy(other.gameObject);
        }
    }

    private void PlayHitSound(Collision coll)
    {
        if (mAudioSource != null && HitSound != null && coll.relativeVelocity.magnitude > 2f)
        {
            mAudioSource.PlayOneShot(HitSound, coll.relativeVelocity.magnitude);
        }
    }
}
