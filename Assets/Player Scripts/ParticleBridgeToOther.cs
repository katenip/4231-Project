using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleBridgeToOther : MonoBehaviour
{
    [Header("Auto Link")]
    public ParticleBridgeToOther partner;

    [Header("References")]
    public ParticleSystem ps;
    public Transform otherCenter;

    [Header("Transfer Timing")]
    [Range(0f, 1f)] public float lastLifePercent = 0.18f;
    public float distanceStartsEarlierMultiplier = 0.01f;
    [Range(0f, 1f)] public float maxPullWindow = 0.45f;

    [Header("How many transfer")]
    [Range(0f, 1f)] public float travelChance = 0.2f;

    [Header("Movement")]
    public float pullStrength = 2.5f;
    public float maxTravelSpeed = 2.5f;
    public float distanceSpeedMultiplier = 0.15f;
    public float maxDistanceSpeedBoost = 3f;
    [Range(0f, 1f)] public float keepSomeOriginalVelocity = 0.8f;

    private ParticleSystem.Particle[] particles;

    void Reset()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void Awake()
    {
        if (ps == null)
            ps = GetComponent<ParticleSystem>();

        AutoResolvePartner();
    }

    void OnValidate()
    {
        if (ps == null)
            ps = GetComponent<ParticleSystem>();

        if (partner != null)
            otherCenter = partner.transform;
    }

    void AutoResolvePartner()
    {
        if (partner != null)
        {
            otherCenter = partner.transform;
            return;
        }

        ParticleBridgeToOther[] all = FindObjectsByType<ParticleBridgeToOther>(FindObjectsSortMode.None);

        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != this)
            {
                partner = all[i];
                otherCenter = partner.transform;

                if (partner.partner == null)
                {
                    partner.partner = this;
                    partner.otherCenter = transform;
                }

                break;
            }
        }
    }

    void LateUpdate()
    {
        if (ps == null) return;

        if (otherCenter == null && partner != null)
            otherCenter = partner.transform;

        if (otherCenter == null) return;

        int count = ps.particleCount;
        if (count <= 0) return;

        if (particles == null || particles.Length < count)
            particles = new ParticleSystem.Particle[count];

        int alive = ps.GetParticles(particles);

        float systemDistance = Vector3.Distance(transform.position, otherCenter.position);
        var main = ps.main;

        for (int i = 0; i < alive; i++)
        {
            ParticleSystem.Particle p = particles[i];

            float lifetime01 = 1f - (p.remainingLifetime / p.startLifetime);

            float dynamicLastLifePercent = Mathf.Clamp(
                lastLifePercent + systemDistance * distanceStartsEarlierMultiplier,
                lastLifePercent,
                maxPullWindow
            );

            float startPullAt = 1f - dynamicLastLifePercent;
            if (lifetime01 < startPullAt)
                continue;

            float seed01 = Hash01(p.randomSeed);
            if (seed01 > travelChance)
                continue;

            Vector3 particleWorldPos = ParticleToWorldPosition(p.position, main);
            Vector3 toTargetWorld = otherCenter.position - particleWorldPos;
            float dist = toTargetWorld.magnitude;
            if (dist < 0.001f) continue;

            Vector3 dirWorld = toTargetWorld / dist;

            float speedBoost = Mathf.Clamp(dist * distanceSpeedMultiplier, 0f, maxDistanceSpeedBoost);
            float dynamicSpeed = maxTravelSpeed + speedBoost;

            Vector3 currentVelocityWorld = VelocityToWorld(p.velocity, main);
            Vector3 desiredVelocityWorld = dirWorld * dynamicSpeed;
            Vector3 carriedVelocityWorld = currentVelocityWorld * keepSomeOriginalVelocity;

            float t = Mathf.InverseLerp(startPullAt, 1f, lifetime01);
            Vector3 newVelocityWorld = Vector3.Lerp(
                currentVelocityWorld,
                carriedVelocityWorld + desiredVelocityWorld,
                t * pullStrength * Time.deltaTime
            );

            float finalMaxSpeed = dynamicSpeed;
            if (newVelocityWorld.magnitude > finalMaxSpeed)
                newVelocityWorld = newVelocityWorld.normalized * finalMaxSpeed;

            p.velocity = WorldToVelocity(newVelocityWorld, main);
            particles[i] = p;
        }

        ps.SetParticles(particles, alive);
    }

    Vector3 ParticleToWorldPosition(Vector3 particlePosition, ParticleSystem.MainModule main)
    {
        switch (main.simulationSpace)
        {
            case ParticleSystemSimulationSpace.World:
                return particlePosition;

            case ParticleSystemSimulationSpace.Local:
                return transform.TransformPoint(particlePosition);

            case ParticleSystemSimulationSpace.Custom:
                if (main.customSimulationSpace != null)
                    return main.customSimulationSpace.TransformPoint(particlePosition);
                return transform.TransformPoint(particlePosition);

            default:
                return particlePosition;
        }
    }

    Vector3 VelocityToWorld(Vector3 velocity, ParticleSystem.MainModule main)
    {
        switch (main.simulationSpace)
        {
            case ParticleSystemSimulationSpace.World:
                return velocity;

            case ParticleSystemSimulationSpace.Local:
                return transform.TransformDirection(velocity);

            case ParticleSystemSimulationSpace.Custom:
                if (main.customSimulationSpace != null)
                    return main.customSimulationSpace.TransformDirection(velocity);
                return transform.TransformDirection(velocity);

            default:
                return velocity;
        }
    }

    Vector3 WorldToVelocity(Vector3 worldVelocity, ParticleSystem.MainModule main)
    {
        switch (main.simulationSpace)
        {
            case ParticleSystemSimulationSpace.World:
                return worldVelocity;

            case ParticleSystemSimulationSpace.Local:
                return transform.InverseTransformDirection(worldVelocity);

            case ParticleSystemSimulationSpace.Custom:
                if (main.customSimulationSpace != null)
                    return main.customSimulationSpace.InverseTransformDirection(worldVelocity);
                return transform.InverseTransformDirection(worldVelocity);

            default:
                return worldVelocity;
        }
    }

    private float Hash01(uint x)
    {
        x ^= 2747636419u;
        x *= 2654435769u;
        x ^= x >> 16;
        x *= 2654435769u;
        x ^= x >> 16;
        x *= 2654435769u;
        return (x & 0x00FFFFFF) / 16777215f;
    }
}