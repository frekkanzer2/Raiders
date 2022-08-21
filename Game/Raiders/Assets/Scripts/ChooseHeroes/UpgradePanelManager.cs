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

    private Upgrade up_std, up_3;
    private Upgrade cumulative;

    private string _available_msg = "Available points: ";
    private string _effective_msg = "Effective bonus: ";
    private string _three_msg = "Challenge bonus: ";

    private int chosen_powerupIndex = 0;

    public Sprite[] iconsList;

    public void OnEnable() {
        this.cumulative = new Upgrade();
        this.up_std = DUNSelectionManager.UPGRADE;
        this.up_3 = DUNSelectionManager.ADDITIONAL_UPGRADE;
        this.cumulative.cumulateHere(this.up_std);
        this.cumulative.cumulateHere(this.up_3);
        OnPressHP();
        availablePointsText.GetComponent<TextMeshProUGUI>().text = _available_msg + DUNSelectionManager.UPGRADE.pointsToAssign;
        foreach (Transform child in containerButtonsPowerups.transform)
            setCardPoints(child.gameObject, 0);
    }

    private void setPreview(string title, int actualPoints, int bonusPoints, Sprite image, string effectiveBonus, string threebonus, int id) {
        chosen_powerupIndex = id;
        chosen_powerupName.GetComponent<TextMeshProUGUI>().text = title;
        chosen_powerupLevel.GetComponent<TextMeshProUGUI>().text = "" + actualPoints + " + " + bonusPoints;
        chosen_powerupEffectiveBonus.GetComponent<TextMeshProUGUI>().text = _effective_msg + effectiveBonus + System.Environment.NewLine + _three_msg + threebonus;
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
        string bonusval = "+" + cumulative.getHpBonus() + " HP | +" + cumulative.getHealBonus() + " heals";
        string bonusval2 = "+" + up_std.getHpBonus() + " HP | +" + up_std.getHealBonus() + " heals";
        setPreview("Heart Points Bonus", up_std.hpLevel, up_3.hpLevel, iconsList[0], bonusval2, bonusval, 0);
    }
    public void OnPressShield() {
        string bonusval = "+" + cumulative.getShieldBonus() + " Shield | +" + cumulative.getGainShieldBonus() + " gains on shield";
        string bonusval2 = "+" + up_std.getShieldBonus() + " Shield | +" + up_std.getGainShieldBonus() + " gains on shield";
        setPreview("Starting Shield Bonus", up_std.startingShield, up_3.startingShield, iconsList[1], bonusval2, bonusval, 1);
    }
    public void OnPressPA() {
        string bonusval = "+" + cumulative.getPaBonus() + " PA";
        string bonusval2 = "+" + up_std.getPaBonus() + " PA";
        setPreview("Action Points (PA) Bonus", up_std.paLevel, up_3.paLevel, iconsList[2], bonusval2, bonusval, 2);
    }
    public void OnPressPM() {
        string bonusval = "+" + cumulative.getPmBonus() + " PM";
        string bonusval2 = "+" + up_std.getPmBonus() + " PM";
        setPreview("Movement Points (PM) Bonus", up_std.pmLevel, up_3.pmLevel, iconsList[3], bonusval2, bonusval, 3);
    }
    public void OnPressINI() {
        string bonusval = "+" + cumulative.getInitBonus() + " INI";
        string bonusval2 = "+" + up_std.getInitBonus() + " INI";
        setPreview("Initiative Bonus", up_std.initLevel, up_3.initLevel, iconsList[4], bonusval2, bonusval, 4);
    }
    public void OnPressATK() {
        Tuple<int, int, int, int> t = cumulative.getAttackBonus();
        Tuple<int, int, int, int> t2 = up_std.getAttackBonus();
        string bonusval = "+" + t.Item1 + "% Earth damage | +" + t.Item2 + " % Fire damage | +" + t.Item3 + " % Air damage | +" + t.Item4 + " % Water damage";
        string bonusval2 = "+" + t2.Item1 + "% Earth damage | +" + t2.Item2 + " % Fire damage | +" + t2.Item3 + " % Air damage | +" + t2.Item4 + " % Water damage";
        setPreview("Damage Bonus", up_std.allAtkLevel, up_3.allAtkLevel, iconsList[5], bonusval2, bonusval, 5);
    }
    public void OnPressATK_E() {
        Tuple<int, int, int, int> t = cumulative.getAttackBonus();
        Tuple<int, int, int, int> t2 = up_std.getAttackBonus();
        string bonusval = "+" + t.Item1 + "% Earth damage | +" + t.Item2 + " % Fire damage | +" + t.Item3 + " % Air damage | +" + t.Item4 + " % Water damage";
        string bonusval2 = "+" + t2.Item1 + "% Earth damage | +" + t2.Item2 + " % Fire damage | +" + t2.Item3 + " % Air damage | +" + t2.Item4 + " % Water damage";
        setPreview("Earth Damage Bonus", up_std.atkEarthLevel, up_3.atkEarthLevel, iconsList[5], bonusval2, bonusval, 6);
    }
    public void OnPressATK_F() {
        Tuple<int, int, int, int> t = cumulative.getAttackBonus();
        Tuple<int, int, int, int> t2 = up_std.getAttackBonus();
        string bonusval = "+" + t.Item1 + "% Earth damage | +" + t.Item2 + " % Fire damage | +" + t.Item3 + " % Air damage | +" + t.Item4 + " % Water damage";
        string bonusval2 = "+" + t2.Item1 + "% Earth damage | +" + t2.Item2 + " % Fire damage | +" + t2.Item3 + " % Air damage | +" + t2.Item4 + " % Water damage";
        setPreview("Fire Damage Bonus", up_std.atkFireLevel, up_3.atkFireLevel, iconsList[5], bonusval2, bonusval, 7);
    }
    public void OnPressATK_A() {
        Tuple<int, int, int, int> t = cumulative.getAttackBonus();
        Tuple<int, int, int, int> t2 = up_std.getAttackBonus();
        string bonusval = "+" + t.Item1 + "% Earth damage | +" + t.Item2 + " % Fire damage | +" + t.Item3 + " % Air damage | +" + t.Item4 + " % Water damage";
        string bonusval2 = "+" + t2.Item1 + "% Earth damage | +" + t2.Item2 + " % Fire damage | +" + t2.Item3 + " % Air damage | +" + t2.Item4 + " % Water damage";
        setPreview("Air Damage Bonus", up_std.atkAirLevel, up_3.atkAirLevel, iconsList[5], bonusval2, bonusval, 8);
    }
    public void OnPressATK_W() {
        Tuple<int, int, int, int> t = cumulative.getAttackBonus();
        Tuple<int, int, int, int> t2 = up_std.getAttackBonus();
        string bonusval = "+" + t.Item1 + "% Earth damage | +" + t.Item2 + " % Fire damage | +" + t.Item3 + " % Air damage | +" + t.Item4 + " % Water damage";
        string bonusval2 = "+" + t2.Item1 + "% Earth damage | +" + t2.Item2 + " % Fire damage | +" + t2.Item3 + " % Air damage | +" + t2.Item4 + " % Water damage";
        setPreview("Water Damage Bonus", up_std.atkWaterLevel, up_3.atkWaterLevel, iconsList[5], bonusval2, bonusval, 9);
    }
    public void OnPressDEF() {
        Tuple<int, int, int, int> t = cumulative.getDefenceBonus();
        Tuple<int, int, int, int> t2 = up_std.getDefenceBonus();
        string bonusval = "+" + t.Item1 + "% Earth RES | +" + t.Item2 + " % Fire RES | +" + t.Item3 + " % Air RES | +" + t.Item4 + " % Water RES";
        string bonusval2 = "+" + t2.Item1 + "% Earth RES | +" + t2.Item2 + " % Fire RES | +" + t2.Item3 + " % Air RES | +" + t2.Item4 + " % Water RES";
        setPreview("Resistence Bonus", up_std.allDefLevel, up_3.allDefLevel, iconsList[6], bonusval2, bonusval, 10);
    }
    public void OnPressDEF_E() {
        Tuple<int, int, int, int> t = cumulative.getDefenceBonus();
        Tuple<int, int, int, int> t2 = up_std.getDefenceBonus();
        string bonusval = "+" + t.Item1 + "% Earth RES | +" + t.Item2 + " % Fire RES | +" + t.Item3 + " % Air RES | +" + t.Item4 + " % Water RES";
        string bonusval2 = "+" + t2.Item1 + "% Earth RES | +" + t2.Item2 + " % Fire RES | +" + t2.Item3 + " % Air RES | +" + t2.Item4 + " % Water RES";
        setPreview("Earth Resistence Bonus", up_std.defEarthLevel, up_3.defEarthLevel, iconsList[6], bonusval2, bonusval, 11);
    }
    public void OnPressDEF_F() {
        Tuple<int, int, int, int> t = cumulative.getDefenceBonus();
        Tuple<int, int, int, int> t2 = up_std.getDefenceBonus();
        string bonusval = "+" + t.Item1 + "% Earth RES | +" + t.Item2 + " % Fire RES | +" + t.Item3 + " % Air RES | +" + t.Item4 + " % Water RES";
        string bonusval2 = "+" + t2.Item1 + "% Earth RES | +" + t2.Item2 + " % Fire RES | +" + t2.Item3 + " % Air RES | +" + t2.Item4 + " % Water RES";
        setPreview("Fire Resistence Bonus", up_std.defFireLevel, up_3.defFireLevel, iconsList[6], bonusval2, bonusval, 12);
    }
    public void OnPressDEF_A() {
        Tuple<int, int, int, int> t = cumulative.getDefenceBonus();
        Tuple<int, int, int, int> t2 = up_std.getDefenceBonus();
        string bonusval = "+" + t.Item1 + "% Earth RES | +" + t.Item2 + " % Fire RES | +" + t.Item3 + " % Air RES | +" + t.Item4 + " % Water RES";
        string bonusval2 = "+" + t2.Item1 + "% Earth RES | +" + t2.Item2 + " % Fire RES | +" + t2.Item3 + " % Air RES | +" + t2.Item4 + " % Water RES";
        setPreview("Air Resistence Bonus", up_std.defAirLevel, up_3.defAirLevel, iconsList[6], bonusval2, bonusval, 13);
    }
    public void OnPressDEF_W() {
        Tuple<int, int, int, int> t = cumulative.getDefenceBonus();
        Tuple<int, int, int, int> t2 = up_std.getDefenceBonus();
        string bonusval = "+" + t.Item1 + "% Earth RES | +" + t.Item2 + " % Fire RES | +" + t.Item3 + " % Air RES | +" + t.Item4 + " % Water RES";
        string bonusval2 = "+" + t2.Item1 + "% Earth RES | +" + t2.Item2 + " % Fire RES | +" + t2.Item3 + " % Air RES | +" + t2.Item4 + " % Water RES";
        setPreview("Water Resistence Bonus", up_std.defWaterLevel, up_3.defWaterLevel, iconsList[6], bonusval2, bonusval, 14);
    }

    public void OnPressSUMMONS() {
        string bonusval = "+" + cumulative.getSummonsBonus() + " SUMMONS";
        string bonusval2 = "+" + up_std.getSummonsBonus() + " SUMMONS";
        setPreview("Number of Summons Bonus", up_std.evocationLevel, up_3.evocationLevel, iconsList[7], bonusval2, bonusval, 15);
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
            this.cumulative = new Upgrade();
            this.up_std = DUNSelectionManager.UPGRADE;
            this.up_3 = DUNSelectionManager.ADDITIONAL_UPGRADE;
            this.cumulative.cumulateHere(this.up_std);
            this.cumulative.cumulateHere(this.up_3);
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
