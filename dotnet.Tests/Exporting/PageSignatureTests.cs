using System;
using System.Security.Cryptography;
using System.Text;
using WishList.Exporting;
using Xunit;

namespace WishList.Tests.Exporting
{
    public class PageSignatureTests
    {
        private static string FullBodyHash(string body)
        {
            using (var sha = SHA256.Create())
            {
                return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(body)));
            }
        }

        [Fact]
        public void SameBodyProducesSameSignatureAndDifferentBodyDiffers()
        {
            Assert.Equal(PageSignature.Compute("[{\"id\":\"a\"}]"), PageSignature.Compute("[{\"id\":\"a\"}]"));
            Assert.NotEqual(PageSignature.Compute("[{\"id\":\"a\"}]"), PageSignature.Compute("[{\"id\":\"b\"}]"));
        }

        [Fact]
        public void ChunkedHashMatchesFullBodyHashAcrossSurrogatePairsAndChunkBoundaries()
        {
            // 😀 is a surrogate pair; the odd prefix shifts pairs across every chunk boundary.
            var body = "x" + new StringBuilder().Insert(0, "é😀ab", 5000).ToString();

            Assert.Equal(FullBodyHash(body), PageSignature.Compute(body));
        }

        [Fact]
        public void EmptyBodyIsSupported()
        {
            Assert.Equal(FullBodyHash(string.Empty), PageSignature.Compute(string.Empty));
        }

        [Fact]
        public void ComputingSignatureDoesNotAllocateACopyOfTheBody()
        {
            // 60k chars stays below the LOH threshold, so the allocation counter sees every copy.
            var body = new string('a', 60000);
            PageSignature.Compute("warm-up");
            const int runs = 50;

            var before = GC.GetAllocatedBytesForCurrentThread();
            for (var i = 0; i < runs; i++)
            {
                PageSignature.Compute(body);
            }

            var perCall = (GC.GetAllocatedBytesForCurrentThread() - before) / runs;

            Assert.True(perCall < 20 * 1024, $"allocated {perCall} bytes per call");
        }
    }
}
