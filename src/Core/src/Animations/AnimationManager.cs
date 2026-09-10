using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Animations
{
	/// <inheritdoc/>
	public class AnimationManager : IAnimationManager, IDisposable
	{
		readonly List<Animation> _animations = new();
		readonly object _animationsLock = new();
		long _lastUpdate;
		bool _disposedValue;

		/// <summary>
		/// Instantiate a new <see cref="AnimationManager"/> object.
		/// </summary>
		/// <param name="ticker">An instance of <see cref="ITicker"/> that will be used to time the animations.</param>
		public AnimationManager(ITicker ticker)
		{
			_lastUpdate = GetCurrentTick();

			Ticker = ticker;
			Ticker.Fire = OnFire;
		}

		/// <inheritdoc/>
		public ITicker Ticker { get; }

		/// <inheritdoc/>
		public double SpeedModifier { get; set; } = 1;

		/// <inheritdoc/>
		public bool AutoStartTicker { get; set; } = true;

		/// <inheritdoc/>
		public void Add(Animation animation)
		{
			bool rejected;
			bool shouldStart;
			lock (_animationsLock)
			{
				rejected = _disposedValue || !Ticker.SystemEnabled;
				if (!rejected)
				{
					if (!_animations.Contains(animation))
						_animations.Add(animation);
				}

				shouldStart = !rejected && AutoStartTicker;
			}

			// If this manager cannot run the animation, release any ownership callback.
			if (rejected)
				animation.OnAnimationManagerDisposed(this);
			else if (shouldStart && !Ticker.IsRunning)
				Start();
		}

		/// <inheritdoc/>
		public void Remove(Animation animation)
		{
			if (RemoveAnimation(animation))
				End();
		}

		void Start()
		{
			_lastUpdate = GetCurrentTick();
			Ticker.Start();
		}

		void End() =>
			Ticker?.Stop();

		static long GetCurrentTick() =>
			Environment.TickCount & int.MaxValue;

		void OnFire()
		{
			if (!Ticker.SystemEnabled)
			{
				// This is a hack - if we're here, the ticker has detected that animations are no longer enabled,
				// and it's invoked the Fire event one last time because that's the only communication mechanism
				// it currently has available with the AnimationManager. We need to force all the running animations
				// to move to their finished state and stop running.

				ForceFinishAnimations();
				return;
			}

			var now = GetCurrentTick();
			var milliseconds = TimeSpan.FromMilliseconds(now - _lastUpdate).TotalMilliseconds;
			_lastUpdate = now;

			foreach (var animation in GetAnimationsSnapshot())
			{
				OnAnimationTick(animation);
			}

			if (!HasAnimations())
				End();

			void OnAnimationTick(Animation animation)
			{
				if (animation.HasFinished)
				{
					RemoveAnimation(animation);
					animation.RemoveFromParent();
					return;
				}

				animation.Tick(AdjustSpeed(milliseconds));

				if (animation.HasFinished)
				{
					RemoveAnimation(animation);
					animation.RemoveFromParent();
				}
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			Animation[] animations;
			lock (_animationsLock)
			{
				if (_disposedValue)
					return;

				_disposedValue = true;
				animations = [.._animations];
				_animations.Clear();
			}

			if (disposing)
			{
				foreach (var animation in animations)
				{
					animation.OnAnimationManagerDisposed(this);
				}

				if (Ticker is IDisposable disposable)
					disposable.Dispose();
			}
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		void ForceFinishAnimations()
		{
			foreach (var animation in GetAnimationsSnapshot())
			{
				ForceFinish(animation);
			}

			End();

			void ForceFinish(Animation animation)
			{
				animation.ForceFinish();
				RemoveAnimation(animation);
				animation.RemoveFromParent();
			}
		}

		Animation[] GetAnimationsSnapshot()
		{
			lock (_animationsLock)
			{
				return [.._animations];
			}
		}

		bool RemoveAnimation(Animation animation)
		{
			lock (_animationsLock)
			{
				_animations.TryRemove(animation);
				return _animations.Count == 0;
			}
		}

		bool HasAnimations()
		{
			lock (_animationsLock)
			{
				return _animations.Count > 0;
			}
		}

		internal virtual double AdjustSpeed(double elapsedMilliseconds)
		{
			return elapsedMilliseconds * SpeedModifier;
		}
	}
}