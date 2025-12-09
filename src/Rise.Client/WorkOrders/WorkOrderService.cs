using System.Net.Http.Json;

using Rise.Shared.Common;
using Rise.Shared.WorkOrders;

namespace Rise.Client.WorkOrders;

public class WorkOrderService(HttpClient httpClient) : IWorkOrderService {
	public async Task<Result<WorkOrderResponse.Index>> GetIndexAsync(QueryRequest.SkipTake req,
		CancellationToken ctx = default) {
		Result<WorkOrderResponse.Index>? result =
			await httpClient.GetFromJsonAsync<Result<WorkOrderResponse.Index>>($"/api/workorders?{req.AsQuery()}", ctx);
		return result!;
	}

	public async Task<Result<WorkOrderResponse.Detail>> GetDetailAsync(
		Guid workOrderId,
		CancellationToken ctx = default
	) {
		Result<WorkOrderResponse.Detail>? result =
			await httpClient.GetFromJsonAsync<Result<WorkOrderResponse.Detail>>(
				$"/api/workorders/{workOrderId}",
				ctx
			);

		return result!;
	}

	public async Task<Result> UpdateStatusAsync(WorkOrderRequest.UpdateStatus req, CancellationToken ctx = default) {
		HttpResponseMessage response =
			await httpClient.PutAsJsonAsync($"/api/workorders/{req.WorkOrderId}/status", req, ctx);

		Result? result = await response.Content.ReadFromJsonAsync<Result>(ctx);
		return result!;
	}
	public async Task<IEnumerable<string>> GetTechnicianNamesAsync(string? query, CancellationToken ctx = default) {
		string q = string.IsNullOrWhiteSpace(query) ? string.Empty : query.Trim();
		var url = $"/api/workorders/technicians?query={Uri.EscapeDataString(q)}";
		var result = await httpClient.GetFromJsonAsync<Result<List<string>>>(url, ctx);
		return result?.Value ?? Enumerable.Empty<string>();
	}
}