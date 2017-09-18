using UnityEngine;
using System.Collections;

public class RotationSensor : ISensor {

	public Quaternion currentRotation;
	public Vector3 eulers;
	private Transform agentTransform;

	// Use this for initialization
	public RotationSensor (Transform agentTransform) {
		this.agentTransform = agentTransform;
		currentRotation = agentTransform.GetComponent<Rigidbody>().rotation;
		eulers = currentRotation.eulerAngles;
	}
	
	// Update is called once per frame
	void ISensor.Update () {
		currentRotation = agentTransform.GetComponent<Rigidbody>().rotation;
		eulers = currentRotation.eulerAngles;
	}

	public Quaternion getRotation(){
		return currentRotation;
	}

	public void CleanUp(){
		agentTransform = null;
	}
}
