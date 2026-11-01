using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SCU
{
    public enum SceneType
    {
        None = 0,
        Empty = 1,
        Title = 2,
        Game = 3,
    }

    public class Main : MonoBehaviour
    {
        public static Main Instance { get; private set; } = null;
        public SceneBase CurrentSceneController => currentScene;


        private SceneBase currentScene = null;

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴되지 않도록 설정 (DontDestroyOnLoad 적용)
        }

        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public void Initialize()
        {
            GameObject goUIManager = new GameObject("SCU.UIManager");
            UIManager uiManager = goUIManager.AddComponent<UIManager>();
            uiManager.Initialize();
            DontDestroyOnLoad(goUIManager);

            ChangeScene(SceneType.Title);
            // Main.Instance.ChangeScene(SceneType.SceneName) 형태로 어디서든 호출 가능
        }

        public void ChangeScene(SceneType sceneType)
        {
            IEnumerator sceneCoroutine = null;
            switch (sceneType)
            {
                case SceneType.Title:
                    sceneCoroutine = ChangeSceneAsync<TitleScene>(SceneType.Title);
                    break;
                case SceneType.Game:
                    sceneCoroutine = ChangeSceneAsync<GameScene>(SceneType.Game);
                    break;
            }
            StartCoroutine(sceneCoroutine);
        }

        private IEnumerator ChangeSceneAsync<T>(SceneType sceneType) where T : SceneBase
        {
            // To do : 로딩 UI 표시
            // var loadingUI = UIManager.Show<LoadingUI>(UIList.LoadingUI); // 이것은 로딩 상태일 때
            var loadingUI = UIManager.Show<LoadingUI>(UIList.LoadingUI);
            loadingUI.LoadingProgress = 0f;
            yield return null;

            // 만약 현재 씬이 비어있지 않다면, OnEndScene을 호출하고 파괴함
            if (currentScene != null)
            {
                yield return currentScene.OnEndScene();
                Destroy(currentScene.gameObject);
            }

            // 빈 씬(Empty Scene) 로드
            var async = SceneManager.LoadSceneAsync(SceneType.Empty.ToString(), LoadSceneMode.Single);
            yield return new WaitUntil(() => async.isDone);

            // 씬 게임 오브젝트를 생성하고 씬 컨트롤러 컴포넌트 추가
            GameObject sceneGo = new GameObject(typeof(T).Name);
            sceneGo.transform.parent = transform;
            currentScene = sceneGo.AddComponent<T>();

            // 씬 로드 및 초기화 진행
            yield return StartCoroutine(currentScene.OnStartScene());

            loadingUI.LoadingProgress = 1f;
            yield return null;

            // To do : 로딩 UI 숨기기
            UIManager.Hide<LoadingUI>(UIList.LoadingUI);
        }
    }
}