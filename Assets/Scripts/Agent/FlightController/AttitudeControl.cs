using UnityEngine;
using System.Collections;

public class AttitudeControl : MonoBehaviour {



	public Engine engine1;
	public Engine engine2;
	public Engine engine3;
	public Engine engine4;
	public SensorModule sensorModule;

	public Quaternion myRotation;

	public Quaternion targetRot; // target rotation
	public Quaternion errorRot; // error rotation
	public Quaternion curRot; // current rotation
	public Vector3 currentRotation;
	Quaternion yawTarget; //Yaw target


	//integrals
	float integralX;
	float integralY;
	float integralZ;

	public float pitchCorrectionX;
	public float pitchCorrectionY;
	public float pitchCorrectionZ;


	//throttle constraints

	public float throtMax;
	public float throtMin;

	Quaternion lastError;


	private int debugSample;


	//tunings

	public float pGain; // the proportional gain
	public float iGain; // the integral gain
	public float dGain; // the derivative gain



	public float targetAltitude;
	private float lastAltError;


	//integral

	private float integralA;

	//altitude tunings

	public float aPGain;
	public float aIGain;
	public float aDGain;


	Vector3 targetVelocity;

	private Vector3 integralV;

	//velocity tunings

	public float vPGain;
	public float vIGain;
	public float vDGain;

	private Vector3 lastVError;


	void Start(){

		myRotation = this.gameObject.rigidbody.rotation;

		targetRot = new Quaternion (0, 0, 0, 1);
		yawTarget = new Quaternion (0, 0, 0, 1);
//		lastError = new Quaternion (0, 0, 0, 1);

		integralX = 0f;
		integralY = 0f;
		integralZ = 0f;

		pGain = 2000f;
		iGain = 1f;
		dGain = 500f;

		throtMax = 20f;
		throtMin = 15f;


		debugSample = 50;


		targetAltitude = 20f;

		integralA = 0f;

		aPGain = 1f;
		aIGain = 0f;
		aDGain = 300f;


		targetVelocity = new Vector3 (0, 0, 0);


		vPGain = 1f;
		vIGain = 0f;
		vDGain = 100f;

		integralV = new Vector3 (0, 0, 0);


	}

	void FixedUpdate(){

		currentRotation = curRot.eulerAngles;

		AltitudeStablise ();
//		VelocityStablise ();
		AttitudeStablise ();

//		this.gameObject.rigidbody.AddForce (new Vector3(0,9.8f*5f,0));

	}


	private void AltitudeStablise(){

		//stablise altitude
		float currentAltitude = sensorModule.altimeter.altitude;
		float altitudeError = targetAltitude - currentAltitude;
		
		//calculate integrals
		integralA += altitudeError * Time.deltaTime;

		//calculate derivatives
		float derivA = (altitudeError - lastAltError);
		lastAltError = altitudeError;
		
		//calculate altitude correction
		float altCorrection = altitudeError * aPGain + integralA * aIGain + derivA * aDGain;

		throtMin = Mathf.Clamp( throtMin + altCorrection,2,98);
		throtMax = throtMin + 5;

		}

	private void AttitudeStablise(){

		//get current rotation
//		curRot = this.gameObject.rigidbody.rotation;
		curRot = sensorModule.rotationSensor.getRotation ();
//		curRot = this.transform.parent.gameObject.transform.rotation;
		//get error signal
		errorRot = Quaternion.Inverse (curRot) * targetRot;

//		Vector3 angles = curRot.eulerAngles;
//		errorRot = angles - targetRot.eulerAngles;

		
		pitchCorrectionX = errorRot.eulerAngles.x;//+ integralX * iGain + derivX * dGain;
		pitchCorrectionY = errorRot.eulerAngles.y;// + integralY * iGain + derivY * dGain;
		pitchCorrectionZ = errorRot.eulerAngles.z;// + integralZ * iGain + derivZ * dGain;
		
		if(pitchCorrectionX > 180) pitchCorrectionX -= 360;
		if(pitchCorrectionY > 180) pitchCorrectionY -= 360;
		if(pitchCorrectionZ > 180) pitchCorrectionZ -= 360;
		
		
		//calculate integrals
		
		integralX += pitchCorrectionX * Time.deltaTime;
		integralY += pitchCorrectionY * Time.deltaTime;
		integralZ += pitchCorrectionZ * Time.deltaTime;
//		
		
		//calculate derivatives
		
		float derivX = (errorRot.eulerAngles.x - lastError.eulerAngles.x);
		float derivY = (errorRot.eulerAngles.y - lastError.eulerAngles.y);
		float derivZ = (errorRot.eulerAngles.z - lastError.eulerAngles.z);
		
		
		
		lastError = errorRot;


		
		//caluculate corrections


		pitchCorrectionX = pitchCorrectionX + integralX * iGain + derivX * dGain;
		pitchCorrectionY = pitchCorrectionY + integralY * iGain + derivY * dGain;
		pitchCorrectionZ = pitchCorrectionZ + integralZ * iGain + derivZ * dGain;

		if(pitchCorrectionX > 180) pitchCorrectionX -= 360;
		if(pitchCorrectionY > 180) pitchCorrectionY -= 360;
		if(pitchCorrectionZ > 180) pitchCorrectionZ -= 360;

		//pitch
		engine1.SetThrottle (Mathf.Clamp ((engine1.getThrottle () - pitchCorrectionZ*.005f), throtMin, throtMax));
		engine3.SetThrottle (Mathf.Clamp ((engine3.getThrottle () + pitchCorrectionZ*.005f), throtMin, throtMax));
		
////		//roll
//		engine2.SetThrottle (throtMin);
//		engine4.SetThrottle (throtMin);
//
//		engine1.SetThrottle (throtMin);
//		engine3.SetThrottle (throtMin);


//
		engine2.SetThrottle (Mathf.Clamp ((engine2.getThrottle () - pitchCorrectionX*.05f), throtMin, throtMax));
		engine4.SetThrottle (Mathf.Clamp ((engine4.getThrottle () + pitchCorrectionX*.05f), throtMin, throtMax));
////		
		//yaw
//		
//		engine1.SetThrottle (Mathf.Clamp ((engine1.getThrottle () + pitchCorrectionY*.05f), throtMin, throtMax));
//		engine3.SetThrottle (Mathf.Clamp ((engine3.getThrottle () + pitchCorrectionY*0.05f), throtMin, throtMax));
//		engine2.SetThrottle (Mathf.Clamp ((engine2.getThrottle () - pitchCorrectionY*.05f), throtMin, throtMax));
//		engine4.SetThrottle (Mathf.Clamp ((engine4.getThrottle () - pitchCorrectionY*.05f), throtMin, throtMax));
	}

}



