using ReactiveUI;

namespace Xymanek.ReactiveUI.Toolkit.Demo.ViewModels;

public partial class UserInfoViewModel : ReactiveObject
{
    /// <summary>
    /// The unique name of this user
    /// </summary>
    [ReactiveProperty]
    private string _username = "";
    
    [ReactiveProperty]
    private string? _displayName;
}