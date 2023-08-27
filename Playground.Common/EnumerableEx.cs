using System.Collections.Generic;
using System.Linq;

namespace Playground.Common;

public static class EnumerableEx
{
	public static IEnumerable<T> Excluding<T>(this IEnumerable<T> enumerable, T value)
	{
		return enumerable.TakeWhile(item => item!.Equals(value));
	}
}