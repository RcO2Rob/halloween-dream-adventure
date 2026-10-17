using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LostDream;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public static class SceneryCollisionBuilder
{
    const string QA = "docs/qa/scenery-collision";

    static string Category(string name)
    {
        foreach (string prefix in new[] { "Tree_", "Pumpkin_", "Gravestone_", "Coffin_", "Cauldron_", "Mushroom_", "Scarecrow_", "Book_Stand_" })
            if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return prefix.TrimEnd('_');
        if (name == "Dream orchard tree") return "Tree";
        if (name == "Autumn pumpkin") return "Pumpkin";
        if (name == "Abandoned timber watchtower") return "Timber";
        return null;
    }

    static bool Visible(MeshFilter mesh, Transform root)
    {
        var renderer = mesh.GetComponent<MeshRenderer>();
        if (renderer == null || !renderer.enabled || mesh.sharedMesh == null || mesh.sharedMesh.vertexCount < 3) return false;
        for (Transform t = mesh.transform; t != root; t = t.parent)
            if (!t.gameObject.activeSelf) return false;
        string name = mesh.name.ToLowerInvariant();
        return !name.Contains("flame") && !name.Contains("fire") && !name.Contains("spark");
    }

    static SceneryObstacle Configure(Transform root, string category, HashSet<Component> allowed, bool undo = false)
    {
        // Potion receivers and the Boss already own their gameplay collision.
        if (root.GetComponentInParent<WitchCauldron>() != null || root.GetComponentInParent<PumpkinBoss>() != null) return null;
        var existing = root.GetComponent<SceneryObstacle>();
        if (existing != null && existing.Solid) return existing;
        var meshes = root.GetComponentsInChildren<MeshFilter>(true).Where(m => Visible(m, root)).ToArray();
        if (meshes.Length == 0) return null;
        if (existing != null) throw new InvalidOperationException("Incomplete scenery collision: " + root.name);
        foreach (var collider in root.GetComponentsInChildren<Collider>(true))
        {
            if (collider.enabled) { allowed.Add(collider); collider.enabled = false; EditorUtility.SetDirty(collider); PrefabUtility.RecordPrefabInstancePropertyModifications(collider); }
        }
        foreach (var body in root.GetComponentsInChildren<Rigidbody>(true))
        {
            allowed.Add(body); body.isKinematic = true; body.useGravity = false; body.detectCollisions = true; EditorUtility.SetDirty(body); PrefabUtility.RecordPrefabInstancePropertyModifications(body);
        }
        var shapes = new List<MeshCollider>();
        foreach (var mesh in meshes)
        {
            // Preserve the artwork's local transform and Ignore Raycast layer.
            // Only the exact mesh surface participates in physics/aim casts.
            var child = new GameObject("Scenery collision - " + category);
            child.transform.SetParent(mesh.transform, false); child.layer = 0;
            if (undo) Undo.RegisterCreatedObjectUndo(child, "Add scenery collision");
            var shape = child.AddComponent<MeshCollider>();
            shape.sharedMesh = mesh.sharedMesh; shape.convex = false; shape.isTrigger = false;
            shapes.Add(shape);
        }
        var obstacle = undo ? Undo.AddComponent<SceneryObstacle>(root.gameObject) : root.gameObject.AddComponent<SceneryObstacle>();
        obstacle.category = category; obstacle.shapes = shapes.ToArray(); EditorUtility.SetDirty(obstacle);
        return obstacle;
    }

    public static SceneryObstacle[] ApplyTo(DreamGame game, HashSet<Component> allowed)
    {
        var roots = new HashSet<Transform>();
        foreach (var mesh in game.GetComponentsInChildren<MeshFilter>(true))
        {
            var prefab = PrefabUtility.GetOutermostPrefabInstanceRoot(mesh.gameObject);
            if (prefab != null && Category(prefab.name) != null) roots.Add(prefab.transform);
        }
        foreach (var t in game.GetComponentsInChildren<Transform>(true))
            if (t.name == "Dream orchard tree" || t.name == "Autumn pumpkin" || t.name == "Abandoned timber watchtower") roots.Add(t);
        foreach (var root in roots)
        {
            bool lanternSkin = root.GetComponentInParent<LanternSkin>() != null;
            if (!root.gameObject.activeInHierarchy && !lanternSkin) continue;
            if (!root.GetComponentsInChildren<MeshFilter>(true).Any(m => Visible(m, root)))
            {
                // Replaced procedural trees must not leave invisible old trunks.
                if (root.name == "Dream orchard tree" || root.name == "Autumn pumpkin")
                    foreach (var c in root.GetComponentsInChildren<Collider>(true))
                        if (c.enabled) { allowed.Add(c); c.enabled = false; EditorUtility.SetDirty(c); }
                continue;
            }
            Configure(root, Category(root.name), allowed);
        }
        Physics.SyncTransforms();
        return game.GetComponentsInChildren<SceneryObstacle>(true);
    }

    [MenuItem("Lost Dream/Physics/Add collision to selected props")]
    public static void ApplySelected()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
        foreach (var root in Selection.transforms)
        {
            Undo.RegisterFullObjectHierarchyUndo(root.gameObject, "Add scenery collision");
            Configure(root, Category(root.name) ?? "Prop", new HashSet<Component>(), true);
            EditorSceneManager.MarkSceneDirty(root.gameObject.scene);
        }
    }

    [MenuItem("Lost Dream/Physics/Apply collision to saved chapters")]
    public static void ApplySavedScenes()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Exit Play mode first.");
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Directory.CreateDirectory(QA);
        var reports = new List<ChapterAudit>();
        foreach (string name in DreamGame.SceneNames)
        {
            string path = "Assets/Scenes/" + name + ".unity";
            var scene = EditorSceneManager.OpenScene(path);
            var game = Object.FindFirstObjectByType<DreamGame>();
            var originals = game.GetComponentsInChildren<Component>(true).Where(c => c != null).ToArray();
            string[] before = originals.Select(Signature).ToArray();
            var allowed = new HashSet<Component>();
            var obstacles = ApplyTo(game, allowed);
            for (int i = 0; i < originals.Length; i++)
                if (!allowed.Contains(originals[i]) && Signature(originals[i]) != before[i])
                    throw new InvalidOperationException("Unrelated component changed: " + name + "/" + originals[i].name);
            if (obstacles.Length == 0 || obstacles.Any(o => !o.Solid)) throw new InvalidOperationException("Missing physical scenery: " + name);
            int shapeCount = obstacles.Sum(o => o.shapes.Length);
            int clearances = ValidateClearances(game);
            // A second pass must not duplicate collider components or alter props.
            var repeated = ApplyTo(game, new HashSet<Component>());
            if (repeated.Length != obstacles.Length || repeated.Sum(o => o.shapes.Length) != shapeCount)
                throw new InvalidOperationException("Collision pass is not idempotent: " + name);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            reports.Add(new ChapterAudit { scene = name, props = obstacles.Length, meshColliders = shapeCount,
                preservedOriginalComponents = originals.Length - allowed.Count, changedPhysicsComponents = allowed.Count,
                clearCharacterPositions = clearances,
                unrelatedComponentsAndTransformsPreserved = true, idempotent = true,
                categories = obstacles.GroupBy(o => o.category).Select(g => g.Key + ": " + g.Count()).OrderBy(s => s).ToArray() });
            Debug.Log("LOST_DREAM_SCENERY_COLLISION: " + name + " props=" + obstacles.Length + " meshes=" + shapeCount);
        }
        File.WriteAllText(QA + "/scene-audit.json", JsonUtility.ToJson(new Audit { passed = true, chapters = reports.ToArray() }, true));
        AssetDatabase.SaveAssets();
    }
    public static void ApplyAndBuild() { ApplySavedScenes(); PrototypeBuilder.BuildCurrentScenes(); }

    static int ValidateClearances(DreamGame game)
    {
        var points = new List<Vector3> { game.spawnPoint };
        points.AddRange(game.lanterns.Select(l => l.safeCheckpoint));
        foreach (var path in game.GetComponentsInChildren<DreamPath>(true).Where(p => p.IsJumpRoute))
        { points.Add(path.jumpEntry.position); points.Add(path.jumpExit.position); }
        foreach (var gate in game.GetComponentsInChildren<GardenIronGate>(true))
            foreach (float offset in new[] { -2f, 0f, 2f }) points.Add(gate.transform.position + gate.transform.forward*offset + Vector3.up*.04f);
        if (game.candySpawnPoints != null)
            points.AddRange(game.candySpawnPoints.Select(p => new Vector3(p.x,p.y-.61f,p.z)));
        foreach (var point in points)
        {
            var obstruction = Physics.OverlapCapsule(point+Vector3.up*.36f,point+Vector3.up*1.74f,.32f,
                ~(1<<2),QueryTriggerInteraction.Ignore).FirstOrDefault(c => c.GetComponentInParent<SceneryObstacle>() != null);
            if (obstruction != null) throw new InvalidOperationException("Scenery blocks a saved spawn/checkpoint/landing/exit/supply position: " +
                game.name + " at " + point + " by " + obstruction.GetComponentInParent<SceneryObstacle>().name);
        }
        return points.Count;
    }

    static string Signature(Component c)
    {
        if (c is Transform t) return t.GetEntityId() + "|" + t.parent?.GetEntityId() + "|" + t.name + "|" +
            t.localPosition.ToString("F9") + "|" + t.localRotation.ToString("F9") + "|" + t.localScale.ToString("F9") + "|" + t.gameObject.activeSelf + "|" + t.gameObject.layer;
        return EditorJsonUtility.ToJson(c);
    }
    [Serializable] class Audit { public bool passed; public ChapterAudit[] chapters; }
    [Serializable] class ChapterAudit
    {
        public string scene; public int props, meshColliders, preservedOriginalComponents, changedPhysicsComponents, clearCharacterPositions;
        public bool unrelatedComponentsAndTransformsPreserved, idempotent; public string[] categories;
    }
}
