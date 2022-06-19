namespace Mdl.Threading;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Toolkit.Diagnostics;

/// <summary>
/// Allow to execute an action at the end of a timer.
/// The <see cref="Start" /> method could be called several times during the timer. The action will be executed
/// only one time after the last call.
/// The action can be executed more than one time if timers does not overlap.
/// </summary>
public class DelayedAction
{
    /// <summary>
    /// Default delay, in milliseconds.
    /// </summary>
    public const int DefaultDelay = 1_000;

    private readonly Action<CancellationToken> _action;
    private CancellationTokenSource? _source;

    /// <summary>
    /// Delay an Action.
    /// </summary>
    /// <param name="action">Action to start.</param>
    /// <param name="delay">Delay in milliseconds.</param>
    public DelayedAction(Action<CancellationToken> action, double delay = DefaultDelay)
        : this(action, TimeSpan.FromMilliseconds(delay))
    {
    }

    public DelayedAction(Action<CancellationToken> action, TimeSpan delay)
    {
        Guard.IsNotNull(action, nameof(action));

        _action = action;
        Delay = delay;
    }

    public TimeSpan Delay { get; set; }

    /// <summary>
    /// Stop the delay or the action.
    /// </summary>
    public void Stop()
    {
        ResetInternalState();
    }

    /// <summary>
    /// Start a timer. At the end invoke the action.
    /// </summary>
    public void Start()
    {
        ResetInternalState();
        _source = new CancellationTokenSource();
        CancellationToken cancellationToken = _source.Token;

        Task.Delay(Delay, cancellationToken)
            .ContinueWith(_ => InvokeAction(cancellationToken), cancellationToken);
    }

    private void InvokeAction(CancellationToken cancellationToken)
    {
        try
        {
            _action.Invoke(cancellationToken);
        }
        finally
        {
            ResetInternalState();
        }
    }

    private void ResetInternalState()
    {
        if (_source is null)
        {
            return;
        }

        _source.Cancel();
        _source.Dispose();
        _source = null;
    }
}
