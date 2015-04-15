using UnityEngine;
using System.Collections;

public class SimFlightController : GenericFlightController {
	
	public SensorModule sensorModule;
	
	public float MIN_ALTITUDE = 100;
	public float MAX_ALTITUDE = 300;
	public float ALTITUDE_VELOCITY = 30;
	
	
	private float X_VEL = 0;
	private float Y_VEL = 0;
	private float Z_VEL = 0;
	
	private float X_VEL_MAX = 5;
	private float Y_VEL_MAX = 4;
	private float Z_VEL_MAX = 5;
	
	private float X_ACCR = 0;
	private float Y_ACCR = 0;
	private float Z_ACCR = 0;

	private float multiplier = 5.0f;

	private Vector3 uvDirection, distanceVector;
	
	// Use this for initialization
	void Start () {	
		
	}
	
	void Update () {
		if (nextDestination != null) {
			Vector3 currentPosition = this.gameObject.transform.position;
			if (this.newDestination) {
				distanceVector = nextDestination - currentPosition;
				
				//pitch 
				float altitude = sensorModule.altimeter.altitude;
				int direction = altitude > nextDestination.y ? -1 : 1;
				
				float pitchAngle = distanceVector.normalized.z*15 + Random.Range(-2,2);
				float rollAngle = distanceVector.normalized.x*15 + Random.Range(-2,2);
				float yawAngle = distanceVector.normalized.y*15 + Random.Range(-2,2);
				
				Quaternion newRot = Quaternion.Euler(new Vector3(pitchAngle, rollAngle, yawAngle));
				
				/* distanceVector.x += Random.Range(-2,2);
				distanceVector.y += Random.Range(-2,2);
				distanceVector.z += Random.Range(-2,2); */
				
				//uvDirection = Vector3.MoveTowards(currentPosition, nextDestination, 30*Time.deltaTime);
				this.gameObject.transform.rotation = Quaternion.Slerp (this.gameObject.transform.rotation, newRot, 3*Time.deltaTime);
				
				X_ACCR = Mathf.Clamp(distanceVector.x * 0.2f, -1f, 1f);
				Y_ACCR = Mathf.Clamp(distanceVector.y * 0.1f, -.5f, .5f);
				Z_ACCR = Mathf.Clamp(distanceVector.z * 0.2f, -1f, 1f); // distance.magnitude 
				
				/* Cap velocity */

				
				this.newDestination = false;
			}
			
			//Debug.DrawLine (currentPosition, nextDestination);
			
			X_VEL += X_ACCR;
			Y_VEL += Y_ACCR;
			Z_VEL += Z_ACCR;
			
			/* Cap velocity */
			if 		(X_VEL > X_VEL_MAX) 	X_VEL = X_VEL_MAX;
			else if (X_VEL < -X_VEL_MAX) 	X_VEL = -X_VEL_MAX; 
			if 		(Y_VEL > Y_VEL_MAX) 	Y_VEL = Y_VEL_MAX;
			else if (Y_VEL < -Y_VEL_MAX) 	Y_VEL = -Y_VEL_MAX;
			if 		(Z_VEL > Z_VEL_MAX) 	Z_VEL = Z_VEL_MAX;
			else if (Z_VEL < -Z_VEL_MAX) 	Z_VEL = -Z_VEL_MAX;
			
			Vector3 nextPosition = new Vector3(currentPosition.x + (X_VEL * multiplier * Time.deltaTime), 
			                                   currentPosition.y + (Y_VEL * multiplier * Time.deltaTime), 
			                                   currentPosition.z + (Z_VEL * multiplier * Time.deltaTime) ); 
			this.gameObject.transform.position = nextPosition;
		}
	}
	
	/*
	// Update is called once per frame
	void Update () {
		if (nextDestination != null) {

			if (this.newDestination) {
				Vector3 distanceVector = nextDestination - this.gameObject.transform.position;

				//pitch 
				float altitude = sensorModule.altimeter.altitude;
				int direction = altitude > nextDestination.y ? -1 : 1;
	//			float desiredAltDelta = Mathf.Clamp(this.transform.position.y + ALTITUDE_VELOCITY*Time.deltaTime*direction, MIN_ALTITUDE, MAX_ALTITUDE); 


				float pitchAngle = distanceVector.normalized.z*15 + Random.Range(-2,2);
				float rollAngle = distanceVector.normalized.x*15 + Random.Range(-2,2);
				float yawAngle = distanceVector.normalized.y*15 + Random.Range(-2,2);

				Quaternion newRot = Quaternion.Euler(new Vector3(pitchAngle, rollAngle, yawAngle));

				distanceVector.x += Random.Range(-2,2);
				distanceVector.y += Random.Range(-2,2);
				distanceVector.z += Random.Range(-2,2);
	//			float desiredAlt = Mathf.Clamp(this.transform.position.y + ALTITUDE_VELOCITY*Time.deltaTime*direction, MIN_ALTITUDE, MAX_ALTITUDE); 

				Vector3 nextPo = Vector3.MoveTowards(this.gameObject.transform.position, nextDestination, 50*Time.deltaTime);
	//			nextPo.y = desiredAlt;
				this.gameObject.transform.rotation = Quaternion.Slerp (this.gameObject.transform.rotation, newRot, 3*Time.deltaTime);
	//			this.gameObject.rigidbody.AddForce(Vector3.MoveTowards(this.gameObject.transform.position, destination, 50*Time.deltaTime).normalized*50);
				this.gameObject.transform.position = nextPo;
				this.newDestination = false;
			}

		}
	}
	*/
	
}
