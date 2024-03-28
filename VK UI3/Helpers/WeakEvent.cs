using System;
using System.Collections.Generic;
using System.Linq;

public class WeakEventManager
{
    private event EventHandler _event;
    private readonly List<WeakReference> _eventHandlers = new List<WeakReference>();

    public event EventHandler Event
    {
        add
        {
            _eventHandlers.Add(new WeakReference(value));
            _event += value;
        }
        remove
        {
            var toRemove = _eventHandlers.FirstOrDefault(wr => wr.Target.Equals(value));
            if (toRemove != null)
            {
                _eventHandlers.Remove(toRemove);
                _event -= value;
            }
        }
    }

    public void RaiseEvent(object sender, EventArgs e)
    {
        for (int i = _eventHandlers.Count - 1; i >= 0; i--)
        {
            var weakRef = _eventHandlers[i];
            if (weakRef.IsAlive)
            {
                var handler = weakRef.Target as EventHandler;
                handler?.Invoke(sender, e);
                if (!weakRef.IsAlive)
                {
                    _eventHandlers.RemoveAt(i);
                }
            }
            else
            {
                _eventHandlers.RemoveAt(i);
            }
        }
    }

}
