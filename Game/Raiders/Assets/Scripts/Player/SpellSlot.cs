using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellSlot : MonoBehaviour
{

    public Spell connectedSpell;
    public bool canUse = true;
    public Sprite noSpell;

    public void setSpell(Spell s) {
        this.connectedSpell = s;
        GetComponent<Image>().sprite = s.icon;
    }

    public void removeSpell() {
        this.connectedSpell = null;
        GetComponent<Image>().sprite = noSpell;
        GetComponent<Image>().color = new Color(1, 1, 1);
    }

    public void OnClickSpellPreview() {
        if (connectedSpell == null) return;
        if (canUse)
            connectedSpell.OnPreviewPressed();
        SoundUi.Instance.playAudio(SoundUi.AudioType.ButtonPressed_Spell);
        TurnsManager.Instance.popupSpell.GetComponent<SpellPopup>().OnSpellPressed(connectedSpell);
    }

}
