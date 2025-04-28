# Property Generator for MVVM

A lightweight tool that **automatically generates boilerplate MVVM properties** from a simple list, or an existing C# file, saving developers hours of repetitive coding. Perfect for WPF, Xamarin, and .NET MAUI projects.

## Features

⚡ **Convert raw property lists to full MVVM properties** in seconds:  
```plaintext

[Type Name] format

string CustomerName
bool IsActive
int? Age

or [Name, Type] format

CustomerName, string  
IsActive, bool  
Age, int?  

↓ (becomes) ↓

#region Property CustomerName  
private string _customerName;  
public string CustomerName  
{  
    get => _customerName;  
    set => SetProperty(ref _customerName, value);  
}  
#endregion  

(etc...)

🛠️ Customizable output:

    Toggle between verbose (OnPropertyChanged), compact (SetProperty) styles, or CommunityTollkit.MVVM style.

    Optional #region wrappers for better code organization.

📋 One-click copy or export to .cs files.

