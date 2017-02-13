// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 11.12.2015
// © 2012-2016 Alexander Egorov

namespace logviewer.logic.fsm
{
    public static class SolidStateConstants
    {
        public const int ErrorStateMachineNotStarted = 1000;
        public const int ErrorTriggerNotValidForState = 1001;
        public const int ErrorMultipleGuardClausesAreTrue = 1002;
        public const int ErrorNoGuardClauseIsTrue = 1003;
        public const int ErrorStateResolverCouldNotResolveType = 1004;
        public const int ErrorStateMachineIsStarted = 1007;
        public const int ErrorMultipleInitialStates = 1008;
        public const int ErrorCannotMixGuardedAndGuardlessTransitions = 1009;
        public const int ErrorTriggerAlreadyConfiguredForState = 1010;
    }
}