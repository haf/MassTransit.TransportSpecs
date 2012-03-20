// Copyright 2012 Henrik Feldt
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

using System;
using Magnum.Extensions;
using MassTransit.BusConfigurators;
using MassTransit.Serialization;
using MassTransit.TestFramework;
using MassTransit.TransportSpecs.SubscriptionMsgs;
using MassTransit.Transports;
using NUnit.Framework;
using SharpTestsEx;

namespace MassTransit.TransportSpecs
{
	[SingleServiceBus]
	public class subscribe_empty_interface_spec<TSerializer, TTransportFac>
		: ForAll_context<TSerializer, TTransportFac>,
		SingleServiceBusFixture
		where TTransportFac : class, ITransportFactory, new()
		where TSerializer : class, IMessageSerializer, new()
	{
		public IServiceBus ServiceBus { get; set; }

		readonly Future<EmptyInterface> _gottenMessage = new Future<EmptyInterface>();

		public Action<ServiceBusConfigurator> ConfigureServiceBus
		{
			get
			{
				return sbc => sbc.Subscribe(s => s.Handler<EmptyInterface>(_gottenMessage.Complete));
			}
		}

		public void Given()
		{
			ServiceBus.Publish<EmptyInterface>(new{});
		}

		[Test]
		public void should_have_received()
		{
			_gottenMessage
				.WaitUntilCompleted(5.Seconds())
				.Should("have received the message").Be.True();
		}

		[Test]
		public void should_have_subscription()
		{
			ServiceBus.ShouldHaveSubscriptionFor<EmptyInterface>();
		}
	}

	public class all_schemes_are_without_slashes<TSerializer, TTransportFac>
		: ForAll_context<TSerializer, TTransportFac>
		where TTransportFac : class, ITransportFactory, new()
		where TSerializer : class, IMessageSerializer, new()
	{
		[Test]
		public void then()
		{
			TransportFactory.Scheme
				.Should().Not.Contain("://");
		}
	}
}