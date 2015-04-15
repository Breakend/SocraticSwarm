using UnityEngine;
using System.Collections;
using System;

public class Engine : MonoBehaviour
{

		public string engName;
		public double idle;
		public float maxPower;
		public float throttle;
		public int engNum;

		void Start(){
			throttle = 0;
		}

		void FixedUpdate ()
		{
				
				Vector3 force = transform.up * (maxPower * (throttle / 100));
				rigidbody.AddForce (force);
				Debug.DrawLine (transform.position, transform.position - force/2);
//				Debug.DrawRay (transform.position, transform.position - force / 2, Color.green);	
		              
		}

		public void IncreaseThrottle ()
		{

				if (throttle < 100) {
						throttle = throttle + 0.1f;
				}
		}
	
		public void DecreaseThrottle ()
		{
				if (throttle > 0) {
						throttle = throttle - 0.1f;
				}
		
		}

		public void CutEngine ()
		{
				throttle = 0;

		}

		public void SetThrottle (float value)
		{
			throttle = Mathf.Clamp (value, 0, 100);
		}

		public float getThrottle ()
		{

				//double roundThrt = Math.Round (Convert.ToDouble(throttle), 2);


				return throttle;
		}
}
