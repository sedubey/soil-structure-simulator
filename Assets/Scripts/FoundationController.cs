using UnityEngine;

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

    void Start()
    {
        Debug.Log("Controller running");

        // Hide Scenario specific sand
        Scenario_1.SetActive(false);

        if (q > Qu) // Failure
        {
            // Scale and move middle sand
            Sand_M.transform.position += new Vector3(0f, -0.1f, 0f);
            Sand_M.transform.localScale += new Vector3(0f, -0.2f, 0f);

            // Move foundation down
            Foundation.transform.position += new Vector3(0f, -0.3f, 0f);

            // Shows the scenario specific sand
            Scenario_1.SetActive(true);
        }

        Debug.Log("Qu = " + Qu);
        Debug.Log("q = " + q);
    }
}