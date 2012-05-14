using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Constraints;

namespace NUnit.Framework {

	// NUnit.Should setups up our .Should() syntax
	//
	// These are additional .Should*() helper methods to help us 
	// write specs that may be easier to read
	public static partial class ShouldExtensions {

		public static void ShouldEqual<T>(this T a, T b) {
			a.Should(Be.EqualTo(b));
		}

		public static void ShouldNotEqual<T>(this T a, T b) {
			a.ShouldNot(Be.EqualTo(b));
		}

		public static void ShouldContain<T>(this IEnumerable<T> list, T item) {
			list.Should(Contain.Item(item));
		}
		
		public static void ShouldNotContain<T>(this IEnumerable<T> list, T item) {
			list.ShouldNot(Contain.Item(item));
		}

		public static void ShouldContain(this string full, string part) {
			full.Should(Be.StringContaining(part));
		}

		public static void ShouldNotContain(this string full, string part) {
			full.ShouldNot(Be.StringContaining(part));
		}

		public static void ShouldBeFalse(this bool b) {
			b.Should(Be.False);
		}

		public static void ShouldBeTrue(this bool b) {
			b.Should(Be.True);
		}
	}
}
