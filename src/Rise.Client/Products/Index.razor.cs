using Microsoft.AspNetCore.Components;

using Rise.Shared.Common;
using Rise.Shared.Products;

namespace Rise.Client.Products;

public partial class Index {
	private IEnumerable<ProductDto.Index>? products;

	[Inject] public required IProductService ProductService { get; set; }

	protected override async Task OnInitializedAsync() {
		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 50,
			OrderBy = "Id"
		};

		Result<ProductResponse.Index> result = await ProductService.GetIndexAsync(request);
		products = result.Value.Products;
	}
}