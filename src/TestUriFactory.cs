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
using MassTransit.Transports;
using MassTransit.Transports.AzureServiceBus;
using MassTransit.Transports.AzureServiceBus.Tests.Framework;
using MassTransit.Transports.Loopback;
using MassTransit.Transports.Msmq;
using MassTransit.Transports.RabbitMq;

namespace MassTransit.TransportSpecs
{
	public static class TestUriFactory
	{
		public static Uri GetUriFor(ITransportFactory fac)
		{
			var asbAccountDetails = new AccountDetails();

			return new Dictionary<Type, Uri>
				{
					{ typeof (TransportFactoryImpl), asbAccountDetails.WithApplication("TransportSpecs").BuildUri() },
					{ typeof(MsmqTransportFactory), new Uri(fac.Scheme + "://localhost/TransportSpecs") },
					{ typeof(RabbitMqTransportFactory), new Uri(fac.Scheme + "://localhost/TransportSpecs") },
					{ typeof(LoopbackTransportFactory), new Uri(fac.Scheme + "://localhost/TransportSpecs")}
				}
				[fac.GetType()];
		}
	}
}