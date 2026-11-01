using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCU
{
    public class CameraSystem : MonoBehaviour
    {
        public static CameraSystem Instance { get; private set; } = null;

        //public bool ISTPSMode { get {return isTPSMode;} }

        /// <summary>
        /// 현재 카메라 시스템이 TPS 모드로 작동 중인지 확인하는 프로퍼티
        /// </summary>
        public bool IsTPSMode => isTPSMode; // private 변수를 외부에서 읽을 수 있도록 공개하기 위한 용도

        public float TargetFOV { get; set; } = 60.0f;

        public Cinemachine.CinemachineVirtualCamera tpsCamera;
        public Cinemachine.CinemachineVirtualCamera fpsCamera;

        public float zoomSpeed = 5.0f;

        private bool isTPSMode = true;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            isTPSMode = tpsCamera.gameObject.activeSelf; // TPS 모드 초기 활성화 상태에 맞춰 tpsCamera 상태 반영
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.T))
            {
                isTPSMode = !isTPSMode;
                tpsCamera.gameObject.SetActive(isTPSMode);
                fpsCamera.gameObject.SetActive(!isTPSMode);
                
                /*if (isTPSMode) // 이거랑 똑같은 기능
                {
                    tpsCamera.gameObject.SetActive(true);
                    tpsCamera.gameObject.SetActive(false);
                }
                else
                {
                    tpsCamera.gameObject.SetActive(false);
                    tpsCamera.gameObject.SetActive(true); 
                }*/
            }
        }


        private void LateUpdate()
        {
            tpsCamera.m_Lens.FieldOfView = Mathf.Lerp(tpsCamera.m_Lens.FieldOfView, TargetFOV, zoomSpeed * Time.deltaTime);
            fpsCamera.transform.forward = fpsCamera.Follow.transform.forward;
        }

    }
}