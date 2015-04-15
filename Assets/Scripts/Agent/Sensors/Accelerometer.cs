using UnityEngine;
using System.Collections;

public class Accelerometer : ISensor{
	
	
	public Vector3 velocity;
	public Vector3 acceleration;
	public Agent agent;
	
	// Use this for initialization
	public Accelerometer (Agent agent) {
		this.agent = agent;
	}
	
	// Update is called once per frame
	void ISensor.Update () {
		if (agent.flightController == null)
			return;
//		this.velocity = this.agent.flightController.velocity;
//		this.acceleration = this.agent.flightController.accleration;
	}

	public void CleanUp(){

		agent = null;
	}
}
