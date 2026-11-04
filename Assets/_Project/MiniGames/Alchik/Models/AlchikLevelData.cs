using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace IntellectualGame.Alchik
{
    /// <summary>One throw: the fixed angle, and the distance of the target. Exactly one force 1-9 lands on it.</summary>
    [Serializable]
    public struct AlchikRound
    {
        public float angle;
        public float distance;
    }

    /// <summary>
    /// Model (config): the physics of a throw and how rounds are drawn. The player picks the force; range, flight time
    /// and height follow from simple projectile formulas, so the trajectory is the same on every device.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Alchik/Level", fileName = "AlchikLevel")]
    public class AlchikLevelData : ScriptableObject
    {
        [Tooltip("On: random angles and targets every run. Off: use the fixed list below.")]
        [SerializeField] bool randomize = true;
        [SerializeField, Min(1)] int throwCount = 5;
        [SerializeField] float[] angles = { 30f, 45f, 60f };
        [SerializeField, Range(1, 9)] int minForce = 3;
        [SerializeField, Range(1, 9)] int maxForce = 9;
        [SerializeField] float gravity = 9.8f;
        [Tooltip("Launch speed of force 1.")]
        [SerializeField] float speedPerForce = 1.25f;
        [Tooltip("A throw hits when it lands this close to the target center.")]
        [SerializeField] float hitRadius = 0.5f;
        [SerializeField] List<AlchikRound> rounds = new List<AlchikRound>();

        public float HitRadius => hitRadius;
        public int MaxAnswer => 9;

        public float Range(int force, float angle)
        {
            float speed = force * speedPerForce;
            return speed * speed * Mathf.Sin(2f * angle * Mathf.Deg2Rad) / gravity;
        }

        public float FlightTime(int force, float angle) =>
            2f * force * speedPerForce * Mathf.Sin(angle * Mathf.Deg2Rad) / gravity;

        public float Apex(int force, float angle)
        {
            float vertical = force * speedPerForce * Mathf.Sin(angle * Mathf.Deg2Rad);
            return vertical * vertical / (2f * gravity);
        }

        public bool IsHit(AlchikRound round, int force) => Mathf.Abs(Range(force, round.angle) - round.distance) <= hitRadius;

        public List<AlchikRound> CreateRun()
        {
            if (!randomize) return new List<AlchikRound>(rounds);

            var run = new List<AlchikRound>();
            for (int i = 0; i < throwCount; i++)
            {
                float angle = angles[Random.Range(0, angles.Length)];
                int force = Random.Range(minForce, maxForce + 1);
                run.Add(new AlchikRound { angle = angle, distance = Range(force, angle) });
            }
            return run;
        }
    }
}
