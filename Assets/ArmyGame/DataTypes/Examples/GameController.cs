using System;
using UnityEngine;

namespace Logic.Examples
{
    public class GameController : MonoBehaviour
    {
        public GameItemSO item;

        private void OnTriggerEnter2D(Collider2D other)
        {
            var otherController = other.GetComponent<GameController>();
            
            var otherItem = otherController.item;
    
            if (otherItem.IsWinner(item))
            {
                Debug.Log("They win");
            }
            else
            {
                Debug.Log("They lose");
            }
        }
    }
}