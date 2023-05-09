using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class CheckTimelineEnd : MonoBehaviour
{

	private PlayableDirector playableDirector;
	//　終了を検知したかどうか
	private bool isEnd;

	// Use this for initialization
	void Start()
	{
		playableDirector = GetComponent<PlayableDirector>();
		isEnd = false;
	}

	// Update is called once per frame
	void Update()
	{
		//　タイムラインが終了したら次のシーンを読み込む
		if (!isEnd && playableDirector.state != PlayState.Playing)
		{
			isEnd = true;
			StartCoroutine(LoadTitleScene());
		}
	}
	//　次のシーンの読み込み
	IEnumerator LoadTitleScene()
	{
		AsyncOperation async = SceneManager.LoadSceneAsync("TitleScene");

		while (true)
		{
			if (!async.isDone)
			{
				yield return null;
			}
		}
	}
}
