using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGameController : MonoBehaviour
{

    int winningTeam = -1;
    int teamDimension = -1;
    List<string> names = new List<string>();

    public Sprite redVersion;
    public GameObject spriteToChange;

    public GameObject prefab_hero;
    public GameObject whereToGenerate;

    public GameObject manager;

    // Start is called before the first frame update
    void Start()
    {
        winningTeam = PlayerPrefs.GetInt("TEAM_WINNER");
        teamDimension = PlayerPrefs.GetInt("TEAM_DIMENSION");
        string teamName = "";
        if (winningTeam == 1)
            teamName = "ALPHA";
        else if (winningTeam == 2) {
            teamName = "BETA";
            spriteToChange.GetComponent<Image>().sprite = redVersion;
        }
        for (int i = 0; i < teamDimension; i++)
            names.Add(PlayerPrefs.GetString("TEAM_" + teamName + "_" + i));
        CharactersLibrary clib = manager.GetComponent<CharactersLibrary>();
        foreach (string name in names) {
            CharacterInfo ci = clib.getCharacterInfoByName(name);
            GameObject generated = Instantiate(prefab_hero);
            generated.transform.SetParent(whereToGenerate.transform);
            generated.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            generated.GetComponent<ChButtonData>().initialize(ci, winningTeam);
            generated.GetComponent<RectTransform>().localScale = new Vector3(2, 2, 1);
        }
    }

    public void OnExitPressed() {
        SceneManager.LoadScene("ChooseCharacters", LoadSceneMode.Single);
    }

}
