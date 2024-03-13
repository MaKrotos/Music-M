using System.Threading;

namespace VK_UI3.Helpers
{
    public class AutoResetVariable<T>
    {
        private T _value;
        private Timer _timer;

        public T Value
        {
            get { return _value; }
            set
            {
                _value = value;
                _timer.Change(15000, Timeout.Infinite);
            }
        }

        public AutoResetVariable()
        {
            _timer = new Timer(_ => _value = default(T), null, Timeout.Infinite, Timeout.Infinite);
        }
    }

}
