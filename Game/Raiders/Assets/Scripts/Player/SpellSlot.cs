using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellSlot : MonoBehaviour
{

    public Spell connectedSpell;
    public bool canUse = true;

    public void setSpell(Spell s) {
        this.connectedSpell = s;
        GetComponent<Image>().sprite = s.icon;
    }

    public void OnClickSpellPreview() {
        if (canUse)
            connectedSpell.OnPreviewPressed();
        SoundUi.Instance.playAudio(SoundUi.AudioType.ButtonPressed_Spell);
        TurnsManager.Instance.popupSpell.GetComponent<SpellPopup>().OnSpellPressed(connectedSpell);
    }

}
