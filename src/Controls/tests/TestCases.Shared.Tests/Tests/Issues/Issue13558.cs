﻿#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue13558 : _IssuesUITest
	{
		public Issue13558(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Android | Picker values are not updtaed in ListView";

		[Test]
		[Category(UITestCategories.ListView)]
		public void Issue13558Test()
		{
			App.WaitForElement("ListViewId");
			VerifyScreenshot();
		}
	}
}
#endif