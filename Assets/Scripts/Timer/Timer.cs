using UnityEngine;
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Object = UnityEngine.Object;

/// <summary>
/// Allows you to run events on a delay without the use of <see cref="Coroutine"/>s
/// or <see cref="MonoBehaviour"/>s.
///
/// 要创建并启动计时器，请使用 <see cref="Register"/> method.
/// </summary>
public class Timer
{
    #region Public Properties/Fields

    /// <summary>
    /// How long the timer takes to complete from start to finish.
    /// </summary>
    public float duration { get; private set; }

    /// <summary>
    /// Whether the timer will run again after completion.
    /// </summary>
    public bool isLooped { get; set; }

    /// <summary>
    /// Whether or not the timer completed running. This is false if the timer was cancelled.
    /// </summary>
    public bool isCompleted { get; private set; }

    /// <summary>
    /// Whether the timer uses real-time or game-time. Real time is unaffected by changes to the timescale
    /// of the game(e.g. pausing, slow-mo), while game time is affected.
    /// </summary>
    public bool usesRealTime { get; private set; }

    /// <summary>
    /// Whether the timer is currently paused.
    /// </summary>
    public bool isPaused
    {
        get { return this._timeElapsedBeforePause.HasValue; }
    }

    /// <summary>
    /// Whether or not the timer was cancelled.
    /// </summary>
    public bool isCancelled
    {
        get { return this._timeElapsedBeforeCancel.HasValue; }
    }

    /// <summary>
    /// Get whether or not the timer has finished running for any reason.
    /// </summary>
    public bool isDone
    {
        get { return this.isCompleted || this.isCancelled || this.isOwnerDestroyed; }
    }

    #endregion

    #region Public Static Methods

    /// <summary>
    /// Register a new timer that should fire an event after a certain amount of time
    /// has elapsed.
    ///
    /// Registered timers are destroyed when the scene changes.
    /// </summary>
    /// <param name="duration">The time to wait before the timer should fire, in seconds.</param>
    /// <param name="onComplete">An action to fire when the timer completes.</param>
    /// <param name="onUpdate">An action that should fire each time the timer is updated. Takes the amount
    /// of time passed in seconds since the start of the timer's current loop.</param>
    /// <param name="isLooped">Whether the timer should repeat after executing.</param>
    /// <param name="useRealTime">Whether the timer uses real-time(i.e. not affected by pauses,
    /// slow/fast motion) or game-time(will be affected by pauses and slow/fast-motion).</param>
    /// <param name="autoDestroyOwner">An object to attach this timer to. After the object is destroyed,
    /// the timer will expire and not execute. This allows you to avoid annoying <see cref="NullReferenceException"/>s
    /// by preventing the timer from running and accessessing its parents' components
    /// after the parent has been destroyed.</param>
    /// <returns>A timer object that allows you to examine stats and stop/resume progress.</returns>
    public static Timer Register(float duration, Action onComplete, Action<float> onUpdate = null,
        bool isLooped = false, bool useRealTime = false, MonoBehaviour autoDestroyOwner = null)
    {
        // create a manager object to update all the timers if one does not already exist.
        if (Timer._manager == null)
        {
            TimerManager managerInScene = Object.FindObjectOfType<TimerManager>();
            if (managerInScene != null)
            {
                Timer._manager = managerInScene;
            }
            else
            {
                GameObject managerObject = new GameObject { name = "TimerManager" };
                Timer._manager = managerObject.AddComponent<TimerManager>();
            }
        }

        Timer timer = new Timer(duration, onComplete, onUpdate, isLooped, useRealTime, autoDestroyOwner);
        Timer._manager.RegisterTimer(timer);
        return timer;
    }

    /// <summary>
    /// Cancels a timer. The main benefit of this over the method on the instance is that you will not get
    /// a <see cref="NullReferenceException"/> if the timer is null.
    /// </summary>
    /// <param name="timer">The timer to cancel.</param>
    public static void Cancel(Timer timer)
    {
        if (timer != null)
        {
            timer.Cancel();
        }
    }

    /// <summary>
    /// Pause a timer. The main benefit of this over the method on the instance is that you will not get
    /// a <see cref="NullReferenceException"/> if the timer is null.
    /// </summary>
    /// <param name="timer">The timer to pause.</param>
    public static void Pause(Timer timer)
    {
        if (timer != null)
        {
            timer.Pause();
        }
    }

    /// <summary>
    /// Resume a timer. The main benefit of this over the method on the instance is that you will not get
    /// a <see cref="NullReferenceException"/> if the timer is null.
    /// </summary>
    /// <param name="timer">The timer to resume.</param>
    public static void Resume(Timer timer)
    {
        if (timer != null)
        {
            timer.Resume();
        }
    }

    public static void CancelAllRegisteredTimers()
    {
        if (Timer._manager != null)
        {
            Timer._manager.CancelAllTimers();
        }

        // if the manager doesn't exist, we don't have any registered timers yet, so don't
        // need to do anything in this case
    }

    public static void PauseAllRegisteredTimers()
    {
        if (Timer._manager != null)
        {
            Timer._manager.PauseAllTimers();
        }

        // if the manager doesn't exist, we don't have any registered timers yet, so don't
        // need to do anything in this case
    }

    public static void ResumeAllRegisteredTimers()
    {
        if (Timer._manager != null)
        {
            Timer._manager.ResumeAllTimers();
        }

        // if the manager doesn't exist, we don't have any registered timers yet, so don't
        // need to do anything in this case
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 停止正在进行或暂停的计时器。计时器的完成回调将不会被调用.
    /// </summary>
    public void Cancel()
    {
        if (this.isDone)
        {
            return;
        }

        this._timeElapsedBeforeCancel = this.GetTimeElapsed();
        this._timeElapsedBeforePause = null;
    }

    /// <summary>
    /// 暂停运行计时器。暂停的计时器可以从暂停的同一点恢复。
    /// </summary>
    public void Pause()
    {
        if (this.isPaused || this.isDone)
        {
            return;
        }

        this._timeElapsedBeforePause = this.GetTimeElapsed();
    }

    /// <summary>
    /// 继续暂停计时器。如果计时器未暂停，则不执行任何操作。
    /// </summary>
    public void Resume()
    {
        if (!this.isPaused || this.isDone)
        {
            return;
        }

        this._timeElapsedBeforePause = null;
    }

    /// <summary>
    /// 获取此计时器当前周期开始后经过的秒数。
    /// </summary>
    /// <returns>此计时器当前周期开始后经过的秒数
    /// 如果计时器是循环的，则为当前循环；如果不是循环的，则为启动。
    ///
    /// 如果计时器已完成运行，则等于持续时间.
    ///
    /// 如果计时器被取消/暂停，则等于计时器之间经过的秒数
    /// 开始以及取消/暂停的时间.</returns>
    public float GetTimeElapsed()
    {
        if (this.isCompleted || this.GetWorldTime() >= this.GetFireTime())
        {
            return this.duration;
        }

        return this._timeElapsedBeforeCancel ??
               this._timeElapsedBeforePause ??
               this.GetWorldTime() - this._startTime;
    }

    /// <summary>
    /// 获取计时器完成前还剩多少秒。
    /// </summary>
    /// <returns>计时器完成前剩余的秒数
    /// 仅当未暂停、取消或完成时才是经过的时间。如果计时器完成才等于零
    /// </returns>
    public float GetTimeRemaining()
    {
        return this.duration - this.GetTimeElapsed();
    }

    /// <summary>
    /// 以比率计算计时器从开始到结束的进度
    /// </summary>
    /// <returns>一个从0到1的值，指示计时器的持续时间已过了多少.</returns>
    public float GetRatioComplete()
    {
        return this.GetTimeElapsed() / this.duration;
    }

    /// <summary>
    /// 获取计时器按比例还剩多少进度.
    /// </summary>
    /// <returns>一个从0到1的值，指示计时器的持续时间还有多长.</returns>
    public float GetRatioRemaining()
    {
        return this.GetTimeRemaining() / this.duration;
    }

    #endregion

    #region Private Static Properties/Fields

    // responsible for updating all registered timers
    private static TimerManager _manager;

    #endregion

    #region Private Properties/Fields

    private bool isOwnerDestroyed
    {
        get { return this._hasAutoDestroyOwner && this._autoDestroyOwner == null; }
    }

    private readonly Action _onComplete;
    private readonly Action<float> _onUpdate;
    private float _startTime;
    private float _lastUpdateTime;

    // for pausing, we push the start time forward by the amount of time that has passed.
    // this will mess with the amount of time that elapsed when we're cancelled or paused if we just
    // check the start time versus the current world time, so we need to cache the time that was elapsed
    // before we paused/cancelled
    private float? _timeElapsedBeforeCancel;
    private float? _timeElapsedBeforePause;

    // after the auto destroy owner is destroyed, the timer will expire
    // this way you don't run into any annoying bugs with timers running and accessing objects
    // after they have been destroyed
    private readonly MonoBehaviour _autoDestroyOwner;
    private readonly bool _hasAutoDestroyOwner;

    #endregion

    #region Private Constructor (use static Register method to create new timer)

    private Timer(float duration, Action onComplete, Action<float> onUpdate,
        bool isLooped, bool usesRealTime, MonoBehaviour autoDestroyOwner)
    {
        this.duration = duration;
        this._onComplete = onComplete;
        this._onUpdate = onUpdate;

        this.isLooped = isLooped;
        this.usesRealTime = usesRealTime;

        this._autoDestroyOwner = autoDestroyOwner;
        this._hasAutoDestroyOwner = autoDestroyOwner != null;

        this._startTime = this.GetWorldTime();
        this._lastUpdateTime = this._startTime;
    }

    #endregion

    #region Private Methods

    private float GetWorldTime()
    {
        return this.usesRealTime ? Time.realtimeSinceStartup : Time.time;
    }

    private float GetFireTime()
    {
        return this._startTime + this.duration;
    }

    private float GetTimeDelta()
    {
        return this.GetWorldTime() - this._lastUpdateTime;
    }

    private void Update()
    {
        if (this.isDone)
        {
            return;
        }

        if (this.isPaused)
        {
            this._startTime += this.GetTimeDelta();
            this._lastUpdateTime = this.GetWorldTime();
            return;
        }

        this._lastUpdateTime = this.GetWorldTime();

        if (this._onUpdate != null)
        {
            this._onUpdate(this.GetTimeElapsed());
        }

        if (this.GetWorldTime() >= this.GetFireTime())
        {

            if (this._onComplete != null)
            {
                this._onComplete();
            }

            if (this.isLooped)
            {
                this._startTime = this.GetWorldTime();
            }
            else
            {
                this.isCompleted = true;
            }
        }
    }

    #endregion

    #region Manager Class (implementation detail, spawned automatically and updates all registered timers)

    /// <summary>
    /// Manages updating all the <see cref="Timer"/>s that are running in the application.
    /// This will be instantiated the first time you create a timer -- you do not need to add it into the
    /// scene manually.
    /// </summary>
    private class TimerManager : MonoBehaviour
    {
        private List<Timer> _timers = new List<Timer>();

        // buffer adding timers so we don't edit a collection during iteration
        private List<Timer> _timersToAdd = new List<Timer>();

        public void RegisterTimer(Timer timer)
        {
            this._timersToAdd.Add(timer);
        }

        public void CancelAllTimers()
        {
            foreach (Timer timer in this._timers)
            {
                timer.Cancel();
            }

            this._timers = new List<Timer>();
            this._timersToAdd = new List<Timer>();
        }

        public void PauseAllTimers()
        {
            foreach (Timer timer in this._timers)
            {
                timer.Pause();
            }
        }

        public void ResumeAllTimers()
        {
            foreach (Timer timer in this._timers)
            {
                timer.Resume();
            }
        }

        // update all the registered timers on every frame
        [UsedImplicitly]
        private void Update()
        {
            this.UpdateAllTimers();
        }

        private void UpdateAllTimers()
        {
            if (this._timersToAdd.Count > 0)
            {
                this._timers.AddRange(this._timersToAdd);
                this._timersToAdd.Clear();
            }

            foreach (Timer timer in this._timers)
            {
                timer.Update();
            }

            this._timers.RemoveAll(t => t.isDone);
        }
    }

    #endregion

}
/*
 * this.AttachTimer(1, () => { Debug.Log("a"); },isLooped:true);    绑定单位注册事件
 * timer = Timer.Register(1, () => { }, isLooped: false);                   注册事件
 * timer.Cancel();  取消
 * timer.Resume();  恢复
 * timer.Pause();   暂停
 */
