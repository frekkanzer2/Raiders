using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConfirmButton : MonoBehaviour
{

    public Sprite deny;
    public Sprite validate;
    public Sprite confirm;
    public GameObject img;
    private Image cimg;

    public SelectionManagerGeneric sm;
    public int team;

    [HideInInspector]
    public int state = -1;

    // Start is called before the first frame update
    void Start() {
        cimg = img.GetComponent<Image>();
        setCanDeny();
        state = 0;
    }

    public void setCanValidate() {
        if (team == 1) {
            sm.isAlphaLocked = false;
        }
        if (team == 2) {
            sm.isBetaLocked = false;
        }
        cimg.sprite = validate;
        state = 1;
    }
    public void setCanConfirm() {
        cimg.sprite = confirm;
        state = 2;
    }
    public void setCanDeny() {
        cimg.sprite = deny;
        state = 0;
    }

    public void OnClick() {
        if (SceneManager.GetActiveScene().name == "DungeonChooseCharacters") {
            if (state == 0) {
                // Deny state, do nothing
                return;
            } else if (state == 1) {
                // Deny state, you are clicking to validate
                if (team == 1) {
                    SoundUi.Instance.playAudio(SoundUi.AudioType.Preview_ConfirmTeam);
                    SelectionContainer sc = sm.gameObject.GetComponent<SelectionContainer>();
                    sm.isAlphaLocked = true;
                    setCanConfirm();
                    sm.setDefinitiveLock();
                }
            }
        } else if (SceneManager.GetActiveScene().name == "ChooseCharacters") {
            if (state == 0) {
                // Deny state, do nothing
                return;
            } else if (state == 1) {
                // Deny state, you are clicking to validate
                if (team == 1) {
                    if (!sm.isBetaLocked) {
                        sm.isAlphaLocked = true;
                        setCanConfirm();
                        SoundUi.Instance.playAudio(SoundUi.AudioType.HeroChoise_Confirm1);
                    } else {
                        SoundUi.Instance.playAudio(SoundUi.AudioType.Preview_ConfirmTeam);
                        SelectionContainer sc = sm.gameObject.GetComponent<SelectionContainer>();
                        if (sc.areSameDimension()) {
                            sm.isAlphaLocked = true;
                            setCanConfirm();
                            sm.setDefinitiveLock();
                        }
                    }
                }
                if (team == 2) {
                    if (!sm.isAlphaLocked) {
                        sm.isBetaLocked = true;
                        setCanConfirm();
                        SoundUi.Instance.playAudio(SoundUi.AudioType.HeroChoise_Confirm1);
                    } else {
                        SoundUi.Instance.playAudio(SoundUi.AudioType.Preview_ConfirmTeam);
                        SelectionContainer sc = sm.gameObject.GetComponent<SelectionContainer>();
                        if (sc.areSameDimension()) {
                            sm.isBetaLocked = true;
                            setCanConfirm();
                            sm.setDefinitiveLock();
                        }
                    }
                }
            }
        }
    }

}
