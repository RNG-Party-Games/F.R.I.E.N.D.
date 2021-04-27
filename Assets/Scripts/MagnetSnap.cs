using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetSnap : MonoBehaviour
{
    MagneticObject[] objects = { null, null, null, null, null };

    int current = 0;
    int max = 1;
    public Transform snapPos, storage;
    public ParticleSystem collectibleParticles;
    // Start is called before the first frame update
    void Start()
    {
        UIManager.instance.DisplaySelector(current, max);
    }

    // Update is called once per frame
    void Update()
    {
        if (Player.instance.MagnetDeployed()) {
            if (Input.mouseScrollDelta != Vector2.zero) {
                ChangeCurrent(current - Mathf.RoundToInt(Input.mouseScrollDelta.y));
            }
            else if (Input.GetKeyDown("1")) ChangeCurrent(0);
            else if (Input.GetKeyDown("2")) ChangeCurrent(1);
            else if (Input.GetKeyDown("3")) ChangeCurrent(2);
            else if (Input.GetKeyDown("4")) ChangeCurrent(3);
            else if (Input.GetKeyDown("5")) ChangeCurrent(4);
        }
    }

    public void ChangeCurrent(int c) {
        if (max > 1) {
            if (objects[current] != null) {
                objects[current].Store(storage);
            }
            GameManager.instance.swap();
            current = c;
            if (current < 0) {
                current = max - 1;
            }
            else if (current >= max) {
                current = 0;
            }
            if (objects[current] != null) {
                objects[current].gameObject.SetActive(true);
                objects[current].Snap(snapPos, true);
                GameManager.instance.MagnetMode(true);
            }
            UIManager.instance.DisplaySelector(current, max);
            Player.instance.ForgetHover();
        }
    }

    public void OnTriggerStay(Collider other) {
        if (objects[current] == null && Player.instance.Magnetized()) {
            if (other.tag == "Collectible") {
                GameManager.instance.crystal();
                collectibleParticles.Play();
                GameManager.instance.RemoveMagnetic(other.GetComponent<MagneticObject>());
                Destroy(other.gameObject);
                Player.instance.GetCollectible();
            }
            else {
                if (other.GetComponent<MagneticObject>() != null) {
                    objects[current] = other.GetComponent<MagneticObject>();
                    if (!objects[current].Snap(snapPos, false)) {
                        objects[current] = null;
                    }
                    else {
                    }
                    GameManager.instance.MagnetMode(true);
                }
            }
            UIManager.instance.SetIcons(objects, max);
        }
    }

    public void Unsnap() {
        objects[current].Unsnap();
        objects[current] = null;
        GameManager.instance.drop();
    }

    public bool Snapped() {
        return objects[current] != null;
    }

    public MagneticObject[] GetObjects() {
        return objects;
    }

    public void Clear(bool death) {
        for(int i = 0; i < objects.Length; ++i) {
            if (objects[i] != null) {
                GameManager.instance.RemoveMagnetic(objects[i]);
                if (death) {
                    Unsnap();
                }
                else {
                    Destroy(objects[i].gameObject);
                }
            }
            objects[i] = null;
        }
        UIManager.instance.SetIcons(objects, max);
    }

    public void SetMax(int m) {
        max = m;
        UIManager.instance.SetIcons(objects, max);
        UIManager.instance.DisplaySelector(current, max);
    }
}
