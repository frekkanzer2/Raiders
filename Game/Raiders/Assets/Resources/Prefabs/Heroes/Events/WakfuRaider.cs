using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WakfuRaider : ParentEvent
{

    int powerup = 0;
    Sprite backup = null;

    public WakfuRaider(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void both_firstExecute() {
        base.both_firstExecute();
        backup = connected.gameObject.GetComponent<SpriteRenderer>().sprite;
        connected.att_a += 20;
        connected.att_e += 20;
        connected.att_w += 20;
        connected.att_f += 20;
        connected.res_a += 20;
        connected.res_e += 20;
        connected.res_w += 20;
        connected.res_f += 20;
        connected.gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Prefabs/Heroes/Transformation/Yugo Wakfu Raider");
        List<Character> enemies = Spell.ut_getEnemies(connected);
        List<Block> pickedBlocks = new List<Block>();
        foreach(Character enemy in enemies) {
            List<Block> freeBlocks = enemy.connectedCell.GetComponent<Block>().getFreeAdjacentBlocks();
            foreach (Block toRem in pickedBlocks) {
                for (int i = 0; i < freeBlocks.Count; i++) {
                    if (toRem.equalsTo(freeBlocks[i])) {
                        freeBlocks.RemoveAt(i);
                        i--;
                    }
                }
            }
            if (freeBlocks.Count == 0) continue;
            UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
            int indexResult = UnityEngine.Random.Range(0, freeBlocks.Count);
            Evocation e = Spell.ut_execute_summon(connected, freeBlocks[indexResult], "Wakfu_Totem", -1);
            if (e != null) {
                pickedBlocks.Add(freeBlocks[indexResult]);
                e.isWakfuTotem = true;
                e.hp = connected.hp;
                e.actual_hp = e.hp;
            }
        }
    }
    
    override public void both_newTurnExecute() {
        base.both_newTurnExecute();
        connected.incrementPM(1);
    }

    public override void restoreCharacter() {
        base.restoreCharacter();
        connected.gameObject.GetComponent<SpriteRenderer>().sprite = backup;
        connected.att_a -= 20;
        connected.att_e -= 20;
        connected.att_w -= 20;
        connected.att_f -= 20;
        connected.res_a -= 20;
        connected.res_e -= 20;
        connected.res_w -= 20;
        connected.res_f -= 20;
    }

}
