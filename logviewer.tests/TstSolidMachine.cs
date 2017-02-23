// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 11.12.2015
// © 2012-2017 Alexander Egorov

using System;
using FluentAssertions;
using FluentValidation;
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
        public void Trigger_WithoutStart_ThrowException()
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

        [Fact]
        public void Configure_MultipleUnguardedTriggers_ThrowException()
        {
            // Arrange
            var sm = new SolidMachine<TelephoneTrigger>();

            // Act
            var exception = Assert.Throws<SolidStateException>(delegate
            {
                sm.State<IdleState>()
                    .On(TelephoneTrigger.PickingUpPhone).GoesTo<DiallingState>()
                    .On(TelephoneTrigger.PickingUpPhone).GoesTo<RingingState>();
            });

            // Assert
            exception.ErrorId.Should().Be(SolidStateConstants.ErrorTriggerAlreadyConfiguredForState);
        }

        [Fact]
        public void Configure_UnguardedAndGuardedTrigger_ThrowException()
        {
            // Arrange
            var sm = new SolidMachine<TelephoneTrigger>();

            // Act
            var exception = Assert.Throws<SolidStateException>(delegate
            {
                sm.State<IdleState>()
                    .On(TelephoneTrigger.PickingUpPhone).GoesTo<DiallingState>()
                    .On(TelephoneTrigger.PickingUpPhone, () => true).GoesTo<RingingState>();
            });

            // Assert
            exception.ErrorId.Should().Be(SolidStateConstants.ErrorCannotMixGuardedAndGuardlessTransitions);
        }

        [Fact]
        public void Configure_MultipleGuardsEvaluateToTrue_ThrowException()
        {
            // Arrange
            var sm = new SolidMachine<TelephoneTrigger>();

            sm.State<IdleState>()
                .IsInitialState()
                .On(TelephoneTrigger.PickingUpPhone, () => 1 + 1 == 2).GoesTo<DiallingState>() //-V3001
                .On(TelephoneTrigger.PickingUpPhone, () => 6 / 2 == 3).GoesTo<RingingState>(); //-V3001

            sm.Start();

            // Act
            var exception = Assert.Throws<SolidStateException>(delegate
            {
                sm.Trigger(TelephoneTrigger.PickingUpPhone);
            });

            // Assert
            exception.ErrorId.Should().Be(SolidStateConstants.ErrorMultipleGuardClausesAreTrue);
        }

        [Fact]
        public void Configure_MultipleInitialStates_ThrowException()
        {
            // Arrange
            var sm = new SolidMachine<TelephoneTrigger>();

            // Act
            var exception = Assert.Throws<SolidStateException>(delegate
            {
                sm.State<IdleState>()
                    .IsInitialState()
                    .On(TelephoneTrigger.PickingUpPhone).GoesTo<DiallingState>()
                    .On(TelephoneTrigger.IncomingCall).GoesTo<RingingState>();

                sm.State<RingingState>()
                    .IsInitialState()
                    .On(TelephoneTrigger.PickingUpPhone).GoesTo<ConversationState>()
                    .On(TelephoneTrigger.HangingUp).GoesTo<IdleState>();
            });

            // Assert
            exception.ErrorId.Should().Be(SolidStateConstants.ErrorMultipleInitialStates);
        }

        [Fact]
        public void Configure_StartWithParameterizedState_ThrowException()
        {
            // Arrange
            var sm = new SolidMachine<TelephoneTrigger>();

            // Act
            var exception = Assert.Throws<ValidationException>(delegate
            {
                sm.State<IdleState>()
                    .On(TelephoneTrigger.IncomingCall).GoesTo<StateWithoutParameterlessConstructor>();
                sm.Start();
            });

            // Assert
            exception.Message.Should().Contain("One or more configured states has no parameterless constructor. Add such constructors or make sure that the StateResolver property is set!");
        }

        [Fact]
        public void Start_WithoutStates_ThrowException()
        {
            // Arrange
            var sm = new SolidMachine<TelephoneTrigger>();

            // Act
            var exception = Assert.Throws<ValidationException>(() => sm.Start());

            // Assert
            exception.Message.Should().Contain("No states have been configured!");
        }

        [Fact]
        public void Trigger_InvalidTriggerSequence_ThrowException()
        {
            // Arrange
            var sm = new SolidMachine<TelephoneTrigger>();

            sm.State<IdleState>()
                .On(TelephoneTrigger.PickingUpPhone).GoesTo<DiallingState>()
                .On(TelephoneTrigger.IncomingCall).GoesTo<RingingState>();

            sm.State<DiallingState>()
                .On(TelephoneTrigger.DialedNumber).GoesTo<WaitForAnswerState>()
                .On(TelephoneTrigger.HangingUp).GoesTo<IdleState>();

            sm.State<RingingState>()
                .On(TelephoneTrigger.PickingUpPhone).GoesTo<ConversationState>()
                .On(TelephoneTrigger.HangingUp).GoesTo<IdleState>();

            sm.State<WaitForAnswerState>()
                .On(TelephoneTrigger.Answered).GoesTo<ConversationState>()
                .On(TelephoneTrigger.NotAnswering).GoesTo<IdleState>()
                .On(TelephoneTrigger.HangingUp).GoesTo<IdleState>();

            sm.State<ConversationState>()
                .On(TelephoneTrigger.HangingUp).GoesTo<IdleState>();

            sm.Start();

            // Act
            var exception = Assert.Throws<SolidStateException>(delegate
            {
                sm.Trigger(TelephoneTrigger.PickingUpPhone);
                sm.Trigger(TelephoneTrigger.PickingUpPhone);
            });

            // Assert
            exception.ErrorId.Should().Be(SolidStateConstants.ErrorTriggerNotValidForState);
        }
    }
}