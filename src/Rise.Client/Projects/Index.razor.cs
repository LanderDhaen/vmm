using Microsoft.AspNetCore.Components;
using Rise.Shared.Common;
using Rise.Shared.Projects;

namespace Rise.Client.Projects;

public partial class Index
{
    private IEnumerable<ProjectDto.Index>? projects;

    [Inject] public required IProjectService ProjectService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var request = new QueryRequest.SkipTake
        {
            Skip = 0,
            Take = 50,
            OrderBy = "Id",
        };

        var result = await ProjectService.GetIndexAsync(request);
        projects = result.Value.Projects;
    }
}