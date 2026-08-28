#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui.ApplicationModel;
using Xunit;

namespace Tests
{
	// Temporary reproduction/verification test for https://github.com/dotnet/maui/issues/37848
	// AppActions.OnAppAction forwards to a static, process-lifetime singleton with a strong
	// event, so subscribers that never explicitly unsubscribe are retained forever.
	public class AppActions_Leak_Tests
	{
		const int SubjectCount = 30;

		[Fact]
		public void AppActions_OnAppAction_DoesNotLeakSubscribers()
		{
			var control = Create(Scenario.Control);
			var leaky = Create(Scenario.Leaky);
			var mitigation = Create(Scenario.Mitigation);

			ForceGc();

			var controlAlive = CountAlive(control);
			var leakyAlive = CountAlive(leaky);
			var mitigationAlive = CountAlive(mitigation);

			Assert.Equal(0, controlAlive);
			Assert.Equal(0, leakyAlive); // Fails today: 30/30 subjects survive (leak)
			Assert.Equal(0, mitigationAlive);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference[] Create(Scenario scenario)
		{
			var references = new WeakReference[SubjectCount];

			for (var i = 0; i < SubjectCount; i++)
			{
				var subject = new Subject();

				if (scenario != Scenario.Control)
					AppActions.OnAppAction += subject.OnAppAction;

				if (scenario == Scenario.Mitigation)
					AppActions.OnAppAction -= subject.OnAppAction;

				references[i] = new WeakReference(subject);
			}

			return references;
		}

		static int CountAlive(IEnumerable<WeakReference> references) =>
			references.Count(reference => reference.IsAlive);

		static void ForceGc()
		{
			for (var i = 0; i < 7; i++)
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
			}
		}

		enum Scenario
		{
			Control,
			Leaky,
			Mitigation
		}

		sealed class Subject
		{
			readonly byte[] _payload = new byte[1024 * 1024];

			public void OnAppAction(object? sender, AppActionEventArgs args) =>
				GC.KeepAlive(_payload);
		}
	}
}
