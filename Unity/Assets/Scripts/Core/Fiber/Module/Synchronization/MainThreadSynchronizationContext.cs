﻿using System;
using System.Threading;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class MainThreadSynchronizationContext: Entity, IAwake
    {
        private readonly ThreadSynchronizationContext threadSynchronizationContext = new();

        public MainThreadSynchronizationContext()
        {
            SynchronizationContext.SetSynchronizationContext(this.threadSynchronizationContext);
        }
        
        public void Update()
        {
            this.threadSynchronizationContext.Update();
        }
        
        public void Post(SendOrPostCallback callback, object state)
        {
            this.Post(() => callback(state));
        }
		
        public void Post(Action action)
        {
            this.threadSynchronizationContext.Post(action);
        }
    }
}