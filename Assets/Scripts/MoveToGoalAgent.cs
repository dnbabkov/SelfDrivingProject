using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MoveToGoalAgent : Agent {

    public bool GUIActive = false;

    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform obstacleParent;
    [SerializeField] private Transform entrance;
    [SerializeField] private List<GameObject> obstaclePrefabs;

    private float timeElapsed;
    private int numberOfAttempts = 0;
    private int numberOfSuccessfulAttempts = 0;
    private int timesCollidedWithAnObstacle;
    private int currentStep;
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
    private float initialDistance;
    private float distanceToTarget;
    private float prevDistanceToTarget;
    private float angleToTarget;
    private float previousRotation;
    private float prevForwardScanRead;
    private float prevAngleToTarget;
    private float timeInContactWithAnObstacle;

    private void Start() {
        timeElapsed = 0f;
        reward = 0f;
        movement = GetComponent<Movement>();
        scanners = GetComponent<Scanners>();
    }

    public override void CollectObservations(VectorSensor sensor) {
        sensor.AddObservation(Vector3.Distance(transform.localPosition, targetTransform.localPosition));
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(transform.rotation.z % 360);
        sensor.AddObservation(Vector3.Angle(transform.up, (targetTransform.localPosition - transform.localPosition).normalized));
        sensor.AddObservation(targetTransform.localPosition);
        sensor.AddObservation(movement.getCurrentSpeed());
    }

    public override void OnEpisodeBegin() {

        currentStep = 0;
        timeInContactWithAnObstacle = 0f;
        timeElapsed = 0f;
        numberOfAttempts += 1;
        Debug.Log("Attempt started");
        ratioOfSuccessfulAttempts = (float)numberOfSuccessfulAttempts / numberOfAttempts;

        transform.localPosition = new Vector3(Random.Range(-8.3f, -5.9f), Random.Range(-4f, 4f));
        transform.Rotate(new Vector3(0, 0, Random.Range(-180f, 180f)));
        entrance.localPosition = new Vector3(-5.3f, Random.Range(-4.2f, 3f));
        targetTransform.localPosition = new Vector3(Random.Range(-3.5f, 7), Random.Range(-3.8f, 3.8f));

        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
        initialDistance = Vector3.Distance(transform.localPosition, targetTransform.localPosition);

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
                int numberOfTries = 0;
                while (!isValid) {
                    
                    if (Vector3.Distance(new Vector3(randomXPosition, randomYPosition), targetTransform.localPosition) < 1.5f) {
                        numberOfTries += 1;
                        randomXPosition = Random.Range(-2f, 2.5f);
                        randomYPosition = Random.Range(-4f, 4f);
                    }
                    else {
                        isValid = true;
                        foreach (Transform child in obstacleParent) {
                            if (Vector3.Distance(new Vector3(randomXPosition, randomYPosition), child.localPosition) < 2f) {
                                randomXPosition = Random.Range(-2f, 2.5f);
                                randomYPosition = Random.Range(-4f, 4f);
                                isValid = false;
                            }
                        }
                    }
                    numberOfTries += 1;
                    if (numberOfTries > 10)
                        break;
                }
                if(isValid) {
                    GameObject obj = Instantiate(obstaclePrefabs[k], new Vector3(randomXPosition, randomYPosition, 0), obstaclePrefabs[0].transform.rotation, obstacleParent);
                    obj.transform.Rotate(new Vector3(0, 0, Random.Range(0f, 360f)));
                }
                
            }
        }*/
    }

    public override void OnActionReceived(ActionBuffers actions) {

        currentStep += 1;
        /*
        float currentXPosition = transform.localPosition.x;
        float currentYPosition = transform.localPosition.y;
        float previousXPosition = previousPosition.x;
        float previousYPosition = previousPosition.y;
        */

        float currentDistance = Vector3.Distance(transform.localPosition, targetTransform.localPosition);
        float previousDistance = Vector3.Distance(previousPosition, targetTransform.localPosition);
        angleToTarget = Vector3.Angle(transform.up, (targetTransform.localPosition - transform.localPosition).normalized);

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

        if (currentDistance < previousDistance) {
            float angleReward = Mathf.Abs(Mathf.Cos(angleToTarget * Mathf.Deg2Rad)) * 2;
            float speedReward = Mathf.Abs(movement.getCurrentSpeed());
            if (currentDistance <= initialDistance / 3)
                speedReward = 1 / (Mathf.Abs(movement.getCurrentSpeed()) + 1);
            SetReward((3f + angleReward) * speedReward);
        }
        /*
        else if (currentDistance > previousDistance) {
            SetReward(-2);
        }
        */
        /*
        if (Mathf.Abs(currentXPosition - targetTransform.localPosition.x) <= 3) {
            if (currentYPosition < previousYPosition) {
                float speedReward = Mathf.Abs(movement.getCurrentSpeed()) + 1;
                SetReward(5f * speedReward);
            }
        }
        */

        if (movement.getCurrentSpeed() == 0) {
            SetReward(-0.5f);
        }

        prevAngleToTarget = angleToTarget;
        previousPosition = transform.localPosition;
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        
    }

    void Update() {

        timeElapsed += Time.deltaTime;

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
        if (GUIActive) {
            GUILayout.Label("Current step is " + currentStep);
            GUILayout.Label("Current speed = " + movement.getCurrentSpeed());
            GUILayout.Label("Current position is:\nx = " + transform.localPosition.x + "\ny = " + transform.localPosition.y);
            GUILayout.Label("Target position is:\nx = " + targetTransform.localPosition.x + "\ny = " + targetTransform.localPosition.y);
            GUILayout.Label("Current distance to target = " + Vector3.Distance(transform.localPosition, targetTransform.localPosition));
            GUILayout.Label("Angle to target is " + Vector3.Angle(transform.up, (targetTransform.localPosition - transform.localPosition).normalized));
            GUILayout.Label("Attempt number is " + numberOfAttempts);
            GUILayout.Label("Number of successful attempts is " + numberOfSuccessfulAttempts);
            GUILayout.Label("Success ratio is " + ratioOfSuccessfulAttempts);
            GUILayout.Label("Time spent in contact with an obstacle this episode (in seconds) is " + timeInContactWithAnObstacle);
            GUILayout.Label("An obstacle has been hit " + timesCollidedWithAnObstacle + " times");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {

        float distanceToGoal = Vector3.Distance(transform.localPosition, targetTransform.localPosition);
        float angleToGoal = Vector3.Angle(transform.up, (targetTransform.localPosition - transform.localPosition).normalized);

        if (collision.gameObject.tag == "Goal") {
            float angleQuotient = Mathf.Abs(Mathf.Cos(angleToGoal * Mathf.Deg2Rad));
            SetReward((400f / (Mathf.Abs(movement.getCurrentSpeed()) + 1f) + angleQuotient * 10) / timeElapsed);
            numberOfSuccessfulAttempts += 1;
            Debug.Log("Reached the target");
            EndEpisode();
        }
        
        if (collision.gameObject.tag == "Obstacle") {
            SetReward(-200f * (Mathf.Abs(movement.getCurrentSpeed()) + 1f));
            Debug.Log("Collided with an obstacle");
            timesCollidedWithAnObstacle += 1;
            //EndEpisode();
        }

        if (collision.gameObject.tag == "Wall")
        {
            SetReward(-200f * (Mathf.Abs(movement.getCurrentSpeed()) + 1f));
            Debug.Log("Collided with the wall");
            timesCollidedWithAnObstacle += 1;
            //EndEpisode();
        }
    }

    private void OnCollisionStay2D(Collision2D collision) {
        if (collision.gameObject.tag == "Obstacle") {
            SetReward(-50f);
            Debug.Log("Hugging an obstacle");
            timeInContactWithAnObstacle += Time.fixedDeltaTime;
        }

        if (collision.gameObject.tag == "Wall")
        {
            SetReward(-50f);
            Debug.Log("Hugging a wall");
            timeInContactWithAnObstacle += Time.fixedDeltaTime;
        }
    }

}
