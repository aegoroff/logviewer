using System;
using logviewer.logic.fsm;

namespace logviewer.tests.support
{
    public class ConversationState : SolidState
    {
    }

    public class DiallingState : SolidState
    {
    }

    public class IdleState : SolidState
    {
    }

    public class RingingState : SolidState
    {
    }

    public class WaitForAnswerState : SolidState
    {
    }

    public class TelephoneBrokenState : SolidState
    {
    }

    public enum TelephoneTrigger
    {
        PickingUpPhone,
        NotAnswering,
        IncomingCall,
        DialedNumber,
        Answered,
        HangingUp
    }

    /// <summary>
    /// A test state that reports the current date back to the state machine in its Entering method.
    /// </summary>
    public class DateReportingState : SolidState
    {
        protected override void DoEntering(object context)
        {
            var dateHolder = context as DateHolder;
            if (dateHolder != null)
            {
                dateHolder.CurrentDate = DateTime.Today;
            }
        }
    }

    public class DateHolder
    {
        public DateTime CurrentDate { get; set; }
    }
}