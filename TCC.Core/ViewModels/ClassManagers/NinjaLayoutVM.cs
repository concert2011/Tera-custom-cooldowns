﻿using TCC.Data;
using TCC.Data.Skills;

namespace TCC.ViewModels
{
    public class NinjaLayoutVM : BaseClassLayoutVM
    {
        private bool _focusOn;

        public Cooldown BurningHeart { get; set; }
        public Cooldown FireAvalanche { get; set; }
        public DurationCooldownIndicator InnerHarmony { get; set; }

        public bool FocusOn
        {
            get => _focusOn;
            set
            {
                if (_focusOn == value) return;
                _focusOn = value;
                N();
            }

        }

        private void FlashOnMaxSt(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StaminaTracker.Maxed))
            {
                BurningHeart.FlashOnAvailable = StaminaTracker.Maxed;
                FireAvalanche.FlashOnAvailable = StaminaTracker.Maxed;
            }
        }

        public override void LoadSpecialSkills()
        {
            SessionManager.DB.SkillsDatabase.TryGetSkill(150700, Class.Ninja, out var bh);
            SessionManager.DB.SkillsDatabase.TryGetSkill(80200, Class.Ninja, out var fa);
            SessionManager.DB.SkillsDatabase.TryGetSkill(230100, Class.Ninja, out var ih);

            BurningHeart = new Cooldown(bh,  false) { CanFlash = true };
            FireAvalanche = new Cooldown(fa,  false) { CanFlash = true };

            InnerHarmony = new DurationCooldownIndicator(Dispatcher)
            {
                Cooldown = new Cooldown(ih, true) {CanFlash = true},
                Buff = new Cooldown(ih, false)
            };

            StaminaTracker.PropertyChanged += FlashOnMaxSt;

        }

        public override void Dispose()
        {
            BurningHeart.Dispose();
            FireAvalanche.Dispose();
            InnerHarmony.Cooldown.Dispose();
            
        }

        public override bool StartSpecialSkill(Cooldown sk)
        {
            if (sk.Skill.IconName == FireAvalanche.Skill.IconName)
            {
                FireAvalanche.Start(sk.Duration);
                return true;
            }
            if (sk.Skill.IconName == BurningHeart.Skill.IconName)
            {
                BurningHeart.Start(sk.Duration);
                return true;
            }
            if (sk.Skill.IconName == InnerHarmony.Cooldown.Skill.IconName)
            {
                InnerHarmony.Cooldown.Start(sk.Duration);
                return true;
            }
            return false;
        }
    }
}
