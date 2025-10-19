using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.Barracuda;

public class ModelSwitcher : MonoBehaviour
{
    public NNModel modelNav, modelLeft, modelRight;
    private ForkliftAgent01 agentScript;
    public GameObject checkpointPackage;
    public GameObject checkpointChange;

    void Start()
    {
        agentScript = GetComponent<ForkliftAgent01>();
    }

    void OnTriggerEnter(Collider other)
    {
        checkpointPackage = agentScript.GetCheckpointPackage();
        checkpointChange = agentScript.GetCheckpointChange();

        BehaviorParameters behaviorParameters = GetComponent<BehaviorParameters>();

        if (behaviorParameters != null)
        {
            if (other.gameObject == checkpointPackage)
            {
                behaviorParameters.Model = modelNav;
                agentScript.MaskLiftActions();
            }
            else if (other.gameObject == checkpointChange)
            {
                if (agentScript.GetSide())
                {
                    behaviorParameters.Model = modelLeft;
                } else
                {
                    behaviorParameters.Model = modelRight;
                }
            }
        }
    }

    public void ResetModel()
    {
        BehaviorParameters behaviorParameters = GetComponent<BehaviorParameters>();
        if (behaviorParameters != null)
        {
            behaviorParameters.Model = null;
        }
    }

    public void SetModel()
    {
        BehaviorParameters behaviorParameters = GetComponent<BehaviorParameters>();
        if (behaviorParameters != null)
        {
            behaviorParameters.Model = modelNav;
        }
    }

    public bool isNavigation()
    {
        BehaviorParameters behaviorParameters = GetComponent<BehaviorParameters>();
        if (behaviorParameters != null)
        {
            if (behaviorParameters.Model == modelNav)
            {
                return true;
            }
            return false;
        }
        return false;
    }
}
