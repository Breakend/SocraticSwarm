using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Altimeter : ISensor{

    
    public float altitude = 0;
	private RaycastHit hit;
	private Transform agentTransform;

	// Use this for initialization
	public Altimeter (Transform agentTransform) {
		this.agentTransform = agentTransform;
	}

	public void CleanUp(){
		agentTransform = null;
	}
	
	// Update is called once per frame
	void ISensor.Update () {

		float alt =0;

		if (Physics.Raycast(agentTransform.position, -Vector3.up, out hit)){
			 alt = hit.distance;
		}

		if (alt < 1)
				return;
		else 
				altitude = alt;

//		Debug.DrawLine(transform.position, -Vector3.up, Color.red);
	
	}

     

    
}
