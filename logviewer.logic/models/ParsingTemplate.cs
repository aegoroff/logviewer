// Created by: egr
// Created at: 24.09.2013
// © 2012-2015 Alexander Egorov

using System.Collections.Generic;
using logviewer.engine;
using logviewer.logic.Properties;
using logviewer.logic.storage;

namespace logviewer.logic.models
{
    public class ParsingTemplate
    {
        public int Index { get; set; }

        [Column("StartMessage")]
        public string StartMessage { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("Filter", Nullable = true)]
        public string Filter { get; set; }
        
        [Column("Compiled")]
        public bool Compiled { get; set; }

        public bool IsEmpty => this.StartMessage == null;

        public string DisplayName
        {
            get
            {
                var gm = new GrokMatcher(this.StartMessage);
                if (string.IsNullOrWhiteSpace(gm.Template))
                {
                    return this.Name;
                }
                return this.Name + " (" + new FileSize(gm.Template.Length, true).Format() + ")"; // Not L10N
            }
        }

        public static IEnumerable<ParsingTemplate> Defaults
        {
            get
            {
                yield return new ParsingTemplate { Index = 0, StartMessage = @"^\[?%{TIMESTAMP_ISO8601:Occured:DateTime}\]?%{DATA}%{LOGLEVEL:Level:LogLevel}%{DATA}", Name = Resources.ParsingTemplateNlog };
                yield return new ParsingTemplate { Index = 1, StartMessage = @"%{IIS}", Name = Resources.ParsingTemplateIis, Filter = @"^#%{DATA}" };
                yield return new ParsingTemplate { Index = 2, StartMessage = @"%{DATA}", Name = Resources.ParsingTemplatePlainText, Compiled = true };
                yield return new ParsingTemplate { Index = 3, StartMessage = @"%{APACHE_SERVER}", Name = Resources.ParsingTemplateApacheServer };
                yield return new ParsingTemplate { Index = 4, StartMessage = @"%{COMMONAPACHELOG_LEVELED}", Name = Resources.ParsingTemplateApacheCommon };
                yield return new ParsingTemplate { Index = 5, StartMessage = @"%{COMBINEDAPACHELOG_LEVELED}", Name = Resources.ParsingTemplateApacheCombined };
                yield return new ParsingTemplate { Index = 6, StartMessage = @"%{SYSLOGTIMESTAMP:Timestamp} (?:%{SYSLOGFACILITY} )?%{SYSLOGPROG}:%{DATA:message}", Name = Resources.ParsingTemplateSyslog };
                yield return new ParsingTemplate { Index = 7, StartMessage = "^\\[%{DATA}\\]\\[%{TIMESTAMP_ISO8601}\\]%{DATA:Level:'i'->LogLevel.Info,'w'->LogLevel.Warn,'e'->LogLevel.Error}\"\\d{3}:\"%{SPACE}%{DATA}", Name = Resources.ParsingTemplateWixBurn }; // Not L10N
                yield return new ParsingTemplate { Index = 8, StartMessage = "^\\[%{TIME}\\](%{WORD:Level:'W'->LogLevel.Warn,'E'->LogLevel.Error,'*'->LogLevel.Info}|%{SPACE})[:]%{DATA}", Name = Resources.ParsingTemplateTeamcityBuildLog, Filter = @"^[^\\[\\s].*" }; // Not L10N
            }
        }
    }
}