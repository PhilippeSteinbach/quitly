/**
 * Monotonic time store for manipulation-resistant streak display.
 *
 * Uses performance.now() (monotonic, never goes backwards) as the drift reference,
 * anchored against a server UTC timestamp. Snapshots survive React re-mounts via
 * localStorage. Stale snapshots (> 24 h old) are discarded so the guard does not
 * fire on first load after a long break.
 */

export interface MonotonicSnapshot {
  /** Server-authoritative UTC epoch (ms) at the time of the last sync. */
  serverUtcMs: number;
  /** performance.now() value (ms) at the time of the last sync. */
  performanceNowMs: number;
  /** Wall-clock epoch (ms) when the snapshot was persisted — used for staleness check. */
  savedAtMs: number;
}

const STORAGE_KEY = 'quitly_monotonic_snapshot';
const STALE_GUARD_MS = 24 * 60 * 60 * 1000; // 24 hours

// ── saveSnapshot ─────────────────────────────────────────────────────────────

/**
 * Persists the current monotonic anchor to localStorage.
 */
export function saveSnapshot(serverUtcMs: number): void {
  const snapshot: MonotonicSnapshot = {
    serverUtcMs,
    performanceNowMs: performance.now(),
    savedAtMs: Date.now(),
  };
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(snapshot));
  } catch {
    // localStorage may be unavailable in private mode — fail silently
  }
}

// ── loadSnapshot ─────────────────────────────────────────────────────────────

/**
 * Returns the stored snapshot, or null if missing or older than 24 h.
 */
export function loadSnapshot(): MonotonicSnapshot | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;

    const snapshot = JSON.parse(raw) as MonotonicSnapshot;
    if (Date.now() - snapshot.savedAtMs > STALE_GUARD_MS) {
      localStorage.removeItem(STORAGE_KEY);
      return null;
    }
    return snapshot;
  } catch {
    return null;
  }
}

// ── getNowUtcMs ──────────────────────────────────────────────────────────────

/**
 * Returns the current UTC epoch (ms) derived from the monotonic snapshot so the
 * display cannot be influenced by device clock changes between server syncs.
 *
 * If no snapshot is available, falls back to Date.now() (first load).
 */
export function getNowUtcMs(): number {
  const snapshot = loadSnapshot();
  if (!snapshot) return Date.now();

  const elapsed = performance.now() - snapshot.performanceNowMs;
  return Math.round(snapshot.serverUtcMs + elapsed);
}
