using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TCC.Data.Abnormalities;

namespace TCC.Data.Databases
{
    public class AbnormalityDatabase : DatabaseBase
    {
        public readonly Dictionary<uint, Abnormality> Abnormalities;
        public static readonly List<uint> NoctIds = new List<uint> { 902, 910, 911, 912, 913, 916, 917, 999010000 };
        public static List<uint> BlueNoctIds = new List<uint> { 920, 921, 922 };

        protected override string FolderName => "hotdot";
        protected override string Extension => "tsv";

        public AbnormalityDatabase(string lang) : base(lang)
        {
            Abnormalities = new Dictionary<uint, Abnormality>();
        }

        public override void Load()
        {
            Abnormalities.Clear();
            //var hd = File.OpenText(FullPath);
            var lines = File.ReadAllLines(FullPath);
            foreach(var l in lines)
            {
                if (l == null) break;
                if (l == "") continue;
                var s = l.Split('\t');
                var id = uint.Parse(s[0]);
                var type = s[1];
                var amount = double.Parse(s[7], CultureInfo.InvariantCulture);
                var isBuff = bool.Parse(s[3]);
                var isShow = bool.Parse(s[14]);
                var name = s[8];
                var tooltip = s[11];
                var icon = s[13];
                var abType = (AbnormalityType)Enum.Parse(typeof(AbnormalityType), s[2]);
                var infinite = s[5] == "0";
                var ab = new Abnormality(id, isShow, isBuff, infinite, abType);
                //============== by HQ 20190324 ========================
                //ab.SetIcon(icon);
                if(Settings.SettingsHolder.KylosHelper)
                {
                    if ((id == 30190396) || (id == 78200396) || (id == 98200396))       //stack 1
                    {
                        ab.SetIcon(icon + "1");
                    }
                    else if ((id == 30190397) || (id == 78200397) || (id == 98200397))  //stack 2
                    {
                        ab.SetIcon(icon + "2");
                    }
                    else if ((id == 30190398) || (id == 78200398) || (id == 98200398))  //stack 3
                    {
                        ab.SetIcon(icon + "3");
                    }
                    /*
                    else if (id == 70271)   //test
                    {
                        //try { System.Windows.Forms.MessageBox.Show("icon:" + icon + ", icon+3:" + icon + "3"); }
                        //catch { System.Windows.Forms.MessageBox.Show("id==70271"); }
                        //ab.SetIcon("icon_items.pastebait05_tex");
                        ab.SetIcon(icon + "3");
                    }
                    */
                    else
                    {
                        ab.SetIcon(icon);
                    }
                }
                else
                {
                    ab.SetIcon(icon);
                }
                //======================================================
                ab.SetInfo(name, tooltip);
                if (type.IndexOf("Absorb", StringComparison.Ordinal) > -1)
                {
                    ab.SetShield((uint)amount); //TODO: may just parse everything instead of doing this
                }
                if (Abnormalities.TryGetValue(id, out var ex)) //.ContainsKey(id))
                {
                    if (!ex.IsShield && ab.IsShield) Abnormalities[id] = ab;
                    if (ab.Infinity && !ex.Infinity) ex.Infinity = false;
                    if (ex.Type != AbnormalityType.Debuff && ab.Type == AbnormalityType.Debuff) ex.Type = AbnormalityType.Debuff;
                    if (!isBuff) ex.IsBuff = false;
                    continue;
                }
                Abnormalities[id] = ab;
            }

            var meme = new Abnormality(10241024, true, true, true, AbnormalityType.Buff);
            meme.SetInfo("Foglio's aura", "Reduces your ping by $H_W_GOOD80$COLOR_END ms when one of $H_W_GOODFoglio$COLOR_END 's characters is nearby.$BRDoes not stack with Skill prediction.");
            meme.SetIcon("icon_items.bloodchipa_tex");

            Abnormalities[meme.Id] = meme;
        }
    }

}
