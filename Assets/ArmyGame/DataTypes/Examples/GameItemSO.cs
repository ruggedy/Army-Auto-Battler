using UnityEngine;

namespace Logic.Examples
{
    [CreateAssetMenu(fileName = "GameItem", menuName = "Tests/GameItem", order = 0)]
    public class GameItemSO : ScriptableObject
    {
        public GameItemSO weakness;
        
        public bool IsWinner(GameItemSO other) => other.weakness.Equals(weakness);
    }
}