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
        Debug.Log("Created spell panel");
    }

    private void Update() {
        if (toSync != null) {
            characterPreview.GetComponent<Image>().sprite = cl.getCharacterInfoByName(toSync.name).characterMidSprite;
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
            spell1.GetComponent<SpellSlot>().setSpell(toSync.spells[0]);
            spell2.GetComponent<SpellSlot>().setSpell(toSync.spells[1]);
            spell3.GetComponent<SpellSlot>().setSpell(toSync.spells[2]);
            spell4.GetComponent<SpellSlot>().setSpell(toSync.spells[3]);
            if (!Spell.canUse(toSync, toSync.spells[0])) {
                spell1.GetComponent<Image>().color = new Color(80f / 255f, 80f / 255f, 80f / 255f, 1);
                spell1.GetComponent<SpellSlot>().canUse = false;
            } else {
                spell1.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                spell1.GetComponent<SpellSlot>().canUse = true;
            }
            if (!Spell.canUse(toSync, toSync.spells[1])) {
                spell2.GetComponent<Image>().color = new Color(80f / 255f, 80f / 255f, 80f / 255f, 1);
                spell2.GetComponent<SpellSlot>().canUse = false;
            } else {
                spell2.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                spell2.GetComponent<SpellSlot>().canUse = true;
            }
            if (!Spell.canUse(toSync, toSync.spells[2])) {
                spell3.GetComponent<Image>().color = new Color(80f / 255f, 80f / 255f, 80f / 255f, 1);
                spell3.GetComponent<SpellSlot>().canUse = false;
            } else {
                spell3.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                spell3.GetComponent<SpellSlot>().canUse = true;
            }
            if (!Spell.canUse(toSync, toSync.spells[3])) {
                spell4.GetComponent<Image>().color = new Color(80f / 255f, 80f / 255f, 80f / 255f, 1);
                spell4.GetComponent<SpellSlot>().canUse = false;
            } else {
                spell4.GetComponent<Image>().color = new Color(1, 1, 1, 1);
                spell4.GetComponent<SpellSlot>().canUse = true;
            }
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
