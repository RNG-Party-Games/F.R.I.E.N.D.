using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Fish : MonoBehaviour
{
    public int index;
    public string name;
    CinemachineDollyCart dolly;
    // Start is called before the first frame update
    void Start()
    {
        dolly = GetComponent<CinemachineDollyCart>();
    }

    // Update is called once per frame
    void Update()
    {
    }
}
