// See https://aka.ms/new-console-template for more information

using ReactiveUI;
using Xymanek.ReactiveUI.Toolkit.Demo.ViewModels;

Console.WriteLine("Hello, World!");

UserInfoViewModel userInfoViewModel = new();

userInfoViewModel.Username = "user0";
userInfoViewModel.Username = "user1";

// Too lazy to dispose
userInfoViewModel.WhenAnyValue(u => u.Username)
    .Subscribe(username => Console.WriteLine("WhenAnyValue(u => u.Username): " + username));

userInfoViewModel.Username = "user2";
userInfoViewModel.Username = "user3";

Console.WriteLine("Final username: " + userInfoViewModel.Username);
