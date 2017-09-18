using UnityEngine;
using System.Collections;

public class AltitudeCost : Cost {
	public static double[] DEFAULT_WEIGHT_COSTS = {1.0};

	public const float MIN_ALTITUDE = 50.0f;
	public const float MAX_ALTITUDE = 500.0f;

	// 1 by default add more like fading factor here
	public static int NUMBER_WEIGHTS = 1;

	public AltitudeCost(KnowledgeBase kb, SensorModule sm) : base(kb, sm){
	}

	public override double GetCostValue(){
		double cost = 0.0;
		float altitude = GetRelativeAltitudeAtPoint (this.desiredPosition);
		if(altitude < MIN_ALTITUDE) cost = Mathf.Exp(MIN_ALTITUDE/altitude);
		if(altitude > MIN_ALTITUDE) cost += Mathf.Exp(altitude/MAX_ALTITUDE);
		return cost;
	}


	public static int GetNumberWeights(){
		return NUMBER_WEIGHTS;
	}

	public static double[] GetDefaultWeightCosts(){
		return DEFAULT_WEIGHT_COSTS;
	}


	private float GetRelativeAltitudeAtPoint(Vector3 pos){
		float diff = pos.y - this.sm.gps.position.y;

		return this.sm.altimeter.altitude + diff;
	}



}
