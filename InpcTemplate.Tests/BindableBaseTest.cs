using InpcTemplate;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace InpcTemplate.Tests
{


	/// <summary>
	///This is a test class for BindableBaseTest and is intended
	///to contain all BindableBaseTest Unit Tests
	///</summary>
	[TestClass()]
	public class BindableBaseTest
	{
		private TestBindableType _sut;

		#region Additional test attributes
		// 
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		[ClassInitialize()]
		public static void MyClassInitialize(TestContext testContext)
		{
		}
		//
		//Use ClassCleanup to run code after all tests in a class have run
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//Use TestInitialize to run code before running each test
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}
		//
		//Use TestCleanup to run code after each test has run
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//
		#endregion


		internal virtual BindableBase_Accessor CreateBindableBase_Accessor()
		{
			// TODO: Instantiate an appropriate concrete class.
			BindableBase_Accessor target = null;
			return target;
		}

		/// <summary>
		///A test for OnPropertyChanging
		///</summary>
		[TestMethod()]
		[DeploymentItem("InpcTemplate.dll")]
		public void OnPropertyChangingTest()
		{
			PrivateObject param0 = null; // TODO: Initialize to an appropriate value
			BindableBase_Accessor target = new BindableBase_Accessor(param0); // TODO: Initialize to an appropriate value
			string propertyName = string.Empty; // TODO: Initialize to an appropriate value
			target.OnPropertyChanging(propertyName);
			Assert.Inconclusive("A method that does not return a value cannot be verified.");
		}

		/// <summary>
		///A test for OnPropertyChanged
		///</summary>
		[TestMethod()]
		[DeploymentItem("InpcTemplate.dll")]
		public void OnPropertyChangedTest()
		{
			PrivateObject param0 = null; // TODO: Initialize to an appropriate value
			BindableBase_Accessor target = new BindableBase_Accessor(param0); // TODO: Initialize to an appropriate value
			string propertyName = string.Empty; // TODO: Initialize to an appropriate value
			target.OnPropertyChanged(propertyName);
			Assert.Inconclusive("A method that does not return a value cannot be verified.");
		}

		/// <summary>
		///A test for VerifyCallerIsProperty
		///</summary>
		[TestMethod()]
		[DeploymentItem("InpcTemplate.dll")]
		public void VerifyCallerIsPropertyTest()
		{
			PrivateObject param0 = null; // TODO: Initialize to an appropriate value
			BindableBase_Accessor target = new BindableBase_Accessor(param0); // TODO: Initialize to an appropriate value
			string propertyName = string.Empty; // TODO: Initialize to an appropriate value
			target.VerifyCallerIsProperty(propertyName);
			Assert.Inconclusive("A method that does not return a value cannot be verified.");
		}

		private class TestBindableType : BindableBase
		{

		}
	}
}
