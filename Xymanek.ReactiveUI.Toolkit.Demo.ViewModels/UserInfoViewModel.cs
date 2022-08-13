using ReactiveUI;

namespace Xymanek.ReactiveUI.Toolkit.Demo.ViewModels;

public partial class UserInfoViewModel : ReactiveObject
{
    [ReactiveProperty]
    private string _username = "";
    
    [ReactiveProperty]
    private string _displayName = "";

    // public string DisplayName
    // {
    //     get => _displayName;
    //     set => this.RaiseAndSetIfChanged(ref _displayName, value);
    // }
}