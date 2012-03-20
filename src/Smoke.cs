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
using System.Threading;
using MassTransit.Serialization;
using MassTransit.Transports;
using NUnit.Framework;

namespace MassTransit.TransportSpecs
{
	[TestFixture]
	public class SmokeTest<TSerializer, TTransportFac>
		: ForAll_context<TSerializer, TTransportFac>
		where TTransportFac : ITransportFactory, new()
		where TSerializer : IMessageSerializer, new()
	{
		[Test]
		public void Smoke()
		{
			Console.WriteLine("Running ({0}, {1})", typeof(TSerializer), typeof(TTransportFac));
		}
	}

	[TestFixture, Timeout(200), Explicit("Should fail due to Timeout attribute on TF")]
	public class SmokeFixture
	{
		[Test]
		public void FailingSmoke()
		{
			Thread.Sleep(300);
		}
	}
}