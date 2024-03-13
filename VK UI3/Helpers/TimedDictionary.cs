using System;
using System.Collections.Generic;
using System.Timers;
using Timer = System.Timers.Timer;

namespace VK_UI3.Helpers
{
    public class TimedDictionary<K, V>
    {
        private class TimedValue<T>
        {
            public DateTime LastAccessed { get; private set; }
            public T Value { get; }

            public TimedValue(T value)
            {
                Value = value;
                LastAccessed = DateTime.Now;
            }

            public T GetValueAndUpdateTime(bool updateTime)
            {
                if (updateTime)
                {
                    LastAccessed = DateTime.Now;
                }
                return Value;
            }
        }
        private Timer cleanupTimer;
        private bool updateTime;

        public TimedDictionary(int timeLive = 15000, int checkTime = 1000, bool updateTime = true)
        {
            this.updateTime = updateTime;
            this.timeLive = timeLive;
            cleanupTimer = new Timer(1000);
            cleanupTimer.Elapsed += CleanupTimerElapsed;
            cleanupTimer.Start();
        }
        int timeLive;

        private Dictionary<K, TimedValue<V>> dictionary = new Dictionary<K, TimedValue<V>>();

        public void Add(K key, V value)
        {
            dictionary.Add(key, new TimedValue<V>(value));
        }

        public V this[K key]
        {
            get { return dictionary[key].GetValueAndUpdateTime(updateTime); }
        }

        public DateTime GetLastAccessedTime(K key)
        {
            return dictionary[key].LastAccessed;
        }

        private void CleanupTimerElapsed(object sender, ElapsedEventArgs e)
        {
            RemoveOldItems();
        }

        public void RemoveOldItems()
        {
            DateTime currentTime = DateTime.Now;
            List<K> keysToRemove = new List<K>();

            foreach (var pair in dictionary)
            {
                if ((currentTime - pair.Value.LastAccessed).TotalMinutes > timeLive)
                {
                    keysToRemove.Add(pair.Key);
                }
            }

            foreach (K key in keysToRemove)
            {
                dictionary.Remove(key);
            }
        }

        public bool ContainsKey(K key)
        {
            return dictionary.ContainsKey(key);
        }

        public bool Remove(K key)
        {
            return dictionary.Remove(key);
        }

        public int Count
        {
            get { return dictionary.Count; }
        }

        public ICollection<K> Keys
        {
            get { return dictionary.Keys; }
        }

        public void Clear()
        {
            dictionary.Clear();
        }
    }
}
