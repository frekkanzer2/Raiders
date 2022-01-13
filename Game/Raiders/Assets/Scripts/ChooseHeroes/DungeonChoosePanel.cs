using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DungeonChoosePanel : MonoBehaviour {

    private List<DungeonUtils> dungeonsList;
    public GameObject textDungeonName;
    public GameObject imageDungeonKeeper;
    private DungeonSave save;
    private int page = 0;

    // Start is called before the first frame update
    void Start()
    {
        save = new DungeonSave();
        dungeonsList = GetComponent<DungeonContainer>().dungeonsList;
        DungeonSave.setAllDungeons(dungeonsList);
        setDungeonPreview();
        page = save.getNextDungeonID(dungeonsList);
        setUpgradeStats();
    }

    private void setDungeonPreview() {
        textDungeonName.GetComponent<TextMeshProUGUI>().text = save.getNextDungeonNameByID(dungeonsList);
        imageDungeonKeeper.GetComponent<Image>().sprite = save.getNextDungeonSpriteByID(dungeonsList);
    }

    private void setDungeonPreview(int customIndex) {
        textDungeonName.GetComponent<TextMeshProUGUI>().text = save.getDungeonNameByID(dungeonsList, customIndex);
        imageDungeonKeeper.GetComponent<Image>().sprite = save.getDungeonSpriteByID(dungeonsList, customIndex);
    }

    public int getSelectedDungeonID() {
        return page;
    }

    public void OnLeftPressed() {
        if (page == 0) return;
        page--;
        setDungeonPreview(page);
        setUpgradeStats();
    }

    public void OnRightPressed() {
        if (page == save.getNextDungeonID(dungeonsList)) return;
        page++;
        setDungeonPreview(page);
        setUpgradeStats();
    }

    private void setUpgradeStats() {
        int multiplier = getSelectedDungeonID() + 1;
        int points = multiplier * 5;
        DUNSelectionManager.UPGRADE = new Upgrade();
        DUNSelectionManager.UPGRADE.availablePoints = points;
        GameObject textToEdit = GameObject.Find("POINTS_TEXT");
        textToEdit.GetComponent<TextMeshProUGUI>().text = "" + points;
    }

}
