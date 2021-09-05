namespace DingleValleyRebooted.Server.Helpers
{
    public static class Helpers
    {
        public static string HashEncode(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
