using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GMDCore.Tweening;

/// <summary>
/// Lightweight tween/timer system. All tweening is linear interpolation.
/// Callbacks fire on the game thread.
/// </summary>
public sealed class TweenManager
{
    public static readonly TweenManager Instance = new();

    // -------------------------------------------------------------------------
    // Inner task types
    // -------------------------------------------------------------------------

    /// <summary>Tweens one or more float properties over a fixed duration.</summary>
    public sealed class TweenGroup : ITweenTask
    {
        private readonly float _duration;
        private float _elapsed;
        private readonly List<(Action<float> Setter, float From, float To)> _props = new();
        private Action _onComplete;
        public bool IsComplete { get; private set; }

        internal TweenGroup(float duration) => _duration = duration;

        /// <summary>Add a property to tween. Setter receives the interpolated value each frame.</summary>
        public TweenGroup Add(Action<float> setter, float from, float to)
        {
            _props.Add((setter, from, to));
            return this;
        }

        /// <summary>Callback invoked once when the tween finishes.</summary>
        public TweenGroup Finish(Action callback) { _onComplete = callback; return this; }

        void ITweenTask.Update(float dt)
        {
            if (IsComplete) return;
            _elapsed += dt;
            float t = Math.Min(_elapsed / _duration, 1f);
            foreach (var (setter, from, to) in _props)
                setter(from + (to - from) * t);
            if (_elapsed >= _duration)
            {
                IsComplete = true;
                _onComplete?.Invoke();
            }
        }
    }

    /// <summary>Fires a callback once after a delay.</summary>
    public sealed class DelayTask : ITweenTask
    {
        private readonly float _delay;
        private readonly Action _callback;
        private float _elapsed;
        public bool IsComplete { get; private set; }

        internal DelayTask(float delay, Action callback)
        {
            _delay    = delay;
            _callback = callback;
        }

        void ITweenTask.Update(float dt)
        {
            if (IsComplete) return;
            _elapsed += dt;
            if (_elapsed >= _delay)
            {
                IsComplete = true;
                _callback?.Invoke();
            }
        }
    }

    /// <summary>Fires a callback at a regular interval, optionally limited and with a finish callback.</summary>
    public sealed class RepeatTask : ITweenTask
    {
        private readonly float  _interval;
        private readonly Action _callback;
        private Action _onFinish;
        private int    _limit = -1;
        private int    _count;
        private float  _elapsed;
        public bool IsComplete { get; private set; }

        internal RepeatTask(float interval, Action callback)
        {
            _interval = interval;
            _callback = callback;
        }

        public RepeatTask Limit(int n) { _limit = n; return this; }
        public RepeatTask Finish(Action callback) { _onFinish = callback; return this; }

        void ITweenTask.Update(float dt)
        {
            if (IsComplete) return;
            _elapsed += dt;
            while (_elapsed >= _interval)
            {
                _elapsed -= _interval;
                _callback?.Invoke();
                _count++;
                if (_limit >= 0 && _count >= _limit)
                {
                    IsComplete = true;
                    _onFinish?.Invoke();
                    return;
                }
            }
        }
    }

    // -------------------------------------------------------------------------
    // Manager state
    // -------------------------------------------------------------------------

    private readonly List<ITweenTask> _tasks = new();
    private readonly List<ITweenTask> _toAdd = new();

    private TweenManager() { }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>Start a tween group. Chain .Add() calls to specify properties, then .Finish() for callback.</summary>
    public TweenGroup Tween(float duration)
    {
        var group = new TweenGroup(duration);
        _toAdd.Add(group);
        return group;
    }

    /// <summary>Schedule a one-shot callback after <paramref name="delay"/> seconds.</summary>
    public void After(float delay, Action callback) => _toAdd.Add(new DelayTask(delay, callback));

    /// <summary>Schedule a callback that fires every <paramref name="interval"/> seconds.</summary>
    public RepeatTask Every(float interval, Action callback)
    {
        var task = new RepeatTask(interval, callback);
        _toAdd.Add(task);
        return task;
    }

    /// <summary>Remove all active tweens and timers. Call when transitioning between major game modes.</summary>
    public void Clear()
    {
        _tasks.Clear();
        _toAdd.Clear();
    }

    public void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Flush pending registrations (safe: new tasks added by callbacks go in _toAdd)
        _tasks.AddRange(_toAdd);
        _toAdd.Clear();

        // Index-based loop: if Clear() is called inside a callback the loop terminates cleanly.
        for (int i = 0; i < _tasks.Count; i++)
            _tasks[i].Update(dt);

        _tasks.RemoveAll(t => t.IsComplete);
    }
}
