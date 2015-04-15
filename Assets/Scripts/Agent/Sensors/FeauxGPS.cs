using UnityEngine;
using System.Collections;

public class FeauxGPS : ISensor{

	private Transform agentTransform;
	public Vector3 position;

	public FeauxGPS(Transform agentTransform){
		this.agentTransform = agentTransform;
	}
	
	// Update is called once per frame
	void ISensor.Update () {
		this.position = this.agentTransform.position;
	}

	public void CleanUp(){
		agentTransform = null;
	}
}
