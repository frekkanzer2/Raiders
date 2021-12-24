using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StatsPanel : MonoBehaviour
{

    public Character toSync;
    private CharactersLibrary cl;

    public GameObject characterPreview;
    public GameObject res_ear;
    public GameObject res_fir;
    public GameObject res_air;
    public GameObject res_wat;
    public GameObject att_ear;
    public GameObject att_fir;
    public GameObject att_air;
    public GameObject att_wat;
    public GameObject hpGui;
    public GameObject paGui;
    public GameObject pmGui;
    public GameObject btnPanelAttDef;
    public GameObject panelAttDef;
    public GameObject spell1;
    public GameObject spell2;
    public GameObject spell3;
    public GameObject spell4;
    private bool canUse1;
    private bool canUse2;
    private bool canUse3;
    private bool canUse4;

    private int stats_att_def_0_1 = 0;

    public void set(Character c, CharactersLibrary cl) {
        this.toSync = c;
        this.cl = cl;
    }

    private void Start() {

    }

    private void Update() {
        if (toSync != null) {

            if (!(toSync is Monster))
                characterPreview.GetComponent<Image>().sprite = cl.getCharacterInfoByName(toSync.name).characterMidSprite;
            else
                characterPreview.GetComponent<Image>().sprite = cl.getCharacterInfoMonster((Monster) toSync).characterMidSprite;
            hpGui.GetComponent<TextMeshProUGUI>().text = "" + toSync.getActualHP();
            paGui.GetComponent<TextMeshProUGUI>().text = "" + toSync.getActualPA();
            pmGui.GetComponent<TextMeshProUGUI>().text = "" + toSync.getActualPM();
            res_ear.GetComponent<TextMeshProUGUI>().text = "" + toSync.res_e + "%";
            res_fir.GetComponent<TextMeshProUGUI>().text = "" + toSync.res_f + "%";
            res_air.GetComponent<TextMeshProUGUI>().text = "" + toSync.res_a + "%";
            res_wat.GetComponent<TextMeshProUGUI>().text = "" + toSync.res_w + "%";
            att_ear.GetComponent<TextMeshProUGUI>().text = "" + toSync.att_e + "%";
            att_fir.GetComponent<TextMeshProUGUI>().text = "" + toSync.att_f + "%";
            att_air.GetComponent<TextMeshProUGUI>().text = "" + toSync.att_a + "%";
            att_wat.GetComponent<TextMeshProUGUI>().text = "" + toSync.att_w + "%";

            if (toSync.spells.Count < 4) {
                spell1.GetComponent<SpellSlot>().removeSpell();
                spell2.GetComponent<SpellSlot>().removeSpell();
                spell3.GetComponent<SpellSlot>().removeSpell();
                spell4.GetComponent<SpellSlot>().removeSpell();
            }
            
            if (toSync.spells.Count >= 1)
                spell1.GetComponent<SpellSlot>().setSpell(toSync.spells[0]);
            if (toSync.spells.Count >= 2)
                spell2.GetComponent<SpellSlot>().setSpell(toSync.spells[1]);
            if (toSync.spells.Count >= 3)
                spell3.GetComponent<SpellSlot>().setSpell(toSync.spells[2]);
            if (toSync.spells.Count == 4)
                spell4.GetComponent<SpellSlot>().setSpell(toSync.spells[3]);

            GameObject panel_turnsWait1 = spell1.transform.GetChild(0).gameObject;
            TextMeshProUGUI text_turnsWait1 = panel_turnsWait1.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
            GameObject panel_turnsWait2 = spell2.transform.GetChild(0).gameObject;
            TextMeshProUGUI text_turnsWait2 = panel_turnsWait2.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
            GameObject panel_turnsWait3 = spell3.transform.GetChild(0).gameObject;
            TextMeshProUGUI text_turnsWait3 = panel_turnsWait3.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
            GameObject panel_turnsWait4 = spell4.transform.GetChild(0).gameObject;
            TextMeshProUGUI text_turnsWait4 = panel_turnsWait4.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
            if (toSync.spells.Count >= 1)
                if (!Spell.canUse(toSync, toSync.spells[0])) {
                    spell1.GetComponent<Image>().color = new Color(80f / 255f, 80f / 255f, 80f / 255f, 1);
                    spell1.GetComponent<SpellSlot>().canUse = false;
                    int remaining_turns = Spell.getRemainingTurns(toSync, spell1.GetComponent<SpellSlot>().connectedSpell);
                    if (remaining_turns > 0) {
                        // Active and set text
                        panel_turnsWait1.SetActive(true);
                        text_turnsWait1.text = "" + remaining_turns;
                    } else {
                        // Deactive
                        panel_turnsWait1.SetActive(false);
                    }
                } else {
                    spell1.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                    spell1.GetComponent<SpellSlot>().canUse = true;
                    panel_turnsWait1.SetActive(false);
                }
            else
                panel_turnsWait1.SetActive(false);

            if (toSync.spells.Count >= 2)
                if (!Spell.canUse(toSync, toSync.spells[1])) {
                    spell2.GetComponent<Image>().color = new Color(80f / 255f, 80f / 255f, 80f / 255f, 1);
                    spell2.GetComponent<SpellSlot>().canUse = false;
                    int remaining_turns = Spell.getRemainingTurns(toSync, spell2.GetComponent<SpellSlot>().connectedSpell);
                    if (remaining_turns > 0) {
                        // Active and set text
                        panel_turnsWait2.SetActive(true);
                        text_turnsWait2.text = "" + remaining_turns;
                    } else {
                        // Deactive
                        panel_turnsWait2.SetActive(false);
                    }
                } else {
                    spell2.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                    spell2.GetComponent<SpellSlot>().canUse = true;
                    panel_turnsWait2.SetActive(false);
                }
            else
                panel_turnsWait2.SetActive(false);

            if (toSync.spells.Count >= 3)
                if (!Spell.canUse(toSync, toSync.spells[2])) {
                    spell3.GetComponent<Image>().color = new Color(80f / 255f, 80f / 255f, 80f / 255f, 1);
                    spell3.GetComponent<SpellSlot>().canUse = false;
                    int remaining_turns = Spell.getRemainingTurns(toSync, spell3.GetComponent<SpellSlot>().connectedSpell);
                    if (remaining_turns > 0) {
                        // Active and set text
                        panel_turnsWait3.SetActive(true);
                        text_turnsWait3.text = "" + remaining_turns;
                    } else {
                        // Deactive
                        panel_turnsWait3.SetActive(false);
                    }
                } else {
                    spell3.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                    spell3.GetComponent<SpellSlot>().canUse = true;
                    panel_turnsWait3.SetActive(false);
                } 
            else
                panel_turnsWait3.SetActive(false);

            if (toSync.spells.Count == 4)
                if (!Spell.canUse(toSync, toSync.spells[3])) {
                    spell4.GetComponent<Image>().color = new Color(80f / 255f, 80f / 255f, 80f / 255f, 1);
                    spell4.GetComponent<SpellSlot>().canUse = false;
                    int remaining_turns = Spell.getRemainingTurns(toSync, spell4.GetComponent<SpellSlot>().connectedSpell);
                    if (remaining_turns > 0) {
                        // Active and set text
                        panel_turnsWait4.SetActive(true);
                        text_turnsWait4.text = "" + remaining_turns;
                    } else {
                        // Deactive
                        panel_turnsWait4.SetActive(false);
                    }
                } else {
                    spell4.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                    spell4.GetComponent<SpellSlot>().canUse = true;
                    panel_turnsWait4.SetActive(false);
                }
            else
                panel_turnsWait4.SetActive(false);
        }
    }

    public void OnClickSwitchAttResPanel() {
        if (stats_att_def_0_1 == 0) {
            panelAttDef.transform.GetChild(0).gameObject.SetActive(true);
            panelAttDef.transform.GetChild(1).gameObject.SetActive(false);
            btnPanelAttDef.transform.GetChild(0).gameObject.SetActive(true);
            btnPanelAttDef.transform.GetChild(1).gameObject.SetActive(false);
            stats_att_def_0_1 = 1;
        } else if (stats_att_def_0_1 == 1) {
            panelAttDef.transform.GetChild(0).gameObject.SetActive(false);
            panelAttDef.transform.GetChild(1).gameObject.SetActive(true);
            btnPanelAttDef.transform.GetChild(0).gameObject.SetActive(false);
            btnPanelAttDef.transform.GetChild(1).gameObject.SetActive(true);
            stats_att_def_0_1 = 0;
        }
    }

}
