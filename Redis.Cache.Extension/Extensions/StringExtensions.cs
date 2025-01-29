namespace Redis.Cache.Extension.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        ///  This method is for creating a HashCode that will be same for all instances of this application.
        ///  The hash code itself is not guaranteed to be stable. Hash codes for identical strings can differ across .NET implementations, across .NET versions, and across .NET platforms (such as 32-bit and 64-bit) for a single version of .NET. In some cases, they can even differ by application domain.
        ///  Default HashCode implies that two subsequent runs of the same program may return different hash codes. https://learn.microsoft.com/en-us/dotnet/api/system.string.gethashcode?view=net-6.0
        ///  This solution was taken from the url below:
        ///  Title: Why is string.GetHashCode() different each time I run my program in .NET Core?
        ///  Url: https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int GetDeterministicHashCode(this string str)
        {
            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1)
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }
    }
}
