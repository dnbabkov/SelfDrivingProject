using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {

    [SerializeField] private float currentSpeed = 0f;
    [SerializeField] private float acceleration = 2f;
    [SerializeField] private float maxSpeed = 2f;
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
    }

    public void accelerate() {
        currentSpeed += acceleration * Time.deltaTime;
    }
    
    public void decelerate() {
        currentSpeed -= acceleration * Time.deltaTime;
    }

    public void rotateRight() {
        this.transform.Rotate(new Vector3(0, 0, -rotationSpeed * Time.deltaTime));
    }

    public void rotateLeft() {
        this.transform.Rotate(new Vector3(0, 0, rotationSpeed * Time.deltaTime));
    }

    public float getCurrentSpeed() {
        return currentSpeed;
    }

    public float getAcceleration() {
        return acceleration;
    }

}
