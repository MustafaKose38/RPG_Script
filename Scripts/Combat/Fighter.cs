using RPG.Core;
using RPG.Movement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Combat
{
    public class Fighter : MonoBehaviour, IAction
    {
        [SerializeField]
        [Range(1f, 10f)]
        float weaponRange = 1f;

        [SerializeField] float timeBetweenAttack = 1f;
        float timeSinceLastAttack = Mathf.Infinity;

        [SerializeField] float weaponDamage = 5f;

        Health target;
        private void Update()
        {
            timeSinceLastAttack += Time.deltaTime;

            if (target == null) return;
            if (target.IsDead()) return;//hedefin �l�p �lmedi�ini kontrol ediyoruz.
            if (!Get�sInRange())
            {
                GetComponent<Mover>().MoveTo(target.transform.position);
            }
            else
            {
                GetComponent<Mover>().Cancel();
                AttackBehavior();
            }
        }

        //CoolDown ile Attack animator� aktifle�tirme
        private void AttackBehavior()
        {
            transform.LookAt(target.transform);
            if (timeSinceLastAttack >= timeBetweenAttack)
            {
                //Bu, Hit() methodunu tetikleyecek.
                TriggerAttack();
                timeSinceLastAttack = 0;

            }

        }

        private void TriggerAttack()
        {
            GetComponent<Animator>().ResetTrigger("stopAttack");
            GetComponent<Animator>().SetTrigger("Attack");
        }

        //Animation �al��t�r
        void Hit()
        {
            if (target == null) return; 
            //hedefe sald�rd���nda damage almas�n� sa�l�yoruz.
            target.TakeDamage(weaponDamage);
        }

        //hedefin menzil i�inde olup olmad���n� hesaplar, sonucu boolean d�nderir.
        private bool Get�sInRange()
        {
            return Vector3.Distance(transform.position, target.transform.position) < weaponRange;
        }


        //hedef bo� de�ilse, health componenti var ve �l� de�ilse attack yap�labilir.
        public bool CanAttack(GameObject combatTarget)
        {
            if (combatTarget == null) { return false; }
            Health targetToTest = combatTarget.GetComponent<Health>();
            return targetToTest != null && !targetToTest.IsDead();
        }

        public void Attack(GameObject combatTarget)
        {
            GetComponent<ActionScheduler>().StartAction(this);
            target = combatTarget.GetComponent<Health>();
        }
        public void Cancel()
        {
            StopAttack();
            target = null;
        }

        private void StopAttack()
        {
            GetComponent<Animator>().ResetTrigger("Attack");
            GetComponent<Animator>().SetTrigger("stopAttack");
        }
    }
}
