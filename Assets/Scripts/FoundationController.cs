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

    public double Qu = 500;   // Set high for failure testing

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float scaleSpeed = 2f;

    private Vector3 foundationStartPosition;
    private Vector3 sandStartPosition;
    private Vector3 sandStartScale;
    private bool hasCachedStartState;

    void Awake()
    {
        CacheStartState();
    }

    void Start()
    {
        Debug.Log("Controller running");
        RunSimulation();
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

        RunSimulation();
    }

    public void RunSimulation()
    {
        CacheStartState();
        StopAllCoroutines();
        ResetGeometry();

        if (q > Qu) // Failure
        {
            if (Sand_M != null)
            {
                // Scale and move middle sand
                StartCoroutine(
                    MoveScale(
                        Sand_M.transform,
                        sandStartPosition + new Vector3(0f, -0.1f, 0f),
                        sandStartScale + new Vector3(0f, -0.2f, 0f)));
            }

            if (Foundation != null)
            {
                // Move foundation down
                StartCoroutine(Move(Foundation.transform, foundationStartPosition + new Vector3(0f, -0.3f, 0f)));
            }

            // Shows the scenario specific sand
            if (Scenario_1 != null)
            {
                Scenario_1.SetActive(true);
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

    private void ResetGeometry()
    {
        if (Scenario_1 != null)
        {
            Scenario_1.SetActive(false);
        }

        if (Foundation != null)
        {
            Foundation.transform.position = foundationStartPosition;
        }

        if (Sand_M != null)
        {
            Sand_M.transform.position = sandStartPosition;
            Sand_M.transform.localScale = sandStartScale;
        }
    }

    IEnumerator Move(Transform obj, Vector3 target)     // Moves objects with basic animation
    {
        while (Vector3.Distance(obj.position, target) > 0.01f)
        {
            obj.position = Vector3.MoveTowards(obj.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        obj.position = target;
    }

    IEnumerator MoveScale(Transform obj, Vector3 targetPos, Vector3 targetScale)     // Moves objects with basic animation
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
