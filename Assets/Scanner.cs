using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Scanner : MonoBehaviour
{
    bool scanning = false;
    public TextMeshProUGUI catalogText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Scan(bool scan) {
        scanning = scan;
        catalogText.text = "";
    }

    private void OnTriggerStay(Collider other) {
        if (scanning && other.tag == "Fish") {
            Player.instance.Catalog(other.GetComponent<Fish>());
            catalogText.text = other.GetComponent<Fish>().name;
        }
        else if(!scanning && other.tag == "Magnetic") {
            catalogText.text = other.GetComponent<MagneticObject>().type.ToString();
        }
    }
    private void OnTriggerExit(Collider other) {
        if (scanning && other.tag == "Fish") {
            catalogText.text = "";
        }
        else if(!scanning && other.tag == "Magnetic") {
            catalogText.text = "";
        }
    }
}
