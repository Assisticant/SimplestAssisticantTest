using Assisticant;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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
            ForView.Initialize();
            object proxy = ForView.Wrap(new TestModel()
            {
                Name = "TestName",
                Count = 6,
            });

            TestModel tmv = ForView.Unwrap<TestModel>(proxy);
            Assert.AreEqual(tmv.NameAndCount, "TestName - 6");

            bool propChanged = false;
            var pc = proxy.GetType().GetEvent("PropertyChanged");
            var te = Delegate.CreateDelegate(
                pc.EventHandlerType,
                null,
                new PropertyChangedEventHandler((object sender, PropertyChangedEventArgs e) =>
                {
                    propChanged = true;
                }).Method);

            pc.AddEventHandler(proxy, te);

            tmv.IncCount();

            //normally, this would happen automatically at then end of the UI thread message cycle
            //but because we're in test, there isn't one, so process the queued messages directly instead
            Process();


            Assert.AreEqual(tmv.NameAndCount, "TestName - 7");

            propChanged.Should().BeTrue();
        }

        private void Process()
        {
            while (_updateQueue.Any())
                _updateQueue.Dequeue()();
        }
    }
}
