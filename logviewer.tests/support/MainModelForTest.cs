using System.Collections.Generic;
using System.Reactive.Concurrency;
using logviewer.engine;
using logviewer.logic.storage;
using logviewer.logic.ui.main;

namespace logviewer.tests.support
{
    public class MainModelForTest : MainModel
    {
        public MainModelForTest(IMainViewModel viewModel, IScheduler backgroundScheduler = null) : base(viewModel, backgroundScheduler)
        {
        }

        protected override ILogStore CreateNewLogStore(ICollection<Semantic> messageSchema) => new LogStore(messageSchema, ":memory:");
    }
}
