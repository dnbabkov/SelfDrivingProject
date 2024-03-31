using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MoveToGoalAgent : Agent {

    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform obstacleParent;
    [SerializeField] private List<GameObject> obstaclePrefabs;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float distanceToTarget;
    private float prevDistanceToTarget;
    private float angleToTarget;

    private void Start() {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    public override void CollectObservations(VectorSensor sensor) {
        sensor.AddObservation(transform.position);
        sensor.AddObservation(transform.rotation.z);
        sensor.AddObservation(targetTransform.position);
        sensor.AddObservation(GetComponent<Scanners>().getDistances());
        sensor.AddObservation(GetComponent<Movement>().getCurrentSpeed());
        sensor.AddObservation(GetComponent<Movement>().getAcceleration());
    }

    public override void OnEpisodeBegin() {

        prevDistanceToTarget = 0;

        transform.localPosition= initialPosition;
        transform.localRotation = initialRotation;
        GetComponent<Movement>().resetParameters();

        foreach (Transform child in obstacleParent)
        {
            GameObject.Destroy(child.gameObject);
        }

        int numberOfObstacles = Random.Range(4, 7);

        for (int k = 0; k < obstaclePrefabs.Count; k++)
        {
            int numberOfTypeKObstacles = Random.Range(1, numberOfObstacles);
            numberOfObstacles -= numberOfTypeKObstacles;
            for (int i = 0; i < numberOfTypeKObstacles; i++)
            {
                float randomXPosition = randomXPosition = Random.Range(obstacleParent.position.x - 8.9f, obstacleParent.position.x + 8.9f);
                float randomYPosition = randomYPosition = Random.Range(obstacleParent.position.y - 5f, obstacleParent.position.y + 5f);
                bool isValid = false;
                while (!isValid)
                {
                    if ((Mathf.Abs(randomXPosition - initialPosition.x) < 1.5f && Mathf.Abs(randomYPosition - initialPosition.y) < 1.5f) || (Mathf.Abs(randomXPosition - targetTransform.localPosition.x) < 1.5f && Mathf.Abs(randomYPosition - targetTransform.localPosition.y) < 1.5f))
                    {
                        randomXPosition = Random.Range(-8.9f, 8.9f);
                        randomYPosition = Random.Range(-5f, 5f);
                    }
                    else
                    {
                        isValid = true;
                        foreach (Transform child in obstacleParent)
                        {
                            if (Mathf.Abs(randomXPosition - child.localPosition.x) < 2.5f && Mathf.Abs(randomYPosition - child.localPosition.y) < 2f)
                            {
                                randomXPosition = Random.Range(obstacleParent.position.x - 8.9f, obstacleParent.position.x + 8.9f);
                                randomYPosition = Random.Range(obstacleParent.position.y - 5f, obstacleParent.position.y + 5f);
                                isValid = false;
                            }
                        }
                    }
                }
                GameObject obj = Instantiate(obstaclePrefabs[k], new Vector3(randomXPosition, randomYPosition, 0), obstaclePrefabs[0].transform.rotation, obstacleParent);
                obj.transform.Rotate(new Vector3(0, 0, Random.Range(0f, 360f)));
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions) {

        Debug.Log(actions.DiscreteActions[0]);

        switch (actions.DiscreteActions[0]) {
            case 0:
                GetComponent<Movement>().accelerate();
                break;
            case 1:
                GetComponent<Movement>().decelerate();
                break;
            case 2:
                break;
        }
        switch (actions.DiscreteActions[1]) {
            case 0:
                GetComponent<Movement>().rotateLeft();
                break;
            case 1:
                GetComponent<Movement>().rotateRight();
                break;
            case 2:
                break;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        
    }

    void Update() {

        distanceToTarget = Vector3.Distance(transform.localPosition, targetTransform.localPosition);
        angleToTarget = Vector3.Angle(transform.up, (targetTransform.localPosition - transform.localPosition).normalized);

        float distanceToObstacleReward = 0f;

        foreach (float distance in GetComponent<Scanners>().getDistances()) {
            distanceToObstacleReward += (distance - 2)*5;
        }
        if (prevDistanceToTarget != 0) {
            SetReward(10 * (prevDistanceToTarget - distanceToTarget) + distanceToObstacleReward + 10/(angleToTarget+1));
        }

        prevDistanceToTarget = distanceToTarget;
    }

    private void OnTriggerEnter2D(Collider2D collision) {

        float distanceToGoal = Vector3.Distance(transform.localPosition, targetTransform.localPosition);
        float angleToGoal = Vector3.Angle(transform.up, (targetTransform.localPosition - transform.localPosition).normalized);

        if (collision.TryGetComponent<Goal>(out Goal goal)) {
            SetReward(100f);
            EndEpisode();
        }
        
        if (collision.TryGetComponent<Obstacle>(out Obstacle obstacle)) {
            SetReward(-50f);
            EndEpisode();
        }

        if (collision.TryGetComponent<Wall>(out Wall wall))
        {
            SetReward(-100f);
            EndEpisode();
        }

    }

}
