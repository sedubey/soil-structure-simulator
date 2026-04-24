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

    // Qu is now read-only and calculated, not set manually
    [Header("Calculated Result")]
    [SerializeField] private double qu = 500; // Renamed to lowercase for consistency
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
        Debug.Log("Controller running");
        CacheStartState();
        ResetVisuals();
        CalculateBearingCapacity(); // Calculate initial Qu
    }

    public void ApplyParametersAndRun(
        int degree,
        double cohesion,
        double unitWeight,
        double width,
        double appliedLoad,
        double undrainedShearStrength,
        double shapeFactor)
    {
        deg = degree;
        c = cohesion;
        y = unitWeight;
        b = width;
        q = appliedLoad;
        Cu = undrainedShearStrength;
        Sc = shapeFactor;

        // Calculate Qu based on the parameters
        if (!CalculateBearingCapacity())
        {
            // If calculation failed (invalid degree), notify the popup
            InteractiveShallowFoundationPopup.NotifyCalculationFailed();
            Debug.LogError("Failed to calculate bearing capacity. Invalid degree: " + deg);
            return;
        }

        RunScenario(2);
    }

    public void RunScenario(int mode)
    {
        CacheStartState();

        if (mode == 0) // Pass
        {
            q = 300;
        }
        else if (mode == 1) // Fail
        {
            q = 1000;
        }
        else if (mode == 2) // Interactive
        {
            // Use current values, but recalculate Qu
            if (!CalculateBearingCapacity())
            {
                InteractiveShallowFoundationPopup.NotifyCalculationFailed();
                Debug.LogError("Failed to calculate bearing capacity. Invalid degree: " + deg);
                return;
            }
        }

        RunSimulation();
    }

    private bool CalculateBearingCapacity()
    {
        // Try to get the bearing capacity factors from the table
        if (!ShallowFoundation.GetNum(deg, out double Nc, out double Nq, out double Ny))
        {
            Debug.LogError("Invalid friction angle: " + deg + "°. Must be one of: 0, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36, 38, 40, 42, 44, 46");
            return false;
        }

        // Calculate bearing capacity based on conditions
        if (c > 0 && Cu <= 0) // Drained condition (has cohesion but no undrained shear)
        {
            // Calculate for both strip and square, use the more appropriate one
            double stripCapacity = ShallowFoundation.StripFootingDrained(deg, c, q, y, b);
            double squareCapacity = ShallowFoundation.SquareFootingDrained(deg, c, Sc, q, y, b, true);
            
            // Use square footing capacity (more conservative)
            Qu = squareCapacity;
            
            Debug.Log($"Drained calculation - Strip: {stripCapacity:F2}, Square: {squareCapacity:F2}, Using: {Qu:F2}");
        }
        else if (Cu > 0) // Undrained condition
        {
            double stripCapacity = ShallowFoundation.StripFootingUndrained(Cu, q);
            double squareCapacity = ShallowFoundation.SquareFootingUndrained(Cu, Sc, q);
            
            // Use square footing capacity
            Qu = squareCapacity;
            
            Debug.Log($"Undrained calculation - Strip: {stripCapacity:F2}, Square: {squareCapacity:F2}, Using: {Qu:F2}");
        }
        else
        {
            // Default to basic bearing capacity equation
            Qu = ShallowFoundation.BearingCapacity(deg, c, q, y, b);
            Debug.Log($"Basic bearing capacity: {Qu:F2}");
        }

        return true;
    }

    public void RunSimulation()
    {
        CacheStartState();
        ResetVisuals();

        // Calculate Qu one more time to ensure it's up to date
        CalculateBearingCapacity();

        Debug.Log($"Qu (capacity) = {Qu:F2}");
        Debug.Log($"q (load) = {q:F2}");

        if (q > Qu)
        {
            Debug.Log("FOUNDATION FAILURE - Load exceeds capacity!");
            
            Vector3 sandTargetPos = sandStartPosition + new Vector3(0f, -0.1f, 0f);
            Vector3 sandTargetScale = sandStartScale + new Vector3(0f, -0.2f, 0f);
            Vector3 foundationTargetPos = foundationStartPosition + new Vector3(0f, -0.3f, 0f);

            if (Scenario_1 != null)
            {
                Scenario_1.SetActive(true);
            }

            if (Sand_M != null)
            {
                StartCoroutine(MoveScale(Sand_M.transform, sandTargetPos, sandTargetScale));
            }

            if (Foundation != null)
            {
                StartCoroutine(Move(Foundation.transform, foundationTargetPos));
            }
            
            // Notify about the failure
            InteractiveShallowFoundationPopup.NotifyCalculationFailed();
        }
        else
        {
            Debug.Log("Foundation is stable - Load is within capacity.");
        }
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