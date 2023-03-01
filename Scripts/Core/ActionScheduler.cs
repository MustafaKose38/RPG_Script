using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Core
{
    public class ActionScheduler : MonoBehaviour
    { // EylemPlanlay�c�

        IAction currentAction;//Mevcut eylemi Tutaca��z


        public void StartAction(IAction action)
        {
            if (currentAction == action) return;
            if (currentAction != null)
            {
                currentAction.Cancel();
            }
            currentAction = action;
        }

        //Mevcut eylemi iptal edecek.
        public void CancelCurrentAction()
        {
            StartAction(null);
        }
    } 
}
