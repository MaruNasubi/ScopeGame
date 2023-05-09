using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class skillbutton : MonoBehaviour
{
    public GameSceneDirector sceneDirector;

    public GameObject SpecialButton;
    public GameObject UsuallyButton;
    public GameObject NinjutuButton;

    public void OnClickUsuallyButton()
    {
        sceneDirector.selectUnit.isUsually = true;
        sceneDirector.selectUnit.isSpecial = false;
        sceneDirector.selectUnit.isNinjutu = false;
    }
    public void OnClickSpecialButton()
    {
        sceneDirector.selectUnit.isUsually = false;
        sceneDirector.selectUnit.isSpecial = true;
        sceneDirector.selectUnit.isNinjutu = false;
    }
    public void OnClickNinjutuButton()
    {
        sceneDirector.selectUnit.isUsually = false;
        sceneDirector.selectUnit.isSpecial = false;
        sceneDirector.selectUnit.isNinjutu = true;
    }


    // Update is called once per frame
    void Update()
    {
        if (sceneDirector.selectUnit == false)
            return;
        SpecialButton.SetActive(sceneDirector.selectUnit.Type == UnitController.TYPE.KAIZOKU);
        NinjutuButton.SetActive(sceneDirector.selectUnit.Type == UnitController.TYPE.SHINOBI);
    }
}