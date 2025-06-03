namespace Jobbr.Server.Storage
{
    /// <summary>
    /// Helper class for generic extension methods.
    /// </summary>
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Deep clone an object.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="a">Object instance.</param>
        /// <returns>A deep clone of the given object.</returns>
        public static T Clone<T>(this T a)
        {
            return a == null ? default : FastCloner.FastCloner.DeepClone(a);
        }
    }
}
