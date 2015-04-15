using UnityEngine;
using System.Collections;

public class CostManager {
	private ArrayList costs;

	public void CleanUp(){
		foreach (Cost c in costs) {
			c.CleanUp();
		}
		costs.Clear ();
		costs = null;
	}

	public Cost GetCostFromName(string name){
		foreach(Cost c in costs){
			// Debug.Log(c.name);
			if(c.name == name){
				return c;
			}
		}
		return null;
	}

	public CostManager(KnowledgeBase knowledgeBase, SensorModule sm){
		ArrayList desiredCosts = new ArrayList ();
		System.Object[] args = new System.Object[]{(System.Object)knowledgeBase, (System.Object)sm};

		int weightIndex = 0;

		foreach (System.Type costType in WorldStateManager.GetListOfCostsToUse()) {
			// Debug.Log("Cost: " + costType.ToString());
			Cost c = (Cost) System.Activator.CreateInstance(costType, args);
			c.SetName(costType.ToString());
			desiredCosts.Add (c);
			//Get the weights to test from the trial runner and set them
			// TODO: make this much better coded
			int numWeights = (int) costType.GetMethod("GetNumberWeights", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Invoke(null, null);
//			Debug.Log(numWeights);
			ArrayList localWeights = c.GetAllWeights();
			for(int i = 0; i < numWeights; i++){
				((Weight) localWeights[i]).weightValue = TrialRunner.GetSingletonInstance().GetTrialWeightValueAt(weightIndex++);}
			}

		costs = desiredCosts;

	}

	public ArrayList GetWeights(){
		ArrayList allweights = new ArrayList();
		foreach(Cost c in costs){
			foreach(Weight w in c.GetAllWeights()){
				allweights.Add (w);
			}
		}

		return allweights;
	}

	public ArrayList GetCosts(){
		return costs;
	}

	public double GetTotalCost(){
		double totalCost = 0.0;
		foreach(Cost c in costs){
			totalCost += c.GetWeightedCostValue();
		}
		return totalCost;
	}


}
