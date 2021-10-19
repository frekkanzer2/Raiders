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

    private int stats_att_def_0_1 = 0;

    public void set(Character c, CharactersLibrary cl) {
        this.toSync = c;
        this.cl = cl;
    }

    private void Update() {
        if (toSync != null) {
            characterPreview.GetComponent<Image>().sprite = cl.getCharacterInfoByName(toSync.name).characterMidSprite;
            hpGui.GetComponent<TextMeshProUGUI>().text = "" + toSync.actual_hp;
            paGui.GetComponent<TextMeshProUGUI>().text = "" + toSync.actual_pa;
            pmGui.GetComponent<TextMeshProUGUI>().text = "" + toSync.actual_pm;
            res_ear.GetComponent<TextMeshProUGUI>().text = "" + toSync.res_e + "%";
            res_fir.GetComponent<TextMeshProUGUI>().text = "" + toSync.res_f + "%";
            res_air.GetComponent<TextMeshProUGUI>().text = "" + toSync.res_a + "%";
            res_wat.GetComponent<TextMeshProUGUI>().text = "" + toSync.res_w + "%";
            att_ear.GetComponent<TextMeshProUGUI>().text = "" + toSync.att_e + "%";
            att_fir.GetComponent<TextMeshProUGUI>().text = "" + toSync.att_f + "%";
            att_air.GetComponent<TextMeshProUGUI>().text = "" + toSync.att_a + "%";
            att_wat.GetComponent<TextMeshProUGUI>().text = "" + toSync.att_w + "%";
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
