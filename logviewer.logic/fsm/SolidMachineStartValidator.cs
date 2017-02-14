// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
// Created by: egr
// Created at: 24.11.2016
// © 2012-2016 Alexander Egorov

using FluentValidation;

namespace logviewer.logic.fsm
{
    public class SolidMachineStartValidator<TTrigger> : AbstractValidator<SolidMachine<TTrigger>>
    {
        public SolidMachineStartValidator()
        {
            this.RuleFor(x => x.InitialState).NotNull().WithMessage("No states have been configured!");
            this.RuleFor(x => x.StateResolver)
                .NotNull()
                .When(x => x.StateResolverRequired)
                .WithMessage(
                    "One or more configured states has no parameterless constructor. Add such constructors or make sure that the StateResolver property is set!");
        }
    }
}