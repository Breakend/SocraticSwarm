using UnityEngine;
using System.Collections;

public interface IDecisionManager {

	CostManager GetCostManager();
	Vector3 getNextDesiredPoint ();
	void CleanUp();
}
