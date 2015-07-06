﻿// Created by: egr
// Created at: 06.07.2015
// © 2012-2015 Alexander Egorov

using logviewer.engine;
using Xunit;
using Xunit.Abstractions;

namespace logviewer.tests
{
    public class TstGrokCompiler
    {
        private readonly ITestOutputHelper output;

        public TstGrokCompiler(ITestOutputHelper output)
        {
            this.output = output;
        }
        
        [Theory]
        [InlineData("%{ID}")]
        [InlineData("%{ID}%{DAT}")]
        [InlineData("%{ID} %{DAT}")]
        [InlineData("%{ID},%{DAT}")]
        [InlineData("%{ID}str%{DAT}")]
        [InlineData("str%{ID}str%{DAT}str")]
        [InlineData("%{ID}\"%{DAT}")]
        [InlineData("%{ID}'%{DAT}")]
        [InlineData("%{ID}\" %{DAT}")]
        [InlineData("%{ID}' %{DAT}")]
        [InlineData("%{ID} \"%{DAT}")]
        [InlineData("%{ID} '%{DAT}")]
        [InlineData("(?:(?:[A-Fa-f0-9]{2}-){5}[A-Fa-f0-9]{2})")]
        [InlineData("(?:(?:[A-Fa-f0-9]{2}:){5}[A-Fa-f0-9]{2})")]
        [InlineData("(?:(?:[A-Fa-f0-9]{4}\\.){2}[A-Fa-f0-9]{4})")]
        public void PositiveCompileTestsNotChangingString(string pattern)
        {
            GrokCompiler compiler = new GrokCompiler(this.output.WriteLine);
            compiler.Compile(pattern);
        }
    }
}