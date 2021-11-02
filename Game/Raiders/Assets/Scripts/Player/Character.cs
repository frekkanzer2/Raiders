using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Character : MonoBehaviour
{

    public string name;
    public List<Spell> spells;
    public int team;
    public int hp;
    public int pa;
    public int pm;
    public int ini;
    public int res_e;
    public int res_f;
    public int res_a;
    public int res_w;
    public int att_e;
    public int att_f;
    public int att_a;
    public int att_w;
    [HideInInspector]
    public int actual_hp;
    [HideInInspector]
    public int actual_pa;
    [HideInInspector]
    public int actual_pm;
    [HideInInspector]
    public GameObject connectedPreview = null;
    public GameObject connectedCell;
    [HideInInspector]
    public int actual_shield = 0;
    [HideInInspector]
    public bool isDead = false;

    private StatsOutputSystem sos;

    public static List<Block> bufferColored = new List<Block>();
    [HideInInspector]
    public bool isMoving = false;
    [HideInInspector]
    public bool isForcedMoving = false;
    private List<Block> followPath = new List<Block>();
    private Block followingBlock = null;
    private int movement_speed = 0;
    [HideInInspector]
    public Spell spellToUse;
    private EventSystem esystem;
    private SpellTurnSystem stsystem;

    public void setPath(List<Block> path) {
        if (isDead) return;
        followPath = path;
        followingBlock = path[0];
        followPath.RemoveAt(0);
        isForcedMoving = true;
    }

    public void addEvent(ParentEvent pe) {
        if (isDead) return;
        esystem.addEvent(pe);
        sos.addEffect_Icon(StatsOutputSystem.Effect.Icon, pe.sprite);
    }

    public void addSpell(Spell sp) {
        if (isDead) return;
        this.stsystem.addEvent(new ParentActiveSpell(sp));
    }

    public void setSpellToUse(Spell s) {
        if (isDead) return;
        this.spellToUse = s;
    }

    public EventSystem getEventSystem() {
        return this.esystem;
    }

    public SpellTurnSystem getSpellSystem() {
        return this.stsystem;
    }

    public void removeSpellToUse() {
        if (isDead) return;
        TurnsManager.Instance.popupSpell.GetComponent<SpellPopup>().OnExit();
        this.spellToUse = null;
    }

    public bool isDebugEnabled = false;

    void Start()
    {
        this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, -20);
        actual_hp = hp;
        actual_pa = pa;
        actual_pm = pm;
        if (pm <= 2)
            movement_speed = 10;
        else if (pm <= 4)
            movement_speed = 16;
        else if (pm <= 6)
            movement_speed = 20;
        else if (pm <= 8)
            movement_speed = 25;
        else movement_speed = 30;
        foreach (Spell s in spells) {
            s.link = this;
        }
        esystem = GetComponent<EventSystem>();
        stsystem = GetComponent<SpellTurnSystem>();
    }

    // Update is called once per frame
    void Update() {

        // Setting lifebar on preview cards
        if (TurnsManager.isGameStarted && this.connectedPreview != null && !this.isDead) {
            Slider s = this.connectedPreview.transform.GetChild(2).gameObject.GetComponent<Slider>();
            s.maxValue = this.hp;
            s.minValue = 0;
            s.value = this.actual_hp;
        }

        if (isForcedMoving) {
            if (this.transform.position.z > -20) this.transform.position = new Vector3(
                    this.transform.position.x,
                    this.transform.position.y,
                    -20
                );
            transform.position = Vector3.MoveTowards(
                transform.position,
                new Vector3(
                    Coordinate.getPosition(followingBlock.coordinate).x,
                    Coordinate.getPosition(followingBlock.coordinate).y,
                    -20
                ),
                30 * Time.deltaTime // Speed
            );
            if (new Vector2(transform.position.x, transform.position.y) == Coordinate.getPosition(followingBlock.coordinate)) {
                // I'm on a new cell
                GameObject previous_link = this.connectedCell;
                previous_link.GetComponent<Block>().linkedObject = null;
                this.connectedCell = followingBlock.gameObject;
                followingBlock.GetComponent<Block>().linkedObject = this.gameObject;
                setZIndex(followingBlock);
                if (followPath.Count > 0) {
                    followingBlock = followPath[0];
                    followPath.RemoveAt(0);
                } else {
                    followingBlock = null;
                    isForcedMoving = false;
                }
            }
        }

        // Don't execute following code if you are not the active player!
        if (TurnsManager.isGameStarted) {
            if (this.name != TurnsManager.active.name || this.team != TurnsManager.active.team) {
                return;
            }
            if (this.connectedCell != null)
                this.setZIndex(this.connectedCell.GetComponent<Block>());
        }

        // Snippet that moves the hero in the selected cell
        if (isMoving) {
            transform.position = Vector3.MoveTowards(
                transform.position, 
                new Vector3(
                    Coordinate.getPosition(followingBlock.coordinate).x, 
                    Coordinate.getPosition(followingBlock.coordinate).y, 
                    -20
                ), 
                movement_speed * Time.deltaTime // Speed
            );
            if (new Vector2(transform.position.x, transform.position.y) == Coordinate.getPosition(followingBlock.coordinate)) {
                // I'm on a new cell
                GameObject previous_link = this.connectedCell;
                previous_link.GetComponent<Block>().linkedObject = null;
                this.connectedCell = followingBlock.gameObject;
                followingBlock.GetComponent<Block>().linkedObject = this.gameObject;
                setZIndex(followingBlock);
                if (followPath.Count > 0) {
                    followingBlock = followPath[0];
                    followPath.RemoveAt(0);
                } else {
                    followingBlock = null;
                    isMoving = false;
                }
            }
        }

        // While playing...
        if (Input.GetMouseButtonDown(0) && !isMoving && TurnsManager.isGameStarted) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null) {
                // ...pressed an hero to display the cells
                if (hit.collider.gameObject.CompareTag("Player")) {
                    Character charPressed = hit.collider.gameObject.GetComponent<Character>();
                    bool isAtt = false;
                    if (charPressed.connectedCell.GetComponent<Block>().canAttackHere && spellToUse != null) {
                        isAtt = true;
                        Spell.executeSpell(TurnsManager.active, charPressed.connectedCell.GetComponent<Block>(), TurnsManager.active.spellToUse);
                    }
                    resetBufferedCells();
                    if (!isAtt)
                        if (charPressed.Equals(TurnsManager.active)) {
                            if (charPressed.actual_pm > 0) {
                                displayMovementCells(TurnsManager.active);
                            }
                        }
                    isAtt = false;
                    // ...pressed a block...
                } else if (hit.collider.gameObject.CompareTag("Block")) {
                    Block selected = hit.collider.gameObject.GetComponent<Block>();
                    // ...where the hero can moves
                    if (selected.canMoveHere) {
                        if (this.Equals(TurnsManager.active)) {
                            Block dest = null;
                            foreach (Block b in bufferColored)
                                if (selected.equalsTo(b)) {
                                    dest = b;
                                    break;
                                }
                            resetBufferedCells();
                            if (dest != null) {
                                followPath = ai_getDestinationPath(TurnsManager.active.connectedCell.GetComponent<Block>(), dest, 800);
                                actual_pm -= followPath.Count - 1;
                                followingBlock = followPath[0];
                                followPath.RemoveAt(0);
                                isMoving = true;
                            }
                        }
                        // ...where the hero can attack
                    } else if (selected.canAttackHere) {
                        Spell.executeSpell(TurnsManager.active, selected, TurnsManager.active.spellToUse);
                        resetBufferedCells();
                    } else {
                        // ...where there's nothing
                        resetBufferedCells();
                    }
                }
                // nothing clicked
            } else {
                resetBufferedCells();
            }
            // clicked a character while choosing phase to remove it from the field
        } else if (Input.GetMouseButtonDown(0) && !isMoving && !TurnsManager.isGameStarted && CameraDragDrop.canMove) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
            if (hit.collider != null) {
                GameObject clicked = hit.collider.gameObject;
                if (clicked.CompareTag("Player") && clicked.GetComponent<Character>().name == this.name && clicked.GetComponent<Character>().team == this.team) {
                    PreparationManager.Instance.OnCellAlreadyChosen(this);
                }
            }
        }

    }

    public void setZIndex(Block toRegolate) {
        if (isDead) return;
        this.GetComponent<SpriteRenderer>().sortingOrder = Coordinate.getBlockZindex(toRegolate.coordinate) + 10;
    }

    public void turnPassed() {
        removeSpellToUse();
        esystem.OnEndTurn();
        stsystem.OnEndTurn();
        actual_pm = pm;
        actual_pa = pa;
    }

    public void newTurn() {
        resetBufferedCells();
        this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, -20);
        esystem.OnStartTurn();
    }

    void resetBufferedCells() {
        foreach (Block b in bufferColored) {
            b.resetColor();
        }
        bufferColored.Clear();
        TurnsManager.active.removeSpellToUse();
    }

    void displayMovementCells(Character origin) {
        if (origin.isMoving) return;
        ai_pmComposer(origin);
        foreach (Block b in bufferColored) {
            b.setMovementColor();
        }
    }

    public void displayAttackCells(Character origin, Spell selected) {
        resetBufferedCells();
        if (origin.isMoving) return;
        List<Block> cantAttackHere = ai_attackComposer(origin, selected);
        foreach (Block b in bufferColored) {
            b.setAttackColor();
        }
        foreach (Block b in cantAttackHere) {
            b.setCantAttackColor();
        }
        bufferColored.AddRange(cantAttackHere);
        TurnsManager.active.setSpellToUse(selected);
    }

    public bool Equals(Character c) {
        return this.name == c.name && this.team == c.team;
    }

    #region FIGHT FUNCTIONS

    public void setupSOS(GameObject prefabToSpawn) {
        sos = GetComponent<StatsOutputSystem>();
        sos.setup(prefabToSpawn);
    }

    public void inflictDamage(int damage) {
        if (isDead) return;
        if (actual_shield > 0) {
            int prev_sh = actual_shield; // 80
            actual_shield -= damage; // 40
            if (actual_shield <= 0) {
                damage = actual_shield * -1;
                actual_shield = 0;
            }
            if (actual_shield <= 0)
                sos.addEffect_Shield(StatsOutputSystem.Effect.Shield, "-" + prev_sh);
            else {
                sos.addEffect_Shield(StatsOutputSystem.Effect.Shield, "-" + damage);
                damage = 0;
            }
        }
        if (damage == 0) return;
        if (this.actual_hp - damage < 0) actual_hp = 0;
        else this.actual_hp -= damage;
        if (actual_hp == 0) setDead();
        sos.addEffect_DMG_Heal(StatsOutputSystem.Effect.HP, damage);
    }

    public void receiveHeal(int heal) {
        if (isDead) return;
        if (this.actual_hp + heal > this.hp) actual_hp = hp;
        else this.actual_hp += heal;
        sos.addEffect_DMG_Heal(StatsOutputSystem.Effect.Heal, heal);
    }

    public void receiveShield(int sh) {
        if (isDead) return;
        this.actual_shield += sh;
        sos.addEffect_Shield(StatsOutputSystem.Effect.Shield, "+" + sh);
    }

    public void removeShield(int sh) {
        if (isDead) return;
        if (this.actual_shield == 0) return;
        int prev_sh = actual_shield;
        this.actual_shield -= sh;
        if (this.actual_shield <= 0) {
            actual_shield = 0;
            sos.addEffect_Shield(StatsOutputSystem.Effect.Shield, "-" + prev_sh);
        } else
            sos.addEffect_Shield(StatsOutputSystem.Effect.Shield, "-" + sh);
    }

    public int getActualHP() {
        return this.actual_hp;
    }

    public int getTotalHP() {
        return this.hp;
    }

    public int getActualPA() {
        return this.actual_pa;
    }

    public void incrementPA(int value) {
        if (isDead) return;
        this.actual_pa += value;
        sos.addEffect_PA_PM(StatsOutputSystem.Effect.PA, "+" + value);
    }

    public void decrementPA(int value) {
        if (isDead) return;
        this.actual_pa -= value;
        sos.addEffect_PA_PM(StatsOutputSystem.Effect.PA, "-" + value);
    }

    public int getActualPM() {
        return this.actual_pm;
    }

    public void incrementPM(int value) {
        if (isDead) return;
        this.actual_pm += value;
        sos.addEffect_PA_PM(StatsOutputSystem.Effect.PM, "+" + value);
    }

    public void decrementPM(int value) {
        if (isDead) return;
        this.actual_pm -= value;
        sos.addEffect_PA_PM(StatsOutputSystem.Effect.PM, "-" + value);
    }

    public void decrementHP_withoutEffect(int value) {
        if (isDead) return;
        this.actual_hp -= value;
        if (actual_hp == 0) setDead();
    }

    public void decrementPA_withoutEffect(int value) {
        if (isDead) return;
        this.actual_pa -= value;
    }

    public void decrementPM_withoutEffect(int value) {
        if (isDead) return;
        this.actual_pm -= value;
    }

    public void setDead() {
        if (isDead) return;
        isDead = true;
        if (TurnsManager.active.Equals(this))
            TurnsManager.Instance.OnNextTurnPressed();
        connectedCell.GetComponent<Block>().linkedObject = null;
        connectedCell = null;
        esystem.removeAllEvents();
        stsystem.removeAllSpells();
        this.hasActivedSacrifice = false;
        this.connectedSacrifice = null;
        StartCoroutine(dead_disappear());
        Destroy(connectedPreview);
        TurnsManager.Instance.turns.Remove(this);
    }

    IEnumerator dead_disappear() {
        SpriteRenderer sr = this.gameObject.GetComponent<SpriteRenderer>();
        while (sr.color.a > 0.05f) {
            sr.color = new Color(sr.color.r, sr.color.b, sr.color.g, sr.color.a - 0.06f);
            yield return new WaitForSeconds(0.05f);
        }
        sr.color = new Color(0, 0, 0, 0);
        this.transform.position = new Vector3(50000, 50000, 0);
    }

    #endregion

    #region IA-ALGORITHMS

    private List<Block> ai_getDestinationPath(Block source, Block destination, int iterationLimit) {
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
        return resultPath;
    }

    private void ai_pmComposer(Character origin) {
        int up = origin.actual_pm, down = origin.actual_pm * -1;
        int counter = 0;
        Block actual = origin.connectedCell.GetComponent<Block>();
        Block _temp;

        while (up >= 0 || down < 0) {
            if (counter == 0) {
                _temp = Map.Instance.getBlock(new Coordinate(actual.coordinate.row + up, actual.coordinate.column));
                if (_temp != null) bufferColored.Add(_temp); // upper limit block
                _temp = Map.Instance.getBlock(new Coordinate(actual.coordinate.row + down, actual.coordinate.column));
                if (_temp != null) bufferColored.Add(_temp); // down limit block
            } else {
                if (up != 0 && down != 0) {
                    int i = counter; int j = 0 - counter;
                    while (i > 0 || j < 0) {
                        _temp = Map.Instance.getBlock(new Coordinate(actual.coordinate.row + up, actual.coordinate.column + i));
                        if (_temp != null) bufferColored.Add(_temp);
                        _temp = Map.Instance.getBlock(new Coordinate(actual.coordinate.row + up, actual.coordinate.column + j));
                        if (_temp != null) bufferColored.Add(_temp);
                        _temp = Map.Instance.getBlock(new Coordinate(actual.coordinate.row + down, actual.coordinate.column + i));
                        if (_temp != null) bufferColored.Add(_temp);
                        _temp = Map.Instance.getBlock(new Coordinate(actual.coordinate.row + down, actual.coordinate.column + j));
                        if (_temp != null) bufferColored.Add(_temp);
                        i--; j++;
                    }
                    _temp = Map.Instance.getBlock(new Coordinate(actual.coordinate.row + up, actual.coordinate.column));
                    if (_temp != null) bufferColored.Add(_temp); // upper limit block
                    _temp = Map.Instance.getBlock(new Coordinate(actual.coordinate.row + down, actual.coordinate.column));
                    if (_temp != null) bufferColored.Add(_temp); // down limit block
                }
                if (up == 0 && down == 0) {
                    int i = counter; int j = 0 - counter;
                    while (i > 0 || j < 0) {
                        _temp = Map.Instance.getBlock(new Coordinate(actual.coordinate.row + up, actual.coordinate.column + i));
                        if (_temp != null) bufferColored.Add(_temp);
                        _temp = Map.Instance.getBlock(new Coordinate(actual.coordinate.row + up, actual.coordinate.column + j));
                        if (_temp != null) bufferColored.Add(_temp);
                        i--; j++;
                    }
                }
            }
            up--; down++; counter++;
        }
        
        // Remove unreachable blocks
        List<Block> toRemove = new List<Block>();
        foreach(Block b in bufferColored) {
            List<Block> path = null;
            path = ai_getDestinationPath(origin.connectedCell.GetComponent<Block>(), b, 800);
            if (path == null) toRemove.Add(b);
            else if (path.Count > origin.actual_pm + 1) toRemove.Add(b);
        }
        foreach(Block b in toRemove)
            bufferColored.Remove(b);

    }

    private List<Block> ai_attackComposer(Character origin, Spell selected) {

        int range_start = selected.minRange;
        int range_end = selected.maxRange;
        
        int up = range_end, down = 0 - range_end;
        int limit_up = range_start, limit_down = 0 - range_start;

        int highCounter = 0, loseCounter = 0;
        int i = 0, j = 0;

        Block actual = origin.connectedCell.GetComponent<Block>();
        Block _temp;

        while (up > 0 || down < 0) {
            if (highCounter == 0) {
                _temp = Map.Instance.getBlock(new Coordinate(actual.coordinate.row + up, actual.coordinate.column));
                if (_temp != null) bufferColored.Add(_temp); // upper limit block
                _temp = Map.Instance.getBlock(new Coordinate(actual.coordinate.row + down, actual.coordinate.column));
                if (_temp != null) bufferColored.Add(_temp); // down limit block
            } else {
                if (up >= limit_up || down <= limit_down) {
                    i = 0 - highCounter;
                    j = highCounter;
                    while (i < 0 || j > 0) {
                        _temp = Map.Instance.getBlock(new Coordinate(actual.coordinate.row + up, actual.coordinate.column + i));
                        if (_temp != null) bufferColored.Add(_temp);
                        _temp = Map.Instance.getBlock(new Coordinate(actual.coordinate.row + up, actual.coordinate.column + j));
                        if (_temp != null) bufferColored.Add(_temp);
                        _temp = Map.Instance.getBlock(new Coordinate(actual.coordinate.row + down, actual.coordinate.column + i));
                        if (_temp != null) bufferColored.Add(_temp);
                        _temp = Map.Instance.getBlock(new Coordinate(actual.coordinate.row + down, actual.coordinate.column + j));
                        if (_temp != null) bufferColored.Add(_temp);
                        i++; j--;
                    }
                    _temp = Map.Instance.getBlock(new Coordinate(actual.coordinate.row + up, actual.coordinate.column));
                    if (_temp != null) bufferColored.Add(_temp); // upper limit block
                    _temp = Map.Instance.getBlock(new Coordinate(actual.coordinate.row + down, actual.coordinate.column));
                    if (_temp != null) bufferColored.Add(_temp); // down limit block
                }
                else {
                    loseCounter++;
                    i = 0 - highCounter;
                    j = highCounter;
                    while (i <= 0-loseCounter || j >= loseCounter) {
                        _temp = Map.Instance.getBlock(new Coordinate(actual.coordinate.row + up, actual.coordinate.column + i));
                        if (_temp != null) bufferColored.Add(_temp);
                        _temp = Map.Instance.getBlock(new Coordinate(actual.coordinate.row + up, actual.coordinate.column + j));
                        if (_temp != null) bufferColored.Add(_temp);
                        _temp = Map.Instance.getBlock(new Coordinate(actual.coordinate.row + down, actual.coordinate.column + i));
                        if (_temp != null) bufferColored.Add(_temp);
                        _temp = Map.Instance.getBlock(new Coordinate(actual.coordinate.row + down, actual.coordinate.column + j));
                        if (_temp != null) bufferColored.Add(_temp);
                        i++; j--;
                    }
                }
            }
            up--; down++; highCounter++;
        }

        int minv = range_start;
        int maxv = range_end;
        while (minv <= maxv) {
            _temp = Map.Instance.getBlock(new Coordinate(actual.coordinate.row, actual.coordinate.column + minv));
            if (_temp != null) bufferColored.Add(_temp);
            _temp = Map.Instance.getBlock(new Coordinate(actual.coordinate.row, actual.coordinate.column + (0-minv)));
            if (_temp != null) bufferColored.Add(_temp);
            minv++;
        }

        List<Block> toRemove = new List<Block>();
        // Removing cells if the spell is in line range
        if (selected.distanceType == Spell.DistanceType.Line) {
            foreach (Block b in bufferColored) {
                if (b.coordinate.row != origin.connectedCell.GetComponent<Block>().coordinate.row &&
                    b.coordinate.column != origin.connectedCell.GetComponent<Block>().coordinate.column) {
                    toRemove.Add(b);
                }
            }
            foreach (Block b in toRemove)
                bufferColored.Remove(b);
            toRemove.Clear();
        }

        // Delete blocks where there's an hero
        if (selected.isJumpOrEvocation) {
            foreach (Block b in bufferColored) {
                if (b.linkedObject != null) {
                    toRemove.Add(b);
                }
            }
            foreach (Block b in toRemove)
                bufferColored.Remove(b);
            toRemove.Clear();
        }

        // LEAVE THIS SECTION OF CODE AS LAST
        // Removing cells if the spell doesn't hit behind obstacles
        if (!selected.overObstacles) {
            foreach (Block b in bufferColored) {
                Debug.DrawLine(origin.connectedCell.GetComponent<Block>().transform.position, b.gameObject.transform.position);
                RaycastHit2D[] hits = Physics2D.LinecastAll(origin.connectedCell.GetComponent<Block>().transform.position, b.gameObject.transform.position);
                foreach (RaycastHit2D hit in hits) {
                    GameObject collided = hit.collider.gameObject;
                    Block retrieved = collided.GetComponent<Block>();
                    if (retrieved != null) {
                        if (retrieved.coordinate.equalsTo(b.coordinate)) {
                            continue;
                        } else if (retrieved.coordinate.equalsTo(origin.connectedCell.GetComponent<Block>().coordinate)) {
                            continue;
                        } else if (retrieved.linkedObject != null) {
                            toRemove.Add(b);
                            break;
                        }
                    }
                }
            }
            foreach (Block b in toRemove)
                bufferColored.Remove(b);
        }
        return toRemove; // This blocks will be colored with another colour

    }

    #endregion

    #region IA-HEURISTICS

    private int h_euclidian(Coordinate start, Coordinate destination, int weight = 1) {
        float dx = Mathf.Abs(start.column - destination.column);
        float dy = Mathf.Abs(start.row - destination.row);
        return (int) (weight * Mathf.Sqrt(dx * dx + dy * dy));
    }

    #endregion

    #region SPECIAL STATS

    [HideInInspector]
    public int accumulationCounter = 0;
    [HideInInspector]
    public bool criticShooting = false;
    [HideInInspector]
    public Character connectedSacrifice = null;
    [HideInInspector]
    public bool hasActivedSacrifice = false;
    [HideInInspector]
    public bool canCritical = true;
    [HideInInspector]
    public bool immuneCloseCombat = false;

    #endregion

}
