using UnityEngine;
using System.Collections;

public class RetainingController : MonoBehaviour
{
    [Header("Parameters")]
    public Transform beams;
    public Transform wood1;
    public Transform wood2;
    public Transform wood3;
    public Transform wood4;
    public Transform wood5;

    [Header("Movement")]
    public float beamDropDist = 5f;             // How far beams drop
    public float woodDropDist = 1f;             // How far wood walls drop
    public float moveSpeed = 2f;                // How fast

    private int curr = 0;                       // Current step

    private Vector3 beamsTarget;
    private Vector3 wood1Target;
    private Vector3 wood2Target;
    private Vector3 wood3Target;
    private Vector3 wood4Target;
    private Vector3 wood5Target;

    private Vector3 beamsStart;
    private Vector3 wood1Start;
    private Vector3 wood2Start;
    private Vector3 wood3Start;
    private Vector3 wood4Start;
    private Vector3 wood5Start;
    private bool hasCachedStart;

    void Start()
    {
        CacheStartState();

        // Calculate where the beams/walls will drop down to (how far)
        beamsTarget = beams.position + Vector3.down * beamDropDist;

        wood1Target = wood1.position + Vector3.down * woodDropDist;
        wood2Target = wood2.position + Vector3.down * woodDropDist;
        wood3Target = wood3.position + Vector3.down * woodDropDist;
        wood4Target = wood4.position + Vector3.down * woodDropDist;
        wood5Target = wood5.position + Vector3.down * woodDropDist;

        ResetScenario();
    }

    public void AdvanceStep()
    {
        if(curr >= 11)
        {
            return;
        }
        curr++;
        Step();
    }

    void Step()
    {
        switch (curr)
        {
            case 1:
                RemoveByTag("DigSteel");    // Delete the soil where steel will be placed
                StartCoroutine(Move(beams, beamsTarget));   // Move beams into place
                break;

            case 2:
                RemoveByTag("Dig1");    // Delete first layer of soil for walls
                break;

            case 3:
                StartCoroutine(Move(wood1, wood1Target));   // Move first layer of walls into place
                break;

            case 4:
                RemoveByTag("Dig2");    // Repeated
                break;

            case 5:
                StartCoroutine(Move(wood2, wood2Target));
                break;

            case 6:
                RemoveByTag("Dig3");
                break;

            case 7:
                StartCoroutine(Move(wood3, wood3Target));
                break;

            case 8:
                RemoveByTag("Dig4");
                break;

            case 9:
                StartCoroutine(Move(wood4, wood4Target));
                break;

            case 10:
                RemoveByTag("Dig5");
                break;

            case 11:
                StartCoroutine(Move(wood5, wood5Target));
                break;

            default:
                break;
        }
    }

    private void CacheStartState()
    {
        if (hasCachedStart)
        {
            return;
        }

        if (beams != null)
        {
            beamsStart = beams.position;
        }

        if (wood1 != null)
        {
            wood1Start = wood1.position;
        }

        if (wood2 != null)
        {
            wood2Start = wood2.position;
        }

        if (wood3 != null)
        {
            wood3Start = wood3.position;
        }

        if (wood4 != null)
        {
            wood4Start = wood4.position;
        }

        if (wood5 != null)
        {
            wood5Start = wood5.position;
        }

        hasCachedStart = true;
    }

    public void ResetScenario()
    {
        CacheStartState();
        StopAllCoroutines();
        curr = 0;

        if (beams != null)
        {
            beams.position = beamsStart;
            SetVisible(beams, false);
        }

        if (wood1 != null)
        {
            wood1.position = wood1Start;
            SetVisible(wood1, false);
        }

        if (wood2 != null)
        {
            wood2.position = wood2Start;
            SetVisible(wood2, false);
        }

        if (wood3 != null)
        {
            wood3.position = wood3Start;
            SetVisible(wood3, false);
        }

        if (wood4 != null)
        {
            wood4.position = wood4Start;
            SetVisible(wood4, false);
        }

        if (wood5 != null)
        {
            wood5.position = wood5Start;
            SetVisible(wood5, false);
        }

        SetByTag("DigSteel", true);
        SetByTag("Dig1", true);
        SetByTag("Dig2", true);
        SetByTag("Dig3", true);
        SetByTag("Dig4", true);
        SetByTag("Dig5", true);
    }

    void RemoveByTag(string tagName)        // Hide my objects based on tag
    {
        SetByTag(tagName, false);
    }

    private void SetByTag(string tagName, bool visible)
    {
        GameObject[] blocks = GameObject.FindGameObjectsWithTag(tagName);
        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i].SetActive(visible);
        }
    }

    IEnumerator Move(Transform obj, Vector3 target)     // Moves objects with basic animation
    {
        SetVisible(obj, true);

        while (Vector3.Distance(obj.position, target) > 0.01f)
        {
            obj.position = Vector3.MoveTowards(obj.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    void SetVisible(Transform obj, bool visible)    // Sets all the child objects visible
    {
        Renderer[] rends = obj.GetComponentsInChildren<Renderer>();

        for (int i = 0; i < rends.Length; i++)
        {
            rends[i].enabled = visible;
        }
    }
}

