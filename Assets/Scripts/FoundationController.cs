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
    public double q = 1000.0;       // Applied load on foundation
    public double Cu = 40.0;
    public double Sc = 1.3;
    public double overburdenPressure = 0.0;  // Overburden pressure (usually y * depth, 0 for surface)
    
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
        Debug.Log("=== FoundationController started on: " + gameObject.name + " ===");
        CacheStartState();
        ResetVisuals();
        CalculateBearingCapacity();
        Debug.Log("=== Initial: Qu=" + Qu + ", Applied load q=" + q + ", Will fail: " + (q > Qu) + " ===");
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
        Debug.Log("=== APPLY PARAMETERS ===");
        Debug.Log("Received: deg=" + degree + ", c=" + cohesion + ", y=" + unitWeight + ", b=" + width + ", q=" + appliedLoad + ", Cu=" + undrainedShearStrength + ", Sc=" + shapeFactor + ", Square=" + squareFooting);
        
        deg = degree;
        c = cohesion;
        y = unitWeight;
        b = width;
        q = appliedLoad;
        Cu = undrainedShearStrength;
        Sc = shapeFactor;
        useSquareFooting = squareFooting;
        overburdenPressure = 0;  // Surface foundation, no overburden

        CalculateBearingCapacity();
        Debug.Log("After calculation: Qu=" + Qu + ", Applied load q=" + q + ", FAIL = " + (q > Qu));
        
        RunSimulation();
    }

    public void RunScenario(int mode)
    {
        Debug.Log("=== RunScenario mode=" + mode + " ===");
        CacheStartState();
        ResetVisuals();

        if (mode == 0) // Pass
        {
            q = 300;
            Qu = 500;
            Debug.Log("PASS: q=" + q + ", Qu=" + Qu);
        }
        else if (mode == 1) // Fail
        {
            q = 1000;
            Qu = 500;
            Debug.Log("FAIL: q=" + q + ", Qu=" + Qu);
        }
        else if (mode == 2) // Interactive
        {
            overburdenPressure = 0;
            CalculateBearingCapacity();
            Debug.Log("INTERACTIVE: Qu=" + Qu + ", q=" + q);
        }

        RunSimulation();
    }

    private void CalculateBearingCapacity()
    {
        Debug.Log("--- CalculateBearingCapacity ---");
        Debug.Log("Input: deg=" + deg + ", c=" + c + ", y=" + y + ", b=" + b + ", Cu=" + Cu + ", Sc=" + Sc + ", Square=" + useSquareFooting + ", overburden=" + overburdenPressure);
        
        if (!ShallowFoundation.GetNum(deg, out double Nc, out double Nq, out double Ny))
        {
            Debug.LogError("INVALID DEGREE: " + deg);
            qu = 0;
            return;
        }

        Debug.Log("Factors: Nc=" + Nc + ", Nq=" + Nq + ", Ny=" + Ny);

        if (useSquareFooting)
        {
            if (Cu > 0 && c <= 0)
            {
                qu = ShallowFoundation.SquareFootingUndrained(Cu, Sc);
                Debug.Log("Square Undrained: Qu = 5.7 * " + Cu + " * " + Sc + " = " + qu);
            }
            else
            {
                qu = ShallowFoundation.SquareFootingDrained(deg, c, Sc, overburdenPressure, y, b, true);
                Debug.Log("Square Drained: Qu = " + c + "*" + Nc + "*" + Sc + " + " + overburdenPressure + "*" + Nq + " + 0.5*" + y + "*" + b + "*" + Ny + "*0.6 = " + qu);
            }
        }
        else
        {
            if (Cu > 0 && c <= 0)
            {
                qu = ShallowFoundation.StripFootingUndrained(Cu);
                Debug.Log("Strip Undrained: Qu = 5.7 * " + Cu + " = " + qu);
            }
            else
            {
                qu = ShallowFoundation.StripFootingDrained(deg, c, overburdenPressure, y, b);
                Debug.Log("Strip Drained: Qu = " + c + "*" + Nc + " + " + overburdenPressure + "*" + Nq + " + 0.5*" + y + "*" + b + "*" + Ny + " = " + qu);
            }
        }
        
        Debug.Log("Calculated Qu = " + qu + " (capacity), Applied q = " + q + " (load)");
    }

    public void RunSimulation()
    {
        Debug.Log("=== RUN SIMULATION ===");
        CacheStartState();
        ResetVisuals();

        Debug.Log("Qu (capacity) = " + qu + ", q (applied load) = " + q);
        Debug.Log("FAILURE CHECK: " + q + " > " + qu + " = " + (q > qu));

        if (q > qu)
        {
            Debug.Log("!!! FOUNDATION FAILS !!!");
            
            InteractiveShallowFoundationPopup.NotifyCalculationFailed();
            
            Vector3 sandTargetPos = sandStartPosition + new Vector3(0f, -0.1f, 0f);
            Vector3 sandTargetScale = sandStartScale + new Vector3(0f, -0.2f, 0f);
            Vector3 foundationTargetPos = foundationStartPosition + new Vector3(0f, -0.3f, 0f);

            if (Scenario_1 != null) Scenario_1.SetActive(true);
            if (Sand_M != null) StartCoroutine(MoveScale(Sand_M.transform, sandTargetPos, sandTargetScale));
            if (Foundation != null) StartCoroutine(Move(Foundation.transform, foundationTargetPos));
        }
        else
            {
            Debug.Log("Foundation is STABLE");
            InteractiveShallowFoundationPopup.NotifyCalculationPassed(); // Add this line
        }
    }

    private void CacheStartState()
    {
        if (hasCachedStartState) return;

        if (Foundation != null) foundationStartPosition = Foundation.transform.position;
        if (Sand_M != null)
        {
            sandStartPosition = Sand_M.transform.position;
            sandStartScale = Sand_M.transform.localScale;
        }
        hasCachedStartState = true;
    }

    public void ResetVisuals()
    {
        StopAllCoroutines();

        if (Foundation != null) Foundation.transform.position = foundationStartPosition;
        if (Sand_M != null)
        {
            Sand_M.transform.position = sandStartPosition;
            Sand_M.transform.localScale = sandStartScale;
        }
        if (Scenario_1 != null) Scenario_1.SetActive(false);
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