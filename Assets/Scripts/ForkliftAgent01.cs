using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using System.Linq;

public class ForkliftAgent01 : Agent
{
    [SerializeField] private Transform spawnPosition;
    [SerializeField] private List<Transform> spawnPositions = new List<Transform>();
    [SerializeField] private List<GameObject> checkpointsPackage = new List<GameObject>();
    [SerializeField] private List<GameObject> checkpointsChange = new List<GameObject>();
    [SerializeField] private List<GameObject> WallsTask02 = new List<GameObject>();

    [SerializeField] private GameObject lift;
    [SerializeField] private GameObject pallet;
    [SerializeField] private GameObject NavPallet;
    [SerializeField] private GameObject package;
    [SerializeField] private GameObject buttons;
    [SerializeField] private bool isTouchingPallet = false;
    [SerializeField] private bool episodeEnded = false;

    [SerializeField] private GameObject CheckpointPackage;
    [SerializeField] private GameObject CheckpointChange;
    [SerializeField] private GameObject WallTask02;
    [SerializeField] private GameObject checkpointsParent;
    [SerializeField] private int numberOfCheckpoints;
    [SerializeField] private List<GameObject> checkpoints = new List<GameObject>();

    [SerializeField] private GameObject points;
    [SerializeField] private GameObject pointsReverse;
    [SerializeField] private GameObject shelvesParent;
    private Dictionary<float, bool> distanceBins = new Dictionary<float, bool>();
    private Dictionary<float, bool> alignmentBins = new Dictionary<float, bool>();
    private Dictionary<float, bool> alignmentPackageBins = new Dictionary<float, bool>();
    private Dictionary<float, bool> alignmentPackageIncreaseBins = new Dictionary<float, bool>();
    private Dictionary<float, bool> liftBins = new Dictionary<float, bool>();
    private Dictionary<float, bool> checkpointDistanceBins = new Dictionary<float, bool>();

    private float minAlignmentAchieved;
    private float minAlignmentPackageAchieved;
    private float maxAlignmentPackageAchieved;
    private float minDistanceAchieved;
    private float maxLiftHeight;
    private bool hasExceededLiftHeight = false;
    private bool hasExceededLiftHeightPackage = false;
    private bool hasLiftedPallet = false;
    private bool hasExceededDistance = false;
    private bool hasExceededAlignment = false;
    private bool hasExceededAlignmentPackage = false;
    private bool hasExceededAlignmentPackageIncrease = false;
    private float lastMinDistanceAchieved;
    private float lastMinAlignmentAchieved;
    private float lastMinAlignmentPackageAchieved;
    private float lastMaxAlignmentPackageAchieved;
    private const float alignmentTolerance = 12f;
    private const float distanceTolerance = 0.5f;
    private int achievedDistanceBins = 0;
    private int achievedAlignmentBins = 0;
    private int achievedAlignmentPackageBins = 0;
    private int achievedAlignmentPackageIncreaseBins = 0;
    private const int minAchievedBins = 2;
    private float minCheckpointDistanceAchieved;
    private int pointsTouched = 0;
    private bool hasCollectedPoint = false;
    private bool palletTouchedWall = false;

    private int nextCheckpointIndex = 0;
    private Dictionary<GameObject, bool> checkpointTouched;
    private int iterations = 0;
    private float rewards = 0;

    private int currentShelfIndex = 0;
    private int currentItemIndex = 0;
    private int wins = 0;
    private bool maskLift = false;
    private bool side = false;

    private List<GameObject> activePallets = new List<GameObject>();
    private List<GameObject> activePackages = new List<GameObject>();
    private List<GameObject> activePoints = new List<GameObject>();
    private List<GameObject> activePointsReverse = new List<GameObject>();

    public GameObject GetCheckpointPackage()
    {
        return CheckpointPackage;
    }

    public GameObject GetCheckpointChange()
    {
        return CheckpointChange;
    }

    public bool GetSide()
    {
        return side;
    }

    void RewardAgent(float reward)
    {
        AddReward(reward);
        rewards += reward;
        //Debug.Log(rewards);
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        if (maskLift)
        {
            actionMask.SetActionEnabled(2, 1, false);
            actionMask.SetActionEnabled(2, 2, false);
        }
    }

    public void MaskLiftActions()
    {
        maskLift = true;
    }

    public void UnmaskLiftActions()
    {
        maskLift = false;
    }

    void Start()
    {
        foreach (Transform checkpoint in checkpointsParent.transform)
        {
            checkpoints.Add(checkpoint.gameObject);
        }
        numberOfCheckpoints = checkpoints.Count;
        wins = 0;

        checkpointTouched = new Dictionary<GameObject, bool>();
        foreach (var checkpoint in checkpoints)
        {
            checkpointTouched[checkpoint] = false;
        }

        alignmentBins = new Dictionary<float, bool>
        {
            { 80f, false },
            { 70f, false },
            { 60f, false },
            { 50f, false },
            { 40f, false },
            { 30f, false },
            { 20f, false },
            { 10f, false },
            { 5f, false },
            { 3f, false },
            { 1f, false }
        };

        alignmentPackageBins = new Dictionary<float, bool>
        {
            { 89f, false },
            { 87f, false },
            { 85f, false },
            { 84f, false },
            { 82f, false },
            { 80f, false },
            { 75f, false },
            { 70f, false },
            { 65f, false },
            { 60f, false },
            { 55f, false },
            { 50f, false },
            { 45f, false },
            { 40f, false },
            { 35f, false },
            { 30f, false },
            { 25f, false },
            { 20f, false },
            { 15f, false },
            { 10f, false },
            { 5f, false },
            { 3f, false },
            { 1f, false }
        };

        alignmentPackageIncreaseBins = new Dictionary<float, bool>
        {
            { 100f, false },
            { 110f, false },
            { 120f, false },
            { 130f, false },
            { 140f, false },
            { 150f, false },
            { 160f, false },
            { 170f, false },
            { 175f, false }
        };

        distanceBins = new Dictionary<float, bool>
        {
            { 6f, false },
            { 5.5f, false },
            { 5f, false },
            { 4.5f, false },
            { 4f, false },
            { 3.5f, false },
            { 3f, false },
            { 2.5f, false },
            { 2.35f, false }
        };

        liftBins = new Dictionary<float, bool>
        {
            { 0f, false },
            { 0.1f, false },
            { 0.2f, false },
            { 0.3f, false },
            { 0.4f, false },
            { 0.5f, false },
            { 0.6f, false },
            { 0.7f, false },
            { 0.8f, false },
            { 0.9f, false },
            { 1.06f, false }
        };
    }

    private void Update()
    {
        //TEMPORARY
        /*
        if (Input.GetKeyDown(KeyCode.R))
        {
            GetComponent<ForkliftControl>().MoveLiftToHeight(1.27f);
        }
        */

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            EndEpisode();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            GetComponent<ModelSwitcher>().SetModel();
        }

        if (hasLiftedPallet && !isTouchingPallet)
        {
            RewardAgent(-300f);
            //EndEpisode();
            //Debug.Log("PACKAGE LEFT");
            return;
        }
        if (package.transform.localPosition.y < -0.4f || pallet.transform.localPosition.y < -0.5f)
        {
            RewardAgent(-300f);
            buttons.GetComponent<CameraSwitcher>().ChangeButtonSprite(2);
            //EndEpisode();
            //Debug.Log("PACKAGE TOUCHED GROUND");
            return;
        }

        if (!palletTouchedWall && (package.GetComponent<Package01>().IsTouchingWall() || pallet.GetComponent<Pallet>().IsTouchingWall()))
        {
            RewardAgent(-270f);
            palletTouchedWall = true;
            //Debug.Log("PACKAGE TOUCHED WALL");
            //EndEpisode();
            return;
        }

        Vector3 forward = transform.forward;
        float angle = Mathf.Abs(Vector3.SignedAngle(forward, pallet.transform.forward, Vector3.up) + 90);
        if (angle > 180) angle = 360 - angle;
        //Debug.Log("Angle: " + angle);

        //Daca un punct a fost colectat se verifica orientarea
        if (hasCollectedPoint && angle < minAlignmentAchieved)
        {
            minAlignmentAchieved = angle;
            lastMinAlignmentAchieved = angle;
            foreach (var bin in alignmentBins.Keys.OrderBy(k => k))
            {
                if (angle <= bin && !alignmentBins[bin])
                {
                    RewardAgent(0.5f);
                    alignmentBins[bin] = true;
                    achievedAlignmentBins++;
                    //Debug.Log("ALIGNMENT ");
                    //Debug.Log(bin);
                    break;
                }
            }
        }
        else if (achievedAlignmentBins >= minAchievedBins)
        {
            if (angle > lastMinAlignmentAchieved + alignmentTolerance && !hasExceededAlignment)
            {
                RewardAgent(-2f);
                hasExceededAlignment = true;
                //Debug.Log("Alignment Increased");
            }
            else if (angle <= lastMinAlignmentAchieved + alignmentTolerance && hasExceededAlignment)
            {
                RewardAgent(2f);
                hasExceededAlignment = false;
                //Debug.Log("Alignment Corrected");
            }
        }

        float angle2 = Mathf.Abs(Vector3.SignedAngle(forward, CheckpointPackage.transform.forward, Vector3.up));
        angle2 = 180 - angle2;
        if (angle2 > 180) angle2 = 360 - angle2;
        //Debug.Log("Angle2: " + angle2);

        if (angle2 < minAlignmentPackageAchieved && hasLiftedPallet)
        {
            minAlignmentPackageAchieved = angle2;
            lastMinAlignmentPackageAchieved = angle2;
            foreach (var bin in alignmentBins.Keys.OrderBy(k => k))
            {
                if (angle2 <= bin && !alignmentPackageBins[bin])
                {
                    RewardAgent(7f);
                    alignmentPackageBins[bin] = true;
                    achievedAlignmentPackageBins++;
                    //Debug.Log("ALIGNMENT PACKAGE");
                    //Debug.Log(bin);
                    break;
                }
            }
        }

        //Penalizari reversibile
        else if (achievedAlignmentPackageBins >= minAchievedBins)
        {
            if (angle2 > lastMinAlignmentPackageAchieved + alignmentTolerance && !hasExceededAlignmentPackage)
            {
                RewardAgent(-20f);
                hasExceededAlignmentPackage = true;
            }
            else if (angle2 <= lastMinAlignmentPackageAchieved + alignmentTolerance && hasExceededAlignmentPackage)
            {
                RewardAgent(20f);
                hasExceededAlignmentPackage = false;
            }
        }

        if (angle2 > maxAlignmentPackageAchieved && hasLiftedPallet)
        {
            maxAlignmentPackageAchieved = angle2;
            lastMaxAlignmentPackageAchieved = angle2;
            foreach (var bin in alignmentPackageIncreaseBins.Keys.OrderBy(k => k))
            {
                if (angle2 >= bin && !alignmentPackageIncreaseBins[bin])
                {
                    RewardAgent(-2f);
                    alignmentPackageIncreaseBins[bin] = true;
                    achievedAlignmentPackageIncreaseBins++;
                    //Debug.Log("ALIGNMENT PACKAGE INCREASE");
                    //Debug.Log(bin);
                    break;
                }
            }
        }
        else if (achievedAlignmentPackageIncreaseBins >= minAchievedBins)
        {
            if (angle2 < lastMaxAlignmentPackageAchieved - alignmentTolerance && !hasExceededAlignmentPackageIncrease)
            {
                RewardAgent(6f);
                hasExceededAlignmentPackageIncrease = true;
            }
            else if (angle2 >= lastMaxAlignmentPackageAchieved - alignmentTolerance && hasExceededAlignmentPackageIncrease)
            {
                RewardAgent(-6f);
                hasExceededAlignmentPackageIncrease = false;
            }
        }

        float distanceToPackage = Vector3.Distance(transform.position, package.transform.position);
        if (distanceToPackage < minDistanceAchieved)
        {
            minDistanceAchieved = distanceToPackage;
            lastMinDistanceAchieved = distanceToPackage;
            foreach (var bin in distanceBins.Keys.OrderBy(k => k))
            {
                if (distanceToPackage <= bin && !distanceBins[bin])
                {
                    RewardAgent(0.5f);
                    distanceBins[bin] = true;
                    achievedDistanceBins++;
                    //Debug.Log("DISTANCE ");
                    //Debug.Log(bin);

                    if (bin == 5f)
                    {
                        MaskLiftActions();
                        GetComponent<ForkliftControl>().MoveLiftToHeight(1.11f);
                        Debug.Log("MASK");
                    }
                    break;
                }
            }
        }
        else if (achievedDistanceBins >= minAchievedBins)
        {
            if (distanceToPackage > lastMinDistanceAchieved + distanceTolerance && !hasExceededDistance)
            {
                RewardAgent(-2f);
                hasExceededDistance = true;
            }
            else if (distanceToPackage <= lastMinDistanceAchieved + distanceTolerance && hasExceededDistance)
            {
                RewardAgent(2f);
                hasExceededDistance = false;
            }
        }

        float liftHeight = lift.transform.localPosition.y;
        if (liftHeight > 1.095f && !hasExceededLiftHeight)
        {
            if (!isTouchingPallet)
            {
                RewardAgent(-40f);
                hasExceededLiftHeight = true;
                //Debug.Log("LIFT HEIGHT EXCEEDED");
            }
            else if (!hasLiftedPallet && liftHeight > 1.20f)
            {
                RewardAgent(120f);
                hasLiftedPallet = true;
                WallsTask02.ForEach(wall => wall.SetActive(false));

                foreach (Transform point in pointsReverse.transform)
                {
                    point.gameObject.SetActive(true);
                }

                buttons.GetComponent<CameraSwitcher>().ChangeButtonSprite(1);
                //Debug.Log("LIFTED PALLET!!!!!!!!");
            }
        }
        else if (liftHeight < 1.09f && hasExceededLiftHeight)
        {
            RewardAgent(7f);
            hasExceededLiftHeight = false;
        }

        if (liftHeight > 1.38f && hasLiftedPallet)
        {
            RewardAgent(-80f);
            //EndEpisode();
        }

        if (liftHeight > maxLiftHeight)
        {
            maxLiftHeight = liftHeight;
            foreach (var bin in liftBins.Keys.OrderBy(k => k))
            {
                if (liftHeight >= bin && !liftBins[bin])
                {
                    RewardAgent(0.5f);
                    liftBins[bin] = true;
                    //Debug.Log("LIFT ");
                    //Debug.Log(bin);
                    break;
                }
            }
        }

        if (hasLiftedPallet)
        {
            float distanceToCheckpoint = Vector3.Distance(transform.position, CheckpointPackage.transform.position);
            if (distanceToCheckpoint < minCheckpointDistanceAchieved)
            {
                minCheckpointDistanceAchieved = distanceToCheckpoint;
                foreach (var bin in checkpointDistanceBins.Keys.OrderBy(k => k))
                {
                    if (distanceToCheckpoint <= bin && !checkpointDistanceBins[bin])
                    {
                        if (angle2 < 90)
                        {
                            RewardAgent(50f);
                            checkpointDistanceBins[bin] = true;
                            //Debug.Log("Checkpoint Distance Achieved: " + bin);
                            break;
                        }
                        else
                        {
                            RewardAgent(-50f);
                            checkpointDistanceBins[bin] = true;
                            //Debug.Log("Checkpoint NEGATIVE Distance Achieved: " + bin);
                            break;
                        }
                    }
                }
            }
        }

        //Debug.Log(angle);
        //Debug.Log(distanceToPackage);
    }

    private void ResetPalletPositions()
    {
        if (shelvesParent == null)
        {
            return;
        }

        foreach (Transform shelf in shelvesParent.transform)
        {
            PackageActivator activator = shelf.GetComponentInChildren<PackageActivator>();
            if (activator != null)
            {
                activator.ResetPalletPositions();
            }
        }
    }

    public int ActivateSequentialPallet()
    {
        Transform selectedShelf = shelvesParent.transform.GetChild(currentShelfIndex);
        PackageActivator activator = selectedShelf.GetComponentInChildren<PackageActivator>();

        if (activator != null)
        {
            if (currentItemIndex < activator.itemPositions.Count)
            {
                activator.ActivatePackage(currentItemIndex);

                Transform selectedItem = activator.itemsContainer.transform.GetChild(currentItemIndex);
                if (selectedItem != null)
                {
                    Transform palletChild = selectedItem.Find("Pallet");
                    Transform packageChild = selectedItem.Find("Cardboard_A");
                    Transform pointsChild = palletChild.Find("Points");
                    Transform pointsReverseChild = selectedItem.Find("PointsReverse");

                    if (palletChild != null && packageChild != null && pointsChild != null)
                    {
                        pallet = palletChild.gameObject;
                        package = packageChild.gameObject;
                        points = pointsChild.gameObject;
                        pointsReverse = pointsReverseChild.gameObject;

                        foreach (Transform point in points.transform)
                        {
                            point.gameObject.SetActive(true);
                        }
                        foreach (Transform point in pointsReverse.transform)
                        {
                            point.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }

        //Actualizare indice secvential
        int lastShelfIndex = currentShelfIndex;
        if (currentItemIndex >= activator.itemPositions.Count)
        {
            currentItemIndex = 0;
            currentShelfIndex++;
            if (currentShelfIndex >= shelvesParent.transform.childCount)
            {
                currentShelfIndex = 0;
            }
        }
        return lastShelfIndex;
    }

    private int ActivateRandomPallet()
    {
        int randomShelfIndex = Random.Range(0, 8);
        if (randomShelfIndex < 4)
        {
            side = true;
        }
        else
        {
            side = false;
        }

        Transform selectedShelf = shelvesParent.transform.GetChild(randomShelfIndex);
        PackageActivator activator = selectedShelf.GetComponentInChildren<PackageActivator>();

        if (activator != null)
        {
            int randomItemIndex = Random.Range(0, activator.itemPositions.Count);

            activator.ActivatePackage(randomItemIndex);

            Transform selectedItem = activator.itemsContainer.transform.GetChild(randomItemIndex);
            if (selectedItem != null)
            {
                Transform palletChild = selectedItem.Find("Pallet");
                Transform packageChild = selectedItem.Find("Cardboard_A");
                Transform pointsChild = palletChild.Find("Points");
                Transform pointsReverseChild = selectedItem.Find("PointsReverse");

                if (palletChild != null && packageChild != null && pointsChild != null)
                {
                    pallet = palletChild.gameObject;
                    package = packageChild.gameObject;
                    points = pointsChild.gameObject;
                    pointsReverse = pointsReverseChild.gameObject;

                    foreach (Transform point in points.transform)
                    {
                        point.gameObject.SetActive(true);
                    }
                    foreach (Transform point in pointsReverse.transform)
                    {
                        point.gameObject.SetActive(false);
                    }
                }
            }
        }
        return randomShelfIndex;
    }

    public override void OnEpisodeBegin()
    {
        episodeEnded = false;
        iterations++;

        UnmaskLiftActions();
        GetComponent<ModelSwitcher>().ResetModel();
        buttons.GetComponent<CameraSwitcher>().ChangeButtonSprite(0);

        pointsTouched = 0;
        rewards = 0;
        hasCollectedPoint = false;
        palletTouchedWall = false;

        gameObject.GetComponent<ForkliftControl>().StopForkliftImmediately();
        gameObject.GetComponent<ForkliftControl>().SetSteeringAngle(0);
        gameObject.GetComponent<ForkliftControl>().ResetLift();
        isTouchingPallet = false;
        hasExceededLiftHeight = false;
        hasLiftedPallet = false;
        hasExceededDistance = false;
        hasExceededAlignment = false;
        hasExceededAlignmentPackage = false;
        achievedDistanceBins = 0;
        achievedAlignmentBins = 0;
        achievedAlignmentPackageBins = 0;

        maxAlignmentPackageAchieved = float.MinValue;
        lastMaxAlignmentPackageAchieved = float.MinValue;
        achievedAlignmentPackageIncreaseBins = 0;
        hasExceededAlignmentPackageIncrease = false;

        //TEMPORARY
        //GetComponent<ForkliftControl>().MoveLiftToHeight(1.11f);

        if (package != null && package.GetComponent<Package01>() != null)
        {
            package.GetComponent<Package01>().setIsTouchingWall(false);
        }

        if (pallet != null && pallet.GetComponent<Pallet>() != null)
        {
            pallet.GetComponent<Pallet>().setIsTouchingWall(false);
        }

        foreach (var key in alignmentBins.Keys.ToList())
        {
            alignmentBins[key] = false;
        }

        foreach (var key in alignmentPackageBins.Keys.ToList())
        {
            alignmentPackageBins[key] = false;
        }

        foreach (var key in alignmentPackageIncreaseBins.Keys.ToList())
        {
            alignmentPackageIncreaseBins[key] = false;
        }

        foreach (var key in distanceBins.Keys.ToList())
        {
            distanceBins[key] = false;
        }

        foreach (var key in liftBins.Keys.ToList())
        {
            liftBins[key] = false;
        }

        minAlignmentAchieved = float.MaxValue;
        minAlignmentPackageAchieved = float.MaxValue;
        minDistanceAchieved = float.MaxValue;
        maxLiftHeight = float.MinValue;
        lastMinAlignmentAchieved = float.MaxValue;
        lastMinAlignmentPackageAchieved = float.MaxValue;
        lastMinDistanceAchieved = float.MaxValue;

        //Resetare checkpoints
        foreach (var checkpoint in checkpoints)
        {
            checkpointTouched[checkpoint] = false;
        }
        nextCheckpointIndex = 0;

        ResetPalletPositions();
        float shelfIndex = ActivateRandomPallet();
        int spawnIndex = 2;

        if (shelfIndex < 2)
        {
            spawnIndex = 0;
        }
        else if (shelfIndex < 6)
        {
            spawnIndex = 1;
        }

        /*
        Vector3 randomOffset = new Vector3(Random.Range(-2.5f, 2.5f), 0, 0);
        transform.localPosition = spawnPositions[spawnIndex].localPosition + randomOffset;

        float randomRotation = Random.Range(-25f, 25f);
        transform.forward = Quaternion.Euler(0, randomRotation, 0) * spawnPositions[spawnIndex].forward;
        //transform.forward = spawnPositions[spawnIndex].forward;
        */

        transform.localPosition = spawnPosition.localPosition;
        transform.forward = spawnPosition.forward;

        CheckpointPackage = checkpointsPackage[spawnIndex];
        CheckpointChange = checkpointsChange[spawnIndex];
        WallTask02 = WallsTask02[spawnIndex];
        WallTask02.SetActive(true);

        //Oprire pachet si paleta pentru resetare
        if (package != null && package.GetComponent<Rigidbody>() != null)
        {
            var packageRb = package.GetComponent<Rigidbody>();
            packageRb.velocity = Vector3.zero;
            packageRb.angularVelocity = Vector3.zero;
        }

        if (pallet != null && pallet.GetComponent<Rigidbody>() != null)
        {
            var palletRb = pallet.GetComponent<Rigidbody>();
            palletRb.velocity = Vector3.zero;
            palletRb.angularVelocity = Vector3.zero;
        }

        foreach (var key in checkpointDistanceBins.Keys.ToList())
        {
            checkpointDistanceBins[key] = false;
        }

        //Bins dinamice modificate in functie de distanta de la paleta la primul checkpoint
        float initialDistanceToCheckpoint = Vector3.Distance(pallet.transform.position, CheckpointPackage.transform.position);
        float binStep = initialDistanceToCheckpoint / 26;
        checkpointDistanceBins.Clear();
        for (float i = binStep; i < initialDistanceToCheckpoint; i += binStep)
        {
            checkpointDistanceBins[i] = false;
        }
        minCheckpointDistanceAchieved = initialDistanceToCheckpoint / 1.01f;

        Debug.Log("==========");
        Debug.Log(wins);
        Debug.Log("/");
        Debug.Log(iterations);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(lift.transform.localPosition.y);
        sensor.AddObservation(GetComponent<ForkliftControl>().GetSteeringAngle());
        sensor.AddObservation(GetComponent<ForkliftControl>().GetCurrentSpeed());

        sensor.AddObservation(Vector3.Dot(transform.forward, checkpoints[nextCheckpointIndex].transform.forward));
        if (GetComponent<ModelSwitcher>().isNavigation())
        {
            sensor.AddObservation(Vector3.Dot(transform.forward, NavPallet.transform.forward));
            //Debug.Log(Vector3.Dot(transform.forward, NavPallet.transform.forward));
        }
        else
        {
            sensor.AddObservation(Vector3.Dot(transform.forward, pallet.transform.forward));
            //Debug.Log(Vector3.Dot(transform.forward, pallet.transform.forward));
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float forwardAmount = 0f;
        float turnAmount = 0f;
        float liftAmount = 0f;
        float brakeAmount = 0f;

        switch (actions.DiscreteActions[0])
        {
            case 0: forwardAmount = 0f; break;
            case 1: forwardAmount = 1f; break;
            case 2: forwardAmount = -1f; break;
        }

        switch (actions.DiscreteActions[1])
        {
            case 0: turnAmount = 0f; break;
            case 1: turnAmount = 1f; break;
            case 2: turnAmount = -1f; break;
        }

        switch (actions.DiscreteActions[2])
        {
            case 0: liftAmount = 0f; break;
            case 1: liftAmount = 1f; break;
            case 2: liftAmount = -1f; break;
        }

        switch (actions.DiscreteActions[3])
        {
            case 0: brakeAmount = 0f; break;
            case 1: brakeAmount = 1f; break;
        }

        gameObject.GetComponent<ForkliftControl>().SetInputs(forwardAmount, turnAmount, liftAmount, brakeAmount);

        /*
        if (forwardAmount > 0)
        {
            RewardAgent(0.1f);
        }
        else if (forwardAmount < 0)
        {
            RewardAgent(-0.1f);
        }
        */

        if (StepCount >= MaxStep)
        {
            RewardAgent(-10f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        int turnAction = 0;
        if (Input.GetAxisRaw("Horizontal") == 1f) turnAction = 1;
        if (Input.GetAxisRaw("Horizontal") == -1f) turnAction = 2;

        int forwardAction = 0;
        if (Input.GetAxisRaw("Vertical") == 1f) forwardAction = 1;
        if (Input.GetAxisRaw("Vertical") == -1f) forwardAction = 2;

        int liftAction = 0;
        if (Input.GetKey(KeyCode.Q) || gameObject.GetComponent<ForkliftControl>().liftRaise == true) liftAction = 1;
        if (Input.GetKey(KeyCode.E)) liftAction = 2;

        int brakeAction = 0;
        if (Input.GetKey(KeyCode.Space)) brakeAction = 1;

        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = forwardAction;
        discreteActions[1] = turnAction;
        discreteActions[2] = liftAction;
        discreteActions[3] = brakeAction;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == pallet)
        {
            isTouchingPallet = true;
        }
        if (collision.gameObject.TryGetComponent<Wall>(out Wall wall))
        {
            RewardAgent(-10.5f);
        }
        else if (collision.gameObject.TryGetComponent<Shelf>(out Shelf shelf))
        {
            RewardAgent(-10.5f);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject == pallet)
        {
            isTouchingPallet = true;
        }
        if (collision.gameObject.TryGetComponent<Wall>(out Wall wall))
        {
            RewardAgent(-.3f);
            //Debug.Log("W");
        }
        else if (collision.gameObject.TryGetComponent<Shelf>(out Shelf shelf))
        {
            RewardAgent(-.3f);
            //Debug.Log("S");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (episodeEnded)
        {
            //Daca episodul s-a terminat deja return
            return;
        }

        if (other.gameObject == CheckpointPackage)
        {
            pallet.layer = LayerMask.NameToLayer("Default");
            package.layer = LayerMask.NameToLayer("Default");
            if (hasLiftedPallet)
            {
                RewardAgent(2000f);
                episodeEnded = true;
                Debug.Log("WIN!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                wins += 1;
                //StartCoroutine(EndEpisodeWithDelay());
                return;
            }
            else
            {
                RewardAgent(-80f);
                episodeEnded = true;
                Debug.Log("LOSS??????????????????????????");
                //StartCoroutine(EndEpisodeWithDelay());
                return;
            }
        }

        if (other.gameObject.TryGetComponent<Point>(out Point point))
        {
            ++pointsTouched;
            hasCollectedPoint = true;
            RewardAgent(30f);
            point.gameObject.SetActive(false);

            if (pointsTouched == 10)
            {
                UnmaskLiftActions();
                Debug.Log("UNMASK");
            }
        }
        else if (other.gameObject.TryGetComponent<ReversePoint>(out ReversePoint reversePoint))
        {
            ++pointsTouched;
            hasCollectedPoint = true;
            RewardAgent(5f);
            reversePoint.gameObject.SetActive(false);
        }
        else if (other.gameObject.TryGetComponent<PointEnd>(out PointEnd pointEnd))
        {
            pointEnd.gameObject.SetActive(false);
            //GetComponent<ForkliftControl>().MoveLiftToHeight(1.25f);
        }

        if (checkpoints.Contains(other.gameObject))
        {
            int checkpointIndex = checkpoints.IndexOf(other.gameObject);
            if (!checkpointTouched[other.gameObject])
            {
                if (checkpointIndex >= nextCheckpointIndex)
                {
                    float enterDegree = Vector3.Dot(transform.forward, other.transform.forward);
                    float rewardMultiplier = -enterDegree;

                    RewardAgent(1.0f * rewardMultiplier);

                    for (int i = 0; i < checkpointIndex; i++)
                    {
                        checkpointTouched[checkpoints[i]] = true;
                    }
                    nextCheckpointIndex = (checkpointIndex + 1) % checkpoints.Count;

                    if (nextCheckpointIndex == 0)
                    {
                        RewardAgent(5f * rewardMultiplier);
                        episodeEnded = true;
                        //Debug.Log("Next checkpoint!");
                        //EndEpisode();
                    }

                    if (lift.transform.localPosition.y <= -0.1f)
                    {
                        RewardAgent(0.5f * rewardMultiplier);
                    }
                    else
                    {
                        RewardAgent(-0.5f * rewardMultiplier);
                    }
                }
            }
            else if (checkpointIndex < nextCheckpointIndex)
            {
                RewardAgent(-0.5f);
            }
        }
    }

    private IEnumerator EndEpisodeWithDelay()
    {
        yield return new WaitForSeconds(0.1f);
        EndEpisode();
    }
}
