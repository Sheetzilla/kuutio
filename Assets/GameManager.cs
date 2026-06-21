using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject pikku;
    public float speed = 100f;
    public GameObject leftWall;
    public GameObject rightWall;
    public int spawnAmount = 10;
    public int spawned = 0;
    public List<GameObject> cubes = new List<GameObject>();

    // World Y below which a dropped cube is destroyed. Set this in the Inspector
    // to a height comfortably below the bottom of the visible play area.
    public float despawnY = -10f;

    private timer timerScript;
    private ScriptKuutio cube;
    private Coroutine dropTimerRoutine;

    // Cubes already mid-removal, so dropCubes never flags the same one twice.
    private HashSet<GameObject> removing = new HashSet<GameObject>();

    // Cube count captured when the timer starts, so the pile drains linearly
    // against this fixed baseline instead of compounding off the shrinking count.
    private int startingCubeCount = 0;

    void Start()
    {
        timerScript = GetComponent<timer>();
        cube = GetComponent<ScriptKuutio>();

        Vector3 left = Camera.main.ScreenToWorldPoint(
            new Vector3(-40f, Screen.height / 2, 10));
        Vector3 right = Camera.main.ScreenToWorldPoint(
            new Vector3(Screen.width + 40f, Screen.height / 2, 10));
        leftWall.transform.position = left;
        rightWall.transform.position = right;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject.CompareTag("Kuutio") &&
                    spawned < 300 &&
                    timerScript.timeLimit > 0 &&
                    !cube.isSpinning)
                    
                {
                    StartCoroutine(spawnMinis(hit));
                    hit.collider.gameObject.GetComponent<Animator>().SetTrigger("click");
                }
            }
        }
    }

    IEnumerator spawnMinis(RaycastHit hit)
    {
        for (int i = 0; i < spawnAmount; i++)
        {
            yield return new WaitForSeconds(0.1f);

            // Scatter the spawn point so 300 bodies aren't born inside each other
            // (overlapping spawns are the single biggest cause of the jitter).
            Vector3 jitter = new Vector3(
                Random.Range(-0.3f, 0.3f),
                Random.Range(-0.3f, 0.3f),
                0f);
            GameObject spawnattu = Instantiate(
                pikku,
                hit.collider.gameObject.transform.position + jitter,
                Quaternion.identity);

            Rigidbody rb = spawnattu.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Help cubes settle and actually fall asleep instead of jittering.
                rb.sleepThreshold = 0.05f;
                rb.maxDepenetrationVelocity = 1f; // don't shove overlaps apart violently
#if UNITY_2022_3_OR_NEWER
                rb.linearDamping = 0.1f;
                rb.angularDamping = 0.5f;
#else
                rb.drag = 0.1f;
                rb.angularDrag = 0.5f;
#endif
                rb.AddForce(new Vector3(
                    Random.Range(-1f, 2f) * speed,
                    Random.Range(-1f, 2f) * speed,
                    0));
            }
            cubes.Add(spawnattu);
        }
        spawned = cubes.Count;

        if (spawned >= 240)
        {
            startingCubeCount = cubes.Count;
            timerScript.StartTimer();

            // Fisher-Yates shuffle so cubes drop in random order.
            for (int i = 0; i < cubes.Count; i++)
            {
                int j = Random.Range(i, cubes.Count);
                GameObject temp = cubes[i];
                cubes[i] = cubes[j];
                cubes[j] = temp;
            }
        }
    }

    public void dropCubes()
    {
        if (timerScript.timerIsRunning == false)
            return;
        if (timerScript.timeLimit <= 0)
            return;

        float percentRemaining = timerScript.timeRemaining / timerScript.timeLimit;
        // Target keep-count is a fraction of the ORIGINAL pile, not the current
        // (shrinking) one — otherwise the percentage compounds each second and the
        // pile collapses far too fast (e.g. half gone in the first ~50s).
        int cubesToKeep = Mathf.RoundToInt(startingCubeCount * percentRemaining);

        for (int i = cubes.Count - 1; i >= 0; i--)
        {
            if (i >= cubesToKeep && !removing.Contains(cubes[i]))
            {
                StartCoroutine(RemoveCubeAfterDelay(cubes[i]));
            }
            // Cubes we're keeping are left untouched. Previously they were
            // WakeUp()'d every second, which re-triggered the pile's jitter;
            // Unity already wakes a body when a neighbor it touches is destroyed.
        }
    }

    IEnumerator RemoveCubeAfterDelay(GameObject cube)
    {
        removing.Add(cube);

        // Disable the collider so this cube falls through everything below it.
        Collider col = cube.GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        // Wake the cube and give it a small downward nudge. Without this a cube
        // that was asleep at the top of the pile just floats in place, because a
        // sleeping rigidbody won't re-evaluate gravity once its supports vanish.
        Rigidbody rb = cube.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.WakeUp();
            rb.AddForce(Vector3.down * 2f, ForceMode.VelocityChange);
        }

        // Wait until the cube falls below the despawnY world height, then destroy
        // it. Using a fixed Y line (instead of the camera viewport) means cubes
        // squeezed off sideways or sitting above the top are never culled early —
        // a cube is only removed once it has genuinely dropped out the bottom.
        // A safety timeout still cleans up anything that somehow never falls.
        float timeout = 20f;
        float elapsed = 0f;
        while (cube != null && elapsed < timeout)
        {
            if (cube.transform.position.y < despawnY)
                break;
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        if (cube != null)
        {
            removing.Remove(cube);
            cubes.Remove(cube);
            Destroy(cube);
        }
        spawned = cubes.Count;
    }

    // Destroys every cube and resets the counter. Called when the player edits
    // the time, forcing a full respawn back up to 300 before the timer restarts.
    public void ClearAllCubes()
    {
        for (int i = 0; i < cubes.Count; i++)
        {
            if (cubes[i] != null)
                Destroy(cubes[i]);
        }
        cubes.Clear();
        spawned = 0;
    }

    // Drops every cube using the same delayed-removal animation as dropCubes,
    // so on a time edit the player sees them fall away instead of vanishing.
    // RemoveCubeAfterDelay updates spawned as each one is destroyed, so the
    // player can begin respawning the new 300 immediately.
    public void DropAllCubes()
    {
        for (int i = cubes.Count - 1; i >= 0; i--)
        {
            if (cubes[i] != null)
            {
                Rigidbody rb = cubes[i].GetComponent<Rigidbody>();
                if (rb != null)
                    rb.WakeUp();
                StartCoroutine(RemoveCubeAfterDelay(cubes[i]));
            }
        }
    }

    // Public start/stop wrappers so other scripts can control the drop loop
    // by handle rather than by string name (string stop never matched the
    // reference-started coroutine before).
    public void StartDropTimer()
    {
        StopDropTimer();
        dropTimerRoutine = StartCoroutine(dropTimer());
    }

    public void StopDropTimer()
    {
        if (dropTimerRoutine != null)
        {
            StopCoroutine(dropTimerRoutine);
            dropTimerRoutine = null;
        }
    }

    public IEnumerator dropTimer()
    {
        while (timerScript.timerIsRunning)
        {
            yield return new WaitForSeconds(1f);
            dropCubes();
        }
        dropTimerRoutine = null;
    }
}