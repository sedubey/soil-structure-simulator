using UnityEngine;

public class Button : MonoBehaviour
{
    public enum ButtonType
    {
        RetainingStep,
        ShallowPass,
        ShallowFail,
        ShallowInteractive
    }

    public ButtonType buttonType;

    public RetainingController retainingController;
    public FoundationController foundationController;

    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (buttonType == ButtonType.RetainingStep)
            {
                retainingController.AdvanceStep();
            }

            if (buttonType == ButtonType.ShallowPass)
            {
                Debug.Log("Running shallow pass");
                foundationController.RunScenario(0);
            }

            if (buttonType == ButtonType.ShallowFail)
            {
                Debug.Log("Running shallow fail");
                foundationController.RunScenario(1);
            }

            if (buttonType == ButtonType.ShallowInteractive)
            {
                Debug.Log("Running shallow int");
                foundationController.RunScenario(2);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something entered: " + other.name);

        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}