﻿using AccountManagement.Domain;
using AccountManagement.Domain.Services;
using Composable.DependencyInjection;

namespace AccountManagement.Tests.Scenarios
{
    public class ChangeAccountEmailScenario
    {
        readonly IServiceLocator _serviceLocator;

        public Email NewEmail = TestData.Email.CreateValidEmail();
        public readonly Email OldEmail;
        public readonly Account Account;

        //Review:mlidbo: Replace optional parameters in scenario constructors with constructor overloading throughout the sample project.
        public ChangeAccountEmailScenario(IServiceLocator serviceLocator, Account account = null)
        {
            _serviceLocator = serviceLocator;
            Account = account ?? new RegisterAccountScenario(serviceLocator).Execute();
            OldEmail = Account.Email;
        }

        public void Execute()
        {
            _serviceLocator.ExecuteTransaction(() => _serviceLocator.Use<IAccountRepository>(repo => repo.Get(Account.Id).ChangeEmail(NewEmail)));
        }
    }
}