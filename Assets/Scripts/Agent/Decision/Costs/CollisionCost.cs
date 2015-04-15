using UnityEngine;
using System.Collections;

public class CollisionCost : Cost {

	public static double[] DEFAULT_WEIGHT_COSTS = {1.0};
	public const double MIN_DISTANCE_BETWEEN_AGENTS = 10.0;

	public static int NUMBER_WEIGHTS = 1;

	public CollisionCost(KnowledgeBase kb, SensorModule sm) : base(kb, sm){
	}


	public static int GetNumberWeights(){
		return NUMBER_WEIGHTS;
	}

	public static double[] GetDefaultWeightCosts(){
		return DEFAULT_WEIGHT_COSTS;
	}

	public override double GetCostValue(){
		double collision = 0.0;
		for (int i = 0; i < kb.updates.Count; i++) {
			float distToAgent = Vector3.Distance (desiredPosition, ((AgentUpdate)this.kb.updates [i]).lastUpdate.position);
			// If the new position is too close to other agents
			if (distToAgent < MIN_DISTANCE_BETWEEN_AGENTS) {
				collision += Mathf.Exp(1/distToAgent);
			}
		}
		return collision;
	}

}
