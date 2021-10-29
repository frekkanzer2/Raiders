using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpellPopup : MonoBehaviour
{

    public GameObject spellType;
    public GameObject spellName;
    public GameObject spellIcon;
    public GameObject spellReqLife;
    public GameObject spellReqPA;
    public GameObject spellReqPM;
    public GameObject spellReqPO;
    public GameObject spellDMG;
    public GameObject spellCC;
    public GameObject spellUT;
    public GameObject spellOverObstacles;
    public GameObject spellEffect;

    public void OnSpellPressed(Spell s) {
        spellName.GetComponent<TextMeshProUGUI>().text = s.name;
        spellIcon.GetComponent<Image>().sprite = s.icon;
        spellReqLife.GetComponent<TextMeshProUGUI>().text = ""+s.hpCost;
        spellReqPA.GetComponent<TextMeshProUGUI>().text = ""+s.paCost;
        spellReqPM.GetComponent<TextMeshProUGUI>().text = ""+s.pmCost;
        spellReqPO.GetComponent<TextMeshProUGUI>().text = s.minRange+"-"+s.maxRange;
        spellDMG.GetComponent<TextMeshProUGUI>().text = "" + s.damage;
        spellCC.GetComponent<TextMeshProUGUI>().text = "" + s.criticalProbability+"%";
        spellUT.GetComponent<TextMeshProUGUI>().text = "" + s.maxTimesInTurn;
        spellEffect.GetComponent<TextMeshProUGUI>().text = s.description;
        if (s.element == Spell.Element.Other) {
            spellType.GetComponent<TextMeshProUGUI>().text = "SKILL SPELL";
            spellType.GetComponent<TextMeshProUGUI>().color = new Color(1, 158f/255f, 0, 1);
        } else if (s.element == Spell.Element.Heal) {
            spellType.GetComponent<TextMeshProUGUI>().text = "HEAL SPELL";
            spellType.GetComponent<TextMeshProUGUI>().color = new Color(1, 160f / 255f, 215f/255f, 1);
        } else if (s.element == Spell.Element.Earth) {
            spellType.GetComponent<TextMeshProUGUI>().text = "EARTH SPELL";
            spellType.GetComponent<TextMeshProUGUI>().color = new Color(132f/255f, 110f/255f, 82f/255f, 1);
        } else if (s.element == Spell.Element.Fire) {
            spellType.GetComponent<TextMeshProUGUI>().text = "FIRE SPELL";
            spellType.GetComponent<TextMeshProUGUI>().color = new Color(224f / 255f, 69f / 255f, 56f / 255f, 1);
        } else if (s.element == Spell.Element.Air) {
            spellType.GetComponent<TextMeshProUGUI>().text = "AIR SPELL";
            spellType.GetComponent<TextMeshProUGUI>().color = new Color(138f / 255f, 185f / 255f, 113f / 255f, 1);
        } else if (s.element == Spell.Element.Water) {
            spellType.GetComponent<TextMeshProUGUI>().text = "WATER SPELL";
            spellType.GetComponent<TextMeshProUGUI>().color = new Color(130f / 255f, 177f / 255f, 226f / 255f, 1);
        }
        if (s.overObstacles) spellOverObstacles.GetComponent<TextMeshProUGUI>().text = "YES";
        else spellOverObstacles.GetComponent<TextMeshProUGUI>().text = "NO";
        this.gameObject.SetActive(true);
    }

    public void OnExit() {
        this.gameObject.SetActive(false);
    }

}
