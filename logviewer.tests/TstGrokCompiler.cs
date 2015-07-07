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
        [InlineData("%{WORD}", @"\b\w+\b")]
        [InlineData("%{ID}' %{} '%{DAT}", "%{ID} %{} %{DAT}")]
        [InlineData("%{ID}\" %{} \"%{DAT}", "%{ID} %{} %{DAT}")]
        [InlineData("%{ID}\"\\\" %{} \\\"\"%{DAT}", "%{ID}\" %{} \"%{DAT}")]
        [InlineData("%{ID}\"\\' %{} \\'\"%{DAT}", "%{ID}' %{} '%{DAT}")]
        [InlineData("%{ID}\"' %{} '\"%{DAT}", "%{ID}' %{} '%{DAT}")]
        [InlineData("%{ID}\"\\' \\\"%{}\\\" \\'\"%{DAT}", "%{ID}' \"%{}\" '%{DAT}")]
        [InlineData("%{ID}\"\\' \\'%{}\\' \\'\"%{DAT}", "%{ID}' '%{}' '%{DAT}")]
        [InlineData("%{ID}'\\\" %{} \\\"'%{DAT}", "%{ID}\" %{} \"%{DAT}")]
        [InlineData("%{ID}'\\' %{} \\''%{DAT}", "%{ID}' %{} '%{DAT}")]
        [InlineData("%{ID}'\" %{} \"'%{DAT}", "%{ID}\" %{} \"%{DAT}")]
        [InlineData("%{ID}\" %{} \"%{DAT}\" %{} \"", "%{ID} %{} %{DAT} %{} ")]
        [InlineData("\" %{} \"%{ID}\" %{} \"%{DAT}\" %{} \"", " %{} %{ID} %{} %{DAT} %{} ")]
        [InlineData("%{ID}''%{DAT}", "%{ID}%{DAT}")]
        [InlineData("%{ID}\"\"%{DAT}", "%{ID}%{DAT}")]
        [InlineData("%{WORD}%{ID}", @"\b\w+\b%{ID}")]
        [InlineData("%{WORD}%{INT}", @"\b\w+\b(?:[+-]?(?:[0-9]+))")]
        [InlineData("%{WORD} %{INT}", @"\b\w+\b (?:[+-]?(?:[0-9]+))")]
        [InlineData("%{WORD} %{INT} ", @"\b\w+\b (?:[+-]?(?:[0-9]+)) ")]
        [InlineData("%{WORD} %{INT}1234", @"\b\w+\b (?:[+-]?(?:[0-9]+))1234")]
        [InlineData("%{WORD} %{INT}str", @"\b\w+\b (?:[+-]?(?:[0-9]+))str")]
        [InlineData("%{WORD}str%{INT}trs", @"\b\w+\bstr(?:[+-]?(?:[0-9]+))trs")]
        [InlineData("%{TIME}", @"(?!<[0-9])(?:2[0123]|[01]?[0-9]):(?:[0-5][0-9])(?::(?:(?:[0-5][0-9]|60)(?:[:.,][0-9]+)?))(?![0-9])")]
        [InlineData("%{TIMESTAMP_ISO8601}", @"(?>\d\d){1,2}-(?:0?[1-9]|1[0-2])-(?:(?:0[1-9])|(?:[12][0-9])|(?:3[01])|[1-9])[T ](?:2[0123]|[01]?[0-9]):?(?:[0-5][0-9])(?::?(?:(?:[0-5][0-9]|60)(?:[:.,][0-9]+)?))?(?:Z|[+-](?:2[0123]|[01]?[0-9])(?::?(?:[0-5][0-9])))?")]
        [InlineData("%{S1:s}%{S2:s}", @"%{S1}%{S2}")]
        [InlineData("%{LOGLEVEL:level}", @"(?<level>([A-a]lert|ALERT|[T|t]race|TRACE|[D|d]ebug|DEBUG|[N|n]otice|NOTICE|[I|i]nfo|INFO|[W|w]arn?(?:ing)?|WARN?(?:ING)?|[E|e]rr?(?:or)?|ERR?(?:OR)?|[C|c]rit?(?:ical)?|CRIT?(?:ICAL)?|[F|f]atal|FATAL|[S|s]evere|SEVERE|EMERG(?:ENCY)?|[Ee]merg(?:ency)?))")]
        [InlineData("%{POSINT:num,int}", @"(?<num>\b(?:[1-9][0-9]*)\b)")]
        [InlineData("%{POSINT:num,'0'->LogLevel.Trace,'1'->LogLevel.Debug,'2'->LogLevel.Info}", @"(?<num>\b(?:[1-9][0-9]*)\b)")]
        [InlineData("%{POSINT:num,\"0\"->LogLevel.Trace,\"1\"->LogLevel.Debug,\"2\"->LogLevel.Info}", @"(?<num>\b(?:[1-9][0-9]*)\b)")]
        [InlineData("%{POSINT:num,'  0 '->LogLevel.Trace,' 1 '->LogLevel.Debug,' 2'->LogLevel.Info}", @"(?<num>\b(?:[1-9][0-9]*)\b)")]
        [InlineData("%{INT:Id,'0'->LogLevel.Trace}", "(?<Id>(?:[+-]?(?:[0-9]+)))")]
        [InlineData("%{INT:Id,'1'->LogLevel.Debug}", "(?<Id>(?:[+-]?(?:[0-9]+)))")]
        [InlineData("%{INT:Id,'2'->LogLevel.Info}", "(?<Id>(?:[+-]?(?:[0-9]+)))")]
        [InlineData("%{INT:Id,'3'->LogLevel.Warn}", "(?<Id>(?:[+-]?(?:[0-9]+)))")]
        [InlineData("%{INT:Id,'4'->LogLevel.Error}", "(?<Id>(?:[+-]?(?:[0-9]+)))")]
        [InlineData("%{INT:Id,'5'->LogLevel.Fatal}", "(?<Id>(?:[+-]?(?:[0-9]+)))")]
        [InlineData("%{INT:Id,'0'->LogLevel.Trace,'1'->LogLevel.Debug,'2'->LogLevel.Info,'3'->LogLevel.Warn,'4'->LogLevel.Error,'5'->LogLevel.Fatal}", "(?<Id>(?:[+-]?(?:[0-9]+)))")]
        [InlineData("%{INT:num_property}", "(?<num_property>(?:[+-]?(?:[0-9]+)))")]
        [InlineData("%{POSINT}%%{POSINT}", "\\b(?:[1-9][0-9]*)\\b{POSINT}")]
        [InlineData("%{POSINT}}%{POSINT}", "\\b(?:[1-9][0-9]*)\\b}\\b(?:[1-9][0-9]*)\\b")]
        [InlineData("%{URIPATH}", @"(?:/[A-Za-z0-9$.+!*'(){},~:;=@#%_\-]*)+")]
        [InlineData("%{NGUSERNAME}", @"[a-zA-Z\.\@\-\+_%]+")]
        [InlineData("%{URIPARAM}", @"\?[A-Za-z0-9$.+!*'|(){},~@#%&/=:;_?\-\[\]]*")]
        [InlineData("%{WORD:word,String}", @"(?<word>\b\w+\b)")]
        [InlineData("%{WORD:word,string}", @"(?<word>\b\w+\b)")]
        public void PositiveCompileTestsThatChangeString(string pattern, string result)
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