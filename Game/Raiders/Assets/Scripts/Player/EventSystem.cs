using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSystem : MonoBehaviour
{
    
    public Character connected;
    public List<ParentEvent> activeEvents = new List<ParentEvent>();

    // Start is called before the first frame update
    void Start()
    {
        connected = GetComponent<Character>();
    }

    public void addEvent(ParentEvent pe) {
        activeEvents.Add(pe);
    }

    public void removeZeroEvents() {
        List<ParentEvent> pevs = new List<ParentEvent>();
        foreach (ParentEvent pe in activeEvents) {
            if (pe.remainingTurns == 0)
                pevs.Add(pe);
        }
        foreach (ParentEvent pe in pevs) {
            pe.restoreCharacter();
            activeEvents.Remove(pe);
        }
    }

    public void OnStartTurn() {
        foreach(ParentEvent pe in activeEvents) {
            pe.OnStartTurn();
        }
    }

    public void OnEndTurn() {
        foreach (ParentEvent pe in activeEvents) {
            pe.OnTurnEnds();
        }
        removeZeroEvents();
    }

    public List<ParentEvent> getEvents(string name) {
        List<ParentEvent> pel = new List<ParentEvent>();
        foreach (ParentEvent pe in activeEvents) {
            if (pe.isName(name)) pel.Add(pe);
        }
        return pel;
    }

}