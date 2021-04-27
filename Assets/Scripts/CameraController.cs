using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    vnc.FX.WaterCamera wc;
    float speed = 3;
    float distanceToTarget = 10, minDistance = 5, maxDistance = 15, effectiveDistanceToTarget, tempDist, nextDistanceToTarget = 10;
    float oldVerticalDistance, nextVerticalDistance = 3, lastSwapped = 0;
    const float topDist = 1.5f, bottomDist = -1.5f;
    public float cameraCollisionDist = 1;
    // Start is called before the first frame update
    bool locked = true, scanning = false;
    private Vector3 previousPosition;
    Camera cam;
    public Color waterFog, skyFog;
    bool inUI = false;
    public Transform appcam;
    public Animator scanner;
    public Scanner cone;
    public AudioSource underwater;

    void Start()
    {
        LockCam(true);
        cam = GetComponent<Camera>();
        wc = GetComponent<vnc.FX.WaterCamera>();
        nextVerticalDistance = topDist;
        oldVerticalDistance = nextVerticalDistance;
        effectiveDistanceToTarget = distanceToTarget;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetButtonDown("Cancel")) {
            LockCam(false);
        }
        else if (Input.GetButtonDown("Scan") && !Player.instance.MagnetDeployed()) {
            scanning = true;
            nextDistanceToTarget = 0;
            scanner.Play("Scan");
            cone.Scan(true);
            GameManager.instance.scan(true);
        }
        else if(Input.GetButtonUp("Scan") && scanning) {
            scanning = false;
            nextDistanceToTarget = 10;
            scanner.Play("NoScan");
            cone.Scan(false);
            GameManager.instance.scan(false);
        }
        else if(!locked && Input.GetMouseButtonDown(0) && !inUI) {
            LockCam(true);
        }
        Vector3 direction = Vector3.zero;
        if (locked) {
            direction = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0);
        }
        if (!inUI) {
            float rotationAroundYAxis = direction.x * speed; // camera moves horizontally
            float rotationAroundXAxis = -direction.y * speed; // camera moves vertically

            cam.transform.position = target.position;

            if (!Player.instance.MagnetDeployed() && !scanning) {
                if (Input.mouseScrollDelta != Vector2.zero) {
                    nextDistanceToTarget -= Input.mouseScrollDelta.y;
                }
                if (nextDistanceToTarget < minDistance) nextDistanceToTarget = minDistance;
                else if (nextDistanceToTarget > maxDistance) nextDistanceToTarget = maxDistance;
            }

            cam.transform.Rotate(new Vector3(1, 0, 0), rotationAroundXAxis);
            cam.transform.Rotate(new Vector3(0, 1, 0), rotationAroundYAxis, Space.World); // <— This is what makes it work!
            float verticalDistance = Mathf.SmoothStep(oldVerticalDistance, nextVerticalDistance, (Time.time - lastSwapped) * 2);
            effectiveDistanceToTarget = Mathf.SmoothStep(effectiveDistanceToTarget, nextDistanceToTarget, Time.deltaTime*20);
            cam.transform.Translate(new Vector3(0, verticalDistance, -effectiveDistanceToTarget));
            if (Player.instance.MagnetReady() && !Player.instance.FreeLook()) {
                int layerMask = 1 << 8;
                RaycastHit hit;
                // Does the ray intersect any objects excluding the player layer
                if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask)) {
                    Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                    Player.instance.HoverObject(hit.collider.GetComponent<MagneticObject>());
                }
                else {
                    Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
                    Player.instance.HoverObject(null);
                }
            }

            if (locked) {
                wc.enabled = transform.position.y < 0;
                if (transform.position.y < 0) {
                    RenderSettings.fogColor = waterFog*GameManager.instance.fogMod;
                    underwater.volume = 1;
                }
                else {
                    RenderSettings.fogColor = skyFog*GameManager.instance.fogMod;
                    underwater.volume = 0;
                }
                if (!scanning) {
                    int layerMask = 1 << 12;
                    RaycastHit hit;
                    if (Physics.Raycast(Player.instance.transform.position, transform.position - Player.instance.transform.position, out hit, effectiveDistanceToTarget + 1, layerMask)) {
                        Debug.DrawRay(Player.instance.transform.position, 100*(transform.position - Player.instance.transform.position), Color.red);
                        transform.position = hit.point + transform.forward * cameraCollisionDist;
                    }
                    else {
                        Debug.DrawRay(Player.instance.transform.position, 100 * (transform.position - Player.instance.transform.position), Color.cyan);
                    }
                }
            }
        }


    }

    public void Magnet(bool magnet) {
        lastSwapped = Time.time;
        if (magnet) {
            tempDist = effectiveDistanceToTarget;
            nextDistanceToTarget = 7;
            oldVerticalDistance = topDist;
            nextVerticalDistance = bottomDist;
            cameraCollisionDist = 2;
        }
        else {
            nextDistanceToTarget = tempDist;
            oldVerticalDistance = bottomDist;
            nextVerticalDistance = topDist;
            cameraCollisionDist = 1;
        }
    }

    public void UICam(bool ui, bool teleport) {
        inUI = ui;
        LockCam(!ui);
        if (ui && teleport) {
            transform.position = appcam.position;
            transform.rotation = appcam.rotation;
        }
    }

    public void LockCam(bool lockcam) {
        locked = lockcam;
        if(locked) {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public bool IsScanning() {
        return scanning;
    }
}