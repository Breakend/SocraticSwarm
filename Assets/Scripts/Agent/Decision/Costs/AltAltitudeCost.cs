using UnityEngine;
using System.Collections;

public class AltAltitudeCost : Cost {
        public static double[] DEFAULT_WEIGHT_COSTS = {0.176022469997406};

	public const float MAX_ALT = 300.0f;

	// 1 by default add more like fading factor here
	public static int NUMBER_WEIGHTS = 1;

	public AltAltitudeCost(KnowledgeBase kb, SensorModule sm) : base(kb, sm){
	}

	public override double GetCostValue(){
		float cost = 0.0f;
		float altitude = GetRelativeAltitudeAtPoint (this.desiredPosition);

		if (altitude < MAX_ALT * .2)
			cost = Mathf.Pow (1.0f - ((altitude) / (MAX_ALT * .2f) ), 2.0f);
		else
			cost = Mathf.Pow ((altitude-(MAX_ALT*.2f))/(MAX_ALT), 2.0f) * 2.0f;

		return Mathf.Min (cost, 1.0f);
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
