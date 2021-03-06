﻿using System;
using Composable.System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace AccountManagement.UserStories
{
    [TestFixture] public class _030_When_a_user_attempt_to_change_their_password_the_operation_fails_if : UserStoryTest
    {
        [Test] public void New_password_is_invalid() =>
            TestData.Passwords.Invalid.All.ForEach(invalidPassword => Scenario.ChangePassword().WithNewPassword(invalidPassword).ExecutingShouldThrow<Exception>());

        [Test] public void OldPassword_is_null() => Scenario.ChangePassword().WithOldPassword(null).ExecutingShouldThrow<Exception>();

        [Test] public void OldPassword_is_empty_string() => Scenario.ChangePassword().WithOldPassword("").ExecutingShouldThrow<Exception>();

        [Test] public void OldPassword_is_not_the_current_password_of_the_account()
        {
            Scenario.ChangePassword().WithOldPassword("Wrong").ExecutingShouldThrow<Exception>().And.Message.ToLower().Should().Contain("password").And.Contain("wrong");
            Host.AssertThrown<Exception>().Message.ToLower().Should().Contain("password").And.Contain("wrong");
        }
    }
}
