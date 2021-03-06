﻿using System;
using Composable.DDD;
using JetBrains.Annotations;

namespace Composable.Tests.KeyValueStorage
{
    class Dog : IPersistentEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Name { get; [UsedImplicitly] set; }
    }
}