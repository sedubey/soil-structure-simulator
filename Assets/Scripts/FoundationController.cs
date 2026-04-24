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

    private Vector3 foundationStartPos;
    private Vector3 sandStartPos;
    private Vector3 sandStartScale;

    void Start()
    {
        Debug.Log("Controller running");

        // Save starting values so reset works later
        foundationStartPos = Foundation.transform.position;
        sandStartPos = Sand_M.transform.position;
        sandStartScale = Sand_M.transform.localScale;

        Scenario_1.SetActive(false);
    }

    public void RunScenario(int mode)
    {
        ResetVisuals();

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
            // UI entered module, your stuff (currently thios part just uses the hardcoded numbers
        }

        if (q > Qu)
        {
            // Calculate targets
            Vector3 sandTargetPos = sandStartPos + new Vector3(0f, -0.1f, 0f);
            Vector3 sandTargetScale = sandStartScale + new Vector3(0f, -0.2f, 0f);
            Vector3 foundationTargetPos = foundationStartPos + new Vector3(0f, -0.3f, 0f);

            Scenario_1.SetActive(true);

            StartCoroutine(MoveScale(Sand_M.transform, sandTargetPos, sandTargetScale));
            StartCoroutine(Move(Foundation.transform, foundationTargetPos));
        }

        Debug.Log("Qu = " + Qu);
        Debug.Log("q = " + q);
    }

    // Resets to original location so you can run it multiple times
    public void ResetVisuals()
    {
        StopAllCoroutines();

        Foundation.transform.position = foundationStartPos;
        Sand_M.transform.position = sandStartPos;
        Sand_M.transform.localScale = sandStartScale;

        Scenario_1.SetActive(false);
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