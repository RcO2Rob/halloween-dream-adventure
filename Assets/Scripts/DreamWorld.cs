using System.Collections.Generic;
using UnityEngine;

namespace LostDream
{
    public static class DreamWorld
    {
        public static DreamGame Build(int level)
        {
            Random.InitState(721 + level);
            var root = new GameObject("Lost Dream - Chapter " + (level + 1));
            var game = root.AddComponent<DreamGame>();
            game.levelIndex = level;
            game.spawnPoint = level == 2 ? new Vector3(0, .12f, -13) : new Vector3(0, .12f, -3);
            game.audioSystem = root.AddComponent<DreamAudio>();
            game.player = Player(root.transform, game.spawnPoint);
            game.gameCamera = Camera(root.transform, game.player, level == 2);
            root.AddComponent<DreamHUD>();
            Lighting(root.transform);
            var lamps = new List<Lantern>();
            var dragons = new List<FlyingDragon>();
            var runes = new List<DreamRune>();
            if (level == 0)
            {
                Island(root.transform, new Vector3(0, 0, 1), 22, 22, "Arrival meadow");
                Island(root.transform, new Vector3(0, 0, 31), 22, 22, "Dragon's crossing");
                Island(root.transform, new Vector3(0, 0, 61), 22, 22, "Candy dream");
                var first = Path(root.transform, new Vector3(0, 0, 12), new Vector3(0, 0, 20), 5);
                var second = Path(root.transform, new Vector3(0, 0, 42), new Vector3(0, 0, 50), 5);
                MakeJumpPath(first, level);
                lamps.Add(Lamp(root.transform, new Vector3(-4, 0, 8), "First light", first, new Vector3(-4, .1f, 6)));
                lamps.Add(Lamp(root.transform, new Vector3(4, 0, 38), "Crossing light", second, new Vector3(4, .1f, 36)));
                runes.Add(Rune(root.transform, new Vector3(4, 0, 6), false));
                dragons.Add(Dragon(root.transform, new Vector3(0, 2.5f, 29), 1.6f, 2));
                Candy(root.transform, new Vector3(-2, .65f, 54));
                Candy(root.transform, new Vector3(2, .65f, 54));
                game.candySpawnPoints = new[] { new Vector3(-2, 0, 54), new Vector3(2, 0, 54) };
                runes.Add(Rune(root.transform, new Vector3(0, 0, 60), true));
                Gate(root.transform, new Vector3(0, 0, 68));
                Sign(root.transform, new Vector3(-5, 0, -1), "MOVE  /  WASD", "FOLLOW THE LANTERNS");
                Sign(root.transform, new Vector3(5.5f, 0, 2), "AIM  /  MOUSE", "LEFT-CLICK  /  BOW");
                Sign(root.transform, new Vector3(-5, 0, 53), "E  /  PICK UP", "RIGHT-CLICK  /  THROW");
            }
            else if (level == 1)
            {
                Island(root.transform, new Vector3(0, 0, 1), 22, 22, "Garden entrance");
                Island(root.transform, new Vector3(-18, 0, 25), 16, 20, "Wandering orchard");
                Island(root.transform, new Vector3(2, 0, 45), 22, 18, "Moonlit terrace");
                Island(root.transform, new Vector3(2, 0, 74), 26, 20, "Dream gate garden");
                var west = Path(root.transform, new Vector3(-5, 0, 10), new Vector3(-15, 0, 17), 4.8f);
                var terrace = Path(root.transform, new Vector3(-12, 0, 31), new Vector3(-2, 0, 38), 4.8f);
                var final = Path(root.transform, new Vector3(2, 0, 54), new Vector3(2, 0, 64), 5);
                MakeJumpPath(final, level);
                lamps.Add(Lamp(root.transform, new Vector3(-6, 0, 7), "Orchard light", west, new Vector3(-6, .1f, 5)));
                lamps.Add(Lamp(root.transform, new Vector3(-16, 0, 30), "Terrace light", terrace, new Vector3(-16, .1f, 28)));
                lamps.Add(Lamp(root.transform, new Vector3(7, 0, 50), "Gate light", final, new Vector3(7, .1f, 48)));
                dragons.Add(Dragon(root.transform, new Vector3(-20, 2.5f, 23), 2, 2));
                dragons.Add(Dragon(root.transform, new Vector3(-14, 2.8f, 29), 1.5f, 2, NightmareKind.Ghost));
                dragons.Add(Dragon(root.transform, new Vector3(-3, 2.8f, 44), 2, 3));
                dragons.Add(Dragon(root.transform, new Vector3(7, 2.5f, 46), 2, 2, NightmareKind.Armabee));
                dragons.Add(Dragon(root.transform, new Vector3(2, 2.8f, 73), 3, 3, NightmareKind.Demon));
                Candy(root.transform, new Vector3(-18, .65f, 19));
                Candy(root.transform, new Vector3(4, .65f, 41));
                Gate(root.transform, new Vector3(2, 0, 81));
                Sign(root.transform, new Vector3(5, 0, -1), "THE DREAM IS BROKEN", "LOOK FOR THE NEXT LIGHT");
            }
            else
            {
                Island(root.transform, new Vector3(0, 0, -13), 17, 10, "Castle approach", false);
                Island(root.transform, new Vector3(0, 0, 12), 30, 28, "Pumpkin keep courtyard", false);
                var entrance = Path(root.transform, new Vector3(0, 0, -8), new Vector3(0, 0, -2), 6);
                MakeJumpPath(entrance, level);
                lamps.Add(Lamp(root.transform, new Vector3(-4, 0, -10), "Castle light", entrance, new Vector3(-4, .1f, -12)));
                Castle(root.transform);
                game.boss = Boss(root.transform, new Vector3(0, .12f, 18));
                game.candySpawnPoints = new[]
                {
                    new Vector3(-9, 0, 4), new Vector3(9, 0, 4), new Vector3(-8, 0, 11),
                    new Vector3(8, 0, 11), new Vector3(-8, 0, 20), new Vector3(8, 0, 20),
                    new Vector3(-4, 0, 7), new Vector3(4, 0, 7)
                };
                foreach (int i in new[] { 0, 1, 4, 5 }) Candy(root.transform, game.candySpawnPoints[i] + Vector3.up * .65f);
                Sign(root.transform, new Vector3(4, 0, -13), "THE PUMPKIN KEEP", "SHIFT  /  DODGE");
            }
            game.lanterns = lamps.ToArray();
            game.dragons = dragons.ToArray();
            game.runes = runes.ToArray();
            return game;
        }

        static PlayerMotor Player(Transform parent, Vector3 point)
        {
            var root = DreamArt.Group("Dreamwalker", parent, point);
            root.layer = 2;
            var cc = root.AddComponent<CharacterController>();
            cc.height = 1.8f; cc.radius = .35f; cc.center = new Vector3(0, .9f, 0); cc.skinWidth = .04f; cc.stepOffset = .28f;
            var player = root.AddComponent<PlayerMotor>();
            var timmyPrefab = Resources.Load<GameObject>("Characters/Timmy");
            if (timmyPrefab != null)
            {
                var timmy = Object.Instantiate(timmyPrefab, root.transform);
                player.visual = timmy.transform;
                player.characterAnimation = timmy.GetComponent<TimmyAnimation>();
                player.characterAnimation.player = player;
                var animator = player.characterAnimation.animator;
                player.leftLeg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                player.rightLeg = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
                player.leftArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                player.rightArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
                var bowProp = DreamArt.Group("Bow — follows left hand", timmy.transform, new Vector3(-.4f, 1, .1f));
                player.characterAnimation.bow = bowProp.transform;
                var curve = bowProp.AddComponent<LineRenderer>();
                curve.useWorldSpace = false; curve.positionCount = 9;
                curve.startWidth = curve.endWidth = .055f; curve.sharedMaterial = DreamArt.Mat("Wood");
                for (int i = 0; i < 9; i++)
                {
                    float angle = Mathf.Lerp(-1.2f, 1.2f, i / 8f);
                    curve.SetPosition(i, new Vector3(0, Mathf.Sin(angle) * .55f, (Mathf.Cos(angle) - 1) * .28f));
                }
                var stringProp = DreamArt.Group("Bow string", bowProp.transform, Vector3.zero).AddComponent<LineRenderer>();
                stringProp.useWorldSpace = false; stringProp.positionCount = 3;
                stringProp.startWidth = stringProp.endWidth = .018f; stringProp.sharedMaterial = DreamArt.Mat("GlowGold");
                stringProp.SetPosition(0, curve.GetPosition(0)); stringProp.SetPosition(1, Vector3.zero); stringProp.SetPosition(2, curve.GetPosition(8));
                player.heldCandy = DreamArt.CandyVisual(timmy.transform, new Vector3(.32f, 1.1f, .42f), .6f);
                player.heldCandy.SetActive(false);
                return player;
            }
            var model = DreamArt.Group("Animated dreamwalker", root.transform, Vector3.zero);
            player.visual = model.transform;
            DreamArt.Shape("Coat", PrimitiveType.Capsule, model.transform, new Vector3(0, 1.15f, 0), new Vector3(.7f, .43f, .55f), "Coat");
            DreamArt.Shape("Face", PrimitiveType.Sphere, model.transform, new Vector3(0, 1.85f, 0), new Vector3(.58f, .6f, .52f), "Skin");
            DreamArt.Shape("Hair", PrimitiveType.Sphere, model.transform, new Vector3(0, 2, -.08f), new Vector3(.62f, .45f, .56f), "Wood");
            for (int side = -1; side <= 1; side += 2)
            {
                DreamArt.Shape("Eye", PrimitiveType.Sphere, model.transform, new Vector3(side * .12f, 1.88f, .235f), new Vector3(.06f, .09f, .04f), "Ink");
                var leg = DreamArt.Group(side < 0 ? "Left leg" : "Right leg", model.transform, new Vector3(side * .19f, .7f, 0));
                DreamArt.Shape("Boot", PrimitiveType.Capsule, leg.transform, new Vector3(0, -.37f, 0), new Vector3(.27f, .32f, .32f), "Ink");
                var arm = DreamArt.Group(side < 0 ? "Left arm" : "Right arm", model.transform, new Vector3(side * .4f, 1.45f, 0));
                DreamArt.Shape("Sleeve", PrimitiveType.Capsule, arm.transform, new Vector3(0, -.22f, 0), new Vector3(.23f, .26f, .23f), "Coat");
                DreamArt.Shape("Hand", PrimitiveType.Sphere, arm.transform, new Vector3(0, -.48f, 0), Vector3.one * .2f, "Skin");
                if (side < 0) { player.leftLeg = leg.transform; player.leftArm = arm.transform; }
                else { player.rightLeg = leg.transform; player.rightArm = arm.transform; }
            }
            DreamArt.Shape("Scarf", PrimitiveType.Cylinder, model.transform, new Vector3(0, 1.6f, 0), new Vector3(.65f, .06f, .57f), "Pumpkin");
            DreamArt.Shape("Hat brim", PrimitiveType.Cylinder, model.transform, new Vector3(0, 2.12f, 0), new Vector3(.94f, .045f, .94f), "CoatDark");
            var hat = DreamArt.Mesh("Pointed hat", "Cone", model.transform, new Vector3(0, 2.15f, 0), new Vector3(.65f, .65f, .65f), "CoatDark");
            hat.transform.localRotation = Quaternion.Euler(0, 0, -12);
            DreamArt.Shape("Hat band", PrimitiveType.Cylinder, model.transform, new Vector3(0, 2.21f, 0), new Vector3(.57f, .05f, .57f), "GlowGold");
            var bow = DreamArt.Group("Bow", player.leftArm, new Vector3(0, -.45f, .12f));
            var line = bow.AddComponent<LineRenderer>();
            line.useWorldSpace = false; line.positionCount = 9; line.startWidth = line.endWidth = .055f; line.sharedMaterial = DreamArt.Mat("Wood");
            for (int i = 0; i < 9; i++)
            { float a = Mathf.Lerp(-1.2f, 1.2f, i / 8f); line.SetPosition(i, new Vector3(0, Mathf.Sin(a) * .55f, Mathf.Cos(a) * .35f)); }
            player.heldCandy = DreamArt.CandyVisual(player.rightArm, new Vector3(0, -.42f, .22f), .6f);
            player.heldCandy.SetActive(false);
            return player;
        }

        static Camera Camera(Transform parent, PlayerMotor player, bool arena)
        {
            var go = DreamArt.Group("Main Camera", parent, player.transform.position + new Vector3(0, 5.65f, -9.52f));
            go.tag = "MainCamera";
            var camera = go.AddComponent<Camera>();
            camera.orthographic = false; camera.fieldOfView = 58; camera.nearClipPlane = .1f; camera.farClipPlane = 250;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.065f, .05f, .14f);
            go.transform.rotation = Quaternion.Euler(25, 0, 0);
            go.AddComponent<AudioListener>();
            var follow = go.AddComponent<DreamCamera>(); follow.target = player.transform; follow.arena = arena;
            return camera;
        }

        static void Lighting(Transform parent)
        {
            var sun = DreamArt.Group("Moonlight", parent, Vector3.zero).AddComponent<Light>();
            sun.type = LightType.Directional; sun.color = new Color(.8f, .8f, 1); sun.intensity = 1.05f;
            sun.shadows = LightShadows.Soft; sun.transform.rotation = Quaternion.Euler(48, -35, 0);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.34f, .29f, .49f);
            RenderSettings.fog = true; RenderSettings.fogColor = new Color(.09f, .065f, .16f);
            RenderSettings.fogMode = FogMode.ExponentialSquared; RenderSettings.fogDensity = .009f;
            var moon = DreamArt.Shape("Dream moon", PrimitiveType.Sphere, parent, new Vector3(34, 30, 60), Vector3.one * 5, "GlowMoon");
            for (int i = 0; i < 90; i++)
                DreamArt.Shape("Distant star", PrimitiveType.Sphere, parent,
                    new Vector3(Random.Range(-65, 65), Random.Range(12, 38), Random.Range(-20, 120)),
                    Vector3.one * Random.Range(.045f, .15f), "GlowMoon");
        }

        static void Island(Transform parent, Vector3 point, float width, float depth, string name, bool garden = true)
        {
            var root = DreamArt.Group(name, parent, point);
            DreamArt.Shape("Solid dream island", PrimitiveType.Cube, root.transform, new Vector3(0, -.45f, 0), new Vector3(width, .9f, depth), garden ? "Ground" : "CastleFloor", true);
            var underside = DreamArt.Mesh("Floating island crystal", "Cone", root.transform, new Vector3(0, -.9f, 0), new Vector3(width * .9f, 8, depth * .9f), "Rock");
            underside.transform.localRotation = Quaternion.Euler(180, 0, 0);
            for (int side = -1; side <= 1; side += 2)
            {
                DreamArt.Shape("Island rim", PrimitiveType.Cube, root.transform, new Vector3(side * width / 2, -.1f, 0), new Vector3(.2f, .4f, depth), "Rim");
                DreamArt.Shape("Island rim", PrimitiveType.Cube, root.transform, new Vector3(0, -.1f, side * depth / 2), new Vector3(width, .4f, .2f), "Rim");
            }
            for (float z = -depth / 2 + 2; z < depth / 2; z += 2.6f)
                DreamArt.Shape("Dream stepping stone", PrimitiveType.Cube, root.transform, new Vector3(Mathf.Sin(z) * .25f, .02f, z), new Vector3(2, .04f, 1.8f), "Stone");
            if (!garden) return;
            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector3 p = new Vector3(side * (width * .5f - 2.2f), 0, -depth * .35f + i * depth * .32f);
                    Tree(root.transform, p, Random.Range(.8f, 1.25f));
                    var pumpkin = DreamArt.Group("Autumn pumpkin", root.transform, p + new Vector3(-side * 1.6f, 0, 1.3f));
                    DreamArt.Pumpkin(pumpkin.transform, Random.Range(.55f, 1));
                }
                for (int i = 0; i < 8; i++)
                    DreamArt.Shape("Garden mushroom", PrimitiveType.Sphere, root.transform,
                        new Vector3(side * Random.Range(4, width / 2 - .8f), .12f, Random.Range(-depth / 2 + 1, depth / 2 - 1)),
                        new Vector3(.4f, .22f, .4f), i % 2 == 0 ? "Leaf" : "LeafGold");
            }
        }

        static void Tree(Transform parent, Vector3 point, float size)
        {
            var root = DreamArt.Group("Dream orchard tree", parent, point);
            root.transform.localScale = Vector3.one * size;
            DreamArt.Shape("Trunk", PrimitiveType.Cylinder, root.transform, Vector3.up * 1.5f, new Vector3(.45f, 1.5f, .45f), "Wood", true);
            for (int i = 0; i < 3; i++)
            {
                var branch = DreamArt.Shape("Branch", PrimitiveType.Cylinder, root.transform, new Vector3((i - 1) * .4f, 2.3f, .1f), new Vector3(.22f, .8f, .22f), "Wood");
                branch.transform.localRotation = Quaternion.Euler(0, i * 120, (i - 1) * -35);
                DreamArt.Shape("Autumn canopy", PrimitiveType.Sphere, root.transform, new Vector3((i - 1) * .7f, 3.2f + i * .2f, .1f), new Vector3(1.8f, 1.6f, 1.8f), i % 2 == 0 ? "Leaf" : "LeafGold");
            }
        }

        public static DreamPath Path(Transform parent, Vector3 from, Vector3 to, float width)
        {
            var root = DreamArt.Group("Nightmare-hidden bridge", parent, (from + to) * .5f);
            root.transform.rotation = Quaternion.LookRotation(to - from);
            float length = Vector3.Distance(from, to) + 1;
            var path = root.AddComponent<DreamPath>();
            path.solid = DreamArt.Group("Restored road", root.transform, Vector3.zero);
            DreamArt.Shape("Bridge collision", PrimitiveType.Cube, path.solid.transform, new Vector3(0, -.2f, 0), new Vector3(width, .4f, length), "Path", true);
            for (int i = 0; i < Mathf.CeilToInt(length); i++)
                DreamArt.Shape("Bridge plank", PrimitiveType.Cube, path.solid.transform, new Vector3(0, .04f, -length / 2 + i + .5f), new Vector3(width - .2f, .08f, .8f), "Stone");
            for (int side = -1; side <= 1; side += 2)
                DreamArt.Shape("Lit bridge edge", PrimitiveType.Cube, path.solid.transform, new Vector3(side * width / 2, .06f, 0), new Vector3(.1f, .12f, length), "GlowTeal");
            path.outline = DreamArt.Group("Faint road silhouette", root.transform, Vector3.zero);
            for (int side = -1; side <= 1; side += 2)
                DreamArt.Shape("Ghost edge", PrimitiveType.Cube, path.outline.transform, new Vector3(side * width / 2, -.12f, 0), new Vector3(.035f, .035f, length), "Mist");
            path.nightmare = DreamArt.Group("Nightmare fog", root.transform, Vector3.zero);
            for (int i = 0; i < 7; i++)
                DreamArt.Shape("Fog cloud", PrimitiveType.Sphere, path.nightmare.transform,
                    new Vector3(Mathf.Sin(i * 2) * width * .25f, .4f, Mathf.Lerp(-length / 2, length / 2, i / 6f)),
                    new Vector3(width * .75f, 1.3f, 2.6f), "Fog");
            path.solid.SetActive(false);
            return path;
        }

        public static void MakeJumpPath(DreamPath path, int chapter)
        {
            if (path.IsJumpRoute) return;
            var bridge = path.solid.GetComponentInChildren<BoxCollider>(true);
            float span = bridge.transform.localScale.z - 1;
            ClearChildren(path.solid.transform);
            ClearChildren(path.outline.transform);
            path.name = "Nightmare-hidden round platforms";
            path.solid.name = "Restored round jumping platforms";
            path.outline.name = "Faint round platform silhouettes";
            int count = chapter == 1 ? 3 : 2;
            float diameter = chapter == 0 ? 2.2f : chapter == 1 ? 1.9f : 1.6f;
            float gap = (span - count * diameter) / (count + 1);
            path.jumpPlatforms = new Transform[count];
            for (int i = 0; i < count; i++)
            {
                float x = (i % 2 == 0 ? -1 : 1) * (chapter == 0 ? .9f : chapter == 1 ? .6f : .7f);
                float height = chapter == 0 ? .12f + i * .06f : chapter == 1 ? (i == 1 ? .65f : .25f) : .4f + i * .4f;
                float z = -span / 2 + gap + diameter / 2 + i * (diameter + gap);
                var platform = DreamArt.Group("Jump platform " + (i + 1), path.solid.transform, new Vector3(x, height, z));
                var body = DreamArt.Shape("Flat round landing surface", PrimitiveType.Cylinder, platform.transform,
                    Vector3.down * .25f, new Vector3(diameter, .25f, diameter), "Stone");
                // Cylinders normally get a rounded capsule collider. Use the actual flat mesh.
                body.AddComponent<MeshCollider>().sharedMesh = body.GetComponent<MeshFilter>().sharedMesh;
                var rim = DreamArt.Group("Glowing landing rim", platform.transform, Vector3.up * .02f);
                DreamArt.Ring(rim.transform, diameter / 2 - .08f, "GlowTeal", 48, .055f);
                DreamArt.Shape("Landing center", PrimitiveType.Cylinder, platform.transform, Vector3.up * .014f,
                    new Vector3(.3f, .014f, .3f), "GlowGold");
                path.jumpPlatforms[i] = platform.transform;
                var silhouette = DreamArt.Group("Unlit round platform " + (i + 1), path.outline.transform, platform.transform.localPosition);
                DreamArt.Ring(silhouette.transform, diameter / 2, "Mist", 48, .035f);
            }
            path.jumpEntry = DreamArt.Group("Jump route entry - on island", path.transform,
                new Vector3(0, .06f, -span / 2 - 1.1f)).transform;
            path.jumpExit = DreamArt.Group("Jump route exit - on island", path.transform,
                new Vector3(0, .06f, span / 2 + 1.1f)).transform;
            DreamArt.Ring(path.jumpEntry, .55f, "GlowGold", 36, .06f);
            DreamArt.Ring(path.jumpExit, .55f, "GlowTeal", 36, .06f);
            Sign(path.transform, new Vector3(3.3f, 0, -span / 2 - 2.2f),
                "SPACE  /  JUMP", chapter == 0 ? "LAND ON THE ROUND LIGHTS" : "AIM FOR EACH GLOWING CIRCLE");
            path.solid.SetActive(false);
        }

        static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                var child = parent.GetChild(i).gameObject;
                if (Application.isPlaying) { child.SetActive(false); Object.Destroy(child); }
                else Object.DestroyImmediate(child);
            }
        }

        static Lantern Lamp(Transform parent, Vector3 point, string name, DreamPath path, Vector3 checkpoint)
        {
            var root = DreamArt.Group(name, parent, point);
            var lamp = root.AddComponent<Lantern>();
            lamp.lanternName = name; lamp.path = path; lamp.safeCheckpoint = checkpoint;
            DreamArt.Shape("Lantern pedestal", PrimitiveType.Cylinder, root.transform, Vector3.up * .3f, new Vector3(1.2f, .3f, 1.2f), "Stone", true);
            var pumpkin = DreamArt.Group("Jack-o-lantern", root.transform, Vector3.up * .6f);
            DreamArt.Pumpkin(pumpkin.transform, 1);
            lamp.flame = DreamArt.Shape("Dream flame", PrimitiveType.Sphere, root.transform, Vector3.up * 1.7f, Vector3.one * .22f, "GlowGold").transform;
            lamp.flame.gameObject.SetActive(false);
            lamp.glow = DreamArt.Group("Lantern glow", root.transform, Vector3.up * 2).AddComponent<Light>();
            lamp.glow.type = LightType.Point; lamp.glow.color = new Color(1, .55f, .2f); lamp.glow.range = 10; lamp.glow.intensity = .3f;
            var ring = DreamArt.Group("Lantern interaction ring", root.transform, Vector3.zero);
            DreamArt.Ring(ring.transform, 1.1f, "GlowGold", 32);
            return lamp;
        }

        public static CandyPickup Candy(Transform parent, Vector3 point, CandyHue hue = CandyHue.Pink)
        {
            var root = DreamArt.Group("Collectible dream candy", parent, point);
            var candy = root.AddComponent<CandyPickup>();
            candy.hue = hue;
            candy.visual = DreamArt.CandyVisual(root.transform, Vector3.zero, 1.1f, hue).transform;
            var ring = DreamArt.Group("Candy glimmer", root.transform, new Vector3(0, -.58f, 0));
            DreamArt.Ring(ring.transform, .45f, CandyPalette.Material(hue), 24, .04f);
            return candy;
        }

        static DreamRune Rune(Transform parent, Vector3 point, bool candyOnly)
        {
            var root = DreamArt.Group(candyOnly ? "Candy nightmare seal" : "Bow practice rune", parent, point);
            var rune = root.AddComponent<DreamRune>(); rune.candyOnly = candyOnly;
            var collider = root.AddComponent<SphereCollider>(); collider.center = Vector3.up * 1.4f; collider.radius = .9f;
            DreamArt.Shape("Rune base", PrimitiveType.Cylinder, root.transform, Vector3.up * .2f, new Vector3(1.5f, .2f, 1.5f), "Stone");
            rune.crystal = DreamArt.Shape("Rotating dream crystal", PrimitiveType.Cube, root.transform, Vector3.up * 1.4f,
                Vector3.one * 1.05f, candyOnly ? "Candy" : "GlowGold").transform;
            rune.crystal.localRotation = Quaternion.Euler(35, 45, 20);
            var ring = DreamArt.Group("Rune circle", root.transform, Vector3.zero);
            DreamArt.Ring(ring.transform, 1.1f, candyOnly ? "Candy" : "GlowGold", 32);
            return rune;
        }

        static FlyingDragon Dragon(Transform parent, Vector3 point, float radius, int health, NightmareKind kind = NightmareKind.Dragon)
        {
            var root = DreamArt.Group("Small nightmare dragon", parent, point);
            var dragon = root.AddComponent<FlyingDragon>(); dragon.home = point; dragon.patrolRadius = radius; dragon.health = health;
            dragon.kind = kind;
            root.AddComponent<SphereCollider>().radius = .9f;
            var body = DreamArt.Group("Animated dragon", root.transform, Vector3.zero); dragon.body = body.transform;
            string modelName = kind != NightmareKind.Dragon ? kind.ToString() : health >= 3 ? "Dragon_Evolved" : "Dragon";
            var prefab = Resources.Load<GameObject>("Monsters/" + modelName);
            if (prefab != null)
            {
                var model = Object.Instantiate(prefab, body.transform, false);
                dragon.modelAnimation = model.GetComponentInChildren<Animation>();
                var sigil = DreamArt.Group("Attack warning", body.transform, new Vector3(0, -.65f, 0));
                DreamArt.Ring(sigil.transform, 1.15f, dragon.AttackMaterial, 28, .10f);
                dragon.chargeMarker = sigil.GetComponent<Renderer>();
                dragon.chargeMarker.enabled = false;
                return dragon;
            }
            DreamArt.Shape("Body", PrimitiveType.Sphere, body.transform, Vector3.zero, new Vector3(.95f, 1, 1.5f), "Dragon");
            DreamArt.Shape("Belly", PrimitiveType.Sphere, body.transform, new Vector3(0, -.12f, .32f), new Vector3(.7f, .75f, .95f), "DragonBelly");
            DreamArt.Shape("Head", PrimitiveType.Sphere, body.transform, new Vector3(0, .5f, .65f), new Vector3(.85f, .8f, .9f), "Dragon");
            for (int side = -1; side <= 1; side += 2)
            {
                DreamArt.Mesh("Horn", "Cone", body.transform, new Vector3(side * .25f, .8f, .5f), new Vector3(.18f, .4f, .18f), "GlowGold");
                DreamArt.Shape("Dragon eye", PrimitiveType.Sphere, body.transform, new Vector3(side * .28f, .6f, .92f), new Vector3(.13f, .15f, .06f), "GlowGold");
                var wing = DreamArt.Group(side < 0 ? "Left wing" : "Right wing", body.transform, new Vector3(side * .36f, .25f, 0));
                DreamArt.Mesh("Wing membrane", "Wing", wing.transform, Vector3.zero, new Vector3(side * 1.3f, 1.3f, 1.3f), "DragonWing");
                if (side < 0) dragon.leftWing = wing.transform; else dragon.rightWing = wing.transform;
            }
            for (int i = 0; i < 3; i++)
                DreamArt.Shape("Tail", PrimitiveType.Sphere, body.transform, new Vector3(0, -.15f - i * .06f, -.8f - i * .32f), Vector3.one * (.4f - i * .08f), "Dragon");
            return dragon;
        }

        static void Gate(Transform parent, Vector3 point)
        {
            var root = DreamArt.Group("Dream gate", parent, point);
            var gate = root.AddComponent<DreamGate>();
            var trigger = root.AddComponent<BoxCollider>(); trigger.isTrigger = true; trigger.center = Vector3.up * 1.5f; trigger.size = new Vector3(3.8f, 3, 1.6f);
            var parts = new List<Renderer>();
            for (int side = -1; side <= 1; side += 2)
            {
                DreamArt.Shape("Arch pillar", PrimitiveType.Cube, root.transform, new Vector3(side * 2.1f, 1.8f, 0), new Vector3(.65f, 3.6f, .65f), "Stone", true);
                parts.Add(DreamArt.Shape("Gate glow", PrimitiveType.Cube, root.transform, new Vector3(side * 1.8f, 1.8f, -.05f), new Vector3(.12f, 3.3f, .7f), "Mist").GetComponent<Renderer>());
            }
            parts.Add(DreamArt.Shape("Arch lintel", PrimitiveType.Cube, root.transform, new Vector3(0, 3.8f, 0), new Vector3(4.8f, .6f, .7f), "Mist").GetComponent<Renderer>());
            gate.glowParts = parts.ToArray();
            var ring = DreamArt.Group("Gate ring", root.transform, Vector3.zero);
            DreamArt.Ring(ring.transform, 2.3f, "GlowTeal", 32);
        }

        static void Sign(Transform parent, Vector3 point, string top, string bottom)
        {
            var root = DreamArt.Group("Dream sign", parent, point);
            DreamArt.Shape("Signpost", PrimitiveType.Cylinder, root.transform, Vector3.up * .8f, new Vector3(.13f, .8f, .13f), "Wood");
            DreamArt.Shape("Signboard", PrimitiveType.Cube, root.transform, Vector3.up * 1.7f, new Vector3(3.6f, 1.1f, .15f), "CoatDark");
            var textGo = DreamArt.Group("Instructions", root.transform, new Vector3(0, 1.7f, -.09f));
            textGo.transform.localRotation = Quaternion.identity;
            var text = textGo.AddComponent<TextMesh>();
            text.text = top + "\n" + bottom; text.fontSize = 48; text.characterSize = .035f; text.anchor = TextAnchor.MiddleCenter;
            text.color = new Color(1, .83f, .53f);
        }

        static PumpkinBoss Boss(Transform parent, Vector3 point)
        {
            var root = DreamArt.Group("The Pumpkin King", parent, point);
            var boss = root.AddComponent<PumpkinBoss>();
            var collider = root.AddComponent<SphereCollider>(); collider.center = Vector3.up * 2.7f; collider.radius = 2.45f;
            DreamArt.Shape("Boss dais", PrimitiveType.Cylinder, root.transform, Vector3.zero, new Vector3(7, .18f, 7), "Rock", true);
            var pumpkin = DreamArt.Group("Animated pumpkin head", root.transform, Vector3.up * .3f);
            DreamArt.Pumpkin(pumpkin.transform, 5);
            boss.pumpkin = pumpkin.transform;
            var weak = new List<Renderer>();
            foreach (var renderer in pumpkin.GetComponentsInChildren<Renderer>())
                if (renderer.name.Contains("eye") || renderer.name == "Smile") weak.Add(renderer);
            boss.weakPoint = weak.ToArray();
            var crown = DreamArt.Group("Floating nightmare crown", root.transform, Vector3.up * 5.1f);
            boss.crown = crown.transform;
            DreamArt.Shape("Crown ring", PrimitiveType.Cylinder, crown.transform, Vector3.zero, new Vector3(2.1f, .15f, 2.1f), "GlowGold");
            for (int i = 0; i < 5; i++)
            {
                float a = Mathf.PI * 2 * i / 5;
                DreamArt.Mesh("Crown point", "Cone", crown.transform, new Vector3(Mathf.Cos(a) * .8f, .12f, Mathf.Sin(a) * .8f), new Vector3(.4f, .65f, .4f), "GlowGold");
            }
            var ring = DreamArt.Group("Nightmare sigil", root.transform, Vector3.zero);
            DreamArt.Ring(ring.transform, 4, "Candy", 48, .06f);
            return boss;
        }

        static void Castle(Transform parent)
        {
            var root = DreamArt.Group("Pumpkin keep walls", parent, Vector3.zero);
            for (int side = -1; side <= 1; side += 2)
            {
                DreamArt.Shape("Castle side wall", PrimitiveType.Cube, root.transform, new Vector3(side * 14, 2, 12), new Vector3(1, 4, 27), "CastleWall", true);
                for (int z = 0; z < 27; z += 3)
                    DreamArt.Shape("Battlement", PrimitiveType.Cube, root.transform, new Vector3(side * 14, 4.5f, z), new Vector3(1.3f, 1, 1.6f), "CastleWall");
                for (int z = 0; z <= 26; z += 26)
                {
                    var tower = DreamArt.Group("Castle tower", root.transform, new Vector3(side * 14, 0, z));
                    DreamArt.Shape("Tower", PrimitiveType.Cylinder, tower.transform, Vector3.up * 3.5f, new Vector3(4.5f, 3.5f, 4.5f), "CastleWall", true);
                    DreamArt.Mesh("Tower roof", "Cone", tower.transform, Vector3.up * 7, new Vector3(5.6f, 3, 5.6f), "CoatDark");
                    DreamArt.Shape("Tower window", PrimitiveType.Cube, tower.transform, new Vector3(0, 4.5f, -2.25f), new Vector3(.7f, 1.4f, .12f), "GlowGold");
                }
            }
            DreamArt.Shape("Keep back wall", PrimitiveType.Cube, root.transform, new Vector3(0, 2.5f, 26), new Vector3(28, 5, 1), "CastleWall", true);
            for (int x = -12; x <= 12; x += 3)
                DreamArt.Shape("Back battlement", PrimitiveType.Cube, root.transform, new Vector3(x, 5.5f, 26), new Vector3(1.7f, 1, 1.4f), "CastleWall");
            for (int x = -8; x <= 8; x += 8)
                DreamArt.Shape("Keep window", PrimitiveType.Cube, root.transform, new Vector3(x, 3, 25.4f), new Vector3(.9f, 1.5f, .15f), "GlowGold");
        }
    }
}
