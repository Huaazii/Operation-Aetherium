using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Aetherium.Core.Interfaces
{
    public enum ConnectionStage
    {
        Preparing, ConnectingService, JoiningSession, SessionConnected, AwaitingAdmission, RoomReady, Failed, Cancelled,
        CleaningUp
    }

    public enum ConnectionOutcome
    {
        None, Success, UserCancelled, Superseded, TimedOut, Rejected, ConnectionFailed, CleanupFailed
    }

    /// <summary>One user action, one monotonic deadline, shared by transport and room admission.</summary>
    public sealed class ConnectionRequest : IDisposable
    {
        public const double TotalBudgetSeconds = 60;
        public const double TransportBudgetSeconds = 50;
        public const double AdmissionBudgetSeconds = 10;
        public const int MaxAttempts = 2;
        private readonly Func<double> clock;
        private readonly double startedAt;
        private readonly CancellationTokenSource cancellation = new CancellationTokenSource();
        private double admissionStartedAt = -1;
        private ConnectionOutcome cancellationOutcome;

        public ConnectionRequest(Func<double> monotonicClock = null)
        {
            clock = monotonicClock ?? (() => (double)Stopwatch.GetTimestamp() / Stopwatch.Frequency);
            startedAt = clock();
            Token = cancellation.Token;
            cancellation.CancelAfter(TimeSpan.FromSeconds(TotalBudgetSeconds));
        }

        public long OperationId { get; private set; }
        public CancellationToken Token { get; }
        public ConnectionStage Stage { get; set; } = ConnectionStage.Preparing;
        public ConnectionOutcome Outcome { get; private set; }
        public string Detail { get; private set; } = string.Empty;
        public int Attempt { get; set; }
        public int RunnerGeneration { get; set; }
        public string Protocol { get; set; } = string.Empty;
        public bool RelayOnly { get; set; }
        public bool ReusedLobby { get; set; }
        public bool FollowsNetworkSceneTransition { get; set; }
        /// <summary>True when the user action is Create/claim, not Join. Used for status wording.</summary>
        public bool IsCreateEntry { get; set; }
        public bool IsDisposed { get; private set; }
        public bool IsFinished => Outcome != ConnectionOutcome.None;
        public double ElapsedSeconds => Math.Max(0, clock() - startedAt);
        public double RemainingSeconds => Math.Max(0, TotalBudgetSeconds - ElapsedSeconds);
        public double RemainingTransportSeconds => Math.Max(0, TransportBudgetSeconds - ElapsedSeconds);
        public double RemainingAdmissionSeconds => admissionStartedAt < 0 ? 0 :
            Math.Max(0, Math.Min(RemainingSeconds, AdmissionBudgetSeconds - (clock() - admissionStartedAt)));
        public ConnectionOutcome CancellationOutcome => cancellationOutcome != ConnectionOutcome.None
            ? cancellationOutcome : ConnectionOutcome.TimedOut;

        public void BindOperation(long operationId)
        {
            if (OperationId != 0 && OperationId != operationId)
                throw new InvalidOperationException("A connection request cannot change operation identity.");
            OperationId = operationId;
        }

        public void BeginAdmission()
        {
            if (admissionStartedAt < 0) admissionStartedAt = clock();
            Stage = ConnectionStage.AwaitingAdmission;
        }

        public void Cancel(ConnectionOutcome reason = ConnectionOutcome.UserCancelled)
        {
            if (IsDisposed || IsFinished || cancellationOutcome != ConnectionOutcome.None) return;
            cancellationOutcome = reason;
            cancellation.Cancel();
        }

        public void ThrowIfCancelled()
        {
            if (IsDisposed || RemainingSeconds <= 0 || Token.IsCancellationRequested)
                throw new OperationCanceledException(Token);
        }

        public bool TryFinish(ConnectionOutcome outcome, string detail)
        {
            if (IsFinished) return false;
            if (outcome == ConnectionOutcome.Success && (Token.IsCancellationRequested || RemainingSeconds <= 0))
            {
                outcome = CancellationOutcome;
                detail = outcome == ConnectionOutcome.UserCancelled ? "Connection cancelled." :
                    outcome == ConnectionOutcome.Superseded ? "Connection superseded." : "Connection deadline reached.";
            }
            Outcome = outcome;
            Detail = detail ?? string.Empty;
            if (outcome != ConnectionOutcome.Success)
                Stage = outcome == ConnectionOutcome.UserCancelled || outcome == ConnectionOutcome.Superseded
                    ? ConnectionStage.Cancelled : ConnectionStage.Failed;
            return true;
        }

        public CancellationTokenSource CreateTransportCancellation()
        {
            ThrowIfCancelled();
            var source = CancellationTokenSource.CreateLinkedTokenSource(Token);
            source.CancelAfter(TimeSpan.FromSeconds(RemainingTransportSeconds));
            return source;
        }

        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            cancellation.Dispose();
        }

        /// <summary>Enforce a deadline even when an SDK task takes time to observe cancellation.</summary>
        public static async Task<T> AwaitAsync<T>(Task<T> task, CancellationToken token)
        {
            var cancelled = new TaskCompletionSource<bool>();
            using (token.Register(() => cancelled.TrySetResult(true)))
            {
                if (await Task.WhenAny(task, cancelled.Task) != task)
                {
                    ObserveLateFault(task);
                    throw new OperationCanceledException(token);
                }
                if (token.IsCancellationRequested)
                {
                    ObserveLateFault(task);
                    throw new OperationCanceledException(token);
                }
                return await task;
            }
        }

        public static void ObserveLateFault(Task task)
        {
            _ = task.ContinueWith(completed => { var ignored = completed.Exception; },
                CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
        }
    }

    public readonly struct ConnectionProgress
    {
        public readonly long OperationId;
        public readonly ConnectionStage Stage;
        public readonly ConnectionOutcome Outcome;
        public readonly int Attempt;
        public readonly double ElapsedSeconds;
        public readonly double RemainingSeconds;
        public readonly string Message;

        public ConnectionProgress(ConnectionRequest request, string message)
        {
            OperationId = request.OperationId;
            Stage = request.Stage;
            Outcome = request.Outcome;
            Attempt = request.Attempt;
            ElapsedSeconds = request.ElapsedSeconds;
            RemainingSeconds = request.RemainingSeconds;
            Message = message;
        }
    }
}
