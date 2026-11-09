using System;
using System.Security.Cryptography;
using System.Text;

namespace WishList.Exporting
{
    /// <summary>
    /// Progress signature of a scroll page. The body is hashed in small chunks so no second
    /// full copy of the page is allocated. The result equals SHA-256 of the UTF-8 body.
    /// </summary>
    public static class PageSignature
    {
        private const int ChunkChars = 2048;

        public static string Compute(string body)
        {
            var encoding = new UTF8Encoding(false);
            var encoder = encoding.GetEncoder();
            var buffer = new byte[encoding.GetMaxByteCount(ChunkChars) + 4];

            using (var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256))
            {
                for (var offset = 0; offset < body.Length; offset += ChunkChars)
                {
                    var count = Math.Min(ChunkChars, body.Length - offset);
                    var last = offset + count >= body.Length;
                    var span = body.AsSpan(offset, count);
                    var written = encoder.GetBytes(span, buffer, last);
                    hash.AppendData(buffer, 0, written);
                }

                return Convert.ToBase64String(hash.GetHashAndReset());
            }
        }
    }
}
