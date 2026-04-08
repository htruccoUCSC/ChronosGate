using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the pre-wave "enemy pan preview" sequence.
///
/// Before each wave the camera pans right into an off-screen staging area where
/// a small group of static enemy sprites is displayed, representing the *types*
/// of enemies that will appear in the upcoming wave at their approximate spawn
/// ratios.  The camera then pans back to the board and the normal wave begins.
///
/// HOW IT FITS IN:
///   GameLoopManager.StartCombatWithPreview() yields on RunPreview() before
///   telling WaveManager to begin spawning real enemies.
/// </summary>
public class EnemyPreviewManager : MonoBehaviour
{
    // ─── Inspector ─────────────────────────────────────────────────────────────

    [Header("References – auto-found if left empty")]
    [SerializeField] private WaveManager  waveManager;
    [SerializeField] private BoardManager boardManager;

    [Header("Preview Layout")]
    [Tooltip("Extra gap (world units) added between the board's right edge and the first " +
             "preview enemy, on top of the automatic offset that hides the board.")]
    [SerializeField] private float previewAreaPadding = 2f;

    [Tooltip("Maximum enemies shown in the preview. " +
             "Small early waves show fewer (down to the real enemy count).")]
    [SerializeField] private int maxPreviewEnemies = 6;

    [Tooltip("Horizontal gap between enemies in the preview grid.")]
    [SerializeField] private float enemySpacingX = 1.5f;

    [Tooltip("Vertical gap between rows in the preview grid.")]
    [SerializeField] private float enemySpacingY = 1.5f;

    [Tooltip("Enemies per row before wrapping to the next row.")]
    [SerializeField] private int enemiesPerRow = 3;

    [Header("Camera Animation")]
    [Tooltip("Seconds to pan from board → preview area (same duration for the return pan).")]
    [SerializeField] private float panDuration = 1.0f;

    [Tooltip("Seconds the camera holds on the preview before panning back.")]
    [SerializeField] private float holdDuration = 2.5f;

    [Tooltip("Easing curve applied to both camera pans. Default = smooth ease-in-out.")]
    [SerializeField] private AnimationCurve panCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // ─── Private state ─────────────────────────────────────────────────────────

    // All GameObjects spawned for the current preview – destroyed after each use.
    private readonly List<GameObject> _previewObjects = new List<GameObject>();

    // ─── Unity lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        if (waveManager  == null) waveManager  = FindFirstObjectByType<WaveManager>();
        if (boardManager == null) boardManager = FindFirstObjectByType<BoardManager>();
    }

    // ─── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Runs the full preview sequence.  Yields until the pan-out, hold, and
    /// pan-back are all complete, then destroys the preview enemies.
    ///
    /// Usage: yield return StartCoroutine(enemyPreviewManager.RunPreview());
    /// </summary>
    public IEnumerator RunPreview()
    {
        if (Camera.main == null || waveManager == null || boardManager == null)
        {
            Debug.LogWarning("[EnemyPreviewManager] Missing dependency – preview skipped.");
            yield break;
        }

        // ── Step 1 · Board geometry ─────────────────────────────────────────
        //
        // BoardManager places tiles starting at its GameObject's world position.
        // Width × Height gives the grid size in tiles (each tile = 1 world unit).
        // BoardManager.CenterCamera() puts the camera at:
        //   (startX + Width/2,  startY + Height/2,  -10)
        // We replicate that calculation so we know exactly where to pan *back* to.

        float startX  = boardManager.transform.position.x;
        float startY  = boardManager.transform.position.y;
        int   boardW  = boardManager.Width;
        int   boardH  = boardManager.Height;

        Vector3 boardCamPos = new Vector3(
            startX + boardW / 2f,
            startY + boardH / 2f,
            -10f
        );

        // ── Step 2 · Choose which enemy prefabs to display ──────────────────
        //
        // We mirror WaveManager.TrySpawnEnemyOnTile() exactly, running the same
        // random roll N times.  This means the ratio of shadow / colour variants
        // in the preview automatically matches the real spawn probabilities.
        //
        // previewCount scales with wave size:
        //   • early wave (2 enemies)  → show 2 enemies
        //   • large wave (50 enemies) → show up to maxPreviewEnemies (e.g. 6)
        // This way the preview still looks meaningful without being overwhelming.

        int previewCount = Mathf.Clamp(waveManager.enemiesPerWave, 1, maxPreviewEnemies);
        List<GameObject> selectedPrefabs = ChoosePreviewPrefabs(previewCount);

        if (selectedPrefabs.Count == 0)
        {
            Debug.LogWarning("[EnemyPreviewManager] No prefabs assigned on WaveManager – preview skipped.");
            yield break;
        }

        // ── Step 3 · Compute the preview area position ──────────────────────
        //
        // The preview area lives off the right side of the board, completely
        // hidden during normal gameplay.
        //
        // camHalfWidth = how many world units are visible to one side of the camera.
        //
        // Placing the preview origin at:
        //   boardRightEdge + camHalfWidth + previewAreaPadding
        // means that when the camera centres on the preview, the board's right
        // edge is just at (or past) the camera's left edge – the board is hidden.

        float camHalfWidth   = Camera.main.orthographicSize * Camera.main.aspect;
        float boardRightEdge = startX + boardW;
        float previewOriginX = boardRightEdge + camHalfWidth + previewAreaPadding;
        float previewOriginY = startY + boardH / 2f; // vertically centred on the board

        // ── Step 4 · Spawn the static preview enemies ───────────────────────

        Vector3 gridCenter   = SpawnPreviewEnemies(selectedPrefabs, new Vector3(previewOriginX, previewOriginY, 0f));
        Vector3 previewCamPos = new Vector3(gridCenter.x, gridCenter.y, -10f);

        // ── Step 5 · Pan camera: board → preview ────────────────────────────

        yield return StartCoroutine(PanCamera(boardCamPos, previewCamPos, panDuration));

        // ── Step 6 · Hold so the player can inspect the preview ─────────────

        yield return new WaitForSecondsRealtime(holdDuration);

        // ── Step 7 · Pan camera: preview → board ────────────────────────────

        yield return StartCoroutine(PanCamera(previewCamPos, boardCamPos, panDuration));

        // ── Step 8 · Destroy all preview objects ────────────────────────────

        CleanupPreview();
    }

    // ─── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Mirrors the spawn-type selection from WaveManager.TrySpawnEnemyOnTile().
    /// Running the same random roll <paramref name="count"/> times and collecting
    /// the results produces a prefab list whose type ratios match the real wave.
    /// </summary>
    private List<GameObject> ChoosePreviewPrefabs(int count)
    {
        // Build the non-shadow pool the same way WaveManager does.
        List<GameObject> nonShadowPool = new List<GameObject>();

        if (waveManager.enemyPrefabs != null && waveManager.enemyPrefabs.Count > 0)
        {
            // Designer filled the generic list → use it exclusively.
            nonShadowPool.AddRange(waveManager.enemyPrefabs);
        }
        else
        {
            // Fall back to the individually-assigned prefab slots.
            if (waveManager.baseEnemyPrefab   != null) nonShadowPool.Add(waveManager.baseEnemyPrefab);
            if (waveManager.enemyPrefab       != null) nonShadowPool.Add(waveManager.enemyPrefab);
            if (waveManager.enemyRedPrefab    != null) nonShadowPool.Add(waveManager.enemyRedPrefab);
            if (waveManager.enemyYellowPrefab != null) nonShadowPool.Add(waveManager.enemyYellowPrefab);
            if (waveManager.enemyGreenPrefab  != null) nonShadowPool.Add(waveManager.enemyGreenPrefab);
        }

        bool canSpawnShadow = waveManager.shadowEnemyPrefab != null;

        List<GameObject> result = new List<GameObject>(count);
        for (int i = 0; i < count; i++)
        {
            GameObject chosen = null;

            if (canSpawnShadow && Random.value < waveManager.shadowSpawnChance)
            {
                // Same probability check as the real spawn
                chosen = waveManager.shadowEnemyPrefab;
            }
            else if (nonShadowPool.Count > 0)
            {
                chosen = nonShadowPool[Random.Range(0, nonShadowPool.Count)];
            }
            else if (canSpawnShadow)
            {
                // Shadow is the only prefab available
                chosen = waveManager.shadowEnemyPrefab;
            }

            if (chosen != null) result.Add(chosen);
        }

        return result;
    }

    /// <summary>
    /// Instantiates <paramref name="prefabs"/> as static display pieces arranged
    /// in a grid centred on <paramref name="origin"/>.
    ///
    /// Returns the world-space centre of the spawned grid, which is where the
    /// camera should point during the hold phase.
    /// </summary>
    private Vector3 SpawnPreviewEnemies(List<GameObject> prefabs, Vector3 origin)
    {
        int totalEnemies = prefabs.Count;

        // How many columns the grid actually uses (never wider than enemiesPerRow)
        int cols = Mathf.Min(totalEnemies, enemiesPerRow);
        int rows = Mathf.CeilToInt((float)totalEnemies / enemiesPerRow);

        // Total span of the grid, then offset so it's centred on origin
        float gridWidth  = (cols - 1) * enemySpacingX;
        float gridHeight = (rows - 1) * enemySpacingY;

        float startX = origin.x - gridWidth  / 2f;
        float startY = origin.y + gridHeight / 2f; // row 0 is at the top; rows grow downward

        for (int i = 0; i < totalEnemies; i++)
        {
            int col = i % enemiesPerRow;
            int row = i / enemiesPerRow;

            float x = startX + col * enemySpacingX;
            float y = startY - row * enemySpacingY;

            GameObject preview = Instantiate(prefabs[i], new Vector3(x, y, 0f), Quaternion.identity);
            DisableEnemyBehaviours(preview);
            _previewObjects.Add(preview);
        }

        // Return the geometric centre of the grid (z = -10 for camera use)
        return new Vector3(origin.x, origin.y, -10f);
    }

    /// <summary>
    /// Disables every component that would make a preview clone move, fight, or
    /// register itself with WaveManager.  The SpriteRenderer is intentionally
    /// left alone so the enemy still looks correct visually.
    /// </summary>
    private void DisableEnemyBehaviours(GameObject go)
    {
        // Stop physics – the Rigidbody2D is Kinematic, but simulated = false
        // prevents any velocity or force from being applied.
        Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        // Disable the BaseEnemy MonoBehaviour so it can't move left, attack
        // towers, self-register with WaveManager, or trigger its own Awake logic.
        BaseEnemy enemy = go.GetComponent<BaseEnemy>();
        if (enemy != null) enemy.enabled = false;

        // Disable all colliders so preview enemies don't interact with towers.
        foreach (Collider2D col in go.GetComponentsInChildren<Collider2D>())
            col.enabled = false;
    }

    /// <summary>
    /// Smoothly moves the main camera from <paramref name="from"/> to
    /// <paramref name="to"/> over <paramref name="duration"/> seconds.
    ///
    /// Uses Time.unscaledDeltaTime so the animation runs at real-world speed
    /// even when the player has the in-game speed multiplier active.
    /// </summary>
    private IEnumerator PanCamera(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t      = Mathf.Clamp01(elapsed / duration);
            float easedT = panCurve.Evaluate(t);
            Camera.main.transform.position = Vector3.LerpUnclamped(from, to, easedT);
            yield return null;
        }
        // Snap to the exact destination to remove any floating-point drift.
        Camera.main.transform.position = to;
    }

    /// <summary>
    /// Destroys every GameObject that was spawned for the current preview cycle.
    /// </summary>
    private void CleanupPreview()
    {
        foreach (GameObject go in _previewObjects)
        {
            if (go != null) Destroy(go);
        }
        _previewObjects.Clear();
    }
}
