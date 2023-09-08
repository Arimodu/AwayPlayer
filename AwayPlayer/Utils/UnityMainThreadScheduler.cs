using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class UnityMainThreadDispatcher : ITickable
{
    [Inject]
    private readonly SiraLog log;

    private readonly Queue<Action> actionQueue = new Queue<Action>();
    private readonly List<EnqueuedTask> delayedActions = new List<EnqueuedTask>();

    public void Tick()
    {
        lock (actionQueue)
        {
            while (actionQueue.Count > 0)
            {
                Action action = actionQueue.Dequeue();
                action.Invoke();
            }
        }

        lock (delayedActions)
        {
            var currentTime = (int)(Time.time * 1000);

            List<EnqueuedTask> actionsToRemove = new List<EnqueuedTask>();

            for (int i = 0; i < delayedActions.Count; i++)
            {
                var action = delayedActions[i];
                if (currentTime >= action.Timeout)
                {
                    //log.Info("Invoking target");
                    action.Invoke();
                    actionsToRemove.Add(action);
                    //log.Info("Target invoked");
                    continue;
                }

                if (action.Callback != null && currentTime >= action.NextCallback)
                {
                    //log.Info("Calling back from main thread");
                    action.InvokeCallback();
                    delayedActions[i].IncrementCallback();
                    //log.Info("Callback finished");
                }
            }

            foreach (var actionToRemove in actionsToRemove)
            {
                delayedActions.Remove(actionToRemove);
            }
        }
    }

    public void Enqueue(Action action)
    {
        lock (actionQueue)
        {
            actionQueue.Enqueue(action);
        }
    }

    public void Enqueue(Func<Task> asyncAction)
    {
        async Task WrapperAsync()
        {
            await asyncAction.Invoke();
        }

        Enqueue(() => WrapperAsync().ConfigureAwait(true));
    }

    public void EnqueueWithDelay(Action target, int delayMilliseconds)
    {
        EnqueueWithDelay(target, delayMilliseconds, null);
    }

    public void EnqueueWithDelay(Action target, int delayMilliseconds, Action<int> callback)
    {
        EnqueueWithDelay(target, delayMilliseconds, callback, 1000); // Default callback interval of 1 second
    }

    public void EnqueueWithDelay(Action target, int delayMilliseconds, Action<int> callback, int callbackInterval)
    {
        int invokeAtMs = ((int)Time.time * 1000) + delayMilliseconds;
        int nextCallback = ((int)Time.time * 1000) + callbackInterval;

        lock (delayedActions)
        {
            delayedActions.Add(new EnqueuedTask(target, invokeAtMs, callback, nextCallback, callbackInterval, invokeAtMs));
        }

        log.Info($"New deffered job enqueued! It will run in {delayMilliseconds} ms and callback every {callbackInterval} ms.");
    }

    public void EnqueueWithDelay(Func<Task> asyncTarget, int delayMilliseconds)
    {
        EnqueueWithDelay(asyncTarget, delayMilliseconds, null);
    }

    public void EnqueueWithDelay(Func<Task> asyncTarget, int delayMilliseconds, Action<int> callback)
    {
        EnqueueWithDelay(asyncTarget, delayMilliseconds, callback, 1000); // Default callback interval of 1 second
    }

    public void EnqueueWithDelay(Func<Task> asyncTarget, int delayMilliseconds, Action<int> callback, int callbackInterval)
    {
        async Task WrapperAsync()
        {
            await asyncTarget.Invoke();
        }

        EnqueueWithDelay(() => WrapperAsync().ConfigureAwait(true), delayMilliseconds, callback, callbackInterval);
    }

    private class EnqueuedTask
    {
        public Action Target { get; private set; }
        public Action<int> Callback { get; private set; }
        public int Timeout { get; private set; }
        public int CallbackInterval { get; private set; }
        public int NextCallback { get; private set; }
        public int EndsAt { get; private set; }

        public EnqueuedTask(Action target, int timeout, Action<int> callback, int nextCallback, int callbackInterval, int endsAt)
        {
            Target = target;
            Callback = callback;
            CallbackInterval = callbackInterval;
            Timeout = timeout;
            NextCallback = nextCallback;
            EndsAt = endsAt;
        }

        public void IncrementCallback() => NextCallback += CallbackInterval;
        public void Invoke() => Target.Invoke();
        public void InvokeCallback() => Callback.Invoke((int)((EndsAt - (Time.time * 1000))/1000));
    }
}
