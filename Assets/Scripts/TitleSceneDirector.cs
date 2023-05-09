using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneDirector : MonoBehaviour
{
    // ゲーム全体のプレイヤー数
    static public int PlayerCount;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void PvP()
    {
        PlayerCount = 2;
        SceneManager.LoadScene("GameScene");
    }

    public void PvE()
    {
        PlayerCount = 1;
        SceneManager.LoadScene("GameScene");
    }

    public void Story()
    {
        SceneManager.LoadScene("StoryScene");
    }
}