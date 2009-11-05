﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using Void.Linq;

namespace Core.Tests
{
    [TestFixture]
    public class SeqTests
    {
        [Test]
        public void FromShouldIteraterFromThisParam()
        {
            Assert.That(Seq.From(12).First(), Is.EqualTo(12));
        }

        [Test]
        public void ToShouldHaveLastElementEqualToArgument()
        {
            Assert.That(1.To(12).Last(), Is.EqualTo(12));
        }

        [Test]
        public void ToShouldHaveCountEqualToToMinusFromPlus1()
        {
            Assert.That(12.To(20).Count(), Is.EqualTo(20-12+1));
        }

        [Test]
        public void StepSizeShouldIterateFromThisParam()
        {
            Assert.That(12.By(2).First(), Is.EqualTo(12));
        }


        [Test]
        public void StepSizeShouldStepByStepsize()
        {
            Assert.That(12.By(2).Second(), Is.EqualTo(14));
            Assert.That(12.By(3).Second(), Is.EqualTo(15));
            Assert.That(12.By(3).Second(), Is.EqualTo(15));
            Assert.That(12.By(3).Third(), Is.EqualTo(18));
        }

        [Test]
        public void CreateShouldEnumerateAllParamsInOrder()
        {
            var oneToTen = 1.To(10);
            Assert.That(Seq.Create(oneToTen.ToArray()), Is.EquivalentTo(oneToTen));
        }
    }
}
