﻿using System;
using System.Collections.Generic;
using System.Linq;
using Composable.DependencyInjection;
using Composable.DependencyInjection.Testing;
using Composable.GenericAbstractions.Time;
using Composable.Logging;
using Composable.Persistence.EventStore;
using Composable.Persistence.EventStore.Refactoring.Migrations;
using Composable.System;
using Composable.System.Linq;
using Composable.Tests.CQRS.EventRefactoring.Migrations.Events;
using FluentAssertions;
using NUnit.Framework;
using IEventStore = Composable.DependencyInjection.Persistence.IEventStore<Composable.Tests.ITestingEventStoreUpdater, Composable.Tests.ITestingEventStoreReader>;

// ReSharper disable AccessToDisposedClosure
// ReSharper disable AccessToModifiedClosure

namespace Composable.Tests.CQRS.EventRefactoring.Migrations
{
    [TestFixture]
    public abstract class EventMigrationTest : EventMigrationTestBase
    {
        protected EventMigrationTest(Type eventStoreType) : base(eventStoreType) { }

        [Test]
        public void Base_class_method_should_detect_incorrect_type_order()
        {
            this.Invoking(
                _ => RunMigrationTest(
                    new MigrationScenario(
                        Seq.OfTypes<Ec1, E1, Ef, Ef>(),
                        Seq.OfTypes<Ec1, Ef, E2, Ef>())))
                .ShouldThrow<Exception>();
        }

        [Test]
        public void Replacing_E1_with_E2()
        {
            RunMigrationTest(new MigrationScenario(
            Seq.OfTypes<Ec1, E1, Ef, Ef>(),
            Seq.OfTypes<Ec1, E2, Ef, Ef>(),
            Replace<E1>.With<E2>()));
        }

        [Test]
        public void Replacing_E1_with_E2_at_end_of_stream()
        {
            RunMigrationTest(new MigrationScenario(
            Seq.OfTypes<Ec1, E1>(),
            Seq.OfTypes<Ec1, E2>(),
            Replace<E1>.With<E2>()));
        }

        [Test]
        public void Replacing_E1_with_E2_E3_at_end_of_stream()
        {
            RunMigrationTest(new MigrationScenario(
            Seq.OfTypes<Ec1, E1>(),
            Seq.OfTypes<Ec1, E2, E3>(),
            Replace<E1>.With<E2, E3>()));
        }

        [Test]
        public void Replacing_E1_with_E2_E3()
        {
            RunMigrationTest(new MigrationScenario(
            Seq.OfTypes<Ec1, E1, Ef>(),
            Seq.OfTypes<Ec1, E2, E3, Ef>(),
            Replace<E1>.With<E2, E3>()));
        }

        [Test]
        public void Replacing_E1_with_E2_E3_2()
        {
            RunMigrationTest(new MigrationScenario(
            Seq.OfTypes<Ec1, E1, Ef, Ef, Ef, Ef>(),
            Seq.OfTypes<Ec1, E2, E3, Ef, Ef, Ef, Ef>(),
            Replace<E1>.With<E2, E3>()));
        }

        [Test]
        public void Replacing_E1_with_E2_then_irrelevant_migration()
        {
            RunMigrationTest(new MigrationScenario(
            Seq.OfTypes<Ec1, E1, Ef>(),
            Seq.OfTypes<Ec1, E2, Ef>(),
            Replace<E1>.With<E2>(),
            Replace<E1>.With<E5>()));
        }

        [Test]
        public void Replacing_E1_with_E2_E3_then_an_unrelated_migration_v2()
        {
            RunMigrationTest(new MigrationScenario(
            Seq.OfTypes<Ec1, E1, Ef>(),
            Seq.OfTypes<Ec1, E2, E3, Ef>(),
            Replace<E1>.With<E2, E3>(),
            Replace<E1>.With<E5>()));
        }

        [Test]
        public void Replacing_E1_with_E2_E3_then_E2_with_E4()
        {
            RunMigrationTest(new MigrationScenario(
            Seq.OfTypes<Ec1, E1, Ef>(),
            Seq.OfTypes<Ec1, E4, E3, Ef>(),
            Replace<E1>.With<E2, E3>(),//Ec1, E2, E3, Ef
            Replace<E2>.With<E4>())); //Ec1, E4, E3, Ef
        }

        [Test]
        public void Inserting_E3_before_E1()
        {
            RunMigrationTest(new MigrationScenario(
            Seq.OfTypes<Ec1, E1, Ef>(),
            Seq.OfTypes<Ec1, E3, E1, Ef>(),
            Before<E1>.Insert<E3>()));
        }

        [Test]
        public void Inserting_E3_E4_before_E1()
        {
            RunMigrationTest(new MigrationScenario(
            Seq.OfTypes<Ec1, E1, Ef>(),
            Seq.OfTypes<Ec1, E3, E4, E1, Ef>(),
            Before<E1>.Insert<E3, E4>()));
        }

        [Test]
        public void Inserting_E2_before_E1_then_E3_before_E2()
        {
            RunMigrationTest(new MigrationScenario(
            Seq.OfTypes<Ec1, E1, Ef>(),
            Seq.OfTypes<Ec1, E3, E2, E1, Ef>(),
            Before<E1>.Insert<E2>(),
            Before<E2>.Insert<E3>()));
        }

        [Test]
        public void Inserting_E3_E4_before_E1_then_E5_before_E3()
        {
            RunMigrationTest(new MigrationScenario(
            Seq.OfTypes<Ec1, E1>(),
            Seq.OfTypes<Ec1, E5, E3, E4, E1>(),
            Before<E1>.Insert<E3, E4>(),//Ec1, E3, E4, E1
            Before<E3>.Insert<E5>())); //Ec1, E5, E3, E4, E1;
        }

        [Test]
        public void Given_Ec1_E1_Ef_Inserting_E3_E4_before_E1_then_E5_before_E4()
        {
            RunMigrationTest(new MigrationScenario(
            Seq.OfTypes<Ec1, E1, Ef>(),
            Seq.OfTypes<Ec1, E3, E5, E4, E1, Ef>(),
            Before<E1>.Insert<E3, E4>(), //Ec1, E3, E4, E1, Ef
            Before<E4>.Insert<E5>())); //Ec1, E3, E5, E4, E1, Ef
        }

        [Test]
        public void Given_Ec1_E1_Inserting_E2_before_E1_then_E3_before_E2()
        {
            RunMigrationTest(new MigrationScenario(
            Seq.OfTypes<Ec1, E1>(),
            Seq.OfTypes<Ec1, E3, E2, E1>(),
            Before<E1>.Insert<E2>(), //Ec1, E2, E1
            Before<E2>.Insert<E3>())); //Ec1, E3, E2, E1
        }

        [Test]
        public void Given_Ec1_E1_Inserting_E3_E2_before_E1_then_E4_before_E3()
        {
            RunMigrationTest(new MigrationScenario(
            Seq.OfTypes<Ec1, E1>(),
            Seq.OfTypes<Ec1, E5, E4, E3, E2, E1>(),
            Before<E1>.Insert<E3, E2>(), //Ec1, E3, E2, E1
            Before<E3>.Insert<E4>(),
            Before<E4>.Insert<E5>())); //Ec1, E4, E3, E2, E1
        }

        [Test]
        public void Inserting_E3_E4_before_E1_then_E5_before_E4_then_replace_E4_with_E6()
        {
            RunMigrationTest(new MigrationScenario(
            Seq.OfTypes<Ec1, E1, Ef>(),
            Seq.OfTypes<Ec1, E6, E5, E4, E1, Ef>(),
            Before<E1>.Insert<E3, E4>(), //Ec1, E3, E4, E1, Ef
            Before<E4>.Insert<E5>(), //Ec1, E3, E5, E4, E1, Ef
            Replace<E3>.With<E6>())); //Ec1, E6, E5, E4, E1, Ef
        }

        [Test]
        public void Inserting_E3_E4_before_E1_then_E5_before_E4_then_replace_E4_with_E6_then_replace_Ef_with_E7_then_insert_E8_after_E7()
        {
            RunMigrationTest(new MigrationScenario
                (Seq.OfTypes<Ec1, E1, Ef>(),
                Seq.OfTypes<Ec1, E6, E5, E4, E1, E7, E8>(),
                Before<E1>.Insert<E3, E4>(), //Ec1, E3, E4, E1, Ef
                Before<E4>.Insert<E5>(), //Ec1, E3, E5, E4, E1, Ef
                Replace<E3>.With<E6>(), //Ec1, E6, E5, E4, E1, Ef
                Replace<Ef>.With<E7>(), //Ec1, E6, E5, E4, E1, E7
                After<E7>.Insert<E8>())); //Ec1, E6, E5, E4, E1, E7, E8
        }

        [Test]
        public void Inserting_E3_E4_before_E1_then_E5_before_E3_2()
        {
            RunMigrationTest(new MigrationScenario(
            Seq.OfTypes<Ec1, E1, Ef, Ef>(),
            Seq.OfTypes<Ec1, E5, E3, E4, E1, Ef, Ef>(),
            Before<E1>.Insert<E3, E4>(), //Ec1, E3, E4, E1, Ef, Ef
            Before<E3>.Insert<E5>())); //Ec1, E5, E3, E4, E1, Ef, Ef
        }

        [Test]
        public void Inserting_E3_E4_before_E1_then_E5_before_E4_2()
        {
            RunMigrationTest(new MigrationScenario(
            Seq.OfTypes<Ec1, E1, Ef, Ef>(),
            Seq.OfTypes<Ec1, E3, E5, E4, E1, Ef, Ef>(),
            Before<E1>.Insert<E3, E4>(),//Ec1, E3, E4 E1, Ef, Ef
            Before<E4>.Insert<E5>())); //Ec1, E3, E5, E4, E1, Ef, Ef
        }

        [Test]
        public void Inserting_E2_after_E1()
        {
            RunMigrationTest(new MigrationScenario(
            Seq.OfTypes<Ec1, E1, Ef, Ef>(),
            Seq.OfTypes<Ec1, E1, E2, Ef, Ef>(),
            After<E1>.Insert<E2>()));

        }

        [Test]
        public void Inserting_E2_after_E1_at_end_of_stream()
        {
            RunMigrationTest(new MigrationScenario(
            Seq.OfTypes<Ec1, E1>(),
            Seq.OfTypes<Ec1, E1, E2>(),
            After<E1>.Insert<E2>()));
        }

        [Test]
        public void Given_Ec1_E1_before_E1_E2_after_E2_E3_throws_NonIdempotentMigrationDetectedException()
        {
            this.Invoking(
                me =>
                RunMigrationTest(
                    new MigrationScenario(
                        Seq.OfTypes<Ec1, E1>(),
                        Seq.OfTypes<Ec1, E2, E3, E1>(),
                        Before<E1>.Insert<E2>(),
                        After<E2>.Insert<E3>())))
                .ShouldThrow<NonIdempotentMigrationDetectedException>();
        }

        [Test]
        public void PersistingMigrationsOfTheSameAggregateMultipleTimes()
        {
            var emptyMigrationsArray = new IEventMigration[0];
            IReadOnlyList<IEventMigration> migrations = emptyMigrationsArray;

            var toDispose = StrictAggregateDisposable.Create();
            try
            {
                var serviceLocator = CreateServiceLocatorForEventStoreType(() => migrations, EventStoreType);
                toDispose.Add(serviceLocator);

                var id = Guid.Parse("00000000-0000-0000-0000-000000000001");

                serviceLocator.Resolve<TestingTimeSource>().FreezeAtUtcTime(DateTime.Parse("2001-01-01 12:00"));

                var aggregate = TestAggregate.FromEvents(serviceLocator.Resolve<IUtcTimeTimeSource>(), id, Seq.OfTypes<Ec1, E1, E2, E3, E4>());
                var initialHistory = aggregate.History;

                ITestingEventStoreUpdater Session() => serviceLocator.Resolve<ITestingEventStoreUpdater>();
                IEventStore EventStore() => serviceLocator.Resolve<IEventStore>();

                var firstSavedHistory = serviceLocator.ExecuteTransactionInIsolatedScope(
                    () =>
                    {
                        Session().Save(aggregate);
                        return Session().Get<TestAggregate>(id).History;
                    });


                AssertStreamsAreIdentical(initialHistory, firstSavedHistory, "first saved history");

                migrations = Seq.Create(Replace<E1>.With<E5>()).ToList();
                ClearCache(serviceLocator);

                var migratedHistory = serviceLocator.ExecuteTransactionInIsolatedScope(() => Session().Get<TestAggregate>(id).History);
                var expectedAfterReplacingE1WithE5 =
                    TestAggregate.FromEvents(serviceLocator.Resolve<IUtcTimeTimeSource>(), id, Seq.OfTypes<Ec1, E5, E2, E3, E4>()).History;
                AssertStreamsAreIdentical(expected: expectedAfterReplacingE1WithE5, migratedHistory: migratedHistory, descriptionOfHistory: "migrated history");

                var historyAfterPersistingButBeforeReload = serviceLocator.ExecuteTransactionInIsolatedScope(
                    () =>
                    {
                        EventStore().PersistMigrations();
                        return Session().Get<TestAggregate>(id).History;
                    });

                AssertStreamsAreIdentical(expected: expectedAfterReplacingE1WithE5, migratedHistory: historyAfterPersistingButBeforeReload, descriptionOfHistory: "migrated, persisted");

                var historyAfterPersistingAndReloading = serviceLocator.ExecuteTransactionInIsolatedScope(() => Session().Get<TestAggregate>(id).History);
                AssertStreamsAreIdentical(expected: expectedAfterReplacingE1WithE5, migratedHistory: historyAfterPersistingAndReloading, descriptionOfHistory: "migrated, persisted, reloaded");

                migrations = Seq.Create(Replace<E2>.With<E6>()).ToList();

                toDispose.Add(serviceLocator = serviceLocator.Clone());

                migratedHistory = serviceLocator.ExecuteTransactionInIsolatedScope(() => Session().Get<TestAggregate>(id).History);
                var expectedAfterReplacingE2WithE6 = TestAggregate.FromEvents(serviceLocator.Resolve<IUtcTimeTimeSource>(), id, Seq.OfTypes<Ec1, E5, E6, E3, E4>()).History;
                AssertStreamsAreIdentical(expected: expectedAfterReplacingE2WithE6, migratedHistory: migratedHistory, descriptionOfHistory: "migrated history");

                historyAfterPersistingButBeforeReload = serviceLocator.ExecuteTransactionInIsolatedScope(
                    () =>
                    {
                        EventStore().PersistMigrations();
                        return Session().Get<TestAggregate>(id).History;
                    });

                AssertStreamsAreIdentical(expected: expectedAfterReplacingE2WithE6, migratedHistory: historyAfterPersistingButBeforeReload, descriptionOfHistory: "migrated, persisted");
                historyAfterPersistingAndReloading = serviceLocator.ExecuteTransactionInIsolatedScope(() => Session().Get<TestAggregate>(id).History);
                AssertStreamsAreIdentical(expected: expectedAfterReplacingE2WithE6, migratedHistory: historyAfterPersistingAndReloading, descriptionOfHistory: "migrated, persisted, reloaded");
            }finally
            {
                toDispose.Dispose();
            }

        }


        [Test]
        public void PersistingMigrationsOfTheSameAggregateMultipleTimesWithEventsAddedInTheMiddleAndAfter()
        {
            var emptyMigrationsArray = new IEventMigration[0];
            IReadOnlyList<IEventMigration> migrations = emptyMigrationsArray;

            var toDispose = StrictAggregateDisposable.Create();
            try
            {
                var serviceLocator = CreateServiceLocatorForEventStoreType(() => migrations, EventStoreType);
                toDispose.Add(serviceLocator);

                var id = Guid.Parse("00000000-0000-0000-0000-000000000001");

                serviceLocator.Resolve<TestingTimeSource>().FreezeAtUtcTime(DateTime.Parse("2001-01-01 12:00"));

                var aggregate = TestAggregate.FromEvents(serviceLocator.Resolve<IUtcTimeTimeSource>(), id, Seq.OfTypes<Ec1, E1, E2, E3, E4>());
                var initialHistory = aggregate.History;

                ITestingEventStoreUpdater Session() => serviceLocator.Resolve<ITestingEventStoreUpdater>();
                IEventStore EventStore() => serviceLocator.Resolve<IEventStore>();

                var firstSavedHistory = serviceLocator.ExecuteTransactionInIsolatedScope(
                    () =>
                    {
                        Session().Save(aggregate);
                        return Session().Get<TestAggregate>(id).History;
                    });


                AssertStreamsAreIdentical(initialHistory, firstSavedHistory, "first saved history");

                migrations = Seq.Create(Replace<E1>.With<E5>()).ToList();
                ClearCache(serviceLocator);

                var migratedHistory = serviceLocator.ExecuteTransactionInIsolatedScope(() => Session().Get<TestAggregate>(id).History);
                var expectedAfterReplacingE1WithE5 =
                    TestAggregate.FromEvents(serviceLocator.Resolve<IUtcTimeTimeSource>(), id, Seq.OfTypes<Ec1, E5, E2, E3, E4>()).History;
                AssertStreamsAreIdentical(expected: expectedAfterReplacingE1WithE5, migratedHistory: migratedHistory, descriptionOfHistory: "migrated history");

                var historyAfterPersistingButBeforeReload = serviceLocator.ExecuteTransactionInIsolatedScope(
                    () =>
                    {
                        EventStore().PersistMigrations();
                        return Session().Get<TestAggregate>(id).History;
                    });

                AssertStreamsAreIdentical(expected: expectedAfterReplacingE1WithE5, migratedHistory: historyAfterPersistingButBeforeReload, descriptionOfHistory: "migrated, persisted");

                var historyAfterPersistingAndReloading = serviceLocator.ExecuteTransactionInIsolatedScope(() => Session().Get<TestAggregate>(id).History);
                AssertStreamsAreIdentical(expected: expectedAfterReplacingE1WithE5, migratedHistory: historyAfterPersistingAndReloading, descriptionOfHistory: "migrated, persisted, reloaded");

                serviceLocator.ExecuteTransactionInIsolatedScope(() => Session().Get<TestAggregate>(id).Publish(new E6(), new E7()));

                migrations = Seq.Create(Replace<E2>.With<E6>()).ToList();
                toDispose.Add(serviceLocator = serviceLocator.Clone());

                migratedHistory = serviceLocator.ExecuteTransactionInIsolatedScope(() => Session().Get<TestAggregate>(id).History);
                var expectedAfterReplacingE2WithE6 = TestAggregate.FromEvents(serviceLocator.Resolve<IUtcTimeTimeSource>(), id, Seq.OfTypes<Ec1, E5, E6, E3, E4, E6, E7>()).History;
                AssertStreamsAreIdentical(expected: expectedAfterReplacingE2WithE6, migratedHistory: migratedHistory, descriptionOfHistory: "migrated history");

                historyAfterPersistingButBeforeReload = serviceLocator.ExecuteTransactionInIsolatedScope(
                    () =>
                    {
                        EventStore().PersistMigrations();
                        return Session().Get<TestAggregate>(id).History;
                    });

                AssertStreamsAreIdentical(expected: expectedAfterReplacingE2WithE6, migratedHistory: historyAfterPersistingButBeforeReload, descriptionOfHistory: "migrated, persisted");
                historyAfterPersistingAndReloading = serviceLocator.ExecuteTransactionInIsolatedScope(() => Session().Get<TestAggregate>(id).History);
                AssertStreamsAreIdentical(expected: expectedAfterReplacingE2WithE6, migratedHistory: historyAfterPersistingAndReloading, descriptionOfHistory: "migrated, persisted, reloaded");

                migrations = Seq.Empty<IEventMigration>().ToList();
                serviceLocator.ExecuteTransactionInIsolatedScope(() => Session().Get<TestAggregate>(id).Publish(new E8(), new E9()));
                historyAfterPersistingAndReloading = serviceLocator.ExecuteTransactionInIsolatedScope(() => Session().Get<TestAggregate>(id).History);
                var expectedAfterReplacingE2WithE6AndRaisingE8E9 = TestAggregate.FromEvents(serviceLocator.Resolve<IUtcTimeTimeSource>(), id, Seq.OfTypes<Ec1, E5, E6, E3, E4, E6, E7, E8, E9>()).History;
                AssertStreamsAreIdentical(expected: expectedAfterReplacingE2WithE6AndRaisingE8E9, migratedHistory: historyAfterPersistingAndReloading, descriptionOfHistory: "migrated, persisted, reloaded");

            }
            finally
            {
                toDispose.Dispose();
            }

        }

        [Test]
        public void UpdatingAnAggregateAfterPersistingMigrations()
        {
            var emptyMigrationsArray = new IEventMigration[0];
            IReadOnlyList<IEventMigration> migrations = emptyMigrationsArray;

            var toDispose = StrictAggregateDisposable.Create();
            try
            {
                var serviceLocator = CreateServiceLocatorForEventStoreType(() => migrations, EventStoreType);
                toDispose.Add(serviceLocator);

                var id = Guid.Parse("00000000-0000-0000-0000-000000000001");

                serviceLocator.Resolve<TestingTimeSource>().FreezeAtUtcTime(DateTime.Parse("2001-01-01 12:00"));

                var initialAggregate = TestAggregate.FromEvents(serviceLocator.Resolve<IUtcTimeTimeSource>(), id, Seq.OfTypes<Ec1, E1>());

                ITestingEventStoreUpdater Session() => serviceLocator.Resolve<ITestingEventStoreUpdater>();
                IEventStore EventStore() => serviceLocator.Resolve<IEventStore>();

                serviceLocator.ExecuteTransactionInIsolatedScope(() => Session().Save(initialAggregate));

                migrations = Seq.Create(Replace<E1>.With<E5>()).ToList();

                serviceLocator.ExecuteTransactionInIsolatedScope(() => EventStore().PersistMigrations());

                migrations = Seq.Create<IEventMigration>().ToList();

                serviceLocator.ExecuteTransactionInIsolatedScope(() => Session().Get<TestAggregate>(id).Publish(new E2()));

                var aggregate = serviceLocator.ExecuteTransactionInIsolatedScope(() => Session().Get<TestAggregate>(id));

                var expected = TestAggregate.FromEvents(serviceLocator.Resolve<IUtcTimeTimeSource>(), id, Seq.OfTypes<Ec1, E5, E2>()).History;
                AssertStreamsAreIdentical(expected: expected, migratedHistory: aggregate.History, descriptionOfHistory: "migrated history");

                var completeEventHistory =serviceLocator.ExecuteInIsolatedScope(() => EventStore().ListAllEventsForTestingPurposesAbsolutelyNotUsableForARealEventStoreOfAnySize()).Cast<AggregateEvent>();
                AssertStreamsAreIdentical(expected: expected, migratedHistory: completeEventHistory, descriptionOfHistory: "streamed persisted history");

                SafeConsole.WriteLine("Version");
                SafeConsole.WriteLine("Aggregate Effective Inserted Manual");
                SafeConsole.WriteLine("A E I M");
                completeEventHistory.ForEach(@event => SafeConsole.WriteLine($"{@event.AggregateVersion} {@event.EffectiveVersion} {@event.InsertedVersion} {@event.ManualVersion}"));

                toDispose.Add(serviceLocator = serviceLocator.Clone());

                completeEventHistory = serviceLocator.ExecuteInIsolatedScope(() => EventStore().ListAllEventsForTestingPurposesAbsolutelyNotUsableForARealEventStoreOfAnySize()).Cast<AggregateEvent>();
                AssertStreamsAreIdentical(expected: expected, migratedHistory: completeEventHistory, descriptionOfHistory: "streamed persisted history");
            }
            finally
            {
                toDispose.Dispose();
            }
        }

        [Test]
        public void Inserting_E2_Before_E1_Persisting_and_then_Inserting_E3_before_E1()
        {
            var firstMigration = Seq.Create(Before<E1>.Insert<E2>()).ToArray();
            var secondMigration = Seq.Create(Before<E1>.Insert<E3>()).ToArray();
            IReadOnlyList<IEventMigration> migrations = new List<IEventMigration>();

            var toDispose = StrictAggregateDisposable.Create();
            try
            {
                var serviceLocator = CreateServiceLocatorForEventStoreType(() => migrations, EventStoreType);
                toDispose.Add(serviceLocator);
                serviceLocator.Resolve<TestingTimeSource>().FreezeAtUtcTime(DateTime.Parse("2001-01-01 12:00"));

                var id = Guid.Parse("00000000-0000-0000-0000-000000000001");
                var initialAggregate = TestAggregate.FromEvents(serviceLocator.Resolve<IUtcTimeTimeSource>(), id, Seq.OfTypes<Ec1, E1>());
                var expectedHistoryAfterFirstMigration = TestAggregate.FromEvents(serviceLocator.Resolve<IUtcTimeTimeSource>(), id, Seq.OfTypes<Ec1, E2, E1>()).History;
                var expectedHistoryAfterSecondMigration = TestAggregate.FromEvents(serviceLocator.Resolve<IUtcTimeTimeSource>(), id, Seq.OfTypes<Ec1, E2, E3, E1>()).History;

                ITestingEventStoreUpdater Session() => serviceLocator.Resolve<ITestingEventStoreUpdater>();
                IEventStore EventStore() => serviceLocator.Resolve<IEventStore>();

                serviceLocator.ExecuteTransactionInIsolatedScope(() => Session().Save(initialAggregate));
                migrations = firstMigration;
                ClearCache(serviceLocator);
                var historyWithFirstMigrationUnPersisted = serviceLocator.ExecuteTransactionInIsolatedScope(() => Session().Get<TestAggregate>(id).History);

                serviceLocator.ExecuteTransactionInIsolatedScope(() => EventStore().PersistMigrations());
                var historyAfterPersistingFirstMigration = serviceLocator.ExecuteTransactionInIsolatedScope(() => Session().Get<TestAggregate>(id).History);
                AssertStreamsAreIdentical(expectedHistoryAfterFirstMigration, historyWithFirstMigrationUnPersisted, nameof(historyWithFirstMigrationUnPersisted));
                AssertStreamsAreIdentical(expectedHistoryAfterFirstMigration, historyAfterPersistingFirstMigration, nameof(historyAfterPersistingFirstMigration));

                migrations = secondMigration;
                toDispose.Add(serviceLocator = serviceLocator.Clone());
                var historyWithSecondMigrationUnPersisted = serviceLocator.ExecuteTransactionInIsolatedScope(() => Session().Get<TestAggregate>(id).History);

                serviceLocator.ExecuteTransactionInIsolatedScope(() => EventStore().PersistMigrations());
                var historyAfterPersistingSecondMigration = serviceLocator.ExecuteTransactionInIsolatedScope(() => Session().Get<TestAggregate>(id).History);
                AssertStreamsAreIdentical(expectedHistoryAfterSecondMigration, historyWithSecondMigrationUnPersisted, nameof(historyWithSecondMigrationUnPersisted));
                AssertStreamsAreIdentical(expectedHistoryAfterSecondMigration, historyAfterPersistingSecondMigration, nameof(historyAfterPersistingSecondMigration));
            }
            finally
            {
                toDispose.Dispose();
            }
        }

        [Test]
        public void Inserting_E2_After_E1_Persisting_and_then_Inserting_E3_after_E1()
        {
            var firstMigration = Seq.Create(After<E1>.Insert<E2>()).ToArray();
            var secondMigration = Seq.Create(After<E1>.Insert<E3>()).ToArray();
            IReadOnlyList<IEventMigration> migrations = new List<IEventMigration>();

            var toDispose = StrictAggregateDisposable.Create();
            try
            {
                var serviceLocator = CreateServiceLocatorForEventStoreType(() => migrations, EventStoreType);
                toDispose.Add(serviceLocator);
                serviceLocator.Resolve<TestingTimeSource>().FreezeAtUtcTime(DateTime.Parse("2001-01-01 12:00"));

                var id = Guid.Parse("00000000-0000-0000-0000-000000000001");
                var initialAggregate = TestAggregate.FromEvents(serviceLocator.Resolve<IUtcTimeTimeSource>(), id, Seq.OfTypes<Ec1, E1>());
                var expectedHistoryAfterFirstMigration = TestAggregate.FromEvents(serviceLocator.Resolve<IUtcTimeTimeSource>(), id, Seq.OfTypes<Ec1, E1, E2>()).History;
                var expectedHistoryAfterSecondMigration = TestAggregate.FromEvents(serviceLocator.Resolve<IUtcTimeTimeSource>(), id, Seq.OfTypes<Ec1, E1, E3, E2>()).History;

                ITestingEventStoreUpdater Session() => serviceLocator.Resolve<ITestingEventStoreUpdater>();
                IEventStore EventStore() => serviceLocator.Resolve<IEventStore>();

                serviceLocator.ExecuteTransactionInIsolatedScope(() => Session().Save(initialAggregate));
                migrations = firstMigration;
                ClearCache(serviceLocator);
                var historyWithFirstMigrationUnPersisted = serviceLocator.ExecuteTransactionInIsolatedScope(() => Session().Get<TestAggregate>(id).History);

                serviceLocator.ExecuteTransactionInIsolatedScope(() => EventStore().PersistMigrations());
                var historyAfterPersistingFirstMigration = serviceLocator.ExecuteTransactionInIsolatedScope(() => Session().Get<TestAggregate>(id).History);
                AssertStreamsAreIdentical(expectedHistoryAfterFirstMigration, historyWithFirstMigrationUnPersisted, nameof(historyWithFirstMigrationUnPersisted));
                AssertStreamsAreIdentical(expectedHistoryAfterFirstMigration, historyAfterPersistingFirstMigration, nameof(historyAfterPersistingFirstMigration));

                migrations = secondMigration;
                toDispose.Add(serviceLocator = serviceLocator.Clone());
                var historyWithSecondMigrationUnPersisted = serviceLocator.ExecuteTransactionInIsolatedScope(() => Session().Get<TestAggregate>(id).History);

                serviceLocator.ExecuteTransactionInIsolatedScope(() => EventStore().PersistMigrations());
                var historyAfterPersistingSecondMigration = serviceLocator.ExecuteTransactionInIsolatedScope(() => Session().Get<TestAggregate>(id).History);
                AssertStreamsAreIdentical(expectedHistoryAfterSecondMigration, historyWithSecondMigrationUnPersisted, nameof(historyWithSecondMigrationUnPersisted));
                AssertStreamsAreIdentical(expectedHistoryAfterSecondMigration, historyAfterPersistingSecondMigration, nameof(historyAfterPersistingSecondMigration));
            }
            finally
            {
                toDispose.Dispose();
            }
        }

        [Test]
        public void Inserting_E2_Before_E1()
        {
            RunMigrationTest(
                new MigrationScenario(
                    Seq.OfTypes<Ec1, E1>(),
                    Seq.OfTypes<Ec1, E2, E1>(),
                    Before<E1>.Insert<E2>()));
        }
    }
}
