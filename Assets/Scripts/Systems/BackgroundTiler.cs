using UnityEngine;

public class BackgroundTiler : MonoBehaviour
{
    [SerializeField] private Sprite backgroundSprite;
    [SerializeField] private Vector3 tileScale = Vector3.one;
    [SerializeField] private int sortingOrder = -10;
    [SerializeField] private string sortingLayerName = "Default";

    private Transform camTransform;
    private float tileWidth;
    private float tileHeight;
    private int numCols;
    private int numRows;
    private int colHalf;
    private int rowHalf;
    private Transform[] tiles;

    void Start()
    {
        camTransform = Camera.main.transform;

        // Visual tile size = sprite size × applied scale
        tileWidth  = backgroundSprite.bounds.size.x * tileScale.x;
        tileHeight = backgroundSprite.bounds.size.y * tileScale.y;

        Camera cam = Camera.main;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        // Cover full screen + one extra tile on every side
        numCols = Mathf.CeilToInt((halfW * 2f) / tileWidth)  + 2;
        numRows = Mathf.CeilToInt((halfH * 2f) / tileHeight) + 2;
        if (numCols % 2 == 0) numCols++;
        if (numRows % 2 == 0) numRows++;

        colHalf = numCols / 2;
        rowHalf = numRows / 2;

        tiles = new Transform[numCols * numRows];
        int idx = 0;

        for (int col = -colHalf; col <= colHalf; col++)
        {
            for (int row = -rowHalf; row <= rowHalf; row++)
            {
                GameObject tile = new GameObject($"BGTile_{col}_{row}");
                tile.transform.parent = transform;
                tile.transform.localScale = tileScale;

                SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
                sr.sprite = backgroundSprite;
                sr.sortingLayerName = sortingLayerName;
                sr.sortingOrder = sortingOrder;

                tiles[idx++] = tile.transform;
            }
        }
    }

    void LateUpdate()
    {
        // Snap to nearest tile grid point each frame — no drift, always covers screen.
        Vector3 camPos = camTransform.position;
        float snapX = Mathf.Round(camPos.x / tileWidth)  * tileWidth;
        float snapY = Mathf.Round(camPos.y / tileHeight) * tileHeight;

        int idx = 0;
        for (int col = -colHalf; col <= colHalf; col++)
        {
            for (int row = -rowHalf; row <= rowHalf; row++)
            {
                tiles[idx++].position = new Vector3(
                    snapX + col * tileWidth,
                    snapY + row * tileHeight,
                    0f
                );
            }
        }
    }
}
