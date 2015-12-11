// Created by: egr
// Created at: 11.12.2015
// © 2012-2015 Alexander Egorov


using logviewer.logic.fsm;

namespace logviewer.logic.ui.network
{
    internal abstract class ProxyState : SolidState
    {
        protected override void DoEntering(object context)
        {
            var mode = (StateMachineMode) context;
            if (mode == StateMachineMode.Read)
            {
                this.Read();
            }
            else
            {
                this.Write();
            }
        }

        protected abstract void Read();

        protected abstract void Write();
    }
}