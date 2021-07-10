﻿using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools.Constraints;
using VContainer.Unity;
using Is = NUnit.Framework.Is;

namespace VContainer.Tests.Unity
{
    public class PlayerLoopItemTest
    {
        private class Ticker : ITickable, IPostTickable, IFixedTickable
        {
            public void Tick() { }
            public void FixedTick() { }
            public void PostTick() { }
        }

        private class TickableLoopItemTest
        {
            [Test]
            public void MoveNextWithoutAllocation()
            {
                var list = new List<ITickable> { new Ticker(), new Ticker() };
                var exceptionHandler = new EntryPointExceptionHandler(exception => { });
                var tickableLoopItem = new TickableLoopItem(list, exceptionHandler);
                
                Assert.That(() =>
                {
                    tickableLoopItem.MoveNext();
                }, Is.Not.AllocatingGCMemory());
            }
        }
        
        private class PostTickableLoopItemTest
        {
            [Test]
            public void MoveNextWithoutAllocation()
            {
                var list = new List<IPostTickable> { new Ticker(), new Ticker() };
                var exceptionHandler = new EntryPointExceptionHandler(exception => { });
                var tickableLoopItem = new PostTickableLoopItem(list, exceptionHandler);
                
                Assert.That(() =>
                {
                    tickableLoopItem.MoveNext();
                }, Is.Not.AllocatingGCMemory());
            }
        }
        
        private class FixedTickableLoopItemTest
        {
            [Test]
            public void MoveNextWithoutAllocation()
            {
                var list = new List<IFixedTickable> { new Ticker(), new Ticker() };
                var exceptionHandler = new EntryPointExceptionHandler(exception => { });
                var tickableLoopItem = new FixedTickableLoopItem(list, exceptionHandler);
                
                Assert.That(() =>
                {
                    tickableLoopItem.MoveNext();
                }, Is.Not.AllocatingGCMemory());
            }
        }
    }
}
