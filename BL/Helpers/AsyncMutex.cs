namespace Helpers;

using System.Threading;

/// <summary>
/// A non-blocking mutex intended for periodic/simulator tasks.
/// If an operation is already running, callers can skip the current invocation.
/// </summary>
internal class AsyncMutex
{
 // Interlocked works with int, not bool.
 //0 = false (not in progress),1 = true (in progress)
 private int _inProgress =0;

 /// <summary>
 /// Atomically sets the state to "in progress" only if it is currently not in progress.
 /// </summary>
 /// <returns>
 /// true => it was already in progress (caller should return/skip)
 /// false => acquired successfully (caller should proceed)
 /// </returns>
 internal bool CheckAndSetInProgress() =>
 Interlocked.CompareExchange(ref _inProgress,1,0) == 1;

 /// <summary>
 /// Releases the "in progress" state.
 /// </summary>
 internal void UnsetInProgress() => Interlocked.Exchange(ref _inProgress,0);
}
