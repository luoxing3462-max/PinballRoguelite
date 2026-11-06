using System;
using System.Collections;
using UnityEngine;

namespace PinballRoguelite
{
    public sealed class GameplayScreen : MonoBehaviour
    {
        public enum CombatTurn
        {
            PlayerInput,
            PlayerLaunch,
            EnemyMove,
            EnemyAttack,
            Result
        }

        public event Action LevelWon;
        public event Action RetryRequested;
        public event Action RestartRunRequested;

        public LevelConfig Config { get; private set; }
        public PinballBoard Board { get; private set; }
        public CombatDisplay Combat { get; private set; }
        public ScoreManager Score { get; } = new ScoreManager();
        public ScoreManager EnemyScore { get; } = new ScoreManager();
        public int ShotsUsed { get; private set; }
        public int ActiveMarbleCount { get; private set; }
        public CombatTurn CurrentTurn { get; private set; }

        private const int MaxSimultaneousMarbles = 8;
        private const float FailedLaunchTimeout = 3.8f;
        [SerializeField] private LaunchPhysicsSettings launchPhysics = new LaunchPhysicsSettings();
        private MapNodeData level;
        private string selectedKind = "normal";
        private bool isCharging;
        private float chargeValue;
        private bool launchInProgress;
        private bool enteredPlayfield;
        private float launchWatchTime;
        private bool resultShown;
        private System.Random enemyRandom;
        private RunEffects effects;
        private bool starRewardClaimed;
        private int totalPlayerLaunches;
        private bool launchSplitPending;
        private MarbleDefinition firstMarble;
        private MarbleDefinition secondMarble;

        private GameplayUiView uiView;

        public void Setup(MapNodeData node, GameplayUiView gameplayUiPrefab, RunEffects runEffects, int mapLayerCount, System.Collections.Generic.IReadOnlyList<string> equippedMarbleIds)
        {
            if (gameplayUiPrefab == null) throw new MissingReferenceException("GameplayUI prefab is required.");
            uiView = Instantiate(gameplayUiPrefab, transform);
            uiView.name = "GameplayUI";
            uiView.Initialize();
            firstMarble = MarbleCatalog.Get(equippedMarbleIds != null && equippedMarbleIds.Count > 0 ? equippedMarbleIds[0] : MarbleCatalog.Normal);
            secondMarble = MarbleCatalog.Get(equippedMarbleIds != null && equippedMarbleIds.Count > 1 ? equippedMarbleIds[1] : MarbleCatalog.Split);
            uiView.ConfigureMarbleButtons(firstMarble, secondMarble);
            uiView.BindInput(() => SelectMarble(firstMarble.Id), () => SelectMarble(secondMarble.Id), StartCharging, ReleaseLaunch);
            Combat = uiView.CombatDisplay;
            level = node;
            effects = runEffects ?? new RunEffects();
            Config = CombatBalanceConfig.CreateLevel(node.Type, node.Layer, mapLayerCount);
            Config.ShotLimit += effects.ShotBonus;
            Score.Reset();
            EnemyScore.Reset();
            ShotsUsed = 0;
            totalPlayerLaunches = 0;
            ActiveMarbleCount = 0;
            selectedKind = firstMarble.Id;
            isCharging = false;
            chargeValue = 0f;
            launchInProgress = false;
            enteredPlayfield = false;
            resultShown = false;
            launchSplitPending = false;
            CurrentTurn = CombatTurn.PlayerInput;
            enemyRandom = new System.Random(System.Guid.NewGuid().GetHashCode());
            Board = new PinballBoard();
            Board.Build(Config, effects);
            Board.StarZone.StarEntered += OnStarEntered;
            Board.SetLauncherLoaded(true, selectedKind);
            Combat.Configure(level.Type, Config.Title, Config.TargetScore, Config.Accent, Config.ShotLimit);
            Combat.ConfigurePlayerHealth(effects.MaximumHealth);
            Combat.SetPlayerHealth(effects.CurrentHealth);
            uiView.SetBoardHint("选择弹珠，按住蓄力：右下上行 → 顶部弧轨左转 → 进入中央");
            UpdateHud();
            UpdateButtons();
        }

        private void Update()
        {
            if (isCharging)
            {
                chargeValue = Mathf.Min(launchPhysics.MaximumChargeTime, chargeValue + Time.deltaTime);
                var chargePercent = launchPhysics.GetChargePercent(chargeValue);
                uiView.SetCharge(chargePercent, true);
                uiView.SetLaunchLabel($"蓄力 {Mathf.RoundToInt(chargePercent)}%\n松开发射");
                uiView.SetBoardHint($"蓄力 {Mathf.RoundToInt(chargePercent)} / 100，松开发射");
            }

            if (!launchInProgress || enteredPlayfield || resultShown || Board == null) return;
            launchWatchTime += Time.deltaTime;
            var returnedToLauncher = false;
            foreach (var marble in Board.ActiveMarbles)
            {
                if (marble == null || !marble.IsActive) continue;
                var position = marble.transform.position;
                if (position.x < PinballBoard.LaunchExitX && position.y > PinballBoard.LaunchExitY)
                {
                    enteredPlayfield = true;
                    if (marble.RequestSplit())
                        uiView.SetBoardHint("出仓分裂！母弹珠已变为 2 颗子弹珠，全部结束后统一结算");
                    else uiView.SetBoardHint("出仓成功，弹珠已沿顶部弧轨进入中央碰撞区");
                    return;
                }
                if (launchWatchTime > 0.5f && position.y < -3.55f && marble.Body.linearVelocity.y < 0f)
                    returnedToLauncher = true;
            }
            if (returnedToLauncher || launchWatchTime >= FailedLaunchTimeout)
                ResetFailedLaunch();
        }

        private void SelectMarble(string kind)
        {
            if (CurrentTurn != CombatTurn.PlayerInput || launchInProgress || resultShown) return;
            selectedKind = kind;
            Board.SetLauncherLoaded(true, selectedKind);
            uiView.SetBoardHint("已装填" + MarbleCatalog.Get(kind).DisplayName + "：" + MarbleCatalog.Get(kind).Description);
            UpdateButtons();
        }

        private void StartCharging()
        {
            if (CurrentTurn != CombatTurn.PlayerInput || launchInProgress || resultShown || ShotsUsed >= Config.ShotLimit) return;
            isCharging = true;
            chargeValue = 0f;
            uiView.SetCharge(0f, true);
        }

        private void ReleaseLaunch()
        {
            if (!isCharging) return;
            isCharging = false;
            Fire(launchPhysics.GetChargePercent(chargeValue));
        }

        private void Fire(float charge)
        {
            if (CurrentTurn != CombatTurn.PlayerInput || launchInProgress || resultShown || ShotsUsed >= Config.ShotLimit) return;
            CurrentTurn = CombatTurn.PlayerLaunch;
            launchInProgress = true;
            enteredPlayfield = false;
            launchWatchTime = 0f;
            ShotsUsed++;
            totalPlayerLaunches++;
            starRewardClaimed = false;
            launchSplitPending = MarbleCatalog.Get(selectedKind).CanSplit;
            ActiveMarbleCount = 0;
            Score.BeginLaunch(CombatBalanceConfig.PlayerBaseAttack, CombatBalanceConfig.BaseCollisionDamage + effects.CollisionBonus);
            var impulse = launchPhysics.EvaluateImpulse(charge);
            Board.SetLauncherLoaded(false, selectedKind);
            SpawnMarble(selectedKind, PinballBoard.LaunchOrigin, Vector2.up * impulse, launchSplitPending);
            uiView.SetLaunchLabel($"发射中\n{Mathf.RoundToInt(charge)}% 力度");
            uiView.SetBoardHint($"弹射力度 {Mathf.RoundToInt(charge)}%，弹珠沿右侧独立通道上行……");
            UpdateHud();
            UpdateButtons();
        }

        private Marble SpawnMarble(string kind, Vector2 position, Vector2 velocity, bool canSplit, int startingScore = 0, float childCollisionMultiplier = 1f)
        {
            if (ActiveMarbleCount >= MaxSimultaneousMarbles) return null;
            var marble = Board.SpawnMarble(kind, position, velocity, canSplit, childCollisionMultiplier);
            marble.BumperHit += OnBumperHit;
            marble.SplitRequested += OnSplitRequested;
            marble.Settled += OnMarbleSettled;
            ActiveMarbleCount++;
            ActiveScore.RegisterMarble(marble.GetInstanceID(), startingScore);
            return marble;
        }

        private void OnBumperHit(Marble marble, int points)
        {
            var collisionDamage = CombatBalanceConfig.BaseCollisionDamage + (CurrentTurn == CombatTurn.EnemyAttack ? 0f : effects.CollisionBonus);
            var attackMultiplier = marble.GetCollisionAttackMultiplier();
            points = Mathf.RoundToInt(collisionDamage * attackMultiplier);
            ActiveScore.AddBumperScore(marble.GetInstanceID(), attackMultiplier);
            var attacker = CurrentTurn == CombatTurn.EnemyAttack ? "怪物弹珠" : "碰撞柱";
            uiView.SetBoardHint($"{attacker} +{points}，本次暂计 {ActiveScore.PendingPreview()}");
            uiView.ShowHitFeedback($"碰撞 +{points}");
            UpdateHud();
        }

        private void OnSplitRequested(Marble parent)
        {
            if (parent == null || !parent.IsActive || ActiveMarbleCount >= MaxSimultaneousMarbles) return;
            var position = (Vector2)parent.transform.position;
            var velocity = parent.Body.linearVelocity;
            var parentDefinition = parent.Definition;
            var carriedScore = ActiveScore.DetachMarble(parent.GetInstanceID());
            parent.ConsumeForSplit();
            Board.Forget(parent);
            ActiveMarbleCount = Mathf.Max(0, ActiveMarbleCount - 1);
            StartCoroutine(SpawnSplitChildren(parentDefinition, position, velocity, carriedScore));
        }

        private IEnumerator SpawnSplitChildren(MarbleDefinition parentDefinition, Vector2 position, Vector2 velocity, int carriedScore)
        {
            yield return new WaitForFixedUpdate();
            var count = 2 + (CurrentTurn == CombatTurn.EnemyAttack ? 0 : effects.ExtraSplitChildren);
            if (resultShown || ActiveMarbleCount > MaxSimultaneousMarbles - count) yield break;
            if (velocity.magnitude < 3f) velocity = Vector2.down * 5.2f;
            for (var index = 0; index < count; index++)
            {
                var spread = count == 1 ? 0f : Mathf.Lerp(-22f, 22f, (float)index / (count - 1));
                var score = index == 0 ? carriedScore : 0;
                SpawnMarble(parentDefinition.Id, position + new Vector2((index - (count - 1) * .5f) * .13f, 0f), Rotate(velocity, spread), false, score, parentDefinition.SplitChildCollisionMultiplier);
            }
            uiView.SetBoardHint(CurrentTurn == CombatTurn.EnemyAttack
                ? "怪物弹珠分裂触发！全部子珠结束后统一结算"
                : "分裂触发！母弹珠已变为 2 颗子弹珠，全部结束后统一结算");
            UpdateHud();
        }

        private static Vector2 Rotate(Vector2 value, float degrees)
        {
            var radians = degrees * Mathf.Deg2Rad;
            var sin = Mathf.Sin(radians);
            var cos = Mathf.Cos(radians);
            return new Vector2(value.x * cos - value.y * sin, value.x * sin + value.y * cos);
        }

        private void OnMarbleSettled(Marble marble, float multiplier)
        {
            if (CurrentTurn != CombatTurn.EnemyAttack && !marble.LastSettlementWasSpecial)
                multiplier = effects.ModifyExitMultiplier(multiplier);
            var gained = ActiveScore.SettleMarble(marble.GetInstanceID(), multiplier);
            Board.Forget(marble);
            ActiveMarbleCount = Mathf.Max(0, ActiveMarbleCount - 1);
            uiView.SetBoardHint(CurrentTurn == CombatTurn.EnemyAttack
                ? $"怪物落入 ×{multiplier:0.##} 区域，暂计 +{gained}"
                : $"落入 ×{multiplier:0.##} 区域，暂计 +{gained}");
            uiView.ShowSettlementFeedback($"本颗结算 +{gained}");
            UpdateHud();
            if (ActiveMarbleCount == 0)
                StartCoroutine(FinishLaunchAfterDelay());
        }

        private IEnumerator FinishLaunchAfterDelay()
        {
            yield return new WaitForSeconds(0.45f);
            if (ActiveMarbleCount != 0 || resultShown) yield break;
            CombatTurn completedTurn = CurrentTurn;
            var gained = ActiveScore.FinishLaunch();
            launchInProgress = false;
            enteredPlayfield = false;
            if (completedTurn == CombatTurn.EnemyAttack)
            {
                gained = effects.ModifyEnemyDamage(CombatBalanceConfig.RollEnemyDamage(Config, enemyRandom));
                CombatBalanceConfig.LogShot(Config, "怪物自动", totalPlayerLaunches, EnemyScore.EffectiveCollisionCount, EnemyScore.ExitMultiplier, 0f, 0f, gained, effects.CurrentHealth);
                uiView.SetBoardHint($"怪物弹珠统一结算 +{gained}，正在攻击玩家");
                yield return Combat.PlayEnemyAttack(gained);
                effects.SyncHealth(Combat.PlayerCurrentHealth);
                if (Combat.PlayerCurrentHealth <= 0)
                {
                    ShowResult(false);
                    yield break;
                }
                if (ShotsUsed >= Config.ShotLimit) ShotsUsed = 0;
                RestorePlayerTurn();
                yield break;
            }

            var permanentBonus = effects.PermanentDamageBonus;
            var nextBonus = effects.NextAttackBonus;
            gained = effects.ResolvePlayerDamage(gained);
            CombatBalanceConfig.LogShot(Config, selectedKind, totalPlayerLaunches, Score.EffectiveCollisionCount, Score.ExitMultiplier, permanentBonus, nextBonus, gained, effects.CurrentHealth);
            if (effects.DangerousSplit)
            {
                effects.TakeDamage(5);
                Combat.SetPlayerHealth(effects.CurrentHealth);
                if (effects.CurrentHealth <= 0) { ShowResult(false); yield break; }
            }
            uiView.SetBoardHint($"本次发射统一结算 +{gained}");
            uiView.ShowDamageFeedback($"本次攻击 +{gained}");
            CurrentTurn = CombatTurn.EnemyMove;
            yield return Combat.PlayAttack(gained, ShotsUsed);
            UpdateHud();
            if (Combat.CurrentHealth <= 0)
            {
                ShowResult(true);
                yield break;
            }
            if (Combat.IsEnemyAtPlayer)
                StartCoroutine(BeginEnemyAttack());
            else
                RestorePlayerTurn();
        }

        private IEnumerator BeginEnemyAttack()
        {
            CurrentTurn = CombatTurn.EnemyAttack;
            isCharging = false;
            chargeValue = 0f;
            uiView.SetCharge(0f, false);
            uiView.SetBoardHint("怪物回合：自动装填普通弹珠……");
            uiView.SetLaunchLabel("怪物回合\n自动发射");
            Board.SetLauncherLoaded(true, "normal");
            UpdateButtons();
            yield return new WaitForSeconds(0.55f);
            if (resultShown || Combat.PlayerCurrentHealth <= 0) yield break;
            launchInProgress = true;
            enteredPlayfield = false;
            launchWatchTime = 0f;
            ActiveMarbleCount = 0;
            EnemyScore.BeginLaunch(CombatBalanceConfig.PlayerBaseAttack, CombatBalanceConfig.BaseCollisionDamage);
            var charge = launchPhysics.GetAutomaticChargePercent(Mathf.Clamp(0.74f + (float)enemyRandom.NextDouble() * 0.16f - 0.08f, 0.62f, 0.88f));
            var impulse = launchPhysics.EvaluateImpulse(charge);
            Board.SetLauncherLoaded(false, "normal");
            SpawnMarble("normal", PinballBoard.LaunchOrigin, Vector2.up * impulse, false);
            uiView.SetBoardHint($"怪物自动发射：{Mathf.RoundToInt(charge)}% 力度，弹珠沿右侧通道上行……");
            uiView.SetLaunchLabel("怪物发射中");
            UpdateHud();
            UpdateButtons();
        }

        private void RestorePlayerTurn()
        {
            CurrentTurn = CombatTurn.PlayerInput;
            Board.SetLauncherLoaded(true, selectedKind);
            chargeValue = 0f;
            uiView.SetCharge(0f, false);
            uiView.SetBoardHint("玩家回合：选择弹珠，按住蓄力后发射");
            UpdateHud();
            UpdateButtons();
        }

        private void ResetFailedLaunch()
        {
            if (!launchInProgress || enteredPlayfield) return;
            Board.DestroyAllMarbles();
            ActiveMarbleCount = 0;
            launchInProgress = false;
            launchWatchTime = 0f;
            if (CurrentTurn == CombatTurn.PlayerLaunch) ShotsUsed = Mathf.Max(0, ShotsUsed - 1);
            ActiveScore.BeginLaunch();
            chargeValue = 0f;
            uiView.SetCharge(0f, false);
            if (CurrentTurn == CombatTurn.EnemyAttack)
            {
                StartCoroutine(BeginEnemyAttack());
                return;
            }
            RestorePlayerTurn();
            uiView.SetBoardHint("弹珠未能成功出仓，本次发射已重置（不消耗次数）");
        }

        private void ShowResult(bool success)
        {
            resultShown = true;
            CurrentTurn = CombatTurn.Result;
            launchInProgress = false;
            Board.SetLauncherLoaded(false, selectedKind);
            UpdateButtons();
            uiView.ShowResult(success, level.Type == MapNodeType.Boss, Score.TotalScore, Config.TargetScore,
                () => LevelWon?.Invoke(), () => RetryRequested?.Invoke(), () => RestartRunRequested?.Invoke());
        }

        private void UpdateHud()
        {
            if (Config == null) return;
            Combat.SetHudState(Score.TotalScore, Score.PendingPreview(), Config.ShotLimit - ShotsUsed);
            Combat.SetHealthFromTotal(Score.TotalScore);
            uiView.SetEffectStatus(effects.StatusText);
        }

        private void UpdateButtons()
        {
            if (Config == null) return;
            var disabled = CurrentTurn != CombatTurn.PlayerInput || launchInProgress || resultShown;
            uiView.SetControls(selectedKind, disabled, CurrentTurn == CombatTurn.PlayerInput && ShotsUsed < Config.ShotLimit);
            if (CurrentTurn == CombatTurn.PlayerInput && !launchInProgress)
                uiView.SetLaunchLabel("按住蓄力\n松开发射");
        }

        private ScoreManager ActiveScore => CurrentTurn == CombatTurn.EnemyAttack ? EnemyScore : Score;

        private void OnStarEntered(Marble marble)
        {
            if (CurrentTurn == CombatTurn.EnemyAttack || starRewardClaimed) return;
            starRewardClaimed = true;
            var rewardNames = new System.Collections.Generic.List<string>();
            var rewardCount = 1 + marble.Definition.ExtraStarRewards;
            for (var index = 0; index < rewardCount; index++)
            {
                var reward = RunEffects.ChooseStarReward(System.Guid.NewGuid().GetHashCode());
                effects.Apply(reward.Id);
                rewardNames.Add(reward.Name);
            }
            Combat.SetPlayerHealth(effects.CurrentHealth);
            uiView.ShowSettlementFeedback("★ 星星奖励：" + string.Join("、", rewardNames));
            UpdateHud();
        }

        private void OnDestroy()
        {
            Board?.Dispose();
        }
    }
}
