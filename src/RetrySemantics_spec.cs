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
using MassTransit.BusConfigurators;
using MassTransit.Serialization;
using MassTransit.Transports;

namespace MassTransit.TransportSpecs
{
	[SingleServiceBus]
	public class when_five_retries_fail_spec<TSerializer, TTransportFac>
		: ForAll_context<TSerializer, TTransportFac>,
		SingleServiceBusFixture
		where TTransportFac : class, ITransportFactory, new()
		where TSerializer : class, IMessageSerializer, new()
	{
		public IServiceBus ServiceBus { get; set; }
		public Action<ServiceBusConfigurator> ConfigureServiceBus { get { return null; } }
		
		public void Given()
		{
		}

		/*
		 * TODO: Implement spec from https://github.com/MassTransit/MassTransit/issues/75
		 */
	}


}