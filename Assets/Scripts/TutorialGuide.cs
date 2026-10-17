using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace LostDream
{
    public enum TutorialStep { FirstLantern, Bow, Jump, Monster, SecondLantern, Candy, Seal, Exit }

    // Runtime-only guidance follows the saved map's references without rewriting it.
    public class TutorialGuide : MonoBehaviour
    {
        public TutorialStep Step { get; private set; }
        public string Title { get; private set; } = "Light the pumpkin lantern";
        public string Instruction { get; private set; } = "Follow the golden trail.\nE / light the pumpkin lantern.";
        public int StepNumber => (int)Step + 1;
        DreamGame game;
        TutorialGroundRoute navigation;
        Transform visuals, ring;
        Material trailMaterial;
        Mesh arrowMesh;
        readonly List<Transform> arrows = new List<Transform>();
        float refresh;
        Vector3 goal;
        int previousLights = -1;

        void Awake()
        {
            game = GetComponent<DreamGame>();
            navigation = new TutorialGroundRoute(game);
            visuals = DreamArt.Group("Tutorial golden ground trail", transform, Vector3.zero).transform;
            visuals.gameObject.layer = 2;
            trailMaterial = new Material(DreamArt.Mat("GlowGold"));
            arrowMesh = new Mesh { name = "Tutorial direction arrow" };
            arrowMesh.vertices = new[] { new Vector3(-.19f,0,-.17f), new Vector3(0,0,.23f), new Vector3(.19f,0,-.17f) };
            arrowMesh.triangles = new[] { 0,1,2 };
            arrowMesh.RecalculateNormals(); arrowMesh.RecalculateBounds();
            ring = DreamArt.Group("Current tutorial target", visuals, Vector3.zero).transform;
            DreamArt.Ring(ring, .9f, "GlowGold", 48, .065f);
            ring.GetComponent<LineRenderer>().sharedMaterial = trailMaterial;
            visuals.gameObject.SetActive(false);
        }
        void Update()
        {
            bool visible = game.Phase == GamePhase.Playing && !game.IntroductionVisible;
            visuals.gameObject.SetActive(visible);
            if (!visible) { refresh = 0; return; }
            refresh -= Time.deltaTime;
            if (refresh <= 0)
            {
                refresh = .45f;
                if (previousLights != game.LitCount)
                { previousLights = game.LitCount; navigation.Clear(); }
                ChooseTarget();
                DrawRoute();
            }
            float pulse = .85f + .15f * Mathf.Sin(Time.time * 3);
            trailMaterial.SetColor("_EmissionColor", new Color(1,.66f,.2f) * pulse);
            ring.localScale = Vector3.one * (1 + .07f * Mathf.Sin(Time.time * 3));
        }
        void ChooseTarget()
        {
            var first = game.lanterns.FirstOrDefault();
            var bow = game.runes.FirstOrDefault(x => x != null && !x.candyOnly && !x.Cleared);
            var enemy = game.dragons.FirstOrDefault(x => x != null && !x.Defeated);
            var second = game.lanterns.Skip(1).FirstOrDefault(x => x != null && !x.Lit);
            var seal = game.runes.FirstOrDefault(x => x != null && x.candyOnly && !x.Cleared);
            if (first != null && !first.Lit)
                Set(TutorialStep.FirstLantern, first.transform.position, "Light the pumpkin lantern",
                    "Follow the golden trail.\nE / light the lantern and reveal the path.");
            else if (bow != null)
                Set(TutorialStep.Bow, bow.transform.position, "Try your bow",
                    "Mouse / aim at the gold crystal.\nLeft-click / shoot an arrow.");
            else if (enemy != null && first != null && first.path != null && first.path.IsJumpRoute && BeforeExit(first.path))
                Set(TutorialStep.Jump, first.path.jumpExit.position, "Jump across the round platforms",
                    "Space / jump from one light to the next.\nA fall returns you to your last lantern.");
            else if (enemy != null)
                Set(TutorialStep.Monster, enemy.transform.position, "Defeat the flying monster",
                    "Mouse / aim. Left-click / shoot.\nKeep moving to dodge its attacks.");
            else if (second != null)
                Set(TutorialStep.SecondLantern, second.transform.position, "Light the next pumpkin lantern",
                    "Follow the trail to the next lantern.\nE / reveal the road to the candy island.");
            else if (seal != null && !game.player.CarryingCandy)
            {
                var candy = game.GetComponentsInChildren<CandyPickup>().OrderBy(x =>
                    Vector3.Distance(x.transform.position, seal.transform.position)).FirstOrDefault();
                Set(TutorialStep.Candy, candy != null ? candy.transform.position : game.candySpawnPoints[0], "Pick up a candy",
                    "Follow the trail. E / pick up candy.\nSupplies refill if you use them all.");
            }
            else if (seal != null)
                Set(TutorialStep.Seal, seal.transform.position, "Break the pink nightmare seal",
                    "Aim at the pink crystal. Q / throw candy.\nMissed? Pick up another candy and try again.");
            else
            {
                var gate = game.GetComponentInChildren<DreamGate>();
                Set(TutorialStep.Exit, gate != null ? gate.transform.position + gate.transform.forward * 2 : game.player.transform.position,
                    "Leave through the open gate", "Follow the golden trail through the gate.\nThe next chapter starts automatically.");
            }
        }
        bool BeforeExit(DreamPath path)
        {
            Vector3 direction = (path.jumpExit.position - path.jumpEntry.position).normalized;
            return Vector3.Dot(game.player.transform.position - path.jumpExit.position, direction) < -.5f;
        }
        void Set(TutorialStep step, Vector3 position, string title, string instruction)
        { Step = step; goal = position; Title = title; Instruction = instruction; }

        void DrawRoute()
        {
            // The exit's colliders move while its leaves open.
            if (Step == TutorialStep.Exit) navigation.Clear();
            var route = new List<Vector3>();
            Vector3 cursor = game.player.transform.position;
            // Explicit jump links keep the route on the real stepping stones.
            foreach (var path in game.lanterns.Select(x => x.path).Where(x => x != null && x.Revealed && x.IsJumpRoute)
                .OrderBy(x => Vector3.Distance(cursor, x.transform.position)))
            {
                Vector3 entry = path.jumpEntry.position, exit = path.jumpExit.position;
                Vector3 direction = (exit - entry).normalized;
                bool forward = Vector3.Dot(cursor - exit, direction) < -.5f && Vector3.Dot(goal - exit, direction) >= -.5f;
                bool backward = Vector3.Dot(cursor - entry, direction) > .5f && Vector3.Dot(goal - entry, direction) <= .5f;
                if (!forward && !backward) continue;
                var anchors = new List<Vector3> { entry };
                anchors.AddRange(path.jumpPlatforms.Select(x => x.position)); anchors.Add(exit);
                if (backward) { anchors.Reverse(); direction = -direction; }
                bool added = false;
                foreach (Vector3 anchor in anchors)
                {
                    if (Vector3.Dot(anchor - cursor, direction) < -.4f) continue;
                    if (!added) { route.AddRange(navigation.Find(cursor, anchor)); added = true; }
                    route.Add(anchor);
                }
                cursor = anchors[anchors.Count - 1];
            }
            Vector3 approach = goal;
            if (Step != TutorialStep.Jump && Step != TutorialStep.Exit)
            {
                Vector3 back = Vector3.ProjectOnPlane(cursor - goal, Vector3.up).normalized;
                approach += back * (Step == TutorialStep.Bow || Step == TutorialStep.Seal ? 3.5f : 1.5f);
            }
            route.AddRange(navigation.Find(cursor, approach));
            int used = 0;
            float spacing = .72f, remaining = .35f;
            for (int i = 1; i < route.Count && used < 100; i++)
            {
                Vector3 delta = route[i] - route[i-1];
                float length = delta.magnitude;
                if (length < .01f) continue;
                Vector3 direction = delta / length;
                for (; remaining < length && used < 100; remaining += spacing)
                {
                    Vector3 sample = route[i-1] + direction * remaining;
                    // Never draw a walkable-looking line in a gap or through scenery.
                    if (!navigation.Ground(sample, out Vector3 ground) || !navigation.Clearance(ground)) continue;
                    if (used == arrows.Count) arrows.Add(NewArrow());
                    Transform arrow = arrows[used++];
                    arrow.gameObject.SetActive(true);
                    arrow.position = ground + Vector3.up * .10f;
                    Vector3 facing = Vector3.ProjectOnPlane(direction, Vector3.up);
                    arrow.rotation = Quaternion.LookRotation(facing.sqrMagnitude > .01f ? facing : Vector3.forward);
                }
                remaining -= length;
            }
            for (int i = used; i < arrows.Count; i++) arrows[i].gameObject.SetActive(false);
            bool onFloor = navigation.Ground(goal, out Vector3 target);
            ring.gameObject.SetActive(onFloor);
            if (onFloor) ring.position = target + Vector3.up * .12f;
        }
        Transform NewArrow()
        {
            var arrow = DreamArt.Group("Golden tutorial arrow", visuals, Vector3.zero);
            arrow.layer = 2;
            arrow.AddComponent<MeshFilter>().sharedMesh = arrowMesh;
            var renderer = arrow.AddComponent<MeshRenderer>(); renderer.sharedMaterial = trailMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
            return arrow.transform;
        }
        void OnDestroy()
        {
            if (trailMaterial != null) Destroy(trailMaterial);
            if (arrowMesh != null) Destroy(arrowMesh);
        }
    }
}
