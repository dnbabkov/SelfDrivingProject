using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScannerLogic : MonoBehaviour
{

    public Color beamColor = new Color(0f, 255f, 0f, 1f);
    [SerializeField] private float detectionDistance = 2;
    [SerializeField] private float beamWidth = 0.02f;
    [SerializeField] private LayerMask obstacleLayer;

    private float distance;
    private Vector2 laserPosition;
    [SerializeField] private LineRenderer lineRenderer;

    // Start is called before the first frame update
    void Start()
    {
        obstacleLayer = LayerMask.GetMask("Obstacle");
        distance = detectionDistance;
        laserPosition = new Vector2(0, beamWidth);
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
        lineRenderer.startColor = beamColor;
        lineRenderer.endColor = beamColor;
        lineRenderer.startWidth = beamWidth;
        lineRenderer.endWidth = beamWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.sortingOrder = 2;
    }

    // Update is called once per frame
    void Update() {
        Vector2 beamEndPoint = transform.position + transform.up * detectionDistance;
        RaycastHit2D collisionPoint = Physics2D.Raycast(this.transform.position, this.transform.up, detectionDistance, obstacleLayer);
        if (collisionPoint.collider != null) {
            lineRenderer.SetPosition(0, this.transform.position);
            lineRenderer.SetPosition(1, collisionPoint.point);
            distance = collisionPoint.distance;
        }
        else {
            lineRenderer.SetPosition(0, this.transform.position);
            lineRenderer.SetPosition(1, beamEndPoint);
            distance = detectionDistance;
        }


    }

    public float getDistance() {
        return distance;
    }
}
