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
        base.inflictDamage(damage, mustSkip);
    }

    public override void newTurn() {
        base.newTurn();
        StartCoroutine(executeMinMax(0.5f, true));
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
            Character target = null;
            if (executable.isOffensiveSpell())
                target = getTargetableEnemy(executable);
            else target = getTargetableAlly(executable);
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
    private Character getClosestEnemy(List<Character> ignore) {
        Tuple<Character, int> tupleClosest = new Tuple<Character, int>(null, -1);
        foreach (Character c in TurnsManager.Instance.turns) {
            if (!c.isDead && c.isEnemyOf(this)) {
                bool canConsider = true;
                foreach(Character i in ignore) {
                    if (c.Equals(i)) {
                        canConsider = false;
                        break;
                    }
                }
                if (!canConsider) continue;
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

    private bool isLineRespected(Spell s, Coordinate start, Coordinate end) {
        if ((start.column == end.column || start.row == end.row) && getDistance(start, end) >= s.minRange && getDistance(start, end) <= s.maxRange)
            return true;
        return false;
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
                bool canHit = true;
                // Checking if range type is in line
                if (s.distanceType == Spell.DistanceType.Line)
                    canHit = isLineRespected(s, origin.connectedCell.GetComponent<Block>().coordinate, c.connectedCell.GetComponent<Block>().coordinate);
                // Checking if there's an obstacle between caster and target
                if (canHit && !s.overObstacles) {
                    Debug.DrawLine(origin.connectedCell.GetComponent<Block>().transform.position, c.gameObject.transform.position);
                    RaycastHit2D[] hits = Physics2D.LinecastAll(origin.connectedCell.GetComponent<Block>().transform.position, c.gameObject.transform.position);
                    foreach (RaycastHit2D hit in hits) {
                        GameObject collided = hit.collider.gameObject;
                        Block retrieved = collided.GetComponent<Block>();
                        if (retrieved != null) {
                           if (retrieved.linkedObject != null) {
                                if (retrieved.linkedObject.GetComponent<Character>() != null) {
                                    if (!retrieved.linkedObject.GetComponent<Character>().Equals(this) && !retrieved.linkedObject.GetComponent<Character>().Equals(c)) {
                                        canHit = false;
                                        break;
                                    }
                                }
                           }
                        }
                    }
                }
                if (!canHit) continue; // next enemy
                // Assignment
                if (tupleTargetable.Item1 == null) {
                    tupleTargetable = new Tuple<Character, int>(c, c.getActualHP());
                } else {
                    if (c.getActualHP() < tupleTargetable.Item2) tupleTargetable = new Tuple<Character, int>(c, c.getActualHP());
                    else if (c.getActualHP() == tupleTargetable.Item2 && getDistanceFromTarget(c) < getDistanceFromTarget(tupleTargetable.Item1)) tupleTargetable = new Tuple<Character, int>(c, c.getActualHP());
                }
            }
        }
        return tupleTargetable.Item1;
    }

    // CAN RETURN NULL
    private Character getTargetableAlly(Spell s) {
        // init
        int minRange = s.minRange, maxRange = s.maxRange;
        Character origin = this;
        Tuple<Character, int> tupleTargetable = new Tuple<Character, int>(null, -1);
        // checking can be target on each enemy
        foreach (Character c in TurnsManager.Instance.turns) {
            if (!c.isDead && !c.isEnemyOf(this) && getDistanceFromTarget(c) >= minRange && getDistanceFromTarget(c) <= maxRange) {
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
                                if (retrieved.linkedObject.GetComponent<Monster>() != null) {
                                    if (retrieved.linkedObject.GetComponent<Monster>().getCompleteName() != this.getCompleteName()) {
                                        canHit = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                if (!canHit) continue; // next enemy
                // Assignment
                if (tupleTargetable.Item1 == null) {
                    tupleTargetable = new Tuple<Character, int>(c, c.getActualHP());
                } else {
                    if (c.getActualHP() > tupleTargetable.Item2) tupleTargetable = new Tuple<Character, int>(c, c.getActualHP());
                    else if (c.getActualHP() == tupleTargetable.Item2 && getDistanceFromTarget(c) > getDistanceFromTarget(tupleTargetable.Item1)) tupleTargetable = new Tuple<Character, int>(c, c.getActualHP());
                }
            }
        }
        return tupleTargetable.Item1;
    }

    IEnumerator executeMinMax(float timing, bool debugEnabled) {
        float newTurnToWait = 3.2f;
        bool canExecute = true;
        bool hasAttacked = false;
        float timeToWait = timing;
        List<Block> whereToMove = null; // set to null after movement
        while (canExecute && !this.isDead) {
            // Wait when a decision is made
            yield return new WaitForSeconds(timeToWait);
            timeToWait = timing;
            newTurnToWait -= timing;
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
                bool mustChangeTarget = false;
                List<Character> ignored = new List<Character>();
                Character close = getClosestEnemy();
                if (close != null) {
                    ignored.Add(close);
                    toMove = close.connectedCell.GetComponent<Block>();
                    if (toMove.getFreeAdjacentBlocks().Count == 0 && getDistanceFromTarget(close) > 1) {
                        toMove = null;
                        mustChangeTarget = true;
                    }
                }
                // Cannot move because the target has no free adjacent cells
                if (mustChangeTarget) {
                    while(mustChangeTarget) {
                        close = getClosestEnemy(ignored);
                        if (close == null) break;
                        mustChangeTarget = false;
                        ignored.Add(close);
                        toMove = close.connectedCell.GetComponent<Block>();
                        if (toMove.getFreeAdjacentBlocks().Count == 0 && getDistanceFromTarget(close) > 1) {
                            toMove = null;
                            mustChangeTarget = true;
                        } else { mustChangeTarget = false; }
                    }
                }
                if (!hasAttacked) {
                    if (behaviour.movementWhenCannotAttack == MovementType.GoAhead &&
                        getDistanceFromTarget(close) == 1)
                        close = null;
                } else {
                    if (behaviour.movementAfterFight == MovementType.GoAhead &&
                        getDistanceFromTarget(close) == 1)
                        close = null;
                }
                
            }
            // Execute the action
            if (toExecute == null && toMove == null) {
                timeToWait = 0.05f;
                // DON'T EXECUTE NOTHING
                canExecute = false;
                if (debugEnabled) Debug.LogWarning("Enemy " + this.getCompleteName() + " cannot do any action!");
            } else if (toExecute != null) {
                // EXECUTE SPELL
                Spell.executeSpell(TurnsManager.active, toExecute.Item2.connectedCell.GetComponent<Block>(), toExecute.Item1);
                hasAttacked = true;
                if (debugEnabled) Debug.LogWarning("Enemy " + this.getCompleteName() + " will attack with spell " + toExecute.Item1.name);
            } else if (toMove != null) {
                // EXECUTE MOVEMENT
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
                    int limit = this.getActualPM();
                    if (limit >= 10) limit *= 2;
                    else if (limit >= 8) limit = (int)(limit * 2.5f);
                    else if (limit >= 5) limit *= 3;
                    else limit *= 5;
                    path = ai_reachEnemy(this.connectedCell.GetComponent<Block>(), toMove, limit);
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
                    timeToWait = 0.05f;
                }
                if (whereToMove != null) {
                    foreach (Block b in whereToMove) {
                        Debug.LogWarning(b.coordinate.display());
                    }
                    this.setMonsterPath(whereToMove); // walk
                    this.decrementPM_withoutEffect(1);
                    if (debugEnabled) Debug.LogWarning("Enemy " + this.getCompleteName() + " is moving of 1 cell");
                } else {
                    this.decrementPM_withoutEffect(this.getActualPM());
                    if (debugEnabled) Debug.LogWarning("Enemy " + this.getCompleteName() + " cannot move!");
                    timeToWait = 0.05f;
                }
            }
        }
        if (debugEnabled) Debug.LogWarning("Enemy " + this.getCompleteName() + " will pass the turn now");
        if (this.Equals(TurnsManager.active) && !this.isDead) {
            // The turn may be changed for poison effects
            if (newTurnToWait <= 0f)
                TurnsManager.Instance.OnNextTurnPressed();
            else StartCoroutine(passTurnWithDelay(newTurnToWait));
        }
    }

    IEnumerator passTurnWithDelay(float time) {
        yield return new WaitForSeconds(time);
        TurnsManager.Instance.OnNextTurnPressed();
    }

    #endregion

    #region IA-ALGORITHMS

    private List<Block> ai_reachEnemy(Block source, Block destination, int iterationLimit) {
        List<Node> leafs = new List<Node>();
        List<Node> analyzed = new List<Node>();
        Tree master = new Tree(source);
        leafs.Add(master.root);
        List<Block> path = null;
        Node reached = null;
        int counter = 0;
        while (reached == null && leafs.Count > 0 && counter <= iterationLimit) {
            counter++;
            List<Node> newLeafs = new List<Node>();
            foreach (Node leaf in leafs) {
                Block analyzing = leaf.item;
                List<Block> adjacents = analyzing.getFreeAdjacentBlocksWithEnemy(destination.linkedObject.GetComponent<Character>().team);
                foreach (Block adjacent in adjacents) {
                    Node temp = new Node(leaf, adjacent); // Creating a new leaf with the previous one as father
                    if (leaf.father != null) {
                        if (!temp.EqualsTo(leaf.father)) {
                            bool found = false;
                            foreach (Node alreadyPresent in analyzed)
                                if (alreadyPresent.EqualsTo(temp)) {
                                    found = true;
                                    break;
                                }
                            if (!found) {
                                newLeafs.Add(temp);
                            }
                        }
                    } else {
                        newLeafs.Add(temp);
                    }
                }
            }
            analyzed.AddRange(leafs);
            leafs.Clear();
            leafs.AddRange(newLeafs);
            newLeafs.Clear();
            foreach (Node leaf in leafs) {
                if (leaf.item.equalsTo(destination))
                    reached = leaf;
            }
        }
        if (reached != null) {
            path = reached.getPathItemsToRoot();
        } else {
            // creating a backup path
            Tuple<Node, int> minDistance = null;
            foreach (Node n in leafs) {
                Block considered = n.item;
                int dist = getDistance(considered.coordinate, destination.coordinate);
                if (minDistance == null) {
                    minDistance = new Tuple<Node, int>(n, dist);
                } else if (dist < minDistance.Item2) {
                    minDistance = new Tuple<Node, int>(n, dist);
                }
            }
            path = minDistance.Item1.getPathItemsToRoot();
        }
        return path;
    }

    #endregion

}
