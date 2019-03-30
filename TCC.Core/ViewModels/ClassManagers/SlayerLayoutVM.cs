﻿using TCC.Data;
using TCC.Data.Skills;

namespace TCC.ViewModels
{
    public class SlayerLayoutVM : BaseClassLayoutVM
    {

        public DurationCooldownIndicator InColdBlood { get; set; }
        
        public Cooldown OverhandStrike { get; set; }
        


        public override void LoadSpecialSkills()
        {
            // In Cold Blood
            SessionManager.DB.SkillsDatabase.TryGetSkill(200200, Class.Slayer, out var icb);
            InColdBlood = new DurationCooldownIndicator(Dispatcher) {
                Buff = new Cooldown(icb, false),
                Cooldown = new Cooldown(icb, true) { CanFlash = true }
            };

            // Overhand Strike
            SessionManager.DB.SkillsDatabase.TryGetSkill(80900, Class.Slayer, out var ohs);
            OverhandStrike = new Cooldown(ohs, false);

        }

        public override void Dispose()
        {
            InColdBlood.Cooldown.Dispose();
        }

        public override bool StartSpecialSkill(Cooldown sk)
        {
            if (sk.Skill.IconName == InColdBlood.Cooldown.Skill.IconName)
            {
                InColdBlood.Cooldown.Start(sk.Duration);
                return true;
            }
            if (sk.Skill.IconName == OverhandStrike.Skill.IconName)
            {
                OverhandStrike.Start(sk.Duration);
                return true;
            }
            
            return false;
        }
    }
}
