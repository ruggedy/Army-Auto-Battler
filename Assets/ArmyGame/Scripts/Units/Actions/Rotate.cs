using System;
using ArmyGame.Units.Base;
using UnityEngine;

namespace ArmyGame.Units.Actions
{
    public class Rotate : MonoBehaviour
    {
        private Transform target;
        [SerializeField] float rotationSpeed = 100f;

        private void Update()
        {
            if (target == null) return;

            var unit = transform.GetComponent<Unit>();

            var direction = (target.position - transform.position).normalized;

            // Rotate the unit
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            var step = rotationSpeed * Time.deltaTime;
            
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, angle), step);
            
            // set the target to null when rotation is complete.
            var forward = (unit.IsPlayerUnit() ? transform.right : -transform.right).normalized;
            var dot = Vector3.Dot(forward, direction);
            if (dot > 0.9f)
            {
                target = null;
            }

            
        }

        public void RotateTo(Transform newTarget)
        {
            target = newTarget;
        }
    }
}