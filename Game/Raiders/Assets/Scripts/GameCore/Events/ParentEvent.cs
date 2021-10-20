using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentEvent {

    public string name;
    public int remainingTurns = 0;
    public Character connected;
    private Mode m;
    private bool used = false;

    public enum Mode {
        Permanent,
        ActivationEachTurn,
        ActivationEachEndTurn
    }

    public ParentEvent(string name, Character c, int duration, Mode mode) {
        this.name = name;
        connected = c;
        remainingTurns = duration;
        m = mode;
    }

    public void OnStartTurn() {
        if (m == Mode.ActivationEachTurn) {
            execute();
        } else if (m == Mode.Permanent) {
            if (!used) {
                used = true;
                execute();
            }
        }
    }

    public void OnTurnEnds() {
        if (m == Mode.ActivationEachEndTurn) {
            execute();
        }
        remainingTurns--;
        if (remainingTurns == 0) {
            connected.GetComponent<EventSystem>().removeZeroEvents();
        }
    }

    // TO IMPLEMENT
    virtual public void execute() {
        if (remainingTurns == 0) return;
    }

    // TO IMPLEMENT - Called when the event is removed from the list
    virtual public void restoreCharacter() {
        
    }

    public bool isName(string n) {
        return this.name == n;
    }

}
