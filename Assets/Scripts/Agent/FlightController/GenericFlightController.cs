using UnityEngine;
using System.Collections;

public class GenericFlightController : MonoBehaviour {

	public Vector3 nextDestination;
	public bool newDestination;

	public void SetNextDestination(Vector3 dest){
		this.nextDestination = dest;
		this.newDestination = true;
	}
	
	public Vector3 GetNextDestination(){
		return this.nextDestination;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
