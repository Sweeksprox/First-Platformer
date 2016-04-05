using UnityEngine;
using System.Collections;

public class EnemyInfo {
        
        public float _health;
        public float _rage;
        public float _strength;
        
        public EnemyInfo(float health, float rage, float strength) {
                this._health = health;
                this._rage = rage;
                this._strength = strength;
        }
}
