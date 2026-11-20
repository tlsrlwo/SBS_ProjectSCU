using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Animations.Rigging 패키지가 안될 때를 대비해 public Rig

namespace SCU
{
    public class PlayerController : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(aimTarget.position, 0.1f);
        }


        public bool IsEnableMovement
        {
            set => isEnableMovement = value;
        }
        // get, set 키워드가 붙는 접근자 프로퍼티(Properties) => 외부에서 값 변경을 안전하게 제어하기 위함

        // 필드 (캐릭터 설정 관련 필드들)
        [Header("Character Setting")]
        public float moveSpeed = 3.0f;
        public float sprintSpeed = 5.0f;
        public float speedChangeRate = 10.0f;

        [Header("Camera Setting")]
        public float cameraHorizontalSpeed = 2.0f;
        public float cameraVerticalSpeed = 2.0f;

        [Range(0.0f, 0.3f)] public float rotationSmoothTime = 0.12f;

        [Header("Sliding Setting")]
        public AnimationCurve slidingCurve;

        [Header("Weapon Holder")]
        public GameObject weaponHolder;

        [Header("Weapon FOV")]
        public float defaultFOV;
        public float aimFOV;

        [Header("Camera Clamping")]
        public float topClamp = 70.0f;
        public float bottomClamp = -30.0f;
        public GameObject cinemachineCameraTarget;
        public float cameraAngleOverride = 0.0f;

        [Header("Animation Rigging")]
        public Transform aimTarget;
        public LayerMask aimingLayer;
        public UnityEngine.Animations.Rigging.Rig aimingRig; // 애니메이션 리그 컴포넌트

        public float aimingIKBlendCurrent;
        public float aimingIKBlendTarget;

        private Animator animator;
        private Camera mainCamera;
        private CharacterController controller;
        private InteractionSensor interactionSensor;
        private Weapon currentWeapon;
        private AnimationEventListener animationEventListener;

        private bool isSprint = false;
        private Vector2 move;
        private float speed;
        private float animationBlend;
        private float targetRotation = 0.0f;
        private float rotationVelocity;
        private float verticalVelocity;

        private Vector2 look;
        private const float _threshold = 0.01f;
        private float cinemachineTargetYaw;
        private float cinemachineTargetPitch;

        private bool isEnableMovement = true;
        private bool isStrafe;
        private bool isReloading = false; // 재장전 중인지 여부 확인 플래그

        private void Awake()
        {
            // GetComponent = 자기 자신 컴포넌트 가져오기 / GetComponentInChildren => 자기 자신 및 한 단계 아래 자식 컴포넌트 탐색
            animator = GetComponentInChildren<Animator>();
            controller = GetComponent<CharacterController>();
            mainCamera = Camera.main;
            interactionSensor = GetComponentInChildren<InteractionSensor>();
            animationEventListener = GetComponentInChildren<AnimationEventListener>();
            animationEventListener.OnTakenAnimationEvent += OnTakenAnimationEvent;

            var weaponGameObject = TransformUtility.FindGameObjectWithTag(weaponHolder, "Weapon");
            currentWeapon = weaponGameObject.GetComponent<Weapon>();
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;

            Cursor.visible = false;
        }

        private void OnEnable()
        {
            interactionSensor.OnDetected += OnDetectedInteraction;
            interactionSensor.OnLost += OnLostInteraction;
        }
        private void OnDetectedInteraction(IInteractable interactable)
        {
            InteractionUI.Instance.AddInteractionData(interactable);
        }


        private void OnLostInteraction(IInteractable interactable)
        {
            InteractionUI.Instance.RemoveInteractionData(interactable);
        }


        private void Update()
        {


            if (Input.GetKeyDown(KeyCode.F))
            {
                InteractionUI.Instance.DoInteract();
            }
            if(Input.GetKeyDown(KeyCode.DownArrow))
            {
                InteractionUI.Instance.SelectNext();
            }
            if(Input.GetKeyDown(KeyCode.UpArrow))
            {
                InteractionUI.Instance.SelectPrev();
            }

            if(Input.GetKeyDown(KeyCode.Space))
            {
                animator.SetTrigger("Trigger_Slide");
            }

            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            move = new Vector2(horizontal, vertical);

           /* if(interactionSensor.HasInteractable)
            {
                move = Vector2.zero;
            }
           */

            float hMouse = Input.GetAxis("Mouse X");
            float vMouse = Input.GetAxis("Mouse Y") * -1; // -1을 곱하는 이유는 마우스 상하 반전을 적용하기 위함 (마우스 아래로 내리면 시점이 위로)
            look = new Vector2(hMouse, vMouse);

            isSprint = Input.GetKey(KeyCode.LeftShift); // 달리기 키 입력 상태 체크 및 달리기 애니메이션 속도 조절 반영 용도
            if(isSprint)
            {
                animator.SetFloat("MotionSpeed", 1.25f);
            }    
            else
            {
                animator.SetFloat("MotionSpeed", 1f);
            }
            isStrafe = Input.GetKey(KeyCode.Mouse1); // 마우스 우클릭 (조준 및 스트레이프 이동)
           
            if(isStrafe) // 캐릭터가 조준 또는 우클릭(스트레이프) 중일 때 카메라의 전방 방향으로 캐릭터 회전 고정
            {
                Vector3 cameraForward = Camera.main.transform.forward.normalized;
                cameraForward.y = 0;
                transform.forward = cameraForward;                    
            }

            if (Input.GetKey(KeyCode.Mouse0)) // 마우스 좌클릭 (공격 / 사격)
            {
                /*if(currentWeapon != null)
                {
                currentWeapon.Shoot();
                }*/
                currentWeapon?.Shoot();
                // currentWeapon?.Shoot(); // 널(Null) 체크를 안전하게 해주는 표현식, 중복 호출되지 않도록 주의

                var cameraForward = Camera.main.transform.forward.normalized;
                cameraForward.y = 0;
                transform.forward = cameraForward;
            }

            if(Input.GetKeyDown(KeyCode.Mouse1)) // 마우스 우클릭 (조준 및 스트레이프와 중복 입력 처리)
            {
                // 줌인 (시야각 축소)
                CameraSystem.Instance.TargetFOV = aimFOV;
                aimingIKBlendTarget = isReloading ? 0f : 1f; // 재장전 중이면 IK를 올리지 않고, 아니면 IK를 올려서 조준 보정
            }
            if(Input.GetKeyUp(KeyCode.Mouse1))
            {
                // 줌아웃 (기본 시야각 복원)
                CameraSystem.Instance.TargetFOV = defaultFOV;
                aimingIKBlendTarget = 0f;
            }

            // 캐릭터 장전(Reload) 입력 처리
            if(Input.GetKeyDown(KeyCode.R))
            {
                animator.SetLayerWeight(1, 1f); // 상체 애니메이션 레이어 1번(BaseLayer는 0번)의 가중치(Weight)를 1로 활성화
                animator.SetTrigger("Trigger_Reload");
                isReloading = true;
            }


            aimingIKBlendCurrent = Mathf.Lerp(aimingIKBlendCurrent, aimingIKBlendTarget, Time.deltaTime * 10f);
            aimingRig.weight = aimingIKBlendCurrent;

            animator.SetBool("IsFPSMode", !CameraSystem.Instance.IsTPSMode); // 1인칭/3인칭 모드 분기 처리
            if (!CameraSystem.Instance.IsTPSMode)
            {
                Vector3 cameraForward = Camera.main.transform.forward;

                float aimRight = Vector3.Dot(transform.right, cameraForward);
                float aimUp = Vector3.Dot(transform.up, cameraForward);

                animator.SetFloat("AimRight", aimRight);
                animator.SetFloat("AimUp", aimUp);
            }

            /*if(CameraSystem.Instance.IsTPSMode == false)
            {
                Vector3 fpsForward = mainCamera.transform.forward;
                fpsForward.y = 0;
                transform.forward = fpsForward;
            }*/ // 정상 작동하지 않아 주석 처리함

            Move();

            animator.SetFloat("Speed", animationBlend);
            animator.SetFloat("Horizontal", move.x);
            animator.SetFloat("Vertical", move.y);
            animator.SetFloat("Strafe", isStrafe ? 1 : 0);

            // 카메라 ViewportPointToRay를 이용하여 화면 중앙을 바라보는 Ray를 생성
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            // Raycast를 이용하여 화면 중앙이 바라보는 Ray의 충돌 지점의 위치를 계산
            // Vector3 aimingTargetPosition = Vector3.zero;
            Vector3 aimingTargetPosition = Camera.main.transform.position + Camera.main.transform.forward * 1000f;

            // Raycast에 성공하면 aimingTargetPosition을 Raycast가 부딪힌 지점의 좌표로 갱신
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 1000f, aimingLayer))
            {
                if(hitInfo.transform.root != transform) // Raycast가 자기 자신(Player Root)에 부딪힌 것이 아닐 때만 타겟 위치 설정
                {
                    aimingTargetPosition = hitInfo.point;
                }
            }
            else // Raycast에 실패하면 카메라 전방 방향으로 1000m 떨어진 가상의 지점을 타겟으로 설정
            {
            
            }

            // 계산된 aimingTargetPosition을 aimTarget의 위치에 반영
            aimTarget.position = aimingTargetPosition;
        }

        private void LateUpdate()
        {
            CameraRotation(); // 카메라는 연산 타이밍 문제로 LateUpdate에서 처리하는 것이 부드러움
        }

        private void CameraRotation()
        {
            // 마우스 입력값이 존재하고 카메라 회전이 고정되어 있지 않은 경우
            if (look.sqrMagnitude >= _threshold)
            {
                // 마우스 입력값에는 Time.deltaTime을 곱하지 않음 (프레임 독립성 유지를 위해)
                float deltaTimeMultiplier = 1.0f;

                cinemachineTargetYaw += look.x * deltaTimeMultiplier * cameraHorizontalSpeed;
                cinemachineTargetPitch += look.y * deltaTimeMultiplier * cameraVerticalSpeed;
            }

            // 카메라 회전 각도 제한 (Clamp) 설정
            cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
            cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, bottomClamp, topClamp);

            // 시네마신(Cinemachine) 가상 카메라가 추적할 타겟의 회전값 업데이트
            cinemachineCameraTarget.transform.rotation = Quaternion.Euler(cinemachineTargetPitch + cameraAngleOverride, cinemachineTargetYaw, 0.0f);
        }

#region 이동
        private void Move()
        {
            // 움직임이 가능한지 체크 
            if (!isEnableMovement)
                return;

            // 이동 속도와 달리기 속도 설정을 바탕으로 목표 속도(Target Speed) 결정, isSprint 이면 sprintSpeed, !isSprint 이면 moveSpeed;
            float targetSpeed = isSprint ? sprintSpeed : moveSpeed;

            // 단순화된 가속 및 감속 로직 (수정이 용이하도록 설계됨)

            // 참고: Vector2의 == 연산자는 근사값을 사용하므로 부동 소수점 오차에 안전하며 magnitude보다 연산 비용이 저렴함
            // 이동 입력이 없다면 목표 속도를 0으로 설정
            if (move == Vector2.zero) targetSpeed = 0.0f;

            // 플레이어의 현재 수평 속도의 길이(magnitude)값
            float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;

            // 현재 속도와 목표 속도가 거의 비슷해졌을 때 불필요하게 보간을 반복하지 않도록 만든 허용 오차
            float speedOffset = 0.1f;

            // 이동방향값을 inputMagnitude 변수에 저장
            float inputMagnitude = move.magnitude;

            // 목표 속도로 가속 또는 감속 처리 
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // 선형 보간 대신 곡선 형태의 결과를 만들어 더욱 자연스러운 속도 변화를 제공
                // 참고: Lerp의 T 값은 내부적으로 클램프되므로 수동으로 속도를 클램프할 필요가 없음
                speed = Mathf.Lerp(
                    currentHorizontalSpeed,                     // 현재 수평 이동 속도
                    targetSpeed * inputMagnitude,               // 목표 속도에 이동입력값을 곱하면 입력의 크기에 따라 실제 목표 속도가 달라짐
                    Time.deltaTime * speedChangeRate            // 보간값(t)을 고정하면 컴퓨터의 성능(FPS)에 따라 보간 속도가 달라질 수 있다. 
                                                                // 따라서 Time.deltaTime과 speedChangeRate를 곱하여 프레임 간 경과 시간에 비례하도록 
                                                                // 보간 비율을 조정함으로써, 서로 다른 FPS 환경에서도 일관된 보간 동작을 구현한다
                    );
                // 속도 보간을 수행하는 코드

                // 소수점 셋째 자리까지 반올림
                speed = Mathf.Round(speed * 1000f) / 1000f;
            }
            else
            {
                speed = targetSpeed;
            }

            // animationBlend = 실제 플레이어의 이동 속도와 애니메이션에 전달할 속도를 분리하기 위해 선언한 변수, 
            // 실제 이동은 서서히 증가하는데 애니메이션은 빠르게 변화 혹은 실제 이동은 빠르게 가는데 애니메이션은 비교적 느리게 변화하는 것 처럼 분리가 가능해짐
            animationBlend = Mathf.Lerp(
                animationBlend,
                targetSpeed,
                Time.deltaTime * speedChangeRate);            

            // animationBlend 가 0.01 보다 작아지면서 Animator의 Speed 파라미터에 아주 작은 값이 계속 전달되는 것을 방지
            if (animationBlend < 0.01f) animationBlend = 0f;

            // 입력 방향 정규화 (Normalize)
            Vector3 inputDirection = new Vector3(move.x, 0.0f, move.y).normalized;

            // 참고: Vector2의 != 연산자는 근사값 오차에 안전하며 magnitude보다 저렴함
            // 이동 입력이 존재할 경우 플레이어를 이동 방향으로 회전시킴
            if (move != Vector2.zero)
            {
                targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                      mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity,
                    rotationSmoothTime);

                if (!isStrafe)
                {
                    // 카메라 위치를 기준으로 입력된 방향을 바라보도록 플레이어 회전 적용
                    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                }
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;

            // CharacterController를 이용한 최종 플레이어 이동 처리
            controller.Move(targetDirection.normalized * (speed * Time.deltaTime) +
                             new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);
        }
#endregion 

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        public void OnTakenAnimationEvent(string eventName)
        {
            if (eventName.Equals("Reload"))
            {
                isReloading = false;
                animator.SetLayerWeight(1, 0f); // 장전 동작이 끝나면 상체 레이어 가중치를 다시 0으로 내려서 기본 애니메이션과 블렌딩되도록 처리
            }
        }
    }
}