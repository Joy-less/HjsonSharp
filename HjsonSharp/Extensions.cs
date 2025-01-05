namespace HjsonSharp;

internal static class Extensions {
    public static void RemoveLast<T>(this IList<T> List) {
        List.RemoveAt(List.Count - 1);
    }
}