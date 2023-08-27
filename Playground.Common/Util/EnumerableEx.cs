namespace Playground.Common;

public static class EnumerableEx
{
	public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable)
	{
		return enumerable
			.Where(x => x is not null)
			.Select(x => x!);
	}
}