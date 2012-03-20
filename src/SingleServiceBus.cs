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
using NUnit.Framework;

namespace MassTransit.TransportSpecs
{
	/// <summary>
	/// This test fixture has a single service bus that it performs tests on.
	/// </summary>
	interface SingleServiceBusFixture
	{
		/// <summary>
		/// This property is set by <see cref="SingleServiceBusAttribute"/>,
		/// or else fixture fails before running tests.
		/// </summary>
		IServiceBus ServiceBus { get; set; }

		/// <summary>
		/// Custom configuration for test fixture implementations. May be null. Invoked/gotten
		/// before <see cref="ServiceBus"/> is set.
		/// </summary>
		Action<ServiceBusConfigurator> ConfigureServiceBus { get; }

		[TestFixtureSetUp]
		void Given();
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class SingleServiceBusAttribute : Attribute, ITestAction
	{
		private IServiceBus bus;

		public void BeforeTest(TestDetails testDetails)
		{
			var fixture = testDetails.Fixture as SingleServiceBusFixture;

			if (fixture == null)
				Assert.Fail(string.Format("Test Fixture '{0}' must implement SingleServiceBusFixture",
					testDetails.FullName));
			
			var fixtureType = testDetails.Fixture.GetType();
			var fixtureGenericTypes = fixtureType.GetGenericArguments();

			var serviceBus = CreateServiceBus(fixture, fixtureGenericTypes);

			WaitForEndpointInitialization(serviceBus);

			fixture.ServiceBus = serviceBus;
		}

		private void WaitForEndpointInitialization(IServiceBus serviceBus)
		{
			serviceBus.Endpoint.InboundTransport.Receive(c1 => c2 => { },
				TimeSpan.FromSeconds(5.0));
		}

		private static IServiceBus CreateServiceBus(SingleServiceBusFixture fixture, Type[] fixtureGenericTypes)
		{
			var serializer = Activator.CreateInstance(fixtureGenericTypes[0]) as IMessageSerializer;
			var transportFactory = Activator.CreateInstance(fixtureGenericTypes[1]) as ITransportFactory;

			Assert.That(serializer, Is.Not.Null);
			Assert.That(transportFactory, Is.Not.Null);

			return ServiceBusFactory.New(sbc =>
				{
					sbc.SetDefaultSerializer(serializer);
					sbc.AddTransportFactory(transportFactory);
					sbc.ReceiveFrom(TestUriFactory.GetUriFor(transportFactory));

					if (fixture.ConfigureServiceBus != null)
						fixture.ConfigureServiceBus(sbc);
				});
		}

		public void AfterTest(TestDetails testDetails)
		{
			if (bus != null)
				bus.Dispose();
		}

		public ActionTargets Targets
		{
			get { return ActionTargets.Default; }
		}
	}
}