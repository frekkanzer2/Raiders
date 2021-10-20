using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellSlot : MonoBehaviour
{

    public Spell connectedSpell;

    public void setSpell(Spell s) {
        this.connectedSpell = s;
        GetComponent<Image>().sprite = s.icon;
    }

    public void OnClickSpellPreview() {
        connectedSpell.OnPreviewPressed();
    }

}
