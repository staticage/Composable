#region usings

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Threading;
using Composable.System.Linq;

#endregion

namespace Composable.DomainEvents
{
    //Review:mlidbo: REMOVE
    static class DomainEvent
    {
        static readonly ThreadLocal<List<Delegate>> ManualSubscribersStorage =
            new ThreadLocal<List<Delegate>>(() => new List<Delegate>());

        static List<Delegate> ManualSubscribers
        {
            get
            {
                Contract.Ensures(Contract.Result<List<Delegate>>() != null);
                Contract.Assume(ManualSubscribersStorage.Value != null);
                return ManualSubscribersStorage.Value;
            }
        }

        [ContractInvariantMethod] static void Invariants()
        {
            Contract.Invariant(ManualSubscribers != null);
        }
       
        [Obsolete("Only use if you are really sure you know what you are doing. Any use except to wrap synchronous calls in a using block may behave erratically with for instance the asp.net threading model...")]
        public static IDisposable RegisterShortTermSynchronousListener<T>(Action<T> callback) where T : IDomainEvent
        {
            ManualSubscribers.Add(callback);
            return new RemoveRegistration(callback);
        }

        class RemoveRegistration : IDisposable
        {
            readonly Delegate _callbackToRemove;

            public RemoveRegistration(Delegate callback)
            {
                _callbackToRemove = callback;
            }

            public void Dispose()
            {
                ManualSubscribers.Remove(_callbackToRemove);
            }
        }

        [ContractVerification(false)]
        public static void Raise<T>(T args) where T : IDomainEvent
        {
            Contract.Requires(args != null);
            //THis is called in tight loops occationally. Do not waste cycles on linq

            for(var index = 0; index < ManualSubscribers.Count; index++)
            {
                if(ManualSubscribers[index] is Action<T>)
                {
                    ((Action<T>)ManualSubscribers[index])(args);
                }
            }
        }
    }
}