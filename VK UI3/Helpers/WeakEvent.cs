using System;
using System.Collections.Generic;
using System.Linq;

public class WeakEventManager
{
    private readonly List<WeakReference<EventHandler>> _eventHandlers = new List<WeakReference<EventHandler>>();
    private readonly object _lock = new object();

    public void AddHandler(EventHandler handler)
    {
        lock (_lock)
        {
            _eventHandlers.Add(new WeakReference<EventHandler>(handler));
        }
    }

    public void RemoveHandler(EventHandler handler)
    {
        lock (_lock)
        {
            _eventHandlers.RemoveAll(wr => wr.TryGetTarget(out var existingHandler) && existingHandler.Equals(handler));
        }
    }

    public void RaiseEvent(object sender, EventArgs e)
    {
        lock (_lock)
        {
            for (int i = _eventHandlers.Count - 1; i >= 0; i--)
            {
                if (_eventHandlers[i].TryGetTarget(out var handler))
                {
                    handler.Invoke(sender, e);
                }
                else
                {
                    _eventHandlers.RemoveAt(i);
                }
            }
        }
    }
}
