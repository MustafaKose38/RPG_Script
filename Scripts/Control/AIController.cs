using RPG.Combat;
using RPG.Core;
using RPG.Movement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RPG.Control
{
    public class AIController : MonoBehaviour
    {
        [SerializeField] float chaseDistance = 5f;//Takip Mesafesi
        [SerializeField] float suspicionTime = 3f;// suspicion=��phe.  Player�n takip mesafesinden ��kt�ktan sonraki bekleme zaman�.
        [SerializeField] PatrolPath patrolPath;
        [SerializeField] float wayPointTolerance = 1f;//Patern noktas�na yak�nl�k mesafe tolerans�
        [SerializeField] float wayPointDwellTime = 3f;//Patern Noktas�nda Bekleme S�resi

        Fighter fighter;
        Health health;
        Mover mover;
        GameObject player;

        Vector3 guardPosition;//Enemy nin patrol paterni yoksa bekledi�i/spawn oldu�u nokta
        float timeSinceLastSawPlayer = Mathf.Infinity;//Player� son g�rd���nden beri ge�en s�re
        int currentWayPointIndex = 0;
        float timeSinceArrivedAtPoint = Mathf.Infinity;//Noktaya Geli�inden Beri Ge�en S�re

        private void Start()
        {
            fighter = GetComponent<Fighter>();
            health = GetComponent<Health>();
            mover = GetComponent<Mover>();
            player = GameObject.FindWithTag("Player");

            guardPosition = transform.position; 
        }

        private void Update()
        {
            if (health.IsDead()) return;

            if (InAttackRangeOfPlayer() && fighter.CanAttack(player))
            {
                
                AttackBehaviour();
            }
            else if (timeSinceLastSawPlayer < suspicionTime)
            {
                SuspicionBehaviour();
            }
            else
            {
                PatrolBehaviour();

            }
            UpdateTimers();
        }

        private void UpdateTimers()
        {
            timeSinceLastSawPlayer += Time.deltaTime;
            timeSinceArrivedAtPoint += Time.deltaTime;
        }
        #region PatrolPath/Devriye paterni
        //��phe s�resi bitti�inde devriye paternine geri d�ner.
        private void PatrolBehaviour()
        {
            Vector3 nextPosition = guardPosition;
            if (patrolPath != null)
            {
                if (AtWayPoint())
                {
                    timeSinceArrivedAtPoint = 0;
                    CycleWayPoint();
                    
                }
                nextPosition = GetCurrentWayPoint();
            }
            if (timeSinceArrivedAtPoint > wayPointDwellTime)
            {
                mover.StartMoveAction(nextPosition);
            }
            
        }
        //Enemy devriye gezerken gitti�i noktaya yakla�t���nda(minimum wayPointTolerance kadar) true verir. aktif olan AtWayPoint() metodu, CycleWayPoint() metodunu �al��t�r�r. CycleWayPoint() metodu sonraki indeksin atamas�n� currentWayPointIndex de�i�kenine yapar. sonras�nda (nextPosition = GetCurrentWayPoint()) komutu �al���r. bu komut yeni indeksin pozisyon bilgisini nextPosition de�i�kenine atar. mover.StartMoveAction(nextPosition) komutu ise enemy nin s�radaki noktaya gitmesini sa�lar.
        private bool AtWayPoint()
        {
            float distanceToWayPoint = Vector3.Distance(transform.position, GetCurrentWayPoint());
            return distanceToWayPoint < wayPointTolerance;
        }
        //bir sonraki indeksi currentWayPointIndex de�i�kenine atar.
        private void CycleWayPoint()
        {
            currentWayPointIndex = patrolPath.GetNextIndex(currentWayPointIndex);
        }
        //ge�erli yol noktas�n� verir. yani indeksi girdi�imizde indexte bulunan noktay� verecektir.
        private Vector3 GetCurrentWayPoint()
        {
            return patrolPath.GetWayPoint(currentWayPointIndex);
        } 
        #endregion

        //player takip mesafesinden ��kt���nda mevcut eylemi iptal eder ve bir s�re(suspicionTime) kadar ��phelendi�i i�in bekler.
        private void SuspicionBehaviour()
        {
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }
        //Player takip mesafesine girdi�inde sald�r�y� aktif eder.
        private void AttackBehaviour()
        {
            timeSinceLastSawPlayer = 0;
            fighter.Attack(player);
        }

        private bool InAttackRangeOfPlayer()//player�n attack menzilinde olup olmad���n� boolean d�nderir.
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            return distanceToPlayer < chaseDistance;
        }

        
        /// <summary>
        /// Unity taraf�ndan Gizmos �izilmesi i�in �a�r�l�r. �rne�in enemy nin hedef kontrol alan�.
        /// Ayr�ca onDrawGizmos() metodu direk �izimi yapar. 
        /// birde OnDrawGizmosSelected() metodu var bu ise unity ekran�nda se�ili olan objenin gizmosunu �izer
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, chaseDistance);
        }
        
    }
}
