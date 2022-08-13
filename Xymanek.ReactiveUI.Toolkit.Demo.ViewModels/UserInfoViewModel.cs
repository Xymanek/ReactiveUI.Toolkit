using ReactiveUI;

namespace Xymanek.ReactiveUI.Toolkit.Demo.ViewModels;

public class UserInfoViewModel : ReactiveObject
{
    [ReactiveProperty]
    private string _username = "";
    
    [ReactiveProperty]
    private string _displayName = "";
}