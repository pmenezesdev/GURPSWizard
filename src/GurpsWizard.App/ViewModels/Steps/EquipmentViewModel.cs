using System.Collections.ObjectModel;
using GurpsWizard.App.ViewModels;
using GurpsWizard.Core.Models;
using GurpsWizard.Data.Entities;
using GurpsWizard.Data.Repositories;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GurpsWizard.App.ViewModels.Steps;

public class EquipmentViewModel : ReactiveObject
{
    private readonly WizardViewModel _wizard;
    private readonly ILibraryRepository _repo;

    [Reactive] public string SearchQuery { get; set; } = "";
    [Reactive] public ObservableCollection<LibraryEquipment> SearchResults { get; private set; } = [];
    [Reactive] public LibraryEquipment? SelectedLibraryEquipment { get; set; }
    [Reactive] public ObservableCollection<EquipmentEntry> AddedEquipment { get; private set; } = [];
    [Reactive] public EquipmentEntry? SelectedAddedEquipment { get; set; }

    [Reactive] public decimal TotalValue { get; private set; }
    [Reactive] public string TotalWeight { get; private set; } = "0 kg";

    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> AddCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> RemoveSelectedCommand { get; }

    public EquipmentViewModel(WizardViewModel wizard, ILibraryRepository repo)
    {
        _wizard = wizard;
        _repo   = repo;

        wizard.WhenAnyValue(x => x.Draft)
              .Subscribe(d =>
              {
                  AddedEquipment = new ObservableCollection<EquipmentEntry>(d.Equipment);
                  RecalcTotals(d.Equipment);
              });

        this.WhenAnyValue(x => x.SearchQuery)
            .Throttle(TimeSpan.FromMilliseconds(250))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(async _ => await SearchAsync());

        var canAdd    = this.WhenAnyValue(x => x.SelectedLibraryEquipment).Select(e => e is not null);
        var canRemove = this.WhenAnyValue(x => x.SelectedAddedEquipment)  .Select(e => e is not null);

        AddCommand           = ReactiveCommand.CreateFromTask(AddSelectedAsync, canAdd);
        RemoveSelectedCommand = ReactiveCommand.Create(RemoveSelected, canRemove);
    }

    private async Task SearchAsync()
    {
        var results = await _repo.SearchEquipmentAsync(SearchQuery);
        SearchResults = new ObservableCollection<LibraryEquipment>(results);
    }

    private Task AddSelectedAsync()
    {
        if (SelectedLibraryEquipment is null) return Task.CompletedTask;

        var eq    = SelectedLibraryEquipment;
        var entry = new EquipmentEntry(eq.GcsId, eq.Name, eq.Value, eq.Weight ?? "");
        var newList = new List<EquipmentEntry>(_wizard.Draft.Equipment) { entry };

        _wizard.Draft = _wizard.Draft with { Equipment = newList };
        return Task.CompletedTask;
    }

    private void RemoveSelected()
    {
        var entry = SelectedAddedEquipment;
        if (entry is null) return;

        var newList = _wizard.Draft.Equipment.Where(e => e != entry).ToList();
        _wizard.Draft = _wizard.Draft with { Equipment = newList };
        SelectedAddedEquipment = null;
    }

    private void RecalcTotals(IEnumerable<EquipmentEntry> items)
    {
        var list = items.ToList();
        TotalValue = list.Sum(e => e.Value * e.Quantity);

        double totalKg = list
            .Where(e => !string.IsNullOrWhiteSpace(e.Weight))
            .Sum(e =>
            {
                var part = e.Weight.Split(' ')[0];
                return double.TryParse(part, System.Globalization.NumberStyles.Any,
                                       System.Globalization.CultureInfo.InvariantCulture, out var kg)
                    ? kg * e.Quantity : 0;
            });
        TotalWeight = $"{totalKg:F2} kg";
    }
}
