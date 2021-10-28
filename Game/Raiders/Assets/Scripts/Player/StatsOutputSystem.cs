using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsOutputSystem : MonoBehaviour {

    public static float WAITING_TIME_BETWEEN_EFFECTS = 0.5f; // in seconds
    
    private GameObject numberPrefab;

    private Color HP_color = new Color(1, 0, 0, 1);
    private Color PA_color = new Color(1, 130f / 255f, 0, 1);
    private Color PM_color = new Color(0, 176f / 255f, 16f / 255f, 1);
    private Color Heal_color = new Color(255f / 255f, 150f / 255f, 196f / 255f, 1);
    private Color Shield_color = new Color(86f / 255f, 64f / 255f, 128f / 255f, 1);

    private List<EffectToExecute> toDisplay = new List<EffectToExecute>();

    private class EffectToExecute {

        private GameObject pref;
        private Color c;
        private string output = null;
        private int value = -1;
        private Vector2 whereToSpawn;
        private bool hasExecuted = false;
        private Sprite icon = null;

        public EffectToExecute(GameObject prefabToSpawn, Color color, string text, Vector2 pos) {
            pref = prefabToSpawn;
            c = color;
            output = text;
            whereToSpawn = pos;
        }

        public EffectToExecute(GameObject prefabToSpawn, Color color, int value, Vector2 pos) {
            pref = prefabToSpawn;
            c = color;
            this.value = value;
            whereToSpawn = pos;
        }

        public EffectToExecute(GameObject prefabToSpawn, Color color, Sprite i, Vector2 pos) {
            pref = prefabToSpawn;
            c = color;
            this.icon = i;
            whereToSpawn = pos;
        }

        public void execute() {
            if (hasExecuted) return;
            else hasExecuted = true;
            GameObject np = Instantiate(pref);
            if (output != null) // display string
                np.GetComponent<NumbersDisplayer>().init(c, output, whereToSpawn);
            // else display by numeric value
            else if (value > 0) np.GetComponent<NumbersDisplayer>().init(c, value, whereToSpawn);
            else np.GetComponent<NumbersDisplayer>().init(c, icon, whereToSpawn);
        }

    }

    public enum Effect {
        HP,
        PA,
        PM,
        Heal,
        Shield,
        Icon
    }
    
    private Color getColorByEffect(Effect e) {
        if (e == Effect.Heal) return Heal_color;
        if (e == Effect.HP) return HP_color;
        if (e == Effect.PA) return PA_color;
        if (e == Effect.PM) return PM_color;
        if (e == Effect.Shield) return Shield_color;
        return new Color(0, 0, 0, 1);
    }

    private Vector2 getSpawnPosition() {
        return new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.y + 3.8f);
    }

    public void addEffect_Icon(Effect type, Sprite icon) {
        EffectToExecute ete = new EffectToExecute(this.numberPrefab, getColorByEffect(type), icon, getSpawnPosition());
        toDisplay.Add(ete);
        execute();
    }

    public void addEffect_PA_PM(Effect type, string text) {
        EffectToExecute ete = new EffectToExecute(this.numberPrefab, getColorByEffect(type), text, getSpawnPosition());
        toDisplay.Add(ete);
        execute();
    }

    public void addEffect_DMG_Heal(Effect type, int value) {
        EffectToExecute ete = new EffectToExecute(this.numberPrefab, getColorByEffect(type), value, getSpawnPosition());
        toDisplay.Add(ete);
        execute();
    }

    public void addEffect_Shield(Effect type, string text) {
        EffectToExecute ete = new EffectToExecute(this.numberPrefab, getColorByEffect(type), text, getSpawnPosition());
        toDisplay.Add(ete);
        execute();
    }

    public void setup(GameObject pref) {
        numberPrefab = pref;
    }

    private bool isExecuting = false;

    private void execute() {
        if (isExecuting) return;
        if (toDisplay.Count > 0) isExecuting = true;
        else {
            isExecuting = false;
            return;
        }
        StartCoroutine(display(WAITING_TIME_BETWEEN_EFFECTS));
    }

    IEnumerator display(float time) {
        yield return new WaitForSeconds(WAITING_TIME_BETWEEN_EFFECTS/2f);
        EffectToExecute ete = toDisplay[0];
        toDisplay.RemoveAt(0);
        ete.execute();
        if (toDisplay.Count == 0) isExecuting = false;
        else {
            yield return new WaitForSeconds(WAITING_TIME_BETWEEN_EFFECTS);
            StartCoroutine(display(0));
        }
    }

}
