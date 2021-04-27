using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BatteryIndicator : MonoBehaviour
{
    public Image ring, ring2, ring3, bolt;
    public Color full;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetBattery(float level, float max, float alpha) {
        ring.fillAmount = Mathf.Clamp(level, 0, 1);
        ring2.fillAmount = Mathf.Clamp(level - 1, 0, 1);
        ring3.fillAmount = Mathf.Clamp(level - 2, 0, 1);
        Color current = new Color(full.r, full.g * (level / max), full.b * (level / max), alpha);
        bolt.color = new Color(1, 1, 1, alpha);
        ring.color = current;
        ring2.color = current;
        ring3.color = current;
    }
}
