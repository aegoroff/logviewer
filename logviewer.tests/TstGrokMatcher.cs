// Created by: egr
// Created at: 02.10.2014
// © 2012-2014 Alexander Egorov

using logviewer.core;
using NUnit.Framework;

namespace logviewer.tests
{
    [TestFixture]
    public class TstGrokMatcher
    {
        [Test]
        public void MatchesUnexist()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{ID}"));
            Assert.That(matcher.Template, Is.EqualTo("%{ID}"));
        }

        [Test]
        public void MatchesSeveraUnexistlWithoutLiteral()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{ID}%{DATE}"));
            Assert.That(matcher.Template, Is.EqualTo("%{ID}%{DATE}"));
        }
        
        [Test]
        public void MatchesSeveralUnexist()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{ID} %{DATE}"));
            Assert.That(matcher.Template, Is.EqualTo("%{ID} %{DATE}"));
        }
        
        [Test]
        public void MatchesSeveralUnexistNotEmptyLiteral()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{ID},%{DATE}"));
            Assert.That(matcher.Template, Is.EqualTo("%{ID},%{DATE}"));
        }
        
        [Test]
        public void NotMatches()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{id}"), Is.False);
            Assert.That(matcher.Template, Is.Null);
        }

        [Test]
        public void MatchesExist()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{WORD}"));
            Assert.That(matcher.Template, Is.EqualTo(@"\b\w+\b"));
        }
        
        [Test]
        public void MatchesExistAndUnexist()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{WORD}%{ID}"));
            Assert.That(matcher.Template, Is.EqualTo(@"\b\w+\b%{ID}"));
        }
    }
}