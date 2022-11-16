using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace Avalonia.Boilerplate {
    public static class Extensions {
        [DebuggerNonUserCode]
        public static int IndexOf<Type>(this IEnumerable<Type> collection, Predicate<Type> match)
        {
            using (IEnumerator<Type> enumerator = collection.GetEnumerator())
            {
                int num = 0;
                while (enumerator.MoveNext())
                {
                    if (match(enumerator.Current))
                        return num;
                    ++num;
                }
            }
            return -1;
        }
        
        private static Action<Exception> unhandledExceptionHandler;

        public static R ExecuteInUIThread<R>(this Dispatcher dispatcher, Func<R> func, DispatcherPriority priority = DispatcherPriority.Normal) {
            try {
                if (dispatcher.CheckAccess()) {
                    return func();
                }

                return dispatcher.InvokeAsync(func, priority).Result;
            } catch (Exception e) {
                unhandledExceptionHandler?.Invoke(e);
                throw;
            }
        }

        public static void ExecuteInUIThread(this Dispatcher dispatcher, Action action, DispatcherPriority priority = DispatcherPriority.Normal) {
            dispatcher.ExecuteInUIThread(() => { action(); return true; }, priority);
        }

        public static void AsyncExecuteInUIThread(this Dispatcher dispatcher, Action action, DispatcherPriority priority = DispatcherPriority.Normal) {
            dispatcher.InvokeAsync(action, priority)
                .ContinueWith(t => unhandledExceptionHandler?.Invoke(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
