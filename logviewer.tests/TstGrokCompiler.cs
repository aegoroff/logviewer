// Created by: egr
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
        [InlineData("%{ID}str%{DAT}str")]
        [InlineData("str%{ID}str%{DAT}")]
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

        [Theory]
        [InlineData("%{POSINT:num,int,int}")]
        [InlineData("%{POSINT:num,int_number}")]
        [InlineData("%{POSINT:_num}")]
        [InlineData("%{POSINT:num1,num1}")]
        [InlineData("%{POSINT:N1}")]
        [InlineData("%{POSINT:1N}")]
        [InlineData("%{POSINT:1n}")]
        [InlineData("%{id}")]
        [InlineData("%{WORD")]
        [InlineData("%{POSINT:num,small}")]
        [InlineData("%{INT:Id,'0'->LogLevel.T}")]
        [InlineData("%{INT:Id,'0'->LogLevel.None}")]
        [InlineData("%{ID:Id,'0'->LogLevel.Trace}")]
        public void NegativeCompileTests(string pattern)
        {
            GrokCompiler compiler = new GrokCompiler(this.output.WriteLine);
            compiler.Compile(pattern);
        }

        [Theory]
        [InlineData("%{TIMESTAMP_ISO8601:datetime}%{DATA:meta}%{LOGLEVEL:level}%{DATA:head}")]
        [InlineData("%{TIMESTAMP_ISO8601:datetime,DateTime}%{DATA:meta}%{LOGLEVEL:level,LogLevel}%{DATA:head}")]
        [InlineData("%{TIMESTAMP_ISO8601:datetime,DateTime}%{DATA:meta}%{LOGLEVEL:level,LogLevel}%{DATA:head,String}")]
        public void ParseRealMessage(string pattern)
        {
            GrokCompiler compiler = new GrokCompiler(this.output.WriteLine);
            compiler.Compile(pattern);
        }
    }
}