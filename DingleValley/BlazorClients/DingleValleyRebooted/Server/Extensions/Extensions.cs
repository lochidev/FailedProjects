using System.Security.Cryptography;
using System.Text;

namespace DingleValleyRebooted.Server.Extensions
{
    public static class Extensions
    {
        public static string ToSha256(this string randomString)
        {
            SHA256 crypt = SHA256.Create();
            string hash = string.Empty;
            byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(randomString));
            foreach (byte theByte in crypto)
            {
                hash += theByte.ToString("x2");
            }
            return hash;
        }
        public static async Task<string> GetRawBodyAsync(
                    this HttpRequest request,
                    Encoding encoding = null)
        {
            if (!request.Body.CanSeek)
            {
                // We only do this if the stream isn't *already* seekable,
                // as EnableBuffering will create a new stream instance
                // each time it's called
                request.EnableBuffering();
            }

            request.Body.Position = 0;

            StreamReader reader = new StreamReader(request.Body, encoding ?? Encoding.UTF8);

            string body = await reader.ReadToEndAsync().ConfigureAwait(false);

            request.Body.Position = 0;

            return body;
        }
    }
}
