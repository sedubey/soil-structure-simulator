using UnityEngine;
using System.Collections;

public class FoundationController : MonoBehaviour
{
    public GameObject Foundation;
    public GameObject Sand_M;
    public GameObject Scenario_1;

    [Header("Parameters")]
    public int deg = 26;
    public double c = 0.0;
    public double y = 19.0;
    public double b = 2.0;
    public double q = 1000.0;
    public double Cu = 40.0;
    public double Sc = 1.3;
    
    [Header("Footing Type")]
    public bool useSquareFooting = true;

    [Header("Calculated Result")]
    [SerializeField] private double qu = 500;
    public double Qu 
    { 
        get { return qu; }
        private set { qu = value; }
    }

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float scaleSpeed = 2f;

    private Vector3 foundationStartPosition;
    private Vector3 sandStartPosition;
    private Vector3 sandStartScale;
    private bool hasCachedStartState;

    void Start()
    {
        Debug.Log("=== FoundationController started on GameObject: " + gameObject.name + " ===");
        CacheStartState();
        ResetVisuals();
        CalculateBearingCapacity();
        Debug.Log("=== Initial Qu = " + Qu + " ===");
    }

    void Update()
    {
        // Press F to test failure indicator
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("=== MANUAL TEST: Showing failure indicator ===");
            InteractiveShallowFoundationPopup.NotifyCalculationFailed();
        }
        
        // Press G to force a failure
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("=== MANUAL TEST: Forcing foundation failure ===");
            q = 2000;
            Qu = 500;
            RunSimulation();
        }
        
        // Press 1 for pass scenario
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("=== KEY 1: Running PASS scenario ===");
            RunScenario(0);
        }
        
        // Press 2 for fail scenario
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("=== KEY 2: Running FAIL scenario ===");
            RunScenario(1);
        }
        
        // Press 3 for interactive scenario
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("=== KEY 3: Running INTERACTIVE scenario ===");
            RunScenario(2);
        }
    }

    public void ApplyParametersAndRun(
        int degree,
        double cohesion,
        double unitWeight,
        double width,
        double appliedLoad,
        double undrainedShearStrength,
        double shapeFactor,
        bool squareFooting)
    {
        Debug.Log("=== ApplyParametersAndRun CALLED ===");
        Debug.Log($"Parameters: deg={degree}, c={cohesion}, y={unitWeight}, b={width}, q={appliedLoad}, Cu={undrainedShearStrength}, Sc={shapeFactor}, Square={squareFooting}");
        
        deg = degree;
        c = cohesion;
        y = unitWeight;
        b = width;
        q = appliedLoad;
        Cu = undrainedShearStrength;
        Sc = shapeFactor;
        useSquareFooting = squareFooting;

        CalculateBearingCapacity();
        Debug.Log($"After calculation: Qu={Qu}, q={q}, Will fail: {q > Qu}");
        RunSimulation();
    }

    public void RunScenario(int mode)
    {
        Debug.Log("=== RunScenario CALLED with mode: " + mode + " on GameObject: " + gameObject.name + " ===");
        CacheStartState();
        ResetVisuals();

        if (mode == 0) // Pass
        {
            q = 300;
            Qu = 500;
            Debug.Log("PASS MODE: Set q=300, Qu=500, q > Qu = " + (q > Qu));
        }
        else if (mode == 1) // Fail
        {
            q = 1000;
            Qu = 500;
            Debug.Log("FAIL MODE: Set q=1000, Qu=500, q > Qu = " + (q > Qu));
        }
        else if (mode == 2) // Interactive
        {
            Debug.Log("INTERACTIVE MODE: Using current values, recalculating Qu");
            CalculateBearingCapacity();
            Debug.Log($"After calculation: Qu={Qu}, q={q}, Will fail: {q > Qu}");
        }

        RunSimulation();
    }

    private void CalculateBearingCapacity()
    {
        Debug.Log($"--- CalculateBearingCapacity: deg={deg}, Square={useSquareFooting} ---");
        
        if (!ShallowFoundation.GetNum(deg, out double Nc, out double Nq, out double Ny))
        {
            Debug.LogError($"INVALID DEGREE: {deg}°");
            return;
        }

        Debug.Log($"Factors: Nc={Nc}, Nq={Nq}, Ny={Ny}");

        if (useSquareFooting)
        {
            if (Cu > 0 && c <= 0)
            {
                Qu = ShallowFoundation.SquareFootingUndrained(Cu, Sc, q);
                Debug.Log($"Square Undrained: Qu = 5.7 * {Cu} * {Sc} + {q} = {Qu}");
            }
            else
            {
                Qu = ShallowFoundation.SquareFootingDrained(deg, c, Sc, q, y, b, true);
                Debug.Log($"Square Drained: Qu = {Qu}");
            }
        }
        else
        {
            if (Cu > 0 && c <= 0)
            {
                Qu = ShallowFoundation.StripFootingUndrained(Cu, q);
                Debug.Log($"Strip Undrained: Qu = 5.7 * {Cu} + {q} = {Qu}");
            }
            else
            {
                Qu = ShallowFoundation.StripFootingDrained(deg, c, q, y, b);
                Debug.Log($"Strip Drained: Qu = {Qu}");
            }
        }
        
        Debug.Log($"--- Final Qu = {Qu} ---");
    }

    public void RunSimulation()
    {
        Debug.Log("=== RunSimulation START ===");
        CacheStartState();
        ResetVisuals();

        Debug.Log($"FINAL CHECK: Qu (capacity) = {Qu:F2}, q (load) = {q:F2}");
        Debug.Log($"FAILURE CHECK: Is {q:F2} > {Qu:F2}? {q > Qu}");

        if (q > Qu)
        {
            Debug.Log("!!! FOUNDATION FAILURE - Starting failure animation !!!");
            
            InteractiveShallowFoundationPopup.NotifyCalculationFailed();
            
            Vector3 sandTargetPos = sandStartPosition + new Vector3(0f, -0.1f, 0f);
            Vector3 sandTargetScale = sandStartScale + new Vector3(0f, -0.2f, 0f);
            Vector3 foundationTargetPos = foundationStartPosition + new Vector3(0f, -0.3f, 0f);

            if (Scenario_1 != null)
            {
                Scenario_1.SetActive(true);
                Debug.Log("Activated Scenario_1");
            }
            else
            {
                Debug.LogWarning("Scenario_1 is null!");
            }

            if (Sand_M != null)
            {
                StartCoroutine(MoveScale(Sand_M.transform, sandTargetPos, sandTargetScale));
                Debug.Log("Started Sand_M movement");
            }
            else
            {
                Debug.LogWarning("Sand_M is null!");
            }

            if (Foundation != null)
            {
                StartCoroutine(Move(Foundation.transform, foundationTargetPos));
                Debug.Log("Started Foundation movement");
            }
            else
            {
                Debug.LogWarning("Foundation is null!");
            }
        }
        else
        {
            Debug.Log("Foundation is STABLE - No failure");
        }
        
        Debug.Log("=== RunSimulation END ===");
    }

    private void CacheStartState()
    {
        if (hasCachedStartState)
        {
            return;
        }

        if (Foundation != null)
        {
            foundationStartPosition = Foundation.transform.position;
        }

        if (Sand_M != null)
        {
            sandStartPosition = Sand_M.transform.position;
            sandStartScale = Sand_M.transform.localScale;
        }

        hasCachedStartState = true;
        Debug.Log("Cached start positions");
    }

    public void ResetVisuals()
    {
        StopAllCoroutines();

        if (Foundation != null)
        {
            Foundation.transform.position = foundationStartPosition;
        }

        if (Sand_M != null)
        {
            Sand_M.transform.position = sandStartPosition;
            Sand_M.transform.localScale = sandStartScale;
        }

        if (Scenario_1 != null)
        {
            Scenario_1.SetActive(false);
        }
    }

    IEnumerator Move(Transform obj, Vector3 target)
    {
        while (Vector3.Distance(obj.position, target) > 0.01f)
        {
            obj.position = Vector3.MoveTowards(obj.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        obj.position = target;
    }

    IEnumerator MoveScale(Transform obj, Vector3 targetPos, Vector3 targetScale)
    {
        while (Vector3.Distance(obj.position, targetPos) > 0.01f || Vector3.Distance(obj.localScale, targetScale) > 0.01f)
        {
            obj.position = Vector3.MoveTowards(obj.position, targetPos, moveSpeed * Time.deltaTime);
            obj.localScale = Vector3.MoveTowards(obj.localScale, targetScale, scaleSpeed * Time.deltaTime);
            yield return null;
        }

        obj.position = targetPos;
        obj.localScale = targetScale;
    }
}