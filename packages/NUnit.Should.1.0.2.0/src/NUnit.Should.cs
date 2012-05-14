using System;
using System.Collections;
using NUnit.Framework.Constraints;

namespace NUnit.Framework {

	#region Aliases to NUnit.Framework classes to improve our BDD syntax
	public class Be      : Is       { public Be(){} }
	public class Have    : Has      { public Have(){} }
	public class Contain : Contains { public Contain(){} }
	#endregion

	/// <summary>
	/// Simple extension methods allowing us to use NUnit constraints as: "foo".Should(Be.StringContaining("o"));
	/// </summary>
	/// <remarks>
	/// ShouldExtensions.Should and ShouldExtensions.ShouldNot are the only methods that are really required 
	/// to give us Should() syntax with NUnit.  We also add a number of Should*() helper methods, however, 
	/// so you can say things like list.ShouldContain("rover") instead of list.Should(Contain.Item("rover"))
	/// </remarks>
	public static partial class ShouldExtensions {
		public static void Should(this object o, IResolveConstraint constraint) {
			Assert.That(o, constraint);
		}
		public static void ShouldNot(this object o, Constraint constraint) {
			Assert.That(o, new NotOperator().ApplyPrefix(constraint));
		}
	}
}
