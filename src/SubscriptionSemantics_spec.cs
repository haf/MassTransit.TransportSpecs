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
using MassTransit.Transports.Loopback;
using NUnit.Framework;
using SharpTestsEx;

namespace MassTransit.TransportSpecs
{
	[TestFixture(typeof(JsonMessageSerializer), typeof(LoopbackTransportFactory))]
	[SingleServiceBus]
	public class subscribe_empty_interface_spec<TSerializer, TTransportFac>
		: SingleServiceBusFixture
		where TTransportFac : class, ITransportFactory, new()
		where TSerializer : class, IMessageSerializer, new()
	{
		public IServiceBus ServiceBus { get; set; }

		public Action<ServiceBusConfigurator> ConfigureServiceBus
		{
			get
			{
				return sbc => sbc.Subscribe(s => s.Handler<EmptyInterface>(_gottenMessage.Complete));
			}
		}

		Future<EmptyInterface> _gottenMessage;

		public void Given()
		{
			Console.WriteLine("Given Called");
			_gottenMessage = new Future<EmptyInterface>();
		}

		public void When()
		{
			ServiceBus.Publish<EmptyInterface>(new { });
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

	[SingleServiceBus]
	public class subscribe_interface_with_data_spec<TSerializer, TTransportFac>
		: ForAll_context<TSerializer, TTransportFac>,
		SingleServiceBusFixture
		where TTransportFac : class, ITransportFactory, new()
		where TSerializer : class, IMessageSerializer, new()
	{
		private const string DataContents = "a b c d å ä ö ü";

		public void Given()
		{
			_gottenMessage = new Future<MessageWithData>();
		}

		public IServiceBus ServiceBus { get; set; }

		public Action<ServiceBusConfigurator> ConfigureServiceBus
		{
			get
			{
				return sbc => sbc.Subscribe(s => 
					s.Handler<MessageWithData>(_gottenMessage.Complete));
			}
		}

		Future<MessageWithData> _gottenMessage;

		public void When()
		{
			ServiceBus.Publish<MessageWithData>(new
				{
					StringMessage = DataContents
				});
		}

		[Test]
		public void should_have_received()
		{
			_gottenMessage
				.WaitUntilCompleted(5.Seconds())
				.Should("have received the message")
				.Be.True();
		}

		[Test]
		public void should_have_same_data()
		{
			_gottenMessage
				.Value
				.StringMessage
				.Should("be the same as the sent data")
				.Be.EqualTo(DataContents);
		}

		[Test]
		public void should_have_subscription()
		{
			ServiceBus.ShouldHaveSubscriptionFor<MessageWithData>();
		}
	}
}