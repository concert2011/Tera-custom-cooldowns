﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using TCC.ViewModels;

namespace TCC
{
    public delegate void UpdateBossEnrageEventHandler(ulong id, bool enraged);
    public delegate void UpdateBossHPEventHandler(ulong id, float hp);

}
namespace TCC.Data
{
    public class Boss : TSPropertyChanged, IDisposable
    {
        public ulong EntityId { get; set; }
        string name;
        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                }
            }
        }

        private SynchronizedObservableCollection<AbnormalityDuration> _buffs;
        public SynchronizedObservableCollection<AbnormalityDuration> Buffs
        {
            get { return _buffs; }
            set
            {
                if (_buffs == value) return;
                _buffs = value;
                NotifyPropertyChanged("Buffs");
            }
        }

        //public event PropertyChangedEventHandler PropertyChanged;
        public static event UpdateBossHPEventHandler BossHPChanged;
        public static event UpdateBossEnrageEventHandler EnragedChanged;

        bool enraged;
        public bool Enraged
        {
            get => enraged;
            set
            {
                if (enraged != value)
                {
                    enraged = value;
                    NotifyPropertyChanged("Enraged");
                    EnragedChanged?.Invoke(EntityId, value);
                }
            }
        }
        float _maxHP;
        public float MaxHP
        {
            get => _maxHP;
            set
            {
                if (_maxHP != value)
                {
                    _maxHP = value;
                    NotifyPropertyChanged("MaxHP");
                }
            }
        }
        float _currentHP;
        public float CurrentHP
        {
            get => _currentHP;
            set
            {
                if (_currentHP != value)
                {
                    _currentHP = value;
                    NotifyPropertyChanged("CurrentHP");
                    NotifyPropertyChanged("CurrentPercentage");
                    //BossHPChanged?.Invoke(EntityId, value);
                }
            }
        }

        public float CurrentPercentage => _maxHP == 0 ? 0 : (_currentHP / _maxHP);

        Visibility visible;
        public Visibility Visible
        {
            get { return visible; }
            set
            {
                if (visible != value)
                {
                    visible = value;
                    NotifyPropertyChanged("Visible");
                }
            }
        }

        private ulong target;
        public ulong Target
        {
            get { return target; }
            set
            {
                if (target != value)
                {
                    target = value;
                    NotifyPropertyChanged("Target");
                }
            }
        }

        private AggroCircle currentAggroType = AggroCircle.None;
        public AggroCircle CurrentAggroType
        {
            get { return currentAggroType; }
            set
            {
                if (currentAggroType != value)
                {
                    currentAggroType = value;
                    NotifyPropertyChanged("CurrentAggroType");
                }
            }
        }


        //public void NotifyPropertyChanged(string v)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(v));
        //}

        public void AddorRefresh(AbnormalityDuration ab)
        {
            var existing = Buffs.FirstOrDefault(x => x.Abnormality.Id == ab.Abnormality.Id);
            if (existing == null)
            {
                if (ab.Abnormality.Infinity) Buffs.Insert(0, ab);
                else Buffs.Add(ab);
                return;
            }
            existing.Duration = ab.Duration;
            existing.DurationLeft = ab.DurationLeft;
            existing.Stacks = ab.Stacks;
            existing.Refresh();

        }

        //public bool HasBuff(Abnormality ab)
        //{
        //    if (Buffs.Any(x => x.Abnormality.Id == ab.Id))
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        public void EndBuff(Abnormality ab)
        {
            try
            {
                var buff = Buffs.FirstOrDefault(x => x.Abnormality.Id == ab.Id);
                if (buff == null) return;
                Buffs.Remove(buff);
                buff.Dispose();
            }
            catch (Exception)
            {
                //Console.WriteLine("Cannot remove {0}", ab.Name);
            }
        }

        public Boss(ulong eId, uint zId, uint tId, float curHP, float maxHP, Visibility visible)
        {
            EntityId = eId;
            Name = EntitiesManager.CurrentDatabase.GetName(tId, zId);
            MaxHP = maxHP;
            CurrentHP = curHP;
            Buffs = new SynchronizedObservableCollection<AbnormalityDuration>();
            Visible = visible;
        }

        public Boss(ulong eId, uint zId, uint tId, Visibility visible)
        {
            EntityId = eId;
            Name = EntitiesManager.CurrentDatabase.GetName(tId, zId);
            MaxHP = EntitiesManager.CurrentDatabase.GetMaxHP(tId, zId);
            CurrentHP = MaxHP;
            Buffs = new SynchronizedObservableCollection<AbnormalityDuration>();
            Visible = visible;
        }

        public override string ToString()
        {
            return String.Format("{0} - {1}", EntityId, Name);
        }

        public void Dispose()
        {
            foreach (var buff in _buffs) buff.Dispose();
        }
    }
}
