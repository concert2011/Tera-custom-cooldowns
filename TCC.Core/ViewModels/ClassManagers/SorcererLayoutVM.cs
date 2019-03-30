﻿using System.Diagnostics;
using TCC.Data;
using TCC.Data.Skills;

namespace TCC.ViewModels
{
    public class SorcererLayoutVM : BaseClassLayoutVM
    {
        public DurationCooldownIndicator ManaBoost { get; set; }

        public Cooldown Fusion { get; set; }
        public Skill PrimeFlame { get; set; }
        public Skill Iceberg { get; set; }
        public Skill ArcaneStorm { get; set; }
        public Skill FusionSkill { get; set; }

        private Skill CurrentFusionSkill
        {
            get
            {
                if (Fire && Ice && Arcane) return FusionSkill;
                if (Fire && Ice) return PrimeFlame;
                if (Ice && Arcane) return Iceberg;
                if (Fire && Arcane) return ArcaneStorm;
                return FusionSkill;
            }
        }

        public bool Fire => SessionManager.CurrentPlayer.Fire;
        public bool Ice => SessionManager.CurrentPlayer.Ice;
        public bool Arcane => SessionManager.CurrentPlayer.Arcane;

        public bool IsBoostFire => SessionManager.CurrentPlayer.FireBoost;
        public bool IsBoostFrost => SessionManager.CurrentPlayer.IceBoost;
        public bool IsBoostArcane => SessionManager.CurrentPlayer.ArcaneBoost;

        public override void LoadSpecialSkills()
        {
            SessionManager.DB.SkillsDatabase.TryGetSkill(340200, Class.Sorcerer, out var mb);
            SessionManager.DB.SkillsDatabase.TryGetSkill(360100, Class.Sorcerer, out var fusion);
            SessionManager.DB.SkillsDatabase.TryGetSkill(360200, Class.Sorcerer, out var primeFlame);
            SessionManager.DB.SkillsDatabase.TryGetSkill(360400, Class.Sorcerer, out var iceberg);
            SessionManager.DB.SkillsDatabase.TryGetSkill(360300, Class.Sorcerer, out var arcaneStorm);

            PrimeFlame = primeFlame; //fire ice
            Iceberg = iceberg; //ice arcane
            ArcaneStorm = arcaneStorm; //fire arcane
            FusionSkill = fusion;

            ManaBoost = new DurationCooldownIndicator(Dispatcher)
            {
                Cooldown = new Cooldown(mb, true) { CanFlash = true },
                Buff = new Cooldown(mb, false)
            };
            Fusion = new Cooldown(fusion, false);

            _sw = new Stopwatch();

        }

        public override void Dispose()
        {
            ManaBoost.Cooldown.Dispose();
        }

        private Stopwatch _sw;
        private long _latestCooldown;

        public override bool StartSpecialSkill(Cooldown sk)
        {

            if (sk.Skill.IconName == ManaBoost.Cooldown.Skill.IconName)
            {
                ManaBoost.Cooldown.Start(sk.Duration);
                return true;
            }
            if (sk.Skill.IconName == PrimeFlame.IconName)
            {
                Fusion.Skill = PrimeFlame;
                Fusion.Start(sk.Duration, sk.Mode);
                if (sk.Mode == CooldownMode.Normal)
                {
                    _latestCooldown = (long)sk.OriginalDuration;
                    _sw.Restart();

                }

                return true;
            }
            if (sk.Skill.IconName == Fusion.Skill.IconName)
            {
                _latestCooldown = (long)sk.OriginalDuration;
                Fusion.Start(sk.Duration, sk.Mode);
                _sw.Restart();
            }
            return false;
        }

        public void EndFireIcePre()
        {
            _sw.Stop();
            Fusion.Start(_latestCooldown > _sw.ElapsedMilliseconds ? (ulong)(_latestCooldown - _sw.ElapsedMilliseconds) : (ulong)_latestCooldown);
        }

        public void NotifyElementChanged()
        {
            N(nameof(Fire));
            N(nameof(Ice));
            N(nameof(Arcane));
            Fusion.Skill = CurrentFusionSkill;
        }

        public void NotifyElementBoostChanged()
        {
            N(nameof(IsBoostFire));
            N(nameof(IsBoostFrost));
            N(nameof(IsBoostArcane));
        }
    }
}
