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
        public void Matches()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{ID}"));
        }

        [Test]
        public void MatchesSeveralWithoutLiteral()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{ID}%{DATE}"));
        }
        
        [Test]
        public void MatchesSeveral()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{ID} %{DATE}"));
        }
        
        [Test]
        public void MatchesSeveralNotEmptyLiteral()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{ID},%{DATE}"));
        }
        
        [Test]
        public void NotMatches()
        {
            GrokMatcher matcher = new GrokMatcher();
            Assert.That(matcher.Match("%{id}"), Is.False);
        }
    }
}