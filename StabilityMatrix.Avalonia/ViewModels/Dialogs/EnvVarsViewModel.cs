﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StabilityMatrix.Core.Attributes;
using StabilityMatrix.Core.Models;

namespace StabilityMatrix.Avalonia.ViewModels.Dialogs;

[View(typeof(EnvVarsViewModel))]
public partial class EnvVarsViewModel : ContentDialogViewModelBase
{
    [ObservableProperty]
    private string title = "Environment Variables";

    [ObservableProperty, NotifyPropertyChangedFor(nameof(EnvVarsView))]
    private ObservableCollection<EnvVarKeyPair> envVars = new();

    public DataGridCollectionView EnvVarsView => new(EnvVars);
    
    [RelayCommand]
    private void AddRow()
    {
        EnvVars.Add(new EnvVarKeyPair());
    }

    [RelayCommand]
    private void RemoveSelectedRow(int selectedIndex)
    {
        try
        {
            EnvVars.RemoveAt(selectedIndex);
        }
        catch (ArgumentOutOfRangeException)
        {
            Debug.WriteLine($"RemoveSelectedRow: Index {selectedIndex} out of range");
        }
    }
}
