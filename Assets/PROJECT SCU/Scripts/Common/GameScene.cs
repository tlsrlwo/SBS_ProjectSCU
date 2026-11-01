using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SCU
{
    public class GameScene : SceneBase
    {
        public override IEnumerator OnStartScene()
        {
            var loadingUI = UIManager.Instance.GetUI<LoadingUI>(UIList.LoadingUI);
            var asyncSceneLoad = SceneManager.LoadSceneAsync(SceneType.Game.ToString(), LoadSceneMode.Single); // Additive 대신 Single을 사용하여 기존 화면을 단일로 교체(밀어내기)
            yield return new WaitUntil(() =>
            {
                float sceneLoadProgress = asyncSceneLoad.progress / 0.9f;
                loadingUI.LoadingProgress = sceneLoadProgress;

                return asyncSceneLoad.isDone;
            }); 
        }

        public override IEnumerator OnEndScene()
        {
            yield return null;
        }
    }
}