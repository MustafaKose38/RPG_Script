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
        [SerializeField] float suspicionTime = 3f;// suspicion=þüphe.  Playerýn takip mesafesinden çýktýktan sonraki bekleme zamaný.
        [SerializeField] PatrolPath patrolPath;
        [SerializeField] float wayPointTolerance = 1f;//Patern noktasýna yakýnlýk mesafe toleransý
        [SerializeField] float wayPointDwellTime = 3f;//Patern Noktasýnda Bekleme Süresi

        Fighter fighter;
        Health health;
        Mover mover;
        GameObject player;

        Vector3 guardPosition;//Enemy nin patrol paterni yoksa beklediði/spawn olduðu nokta
        float timeSinceLastSawPlayer = Mathf.Infinity;//Playerý son gördüðünden beri geçen süre
        int currentWayPointIndex = 0;
        float timeSinceArrivedAtPoint = Mathf.Infinity;//Noktaya Geliþinden Beri Geçen Süre

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
        //Þüphe süresi bittiðinde devriye paternine geri döner.
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
        //Enemy devriye gezerken gittiði noktaya yaklaþtýðýnda(minimum wayPointTolerance kadar) true verir. aktif olan AtWayPoint() metodu, CycleWayPoint() metodunu çalýþtýrýr. CycleWayPoint() metodu sonraki indeksin atamasýný currentWayPointIndex deðiþkenine yapar. sonrasýnda (nextPosition = GetCurrentWayPoint()) komutu çalýþýr. bu komut yeni indeksin pozisyon bilgisini nextPosition deðiþkenine atar. mover.StartMoveAction(nextPosition) komutu ise enemy nin sýradaki noktaya gitmesini saðlar.
        private bool AtWayPoint()
        {
            float distanceToWayPoint = Vector3.Distance(transform.position, GetCurrentWayPoint());
            return distanceToWayPoint < wayPointTolerance;
        }
        //bir sonraki indeksi currentWayPointIndex deðiþkenine atar.
        private void CycleWayPoint()
        {
            currentWayPointIndex = patrolPath.GetNextIndex(currentWayPointIndex);
        }
        //geçerli yol noktasýný verir. yani indeksi girdiðimizde indexte bulunan noktayý verecektir.
        private Vector3 GetCurrentWayPoint()
        {
            return patrolPath.GetWayPoint(currentWayPointIndex);
        } 
        #endregion

        //player takip mesafesinden çýktýðýnda mevcut eylemi iptal eder ve bir süre(suspicionTime) kadar þüphelendiði için bekler.
        private void SuspicionBehaviour()
        {
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }
        //Player takip mesafesine girdiðinde saldýrýyý aktif eder.
        private void AttackBehaviour()
        {
            timeSinceLastSawPlayer = 0;
            fighter.Attack(player);
        }

        private bool InAttackRangeOfPlayer()//playerýn attack menzilinde olup olmadýðýný boolean dönderir.
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            return distanceToPlayer < chaseDistance;
        }

        
        /// <summary>
        /// Unity tarafýndan Gizmos çizilmesi için çaðrýlýr. örneðin enemy nin hedef kontrol alaný.
        /// Ayrýca onDrawGizmos() metodu direk çizimi yapar. 
        /// birde OnDrawGizmosSelected() metodu var bu ise unity ekranýnda seçili olan objenin gizmosunu çizer
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, chaseDistance);
        }
        
    }
}
