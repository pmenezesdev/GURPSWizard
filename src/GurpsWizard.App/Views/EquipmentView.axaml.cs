using Avalonia;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using GurpsWizard.App.ViewModels.Steps;
using GurpsWizard.Data.Entities;
#pragma warning disable CS0618  // Avalonia DnD deprecated APIs still functional in 11.3.x

namespace GurpsWizard.App.Views;

public partial class EquipmentView : ReactiveUserControl<EquipmentViewModel>
{
    private Point _pressedAt;
    private bool  _isDragging;
    private const double DragThreshold = 8.0;

    public EquipmentView()
    {
        InitializeComponent();

        SearchResultsList.PointerPressed += OnSearchPointerPressed;
        SearchResultsList.PointerMoved   += OnSearchPointerMoved;
        SearchResultsList.DoubleTapped   += OnSearchDoubleTapped;

        DragDrop.SetAllowDrop(AddedPanel, true);
        AddedPanel.AddHandler(DragDrop.DropEvent,     OnAddedPanelDrop);
        AddedPanel.AddHandler(DragDrop.DragOverEvent, OnAddedPanelDragOver);
    }

    private void OnSearchPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
            _pressedAt = e.GetPosition(null);
    }

    private async void OnSearchPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDragging) return;
        if (!e.GetCurrentPoint(null).Properties.IsLeftButtonPressed) return;

        var pos = e.GetPosition(null);
        if (Math.Abs(pos.X - _pressedAt.X) < DragThreshold &&
            Math.Abs(pos.Y - _pressedAt.Y) < DragThreshold) return;

        if (ViewModel?.SelectedLibraryEquipment is not { } equipment) return;

        _isDragging = true;
        try
        {
            var data = new DataObject();
            data.Set("LibraryEquipment", equipment);
            await DragDrop.DoDragDrop(e, data, DragDropEffects.Copy);
        }
        finally { _isDragging = false; }
    }

    private void OnSearchDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (ViewModel?.SelectedLibraryEquipment is not null)
            ViewModel.AddCommand.Execute().Subscribe();
    }

    private static void OnAddedPanelDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = e.Data.Contains("LibraryEquipment")
            ? DragDropEffects.Copy
            : DragDropEffects.None;
    }

    private void OnAddedPanelDrop(object? sender, DragEventArgs e)
    {
        if (e.Data.Get("LibraryEquipment") is LibraryEquipment equipment && ViewModel is { } vm)
        {
            vm.SelectedLibraryEquipment = equipment;
            vm.AddCommand.Execute().Subscribe();
        }
    }
}
