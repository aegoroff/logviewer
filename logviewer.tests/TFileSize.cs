using NUnit.Framework;
using logviewer.core;

namespace logviewer.tests
{
    [TestFixture]
    internal class TFileSize
    {
        private static void Test(ulong size, SizeUnit unit, double value)
        {
            var sz = new FileSize(size);
            Assert.AreEqual(size, sz.Bytes);
            Assert.AreEqual(unit, sz.Unit);
            Assert.AreEqual(value, sz.Value);
        }

        [Test]
        public void Bytes()
        {
            Test(1023UL, SizeUnit.Bytes, 0.0);
        }

        [Test]
        public void BytesZero()
        {
            Test(0UL, SizeUnit.Bytes, 0.0);
        }

        [Test]
        public void EBytes()
        {
            Test(2UL * 1024UL * 1024UL * 1024UL * 1024UL * 1024UL * 1024UL, SizeUnit.EBytes, 2.0);
        }

        [Test]
        public void GBytes()
        {
            Test(2UL * 1024UL * 1024UL * 1024UL, SizeUnit.GBytes, 2.0);
        }

        [Test]
        public void KBytes()
        {
            Test(2048UL, SizeUnit.KBytes, 2.0);
        }

        [Test]
        public void KBytesBorder()
        {
            Test(1024UL, SizeUnit.KBytes, 1.0);
        }

        [Test]
        public void MBytes()
        {
            Test(2UL * 1024UL * 1024UL, SizeUnit.MBytes, 2.0);
        }

        [Test]
        public void MaxValue()
        {
            Test(ulong.MaxValue, SizeUnit.EBytes, 16.0);
        }

        [Test]
        public void PBytes()
        {
            Test(2UL * 1024UL * 1024UL * 1024UL * 1024UL * 1024UL, SizeUnit.PBytes, 2.0);
        }

        [Test]
        public void TBytes()
        {
            Test(2UL * 1024UL * 1024UL * 1024UL * 1024UL, SizeUnit.TBytes, 2.0);
        }
    }
}