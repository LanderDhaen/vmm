using System.Net.Http.Json;


using Rise.Shared.Common;
using Rise.Shared.WorkOrders;

namespace Rise.Client.WorkOrders;

public class WorkOrderService(HttpClient httpClient) : IWorkOrderService {
	public async Task<Result<WorkOrderResponse.Index>> GetIndexAsync(QueryRequest.SkipTake req, CancellationToken ctx = default) {
		Result<WorkOrderResponse.Index>? result =
			await httpClient.GetFromJsonAsync<Result<WorkOrderResponse.Index>>($"/api/workorders?{req.AsQuery()}", ctx);
		return result!;
	}
}