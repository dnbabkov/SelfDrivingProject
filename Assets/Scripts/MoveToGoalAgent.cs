using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MoveToGoalAgent : Agent {

    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform obstacleParent;
    [SerializeField] private Transform entrance;
    [SerializeField] private List<GameObject> obstaclePrefabs;

    private int numberOfAttempts = 0;
    private int numberOfSuccessfulAttempts = 0;
    private float ratioOfSuccessfulAttempts = 0f;
    private List<Transform> scannerObjects;
    private float reward;
    private float currentAngle;
    private Scanners scanners;
    private Movement movement;
    private Vector3 previousPosition;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float[] previousDistances;
    private float distanceToTarget;
    private float prevDistanceToTarget;
    private float angleToTarget;
    private float previousRotation;
    private float prevForwardScanRead;
    private float prevAngleToTarget;

    private void Start() {
        reward = 0f;
        //scannerObjects = new List<Transform>();
        /*foreach (Transform child in transform) {
            scannerObjects.Add(child);
        }*/
        movement = GetComponent<Movement>();
        scanners = GetComponent<Scanners>();
    }

    public override void CollectObservations(VectorSensor sensor) {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(transform.rotation.z % 360);
        sensor.AddObservation(Vector3.Angle(transform.up, (targetTransform.localPosition - transform.localPosition).normalized));
        sensor.AddObservation(targetTransform.localPosition);
        sensor.AddObservation(movement.getCurrentSpeed());
    }

    public override void OnEpisodeBegin() {

        numberOfAttempts += 1;
        ratioOfSuccessfulAttempts = (float)numberOfSuccessfulAttempts / numberOfAttempts;

        transform.localPosition = new Vector3(Random.Range(-8.3f, -5.9f), Random.Range(-4f, 4f));
        transform.Rotate(new Vector3(0, 0, Random.Range(-180f, 180f)));
        entrance.localPosition = new Vector3(-5.3f, Random.Range(-4.2f, 3f));
        targetTransform.localPosition = new Vector3(Random.Range(-3.5f, 7), Random.Range(-3.8f, 3.8f));

        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;

        prevDistanceToTarget = 0f;
        previousRotation = initialRotation.z;

        transform.localPosition= initialPosition;
        transform.localRotation = initialRotation;
        movement.resetParameters();

        foreach (Transform child in obstacleParent)
        {
            GameObject.Destroy(child.gameObject);
        }

        int numberOfObstacles = Random.Range(3, 5);

        /*for (int k = 0; k < obstaclePrefabs.Count; k++) {
            int numberOfTypeKObstacles = Random.Range(1, numberOfObstacles);
            numberOfObstacles -= numberOfTypeKObstacles;
            for (int i = 0; i < numberOfTypeKObstacles; i++) {
                float randomXPosition = Random.Range(-2f, 2.5f);
                float randomYPosition = Random.Range(-4f, 4f);
                bool isValid = false;
                while (!isValid) {
                    int numberOfTries = 0;
                    if (Vector3.Distance(new Vector3(randomXPosition, randomYPosition), targetTransform.localPosition) < 1.5f) {
                        numberOfTries += 1;
                        randomXPosition = Random.Range(-2f, 2.5f);
                        randomYPosition = Random.Range(-4f, 4f);
                    }
                    else {
                        isValid = true;
                        foreach (Transform child in obstacleParent) {
                            if (Vector3.Distance(new Vector3(randomXPosition, randomYPosition), child.localPosition) < 2f) {
                                numberOfTries += 1;
                                randomXPosition = Random.Range(-2f, 2.5f);
                                randomYPosition = Random.Range(-4f, 4f);
                                isValid = false;
                            }
                        }
                    }
                    if (numberOfTries > 10)
                        isValid = true;
                }
                GameObject obj = Instantiate(obstaclePrefabs[k], new Vector3(randomXPosition, randomYPosition, 0), obstaclePrefabs[0].transform.rotation, obstacleParent);
                obj.transform.Rotate(new Vector3(0, 0, Random.Range(0f, 360f)));
            }
        }*/
    }

    public override void OnActionReceived(ActionBuffers actions) {

        float currentDistance = Vector3.Distance(transform.localPosition, targetTransform.localPosition);
        float previousDistance = Vector3.Distance(previousPosition, targetTransform.localPosition);

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

        /*if (movement.getCurrentSpeed() <= 0) {
            SetReward(-0.2f);
        }*/

        if (currentDistance < previousDistance) {
            float angleReward = Mathf.Abs(Mathf.Cos(angleToTarget * Mathf.Deg2Rad)) / 2;
            float speedReward = movement.getCurrentSpeed() + 1;
            SetReward((1f + angleReward) * speedReward);
        } else if (currentDistance > previousDistance) {
            SetReward(-1f);
        }

        if (movement.getCurrentSpeed() == 0) {
            SetReward(-0.5f);
        }

        /*
        if (prevAngleToTarget < angleToTarget) {
            SetReward(-0.1f);
        } else if (prevAngleToTarget > angleToTarget) {
            SetReward(0.1f);
        }
        */
        prevAngleToTarget = angleToTarget;
        previousPosition = transform.localPosition;
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        
    }

    void Update() {
        /*
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
        */
    }

    private void OnGUI() {
        GUILayout.Label("Current speed = " + movement.getCurrentSpeed());
        GUILayout.Label("Current position is:\nx = " + transform.localPosition.x + "\ny = " + transform.localPosition.y);
        GUILayout.Label("Target position is:\nx = " + targetTransform.localPosition.x + "\ny = " + targetTransform.localPosition.y);
        GUILayout.Label("Current distance to target = " + Vector3.Distance(transform.localPosition, targetTransform.localPosition));
        GUILayout.Label("Angle to target is " + Vector3.Angle(transform.up, (targetTransform.localPosition - transform.localPosition).normalized));
        GUILayout.Label("Attempt number is " + numberOfAttempts);
        GUILayout.Label("Number of successful attempts is " + numberOfSuccessfulAttempts);
        GUILayout.Label("Success ratio is " + ratioOfSuccessfulAttempts);
    }

    private void OnTriggerEnter2D(Collider2D collision) {

        float distanceToGoal = Vector3.Distance(transform.localPosition, targetTransform.localPosition);
        float angleToGoal = Vector3.Angle(transform.up, (targetTransform.localPosition - transform.localPosition).normalized);

        if (collision.TryGetComponent<Goal>(out Goal goal)) {
            float angleQuotient = Mathf.Abs(Mathf.Cos(angleToGoal * Mathf.Deg2Rad));
            SetReward(400f / (Mathf.Abs(movement.getCurrentSpeed()) + 1f) + angleQuotient * 10);
            numberOfSuccessfulAttempts += 1;
            Debug.Log("Reached the target");
            EndEpisode();
        }
        
        if (collision.TryGetComponent<Obstacle>(out Obstacle obstacle)) {
            SetReward(-400f * (Mathf.Abs(movement.getCurrentSpeed()) + 1f));
            Debug.Log("Collided with an obstacle");
            EndEpisode();
        }

        if (collision.TryGetComponent<Wall>(out Wall wall))
        {
            SetReward(-400f * (Mathf.Abs(movement.getCurrentSpeed()) + 1f));
            Debug.Log("Collided with the wall");
            EndEpisode();
        }
    }
}
