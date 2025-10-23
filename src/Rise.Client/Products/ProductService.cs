using System.Net.Http.Json;

using Rise.Shared.Common;
using Rise.Shared.Products;

namespace Rise.Client.Products;

public class ProductService(HttpClient httpClient) : IProductService {
	public async Task<Result<ProductResponse.Create>> CreateAsync(ProductRequest.Create request,
		CancellationToken ctx = default) {
		HttpResponseMessage response = await httpClient.PostAsJsonAsync("/api/products", request, ctx);
		Result<ProductResponse.Create>? result =
			await response.Content.ReadFromJsonAsync<Result<ProductResponse.Create>>(ctx);
		return result!;
	}

	public async Task<Result<ProductResponse.Index>> GetIndexAsync(QueryRequest.SkipTake request,
		CancellationToken ctx = default) {
		Result<ProductResponse.Index>? result =
			await httpClient.GetFromJsonAsync<Result<ProductResponse.Index>>("/api/products", ctx);
		return result!;
	}
}