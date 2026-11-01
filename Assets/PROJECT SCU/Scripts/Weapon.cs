using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCU
{
    public class Weapon : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(firePosition.position, firePosition.position + firePosition.forward * range);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(Camera.main.transform.position, Camera.main.transform.position + Camera.main.transform.forward * range);
        }

        public GameObject muzzlePrefab;
        public Transform firePosition;
        public Transform bulletPrefab;

        public float fireRate = 0.1f;
        public float range = 100f;

        private float lastShootTime = 0f;
               

        public void Shoot()
        {
            if(Time.time > lastShootTime + fireRate)
            {
                // 사격 가능 (연사 쿨타임 체크 완료)

                lastShootTime = Time.time;

                // 총구 이펙트(Muzzle) 생성
                var newMuzzle = Instantiate(muzzlePrefab);
                newMuzzle.transform.SetPositionAndRotation(firePosition.position, firePosition.rotation);
                newMuzzle.gameObject.SetActive(true);
                Destroy(newMuzzle, 1f);

                // 총알(Bullet) 생성
                var newBullet = Instantiate(bulletPrefab);

                var cameraTransfrom = Camera.main.transform;
                newBullet.transform.SetPositionAndRotation(firePosition.position, firePosition.rotation); // cameraTransfrom.position, cameraTransfrom.rotation 등으로 바꿀 수 있음
                newBullet.gameObject.SetActive(true);

                // 총알 오브젝트가 플레이어와 충돌하지 않고 유니티 물리 엔진 관통이 정상 작동하도록 설정 (플레이어와 총알 간의 충돌 무시)
                Physics.IgnoreCollision(newBullet.GetComponent<Collider>(), transform.root.GetComponent<Collider>());
            }
        }
    }
}