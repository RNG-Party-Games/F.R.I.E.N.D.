using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFX : MonoBehaviour
{
    public void SetInvoke(string method, float time) {
        Invoke(method, time);
    }

    public void Kill() {
        Destroy(gameObject);
    }
}
