using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LostDream
{
    // A small ground grid avoids existing scenery; jumps are linked by TutorialGuide.
    internal sealed class TutorialGroundRoute
    {
        readonly Collider[] floors;
        readonly Dictionary<Vector2Int, Vector3> positions = new Dictionary<Vector2Int, Vector3>();
        readonly HashSet<Vector2Int> blocked = new HashSet<Vector2Int>();
        const int Mask = ~(1 << 2);
        public TutorialGroundRoute(DreamGame game)
        {
            floors = game.GetComponentsInChildren<Collider>(true).Where(x => x.name == "Solid dream island" ||
                x.name == "Bridge collision" || x.name == "Flat round landing surface").ToArray();
        }
        public void Clear() { positions.Clear(); blocked.Clear(); }
        public bool Ground(Vector3 point, out Vector3 ground)
        {
            ground = point; bool found = false;
            Ray ray = new Ray(new Vector3(point.x, 8, point.z), Vector3.down);
            float nearest = float.MaxValue;
            foreach (var floor in floors)
                if (floor != null && floor.enabled && floor.gameObject.activeInHierarchy && floor.Raycast(ray, out RaycastHit hit, 16) && hit.distance < nearest)
                { nearest = hit.distance; ground = hit.point; found = true; }
            return found;
        }
        public bool Clearance(Vector3 point) => !Physics.CheckCapsule(point + Vector3.up * .5f,
            point + Vector3.up * 1.4f, .34f, Mask, QueryTriggerInteraction.Ignore);
        bool Cell(Vector2Int key, out Vector3 point)
        {
            if (positions.TryGetValue(key, out point)) return true;
            if (blocked.Contains(key)) return false;
            if (Ground(new Vector3(key.x,0,key.y), out point) && Clearance(point))
            { positions[key] = point; return true; }
            blocked.Add(key); return false;
        }
        bool Nearest(Vector3 point, out Vector2Int result)
        {
            result = default; float best = float.MaxValue;
            Vector2Int center = new Vector2Int(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.z));
            for (int x = -2; x <= 2; x++) for (int z = -2; z <= 2; z++)
            {
                var key = center + new Vector2Int(x,z);
                if (!Cell(key, out Vector3 candidate)) continue;
                float distance = (candidate - point).sqrMagnitude;
                if (distance < best) { result = key; best = distance; }
            }
            return best < float.MaxValue;
        }
        public List<Vector3> Find(Vector3 from, Vector3 to)
        {
            var route = new List<Vector3>();
            if (!Nearest(from, out Vector2Int start) || !Nearest(to, out Vector2Int end)) return route;
            var open = new List<Vector2Int> { start };
            var closed = new HashSet<Vector2Int>();
            var parent = new Dictionary<Vector2Int, Vector2Int>();
            var cost = new Dictionary<Vector2Int, float> { [start] = 0 };
            int budget = 1800;
            while (open.Count > 0 && budget-- > 0)
            {
                int best = 0;
                for (int i = 1; i < open.Count; i++)
                    if (cost[open[i]] + Vector2Int.Distance(open[i],end) < cost[open[best]] + Vector2Int.Distance(open[best],end)) best = i;
                Vector2Int current = open[best]; open.RemoveAt(best);
                if (current == end)
                {
                    route.Add(positions[current]);
                    while (parent.TryGetValue(current, out Vector2Int previous))
                    { current = previous; route.Add(positions[current]); }
                    route.Reverse(); return route;
                }
                closed.Add(current);
                for (int x = -1; x <= 1; x++) for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && z == 0) continue;
                    var next = current + new Vector2Int(x,z);
                    if (closed.Contains(next) || !Cell(next,out Vector3 point)) continue;
                    Vector3 delta = point - positions[current];
                    if (Mathf.Abs(delta.y) > .35f || Physics.CapsuleCast(positions[current] + Vector3.up * .5f,
                        positions[current] + Vector3.up * 1.4f, .34f, delta.normalized, delta.magnitude, Mask, QueryTriggerInteraction.Ignore)) continue;
                    float tentative = cost[current] + delta.magnitude;
                    if (cost.TryGetValue(next,out float old) && old <= tentative) continue;
                    cost[next] = tentative; parent[next] = current;
                    if (!open.Contains(next)) open.Add(next);
                }
            }
            return route;
        }
    }
}
