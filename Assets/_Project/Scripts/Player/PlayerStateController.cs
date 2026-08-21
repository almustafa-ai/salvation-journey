using UnityEngine;

namespace SalvationJourney.Player
{
    public class PlayerStateController : MonoBehaviour
    {
        [Header("Core Resources")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float maxResolve = 100f;
        [SerializeField] private float maxStamina = 100f;

        [Header("Prayer")]
        [SerializeField] private float stillnessDuration = 6f;
        [SerializeField] private float fearResistanceBonus = 0.60f;

        public float Health { get; private set; }
        public float Resolve { get; private set; }
        public float Stamina { get; private set; }

        public bool IsUsingStillness { get; private set; }

        private float stillnessTimer;

        private void Awake()
        {
            Health = maxHealth;
            Resolve = maxResolve;
            Stamina = maxStamina;
        }

        private void Update()
        {
            UpdateStillness();
        }

        public void TakePhysicalDamage(float amount)
        {
            Health = Mathf.Clamp(Health - amount, 0f, maxHealth);

            if (Health <= 0f)
            {
                HandlePhysicalDefeat();
            }
        }

        public void TakeResolveDamage(float amount)
        {
            float finalDamage = amount;

            if (IsUsingStillness)
            {
                finalDamage *= 1f - fearResistanceBonus;
            }

            Resolve = Mathf.Clamp(
                Resolve - finalDamage,
                0f,
                maxResolve
            );

            if (Resolve <= 0f)
            {
                HandleBrokenResolve();
            }
        }

        public bool SpendStamina(float amount)
        {
            if (Stamina < amount)
                return false;

            Stamina -= amount;
            return true;
        }

        public void RestoreStamina(float amount)
        {
            Stamina = Mathf.Clamp(
                Stamina + amount,
                0f,
                maxStamina
            );
        }

        public void ActivateStillness()
        {
            if (IsUsingStillness)
                return;

            IsUsingStillness = true;
            stillnessTimer = stillnessDuration;

            Debug.Log(
                "Stillness activated: fear resistance increased."
            );
        }

        private void UpdateStillness()
        {
            if (!IsUsingStillness)
                return;

            stillnessTimer -= Time.deltaTime;

            if (stillnessTimer <= 0f)
            {
                IsUsingStillness = false;

                Debug.Log(
                    "Stillness ended."
                );
            }
        }

        private void HandlePhysicalDefeat()
        {
            Debug.Log(
                "Elias has fallen. Return to the latest refuge."
            );
        }

        private void HandleBrokenResolve()
        {
            Debug.Log(
                "Elias' Resolve has been broken."
            );
        }
    }
}
