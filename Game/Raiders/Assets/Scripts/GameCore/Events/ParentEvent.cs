using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentEvent {

    public string name;
    public int remainingTurns = 0;
    public Character connected;
    private Mode m;
    private bool used = false;
    public Sprite sprite;

    public enum Mode {
        Permanent,
        ActivationEachTurn,
        ActivationEachEndTurn,
        PermanentAndEachTurn
    }

    public ParentEvent(string name, Character c, int duration, Mode mode, Sprite s) {
        this.name = name;
        connected = c;
        remainingTurns = duration;
        m = mode;
        sprite = s;
    }

    public void OnStartTurn() {
        if (this.connected.isDead) return;
        if (m == Mode.ActivationEachTurn) {
            execute();
        } else if (m == Mode.Permanent) {
            if (!used) {
                used = true;
                execute();
            }
        } else if (m == Mode.PermanentAndEachTurn) {
            if (!used) {
                used = true;
                both_firstExecute();
            }
            both_newTurnExecute();
        }
    }

    public void OnTurnEnds() {
        if (this.connected.isDead) return;
        if (m == Mode.ActivationEachEndTurn) {
            execute();
        }
        remainingTurns--;
    }

    // TO IMPLEMENT
    virtual public void execute() {
        if (this.connected.isDead) return;
        if (remainingTurns == 0) return;
    }

    // TO IMPLEMENT EVER AND EVER!
    virtual public void restoreCharacter() {
        if (this.connected.isDead) return;

    }

    // TO IMPLEMENT - if you are using PermanentAndEachTurn
    virtual public void both_firstExecute() {
        if (this.connected.isDead) return;

    }

    // TO IMPLEMENT - if you are using PermanentAndEachTurn
    virtual public void both_newTurnExecute() {
        if (this.connected.isDead) return;
        if (remainingTurns == 0) return;

    }

    public bool isName(string n) {
        return this.name == n;
    }

    public void useIstantanely() {
        if (this.connected.isDead) return;
        if (m == Mode.ActivationEachTurn) {
            execute();
        } else if (m == Mode.Permanent) {
            if (!used) {
                used = true;
                execute();
            }
        } else if (m == Mode.PermanentAndEachTurn) {
            if (!used) {
                used = true;
                both_firstExecute();
                both_newTurnExecute();
            }
        }
    }

}
