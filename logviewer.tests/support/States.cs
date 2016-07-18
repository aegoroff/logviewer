using System;
using logviewer.logic.Annotations;
using logviewer.logic.fsm;

namespace logviewer.tests.support
{
    [PublicAPI]
    public class ConversationState : SolidState
    {
    }

    [PublicAPI]
    public class DiallingState : SolidState
    {
    }

    [PublicAPI]
    public class IdleState : SolidState
    {
    }

    [PublicAPI]
    public class RingingState : SolidState
    {
    }

    [PublicAPI]
    public class WaitForAnswerState : SolidState
    {
    }

    [PublicAPI]
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
    ///     A test state that reports the current date back to the state machine in its Entering method.
    /// </summary>
    [PublicAPI]
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

    [PublicAPI]
    public class StateWithoutParameterlessConstructor : SolidState
    {
        private readonly int number;

        // Constructor

        public StateWithoutParameterlessConstructor(int number)
        {
            this.number = number;
        }

        // Methods

        protected override void DoEntering(object context)
        {
            // Write the number to console
            Console.WriteLine(@"Entering {0} : number = {1}", this.GetType().Name, this.number);
        }

        protected override void DoExiting(object context)
        {
            Console.WriteLine(@"Exiting {0}", this.GetType().Name);
        }
    }
}