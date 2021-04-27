using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpeechBubble : MonoBehaviour
{
    public static SpeechBubble instance;
    public List<string> tutorials;
    public TextMeshProUGUI text;
    public List<SpriteRenderer> arrows;
    int index = 0;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        text.text = tutorials[index];
        foreach(SpriteRenderer sr in arrows) {
            sr.enabled = false;
        }
        arrows[index].enabled = true;
    }

    public void Advance() {
            if (index < arrows.Count && arrows[index] != null) {
                arrows[index].enabled = false;
        }
        if (index < tutorials.Count - 1) {
            index++;
            UIManager.instance.talk();
            text.text = tutorials[index];
            if (index < arrows.Count && arrows[index] != null) {
                arrows[index].enabled = true;
            }
        }
    }
    
}
