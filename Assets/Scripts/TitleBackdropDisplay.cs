using UnityEngine;

namespace LostDream
{
    // Render the image through the camera's normal color pipeline. Menu controls
    // remain in DreamHUD; this canvas introduces no gameplay interaction or collision.
    public class TitleBackdropDisplay : MonoBehaviour
    {
        DreamGame game;
        Canvas canvas;
        Texture2D image;
        Material material;
        Mesh matte, picture;
        CanvasRenderer matteRenderer, pictureRenderer;
        Vector2 size;

        void Start()
        {
            game = DreamGame.Instance;
            if (game == null || game.Phase != GamePhase.Menu) { enabled = false; return; }
            image = Resources.Load<Texture2D>("UI/TitleBackdrop");
            material = new Material(Resources.Load<Shader>("UI/TitleBackdrop"));
            var root = new GameObject("Start screen artwork", typeof(RectTransform), typeof(Canvas));
            root.transform.SetParent(transform, false); root.layer = 5;
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = game.gameCamera;
            canvas.planeDistance = .3f; canvas.sortingOrder = 1000;
            matte = new Mesh { name = "Cover margins" }; picture = new Mesh { name = "Cover image" };
            matteRenderer = Surface("Matching night backdrop", matte);
            pictureRenderer = Surface("Selected cover composition", picture);
            Canvas.ForceUpdateCanvases();
            Resize();
        }

        CanvasRenderer Surface(string name, Mesh mesh)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.layer = 5; go.transform.SetParent(canvas.transform, false);
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            var renderer = go.GetComponent<CanvasRenderer>();
            renderer.materialCount = 1; renderer.SetMaterial(material, 0); renderer.SetTexture(image); renderer.SetMesh(mesh);
            return renderer;
        }

        void LateUpdate()
        {
            if (canvas == null) return;
            bool visible = game.Phase == GamePhase.Menu;
            if (canvas.gameObject.activeSelf != visible) canvas.gameObject.SetActive(visible);
            if (visible) Resize();
        }

        void Resize()
        {
            Vector2 next = ((RectTransform)canvas.transform).rect.size;
            if (next.x < 1 || next.y < 1 || next == size) return;
            size = next;
            Vector2 corner = new Vector2(.5f / image.width, 1 - .5f / image.height);
            Quad(matte, size, new[] { corner, corner, corner, corner });
            float fit = Mathf.Min(size.x / image.width, size.y / image.height);
            Quad(picture, new Vector2(image.width, image.height) * fit,
                new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right });
            matteRenderer.SetMesh(matte); pictureRenderer.SetMesh(picture);
        }

        static void Quad(Mesh mesh, Vector2 size, Vector2[] uv)
        {
            float x = size.x / 2, y = size.y / 2;
            mesh.Clear();
            mesh.vertices = new[] { new Vector3(-x, -y), new Vector3(-x, y), new Vector3(x, y), new Vector3(x, -y) };
            mesh.uv = uv; mesh.triangles = new[] { 0, 1, 2, 2, 3, 0 }; mesh.RecalculateBounds();
        }

        void OnDestroy()
        {
            if (matte != null) Destroy(matte);
            if (picture != null) Destroy(picture);
            if (material != null) Destroy(material);
        }
    }
}
