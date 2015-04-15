using UnityEngine;
using System.Collections;

public class NearnessCost : Cost {

	Weight fadingFactor;

	public static double[] DEFAULT_WEIGHT_COSTS = {.10, .5};
	public const double COMMUNICATION_RANGE = 150;

	// Main weight and fading factor
	public static int NUMBER_WEIGHTS = 2;

	public NearnessCost(KnowledgeBase kb, SensorModule sm) : base(kb, sm){
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
		double nearness = 0.0;
		for (int i = 0; i < kb.updates.Count; i++) {
			double distToAgent = Vector3.Distance (desiredPosition, ((AgentUpdate)this.kb.updates [i]).lastUpdate.position);
			nearness += Mathf.Pow ((float)fadingFactor.weightValue, i+1) * Mathf.Exp ((float)(distToAgent/COMMUNICATION_RANGE));
		}
		return 1/nearness;
	}

}
