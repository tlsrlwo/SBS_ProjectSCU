using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCU
{
    public class AnimatorSyncTransform : MonoBehaviour
    {
        private Animator animator;
        private PlayerController playerController;
        private float slidingTime = 0f;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            playerController = GetComponentInParent<PlayerController>();
        }

        private void OnAnimatorMove() // 이 함수를 정의하면 인스펙터 창의 애니메이터 컴포넌트에서 Root Motion 체크박스의 표시 상태가 변경됨
        {

            bool isSliding = animator.GetCurrentAnimatorStateInfo(0).IsName("Sliding");
            if (isSliding)
            {
                slidingTime += Time.deltaTime;
                float curveValue = playerController.slidingCurve.Evaluate(slidingTime /2f);
                transform.root.position = animator.rootPosition + animator.deltaPosition * curveValue;
            }
            else
            {
                slidingTime = 0f;
                transform.root.position = animator.rootPosition;
                transform.root.rotation = animator.rootRotation;

            }
        }
    }
}
