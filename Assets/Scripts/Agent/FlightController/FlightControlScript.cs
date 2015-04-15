using UnityEngine;
using System.Collections;
using System;

public class FlightControlScript : GenericFlightController
{


	public SensorModule sensorModule;

	private float altitude;
	private Vector3 velocity;
	private Vector3 angularVelocity;
	private Quaternion desiredRotation;
	private Quaternion currentRotation;
	private Quaternion errorQuaternion;
	private float sensitivity;
	private float desiredAltitude;
	private float idleThrottle;

	public float rollAngle;
	public float yawAngle;
	public float pitchAngle;

	public GameObject explosion; // drag your explosion prefab here

	void OnCollisionEnter(){
		GameObject expl = Instantiate(explosion, transform.position, Quaternion.identity) as GameObject;
		Destroy(gameObject); // destroy the grenade
		Destroy(expl, 3); // delete the explosion after 3 seconds
	}

	// Use this for initialization
	void Start ()
	{

		altitude = sensorModule.altimeter.altitude;
		desiredRotation = new Quaternion (0, 0, 0, 1);
		currentRotation = sensorModule.rotationSensor.getRotation ();
//		velocity = sensorModule.parent.rigidbody.velocity;
//		angularVelocity = sensorModule.parent.rigidbody.angularVelocity;
		sensitivity = 0.5f;
		desiredAltitude = 0;
		idleThrottle = 15.9f;
//		Idle ();

		rollAngle = 0;
		yawAngle = 0;
		pitchAngle = 0;
				

	}

	// Update is called once per frame
	void Update ()
	{

		//get readings from sensors
		altitude = sensorModule.altimeter.altitude;
		currentRotation = sensorModule.rotationSensor.getRotation ();
//		velocity = sensorModule.parent.rigidbody.velocity;
//		angularVelocity = sensorModule.parent.rigidbody.angularVelocity;

	
//		Quaternion moveVector = new Quaternion (5, 0, 0, 1);

		Vector3 destination = GetNextDestination ();

		float desiredAlt = destination.y;


		//TODO fix this once we have collision detection/prevention
		destination.y = altitude;

		float distance = Vector3.Distance(destination, transform.position);
	
		// Pitch angle relative to desired velocity (i.e. distance to target)

//		pitchAngle = Mathf.Clamp (-(destination.z-transform.position.z)*.1f, -2, 2);

//		 Yaw relative to desired direction

//		yawAngle = 90-Mathf.Rad2Deg*(Mathf.Atan2(destination.x - transform.position.x, destination.z - transform.position.z));
//		yawAngle = 0f;


//		rollAngle = Mathf.Clamp ((destination.x-transform.position.x)*.10f, -2, 2);

		if (Input.GetKey (KeyCode.UpArrow)) {
			pitchAngle = Mathf.Clamp (pitchAngle+1, -10, 10);
		}

		if (Input.GetKey (KeyCode.DownArrow)) {
			pitchAngle = Mathf.Clamp (pitchAngle-1, -10, 10);
		}

		if (Input.GetKey (KeyCode.RightArrow)) {
			rollAngle = Mathf.Clamp (rollAngle+1, -10, 10);
		}
		
		if (Input.GetKey (KeyCode.LeftArrow)) {
			rollAngle = Mathf.Clamp (rollAngle-1, -10, 10);
		}

//		rollAngle = 10;
		this.gameObject.GetComponent<AttitudeControl> ().targetRot = Quaternion.Euler(rollAngle,yawAngle,pitchAngle);;
//		this.gameObject.rigidbody.AddForce ((nextDestination - this.transform.position).normalized);

		this.gameObject.GetComponent<AttitudeControl> ().targetAltitude = 20;
			
		
	}

}
