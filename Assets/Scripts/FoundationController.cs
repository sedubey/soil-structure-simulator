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

    void Start()
    {
        Debug.Log("Controller running");

        // Hide Scenario specific sand
        Scenario_1.SetActive(false);

        if (q > Qu) // Failure
        {
            // Scale and move middle sand
            StartCoroutine(MoveScale(Sand_M.transform, Sand_M.transform.position += new Vector3(0f, -0.1f, 0f), Sand_M.transform.localScale += new Vector3(0f, -0.2f, 0f)));

            // Move foundation down
            StartCoroutine(Move(Foundation.transform, Foundation.transform.position + new Vector3(0f, -0.3f, 0f)));

            // Shows the scenario specific sand
            Scenario_1.SetActive(true);
        }

        Debug.Log("Qu = " + Qu);
        Debug.Log("q = " + q);
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