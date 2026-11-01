using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SCU
{
    public class TitleScene : SceneBase 
    {
        public override IEnumerator OnStartScene()
        {
            var loadingUI = UIManager.Instance.GetUI<LoadingUI>(UIList.LoadingUI);
            var asyncSceneLoad = SceneManager.LoadSceneAsync(SceneType.Title.ToString(), LoadSceneMode.Single);
            yield return new WaitUntil(() =>
            {
                float sceneLoadProgress = asyncSceneLoad.progress / 0.9f;
                loadingUI.LoadingProgress = sceneLoadProgress;

                return asyncSceneLoad.isDone;
            });

            UIManager.Show<TitleUI>(UIList.TitleUI); // TitleUI 프리팹 화면에 띄우기
        }

        public override IEnumerator OnEndScene()
        {
            yield return null;

            UIManager.Hide<TitleUI>(UIList.TitleUI); // 씬이 끝날 때 타이틀 UI 닫기(숨기기)
        }
    }
}