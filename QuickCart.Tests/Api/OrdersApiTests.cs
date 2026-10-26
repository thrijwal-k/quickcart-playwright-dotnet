using System.Net;
using NUnit.Framework;
using RestSharp;

namespace QuickCart.Tests.Api;

// Reqnroll runs NUnit fixtures instance-per-test-case; the API fixtures share one client per fixture instead.
[TestFixture]
[FixtureLifeCycle(LifeCycle.SingleInstance)]
[Category("api")]
public sealed class OrdersApiTests : ApiTestBase
{
    private static readonly Customer ValidCustomer = new("Sam Shopper", "sam@example.com", "BS1 4DJ");

    [Test]
    public async Task Creates_an_order_calculates_the_total_and_reads_it_back()
    {
        var order = new OrderRequest(new[] { new OrderItem(3, 1), new OrderItem(5, 2) }, ValidCustomer);
        var created = await Client.ExecutePostAsync<OrderResponse>(new RestRequest("/api/orders").AddJsonBody(order));

        Assert.That(created.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(created.Data!.Status, Is.EqualTo("CONFIRMED"));
        Assert.That(created.Data.Total, Is.EqualTo(168.98m));

        var fetched = await Client.ExecuteGetAsync<OrderResponse>(new RestRequest($"/api/orders/{created.Data.Id}"));
        Assert.That(fetched.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(fetched.Data!.Id, Is.EqualTo(created.Data.Id));
        Assert.That(fetched.Data.Total, Is.EqualTo(created.Data.Total));
    }

    private static IEnumerable<TestCaseData> InvalidOrders()
    {
        yield return new TestCaseData(new OrderRequest(Array.Empty<OrderItem>(), ValidCustomer), "Order must contain at least one item")
            .SetName("Rejects an empty cart");
        yield return new TestCaseData(new OrderRequest(new[] { new OrderItem(99, 1) }, ValidCustomer), "Unknown product 99")
            .SetName("Rejects an unknown product");
        yield return new TestCaseData(new OrderRequest(new[] { new OrderItem(1, 0) }, ValidCustomer), "Invalid quantity for Wireless Headphones")
            .SetName("Rejects a zero quantity");
        yield return new TestCaseData(new OrderRequest(new[] { new OrderItem(3, 6) }, ValidCustomer), "Only 5 of Smart Watch in stock")
            .SetName("Rejects more than in stock");
        yield return new TestCaseData(new OrderRequest(new[] { new OrderItem(4, 1) }, ValidCustomer), "Only 0 of Fitness Band in stock")
            .SetName("Rejects a sold-out product");
        yield return new TestCaseData(new OrderRequest(new[] { new OrderItem(1, 1) }, ValidCustomer with { Email = "x" }), "A valid email is required")
            .SetName("Rejects a bad email");
        yield return new TestCaseData(new OrderRequest(new[] { new OrderItem(1, 1) }, ValidCustomer with { Postcode = "123" }), "A valid UK postcode is required")
            .SetName("Rejects a bad postcode");
    }

    [TestCaseSource(nameof(InvalidOrders))]
    public async Task Invalid_orders_are_rejected_with_422(OrderRequest order, string expectedError)
    {
        var response = await Client.ExecutePostAsync(new RestRequest("/api/orders").AddJsonBody(order));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
        Assert.That(ReadErrors(response), Does.Contain(expectedError));
    }

    [Test]
    public async Task Returns_404_for_an_unknown_order()
    {
        var response = await Client.ExecuteGetAsync(new RestRequest("/api/orders/1"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
