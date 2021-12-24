using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManagerGeneric : MonoBehaviour
{

    public GameObject teamAreferenceToCharSlider;
    public GameObject teamAreferenceToPreviewBtnSlider;
    public GameObject teamBreferenceToCharSlider;
    public GameObject teamBreferenceToPreviewBtnSlider;

    public GameObject prefabCharacter;
    public GameObject prefabButtonAlpha;
    public GameObject prefabButtonBeta;
    public GameObject prefabConfirmButtonAlpha;
    public GameObject prefabConfirmButtonBeta;

    [HideInInspector]
    public ConfirmButton specialButtonA;
    [HideInInspector]
    public ConfirmButton specialButtonB;

    public GameObject blackScreen;

    [HideInInspector]
    public bool canAlphaChoose = true;
    [HideInInspector]
    public bool isAlphaLocked = false;
    [HideInInspector]
    public bool canBetaChoose = true;
    [HideInInspector]
    public bool isBetaLocked = false;

    [HideInInspector]
    public static bool definitiveLock = false;

    public virtual void setDefinitiveLock() {
    }

    public virtual void registerCharacterChosen(CharacterInfo ci, ChButtonData ch, int team) {
    }

    public virtual void unregisterCharacterChosen(CharacterInfo ci, int team) {
    }

}
