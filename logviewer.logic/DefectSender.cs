﻿// Created by: egr
// Created at: 11.09.2015
// © 2012-2016 Alexander Egorov

using System;
using System.Reflection;
using System.Threading.Tasks;
using logviewer.logic.Annotations;
using logviewer.logic.support;
using Octokit;

namespace logviewer.logic
{
    public sealed class DefectSender
    {
        private readonly string account;
        private readonly string project;

        private const string Key =
            @"PAA/AHgAbQBsACAAdgBlAHIAcwBpAG8AbgA9ACIAMQAuADAAIgAgAGUAbgBjAG8AZABpAG4AZwA9ACIAdQB0AGYALQAxADYAIgA/AD4APABSAFMAQQBQAGEAcgBhAG0AZQB0AGUAcgBzACAAeABtAGwAbgBzADoAeABzAGQAPQAiAGgAdAB0AHAAOgAvAC8AdwB3AHcALgB3ADMALgBvAHIAZwAvADIAMAAwADEALwBYAE0ATABTAGMAaABlAG0AYQAiACAAeABtAGwAbgBzADoAeABzAGkAPQAiAGgAdAB0AHAAOgAvAC8AdwB3AHcALgB3ADMALgBvAHIAZwAvADIAMAAwADEALwBYAE0ATABTAGMAaABlAG0AYQAtAGkAbgBzAHQAYQBuAGMAZQAiAD4APABFAHgAcABvAG4AZQBuAHQAPgBBAFEAQQBCADwALwBFAHgAcABvAG4AZQBuAHQAPgA8AE0AbwBkAHUAbAB1AHMAPgAyAHQASAA2AGgASABwAGUARABZAE8ANABDADYAQgBlAFgAaQB2AFMATQBSAFkAaABFAE8AcgBxADQAbgBnAHAAYgBHAHgARQBJAGMAZwAvAHkASgBCAHIAegArAGYAawBCAHMAMwA3ADYAQQA5AFMAbgBEAGMAbwBuAHkASgBYAEMAagBEAEoASQBpAGoAUAA3ADIATwB5AEQAVwBWAHUAQQB3AHoAZwBiADAAKwBvAFAAWgBYAEIAVAAyAHYALwBOAEMAMABOAGsAWAA2AHoATgBuAEYAMQA5AFEARQB0AEUAbwBXADcATgBxAEsAdQBUAE8AVwA3AFMARABKADIANwBQAFgARQA2AHAAUABpAE4ARgBVAHUAMABjAEIAawB3AEMANgBqAEIARwBIAHkAYgBFAFUAdQB6AFQAVQBVAHcAZgBDADgAbQBEAG8AeABqAGMAVgBYAGIAZgBJADAAbABsAHMANwBRAHIAZwA5AG8AcAA3AC8AUQBYAGgANwA0AHEAaQBtAFEAVQBhAG0AMABkAEwASgBHAG0AbQBrAGYAUQA5AGIAbABZAGQAMQBNAG0AZQA4ADIARwBBADcANwB4AGwAeQBOAHcAZgB5ADMASABEADgAVQBVAHgASgBFAEgAcQA4AG8AbwA3AHUAeQBVAFAAVQBVAFgATQBKADEAKwB2AEYALwBvAFMAVQBjAHkANgAyAHIAcQBuAFkAUQBNAHcAaQBlADYAYgB5AGMAUQBTAGwARwBCADEATABQAHUAUgA1AEsAZgBsAE8ARwBQAEQASQBoAEYAWQBsAG8ANwBUAGcANABNAEkASwB3AFgAMgBiAGEAeQB5AFgAQwBwAGIATgBUAEEAMQAwAFIAYQByAEMAUQBkAEQAVgBqADIAcgAyAEcAdwA9AD0APAAvAE0AbwBkAHUAbAB1AHMAPgA8AFAAPgA1AHIAUgBWADEAcQBLAFUAbQBIAEYAOQBMADAAVgBQAHkANgBjAGUAUABaAGUANgB3AFAAbwBYAGYASABqAEgAYwBpAGsANQB5AGcAbwArAEMAMQBsAFYARwBjAEwAdQBxAGIAeABHAG8AZAA1AHIAdwA3AFEAUABUAEoAagBXAE4AbwBoAEkAcQBSAEsAcwBaAFgAOAB4AG0ANABnADUALwBoAFcAaQBBAHoAcgB4AFQAcwBWAE8AaQBTAEoAUwA5ADMATQB6AGcAaABJAEwAawB4ADUARwBjAHkANwBCAE4AbgBNAG8ATgArAEwAVwBwADgATgBYAEcAUgBnAFEANQArADUAaQBSADIAVgBYAE4AUgBrAEUATgA0AFoANAB6ACsAdgB0AGcAZABBAHMAQQAvAGgAMABvADIAZgBlAEUASQB6AEsAcgBPAGsANABMAGIATQA9ADwALwBQAD4APABRAD4AOAB0AEEAUQBlAHIAQgBwAG4AdgBQAFYAUQAwAHkAbwBmAEQAYQBSAC8AMgBUAEkAdgBiAHMAQgBpADcATwBtAE8AOABlAFEAQQB6AHoAbQBoAGoATAA0AGUAKwB3AGoAVgBnAGMAUABxAGMAZgBFAHgAeQBZAFMAOABOAG4AdAB1ADcAZQA5ADUASwBRAFgAZgArAGoALwB6AFMAdwBNADcAZQBMAGYAcABrAGkASQBWAEUAKwBrADYAMwB6AEsAbQA4AFAAWgBHAFcAegBTAEYAZQBvAFcAdABCADAAbABEAC8AOABwAFAAUQBBAFoAMwBaAG4AQQA2AE8ATABzAHAASwAzAGsATgBvAEIAVABpAHEAeQAvAFoATQBVAEEAeABIACsAegBlAHAAUwA4AGQAYwBCAFoAOAA0ADIAWAA2AEUASwB5AE4AMABHADcAOABmAGsAPQA8AC8AUQA+ADwARABQAD4AUwBBADEAQwBVAGMAcAB0AEIAagA5AEwAagBaAHYAawBGAEEASwBaAG0AegByAHMAQgBLAEUAVwAvAEwAVAByADkAVwBlAFMAdQBOAHMAQwBEAGEARgBVAHgAcgByAHUAdAB1AHcAcQBVAHgAdQBZAEoASQBMAHoAMQBFAFIAWABnAHEAaQBGAHEATwBoAEEARgAyAEwAbQBKADIARQBnAFcANAA1ACsARAB1AHcAMQB1AFIAZwArAE0AdQA0AFoAWQAxAGMARABXAHoAZwB2AGoAVAA4AEQAVwBhADMAYwBnAGgAagB0AEUAcQBjAHgAeQB5AHQAaQA1AEYAOQB6AHkARAB2AFgAVABkADAARQBvADMAeQB3AHEASABiAEMALwBJAHoAKwBuAEgAQgBiAGoAdABIAGwAKwA3AEwAUQAwAC8ASgBrAHkAaQBQADgAZQA2AGsAPQA8AC8ARABQAD4APABEAFEAPgBRAGkAeQBzAFkAUgBPAFYASgBZAG4AawBsAEYAdgBmAEMAMABOAEEARgBPACsAZAB1AGYAKwB1AFUAeQBtAFoANAAyADcAbQA4ADgAcgAwAE4ASgB6AE4AbABkAEIAcwBiAFAAUwB0AHMAeQBMAEwAbwBVADcAaQBHAEoAdgA4AEsANAAzAFAAQgBmAEkAbQBxAFcAaQBSADEAcABQAHIAegBpAFYAbgB1AEkAVABLAEMAdABoAE4ANQBSAFAAMABqAC8ARQBYAFMATgBPAGMAaQBmAFoAcgBTADUAawBwADAAYQB0AEQAegBPAGsAcgBJADMAYgBVADIAVwBPAEgAQgBQAGEAVABYAEwAcwBxAGkAWAArADkAQgBiAFMAZgBHAGUANgBIAHQAcAArADAAbwBRADcAVQBOAFEAbABnADMAaAA3AFgAUABUAEkAQQBDADkAawA9ADwALwBEAFEAPgA8AEkAbgB2AGUAcgBzAGUAUQA+AEIAUABjACsAZABpAHkAMgAxAE8AcAA5AHoAVQBJAEMARgBVAEsANQBxAGcAZwA1AGwAegBaADcANQByAHUANQBCAGwAVAA3AG4AdQBKADcAbwB3AFYAWABqAHQASwB6AGcAcgBTADAAQwBmAHMAdgBrAGIAQgBkAGQARgBaAEkAMQBSAC8AVwBlAFUAcwAwAEoAUgBYAG0ARQBaAFkAVABRAFMAdAA2AFgALwBOAHEANwBjAHAAOABVAEwALwBEACsAdwAzAEkAWABYAFcAYQBLAEYAUgB3AHIASABuAGMAQwByAG4AegBQAHkAVABtAEUANwAxAFMASgBXAEoAbQBJAFAAbABrAEUATgA0AGQATwByADcARQAzAEUANwBxAFgASAB5AFUAZQBiAE4AdQB5AHkAUAB4AFoAQQBGAEMAbABVAEgANQBOAFAAUwBmAEQARAAwAD0APAAvAEkAbgB2AGUAcgBzAGUAUQA+ADwARAA+AFEAQwBEAFMAbQBOAG0AZAB6ADcAcwBmAEMASABVACsAdwByADEAMwA4ADcALwBIAEEAQQBUAEEAdwBvADgAeABvAEoAOQBZAEYAawBoAHYALwBQAEQAcQBUADcAdABuAGQAZQBFAFAASgB0AEsANgB4AEsASgBiAHEAcwB0AHYARQA4AEwANQBaAHkAUAByADAASAAzADQAcwAvAGwAYQBWAE4AVwBFAHYAcwBwAHIAQgBXAFYAcQBpAGsASwBkAHYANgBaAEcAZQBEAEgAWQBCAHEAZwBZAHkAQwAxAEkAagBzAG0AWABkAEsAcwBYAGgAdAB5ADYAMQBxADEASABjAGkAbQBTAHgASwBhAEQAWQBBAC8AbwAwAFQAcgBmAHEALwB6AGUAOQBJADEAWgA5AFEAWQBQAGMAYgA5AFIATABvAGQARgBWAGgAMgBUADQAVABaACsASQA2AEIARwBBAGIAdAAwAFEAdwBHAHUAawBkAG8AeQBHADcARwBYAFUAVwA1AHAARwBTAGwAVABlAGEAaABZAFgAZwBvAEUAawBiADQAVgA2AEwAOQBIAHQARgA4AC8AcwB6AGMAKwA3AHIAYwBtACsAdgBBAHQAQwB1AFAAVgA4AC8AcgBxAEwAYQBBAHEAawBtAFgAUABRAGoANQBlAFAANQBhADUAdwB2AFgAUwBLAG4ANQBJAEIAQwBmAGoAcQA0ADMAdQB2AE8AMABOAE4AYwA3AFcARgBMAEoAQwBUAGIAZQBmAHcAawBJAEwAMwBLAHIAWQBXAGwAeQBxADMAVgBjAGQATQAxAEkAWABvAHEAZwBYAEsAawBvAEMAMwBRAGkARwBVAFgARAAxACsARQB5AHYAbwBtADUAZAA3AFQANABSAG0ALwA1AEIAMgBRAD0APQA8AC8ARAA+ADwALwBSAFMAQQBQAGEAcgBhAG0AZQB0AGUAcgBzAD4A";

        private const string Token =
            @"Sya/5XqQV1YGR0rLKiVO2/6CiB5DDLLGC4uJar482FmWixb0oXi7GgHrXq3Wz62X5UE19irF2paA3llA2U7fJW+eS2nmkxkxaCOQf8oTHNpEQcZuDTAMGlkD1rr7PowlQ/xQ1n5ainrarWK8fCxmRA01wdHXCfDL1piwkmPqmYmLjELajZBEI9WzNakhcs/4VZRyccVNbrcZscKlgPJ6+8ZvDlcIQjYFdKsCtFazt6Lg+WSeyAEKFJtf8TlAwjNB0QWhh4Yvwim1PN9/Nk63KH+u99x6oRbjHyfYafJtK4eScQgxMlQDat262AHVPU1L+j8dV0G9Y2rc8R2qzIgX9w==";

        public DefectSender(string account, string project)
        {
            this.account = account;
            this.project = project;
        }

        [PublicAPI]
        public async Task<int> Send(Exception exception)
        {
            try
            {
                // This protection rather weak for human but robot hardly handle it.
                // in case of compromise just regenerate token
                var crypt = new AsymCrypt
                {
                    PrivateKey = Key
                };
                var github = new GitHubClient(new ProductHeaderValue(this.project))
                {
                    Credentials = new Credentials(crypt.Decrypt(Token))
                };
                var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                var title = $"Unhandled exception occured on {Environment.MachineName} at {DateTime.UtcNow:s}. App version: {version}";
                var createIssue = new NewIssue(title)
                {
                    Body = exception.ToString()
                };
                createIssue.Labels.Add(@"exception");
                createIssue.Labels.Add(@"bug");
                createIssue.Labels.Add(@"severity-high");
                var issue = await github.Issue.Create(this.account, this.project, createIssue);
                Log.Instance.InfoFormatted(@"Issue #{0} created", issue.Number);
                Log.Instance.Fatal(exception.Message, exception);
                return issue.Number;
            }
            catch (Exception e)
            {
                Log.Instance.Fatal(e.Message, e);
            }
            return 0;
        }
    }
}