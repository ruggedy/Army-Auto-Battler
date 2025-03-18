using System;
using System.Linq;
using ArmyGame.Units.Base.DataTypes;
using UnityEngine;

namespace ArmyGame.Units.Base.Appearance
{
    public class UnitAppearance : MonoBehaviour
    {
        public SpriteRenderer SpriteRenderer;

        [SerializeField] private SpriteSet[] SpriteSets;

        public void SetState(UnitHealthState state)
        {
            var newSprite = SpriteSets.First(spriteSet => spriteSet.unitHealthState == state).sprite;

            if (newSprite != null)
            {
                SpriteRenderer.sprite = newSprite;
            }
        }
    }
}