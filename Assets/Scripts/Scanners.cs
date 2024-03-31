using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scanners : MonoBehaviour {

    [SerializeField] private int numberOfScanners = 5;
    [SerializeField] private int FOV = 120; //degrees
    
    private GameObject[] scannerObjects;
    private int count;

    // Start is called before the first frame update
    void Start() {
        count = FOV / (numberOfScanners - 1);
        scannerObjects = new GameObject[numberOfScanners];
        for (int i = 0; i < numberOfScanners; i++) {
            float currentDegree = count * i - FOV / 2;
            GameObject obj= new GameObject();
            obj.transform.parent = this.transform;
            obj.transform.localPosition = new Vector3(0, 0, 0);
            ScannerLogic scanner = obj.AddComponent<ScannerLogic>();

            scannerObjects[i] = obj;
            scannerObjects[i].transform.Rotate(new Vector3(0, 0, currentDegree));
            
        }
    }

    public float[] getDistances() {

        

        float[] distances = new float[numberOfScanners];
        
        for (int i = 0; i < numberOfScanners; i++) {
            distances[i] = scannerObjects[i].GetComponent<ScannerLogic>().getDistance();
        }

        return distances;
    }
}
