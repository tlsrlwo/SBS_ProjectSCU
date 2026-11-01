using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCU
{
    public class LocomotionBehaviour : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
           
        }

        // 만약 이게 PlayerController의 Update 함수에서 계속 작동하면 (Move 함수 등) 코드가 충돌하거나 문제가 생길 수 있음

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
           
        }

    }
}

