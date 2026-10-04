using System;
using Aetherium.Core.Interfaces;
using Aetherium.Core.Models;
using Battlefield.Player;
using Battlefield.Runtime;
using Fusion;

namespace Battlefield.Match
{
    public enum ClassChangeResult : byte
    {
        Success, InvalidStage, InvalidPlayer, InvalidClass, OutsideBase, Dead,
        AlreadySelected, Cooldown, StaleRequest, ActionInProgress, ResourcesNotReady, Failed,
        InCombat, RateLimited
    }

    public sealed partial class BattlefieldMatchState
    {
        public const float ClassChangeIntervalSeconds = 5f;
        public const float ClassChangeOutOfCombatSeconds = 3f;
        public event Action<int, ClassChangeResult> OnClassChangeResult;

        // Two requests per second per persistent seat, including rejected requests.
        private struct ClassChangeRequestBudget
        {
            public long Token;
            public int Round;
            public int Remaining;
            public TickTimer RefillTimer;
        }

        private readonly ClassChangeRequestBudget[] classChangeRequestBudgets = new ClassChangeRequestBudget[MaxSeats];

        private bool TryConsumeClassChangeRequest(PlayerRef requester)
        {
            if (!CanWriteState || Runner == null || !requester.IsRealPlayer) return false;
            int index = FindSeatByPlayer(requester);
            if (index < 0 || !Seats[index].IsConnected || Seats[index].ConnectionTokenId == 0L) return false;
            ClassChangeRequestBudget budget = classChangeRequestBudgets[index];
            long token = Seats[index].ConnectionTokenId;
            if (budget.Token != token || budget.Round != RoundId || budget.RefillTimer.ExpiredOrNotRunning(Runner))
                budget = new ClassChangeRequestBudget
                {
                    Token = token, Round = RoundId, Remaining = 2,
                    RefillTimer = TickTimer.CreateFromSeconds(Runner, 1f)
                };
            if (budget.Remaining <= 0) return false;
            budget.Remaining--;
            classChangeRequestBudgets[index] = budget;
            return true;
        }

        public bool SubmitBaseClassChange(PlayerClassId classId)
        {
            if (Object == null || !Object.IsValid || Runner == null) return false;
            int index = FindSeatByPlayer(Runner.LocalPlayer);
            if (index < 0) return false;
            int version = Seats[index].ClassChangeVersion;
            if (CanWriteState)
            {
                ClassChangeResult result = TryConsumeClassChangeRequest(Runner.LocalPlayer)
                    ? TryApplyBaseClassChange(Runner.LocalPlayer, classId, RoundId, version) : ClassChangeResult.RateLimited;
                OnClassChangeResult?.Invoke(version, result);
            }
            else RpcSubmitBaseClassChange((byte)classId, RoundId, version);
            return true;
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RpcSubmitBaseClassChange(byte classId, int roundId, int version, RpcInfo info = default)
        {
            // Drop excess traffic without generating more response traffic.
            if (!TryConsumeClassChangeRequest(info.Source)) return;
            ClassChangeResult result = TryApplyBaseClassChange(info.Source, (PlayerClassId)classId, roundId, version);
            RpcBaseClassChangeResult(info.Source, roundId, version, (byte)result);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RpcBaseClassChangeResult([RpcTarget] PlayerRef requester, int roundId, int version, byte result)
        {
            if (Runner != null && Runner.LocalPlayer == requester && RoundId == roundId)
                OnClassChangeResult?.Invoke(version, (ClassChangeResult)result);
        }

        public ClassChangeResult TryApplyBaseClassChange(PlayerRef requester, PlayerClassId target,
            int expectedRound, int expectedVersion)
        {
            if (!CanWriteState || CurrentStage != MatchStage.InMatch) return ClassChangeResult.InvalidStage;
            if (!IsPlayableClass(target)) return ClassChangeResult.InvalidClass;
            int index = requester.IsRealPlayer ? FindSeatByPlayer(requester) : -1;
            if (index < 0) return ClassChangeResult.InvalidPlayer;
            PlayerSeatState seat = Seats[index];
            if (!seat.IsConnected || !seat.IsLocked || seat.ConnectionTokenId == 0L)
                return ClassChangeResult.InvalidPlayer;
            if (expectedRound != RoundId || expectedVersion != seat.ClassChangeVersion)
                return ClassChangeResult.StaleRequest;
            if (seat.ClassId == (byte)target) return ClassChangeResult.AlreadySelected;
            IBattlefieldSceneRoot root = BattlefieldSceneRootProvider.Instance;
            if (root == null || !root.TryGetPlayerByToken(seat.ConnectionTokenId, out var component) ||
                !(component is BattlefieldPlayer source) || source.Object == null ||
                !source.Object.IsValid || source.Object.InputAuthority != requester)
                return ClassChangeResult.InvalidPlayer;
            if (!source.IsAlive) return ClassChangeResult.Dead;
            if (!BattlefieldClassChangeZone.Contains((TeamIdentity)seat.Team, source.transform.position))
                return ClassChangeResult.OutsideBase;
            if (!source.HealthModule.ClassChangeCombatTimer.ExpiredOrNotRunning(Runner)) return ClassChangeResult.InCombat;
            if (!seat.ClassChangeTimer.ExpiredOrNotRunning(Runner)) return ClassChangeResult.Cooldown;
            FusionWeaponController weapons = source.GetComponent<FusionWeaponController>();
            if (weapons == null || weapons.IsWeaponSwitchLocked) return ClassChangeResult.ActionInProgress;
            FusionAbilityController ability = source.GetComponent<FusionAbilityController>();
            if (ability == null || root.Bootstrap == null) return ClassChangeResult.ResourcesNotReady;

            TickTimer cooldown = ability.GetClassChangeCooldown(seat.SharedAbilityCooldown);
            ClassChangeResult replacementResult = root.Bootstrap.TryReplacePlayerClass(source, target,
                seat.ClassChangeVersion + 1, cooldown, out BattlefieldPlayer replacement);
            if (replacementResult != ClassChangeResult.Success) return replacementResult;

            // Read the seat again: replacement must never overwrite concurrent combat statistics.
            seat = Seats[index];
            seat.ClassId = (byte)target;
            seat.ClassChangeVersion++;
            seat.SharedAbilityCooldown = cooldown;
            seat.ClassChangeTimer = TickTimer.CreateFromSeconds(Runner, ClassChangeIntervalSeconds);
            WriteSeat(index, seat);
            replacement.CommitClassChangeReplacement();
            Runner.SetPlayerObject(requester, replacement.Object);
            Runner.Despawn(source.Object);
            return ClassChangeResult.Success;
        }

        public TickTimer GetSharedAbilityCooldown(long token)
        {
            return TryGetPlayerSeatByToken(token, out var seat, out _) ? seat.SharedAbilityCooldown : default;
        }

        public void SetSharedAbilityCooldown(long token, TickTimer cooldown)
        {
            if (!CanWriteState) return;
            // Team-relative spawn slot is not the index in the six-seat network array.
            int index = FindSeatByToken(token);
            if (index < 0) return;
            PlayerSeatState seat = Seats[index];
            seat.SharedAbilityCooldown = cooldown;
            Seats.Set(index, seat);
        }
    }
}
