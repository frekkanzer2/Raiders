using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Threading;
using System.Threading.Tasks;

public class Character : MonoBehaviour
{

    private int AI_SEARCHPATH_STEPS = 100;

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
    public int numberOfSummons;
    public bool isEvocation;
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
    public bool canMovedByEffects;
    private int kamaCounter = 0;

    private StatsOutputSystem sos;

    public static List<Block> bufferColored = new List<Block>();
    [HideInInspector]
    public bool isMoving = false;
    [HideInInspector]
    public bool isForcedMoving = false;
    [HideInInspector]
    public bool isBotMoving = false;
    private List<Block> followPath = new List<Block>();
    private Block followingBlock = null;
    private int movement_speed = 0;
    [HideInInspector]
    public Spell spellToUse;
    private EventSystem esystem;
    private SpellTurnSystem stsystem;
    [HideInInspector]
    public List<Evocation> summons;
    [HideInInspector]
    public List<MonsterEvocation> monsterSummons;
    [HideInInspector]
    public int summonsIdCounter = 0;

    public void setPath(List<Block> path) {
        if (isDead) return;
        followPath = path;
        followingBlock = path[0];
        followPath.RemoveAt(0);
        isForcedMoving = true;
    }

    public void setMonsterPath(List<Block> path) {
        if (isDead) return;
        followPath = path;
        followingBlock = path[0];
        followPath.RemoveAt(0);
        isBotMoving = true;
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
        if (!(this is Monster)) {
            if (pm <= 2)
                movement_speed = 10;
            else if (pm <= 4)
                movement_speed = 16;
            else if (pm <= 6)
                movement_speed = 20;
            else if (pm <= 8)
                movement_speed = 25;
            else movement_speed = 30;
        } else {
            movement_speed = 35;
        }
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

        // In-game movement when pushed from enemies
        if (isForcedMoving && !this.isDead) {
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
            if (new Vector2(transform.position.x, transform.position.y) == Coordinate.getPosition(followingBlock.coordinate) && !this.isDead) {
                // I'm on a new cell
                if (this.getEventSystem().getEvents("Toxic Injection").Count > 0) {
                    // damage on movement effect
                    ToxicInjectionEvent tie = (ToxicInjectionEvent)this.getEventSystem().getEvents("Toxic Injection")[0];
                    this.inflictDamage(Spell.calculateDamage(tie.referencedSpell.link, this, tie.referencedSpell) * this.getEventSystem().getEvents("Toxic Injection").Count);
                }
                if (!this.isDead) {
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
        }

        // In-game movement when pushed from enemies
        if (isBotMoving && !this.isDead && this is Monster) {
            if (this.transform.position.z > -20) this.transform.position = new Vector3(
                    this.transform.position.x,
                    this.transform.position.y,
                    -20
                );
            if (followingBlock == this.connectedCell.GetComponent<Block>() || followingBlock.isFree) {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    new Vector3(
                        Coordinate.getPosition(followingBlock.coordinate).x,
                        Coordinate.getPosition(followingBlock.coordinate).y,
                        -20
                    ),
                    30 * Time.deltaTime // Speed
                );
                if (new Vector2(transform.position.x, transform.position.y) == Coordinate.getPosition(followingBlock.coordinate) && !this.isDead) {
                    if (!this.isDead) {
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
                            isBotMoving = false;
                        }
                    }
                }
            } else {
                isBotMoving = false;
                followingBlock = null;
            }
        }

        // Don't execute following code if you are not the active player!
        if (TurnsManager.isGameStarted) {
            if (!this.isEvocation && !(this is Monster))
                if (this.name != TurnsManager.active.name || this.team != TurnsManager.active.team) {
                    return;
                }
            if (this.isEvocation) {
                if (!TurnsManager.active.isEvocation) return;
                if (this is Evocation && TurnsManager.active is Evocation) {
                    if (((Evocation)this).getCompleteName() != ((Evocation)TurnsManager.active).getCompleteName() || this.team != TurnsManager.active.team) {
                        return;
                    }
                } else if (this is MonsterEvocation && TurnsManager.active is MonsterEvocation) {
                    if (((MonsterEvocation)this).getCompleteName() != ((MonsterEvocation)TurnsManager.active).getCompleteName() || this.team != TurnsManager.active.team) {
                        return;
                    }
                } else if (this is MonsterEvocation && TurnsManager.active is Evocation) {
                    if (((MonsterEvocation)this).getCompleteName() != ((Evocation)TurnsManager.active).getCompleteName() || this.team != TurnsManager.active.team) {
                        return;
                    }
                } else if (this is Evocation && TurnsManager.active is MonsterEvocation) {
                    if (((Evocation)this).getCompleteName() != ((MonsterEvocation)TurnsManager.active).getCompleteName() || this.team != TurnsManager.active.team) {
                        return;
                    }
                }
            }
            if (this is Monster) {
                if (!(TurnsManager.active is Monster)) return;
                if (((Monster)this).getCompleteName() != ((Monster)TurnsManager.active).getCompleteName() || this.team != TurnsManager.active.team) {
                    return;
                }
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

        // Following code must be executed if and only if the actual player is not the Monster
        if (this is Monster) return;

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
                                followPath = ai_getDestinationPath(TurnsManager.active.connectedCell.GetComponent<Block>(), dest, AI_SEARCHPATH_STEPS);
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
                if (clicked.CompareTag("Player") && !(clicked.GetComponent<Character>() is Monster) && this.Equals(clicked.GetComponent<Character>())) {
                    PreparationManager.Instance.OnCellAlreadyChosen(this);
                }
            }
        }

        // DEBUG TO REMOVE
        if (Input.GetKeyDown(KeyCode.D)) {
            Debug.LogWarning("Debug cheat command!");
            List<Character> tempDmg = new List<Character>();
            tempDmg.AddRange(TurnsManager.Instance.turns);
            foreach (Character c in tempDmg)
                if (c.isEnemyOf(this))
                    c.inflictDamage(100);
        }

    }

    public void setZIndex(Block toRegolate) {
        if (isDead) return;
        this.GetComponent<SpriteRenderer>().sortingOrder = Coordinate.getBlockZindex(toRegolate.coordinate) + 5000;
    }

    public void turnPassed() {
        removeSpellToUse();
        esystem.OnEndTurn();
        stsystem.OnEndTurn();
        actual_pm = pm;
        actual_pa = pa;
    }

    public virtual void newTurn() {
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
        String firstName, secondName;
        if (this is Evocation) firstName = ((Evocation)this).getCompleteName();
        else if (this is Monster) firstName = ((Monster)this).getCompleteName();
        else firstName = this.name;
        if (c is Evocation) secondName = ((Evocation)c).getCompleteName();
        else if (c is Monster) secondName = ((Monster)c).getCompleteName();
        else secondName = c.name;
        return firstName.Equals(secondName) && this.team == c.team;
    }

    public bool EqualsNames(Character c) {
        String firstName, secondName;
        if (this is Evocation) firstName = ((Evocation)this).getCompleteName();
        else if (this is Monster) firstName = ((Monster)this).getCompleteName();
        else firstName = this.name;
        if (c is Evocation) secondName = ((Evocation)c).getCompleteName();
        else if (c is Monster) secondName = ((Monster)c).getCompleteName();
        else secondName = c.name;
        return firstName.Equals(secondName);
    }

    public bool isEnemyOf(Character c) {
        return (c.team != this.team);
    }

    #region FIGHT FUNCTIONS

    public void setupSOS(GameObject prefabToSpawn) {
        sos = GetComponent<StatsOutputSystem>();
        sos.setup(prefabToSpawn);
    }

    public virtual void inflictDamage(int damage, bool mustSkip = false) {
        if (isDead)
            return;
        if (mustSkip && damage >= getActualHP() + actual_shield)
            damage = (getActualHP() + actual_shield) - 1;
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
        if (damage == 0)
            return;
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

    public int getKama() {
        return this.kamaCounter;
    }

    public void incrementKama(int value) {
        if (isDead) return;
        this.kamaCounter += value;
        sos.addEffect_PA_PM(StatsOutputSystem.Effect.Kama, "+" + value + "K");
    }

    public void decrementPA(int value) {
        if (isDead) return;
        this.actual_pa -= value;
        sos.addEffect_PA_PM(StatsOutputSystem.Effect.PA, "-" + value);
    }

    public void decrementKama(int value) {
        if (isDead) return;
        this.kamaCounter -= value;
        sos.addEffect_PA_PM(StatsOutputSystem.Effect.Kama, "-" + value + "K");
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

    public virtual void setDead() {
        if (isDead) return;
        isDead = true;
        this.actual_hp = 0;
        if (this is Character && !(this is Monster)) {
            if (!isEvocation) {
                // Deleting summons in safe way
                List<Evocation> evoTemp = new List<Evocation>();
                evoTemp.AddRange(this.summons);
                foreach (Evocation e in evoTemp)
                    this.summons.Remove(e);
                foreach (Evocation e in evoTemp)
                    e.inflictDamage(e.actual_hp);
            }
        } else if (this is Monster) {
            if (!isEvocation) {
                // Deleting summons in safe way
                List<MonsterEvocation> evoTemp = new List<MonsterEvocation>();
                evoTemp.AddRange(this.monsterSummons);
                foreach (MonsterEvocation e in evoTemp)
                    this.monsterSummons.Remove(e);
                foreach (MonsterEvocation e in evoTemp)
                    e.inflictDamage(e.actual_hp);
            }
        }
        if (TurnsManager.active.Equals(this))
            TurnsManager.Instance.OnNextTurnPressed();
        connectedCell.GetComponent<Block>().linkedObject = null;
        connectedCell = null;
        esystem.removeAllEvents();
        stsystem.removeAllSpells();
        // dead player is the target of the sacrifice
        if (this.connectedSacrifice != null) {
            this.connectedSacrifice = null;
        }
        // dead player is the caster of the sacrifice
        if (this.hasActivedSacrifice) {
            this.hasActivedSacrifice = false;
            foreach(Character c in TurnsManager.Instance.turns) {
                if (c.connectedSacrifice != null)
                    if (c.connectedSacrifice.Equals(this))
                        c.connectedSacrifice = null;
            }
            this.connectedSacrifice = null;
        }
        StartCoroutine(dead_disappear());
        Destroy(connectedPreview);
        TurnsManager.Instance.turns.Remove(this);
        if (this.isEvocation) return;
        bool matchEnded = true;
        foreach(Character c in TurnsManager.Instance.turns) {
            if (!c.isEvocation)
                if (!c.isEnemyOf(this)) {
                    matchEnded = false;
                    break;
                }
        }
        if (this.team == 1)
            PlayerPrefs.SetInt("TEAM_WINNER", 2);
        else
            PlayerPrefs.SetInt("TEAM_WINNER", 1);
        if (matchEnded && SelectionContainer.DUNGEON_MonsterCharactersInfo == null)
            StartCoroutine(endMatch());
        else if (matchEnded && SelectionContainer.DUNGEON_MonsterCharactersInfo != null)
            StartCoroutine(endMatchMonsters());
    }

    IEnumerator endMatch() {
        resetBufferedCells();
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene("FightResultScene", LoadSceneMode.Single);
    }

    IEnumerator endMatchMonsters() {
        resetBufferedCells();
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene("DUNFightResultScene", LoadSceneMode.Single);
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

    protected class Tree {
        public Node root;
        public Tree(Block start) {
            root = new Node(start);
        }
    }

    protected class Node {
        public Node father;
        public Block item;
        public List<Node> childrens;
        public Node(Block item) {
            this.father = null;
            this.item = item;
            this.childrens = new List<Node>();
        }
        public Node(Node father, Block item) {
            this.father = father;
            this.item = item;
            this.childrens = new List<Node>();
        }
        public List<Block> getPathItemsToRoot() {
            Node actual = this;
            Node previous = this;
            List<Block> toReturn = new List<Block>();
            while (previous != null) {
                toReturn.Insert(0, previous.item);
                previous = actual.father;
                if (previous != null) {
                    actual = previous;
                }
            }
            return toReturn;
        }
        public bool EqualsTo(Node n) {
            if (n.item.equalsTo(this.item)) return true;
            else return false;
        }
        public override bool Equals(object obj) {
            return EqualsTo((Node)obj);
        }
    }

    // iterationLimit = actual_pm
    public List<Block> ai_getReachableBlocks(Block source, int iterationLimit) {
        List<Node> leafs = new List<Node>();
        List<Node> analyzed = new List<Node>();
        Tree master = new Tree(source);
        leafs.Add(master.root);
        List<Block> visitedBlocks = new List<Block>();
        int counter = 0;
        while (leafs.Count > 0 && counter <= iterationLimit) {
            counter++;
            List<Node> newLeafs = new List<Node>();
            foreach (Node leaf in leafs) {
                Block analyzing = leaf.item;
                List<Block> adjacents = analyzing.getFreeAdjacentBlocks();
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
                            if (!found)
                                newLeafs.Add(temp);
                        }
                    } else
                        newLeafs.Add(temp);
                }
                visitedBlocks.Add(leaf.item);
            }
            analyzed.AddRange(leafs);
            leafs.Clear();
            leafs.AddRange(newLeafs);
            newLeafs.Clear();
        }
        return visitedBlocks;
    }

    public List<Block> ai_getDestinationPath(Block source, Block destination, int iterationLimit) {
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
            foreach(Node leaf in leafs) {
                Block analyzing = leaf.item;
                List<Block> adjacents = analyzing.getFreeAdjacentBlocks();
                foreach(Block adjacent in adjacents) {
                    Node temp = new Node(leaf, adjacent); // Creating a new leaf with the previous one as father
                    if (leaf.father != null) {
                        if (!temp.EqualsTo(leaf.father)) {
                            bool found = false;
                            foreach (Node alreadyPresent in analyzed)
                                if (alreadyPresent.EqualsTo(temp)) {
                                    found = true;
                                    break;
                                }
                            if (!found)
                                newLeafs.Add(temp);
                        }
                    } else
                        newLeafs.Add(temp);
                }
            }
            analyzed.AddRange(leafs);
            leafs.Clear();
            leafs.AddRange(newLeafs);
            newLeafs.Clear();
            foreach (Node leaf in leafs)
                if (leaf.item.equalsTo(destination))
                    reached = leaf;
        }
        if (reached != null) {
            path = reached.getPathItemsToRoot();
        }
        return path;
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
        SynchronizedCollection<Block> blocksToRemove = new SynchronizedCollection<Block>();
        foreach(Block b in bufferColored) {
            if (b.linkedObject != null)
                blocksToRemove.Add(b);
        }
        foreach(Block b in blocksToRemove)
            bufferColored.Remove(b);
        blocksToRemove.Clear();
        Block originBlock = origin.connectedCell.GetComponent<Block>();
        // Reachable blocks
        List<Block> reachables = null;
        reachables = ai_getReachableBlocks(originBlock, origin.actual_pm);
        // Checking all reachable blocks
        Task[] allTasks = new Task[bufferColored.Count];
        int taskIndex = 0;
        foreach (Block bufferedBlock in bufferColored) {
            allTasks[taskIndex] = Task.Factory.StartNew(
                    () => th_work_DestPath(
                            bufferedBlock,
                            reachables,
                            blocksToRemove // reference to synchronized list
                        )
                );
            taskIndex++;
        }
        Task.WaitAll(allTasks);
        foreach (Block b in blocksToRemove)
            bufferColored.Remove(b);
    }

    public void th_work_DestPath(Block toCheck, List<Block> list, SynchronizedCollection<Block> toRemove) {
        bool found = false;
        foreach (Block b in list)
            if (toCheck.equalsTo(b)) {
                found = true;
                break;
            }
        if (!found)
            toRemove.Add(toCheck);
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

        // Removing cells if the spell is in diagonal range
        if (selected.distanceType == Spell.DistanceType.Diagonal) {
            Block source = origin.connectedCell.GetComponent<Block>();
            foreach (Block b in bufferColored) {
                if (Mathf.Abs(source.coordinate.row - b.coordinate.row) != Mathf.Abs(source.coordinate.column - b.coordinate.column))
                    toRemove.Add(b);
            }
            foreach (Block b in toRemove)
                bufferColored.Remove(b);
            toRemove.Clear();
        }

        // Delete blocks where there's an hero
        if (selected.isJump || selected.isSummon) {
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
