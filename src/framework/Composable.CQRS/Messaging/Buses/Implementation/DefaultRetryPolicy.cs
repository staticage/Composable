﻿using System;

namespace Composable.Messaging.Buses.Implementation
{
    class DefaultRetryPolicy
    {
        internal static int Tries = 5;
        int _remainingTries;
        // ReSharper disable UnusedParameter.Local parameters are there to enable implementation to take the type of message and exception into account when deciding on whether or not to retry and how long to wait before retrying.
        internal DefaultRetryPolicy(BusApi.IMessage message) => _remainingTries = Tries;
        public bool TryAwaitNextRetryTimeForException(Exception exception) => --_remainingTries > 0;
        // ReSharper restore UnusedParameter.Local
    }
}
