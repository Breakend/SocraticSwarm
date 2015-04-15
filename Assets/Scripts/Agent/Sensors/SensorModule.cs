using UnityEngine;
using System.Collections;

public class SensorModule {

	private ArrayList sensors;

	public Altimeter altimeter;
	public FeauxGPS gps;
	public BatteryLifeSensor battery;
	public Accelerometer accelerometer;

	// TODO: Should this be a gyroscope??
	public RotationSensor rotationSensor;

	public void CleanUp(){
		foreach(ISensor s in sensors){
			s.CleanUp();
		}
		sensors.Clear ();
		sensors = null;
		altimeter = null;
		gps = null;
		battery = null;
		accelerometer = null;
	}

	// Use this for initialization
	public SensorModule (Agent agent) {
		sensors = new ArrayList ();
		//TODO: This relies on the fact that this.transform is the same as agent's transform. 
		//		If this is not the case (i.e. the script isn't attached correctly, this will cause problems
		altimeter = new Altimeter (agent.gameObject.transform);
		sensors.Add (altimeter);
		rotationSensor = new RotationSensor (agent.gameObject.transform);
		sensors.Add (rotationSensor);
		gps = new FeauxGPS (agent.gameObject.transform);
		sensors.Add (gps);
		battery = new BatteryLifeSensor ();
		sensors.Add (battery);
		accelerometer = new Accelerometer (agent);
		sensors.Add (accelerometer);
	}
	
	// Update is called once per frame
	public void Update () {
		foreach (ISensor sensor in sensors) {
			sensor.Update();
		}
	}
}
