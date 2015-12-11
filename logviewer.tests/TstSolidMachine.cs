using System;
using FluentAssertions;
using logviewer.logic.fsm;
using logviewer.tests.support;
using Xunit;

namespace logviewer.tests
{
    public class TstSolidMachine
    {
        [Fact]
        public void State_IsInitialStateExplicitSet_RingingState()
        {
            // Arrange
            var sm = new SolidMachine<TelephoneTrigger>();

            // Act
            sm.State<IdleState>()
                .On(TelephoneTrigger.PickingUpPhone).GoesTo<DiallingState>()
                .On(TelephoneTrigger.IncomingCall).GoesTo<RingingState>();

            sm.State<RingingState>()
                .IsInitialState()
                .On(TelephoneTrigger.PickingUpPhone).GoesTo<ConversationState>()
                .On(TelephoneTrigger.NotAnswering).GoesTo<IdleState>();

            sm.Start();

            // Assert
            sm.CurrentState.Should().BeOfType<RingingState>();
        }

        [Fact]
        public void State_IsInitialStateImplicit_IdleState()
        {
            // Arrange
            var sm = new SolidMachine<TelephoneTrigger>();

            // Act
            sm.State<IdleState>()
                .On(TelephoneTrigger.PickingUpPhone).GoesTo<DiallingState>()
                .On(TelephoneTrigger.IncomingCall).GoesTo<RingingState>();

            sm.State<RingingState>()
                .On(TelephoneTrigger.PickingUpPhone).GoesTo<ConversationState>()
                .On(TelephoneTrigger.HangingUp).GoesTo<IdleState>();

            sm.Start();

            // Assert
            sm.CurrentState.Should().BeOfType<IdleState>();
        }

        [Fact]
        public void Trigger_PickingUpPhone_CurrentIsDiallingState()
        {
            // Arrange
            var sm = new SolidMachine<TelephoneTrigger>();

            sm.State<IdleState>()
                .IsInitialState()
                .On(TelephoneTrigger.PickingUpPhone).GoesTo<DiallingState>();

            sm.Start();

            // Act
            sm.Trigger(TelephoneTrigger.PickingUpPhone);

            // Assert
            sm.CurrentState.Should().BeOfType<DiallingState>();
        }

        [Fact]
        public void Trigger_WithoutStart_ThrowSolidStateException()
        {
            // Arrange
            var sm = new SolidMachine<TelephoneTrigger>();

            sm.State<IdleState>()
                .IsInitialState()
                .On(TelephoneTrigger.PickingUpPhone).GoesTo<DiallingState>();

            // Act
            Assert.Throws<SolidStateException>(delegate { sm.Trigger(TelephoneTrigger.PickingUpPhone); });
        }

        [Fact]
        public void Trigger_StateHistory_HistoryShouldBeAsSpecified()
        {
            // Arrange
            var sm = new SolidMachine<TelephoneTrigger>();
            sm.State<IdleState>()
                .IsInitialState()
                .On(TelephoneTrigger.PickingUpPhone).GoesTo<DiallingState>();
            sm.State<DiallingState>()
                .On(TelephoneTrigger.DialedNumber).GoesTo<WaitForAnswerState>();
            sm.State<WaitForAnswerState>()
                .On(TelephoneTrigger.Answered).GoesTo<ConversationState>();
            sm.Start();

            // Act
            sm.Trigger(TelephoneTrigger.PickingUpPhone);
            sm.Trigger(TelephoneTrigger.DialedNumber);
            sm.Trigger(TelephoneTrigger.Answered);

            // Assert
            sm.StateHistory.Length.Should().Be(3, "State history must not be empty!");
            sm.StateHistory[0].Should().Be(typeof (WaitForAnswerState));
            sm.StateHistory[1].Should().Be(typeof (DiallingState));
            sm.StateHistory[2].Should().Be(typeof (IdleState));
        }

        [Fact]
        public void Start_StateHistory_Empty()
        {
            // Arrange
            var sm = new SolidMachine<TelephoneTrigger>();
            sm.State<IdleState>()
                .IsInitialState()
                .On(TelephoneTrigger.PickingUpPhone).GoesTo<DiallingState>();
            sm.State<DiallingState>()
                .On(TelephoneTrigger.DialedNumber).GoesTo<WaitForAnswerState>();
            sm.State<WaitForAnswerState>()
                .On(TelephoneTrigger.Answered).GoesTo<ConversationState>();

            // Act
            sm.Start();

            // Assert
            sm.StateHistory.Length.Should().Be(0, "State history must be empty!");
        }

        [Fact]
        public void Trigger_UsingExplicitContext_ContextChanged()
        {
            // Arrange
            var context = new DateHolder();

            var sm = new SolidMachine<TelephoneTrigger>(context);

            sm.State<IdleState>()
                .On(TelephoneTrigger.PickingUpPhone).GoesTo<DateReportingState>();

            sm.Start();

            // Act
            sm.Trigger(TelephoneTrigger.PickingUpPhone);

            // Assert
            context.CurrentDate.Should().Be(DateTime.Today);
        }

        [Theory]
        [InlineData(true, nameof(DiallingState))]
        [InlineData(false, nameof(TelephoneBrokenState))]
        public void Trigger_GuardClause_ContextChanged(bool isPhoneWorking, string stateName)
        {
            // Arrange
            var sm = new SolidMachine<TelephoneTrigger>();
            sm.State<IdleState>()
                .On(TelephoneTrigger.PickingUpPhone, () => isPhoneWorking).GoesTo<DiallingState>()
                .On(TelephoneTrigger.PickingUpPhone, () => !isPhoneWorking).GoesTo<TelephoneBrokenState>()
                .On(TelephoneTrigger.IncomingCall).GoesTo<RingingState>();

            sm.State<RingingState>()
                .On(TelephoneTrigger.PickingUpPhone).GoesTo<ConversationState>()
                .On(TelephoneTrigger.NotAnswering).GoesTo<IdleState>();

            sm.State<TelephoneBrokenState>()
                .On(TelephoneTrigger.HangingUp).GoesTo<IdleState>();

            sm.Start();

            // Act
            sm.Trigger(TelephoneTrigger.PickingUpPhone);

            // Assert
            sm.CurrentState.GetType().Name.Should().Be(stateName);
        }
    }
}