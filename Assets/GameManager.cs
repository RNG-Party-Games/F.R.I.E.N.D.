using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    List<MagneticObject> objects;
    public Material magnetmat, collectiblemat;
    public Light directional;
    float time = -1000;
    public Color sky;
    public float fogMod = 1;
    public GameObject SFX;
    public AudioClip armsfx, crystalsfx, dropsfx, givesfx, swapsfx, batterysfx, signalsfx, magnetloop, menuclick, menuclose, menuopen, pickupsfx, diesfxfast, diesfx, scansfx, swimsfx, upgradesfx, scannewsfx, girl1;
    public AudioSource magnetLoop, scanLoop, armSource;
    // Start is called before the first frame update
    void Awake()
    {
        if(instance == null) {
            instance = this;
        }
    }

    private void Start() {
        objects = new List<MagneticObject>(FindObjectsOfType<MagneticObject>());
        Player.instance.maxcollectibles = 0;
        foreach (MagneticObject m in objects) {
            if (m.type == MagneticObject.ItemType.Collectible) {
                Player.instance.maxcollectibles++;
            }
        }
        Player.instance.DisplayCollectibles();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate() {
        time += 0.2f;
        if(time > 1000) {
            time = -1000;
        }
        float mod = Mathf.Abs(time / 1000);
        RenderSettings.skybox.SetColor("_SkyTint", new Color(sky.r*mod, sky.g*mod, sky.b*mod));
        RenderSettings.skybox.SetFloat("_Exposure", 3*mod + 0.2f);
        fogMod = mod;
        directional.intensity = mod;
    }

    public void PlaySFX(AudioClip clip, float volume) {
        GameObject newSFX = Instantiate(SFX, transform);
        AudioSource source = newSFX.GetComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.Play();
        newSFX.GetComponent<SFX>().SetInvoke("Kill", clip.length);
    }

    public void arm() {
        armSource.Play();
    }

    public void crystal() {
        PlaySFX(crystalsfx, 1);
    }

    public void give() {
        PlaySFX(givesfx, 0.4f);
    }

    public void drop() {
        PlaySFX(dropsfx, 1);
    }

    public void pickup() {
        PlaySFX(pickupsfx, 1);
    }

    public void upgrade() {
        PlaySFX(upgradesfx, 1);
    }

    public void swap() {
        PlaySFX(swapsfx, 1);
    }

    public void talk() {
        PlaySFX(girl1, 1);
    }
    
    public void magnet(bool on) {
        if(on && !magnetLoop.isPlaying) {
            magnetLoop.Play();
        }
        else if(!on && magnetLoop.isPlaying) {
            magnetLoop.Stop();
        }
    }

    public void scan(bool on) {
        if (on && !scanLoop.isPlaying) {
            scanLoop.Play();
        }
        else if (!on && scanLoop.isPlaying) {
            scanLoop.Stop();
        }
    }

    public void scannew() {
        PlaySFX(scannewsfx, 1);
    }

    public void lowsignal() {
        PlaySFX(signalsfx, 1);
    }

    public void lowbattery() {
        PlaySFX(batterysfx, 1);
    }

    public void die() {
        PlaySFX(diesfxfast, 1);
    }

    public void MagnetMode(bool magnet) {
        foreach(MagneticObject m in objects) {
            if (m.type == MagneticObject.ItemType.Collectible) {
                m.Magnetize(collectiblemat, magnet);
            }
            else {
                m.Magnetize(magnetmat, magnet);
            }
        }
    }

    public void RemoveMagnetic(MagneticObject m) {
        objects.Remove(m);
    }
}
