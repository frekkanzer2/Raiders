using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : Character {

    public enum MovementType {
        Random,
        GoAhead,
        GoAway,
        Rest
    }
    
    [System.Serializable]
    public class MonsterBehavior {
        public int[] spellsOrderToCheckId;
        public MovementType movementAfterFight;
        public MovementType movementWhenCannotAttack;
    }

    public MonsterBehavior behaviour;

    public int id;

    [HideInInspector]
    public bool isCommunionActive = false;

    public override void setDead() {
        base.setDead();
    }

    public string getCompleteName() {
        return this.name + id;
    }

    public override void inflictDamage(int damage, bool mustSkip = false) {
        base.inflictDamage(damage);
    }

    public override void newTurn() {
        base.newTurn();
        StartCoroutine(executeMinMax(1f, true));
    }

    public override bool Equals(object obj) {
        if (obj == null) return false;
        Monster monster = obj as Monster;
        if (monster == null) return false;
        return monster.getCompleteName().Equals(this.getCompleteName());
    }

    #region BOT

    private List<Spell> getCanExecuteSpells() {
        List<Spell> toReturn = new List<Spell>();
        foreach (int s_id in this.behaviour.spellsOrderToCheckId)
            if (Spell.canUse(this, this.spells[s_id])) {
                toReturn.Add(this.spells[s_id]);
            }
        return toReturn;
    }

    // CAN RETURN NULL
    private Tuple<Spell, Character> chooseSpellToExecute(List<Spell> executables) {
        Tuple<Spell, Character> toReturn = null;
        foreach (Spell executable in executables) {
            Character target = getTargetableEnemy(executable);
            if (target != null) {
                toReturn = new Tuple<Spell, Character>(executable, target);
                Debug.Log("FOUND SPELL " + toReturn.Item1.name + " TO CAST ON " + toReturn.Item2.name);
                break;
            }
        }
        return toReturn;
    }

    private int getDistanceFromTarget(Character target) {
        Coordinate selfCoord, targetCoord;
        selfCoord = this.connectedCell.GetComponent<Block>().coordinate;
        targetCoord = target.connectedCell.GetComponent<Block>().coordinate;
        int dist_row = Mathf.Abs(selfCoord.row - targetCoord.row);
        int dist_col = Mathf.Abs(selfCoord.column - targetCoord.column);
        return dist_row + dist_col;
    }

    private int getDistance(Coordinate start, Coordinate end) {
        int dist_row = Mathf.Abs(start.row - end.row);
        int dist_col = Mathf.Abs(start.column - end.column);
        return dist_row + dist_col;
    }

    // CAN RETURN NULL
    private Character getClosestEnemy() {
        Tuple<Character, int> tupleClosest = new Tuple<Character, int>(null, -1);
        foreach(Character c in TurnsManager.Instance.turns) {
            if (!c.isDead && c.isEnemyOf(this)) {
                if (tupleClosest.Item1 == null) tupleClosest = new Tuple<Character, int>(c, getDistanceFromTarget(c));
                else {
                    int distance = getDistanceFromTarget(c);
                    if (distance < tupleClosest.Item2) tupleClosest = new Tuple<Character, int>(c, getDistanceFromTarget(c));
                    else if (distance == tupleClosest.Item2 && c.getActualHP() < tupleClosest.Item1.getActualHP()) tupleClosest = new Tuple<Character, int>(c, getDistanceFromTarget(c));
                }
            }
        }
        return tupleClosest.Item1;
    }

    // CAN RETURN NULL
    private Character getTargetableEnemy(Spell s) {
        // init
        int minRange = s.minRange, maxRange = s.maxRange;
        Character origin = this;
        Tuple<Character, int> tupleTargetable = new Tuple<Character, int>(null, -1);
        // checking can be target on each enemy
        foreach (Character c in TurnsManager.Instance.turns) {
            if (!c.isDead && c.isEnemyOf(this) && getDistanceFromTarget(c) >= minRange && getDistanceFromTarget(c) <= maxRange) {
                Debug.LogError("Found " + c.name);
                if (c.name == "Katina") { // DEBUG
                    Debug.LogWarning("DISTANCE FROM " + c.name + ": " + getDistanceFromTarget(c));
                    Debug.Log("Katina coords: " + c.connectedCell.GetComponent<Block>().coordinate.display());
                    Debug.Log(this.getCompleteName() + " coords: " + this.connectedCell.GetComponent<Block>().coordinate.display());
                }
                // Checking if there's an obstacle between caster and target
                bool canHit = true;
                if (!s.overObstacles) {
                    Debug.DrawLine(origin.connectedCell.GetComponent<Block>().transform.position, c.gameObject.transform.position);
                    RaycastHit2D[] hits = Physics2D.LinecastAll(origin.connectedCell.GetComponent<Block>().transform.position, c.gameObject.transform.position);
                    foreach (RaycastHit2D hit in hits) {
                        GameObject collided = hit.collider.gameObject;
                        Block retrieved = collided.GetComponent<Block>();
                        if (retrieved != null) {
                           if (retrieved.linkedObject != null) {
                                canHit = false;
                                break;
                           }
                        }
                    }
                }
                if (!canHit) continue; // next enemy
                Debug.Log("Can hit enemy " + c.name + " because there are no obstacles");
                // Assignment
                if (tupleTargetable.Item1 == null) {
                    Debug.Log("NULL CASE - ASSIGNING FOR FIRST TIME " + c.name);
                    tupleTargetable = new Tuple<Character, int>(c, c.getActualHP());
                } else {
                    if (c.getActualHP() < tupleTargetable.Item2) tupleTargetable = new Tuple<Character, int>(c, c.getActualHP());
                    else if (c.getActualHP() == tupleTargetable.Item2 && getDistanceFromTarget(c) < getDistanceFromTarget(tupleTargetable.Item1)) tupleTargetable = new Tuple<Character, int>(c, c.getActualHP());
                    Debug.Log("MINOR CASE - ASSIGNING " + c.name);
                }
            } else {
                if (c.isEnemyOf(this))
                    if (c.name == "Katina") { // DEBUG
                        Debug.LogWarning("DISTANCE FROM " + c.name + ": " + getDistanceFromTarget(c));
                        Debug.Log("Katina coords: " + c.connectedCell.GetComponent<Block>().coordinate.display());
                        Debug.Log(this.getCompleteName() + " coords: " + this.connectedCell.GetComponent<Block>().coordinate.display());
                    }
            }
        }
        return tupleTargetable.Item1;
    }

    IEnumerator executeMinMax(float timing, bool debugEnabled) {
        bool canExecute = true;
        bool hasAttacked = false;
        List<Block> whereToMove = null; // set to null after movement
        while (canExecute) {
            // Wait when a decision is made
            yield return new WaitForSeconds(timing);
            whereToMove = null;
            List<Spell> validSpells = getCanExecuteSpells();
            Tuple<Spell, Character> toExecute = null;
            Block toMove = null;
            // Choosing a spell to execute
            if (validSpells.Count > 0) {
                toExecute = chooseSpellToExecute(validSpells);
            }
            // If cannot execute spell, go ahead the character
            if (toExecute == null && this.getActualPM() > 0) {
                Character close = getClosestEnemy();
                if (getDistanceFromTarget(close) == 1) close = null;
                if (close != null) {
                    toMove = close.connectedCell.GetComponent<Block>();
                    if (toMove.getFreeAdjacentBlocks().Count == 0) toMove = null;
                }
            }
            // Execute the action
            if (toExecute == null && toMove == null) {
                // DON'T EXECUTE NOTHING
                canExecute = false;
                if (debugEnabled) Debug.LogWarning("Enemy " + this.getCompleteName() + " cannot do any action!");
            } else if (toExecute != null) {
                Spell.executeSpell(TurnsManager.active, toExecute.Item2.connectedCell.GetComponent<Block>(), toExecute.Item1);
                hasAttacked = true;
                if (debugEnabled) Debug.LogWarning("Enemy " + this.getCompleteName() + " will attack with spell " + toExecute.Item1.name);
            } else if (toMove != null) {
                MovementType mt;
                if (hasAttacked) {
                    // move after attack
                    mt = behaviour.movementAfterFight;
                } else {
                    // before attack
                    mt = behaviour.movementWhenCannotAttack;
                }
                List<Block> path = null;
                if (mt == MovementType.GoAhead) {
                    path = ai_astarpath(this.connectedCell.GetComponent<Block>(), toMove, AI_SEARCHPATH_STEPS);
                    if (path.Count >= 2) {
                        path.RemoveRange(2, path.Count - 2);
                        whereToMove = path;
                    }
                } else if (mt == MovementType.GoAway) {
                    List<Block> allowedDestinations = new List<Block>();
                    Character enemy = getClosestEnemy();
                    int actualDistance = getDistanceFromTarget(enemy);
                    foreach (Block b in this.connectedCell.GetComponent<Block>().getFreeAdjacentBlocks()) {
                        if (getDistance(b.coordinate, enemy.connectedCell.GetComponent<Block>().coordinate) > actualDistance) {
                            allowedDestinations.Add(b);
                        }
                    }
                    if (allowedDestinations.Count > 0) {
                        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
                        int chosenIndex = UnityEngine.Random.Range(0, allowedDestinations.Count);
                        whereToMove = new List<Block>();
                        whereToMove.Add(this.connectedCell.GetComponent<Block>());
                        whereToMove.Add(allowedDestinations[chosenIndex]);
                    }
                } else if (mt == MovementType.Random) {
                    List<Block> availables = new List<Block>();
                    foreach (Block b in this.connectedCell.GetComponent<Block>().getFreeAdjacentBlocks()) {
                        availables.Add(b);
                    }
                    if (availables.Count > 0) {
                        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
                        int chosenIndex = UnityEngine.Random.Range(0, availables.Count);
                        whereToMove = new List<Block>();
                        whereToMove.Add(this.connectedCell.GetComponent<Block>());
                        whereToMove.Add(availables[chosenIndex]);
                    }
                } else if (mt == MovementType.Rest) {
                    whereToMove = null;
                }
                if (whereToMove != null) {
                    this.setPath(whereToMove); // walk
                    this.decrementPM_withoutEffect(1);
                    if (debugEnabled) Debug.LogWarning("Enemy " + this.getCompleteName() + " is moving of 1 cell");
                } else {
                    this.decrementPM_withoutEffect(this.getActualPM());
                    if (debugEnabled) Debug.LogWarning("Enemy " + this.getCompleteName() + " cannot move!");
                }
            }
        }
        if (debugEnabled) Debug.LogWarning("Enemy " + this.getCompleteName() + " will pass the turn now");
        TurnsManager.Instance.OnNextTurnPressed();
    }

    #endregion

    #region IA-ALGORITHMS

    private int AI_SEARCHPATH_STEPS = 30;

    private List<Block> ai_astarpath(Block source, Block destination, int iterationLimit) {
        List<Block> BACKUP_PATH = null;
        List<Block> resultPath = null;
        List<Tuple<List<Block>, float>> queue = new List<Tuple<List<Block>, float>>();
        List<Block> inject = new List<Block>();
        inject.Add(source.GetComponent<Block>());
        Tuple<List<Block>, float> start = new Tuple<List<Block>, float>(inject, h_euclidian(source.GetComponent<Block>().coordinate, destination.coordinate, 2));
        queue.Add(start);
        int counter = 0;
        while (counter < iterationLimit && queue.Count > 0) {
            counter++;
            List<Tuple<List<Block>, float>> toDelete = new List<Tuple<List<Block>, float>>();
            List<Tuple<List<Block>, float>> toAdd = new List<Tuple<List<Block>, float>>();
            Tuple<List<Block>, float> record = queue[0];
            bool addedToDelete = false;
            List<Block> tracking = record.Item1; // getting path
            BACKUP_PATH = new List<Block>();
            BACKUP_PATH.AddRange(tracking); // Saving actual tracking path
            Block lastNode = tracking[tracking.Count - 1]; // actual node where we're working on
            // GETTING ADIACENT BLOCKS
            Block _tmp = Map.Instance.getBlock(new Coordinate(lastNode.coordinate.row, lastNode.coordinate.column + 1));
            if (_tmp != null)
                if (_tmp.linkedObject)
                    _tmp = null;
            float _tmp_h = -1;
            bool hasFound = false;
            if (_tmp != null) {
                foreach (Block b in tracking) { // checking if block was already in the path
                    if (b.equalsTo(_tmp)) {
                        hasFound = true;
                        break;
                    }
                }
                if (!hasFound) {
                    // replace old path with new path
                    if (!addedToDelete) toDelete.Add(record);
                    addedToDelete = true;
                    _tmp_h = h_euclidian(_tmp.coordinate, destination.coordinate, 2);
                    List<Block> newTracking = new List<Block>(tracking);
                    newTracking.Add(_tmp);
                    if (_tmp.equalsTo(destination)) resultPath = newTracking;
                    toAdd.Add(new Tuple<List<Block>, float>(newTracking, _tmp_h));
                } else {
                    // if the new node is already in the path, it's a cycle!
                    // so the path is not good, we'll delete it
                    if (!addedToDelete) toDelete.Add(record);
                    addedToDelete = true;
                }
            }
            _tmp = Map.Instance.getBlock(new Coordinate(lastNode.coordinate.row, lastNode.coordinate.column - 1));
            if (_tmp != null)
                if (_tmp.linkedObject)
                    _tmp = null;
            _tmp_h = -1;
            hasFound = false;
            if (_tmp != null) {
                foreach (Block b in tracking) { // checking if block was already in the path
                    if (b.equalsTo(_tmp)) {
                        hasFound = true;
                        break;
                    }
                }
                if (!hasFound) {
                    // replace old path with new path
                    if (!addedToDelete) toDelete.Add(record);
                    addedToDelete = true;
                    _tmp_h = h_euclidian(_tmp.coordinate, destination.coordinate, 2);
                    List<Block> newTracking = new List<Block>(tracking);
                    newTracking.Add(_tmp);
                    if (_tmp.equalsTo(destination)) resultPath = newTracking;
                    toAdd.Add(new Tuple<List<Block>, float>(newTracking, _tmp_h));
                } else {
                    // if the new node is already in the path, it's a cycle!
                    // so the path is not good, we'll delete it
                    if (!addedToDelete) toDelete.Add(record);
                    addedToDelete = true;
                }
            }
            _tmp = Map.Instance.getBlock(new Coordinate(lastNode.coordinate.row + 1, lastNode.coordinate.column));
            if (_tmp != null)
                if (_tmp.linkedObject)
                    _tmp = null;
            _tmp_h = -1;
            hasFound = false;
            if (_tmp != null) {
                foreach (Block b in tracking) { // checking if block was already in the path
                    if (b.equalsTo(_tmp)) {
                        hasFound = true;
                        break;
                    }
                }
                if (!hasFound) {
                    // replace old path with new path
                    if (!addedToDelete) toDelete.Add(record);
                    addedToDelete = true;
                    _tmp_h = h_euclidian(_tmp.coordinate, destination.coordinate, 2);
                    List<Block> newTracking = new List<Block>(tracking);
                    newTracking.Add(_tmp);
                    if (_tmp.equalsTo(destination)) resultPath = newTracking;
                    toAdd.Add(new Tuple<List<Block>, float>(newTracking, _tmp_h));
                } else {
                    // if the new node is already in the path, it's a cycle!
                    // so the path is not good, we'll delete it
                    if (!addedToDelete) toDelete.Add(record);
                    addedToDelete = true;
                }
            }
            _tmp = Map.Instance.getBlock(new Coordinate(lastNode.coordinate.row - 1, lastNode.coordinate.column));
            if (_tmp != null)
                if (_tmp.linkedObject) {
                    _tmp = null;
                }
            _tmp_h = -1;
            hasFound = false;
            if (_tmp != null) {
                foreach (Block b in tracking) { // checking if block was already in the path
                    if (b.equalsTo(_tmp)) {
                        hasFound = true;
                        break;
                    }
                }
                if (!hasFound) {
                    // replace old path with new path
                    if (!addedToDelete) toDelete.Add(record);
                    addedToDelete = true;
                    _tmp_h = h_euclidian(_tmp.coordinate, destination.coordinate, 2);
                    List<Block> newTracking = new List<Block>(tracking);
                    newTracking.Add(_tmp);
                    if (_tmp.equalsTo(destination)) resultPath = newTracking;
                    toAdd.Add(new Tuple<List<Block>, float>(newTracking, _tmp_h));
                } else {
                    // if the new node is already in the path, it's a cycle!
                    // so the path is not good, we'll delete it
                    if (!addedToDelete) toDelete.Add(record);
                    addedToDelete = true;
                }
            }
            if (resultPath == null) {
                // Apply recorded edits
                foreach (Tuple<List<Block>, float> r in toDelete) {
                    queue.Remove(r);
                }
                foreach (Tuple<List<Block>, float> r in toAdd) {
                    queue.Add(r);
                }
                // Sorting list
                AStarTupleComparer astc = new AStarTupleComparer();
                queue.Sort(astc);
            } else break;
        }
        if (resultPath == null)
            resultPath = BACKUP_PATH;
        return resultPath;
    }

    #endregion

    #region IA-HEURISTICS

    private int h_euclidian(Coordinate start, Coordinate destination, int weight = 1) {
        float dx = Mathf.Abs(start.column - destination.column);
        float dy = Mathf.Abs(start.row - destination.row);
        return (int)(weight * Mathf.Sqrt(dx * dx + dy * dy));
    }

    #endregion

}
