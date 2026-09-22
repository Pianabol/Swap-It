using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private LayerMask boardLayer;
    [SerializeField] private float rayDistance = 100f;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    private void HandleClick()
    {
        if (targetCamera == null) return;

        Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red, 2f);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, boardLayer))
        {
            if (hit.collider.TryGetComponent(out Board board))
            {
                Vector2Int gridPos = board.WorldToGrid(hit.point);

                if (board.IsValidCoordinate(gridPos.x, gridPos.y))
                {
                    Debug.Log($"<color=cyan>[BOARD CLICK]</color> Hücre: [{gridPos.x}, {gridPos.y}] | Hit Point: {hit.point}");
                    
                    // YENİ EKLENEN SATIR:
                    board.OnCellClicked(gridPos.x, gridPos.y);
                }
                else
                {
                    Debug.Log($"<color=yellow>[OUT OF BOUNDS]</color> Grid sınırı dışı: [{gridPos.x}, {gridPos.y}]");
                }
            }
        }
        else
        {
            Debug.Log("<color=grey>[NO HIT]</color> Tahtaya isabet etmedi.");
        }
    }
}