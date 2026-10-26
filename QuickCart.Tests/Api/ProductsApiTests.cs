using System.Net;
using NUnit.Framework;
using QuickCart.Tests.Support;
using RestSharp;

namespace QuickCart.Tests.Api;

[TestFixture]
[Category("api")]
public sealed class ProductsApiTests : ApiTestBase
{
    [Test]
    public async Task Lists_every_product_with_valid_fields()
    {
        var response = await Client.ExecuteGetAsync<List<ProductDto>>(new RestRequest("/api/products"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Data, Has.Count.EqualTo(6));
        Assert.Multiple(() =>
        {
            foreach (var product in response.Data!)
            {
                Assert.That(product.Name, Is.Not.Empty);
                Assert.That(product.Category, Is.Not.Empty);
                Assert.That(product.Price, Is.GreaterThan(0m));
                Assert.That(product.Stock, Is.GreaterThanOrEqualTo(0));
            }
        });
    }

    [Test]
    [Category("database")]
    public async Task Products_api_matches_the_reference_database()
    {
        var response = await Client.ExecuteGetAsync<List<ProductDto>>(new RestRequest("/api/products"));
        var expected = ReferenceDatabase.QueryProducts();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Data!.Select(p => p.Id), Is.EqualTo(expected.Select(p => p.Id)));
        Assert.Multiple(() =>
        {
            foreach (var row in expected)
            {
                var actual = response.Data!.Single(p => p.Id == row.Id);
                Assert.That(actual.Name, Is.EqualTo(row.Name), $"name of product {row.Id}");
                Assert.That((double)actual.Price, Is.EqualTo(row.Price).Within(0.001), $"price of {row.Name}");
                Assert.That(actual.Stock, Is.EqualTo(row.Stock), $"stock of {row.Name}");
            }
        });
    }

    [TestCase("SMART", "Smart Watch")]
    [TestCase("watch", "Smart Watch")]
    [TestCase("band", "Fitness Band")]
    public async Task Search_is_case_insensitive(string term, string expectedName)
    {
        var request = new RestRequest("/api/products").AddQueryParameter("q", term);
        var response = await Client.ExecuteGetAsync<List<ProductDto>>(request);

        Assert.That(response.Data!.Select(p => p.Name), Is.EqualTo(new[] { expectedName }));
    }

    [Test]
    public async Task Returns_one_product_by_id()
    {
        var response = await Client.ExecuteGetAsync<ProductDto>(new RestRequest("/api/products/3"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Data!.Name, Is.EqualTo("Smart Watch"));
        Assert.That(response.Data.Price, Is.EqualTo(129m));
    }

    [Test]
    public async Task Returns_404_for_an_unknown_product()
    {
        var response = await Client.ExecuteGetAsync(new RestRequest("/api/products/999"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(ReadError(response), Is.EqualTo("Product not found"));
    }

    [Test]
    public async Task Valid_login_returns_a_token()
    {
        var request = new RestRequest("/api/login").AddJsonBody(new { username = "shopper", password = "Passw0rd!" });
        var response = await Client.ExecutePostAsync<LoginResponse>(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Data!.Token, Does.StartWith("token-shopper-"));
        Assert.That(response.Data.Name, Is.EqualTo("Sam Shopper"));
    }

    [TestCase("shopper", "wrong", HttpStatusCode.Unauthorized)]
    [TestCase("locked_user", "Passw0rd!", HttpStatusCode.Forbidden)]
    public async Task Rejected_logins_return_the_right_status(string username, string password, HttpStatusCode expected)
    {
        var request = new RestRequest("/api/login").AddJsonBody(new { username, password });
        var response = await Client.ExecutePostAsync(request);

        Assert.That(response.StatusCode, Is.EqualTo(expected));
    }
}
