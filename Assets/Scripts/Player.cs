using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using TMPro;

public class Player : MonoBehaviour
{
    Camera cam;
    Rigidbody rb;
    public float speed = 2.0f;
    float effectivespeed = 2.0f;
    float turningSpeed = 10.0f;
    bool magnet = false;
    public static Player instance;
    MagneticObject hover = null;
    public Transform magnetTransform, spawnPoint;
    public MagnetSnap snap;
    float strength = 1;
    public Animator arm;
    public Transform station;
    public PostProcessVolume signalVolume, batteryVolume;
    public Animator signalBlink, batteryBlink;
    Animator anim;
    bool itemOut = false;
    public ParticleSystemForceField forcefield;
    bool freelook = false;
    bool inUI = false;
    bool dying = false;
    float thrustCost = 0.01f;
    bool shownpopup = false;

    float maxDistance = 70, midDistance = 50;
    public int distanceProgress, batteryProgress, magnetProgress, speedProgress;
    float[] distanceAmounts = { 70, 130, 210, 300, 400, 600 };
    float[] batteryAmounts = { 100, 150, 200, 250, 300 };
    float[] speedAmounts = { 8, 12, 16 };
    float[] magnetAmounts = { 10, 20, 30 };
    float magnetPullDistance = 10;
    float batteryLevel = 100, maxBattery = 100;
    int maxStorage = 1;
    int collectibles = 0;
    public int maxcollectibles = 0;
    public BatteryIndicator battery;
    public TextMeshProUGUI collectibleText, fishText;
    public ParticleSystem bubbles;
    public GameObject scanner;
    bool batteryPlayed, signalPlayed;
    public AudioSource swim;

    bool[] catalog = { false, false, false, false, false, false, false, false, false, false, false, false };



    private void Awake() {
        cam = Camera.main;
        anim = GetComponent<Animator>();
        if (instance == null) {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody>();
        maxDistance = distanceAmounts[distanceProgress];
        midDistance = distanceAmounts[distanceProgress] - 20;
        batteryLevel = maxBattery = batteryAmounts[batteryProgress];
        effectivespeed = speed = speedAmounts[speedProgress];
        magnetPullDistance = magnetAmounts[magnetProgress];
        SetFishText();
    }

    // Update is called once per frame
    void Update() {
        rb.drag = 2;
        swim.volume = rb.velocity.magnitude / 5;
        freelook = Input.GetButton("Ctrl");
        float distanceToStation = Vector3.Distance(station.position, transform.position);
        signalVolume.weight = Mathf.Clamp((distanceToStation - midDistance) / (maxDistance - midDistance), 0, 1);
        signalBlink.SetBool("low", distanceToStation > (maxDistance - 10));
        if (distanceToStation > (maxDistance - 10) && !signalPlayed) {
            signalPlayed = true;
            GameManager.instance.lowsignal();
        }
        else if (distanceToStation < (maxDistance - 10) && signalPlayed) {
            signalPlayed = !signalPlayed;
        }
        batteryVolume.weight = Mathf.Clamp((-Mathf.Clamp(batteryLevel / maxBattery, 0.001f, 1) + 0.2f) * 5, 0, 1);
        batteryBlink.SetBool("low", batteryLevel / maxBattery < 0.2f);
        if (batteryLevel / maxBattery < 0.2f && !batteryPlayed) {
            batteryPlayed = true;
            GameManager.instance.lowbattery();
        }
        if (batteryLevel <= 0 && !dying) {
            Die();
        }

        Vector3 toTarget = (station.position - transform.position).normalized;
        if (!inUI && !dying && !cam.GetComponent<CameraController>().IsScanning()) {
            if (Input.GetButtonDown("Magnet")) {
                magnet = true;
                GameManager.instance.MagnetMode(magnet);

                float time = 0;
                if (arm.GetCurrentAnimatorStateInfo(0).IsName("MagnetRetract")) {
                    time = arm.GetCurrentAnimatorStateInfo(0).length - arm.GetCurrentAnimatorStateInfo(0).normalizedTime;
                }
                arm.Play("MagnetExtend", 0, time);
                GameManager.instance.arm();

                cam.GetComponent<CameraController>().Magnet(true);
                effectivespeed = speed / 2;
            }
        }
        if (Input.GetButtonUp("Magnet") && magnet) {
            magnet = false;
            GameManager.instance.MagnetMode(magnet);

            float time = 0;
            if (arm.GetCurrentAnimatorStateInfo(0).IsName("MagnetExtend")) {
                time = arm.GetCurrentAnimatorStateInfo(0).length - arm.GetCurrentAnimatorStateInfo(0).normalizedTime;
            }
            arm.Play("MagnetRetract", 0, time);
            GameManager.instance.arm();

            cam.GetComponent<CameraController>().Magnet(false);
            effectivespeed = speed;
        }
        if (MagnetReady()) {
            if (hover != null && Vector3.Distance(magnetTransform.position, hover.transform.position) > magnetPullDistance) {
                hover.Unhover();
                LetGoObject();
                hover = null;
            }
            if (Magnetized() && hover != null) {
                GrabObject();
            }
            if (Input.GetMouseButtonDown(0)) {
                if (snap.Snapped()) {
                    snap.Unsnap();
                }
            }
            else if (Input.GetMouseButtonUp(0)) {
                if (hover != null) {
                    LetGoObject();
                }
            }
        }
    }

    void FixedUpdate() {
        CheckInput();
        SetBattery();
    }

    void CheckInput() {
        float distanceToStation = Vector3.Distance(station.position, transform.position);
        Vector3 toTarget = (station.position - transform.position).normalized;
        if (!inUI && !dying) {
            // Movement
            if (magnet && !freelook) {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(cam.transform.forward), Time.deltaTime * turningSpeed);
            }
            if(cam.GetComponent<CameraController>().IsScanning()) {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(cam.transform.forward), Time.deltaTime * turningSpeed);
            }
            if (Input.GetAxis("Vertical") > 0) {
                if (!freelook) {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(cam.transform.forward), Time.deltaTime * turningSpeed);
                }
                if (distanceToStation < maxDistance || Vector3.Dot(toTarget, transform.forward) > 0) {
                    rb.AddForce(transform.forward * effectivespeed, ForceMode.Acceleration);
                    batteryLevel -= thrustCost;
                    rb.drag = 1;
                    EmitBubble();
                }
            }
            else if (Input.GetAxis("Vertical") < 0) {
                if (!freelook) {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(cam.transform.forward), Time.deltaTime * turningSpeed);
                }
                if (distanceToStation < maxDistance || Vector3.Dot(toTarget, -transform.forward) > 0) {
                    rb.AddForce(-transform.forward * effectivespeed / 2, ForceMode.Acceleration);
                    batteryLevel -= thrustCost;
                    rb.drag = 1;
                    EmitBubble();
                }
            }
            if (Input.GetAxis("Horizontal") > 0) {
                if (distanceToStation < maxDistance || Vector3.Dot(toTarget, transform.right) > 0) {
                    rb.AddForce(transform.right * effectivespeed / 2, ForceMode.Acceleration);
                    batteryLevel -= thrustCost;
                    rb.drag = 1;
                    EmitBubble();
                }
            }
            else if (Input.GetAxis("Horizontal") < 0) {
                if (distanceToStation < maxDistance || Vector3.Dot(toTarget, -transform.right) > 0) {
                    rb.AddForce(-transform.right * effectivespeed / 2, ForceMode.Acceleration);
                    batteryLevel -= thrustCost;
                    rb.drag = 1;
                    EmitBubble();
                }
            }
            if (Input.GetAxis("Up") > 0) {
                if (distanceToStation < maxDistance || Vector3.Dot(toTarget, transform.up) > 0) {
                    rb.AddForce(transform.up * effectivespeed / 2, ForceMode.Acceleration);
                    batteryLevel -= thrustCost;
                    rb.drag = 1;
                    EmitBubble();
                }
            }
            else if (Input.GetAxis("Up") < 0) {
                if (distanceToStation < maxDistance || Vector3.Dot(toTarget, -transform.up) > 0) {
                    rb.AddForce(-transform.up * effectivespeed / 2, ForceMode.Acceleration);
                    batteryLevel -= thrustCost;
                    rb.drag = 1;
                    EmitBubble();
                }
            }
        }
    }

    public void Die() {
        scanner.SetActive(false);
        anim.Play("Death");
        dying = true;
        snap.Clear(true);
        bubbles.Stop();
        Halt();
        GameManager.instance.die();
    }

    public void Respawn() {
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        Charge();
        batteryPlayed = false;
    }

    public void RespawnComplete() {
        dying = false;
        bubbles.Play();
        scanner.SetActive(true);
    }

    public void EmitBubble() {
        if (Random.Range(0, 10) < 2) {
            bubbles.Emit(1);
        }
    }

    public bool FreeLook() {
        return freelook;
    }

    public void InUI(bool ui, bool teleport) {
        inUI = ui;
        cam.GetComponent<CameraController>().UICam(ui, teleport);
    }

    public void Charge() {
        batteryLevel = maxBattery;
        SetBattery();
    }

    public void SetBattery() {
        battery.SetBattery(batteryLevel / 100, maxBattery / 100, Mathf.Clamp(rb.velocity.magnitude - 0.5f, 0, 1));
    }

    public void IncreaseBattery() {
        batteryProgress++;
        maxBattery = batteryAmounts[batteryProgress];
        batteryLevel = maxBattery;
        GameManager.instance.upgrade();
    }

    public void IncreaseRange() {
        distanceProgress++;
        maxDistance = distanceAmounts[distanceProgress];
        midDistance = maxDistance - 20;
        GameManager.instance.upgrade();
    }

    public void IncreaseStorage() {
        maxStorage++;
        snap.SetMax(maxStorage);
        GameManager.instance.upgrade();
    }

    public void IncreaseSpeed() {
        speedProgress++;
        speed = speedAmounts[speedProgress];
        GameManager.instance.upgrade();
    }

    public void IncreaseMagnet() {
        magnetProgress++;
        magnetPullDistance = magnetAmounts[magnetProgress];
        GameManager.instance.upgrade();
    }

    public bool MagnetDeployed() {
        return magnet;
    }

    public bool MagnetReady() {
        return magnet && itemOut;
    }

    public bool Magnetized() {
        return MagnetReady() && Input.GetMouseButton(0);
    }

    public void HoverObject(MagneticObject mo) {
        if (mo != hover && hover != null && !snap.Snapped()) {
            hover.Unhover();
            LetGoObject();
        }
        if(mo != null && !snap.Snapped()) {
            if(Vector3.Distance(magnetTransform.position, mo.transform.position) < magnetPullDistance) {
                hover = mo;
                hover.Hover();
            }
        }
        else {
            hover = null;
            GameManager.instance.magnet(false);
        }
    }
    public void GrabObject() {
        hover.Pull(magnetTransform.position, strength);
        GameManager.instance.magnet(true);
    }

    public void ForgetHover() {
        if (hover != null) {
            GameManager.instance.magnet(false);
            hover.Unhover();
            hover = null;
        }
    }

    public void LetGoObject() {
        if (!snap.Snapped()) {
            GameManager.instance.magnet(false);
            hover.LetGo();
        }
    }

    public void ItemToggle() {
        itemOut = !itemOut;
    }

    public MagneticObject[] GetInventory() {
        return snap.GetObjects();
    }

    public void ClearInventory() {
        snap.Clear(false);
    }

    public int GetStorage() {
        return maxStorage;
    }

    public void Halt() {
        rb.velocity = rb.velocity*0.2f;
    }

    public void GetCollectible() {
        ++collectibles;
        DisplayCollectibles();
    }

    public void DisplayCollectibles() {
        collectibleText.text = collectibles + " / " + maxcollectibles;
    }

    public float GetAntennaBar() {
        return Mathf.Clamp((float) (distanceProgress) / (distanceAmounts.Length - 1), 0, 1);
    }

    public float GetStorageBar() {
        return Mathf.Clamp((float) (maxStorage -1) / 4.0f, 0, 1);
    }

    public float GetMovementBar() {
        return Mathf.Clamp((float)(speedProgress) / (speedAmounts.Length - 1), 0, 1);
    }

    public float GetBatteryBar() {
        return Mathf.Clamp((float)(batteryProgress) / (batteryAmounts.Length - 1), 0, 1);
    }

    public float GetMagnetBar() {
        return Mathf.Clamp((float) (magnetProgress) / (magnetAmounts.Length - 1), 0, 1);
    }

    public void Catalog(Fish f) {
        if (!catalog[f.index]) {
            GameManager.instance.scannew();
            catalog[f.index] = true;
        }
        SetFishText();
    }

    public void SetFishText() {
        int sum = 0;
        foreach (bool b in catalog) {
            if (b) sum++;
        }
        if(sum >= catalog.Length && !shownpopup) {
            UIManager.instance.ShowPopup();
            shownpopup = true;
        }
        fishText.text = sum + " / " + catalog.Length;
    }
}
