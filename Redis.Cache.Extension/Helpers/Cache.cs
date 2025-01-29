namespace Redis.Cache.Extension.Helpers
{
    public static class Cache
    {
        private static bool _isEnabled = false;

        public static bool IsEnabled => _isEnabled;

        internal static void EnableCache()
        {
            _isEnabled = true;
        }
    }
}
