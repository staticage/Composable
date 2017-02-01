﻿using System;
using System.Collections.Generic;
using Castle.Windsor;
using Composable.ServiceBus;
using Composable.System.Transactions;
using System.Linq;
using Composable.System.Reflection;
using JetBrains.Annotations;

namespace Composable.CQRS.Testing
{
    public class DummyServiceBus : IServiceBus
    {
        List<Tuple<Type, Func<object, IEnumerable<IMessage>>>> _localHandlers = new List<Tuple<Type, Func<object, IEnumerable<IMessage>>>>();

        readonly IWindsorContainer _serviceLocator;

        public DummyServiceBus(IWindsorContainer serviceLocator)
        {
            _serviceLocator = serviceLocator;
        }

        readonly IList<object> _published = new List<object>();

        public IEnumerable<object> Published { get { return _published; } }

        public void Reset()
        {
            _localHandlers.Clear();
            _published.Clear();
        }

        public void Publish(object message)
        {
            ((dynamic)this).Publish((dynamic)message);
        }

        [UsedImplicitly] public void Publish<TMessage>(TMessage message) where TMessage : IMessage
        {
            var handlerTypes = message.GetType().GetAllTypesInheritedOrImplemented()                                
                .Where(t => t.Implements(typeof(IMessage)))
                .Select(t => typeof(IHandleMessages<>).MakeGenericType(t))
                .ToArray();

             _published.Add(message);

            var handlers = new List<object>();
            foreach(var handlerType in handlerTypes)
            {
                handlers.AddRange(_serviceLocator.ResolveAll(handlerType).Cast<object>());
            }

            var transformedMessages = new List<IMessage>();

            InTransaction.Execute(() =>
            {
                //var handlers = handlerTypes.SelectMany(type =>_serviceLocator.GetAllInstances(type)).ToArray();
                foreach(dynamic handler in handlers)
                {
                    handler.Handle((dynamic)message);
                }

                foreach (var handler in _localHandlers.Where(t => t.Item1.IsAssignableFrom(typeof(TMessage))))
                    transformedMessages.AddRange(handler.Item2(message));
            });

            transformedMessages.ForEach(Publish);
        }

        public void SendLocal(object message)
        {
            Publish(message);
        }

        public void Send(object message)
        {
            Publish(message);
        }

        public void Reply(object message)
        {
            Publish(message);
        }
        public void SendAtTime(DateTime sendAt, object message) { throw new NotImplementedException(); }
    }
}