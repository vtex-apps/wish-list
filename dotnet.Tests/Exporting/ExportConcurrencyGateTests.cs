using WishList.Exporting;
using Xunit;

namespace WishList.Tests.Exporting
{
    public class ExportConcurrencyGateTests
    {
        [Fact]
        public void SecondLeaseIsRejectedUntilFirstLeaseIsDisposed()
        {
            var gate = new ExportConcurrencyGate();

            Assert.True(gate.TryAcquire(out var first));
            Assert.False(gate.TryAcquire(out _));
            first.Dispose();
            Assert.True(gate.TryAcquire(out var second));
            second.Dispose();
        }

        [Fact]
        public void DisposingLeaseTwiceDoesNotOverReleaseSemaphore()
        {
            var gate = new ExportConcurrencyGate();
            gate.TryAcquire(out var lease);

            lease.Dispose();
            lease.Dispose();

            Assert.True(gate.TryAcquire(out var next));
            Assert.False(gate.TryAcquire(out _));
            next.Dispose();
        }
    }
}
