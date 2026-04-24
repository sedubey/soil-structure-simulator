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

    public double Qu = 500;

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
    }

    public void ApplyParametersAndRun(
        int degree,
        double cohesion,
        double unitWeight,
        double width,
        double appliedLoad,
        double undrainedShearStrength,
        double shapeFactor,
        double ultimateBearingCapacity)
    {
        deg = degree;
        c = cohesion;
        y = unitWeight;
        b = width;
        q = appliedLoad;
        Cu = undrainedShearStrength;
        Sc = shapeFactor;
        Qu = ultimateBearingCapacity;

        RunScenario(2);
    }

    public void RunScenario(int mode)
    {
        CacheStartState();

        if (mode == 0) // Pass
        {
            q = 300;
            Qu = 500;
        }
        else if (mode == 1) // Fail
        {
            q = 1000;
            Qu = 500;
        }
        else if (mode == 2) // Interactive
        {
            // Use current values set in inspector/UI.
        }

        RunSimulation();
    }

    public void RunSimulation()
    {
        CacheStartState();
        ResetVisuals();

        if (q > Qu)
        {
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
        }

        Debug.Log("Qu = " + Qu);
        Debug.Log("q = " + q);
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

    // Resets to original location so you can run it multiple times.
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
