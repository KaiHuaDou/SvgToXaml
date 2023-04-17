#pragma warning disable CA1002
#pragma warning disable CA1045

using System;
using System.Collections.Generic;
using System.Threading;

namespace SvgToXaml.Command;

/// <summary>
/// Handles management and dispatching of EventHandlers in a weak way.
/// </summary>
public static class WeakEventHandlerManager
{
    private static readonly SynchronizationContext SyncContext = SynchronizationContext.Current;

    /// <summary>
    /// Invokes the handlers
    /// </summary>
    /// <param name="o"/>
    /// <param name="handlers"/>
    public static void CallWeakReferenceHandlers(object o, List<WeakReference> handlers)
    {
        if (handlers == null)
            return;
        EventHandler[] callees = new EventHandler[handlers.Count];
        int count = 0;
        int num = CleanupOldHandlers(handlers, callees, count);
        for (int index = 0; index < num; ++index)
            CallHandler(o, callees[index]);
    }

    private static void CallHandler(object o, EventHandler eventHandler)
    {
        if (eventHandler == null) return;
        if (SyncContext != null)
            SyncContext.Post(o => eventHandler(o, EventArgs.Empty), null);
        else eventHandler(o, EventArgs.Empty);
    }

    private static int CleanupOldHandlers(List<WeakReference> handlers, EventHandler[] callees, int count)
    {
        for (int index = handlers.Count - 1; index >= 0; --index)
        {
            if (handlers[index].Target is EventHandler eventHandler)
            {
                callees[count] = eventHandler;
                ++count;
            }
            else
            {
                handlers.RemoveAt(index);
            }
        }
        return count;
    }

    /// <summary>
    /// Adds a handler to the supplied list in a weak way.
    /// </summary>
    /// <param name="handlers">Existing handler list.  It will be created if null.</param><param name="handler">Handler to add.</param><param name="defaultListSize">Default list size.</param>
    public static void AddWeakReferenceHandler(ref List<WeakReference> handlers, EventHandler handler, int defaultListSize)
    {
        handlers ??= defaultListSize > 0 ? new List<WeakReference>(defaultListSize) : new List<WeakReference>( );
        handlers.Add(new WeakReference(handler));
    }

    /// <summary>
    /// Removes an event handler from the reference list.
    /// </summary>
    /// <param name="handlers">Handler list to remove reference from.</param><param name="handler">Handler to remove.</param>
    public static void RemoveWeakReferenceHandler(List<WeakReference> handlers, EventHandler handler)
    {
        if (handlers is null) return;
        for (int index = handlers.Count - 1; index >= 0; --index)
        {
            if (handlers[index].Target is not EventHandler eventHandler || eventHandler == handler)
                handlers.RemoveAt(index);
        }
    }
}
