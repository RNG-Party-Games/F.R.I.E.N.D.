using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CollectionStation : MonoBehaviour
{
    public static CollectionStation instance;
    public TextMeshProUGUI managePopup;
    bool playerNear = false;
    int[] resources = { 0, 0, 0, 0, 0, 0 };
    bool[] discovered = { true, true, false, false, false, false };
    string[] resourceNames = { "Ore", "Scrap", "Radio Parts", "Gears", "Batteries", "Microchips" };
    bool collected = false, upgraded = false;

    int[,] antennaUpgrades = new int[,] { { 0, 1, 1, 0, 0, 0 }, { 0, 2, 2, 0, 0, 1 }, { 0, 3, 4, 0, 0, 2 }, { 0, 4, 4, 0, 0, 3 }, { 0, 5, 6, 0, 0, 3 } }; // 5
    int[,] storageUpgrades = new int[,] { { 1, 1, 0, 1, 0, 0 }, { 3, 4, 0, 1, 0, 0 }, { 5, 5, 0, 1, 0, 0 }, { 7, 6, 0, 2, 0, 0 } }; // 4
    int[,] speedUpgrades = new int[,] { { 0, 2, 0, 1, 0, 0 }, { 0, 4, 0, 2, 0, 0 }, { 0, 7, 0, 3, 0, 0 } }; // 3
    int[,] batteryUpgrades = new int[,] { { 2, 1, 0, 0, 1, 0 }, { 3, 2, 0, 0, 2, 0 }, { 5, 2, 0, 0, 3, 0 }, { 6, 3, 0, 0, 4, 0 } }; // 4
    int[,] magnetUpgrades = new int[,] { { 2, 0, 0, 0, 0, 0 }, { 4, 0, 0, 0, 0, 0 } }; // 2

    void Awake()
    {
        if(instance == null) {
            instance = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(playerNear && Input.GetButtonDown("Use") && !UIManager.instance.inUI) {
            OpenStation();
        }
    }

    private void OnTriggerEnter(Collider collider) {
        if(collider.tag == "Player") {
            playerNear = true;
            managePopup.enabled = playerNear;
        }
    }
    private void OnTriggerExit(Collider collider) {
        if (collider.tag == "Player") {
            playerNear = false;
            managePopup.enabled = playerNear;
        }
    }

    public void OpenStation() {
        AddItems(Player.instance.GetInventory());
        Player.instance.Halt();
        UIManager.instance.SetResources(resources[0], resources[1], resources[2], resources[3], resources[4], resources[5]);
        bool empty = true;
        for (int i = 0; i < resources.Length; ++i) {
            if (resources[i] > 0) {
                empty = false;
                if(!collected) {
                    collected = true;
                    SpeechBubble.instance.Advance();
                }
                discovered[i] = true;
            }
        }
        if(!empty) {
            GameManager.instance.give();
        }
        UIManager.instance.OpenResources();
    }

    public bool CanUpgradeModule(int index) {
        int[][,] modules = { antennaUpgrades, storageUpgrades, speedUpgrades, batteryUpgrades, magnetUpgrades };
        int[] progress = { Player.instance.distanceProgress, Player.instance.GetStorage() - 1, Player.instance.speedProgress, Player.instance.batteryProgress, Player.instance.magnetProgress };
        for(int i = 0; i < resources.Length; ++i) {
            if(resources[i] < modules[index][progress[index], i]) {
                return false;
            }
        }
        return true;
    }

    public void UpgradeAntenna() {
        if (CanUpgradeModule(0)) {
            for (int i = 0; i < resources.Length; ++i) {
                resources[i] -= antennaUpgrades[Player.instance.distanceProgress, i];
            }
            UpgradeAdvance();
            Player.instance.IncreaseRange();
        }
        UIManager.instance.CalcButtons();
    }

    public void UpgradeStorage() {
        if (CanUpgradeModule(1)) {
            for (int i = 0; i < resources.Length; ++i) {
                resources[i] -= storageUpgrades[Player.instance.GetStorage()-1, i];
            }
            UpgradeAdvance();
            Player.instance.IncreaseStorage();
        }
        UIManager.instance.CalcButtons();
    }

    public void UpgradeSpeed() {
        if (CanUpgradeModule(2)) {
            for (int i = 0; i < resources.Length; ++i) {
                resources[i] -= speedUpgrades[Player.instance.speedProgress, i];
            }
            UpgradeAdvance();
            Player.instance.IncreaseSpeed();
        }
        UIManager.instance.CalcButtons();
    }

    public void UpgradeBattery() {
        if (CanUpgradeModule(3)) {
            for (int i = 0; i < resources.Length; ++i) {
                resources[i] -= batteryUpgrades[Player.instance.batteryProgress, i];
            }
            UpgradeAdvance();
            Player.instance.IncreaseBattery();
        }
        UIManager.instance.CalcButtons();
    }

    public void UpgradeMagnet() {
        if (CanUpgradeModule(4)) {
            for (int i = 0; i < resources.Length; ++i) {
                resources[i] -= magnetUpgrades[Player.instance.magnetProgress, i];
            }
            UpgradeAdvance();
            Player.instance.IncreaseMagnet();
        }
        UIManager.instance.CalcButtons();
    }

    void UpgradeAdvance() {
        if (!upgraded) {
            upgraded = true;
            SpeechBubble.instance.Advance();
        }
    }

    void AddItems(MagneticObject[] objects) {
        foreach(MagneticObject o in objects) {
            if(o != null) {
                resources[(int) o.type]++;
            }
        }
        Player.instance.ClearInventory();
        Player.instance.Charge();
    }

    public string GenerateUpgradeText(int index) {
        int[][,] modules = { antennaUpgrades, storageUpgrades, speedUpgrades, batteryUpgrades, magnetUpgrades };
        int[] progress = { Player.instance.distanceProgress, Player.instance.GetStorage() - 1, Player.instance.speedProgress, Player.instance.batteryProgress, Player.instance.magnetProgress };
        string text = "";
        int lines = 0;
        for(int i = 0; i < resources.Length; ++i) {
            int req = modules[index][progress[index], i];
            if (req > 0) { // there is a resource required here
                if(lines > 0) {
                    text += "\n";
                }
                if (discovered[i]) {
                    text += req + " " + resourceNames[i];
                }
                else {
                    text += req + " ???";
                }
                lines++;
            }
        }
        return text;
    }
}
