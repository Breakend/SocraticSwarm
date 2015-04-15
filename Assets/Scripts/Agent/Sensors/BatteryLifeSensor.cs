using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BatteryLifeSensor : ISensor{

	//TODO: calculate a better drain rate based on real values
	private float BATTERY_DRAIN_RATE = .001667f; //percentage per second (assumes 600 second flight time) 
	public float batteryLife;
	// Use this for initialization
	public BatteryLifeSensor () {
		//TODO: could add velocity, etc. from flight controller to make it more realistic based on expenditure 
		// on energy intensive resources
		batteryLife = 1.0f;
	}
	
	// Update is called once per frame
	void ISensor.Update () {
		//TODO: have something visible happen when the battery runs out
		batteryLife -= BATTERY_DRAIN_RATE * Time.deltaTime;
	}

	void ISensor.CleanUp(){
		
	}
	
	
	
	
}
