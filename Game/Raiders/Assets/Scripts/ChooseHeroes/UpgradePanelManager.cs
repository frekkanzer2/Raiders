using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePanelManager : MonoBehaviour
{

    public GameObject containerButtonsPowerups;
    public GameObject availablePointsText;
    public GameObject chosen_powerupImage;
    public GameObject chosen_powerupName;
    public GameObject chosen_powerupLevel;
    public GameObject chosen_powerupEffectiveBonus;
    private List<GameObject> powerup_cards;

    private string _available_msg = "Available points: ";
    private string _effective_msg = "Effective bonus: ";

    private int chosen_powerupIndex = 0;

    public Sprite[] iconsList;

    public void OnEnable() {
        string _bonusval = "+" + DUNSelectionManager.UPGRADE.getHpBonus() + " HP | +" + DUNSelectionManager.UPGRADE.getHealBonus() + " heals";
        setPreview("Heart Points Bonus", 0, iconsList[0], _bonusval, 0);
        availablePointsText.GetComponent<TextMeshProUGUI>().text = _available_msg + DUNSelectionManager.UPGRADE.pointsToAssign;
        foreach (Transform child in containerButtonsPowerups.transform)
            setCardPoints(child.gameObject, 0);
    }

    private void setPreview(string title, int actualPoints, Sprite image, string effectiveBonus, int id) {
        chosen_powerupIndex = id;
        chosen_powerupName.GetComponent<TextMeshProUGUI>().text = title;
        chosen_powerupLevel.GetComponent<TextMeshProUGUI>().text = "" + actualPoints;
        chosen_powerupEffectiveBonus.GetComponent<TextMeshProUGUI>().text = _effective_msg + effectiveBonus;
        chosen_powerupImage.GetComponent<Image>().sprite = image;
        availablePointsText.GetComponent<TextMeshProUGUI>().text = _available_msg + DUNSelectionManager.UPGRADE.pointsToAssign;
        prioritizeCard(id);
        setCardPoints(id, actualPoints);
    }

    // call this function to flag a card as chosen
    private void prioritizeCard(int index) {
        if (powerup_cards == null) {
            powerup_cards = new List<GameObject>();
            foreach (Transform child in containerButtonsPowerups.transform) {
                powerup_cards.Add(child.gameObject);
                setCardAsChosen(child.gameObject, false);
                setCardPoints(child.gameObject, 0);
            }
        }
        for (int i = 0; i < powerup_cards.Count; i++)
            if (index == i) setCardAsChosen(powerup_cards[i]);
            else setCardAsChosen(powerup_cards[i], false);
    }

    // do not call it on downside functions
    private void setCardAsChosen(GameObject card, bool isChosen = true) {
        GameObject chosen = card.transform.GetChild(0).gameObject;
        if (!isChosen) chosen.SetActive(false);
        else chosen.SetActive(true);
    }

    // do not call it on downside functions
    private void setCardPoints(GameObject card, int points) {
        GameObject val = card.transform.GetChild(2).gameObject;
        val.GetComponent<TextMeshProUGUI>().text = "" + points;
    }
    private void setCardPoints(int id, int points) {
        for (int i = 0; i < powerup_cards.Count; i++)
            if (id == i) {
                GameObject val = powerup_cards[i].transform.GetChild(2).gameObject;
                val.GetComponent<TextMeshProUGUI>().text = "" + points;
                break;
            }
    }

    #region OnPress Cards

    public void OnPressHP() {
        string bonusval = "+" + DUNSelectionManager.UPGRADE.getHpBonus() + " HP | +" + DUNSelectionManager.UPGRADE.getHealBonus() + " heals";
        setPreview("Heart Points Bonus", DUNSelectionManager.UPGRADE.hpLevel, iconsList[0], bonusval, 0);
    }
    public void OnPressShield() {
        string bonusval = "+" + DUNSelectionManager.UPGRADE.getShieldBonus() + " Shield | +" + DUNSelectionManager.UPGRADE.getGainShieldBonus() + " gains on shield";
        setPreview("Starting Shield Bonus", DUNSelectionManager.UPGRADE.startingShield, iconsList[1], bonusval, 1);
    }
    public void OnPressPA() {
        string bonusval = "+" + DUNSelectionManager.UPGRADE.getPaBonus() + " PA";
        setPreview("Action Points (PA) Bonus", DUNSelectionManager.UPGRADE.paLevel, iconsList[2], bonusval, 2);
    }
    public void OnPressPM() {
        string bonusval = "+" + DUNSelectionManager.UPGRADE.getPmBonus() + " PM";
        setPreview("Movement Points (PM) Bonus", DUNSelectionManager.UPGRADE.pmLevel, iconsList[3], bonusval, 3);
    }
    public void OnPressINI() {
        string bonusval = "+" + DUNSelectionManager.UPGRADE.getInitBonus() + " INI";
        setPreview("Initiative Bonus", DUNSelectionManager.UPGRADE.initLevel, iconsList[4], bonusval, 4);
    }
    public void OnPressATK() {
        Tuple<int, int, int, int> t = DUNSelectionManager.UPGRADE.getAttackBonus();
        string bonusval = "+" + t.Item1 + "% Earth damage | +" + t.Item2 + " % Fire damage | +" + t.Item3 + " % Air damage | +" + t.Item4 + " % Water damage";
        setPreview("Damage Bonus", DUNSelectionManager.UPGRADE.allAtkLevel, iconsList[5], bonusval, 5);
    }
    public void OnPressATK_E() {
        Tuple<int, int, int, int> t = DUNSelectionManager.UPGRADE.getAttackBonus();
        string bonusval = "+" + t.Item1 + "% Earth damage | +" + t.Item2 + " % Fire damage | +" + t.Item3 + " % Air damage | +" + t.Item4 + " % Water damage";
        setPreview("Earth Damage Bonus", DUNSelectionManager.UPGRADE.atkEarthLevel, iconsList[5], bonusval, 6);
    }
    public void OnPressATK_F() {
        Tuple<int, int, int, int> t = DUNSelectionManager.UPGRADE.getAttackBonus();
        string bonusval = "+" + t.Item1 + "% Earth damage | +" + t.Item2 + " % Fire damage | +" + t.Item3 + " % Air damage | +" + t.Item4 + " % Water damage";
        setPreview("Fire Damage Bonus", DUNSelectionManager.UPGRADE.atkFireLevel, iconsList[5], bonusval, 7);
    }
    public void OnPressATK_A() {
        Tuple<int, int, int, int> t = DUNSelectionManager.UPGRADE.getAttackBonus();
        string bonusval = "+" + t.Item1 + "% Earth damage | +" + t.Item2 + " % Fire damage | +" + t.Item3 + " % Air damage | +" + t.Item4 + " % Water damage";
        setPreview("Air Damage Bonus", DUNSelectionManager.UPGRADE.atkAirLevel, iconsList[5], bonusval, 8);
    }
    public void OnPressATK_W() {
        Tuple<int, int, int, int> t = DUNSelectionManager.UPGRADE.getAttackBonus();
        string bonusval = "+" + t.Item1 + "% Earth damage | +" + t.Item2 + " % Fire damage | +" + t.Item3 + " % Air damage | +" + t.Item4 + " % Water damage";
        setPreview("Water Damage Bonus", DUNSelectionManager.UPGRADE.atkWaterLevel, iconsList[5], bonusval, 9);
    }
    public void OnPressDEF() {
        Tuple<int, int, int, int> t = DUNSelectionManager.UPGRADE.getDefenceBonus();
        string bonusval = "+" + t.Item1 + "% Earth RES | +" + t.Item2 + " % Fire RES | +" + t.Item3 + " % Air RES | +" + t.Item4 + " % Water RES";
        setPreview("Resistence Bonus", DUNSelectionManager.UPGRADE.allDefLevel, iconsList[6], bonusval, 10);
    }
    public void OnPressDEF_E() {
        Tuple<int, int, int, int> t = DUNSelectionManager.UPGRADE.getDefenceBonus();
        string bonusval = "+" + t.Item1 + "% Earth RES | +" + t.Item2 + " % Fire RES | +" + t.Item3 + " % Air RES | +" + t.Item4 + " % Water RES";
        setPreview("Earth Resistence Bonus", DUNSelectionManager.UPGRADE.defEarthLevel, iconsList[6], bonusval, 11);
    }
    public void OnPressDEF_F() {
        Tuple<int, int, int, int> t = DUNSelectionManager.UPGRADE.getDefenceBonus();
        string bonusval = "+" + t.Item1 + "% Earth RES | +" + t.Item2 + " % Fire RES | +" + t.Item3 + " % Air RES | +" + t.Item4 + " % Water RES";
        setPreview("Fire Resistence Bonus", DUNSelectionManager.UPGRADE.defFireLevel, iconsList[6], bonusval, 12);
    }
    public void OnPressDEF_A() {
        Tuple<int, int, int, int> t = DUNSelectionManager.UPGRADE.getDefenceBonus();
        string bonusval = "+" + t.Item1 + "% Earth RES | +" + t.Item2 + " % Fire RES | +" + t.Item3 + " % Air RES | +" + t.Item4 + " % Water RES";
        setPreview("Air Resistence Bonus", DUNSelectionManager.UPGRADE.defAirLevel, iconsList[6], bonusval, 13);
    }
    public void OnPressDEF_W() {
        Tuple<int, int, int, int> t = DUNSelectionManager.UPGRADE.getDefenceBonus();
        string bonusval = "+" + t.Item1 + "% Earth RES | +" + t.Item2 + " % Fire RES | +" + t.Item3 + " % Air RES | +" + t.Item4 + " % Water RES";
        setPreview("Water Resistence Bonus", DUNSelectionManager.UPGRADE.defWaterLevel, iconsList[6], bonusval, 14);
    }

    public void OnPressSUMMONS() {
        string bonusval = "+" + DUNSelectionManager.UPGRADE.getSummonsBonus() + " SUMMONS";
        setPreview("Number of Summons Bonus", DUNSelectionManager.UPGRADE.evocationLevel, iconsList[7], bonusval, 15);
    }

    #endregion

    #region OnPress Quantifier Buttons

    public void OnPress_PlusOne() {
        executePowerup(1);
    }
    public void OnPress_PlusFive() {
        executePowerup(5);
    }
    public void OnPress_PlusTen() {
        executePowerup(10);
    }
    public void OnPress_MinusOne() {
        executePowerup(-1);
    }
    public void OnPress_MinusFive() {
        executePowerup(-5);
    }
    public void OnPress_MinusTen() {
        executePowerup(-10);
    }
    public void executePowerup(int variation) {
        bool result = true;
        switch(chosen_powerupIndex) {
            case 0:
                result = DUNSelectionManager.UPGRADE.executePowerup_hpLevel(variation);
                break;
            case 1:
                result = DUNSelectionManager.UPGRADE.executePowerup_startingShield(variation);
                break;
            case 2:
                result = DUNSelectionManager.UPGRADE.executePowerup_paLevel(variation);
                break;
            case 3:
                result = DUNSelectionManager.UPGRADE.executePowerup_pmLevel(variation);
                break;
            case 4:
                result = DUNSelectionManager.UPGRADE.executePowerup_initLevel(variation);
                break;
            case 5:
                result = DUNSelectionManager.UPGRADE.executePowerup_allAtkLevel(variation);
                break;
            case 6:
                result = DUNSelectionManager.UPGRADE.executePowerup_atkEarthLevel(variation);
                break;
            case 7:
                result = DUNSelectionManager.UPGRADE.executePowerup_atkFireLevel(variation);
                break;
            case 8:
                result = DUNSelectionManager.UPGRADE.executePowerup_atkAirLevel(variation);
                break;
            case 9:
                result = DUNSelectionManager.UPGRADE.executePowerup_atkWaterLevel(variation);
                break;
            case 10:
                result = DUNSelectionManager.UPGRADE.executePowerup_allDefLevel(variation);
                break;
            case 11:
                result = DUNSelectionManager.UPGRADE.executePowerup_defEarthLevel(variation);
                break;
            case 12:
                result = DUNSelectionManager.UPGRADE.executePowerup_defFireLevel(variation);
                break;
            case 13:
                result = DUNSelectionManager.UPGRADE.executePowerup_defAirLevel(variation);
                break;
            case 14:
                result = DUNSelectionManager.UPGRADE.executePowerup_defWaterLevel(variation);
                break;
            case 15:
                result = DUNSelectionManager.UPGRADE.executePowerup_evocationLevel(variation);
                break;
        }
        if (result) {
            // can update GUI
            switch (chosen_powerupIndex) {
                case 0:
                    OnPressHP();
                    break;
                case 1:
                    OnPressShield();
                    break;
                case 2:
                    OnPressPA();
                    break;
                case 3:
                    OnPressPM();
                    break;
                case 4:
                    OnPressINI();
                    break;
                case 5:
                    OnPressATK();
                    break;
                case 6:
                    OnPressATK_E();
                    break;
                case 7:
                    OnPressATK_F();
                    break;
                case 8:
                    OnPressATK_A();
                    break;
                case 9:
                    OnPressATK_W();
                    break;
                case 10:
                    OnPressDEF();
                    break;
                case 11:
                    OnPressDEF_E();
                    break;
                case 12:
                    OnPressDEF_F();
                    break;
                case 13:
                    OnPressDEF_A();
                    break;
                case 14:
                    OnPressDEF_W();
                    break;
                case 15:
                    OnPressSUMMONS();
                    break;
            }
        }
    }

    #endregion

}
