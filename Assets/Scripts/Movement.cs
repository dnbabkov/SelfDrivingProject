using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {

    [SerializeField] private float currentSpeed = 0f;
    [SerializeField] private float acceleration = 0f;
    [SerializeField] private float maxSpeed = 2f;
    [SerializeField] private float maxAcceleration = 1f;
    [SerializeField] private float rotationSpeed = 15f;

    

    // Start is called before the first frame update
    void Start() {
    }

    // Update is called once per frame
    void Update()
    {
        float time = Time.deltaTime;
        currentSpeed += acceleration * time;
        if (currentSpeed >= maxSpeed)
            currentSpeed = maxSpeed;
        if (currentSpeed < 0 && currentSpeed <= -maxSpeed)
        {
            currentSpeed = -maxSpeed;
        }
        this.transform.position += transform.up * (currentSpeed * time);
        if (Input.GetKey(KeyCode.W)) {
            accelerate();
        }
        if (Input.GetKey(KeyCode.S)) {
            decelerate();
        }
        if (Input.GetKey(KeyCode.D)) {
            rotateRight();
        }
        if (Input.GetKey(KeyCode.A)) {
            rotateLeft();
        }

    }

    public void resetParameters() {
        currentSpeed = 0;
        acceleration = 0;
    }

    public void accelerate() {
        if (acceleration < 0) {
            acceleration = 0;
        }
        if (acceleration >= maxAcceleration) {
            acceleration = maxAcceleration;
        } else {
            acceleration += 1f * Time.deltaTime;
        }
    }
    
    public void decelerate() {
        if (acceleration >= 0) {
            acceleration = 0;
        }
        if (acceleration <= -maxAcceleration) {
            acceleration = -maxAcceleration;
        } else {
            acceleration -= 1f * Time.deltaTime;
        }
    }

    public void rotateRight() {
        this.transform.Rotate(new Vector3(0, 0, -rotationSpeed * Time.deltaTime));
    }

    public void rotateLeft() {
        this.transform.Rotate(new Vector3(0, 0, rotationSpeed * Time.deltaTime));
    }

    public void rotateRightAndAccelerate() {
        accelerate();
        rotateRight();
    }

    public void rotateLeftAndAccelerate() {
        accelerate();
        rotateLeft();
    }

    public float getCurrentSpeed() {
        return currentSpeed;
    }

    public float getAcceleration() {
        return acceleration;
    }

}
