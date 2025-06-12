using AppMonitoreo.Models;
using CommunityToolkit.Mvvm.Input;

namespace AppMonitoreo.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}