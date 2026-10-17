using System.Collections;
using System.Linq;
using UnityEngine;

namespace LostDream
{
    public partial class PrototypeSmoke
    {
        IEnumerator SceneryCollisionReview()
        {
            if (!System.Environment.GetCommandLineArgs().Contains("--scenery-contact-smoke"))
            for (int chapter = 0; chapter < 4; chapter++)
            {
                DreamGame.Instance.LoadChapter(chapter); yield return new WaitForSecondsRealtime(.6f);
                var game = DreamGame.Instance; game.player.automation = true;
                var obstacles = game.GetComponentsInChildren<SceneryObstacle>(true);
                Require(obstacles.Length > 0 && obstacles.All(o => o.Solid),"Chapter " + (chapter+1) + " loads its baked solid props");
                Require(obstacles.SelectMany(o => o.shapes).All(c => c.sharedMesh == c.transform.parent.GetComponent<MeshFilter>().sharedMesh &&
                    c.transform.localPosition == Vector3.zero && c.transform.localScale == Vector3.one && c.transform.localRotation == Quaternion.identity),
                    "Collision follows the actual mesh silhouette instead of an oversized bounding box: " + (chapter+1));
                Require(game.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Dream orchard tree" &&
                    t.GetComponentsInChildren<Renderer>(true).All(r => !r.enabled)).All(t => t.GetComponentsInChildren<Collider>(true).All(c => !c.enabled)),
                    "Hidden replaced trees leave no invisible trunk collision: " + (chapter+1));
                Require(game.GetComponentsInChildren<WitchCauldron>(true).All(p => p.GetComponentsInChildren<SceneryObstacle>(true).Length == 0),
                    "Potion receivers keep their authoritative gameplay collider: " + (chapter+1));
                var lamp = game.lanterns[0];
                var skin = lamp.GetComponent<LanternSkin>();
                Require(skin.unlit.GetComponentsInChildren<SceneryObstacle>().Any(o => o.Solid) &&
                    !skin.lit.GetComponentsInChildren<SceneryObstacle>(true).Any(o => o.gameObject.activeInHierarchy),"Unlit pumpkin has collision without enabling the lit variant: " + (chapter+1));
                game.player.Teleport(lamp.safeCheckpoint); yield return new WaitForSeconds(.12f);
                Require(game.player.Interact() && lamp.Lit,"Fitted collision leaves the lantern interaction reachable: " + (chapter+1));
                yield return null;
                Require(skin.lit.GetComponentsInChildren<SceneryObstacle>().Any(o => o.Solid) &&
                    !skin.unlit.GetComponentsInChildren<SceneryObstacle>(true).Any(o => o.gameObject.activeInHierarchy),"Lighting switches the visible collision together with the artwork: " + (chapter+1));
                // Real controller traverses each chapter's existing teaching route.
                var route = game.GetComponentsInChildren<DreamPath>().First(p => p.IsJumpRoute);
                route.Reveal(); yield return JumpRouteChecks(game,route);
            }
            DreamGame.Instance.LoadChapter(0); yield return new WaitForSecondsRealtime(.6f);
            var tutorial = DreamGame.Instance; tutorial.player.automation = true;
            var samples = tutorial.GetComponentsInChildren<SceneryObstacle>().Where(o => o.gameObject.activeInHierarchy).ToArray();
            foreach (string category in new[] { "Tree", "Pumpkin" })
            {
                var sample = samples.Where(o => o.category == category).OrderByDescending(o => o.WorldBounds.size.y).First();
                yield return WalkIntoScenery(sample,category);
            }
            Save(true, ""); Debug.Log("LOST_DREAM_SCENERY_COLLISION_SMOKE_PASSED: " + checks.Count); Application.Quit(0);
        }

        IEnumerator WalkIntoScenery(SceneryObstacle source, string category)
        {
            // Duplicate the real saved prop onto a clear temporary floor so that
            // fences, enemies and island edges cannot produce a false positive.
            var fixture = new GameObject("Temporary actual-mesh collision fixture");
            fixture.transform.position = new Vector3(1000,0,1000);
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube); floor.transform.SetParent(fixture.transform,false);
            floor.transform.localPosition = new Vector3(0,-.5f,0); floor.transform.localScale = new Vector3(30,1,30);
            var copy = Instantiate(source.gameObject,fixture.transform).GetComponent<SceneryObstacle>();
            copy.transform.localScale = source.transform.lossyScale; copy.gameObject.SetActive(true);
            Physics.SyncTransforms(); var bounds = copy.WorldBounds;
            copy.transform.position += fixture.transform.position - new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
            Physics.SyncTransforms(); bounds = copy.WorldBounds;
            var player = DreamGame.Instance.player;
            Ray ray = default; RaycastHit contact = default; bool found = false;
            float y = Mathf.Min(.9f,bounds.size.y*.55f);
            foreach (Vector3 axis in new[] { Vector3.right,Vector3.forward })
            {
                Vector3 side = Vector3.Cross(axis,Vector3.up);
                for (int i = -5; i <= 5 && !found; i++)
                {
                    Vector3 origin = new Vector3(bounds.center.x,y,bounds.center.z) + side*(i*.1f) - axis*(bounds.extents.magnitude+2);
                    Ray candidate = new Ray(origin,axis);
                    if (Physics.Raycast(candidate,out RaycastHit hit,20,~(1<<2),QueryTriggerInteraction.Ignore) &&
                        hit.collider.GetComponentInParent<SceneryObstacle>() == copy)
                    { ray=candidate; contact=hit; found=true; }
                }
                if (found) break;
            }
            Require(found,"A body-height approach ray finds the actual " + category + " surface");
            Vector3 start = contact.point-ray.direction*1.4f; start.y=.04f;
            player.Teleport(start); player.automationMove=Vector3.zero; yield return new WaitForSeconds(.2f);
            player.automationMove=ray.direction;
            var controller=player.GetComponent<CharacterController>(); int contacts=0;
            float peak=player.transform.position.y, end=Time.time+.8f;
            while (Time.time<end)
            {
                peak=Mathf.Max(peak,player.transform.position.y);
                if ((controller.collisionFlags & CollisionFlags.Sides)!=0) contacts++;
                yield return null;
            }
            player.automationMove=Vector3.zero;
            float forward = Vector3.Dot(player.transform.position-contact.point,ray.direction);
            Debug.Log("SCENERY_CONTACT: " + category + " name="+source.name+" contacts="+contacts+" forward="+forward+" rise="+peak+
                " start="+start+" end="+player.transform.position+" contact="+contact.point+" normal="+contact.normal);
            Require(contacts>0,
                "Walking into " + category + " produces actual controller side contacts, blocking or sliding along its surface");
            var camera = DreamGame.Instance.gameCamera; var follow=camera.GetComponent<DreamCamera>(); follow.enabled=false;
            camera.transform.position=ray.origin; camera.transform.rotation=Quaternion.LookRotation(ray.direction);
            Require(Vector3.Distance(player.AimFromScreenPoint(player.AimScreenPoint),contact.point)<.05f,
                "The central reticle selects the same solid " + category + " surface");
            var arrow=DreamProjectile.Spawn(contact.point-ray.direction,ray.direction*31,false);
            yield return new WaitForSeconds(.12f);
            Require(arrow==null,"A real arrow stops at the " + category + " mesh");
            camera.transform.position=player.transform.position+new Vector3(4,3,-5); camera.transform.LookAt(contact.point);
            yield return Capture("solid-"+category.ToLowerInvariant());
            follow.enabled=true;
            if (category=="Tree")
            {
                player.Teleport(start); yield return new WaitForSeconds(.2f);
                Require(player.TryDash(ray.direction),"Real grounded dash starts toward the tree");
                contacts=0; end=Time.time+.3f;
                while (Time.time<end) { if ((controller.collisionFlags & CollisionFlags.Sides)!=0) contacts++; yield return null; }
                Require(contacts>0,"Dashing collides with the actual tree trunk rather than ignoring it");
            }
            else
            {
                player.Teleport(start); yield return new WaitForSeconds(.2f);
                Require(player.TryJump(),"Jump remains available beside a solid pumpkin");
                player.automationMove=ray.direction; peak=0; end=Time.time+1;
                while (Time.time<end) { peak=Mathf.Max(peak,player.transform.position.y); yield return null; }
                player.automationMove=Vector3.zero;
                Require(peak>1.5f && Vector3.Dot(player.transform.position-contact.point,ray.direction)>.5f,
                    "A real jump clears the small pumpkin obstacle");
            }
            player.Teleport(DreamGame.Instance.spawnPoint); Destroy(fixture); yield return null;
        }
    }
}
