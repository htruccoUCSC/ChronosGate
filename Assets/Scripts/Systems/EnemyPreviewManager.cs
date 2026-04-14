using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the pre-wave "enemy pan preview" sequence.
///
/// Before each wave the camera pans right into an off-screen staging area that
/// shows a scaled-down sample of the incoming enemies.  The number of preview
/// icons grows with wave size (controlled by previewRatio) up to a set maximum,
/// and each slot is filled using the same random probabilities WaveManager uses
/// when spawning real enemies — so frequent types appear more often and the
/// ratios accurately reflect what the player is about to face.
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

    [Header("Preview Count Scaling")]
    [Tooltip("Fraction of the real enemy count to show in the preview.\n" +
             "E.g. 0.3 means a 10-enemy wave shows 3 preview icons.\n" +
             "Always clamped between Min and Max Preview Enemies.")]
    [Range(0.05f, 1f)]
    [SerializeField] private float previewRatio = 0.3f;

    [Tooltip("Fewest icons ever shown, even on wave 1.")]
    [SerializeField] private int minPreviewEnemies = 1;

    [Tooltip("Most icons ever shown, no matter how large the wave gets.")]
    [SerializeField] private int maxPreviewEnemies = 9;

    [Header("Preview Layout")]
    [Tooltip("Extra gap (world units) added on top of the automatic offset that hides the board.")]
    [SerializeField] private float previewAreaPadding = 12f;

    [Tooltip("Horizontal gap between enemies in the preview grid.")]
    [SerializeField] private float enemySpacingX = 1.5f;

    [Tooltip("Vertical gap between rows in the preview grid.")]
    [SerializeField] private float enemySpacingY = 1.5f;

    [Tooltip("Enemies per row before wrapping to the next row.")]
    [SerializeField] private int enemiesPerRow = 3;

    [Header("Camera Animation")]
    [Tooltip("Seconds the camera stays on the board at the very start of the game " +
             "before panning to the first enemy preview. Has no effect on later waves.")]
    [SerializeField] private float initialBoardViewDelay = 2f;

    [Tooltip("Seconds to pan from board → preview area (and back).")]
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

        // ── Initial board view delay (wave 1 only) ──────────────────────────
        // Give the player a moment to look at their starting board before the
        // camera pans away for the first time.
        if (waveManager.currentWave == 1 && initialBoardViewDelay > 0f)
            yield return new WaitForSecondsRealtime(initialBoardViewDelay);

        // ── Step 1 · Board geometry ─────────────────────────────────────────
        //
        // The board starts at boardManager.transform.position and extends
        // Width tiles right / Height tiles up (1 tile = 1 world unit).
        // We replicate BoardManager.CenterCamera()'s formula so we know exactly
        // where to pan back to at the end.

        float startX = boardManager.transform.position.x;
        float startY = boardManager.transform.position.y;
        int   boardW = boardManager.Width;
        int   boardH = boardManager.Height;

        Vector3 boardCamPos = new Vector3(
            startX + boardW / 2f,
            startY + boardH / 2f,
            -10f
        );

        // ── Step 2 · Refresh the spawnable pool for this wave ───────────────
        //
        // SpawnableManager.UpdateSpawnablesForRound() clears and rebuilds
        // activeSpawnables based on which enemy types have unlocked so far.
        // Calling it here guarantees the list is current even if WaveManager
        // hasn't updated it yet for the incoming wave.

        SpawnableManager sm = waveManager.spawnableManager;
        if (sm != null)
            sm.UpdateSpawnablesForRound(waveManager.currentWave);
        else
            Debug.LogWarning("[EnemyPreviewManager] SpawnableManager not assigned on WaveManager – preview may be incomplete.");

        // ── Step 3 · Decide how many preview icons to show ──────────────────
        //
        // previewCount = ceil(enemiesPerWave * previewRatio), clamped to [min, max].
        //
        // Examples with previewRatio = 0.3:
        //   Wave 1 (2 enemies)  → ceil(0.6) = 1  (hits minPreviewEnemies)
        //   Wave 2 (6 enemies)  → ceil(1.8) = 2
        //   Wave 3 (10 enemies) → ceil(3.0) = 3
        //   Wave 5 (18 enemies) → ceil(5.4) = 6
        //   Wave 8 (30 enemies) → ceil(9.0) = 9  (hits maxPreviewEnemies)
        //
        // After that point the count stays at maxPreviewEnemies regardless of
        // how many more enemies are added, so the preview never becomes crowded.

        int previewCount = Mathf.Clamp(
            Mathf.CeilToInt(waveManager.enemiesPerWave * previewRatio),
            minPreviewEnemies,
            maxPreviewEnemies
        );

        // ── Step 4 · Sample the preview prefabs ─────────────────────────────
        //
        // Each slot is filled by running the same random decision WaveManager
        // uses: roll for shadow vs. non-shadow, then pick randomly from the
        // non-shadow pool.  Running this previewCount times means frequent types
        // appear more often and duplicate icons emerge naturally, just like a
        // real wave — without needing any extra ratio logic.

        List<GameObject> prefabs = SamplePreviewPrefabs(previewCount, sm);

        if (prefabs.Count == 0)
        {
            Debug.LogWarning("[EnemyPreviewManager] No prefabs found – preview skipped.");
            yield break;
        }

        // ── Step 5 · Compute the preview area position ──────────────────────
        //
        // Enemies are placed just to the right of the board with previewAreaPadding
        // as the gap.  The camera centres on the enemy grid, so the board's right
        // edge stays visible on the left side of the frame.

        float boardRightEdge = startX + boardW;
        float previewOriginX = boardRightEdge + previewAreaPadding;
        float previewOriginY = startY + boardH / 2f; // vertically centred on the board

        // ── Step 6 · Spawn the static preview enemies ───────────────────────

        Vector3 gridCenter    = SpawnPreviewEnemies(prefabs, new Vector3(previewOriginX, previewOriginY, 0f));
        Vector3 previewCamPos = new Vector3(gridCenter.x, gridCenter.y, -10f);

        // ── Step 7 · Pan camera: board → preview ────────────────────────────

        yield return StartCoroutine(PanCamera(boardCamPos, previewCamPos, panDuration));

        // ── Step 8 · Hold so the player can inspect the preview ─────────────

        yield return new WaitForSecondsRealtime(holdDuration);

        // ── Step 9 · Pan camera: preview → board ────────────────────────────

        yield return StartCoroutine(PanCamera(previewCamPos, boardCamPos, panDuration));

        // ── Step 10 · Destroy all preview objects ───────────────────────────

        CleanupPreview();
    }

    // ─── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Builds the preview enemy list with two passes:
    /// <list type="number">
    ///   <item>
    ///     <b>Guaranteed pass</b> — one of every type that can spawn this wave is
    ///     added first, so the player always sees the full enemy roster.
    ///   </item>
    ///   <item>
    ///     <b>Random fill</b> — remaining slots (up to the scaled count or
    ///     maxPreviewEnemies) are filled using the same probability roll WaveManager
    ///     uses, so frequent types appear more often.
    ///   </item>
    /// </list>
    /// The final list is shuffled so the guaranteed enemies don't always appear
    /// in the same grid positions.
    /// </summary>
    private List<GameObject> SamplePreviewPrefabs(int requestedCount, SpawnableManager sm)
    {
        // ── Build the non-shadow pool ─────────────────────────────────────────
        // SpawnableManager is the source of truth for which types are unlocked.
        // Fall back to WaveManager's neutral-only fallback if SM is empty, so the
        // preview never shows enemies that can't actually spawn.
        List<GameObject> nonShadowPool = new List<GameObject>();
        if (sm != null && sm.SpawnableList.activeSpawnables.Count > 0)
        {
            nonShadowPool.AddRange(sm.SpawnableList.activeSpawnables);
        }
        else
        {
            if (waveManager.baseEnemyPrefab != null)
                nonShadowPool.Add(waveManager.baseEnemyPrefab);
            else if (waveManager.enemyPrefab != null)
                nonShadowPool.Add(waveManager.enemyPrefab);
        }

        // Mirror WaveManager.TrySpawnEnemyOnTile()'s shadow gate exactly.
        bool canSpawnShadow = waveManager.shadowEnemyPrefab != null
                           && waveManager.currentWave >= waveManager.shadowUnlockWave;

        // ── Pass 1 · Guaranteed set ───────────────────────────────────────────
        // One of each unique type so early rounds always show the full roster.
        List<GameObject> result = new List<GameObject>(nonShadowPool);
        if (canSpawnShadow)
            result.Add(waveManager.shadowEnemyPrefab);

        // The effective total must cover all unique types but is still capped at
        // maxPreviewEnemies so the grid never becomes unreadably large.
        int effectiveCount = Mathf.Clamp(
            Mathf.Max(requestedCount, result.Count),
            result.Count,
            maxPreviewEnemies
        );

        // ── Pass 2 · Random fill ─────────────────────────────────────────────
        // Remaining slots use probability-weighted sampling so the distribution
        // reflects how often each type actually appears in the wave.
        for (int i = result.Count; i < effectiveCount; i++)
        {
            GameObject chosen = null;

            if (canSpawnShadow && Random.value < waveManager.shadowSpawnChance)
                chosen = waveManager.shadowEnemyPrefab;
            else if (nonShadowPool.Count > 0)
                chosen = nonShadowPool[Random.Range(0, nonShadowPool.Count)];
            else if (canSpawnShadow)
                chosen = waveManager.shadowEnemyPrefab;

            if (chosen != null) result.Add(chosen);
        }

        // ── Shuffle ───────────────────────────────────────────────────────────
        // Randomise positions so the guaranteed enemies don't always cluster in
        // the top-left of the grid.
        Shuffle(result);

        return result;
    }

    /// <summary>
    /// Fisher-Yates in-place shuffle using Unity's Random.
    /// </summary>
    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    /// <summary>
    /// Instantiates the prefabs as static display pieces arranged in a centred
    /// grid at <paramref name="origin"/>.  Returns the world-space centre of the
    /// grid (used to aim the camera during the hold phase).
    /// </summary>
    private Vector3 SpawnPreviewEnemies(List<GameObject> prefabs, Vector3 origin)
    {
        int totalEnemies = prefabs.Count;
        int cols = Mathf.Min(totalEnemies, enemiesPerRow);
        int rows = Mathf.CeilToInt((float)totalEnemies / enemiesPerRow);

        // Compute the grid's bounding box, then shift start so it centres on origin.
        float gridWidth  = (cols - 1) * enemySpacingX;
        float gridHeight = (rows - 1) * enemySpacingY;

        float startX = origin.x - gridWidth  / 2f;
        float startY = origin.y + gridHeight / 2f; // rows grow downward from the top

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

        // Return the geometric centre of the grid (z = -10 so it's camera-ready)
        return new Vector3(origin.x, origin.y, -10f);
    }

    /// <summary>
    /// Disables every component that would make a preview clone move, fight, or
    /// register itself with WaveManager.  SpriteRenderer is left enabled so the
    /// enemy still looks correct visually.
    /// </summary>
    private void DisableEnemyBehaviours(GameObject go)
    {
        // Stop physics – the body is Kinematic, but simulated = false prevents
        // any velocity or force from being applied during the preview.
        Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        // Disable the BaseEnemy MonoBehaviour so it can't move left, attack
        // towers, self-register with WaveManager, or run its own Awake logic.
        BaseEnemy enemy = go.GetComponent<BaseEnemy>();
        if (enemy != null) enemy.enabled = false;

        // Disable colliders so preview enemies don't interact with anything.
        foreach (Collider2D col in go.GetComponentsInChildren<Collider2D>())
            col.enabled = false;
    }

    /// <summary>
    /// Smoothly moves the main camera from <paramref name="from"/> to
    /// <paramref name="to"/> over <paramref name="duration"/> seconds.
    /// Uses Time.unscaledDeltaTime so the pan is unaffected by the in-game
    /// speed multiplier.
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
        Camera.main.transform.position = to; // snap to remove floating-point drift
    }

    /// <summary>
    /// Destroys every GameObject spawned for the current preview cycle.
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
