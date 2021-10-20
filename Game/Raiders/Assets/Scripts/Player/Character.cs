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

    public static List<Block> bufferColored = new List<Block>();
    private bool isMoving = false;
    private List<Block> followPath = new List<Block>();
    private Block followingBlock = null;
    private int movement_speed = 0;

    public bool isDebugEnabled = false;

    void Start()
    {
        if (isDebugEnabled) Debug.LogWarning("DEBUG > Connect manually the hero to the cell and start by pressing SPACE key");
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
    }

    // Update is called once per frame
    void Update()
    {

        if (TurnsManager.isGameStarted && this.connectedPreview != null) {
            Slider s = this.connectedPreview.transform.GetChild(2).gameObject.GetComponent<Slider>();
            s.maxValue = this.hp;
            s.minValue = 0;
            s.value = this.actual_hp;
        }

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

        if (Input.GetMouseButtonDown(0) && !isMoving && TurnsManager.isGameStarted) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null) {
                if (hit.collider.gameObject.CompareTag("Player")) {
                    resetBufferedCells();
                    Character charPressed = hit.collider.gameObject.GetComponent<Character>();
                    if (charPressed.Equals(TurnsManager.active)) {
                        if (charPressed.actual_pm > 0) {
                            displayMovementCells(TurnsManager.active);
                        }
                    }
                } else if (hit.collider.gameObject.CompareTag("Block")) {
                    Block selected = hit.collider.gameObject.GetComponent<Block>();
                    if (this.Equals(TurnsManager.active)) {
                        Block dest = null;
                        foreach (Block b in bufferColored)
                            if (selected.equalsTo(b)) {
                                dest = b;
                                break;
                            }
                        resetBufferedCells();
                        if (dest != null) {
                            followPath = ai_getDestinationPath(TurnsManager.active.connectedCell.GetComponent<Block>(), dest, 100);
                            actual_pm -= followPath.Count - 1;
                            followingBlock = followPath[0];
                            followPath.RemoveAt(0);
                            isMoving = true;
                        }
                    }
                }
            } else {
                resetBufferedCells();
            }
        } else if (Input.GetMouseButtonDown(0) && !isMoving && !TurnsManager.isGameStarted && CameraDragDrop.canMove) {
            // Clicked positioned preview
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
            if (hit.collider != null) {
                GameObject clicked = hit.collider.gameObject;
                if (clicked.CompareTag("Player") && clicked.GetComponent<Character>().name == this.name && clicked.GetComponent<Character>().team == this.team) {
                    PreparationManager.Instance.OnCellAlreadyChosen(this);
                }
            }
        }

        if (isDebugEnabled) {
            if (Input.GetKeyDown(KeyCode.Space)) {
                Debug.LogWarning("DEBUG > Pressed SPACE key: new turn started");
                TurnsManager.active = this;
                this.connectedCell.GetComponent<Block>().linkedObject = this.gameObject;
                turnPassed();
                newTurn();
            }
        }

    }

    public void setZIndex(Block toRegolate) {
        this.GetComponent<SpriteRenderer>().sortingOrder = Coordinate.getBlockZindex(toRegolate.coordinate) + 10;
    }

    public void turnPassed() {
        actual_pm = pm;
        actual_pa = pa;
    }

    public void newTurn() {
        resetBufferedCells();
        this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, -20);
    }

    void resetBufferedCells() {
        foreach (Block b in bufferColored) {
            b.resetColor();
        }
        bufferColored.Clear();
    }

    void displayMovementCells(Character origin) {

        ai_pmComposer(origin);
        foreach (Block b in bufferColored) {
            b.setMovementColor();
        }

    }

    public bool Equals(Character c) {
        return this.name == c.name && this.team == c.team;
    }

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
            path = ai_getDestinationPath(origin.connectedCell.GetComponent<Block>(), b, 100);
            if (path == null) toRemove.Add(b);
            else if (path.Count > origin.actual_pm + 1) toRemove.Add(b);
        }
        foreach(Block b in toRemove)
            bufferColored.Remove(b);

    }

    #endregion

    #region ai-heuristics

    private int h_euclidian(Coordinate start, Coordinate destination, int weight = 1) {
        float dx = Mathf.Abs(start.column - destination.column);
        float dy = Mathf.Abs(start.row - destination.row);
        return (int) (weight * Mathf.Sqrt(dx * dx + dy * dy));
    }

    #endregion

}
