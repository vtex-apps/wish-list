using System.Collections.Generic;
using WishList.Exporting;
using WishList.Models;
using Xunit;

namespace WishList.Tests.Exporting
{
    public class ScrollSessionTests
    {
        [Fact]
        public void AcceptAccumulatesBoundedPageMetadata()
        {
            var session = new ScrollSession("token-a");
            var page = new ScrollPage(
                new List<WishListWrapper> { new WishListWrapper { Id = "1" } },
                128,
                false,
                "signature-a");

            session.Accept(page);

            Assert.Equal("token-a", session.Token);
            Assert.Equal(1, session.PagesRead);
            Assert.Equal(1, session.RecordsRead);
            Assert.Equal(128, session.BytesRead);
            Assert.Equal("signature-a", session.PreviousSignature);
        }

        [Fact]
        public void AcceptRejectsRepeatedNonTerminalPage()
        {
            var session = new ScrollSession("token-a");
            var page = new ScrollPage(
                new List<WishListWrapper> { new WishListWrapper { Id = "1" } },
                128,
                false,
                "same-signature");

            session.Accept(page);

            var error = Assert.Throws<ScrollNoProgressException>(() => session.Accept(page));
            Assert.Equal(ExportResultCode.NoProgress, error.ResultCode);
        }

        [Fact]
        public void RepeatedPageIsCountedBeforeTheExportIsAborted()
        {
            var session = new ScrollSession("token-a");
            var page = new ScrollPage(
                new List<WishListWrapper> { new WishListWrapper { Id = "1" } },
                128,
                false,
                "same-signature");
            session.Accept(page);

            Assert.Throws<ScrollNoProgressException>(() => session.Accept(page));

            Assert.Equal(2, session.PagesRead);
            Assert.Equal(2, session.RecordsRead);
            Assert.Equal(256, session.BytesRead);
        }

        [Fact]
        public void TokenIsImmutableAndOwnedByOneSession()
        {
            var first = new ScrollSession("token-a");
            var second = new ScrollSession("token-b");

            Assert.Equal("token-a", first.Token);
            Assert.Equal("token-b", second.Token);
        }
    }
}
