using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {

    [SerializeField] private Transform cameraTransform;
    [SerializeField] private List<Transform> roomPositions;

    private int currentRoomNumber;

    // Start is called before the first frame update
    void Start() {
        currentRoomNumber = 0;
        showRoom(currentRoomNumber);
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            currentRoomNumber += 1;
            if (currentRoomNumber > roomPositions.Count - 1) {
                currentRoomNumber = 0;
            }
            showRoom(currentRoomNumber);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            currentRoomNumber -= 1;
            if (currentRoomNumber < 0) {
                currentRoomNumber = roomPositions.Count - 1;
            }
            showRoom(currentRoomNumber);
        }
    }

    private void showRoom(int roomNumber) {
        cameraTransform.position = new Vector3(roomPositions[roomNumber].position.x, roomPositions[roomNumber].position.y, cameraTransform.position.z);
    }

    private void OnGUI() {
        GUI.Label(new Rect (10, (int)(Screen.height * 0.95f), 250, 20), "Current room number is " + (currentRoomNumber + 1));
    }

}
