using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCU
{
    public class UIManager : MonoBehaviour
    {
        // 싱글톤 인스턴스 (어디서나 UIManager에 쉽게 접근할 수 있도록 전역 변수 제공)
        public static UIManager Instance { get; private set; } = null;

        // 제네릭을 활용하여 원하는 UI를 화면에 띄우고 활성화하는 정적(Static) 메서드
        public static T Show<T>(UIList uiPrefabName) where T : UIBase
        {
            if (Instance == null)
                return null;

            T ui = Instance.GetUI<T>(uiPrefabName);
            if (ui == null)
                return null;

            ui.Show(); // UIBase에 정의된 Show 메서드 호출
            return ui;
        }

        // 제네릭을 활용하여 원하는 UI를 화면에서 숨기는 정적(Static) 메서드
        public static T Hide<T>(UIList uiPrefabName) where T : UIBase
        {
            if (Instance == null)
                return null;

            T ui = Instance.GetUI<T>(uiPrefabName);
            if (ui == null)
                return null;

            ui.Hide(); // UIBase에 정의된 Hide 메서드 호출
            return ui;
        }


        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        // 로드된 UI들을 관리하기 위한 컨테이너 딕셔너리 (UI 종류와 해당 UIBase 컴포넌트를 매핑)
        public Dictionary<UIList, UIBase> loadedUIContainer = new Dictionary<UIList, UIBase>();

        [SerializeField] private Transform panelRoot; // 일반 패널 UI들이 생성될 부모 Transform
        [SerializeField] private Transform popupRoot; // 팝업 UI들이 생성될 부모 Transform

        // UIManager 초기화 작업 (UI 루트 생성 및 열거형 기반 컨테이너 미리 채우기)
        public void Initialize()
        {
            // 패널 UI들의 부모 오브젝트가 없다면 동적으로 생성
            if (panelRoot == null)
            {
                GameObject goPanelRoot = new GameObject("Panel Root");
                goPanelRoot.transform.SetParent(transform);
                goPanelRoot.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                goPanelRoot.transform.localScale = Vector3.one;
                panelRoot = goPanelRoot.transform;
            }

            // 팝업 UI들의 부모 오브젝트가 없다면 동적으로 생성
            if (popupRoot == null)
            {
                GameObject goPopupRoot = new GameObject("Popup Root");
                goPopupRoot.transform.SetParent(transform);
                goPopupRoot.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                goPopupRoot.transform.localScale = Vector3.one;
                popupRoot = goPopupRoot.transform;
            }

            // SCENE_PANEL 범위에 해당하는 UI 목록을 딕셔너리에 null 값으로 초기 등록
            for (int index = (int)UIList.SCENE_PANEL_START + 1; index < (int)UIList.SCENE_PANEL_END; index++)
            {
                loadedUIContainer.Add((UIList)index, null);
            }

            // SCENE_POPUP 범위에 해당하는 UI 목록을 딕셔너리에 null 값으로 초기 등록
            for (int index = (int)UIList.SCENE_POPUP_START + 1; index < (int)UIList.SCENE_POPUP_END; index++)
            {
                loadedUIContainer.Add((UIList)index, null);
            }
        }

        // 특정 UI 프리팹을 리소스 폴더에서 불러오거나, 이미 로드된 경우 가져오는 핵심 메서드
        public T GetUI<T>(UIList uiPrefabName) where T : UIBase
        {
            // 1. 요청한 UI가 '패널(Panel)' 범위에 속해 있는 경우 처리
            if (UIList.SCENE_PANEL_START < uiPrefabName && uiPrefabName < UIList.SCENE_PANEL_END)
            {
                if (loadedUIContainer.ContainsKey(uiPrefabName))
                {
                    // 아직 메모리에 로드되지 않았다면(null이라면) 리소스에서 동적 로드
                    if (loadedUIContainer[uiPrefabName] == null)
                    {
                        string path = $"UI/Prefab/{uiPrefabName}";
                        UIBase loadedUI = Resources.Load<UIBase>(path);

                        if (loadedUI == null)
                            return null;

                        T result = loadedUI.GetComponent<T>();
                        if (result == null)
                            return null;

                        // 패널 루트(panelRoot) 하위에 인스턴스화(생성)하고 딕셔너리에 저장
                        loadedUIContainer[uiPrefabName] = Instantiate(loadedUI, panelRoot).GetComponent<T>();
                        if (loadedUIContainer[uiPrefabName])
                            loadedUIContainer[uiPrefabName].gameObject.SetActive(false); // 생성 직후에는 꺼두기

                        return loadedUIContainer[uiPrefabName].GetComponent<T>();
                    }
                    else
                    {
                        // 이미 로드되어 있다면 저장된 인스턴스 반환
                        return loadedUIContainer[uiPrefabName].GetComponent<T>();
                    }
                }
            }

            // 2. 요청한 UI가 '팝업(Popup)' 범위에 속해 있는 경우 처리
            if (UIList.SCENE_POPUP_START < uiPrefabName && uiPrefabName < UIList.SCENE_POPUP_END)
            {
                if (loadedUIContainer.ContainsKey(uiPrefabName))
                {
                    // 아직 메모리에 로드되지 않았다면(null이라면) 리소스에서 동적 로드
                    if (loadedUIContainer[uiPrefabName] == null)
                    {
                        string path = $"UI/Prefab/{uiPrefabName}";
                        UIBase loadedUI = Resources.Load<UIBase>(path);

                        if (loadedUI == null)
                            return null;

                        T result = loadedUI.GetComponent<T>();
                        if (result == null)
                            return null;

                        // 팝업 루트(popupRoot) 하위에 인스턴스화(생성)하고 딕셔너리에 저장
                        loadedUIContainer[uiPrefabName] = Instantiate(loadedUI, popupRoot).GetComponent<T>();
                        if (loadedUIContainer[uiPrefabName])
                            loadedUIContainer[uiPrefabName].gameObject.SetActive(false); // 생성 직후에는 꺼두기

                        return loadedUIContainer[uiPrefabName].GetComponent<T>();
                    }
                    else
                    {
                        // 이미 로드되어 있다면 저장된 인스턴스 반환
                        return loadedUIContainer[uiPrefabName].GetComponent<T>();
                    }
                }
            }

            return null;
        }
    }
}