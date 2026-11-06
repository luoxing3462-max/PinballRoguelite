using System;
using System.Collections.Generic;
using UnityEngine;

namespace PinballRoguelite
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public sealed class Marble : MonoBehaviour
    {
        public event Action<Marble, int> BumperHit;
        public event Action<Marble> SplitRequested;
        public event Action<Marble, float> Settled;

        public string Kind { get; private set; } = "normal";
        public MarbleDefinition Definition { get; private set; }
        public bool CanSplit { get; private set; }
        public bool IsActive { get; private set; } = true;
        public bool LastSettlementWasSpecial { get; private set; }
        public Rigidbody2D Body { get; private set; }

        private const float MaximumLifetime = 14f;
        private const float MaximumLinearSpeed = 26f;
        private const float MaximumBounciness = 0.82f;
        private const float StuckSpeed = 0.08f;
        private const float StuckTime = 1.25f;
        private readonly HashSet<int> scoredBumpers = new HashSet<int>();
        private CircleCollider2D marbleCollider;
        private SpriteRenderer visual;
        private bool splitUsed;
        private int bumperHitCount;
        private int remainingPierces;
        private float collisionAttackMultiplier = 1f;
        private Vector2 velocityBeforePhysicsStep;
        private float lifetime;
        private float lowSpeedTime;

        private void Awake()
        {
            Body = GetComponent<Rigidbody2D>();
            Body.gravityScale = 1.1f;
            Body.mass = 1f;
            Body.linearDamping = 0.08f;
            Body.angularDamping = 0.2f;
            Body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            Body.interpolation = RigidbodyInterpolation2D.Interpolate;
            Body.sleepMode = RigidbodySleepMode2D.NeverSleep;

            marbleCollider = GetComponent<CircleCollider2D>();
            marbleCollider.radius = 0.19f;
            marbleCollider.sharedMaterial = new PhysicsMaterial2D("MarbleMaterial")
            {
                bounciness = 0.64f,
                friction = 0.04f
            };
            visual = GetComponentInChildren<SpriteRenderer>();
            if (visual == null)
                visual = SpriteFactory.AddVisual(transform, SpriteFactory.Circle, GamePalette.Cyan,
                    Vector2.one * 0.38f, 5);
        }

        public void Configure(string kind, Vector2 initialImpulse, bool canSplit, float childCollisionMultiplier = 1f)
        {
            Definition = MarbleCatalog.Get(kind);
            Kind = Definition.Id;
            CanSplit = canSplit && Definition.CanSplit;
            scoredBumpers.Clear();
            splitUsed = false;
            bumperHitCount = 0;
            remainingPierces = Definition.PierceNormalBumperCount;
            collisionAttackMultiplier = Mathf.Clamp01(childCollisionMultiplier);
            IsActive = true;
            lifetime = 0f;
            lowSpeedTime = 0f;
            LastSettlementWasSpecial = false;
            visual.color = Definition.Color;
            Body.gravityScale = 1.1f * Definition.GravityScaleMultiplier;
            marbleCollider.sharedMaterial = new PhysicsMaterial2D("MarbleMaterial_" + Definition.Id)
            {
                bounciness = Mathf.Min(Definition.Bounciness, MaximumBounciness),
                friction = 0.04f
            };
            Body.linearVelocity = Vector2.zero;
            Body.angularVelocity = 0f;
            Body.AddForce(initialImpulse, ForceMode2D.Impulse);
            gameObject.name = canSplit ? "SplitMarble" : "Marble";
        }

        private void FixedUpdate()
        {
            if (!IsActive) return;
            ClampExcessLinearSpeed();
            velocityBeforePhysicsStep = Body.linearVelocity;
            lifetime += Time.fixedDeltaTime;
            if (transform.position.y < -6.2f || lifetime > MaximumLifetime)
            {
                Settle(1);
                return;
            }

            if (Body.linearVelocity.magnitude < StuckSpeed) lowSpeedTime += Time.fixedDeltaTime;
            else lowSpeedTime = 0f;

            if (lowSpeedTime >= StuckTime)
            {
                Settle(1f);
            }
        }

        private void ClampExcessLinearSpeed()
        {
            var velocity = Body.linearVelocity;
            if (velocity.sqrMagnitude <= MaximumLinearSpeed * MaximumLinearSpeed) return;
            Body.linearVelocity = velocity.normalized * MaximumLinearSpeed;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!IsActive) return;
            var bumper = collision.collider.GetComponent<Bumper2D>();
            if (bumper == null) bumper = collision.otherCollider.GetComponent<Bumper2D>();
            if (bumper == null) return;

            if (!scoredBumpers.Add(bumper.GetInstanceID())) return;
            bumperHitCount++;
            BumperHit?.Invoke(this, bumper.Points);

            if (remainingPierces > 0 && !bumper.SplitTrigger)
            {
                remainingPierces--;
                Body.linearVelocity = velocityBeforePhysicsStep;
            }

        }

        public bool RequestSplit()
        {
            if (!CanSplit || splitUsed || !IsActive) return false;
            splitUsed = true;
            SplitRequested?.Invoke(this);
            return true;
        }

        public float GetCollisionAttackMultiplier()
        {
            return Definition.GetCollisionMultiplier(bumperHitCount, collisionAttackMultiplier);
        }

        public void Settle(float multiplier, bool specialExit = false)
        {
            if (!IsActive) return;
            IsActive = false;
            marbleCollider.enabled = false;
            Body.simulated = false;
            visual.enabled = false;
            LastSettlementWasSpecial = specialExit;
            Settled?.Invoke(this, Mathf.Max(0f, multiplier));
            Destroy(gameObject, 0.05f);
        }

        public void ConsumeForSplit()
        {
            if (!IsActive) return;
            IsActive = false;
            marbleCollider.enabled = false;
            Body.simulated = false;
            visual.enabled = false;
            Destroy(gameObject, 0.02f);
        }
    }
}
