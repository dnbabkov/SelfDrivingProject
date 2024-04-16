using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {

    [SerializeField] private float currentSpeed = 0f;
    [SerializeField] private float acceleration = 2f;
    [SerializeField] private float maxSpeed = 2f;
    [SerializeField] private float rotationSpeed = 15f;
    private Rigidbody2D rb;
    

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float time = Time.fixedDeltaTime;

        if (currentSpeed >= maxSpeed)
            currentSpeed = maxSpeed;

        if (currentSpeed < 0 && currentSpeed <= -maxSpeed)
            currentSpeed = -maxSpeed;

        transform.position += transform.up * (currentSpeed * time);

        //rb.MovePosition(rb.position + (new Vector2(0f, 1f) * rb.rotation * currentSpeed * time));

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
        currentSpeed += acceleration * Time.fixedDeltaTime;
    }
    
    public void decelerate() {
        currentSpeed -= acceleration * Time.fixedDeltaTime;
    }

    public void rotateRight() {
        //rb.AddTorque(rotationSpeed);
        transform.Rotate(new Vector3(0, 0, -rotationSpeed * Time.fixedDeltaTime));
    }

    public void rotateLeft() {
        //rb.AddTorque(-rotationSpeed);
        transform.Rotate(new Vector3(0, 0, rotationSpeed * Time.fixedDeltaTime));
    }

    public float getCurrentSpeed() {
        return currentSpeed;
    }

    public float getAcceleration() {
        return acceleration;
    }

}
