using Assisticant;
using Assisticant.Descriptors;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace SimplestAssisticantTest
{
    [TestClass]
    public class ViewModelTests
    {
        private static Queue<Action> _updateQueue;

        [TestInitialize]
        public void Initialize()
        {
            if (_updateQueue == null)
            {
                _updateQueue = new Queue<Action>();
                UpdateScheduler.Initialize(a => _updateQueue.Enqueue(a));
            }
            _updateQueue.Clear();
        }

        [TestMethod]
        public void ViewModelPropertyChangeTest()
        {
            object proxy = ForView.Wrap(new TestModel()
            {
                Name = "TestName",
                Count = 6,
            });

            string nameAndCount = (string)GetValue<TestModel>(proxy, "NameAndCount");
            Assert.AreEqual(nameAndCount, "TestName - 6");

            bool propChanged = false;
            var inpc = (INotifyPropertyChanged)proxy;
            inpc.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                propChanged = true;
            };

            TestModel tmv = ForView.Unwrap<TestModel>(proxy);
            tmv.IncCount();

            //normally, this would happen automatically at then end of the UI thread message cycle
            //but because we're in test, there isn't one, so process the queued messages directly instead
            Process();

            nameAndCount = (string)GetValue<TestModel>(proxy, "NameAndCount");
            Assert.AreEqual(tmv.NameAndCount, "TestName - 7");

            propChanged.Should().BeTrue();
        }

        private void Process()
        {
            while (_updateQueue.Any())
                _updateQueue.Dequeue()();
        }

        private object GetValue<T>(object proxy, string propertyName)
        {
            return PlatformProxy<T>.TypeDescriptor
                .GetProperties()
                .OfType<PropertyDescriptor>()
                .Where(p => p.Name == propertyName)
                .Select(p => p.GetValue(proxy))
                .Single();
        }
    }
}
