﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.1.0.0
//      SpecFlow Generator Version:2.0.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace logviewer.tests
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.1.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public partial class SpecOpenLogFileFeature : Xunit.IClassFixture<SpecOpenLogFileFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "SpecOpenLogFile.feature"
#line hidden
        
        public SpecOpenLogFileFeature()
        {
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "SpecOpenLogFile", "\tTesting opening log file and parsing it", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        public virtual void TestInitialize()
        {
        }
        
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        public virtual void SetFixture(SpecOpenLogFileFeature.FixtureData fixtureData)
        {
        }
        
        void System.IDisposable.Dispose()
        {
            this.ScenarioTearDown();
        }
        
        [Xunit.FactAttribute(DisplayName="No filtering case")]
        [Xunit.TraitAttribute("FeatureTitle", "SpecOpenLogFile")]
        [Xunit.TraitAttribute("Description", "No filtering case")]
        [Xunit.TraitAttribute("Category", "mainmodel")]
        public virtual void NoFilteringCase()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("No filtering case", new string[] {
                        "mainmodel"});
#line 5
this.ScenarioSetup(scenarioInfo);
#line 6
 testRunner.Given("I have file \"test.log\" on disk", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 7
 testRunner.When("I press open with default filtering parameters", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 8
 testRunner.Then("the number of shown messages should be 2", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Filtering by level")]
        [Xunit.TraitAttribute("FeatureTitle", "SpecOpenLogFile")]
        [Xunit.TraitAttribute("Description", "Filtering by level")]
        [Xunit.TraitAttribute("Category", "mainmodel")]
        public virtual void FilteringByLevel()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Filtering by level", new string[] {
                        "mainmodel"});
#line 11
this.ScenarioSetup(scenarioInfo);
#line 12
 testRunner.Given("I have file \"test.log\" on disk", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 13
 testRunner.When("I press open with min level \"Fatal\" and max level \"Fatal\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 14
 testRunner.Then("the number of shown messages should be 0", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="No filtering case. reload")]
        [Xunit.TraitAttribute("FeatureTitle", "SpecOpenLogFile")]
        [Xunit.TraitAttribute("Description", "No filtering case. reload")]
        [Xunit.TraitAttribute("Category", "mainmodel")]
        public virtual void NoFilteringCase_Reload()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("No filtering case. reload", new string[] {
                        "mainmodel"});
#line 17
this.ScenarioSetup(scenarioInfo);
#line 18
 testRunner.Given("I have file \"test.log\" on disk", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 19
 testRunner.When("I press open with default filtering parameters and then reload", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 20
 testRunner.Then("the number of shown messages should be 2", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.1.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                SpecOpenLogFileFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                SpecOpenLogFileFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
