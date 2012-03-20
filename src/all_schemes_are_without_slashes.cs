using MassTransit.Serialization;
using MassTransit.Transports;
using NUnit.Framework;
using SharpTestsEx;

namespace MassTransit.TransportSpecs
{
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