using UnityEngine;
using System.Collections;

public class NewFlightController {

	public GameObject agentObject;
	public SensorModule sensorModule;

	public Vector3 destination;
	public Vector3 velocity;
	public Vector3 accleration;
	public bool newDestination;

	private Vector3 distanceVector;

	public static Vector3 acclScale = new Vector3(6f,3f,6f);
	private static float MAX_VELOCITY = 40;

	public void CleanUp(){
		agentObject = null;
		sensorModule = null;
	}


	public static float MaxVelocity{
		get{
			return MAX_VELOCITY;		
		}
	}

	public NewFlightController (GameObject agentObject, SensorModule sensorModule) {
		this.agentObject = agentObject;
		this.sensorModule = sensorModule;
	}

	public void SetNextDestination(Vector3 dest){
		this.destination = dest;
		this.newDestination = true;
	}

	public Vector3 GetNextDestination(){
		return this.destination;
	}

	public void UpdateMotion (float distanceToGoal) {
		if (destination != null) {
			Vector3 currentPosition = agentObject.transform.position;
			if (this.newDestination) {
				distanceVector = destination - currentPosition;

				//pitch
				float altitude = sensorModule.altimeter.altitude;
				int direction = altitude > destination.y ? -1 : 1;

				float pitchAngle = distanceVector.normalized.z*15 + Random.Range(-2,2);
				float rollAngle = distanceVector.normalized.x*15 + Random.Range(-2,2);
				float yawAngle = distanceVector.normalized.y*15 + Random.Range(-2,2);

				Quaternion newRot = Quaternion.Euler(new Vector3(pitchAngle, rollAngle, yawAngle));

				/* distanceVector.x += Random.Range(-2,2);
				distanceVector.y += Random.Range(-2,2);
				distanceVector.z += Random.Range(-2,2); */

				//uvDirection = Vector3.MoveTowards(currentPosition, nextDestination, 30*Time.deltaTime);
				agentObject.transform.rotation = Quaternion.Slerp (agentObject.transform.rotation, newRot, 3 * Time.deltaTime);

				accleration = distanceVector.normalized;
				accleration.Scale (acclScale);

				this.newDestination = false;
			}

			//Debug.DrawLine (currentPosition, nextDestination);

			/* If within distance of the goal */
			if (distanceToGoal >= 0 && distanceToGoal < 100) {
				float factor = distanceToGoal / 100;
				factor = Mathf.Max(factor, 0.6f);
				accleration *=  factor;

				velocity += accleration*Time.deltaTime*50;
				if (velocity.magnitude > (factor * MAX_VELOCITY) ) {
					velocity = factor * MAX_VELOCITY * velocity.normalized;
				}
			} else {
				velocity += accleration*Time.deltaTime*50;
				if (velocity.magnitude > MAX_VELOCITY) {
					velocity = MAX_VELOCITY * velocity.normalized;
				}
			}


			Vector3 nextPosition = new Vector3(currentPosition.x + (velocity.x * Time.deltaTime),
			                                   currentPosition.y + (velocity.y * Time.deltaTime),
			                                   currentPosition.z + (velocity.z * Time.deltaTime));
			agentObject.transform.position = nextPosition;
		}

	}

}