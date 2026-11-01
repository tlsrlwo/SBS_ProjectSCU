using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCU
{
    public class CharacterBase : MonoBehaviour
    {
        public float currentHP;
        public float maxHP;

        public delegate void OnDamage(float currentHP, float maxHP);
        public delegate void OnCharacterDead();

        // public event OnDamage onDamageCallbackEvent; // event 키워드를 붙이면 외부에서 직접 호출할 수 없고, += 와 -= 연산만 가능해짐 (안전성 확보)
        public OnDamage onDamageCallback;                    // 데미지 입었을 때 체력 상태를 갱신하기 위한 델리게이트
        public OnCharacterDead onCharacterDead;

        public System.Action<float, float> onDamagedAction;


        public void Damage(float damage)
        {
            currentHP -= damage;

            onDamageCallback(currentHP, maxHP);
            onDamagedAction(currentHP, maxHP);

            if(currentHP <= 0)
            {
                onCharacterDead();      // 캐릭터가 죽었음을 알리는 이벤트 체인을 호출하여 구독자들에게 알림
                Destroy(gameObject);
            }
        }

        [ContextMenu("Damage Debug")]
        public void DamageDebugButton()
        {
            Damage(20);
        }
     
    }
}