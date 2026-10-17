using System.Collections.Generic;
using UnityEngine;

namespace LostDream
{
    // Bounded, scene-owned celebration. No collision, damage, audio or gameplay RNG.
    public class VictoryFireworks : MonoBehaviour
    {
        struct Rocket { public Vector3 from, to; public Color color; public float age, duration; }
        readonly List<Rocket> rockets = new List<Rocket>();
        readonly System.Random random = new System.Random(20261016);
        readonly Color[] palette = {
            new Color(1, .7f, .22f), new Color(1, .32f, .62f),
            new Color(.25f, 1, .83f), new Color(.6f, .48f, 1)
        };
        DreamGame game;
        ParticleSystem sparks;
        Material material;
        Vector3 origin;
        float age, nextLaunch, backdropDepth;
        int launches;
        public int BurstsCompleted { get; private set; }
        public int LiveParticles => sparks == null ? 0 : sparks.particleCount;
        public Vector3 LastBurstPosition { get; private set; }

        public static VictoryFireworks Begin(DreamGame game)
        {
            var existing = game.GetComponentInChildren<VictoryFireworks>();
            if (existing != null) return existing;
            var root = new GameObject("Victory fireworks");
            root.transform.SetParent(game.transform, false); root.layer = 2;
            var show = root.AddComponent<VictoryFireworks>(); show.Initialize(game);
            return show;
        }

        void Initialize(DreamGame owner)
        {
            game = owner;
            origin = owner.VictoryCenter;
            backdropDepth = Mathf.Max(22, owner.boss.arenaMax.y - origin.z + 8);
            material = new Material(Resources.Load<Shader>("Effects/VictorySpark"));
            material.name = "Victory spark glow";
            var child = new GameObject("Sky sparks"); child.layer = 2; child.transform.SetParent(transform, false);
            sparks = child.AddComponent<ParticleSystem>();
            sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = sparks.main;
            main.loop = true; main.playOnAwake = false; main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.useUnscaledTime = true; main.maxParticles = 1800;
            main.startSpeed = 0; main.startLifetime = 2; main.gravityModifier = .12f;
            main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
            var emission = sparks.emission; emission.enabled = false;
            var shape = sparks.shape; shape.enabled = false;
            var color = sparks.colorOverLifetime; color.enabled = true;
            var fade = new Gradient();
            fade.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, .55f), new GradientAlphaKey(0, 1) });
            color.color = fade;
            var size = sparks.sizeOverLifetime; size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1, new AnimationCurve(new Keyframe(0, .8f), new Keyframe(.08f, 1), new Keyframe(.75f, .65f), new Keyframe(1, 0)));
            var trails = sparks.trails; trails.enabled = true; trails.ratio = .65f;
            trails.lifetime = .45f; trails.dieWithParticles = true;
            trails.widthOverTrail = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, .8f, 1, 0));
            var renderer = sparks.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material; renderer.trailMaterial = material;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.maxParticleSize = .1f;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            sparks.Play();
            // Two offset opening rockets give the first celebration a clear silhouette.
            Launch(-9, 16, palette[0]); Launch(10, 19, palette[1]); nextLaunch = 1.35f;
        }

        float Range(float min, float max) { return Mathf.Lerp(min, max, (float)random.NextDouble()); }

        void Launch(float x, float y, Color color)
        {
            rockets.Add(new Rocket {
                from = origin + new Vector3(x * .65f, 3, backdropDepth),
                to = origin + new Vector3(x, y, backdropDepth + Range(0, 5)),
                color = color, duration = Range(.85f, 1.15f)
            });
            launches++;
        }

        void Update()
        {
            if (game == null || game.Phase != GamePhase.Victory) { Destroy(gameObject); return; }
            float dt = Time.unscaledDeltaTime; age += dt;
            if (age >= nextLaunch)
            {
                Launch(Range(-15, 15), Range(14, 22), palette[launches % palette.Length]);
                nextLaunch = age + (age < 12 ? Range(.75f, 1.2f) : Range(1.8f, 2.6f));
            }
            for (int i = rockets.Count - 1; i >= 0; i--)
            {
                Rocket rocket = rockets[i]; rocket.age += dt;
                float t = Mathf.Clamp01(rocket.age / rocket.duration);
                Vector3 point = Vector3.Lerp(rocket.from, rocket.to, 1 - (1 - t) * (1 - t));
                for (int n = 0; n < 2; n++)
                    Emit(point + new Vector3(Range(-.05f, .05f), Range(-.15f, .05f), 0), Vector3.down * .45f,
                        new Color(1, .8f, .4f), .11f, .32f);
                if (t >= 1) { Burst(rocket.to, rocket.color); rockets.RemoveAt(i); }
                else rockets[i] = rocket;
            }
        }

        void Burst(Vector3 center, Color color)
        {
            BurstsCompleted++; LastBurstPosition = center;
            // Evenly spread rays with small depth variation, rather than dense random clumps.
            for (int i = 0; i < 88; i++)
            {
                float angle = i * Mathf.PI * 2 / 88 + Range(-.025f, .025f);
                Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), Range(-.45f, .45f)).normalized;
                float speed = Range(3.2f, 5.2f);
                Emit(center, direction * speed, Color.Lerp(color, Color.white, Range(0, .2f)), Range(.23f, .32f), Range(2.3f, 3.1f));
                if (i % 3 == 0) Emit(center, direction * speed * .55f, color, .18f, Range(1.6f, 2.4f));
            }
            Emit(center, Vector3.zero, Color.white, 1.1f, .15f);
        }

        void Emit(Vector3 point, Vector3 velocity, Color color, float size, float lifetime)
        {
            var particle = new ParticleSystem.EmitParams {
                position = point, velocity = velocity, startColor = color, startSize = size, startLifetime = lifetime
            };
            sparks.Emit(particle, 1);
        }

        void OnDestroy() { if (material != null) Destroy(material); }
    }
}
