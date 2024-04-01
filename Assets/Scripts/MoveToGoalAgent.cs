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
    private List<Transform> scannerObjects;
    private float reward;
    private Scanners scanners;
    private Movement movement;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float distanceToTarget;
    private float prevDistanceToTarget;
    private float angleToTarget;
    private float previousRotation;
    private float prevForwardScanRead;

    private void Start() {
        reward = 0f;
        scannerObjects = new List<Transform>();
        foreach (Transform child in transform) {
            scannerObjects.Add(child);
        }
        movement = GetComponent<Movement>();
        scanners = GetComponent<Scanners>();
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    public override void CollectObservations(VectorSensor sensor) {
        sensor.AddObservation(transform.position);
        sensor.AddObservation(Vector3.Angle(transform.up, (targetTransform.localPosition - transform.localPosition).normalized));
        sensor.AddObservation(targetTransform.position);
        sensor.AddObservation(scanners.getDistances());
        sensor.AddObservation(movement.getCurrentSpeed());
        sensor.AddObservation(movement.getAcceleration());
    }

    public override void OnEpisodeBegin() {

        prevForwardScanRead = scanners.getDistances()[scanners.getDistances().Length/2];
        reward = 0f;
        prevDistanceToTarget = 0f;
        previousRotation = initialRotation.z;

        transform.localPosition= initialPosition;
        transform.localRotation = initialRotation;
        movement.resetParameters();

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
                movement.accelerate();
                break;
            case 1:
                movement.decelerate();
                break;
        }
        switch (actions.DiscreteActions[1]) {
            case 0:
                movement.rotateLeft();
                break;
            case 1:
                movement.rotateRight();
                break;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        
    }

    void Update() {

        distanceToTarget = Vector3.Distance(transform.localPosition, targetTransform.localPosition);
        angleToTarget = Vector3.Angle(transform.up, (targetTransform.localPosition - transform.localPosition).normalized);

        //float distanceToObstacleReward = 0f;
        float[] distances = scanners.getDistances();
        float angleToTargetReward = 100 / (angleToTarget + 1);
        float distanceReward = 100 * (prevDistanceToTarget - distanceToTarget);
        

        for (int i = 0; i < scannerObjects.Count; i++) {
            if (distances[i] != 2) {
                //distanceToObstacleReward += - (50 / (distances[i] + 1) + Mathf.Abs(scannerObjects[i].localRotation.z) / 2);
                angleToTargetReward = 0f;
                if (scannerObjects[i].localRotation.z == 0) {
                    //distanceReward = 100 * (distanceToTarget - prevDistanceToTarget) / distances[i];
                    if (prevForwardScanRead - distances[i] > 0) {
                        SetReward(-4f);
                    } else {
                        SetReward(4f);
                    }
                }
            }            
        }

        if (movement.getCurrentSpeed() < 0) {
            SetReward(-3f);
        }

        for (int i = 0; i < scannerObjects.Count/2; i++) {
            if (distances[i] != 2 && distances[scannerObjects.Count-1-i] != 2) {
                if (distances[i] > distances[scannerObjects.Count - 1 - i]) {
                    if (previousRotation - transform.rotation.z < 0) {
                        SetReward(2f);
                    } 
                }
                else {
                    if (previousRotation - transform.rotation.z > 0) {
                        SetReward(2f);
                    } 
                }
            } else if (distances[i] != 2) {
                angleToTargetReward = 0;
                distanceReward = 0;
                if (previousRotation - transform.rotation.z < 0) {
                    SetReward(2f);
                }
            } else if(distances[scannerObjects.Count - 1 - i] != 2) {
                angleToTargetReward = 0;
                distanceReward = 0;
                if (previousRotation - transform.rotation.z > 0) {
                    SetReward(2f);
                } 
            }
        }

        reward = distanceReward + angleToTargetReward;
        // + distanceToObstacleReward
        if (prevDistanceToTarget != 0) {
            SetReward(reward);
        }

        prevForwardScanRead = scanners.getDistances()[scanners.getDistances().Length / 2];
        prevDistanceToTarget = distanceToTarget;
        previousRotation = transform.localRotation.z;
    }

    private void OnGUI() {
        GUILayout.Label("Current speed = " + movement.getCurrentSpeed());
        GUILayout.Label("Current acceleration = " + movement.getAcceleration());
        for (int i = 0; i < scanners.getDistances().Length; i++) {
            GUILayout.Label("Scanner number " + (i + 1) + " returns the distance of " + scanners.getDistances()[i]);
        }
        GUILayout.Label("Angle to target is " + Vector3.Angle(transform.up, (targetTransform.localPosition - transform.localPosition).normalized));
        GUILayout.Label("Current reward is " + reward);
    }

    private void OnTriggerEnter2D(Collider2D collision) {

        float distanceToGoal = Vector3.Distance(transform.localPosition, targetTransform.localPosition);
        float angleToGoal = Vector3.Angle(transform.up, (targetTransform.localPosition - transform.localPosition).normalized);

        if (collision.TryGetComponent<Goal>(out Goal goal)) {
            SetReward(100f);
            EndEpisode();
        }
        
        if (collision.TryGetComponent<Obstacle>(out Obstacle obstacle)) {
            SetReward(-100f);
            EndEpisode();
        }

        if (collision.TryGetComponent<Wall>(out Wall wall))
        {
            SetReward(-100f);
            EndEpisode();
        }

    }

}
