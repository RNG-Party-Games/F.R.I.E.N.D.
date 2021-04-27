using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public List<Image> boxes;
    public List<Image> icons;
    public Image resourceUI;
    public Sprite empty;
    public PostProcessVolume game, ui;
    public Sprite unselected, selected, emptyicon, overmax;
    public TextMeshProUGUI oreNum, scrapNum, radioNum, gearNum, batteryNum, chipNum;
    public Button antenna, storage, movement, battery, magnet;
    public TextMeshProUGUI antennaUpgrade, storageUpgrade, movementUpgrade, batteryUpgrade, magnetUpgrade;
    public Image antennaBar, storageBar, movementBar, batteryBar, magnetBar;
    public GameObject staminabar, itembar, e, bubble, scantext;
    string buttonActive = "#86E7F5", buttonInactive = "#7A8587";
    bool needsTalk = false;
    public bool inUI = false;
    public GameObject popup, fade;

    void Awake() {
        if(instance == null) {
            instance = this;
        }
    }

    private void Start() {
        CalcButtons();
        CloseResources();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CalcButtons() {
        antenna.interactable = CollectionStation.instance.CanUpgradeModule(0);
        antennaUpgrade.text = CollectionStation.instance.GenerateUpgradeText(0);
        antennaBar.fillAmount = Player.instance.GetAntennaBar();

        storage.interactable = CollectionStation.instance.CanUpgradeModule(1);
        storageUpgrade.text = CollectionStation.instance.GenerateUpgradeText(1);
        storageBar.fillAmount = Player.instance.GetStorageBar();

        movement.interactable = CollectionStation.instance.CanUpgradeModule(2);
        movementUpgrade.text = CollectionStation.instance.GenerateUpgradeText(2);
        movementBar.fillAmount = Player.instance.GetMovementBar();

        battery.interactable = CollectionStation.instance.CanUpgradeModule(3);
        batteryUpgrade.text = CollectionStation.instance.GenerateUpgradeText(3);
        batteryBar.fillAmount = Player.instance.GetBatteryBar();

        magnet.interactable = CollectionStation.instance.CanUpgradeModule(4);
        magnetUpgrade.text = CollectionStation.instance.GenerateUpgradeText(4);
        magnetBar.fillAmount = Player.instance.GetMagnetBar();

    }

    public void SetIcons(MagneticObject[] objects, int max) {
        for(int i = 0; i < icons.Count; ++i) {
            if(objects[i] == null || i >= max) {
                icons[i].sprite = emptyicon;
            }
            else {
                icons[i].sprite = objects[i].icon;
            }
        }
    }

    public void DisplaySelector(int index, int max) {
        for(int i = 0; i < boxes.Count; ++i) {
            if(i == index) {
                boxes[i].sprite = selected;
            }
            else {
                boxes[i].sprite = unselected;
            }
            if(i >= max) {
                boxes[i].sprite = overmax;
            }
        }
    }

    public void OpenResources() {
        CalcButtons();
        resourceUI.gameObject.SetActive(true);
        game.weight = 0;
        ui.weight = 1;
        staminabar.SetActive(false);
        itembar.SetActive(false);
        e.SetActive(false);
        scantext.SetActive(false);
        bubble.SetActive(false);
        Player.instance.InUI(true, true);
        inUI = true;
        fade.SetActive(false);
    }

    public void CloseResources() {
        resourceUI.gameObject.SetActive(false);
        game.weight = 1;
        ui.weight = 0;
        staminabar.SetActive(true);
        itembar.SetActive(true);
        e.SetActive(true);
        scantext.SetActive(true);
        bubble.SetActive(true);
        Player.instance.InUI(false, false);
        inUI = false;
        if(needsTalk) {
            needsTalk = false;
            GameManager.instance.talk();
        }
    }

    public void SetResources(int ore, int scrap, int radio, int gears, int batteries, int chips) {
        oreNum.text = ore.ToString();
        scrapNum.text = scrap.ToString();
        radioNum.text = radio.ToString();
        gearNum.text = gears.ToString();
        batteryNum.text = batteries.ToString();
        chipNum.text = chips.ToString();
    }

    public void ShowPopup() {
        popup.SetActive(true);
        Player.instance.InUI(true, false);
        scantext.SetActive(false);
        staminabar.SetActive(false);
        itembar.SetActive(false);
        inUI = true;
    }

    public void ClosePopup() {
        popup.SetActive(false);
        Player.instance.InUI(false, false);
        scantext.SetActive(true);
        staminabar.SetActive(true);
        itembar.SetActive(true);
        inUI = false;
    }

    public void talk() {
        needsTalk = true;
    }
}
