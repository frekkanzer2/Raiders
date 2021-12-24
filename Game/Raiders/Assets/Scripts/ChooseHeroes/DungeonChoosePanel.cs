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
    }

    public void OnRightPressed() {
        if (page == save.getNextDungeonID(dungeonsList)) return;
        page++;
        setDungeonPreview(page);
    }

}
