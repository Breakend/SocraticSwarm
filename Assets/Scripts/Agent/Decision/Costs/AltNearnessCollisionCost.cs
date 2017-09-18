using UnityEngine;
using System.Collections;

public class AltNearnessCollisionCost : Cost {

	Weight fadingFactor;
	public static double[] DEFAULT_WEIGHT_COSTS = { 0.352098226547241, 0.319006323814392};

	// public static double[] DEFAULT_WEIGHT_COSTS = {.4, .4};
	public const double COMMUNICATION_RANGE = 200;
	public const int NEIGHBOURHOOD = 7;
	public const double MAX_FARNESS_PENALTY = .75;

	// Main weight and fading factor
	public static int NUMBER_WEIGHTS = 2;

	public AltNearnessCollisionCost(KnowledgeBase kb, SensorModule sm) : base(kb, sm){
		fadingFactor = new Weight ();
		weights.Add (fadingFactor);
	}


	public static int GetNumberWeights(){
		return NUMBER_WEIGHTS;
	}

	public static double[] GetDefaultWeightCosts(){
		return DEFAULT_WEIGHT_COSTS;
	}

	public override double GetCostValue(){
		// cost should be low around a middle ground and then go toward infinite during a collision point
		// or during loss of communication range (while allowing a dropout)
		double nearness = 0.0;
		for (int i = 0; i < kb.updates.Count && i < NEIGHBOURHOOD; i++) {
			double distToAgent = Vector3.Distance (desiredPosition, ((AgentUpdate)this.kb.updates [i]).lastUpdate.position);

			//Should be normalized to 0 between 0 and comm_range
			if(distToAgent < COMMUNICATION_RANGE*.8)
				nearness += Mathf.Pow((float)fadingFactor.weightValue, i+1)*Mathf.Pow((((float)distToAgent*2.0f/((float)COMMUNICATION_RANGE))-1.0f), 2.0f);
			else
				nearness += Mathf.Pow((float)fadingFactor.weightValue, i+1)*MAX_FARNESS_PENALTY;
		}
		return Mathf.Min((float) nearness/kb.updates.Count, 1.0f);
	}
}
