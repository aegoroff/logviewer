using NUnit.Framework;
using logviewer.core;

namespace logviewer.tests
{
    [TestFixture]
    internal class TstFileSize
    {
        [TestCase(0UL, SizeUnit.Bytes, 0.0)]
        [TestCase(1023UL, SizeUnit.Bytes, 0.0)]
        [TestCase(1024UL, SizeUnit.KBytes, 1.0)]
        [TestCase(2UL * 1024UL, SizeUnit.KBytes, 2.0)]
        [TestCase(2UL * 1024UL * 1024UL, SizeUnit.MBytes, 2.0)]
        [TestCase(2UL * 1024UL * 1024UL * 1024UL, SizeUnit.GBytes, 2.0)]
        [TestCase(2UL * 1024UL * 1024UL * 1024UL * 1024UL, SizeUnit.TBytes, 2.0)]
        [TestCase(2UL * 1024UL * 1024UL * 1024UL * 1024UL * 1024UL, SizeUnit.PBytes, 2.0)]
        [TestCase(2UL * 1024UL * 1024UL * 1024UL * 1024UL * 1024UL * 1024UL, SizeUnit.EBytes, 2.0)]
        public void Normalize(ulong size, SizeUnit unit, double value)
        {
            var sz = new FileSize(size);
            Assert.That(sz.Bytes, Is.EqualTo(size));
            Assert.That(sz.Unit, Is.EqualTo(unit));
            Assert.That(sz.Value, Is.EqualTo(value));
        }
    }
}