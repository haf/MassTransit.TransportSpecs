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
using System.Collections.Generic;
using MassTransit.Serialization;
using MassTransit.Transports;
using MassTransit.Transports.AzureServiceBus;
using MassTransit.Transports.Loopback;
using MassTransit.Transports.Msmq;
using MassTransit.Transports.RabbitMq;
using NUnit.Framework;

namespace MassTransit.TransportSpecs
{
	interface ForAllContext {}

	[TestFixture(typeof(BinaryMessageSerializer), typeof(LoopbackTransportFactory))]
	[TestFixture(typeof(BsonMessageSerializer), typeof(LoopbackTransportFactory))]
	[TestFixture(typeof(JsonMessageSerializer), typeof(LoopbackTransportFactory))]
	[TestFixture(typeof(XmlMessageSerializer), typeof(LoopbackTransportFactory))]
	[TestFixture(typeof(VersionOneXmlMessageSerializer), typeof(LoopbackTransportFactory))]

	[TestFixture(typeof(BinaryMessageSerializer), typeof(MsmqTransportFactory))]
	[TestFixture(typeof(BsonMessageSerializer), typeof(MsmqTransportFactory))]
	[TestFixture(typeof(JsonMessageSerializer), typeof(MsmqTransportFactory))]
	[TestFixture(typeof(XmlMessageSerializer), typeof(MsmqTransportFactory))]
	[TestFixture(typeof(VersionOneXmlMessageSerializer), typeof(MsmqTransportFactory))]

	[TestFixture(typeof(BinaryMessageSerializer), typeof(RabbitMqTransportFactory))]
	[TestFixture(typeof(BsonMessageSerializer), typeof(RabbitMqTransportFactory))]
	[TestFixture(typeof(JsonMessageSerializer), typeof(RabbitMqTransportFactory))]
	[TestFixture(typeof(XmlMessageSerializer), typeof(RabbitMqTransportFactory))]
	[TestFixture(typeof(VersionOneXmlMessageSerializer), typeof(RabbitMqTransportFactory))]

	[TestFixture(typeof(BinaryMessageSerializer), typeof(TransportFactoryImpl))]
	[TestFixture(typeof(BsonMessageSerializer), typeof(TransportFactoryImpl))]
	[TestFixture(typeof(JsonMessageSerializer), typeof(TransportFactoryImpl))]
	[TestFixture(typeof(XmlMessageSerializer), typeof(TransportFactoryImpl))]
	[TestFixture(typeof(VersionOneXmlMessageSerializer), typeof(TransportFactoryImpl))]

	[Timeout(20000)]
	public abstract class ForAll_context<TSerializer, TTransportFac>
		: ForAllContext
		where TTransportFac : class, ITransportFactory, new()
		where TSerializer : class, IMessageSerializer, new()
	{
		private TTransportFac _transportFactory;

		// parameters: http://nunit.org/index.php?p=testFixture&r=2.5

		/// <summary>Gets all supported message sizes in bytes</summary>
		protected virtual IEnumerable<int> MessageSizes
		{
			get { return new[]{ 2 * 1024, 20 * 1024, 200000 * 1024, 256 * 1024, 1024 * 1024 };}
		}

		protected virtual TTransportFac TransportFactory
		{
			get { return _transportFactory ?? (_transportFactory = Activator.CreateInstance<TTransportFac>()); }
		}
	}
}