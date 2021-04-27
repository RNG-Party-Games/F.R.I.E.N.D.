using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class MagneticObject : MonoBehaviour
{
    Rigidbody rb;
    Collider c;
    bool magnetic = false;
    float lastSnapped = -100;
    public Sprite icon;
    Vector3 initScale;
    public ParticleSystem magnetParticles;
    bool snapped = false;
    public enum ItemType { Ore, Scrap, Radio, Gear, Battery, Chip, Collectible };
    public List<MeshRenderer> mr;
    public ItemType type;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        c = GetComponent<Collider>();
        initScale = transform.localScale;
        magnetParticles.Play();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Magnetize(Material magnetmat, bool magnet) {
        magnetic = magnet;
        Material[] mats = mr[0].materials;
        if(magnetic && !snapped) {
            mats[1] = magnetmat;
            foreach(MeshRenderer m in mr) {
                m.materials = mats;
            }
        }
        else {
            mats[1] = null;
            foreach (MeshRenderer m in mr) {
                m.materials = mats;
            }
        }
    }

    public void Pull(Vector3 pos, float strength) {
        rb.useGravity = false;
        rb.AddForce((pos - transform.position) * strength, ForceMode.Force);
    }

    public void LetGo() {
        c.enabled = true;
        rb.useGravity = true;
        rb.isKinematic = false;
    }

    public void Hover() {
        ExternalForcesModule extForce = magnetParticles.externalForces;
        extForce.enabled = true;
    }

    public void Unhover() {
        StopFlowing();
        ExternalForcesModule extForce = magnetParticles.externalForces;
        extForce.enabled = false;
    }

    public void StopFlowing() {
        magnetParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        magnetParticles.Play();
    }

    public void Unsnap() {
        LetGo();
        transform.parent = null;
        lastSnapped = Time.time;
        magnetParticles.Play();
        snapped = false;
        Rescale();
    }

    public void Store(Transform t) {
        c.enabled = false;
        rb.useGravity = false;
        rb.isKinematic = true;
        gameObject.SetActive(false);
        transform.position = t.position;
        transform.parent = t;
        lastSnapped = Time.time;
        magnetParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        Rescale();
    }

    public bool Snap(Transform t, bool force) {
        magnetParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (force || Time.time - lastSnapped > 2) {
            GameManager.instance.pickup();
            GameManager.instance.magnet(false);
            c.enabled = false;
            rb.useGravity = false;
            rb.isKinematic = true;
            transform.position = t.position;
            transform.parent = t;
            lastSnapped = Time.time;
            Rescale();
            snapped = true;
            return true;
        }
        return false;
    }

    public void Rescale() {
        transform.localScale = initScale;
    }
}
