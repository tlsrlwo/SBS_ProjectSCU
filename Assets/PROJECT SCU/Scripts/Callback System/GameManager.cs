using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCU
{
    public class GameManager : MonoBehaviour
    {
        public bool isGameOVer;

        public CharacterBase playerCharacter;

        private void Start()
        {
            playerCharacter.onCharacterDead += GameOverCheck;   // CharacterBase의 사망 이벤트 구독 등록
        }

        void GameOverCheck()
        {
            // to do : game over
            Debug.Log("Game Over");
        }
    }
}
