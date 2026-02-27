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
    [Reactive] public string? SelectedCategory { get; set; }
    [Reactive] public ObservableCollection<string> Categories { get; private set; } = [];
    [Reactive] public ObservableCollection<LibraryEquipment> SearchResults { get; private set; } = [];
    [Reactive] public LibraryEquipment? SelectedLibraryEquipment { get; set; }
    [Reactive] public ObservableCollection<EquipmentEntry> AddedEquipment { get; private set; } = [];
    [Reactive] public EquipmentEntry? SelectedAddedEquipment { get; set; }

    [Reactive] public decimal TotalValue { get; private set; }
    [Reactive] public string TotalWeight { get; private set; } = "0.00 kg";

    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> AddCommand { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> RemoveSelectedCommand { get; }
    public ReactiveCommand<object, System.Reactive.Unit> OpenReferenceCommand { get; }

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

        this.WhenAnyValue(x => x.SearchQuery, x => x.SelectedCategory)
            .Throttle(TimeSpan.FromMilliseconds(250))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(async _ => await SearchAsync());

        _ = LoadCategoriesAsync();

        var canAdd    = this.WhenAnyValue(x => x.SelectedLibraryEquipment).Select(e => e is not null);
        var canRemove = this.WhenAnyValue(x => x.SelectedAddedEquipment)  .Select(e => e is not null);

        AddCommand           = ReactiveCommand.CreateFromTask(AddSelectedAsync, canAdd);
        RemoveSelectedCommand = ReactiveCommand.Create(RemoveSelected, canRemove);
        OpenReferenceCommand = ReactiveCommand.Create<object>(param =>
        {
            if (param is LibraryEquipment le) PdfService.OpenReference(le.Reference, le.Name);
            else if (param is EquipmentEntry ee) PdfService.OpenReference(ee.Reference, ee.Name);
        });
    }

    private async Task LoadCategoriesAsync()
    {
        var cats = await _repo.GetEquipmentCategoriesAsync();
        var list = new List<string> { "Tudo" };
        list.AddRange(cats);
        Categories = new ObservableCollection<string>(list);
        SelectedCategory = "Tudo";
    }

    private async Task SearchAsync()
    {
        var category = SelectedCategory == "Tudo" ? null : SelectedCategory;
        var results = await _repo.SearchEquipmentAsync(SearchQuery, category);
        SearchResults = new ObservableCollection<LibraryEquipment>(results);
    }

    private Task AddSelectedAsync()
    {
        if (SelectedLibraryEquipment is null) return Task.CompletedTask;

        var eq = SelectedLibraryEquipment;
        var currentList = new List<EquipmentEntry>(_wizard.Draft.Equipment);
        
        // Verifica se o item já existe pelo ID da definição
        var existing = currentList.FirstOrDefault(e => e.DefinitionId == eq.GcsId);
        
        if (existing != null)
        {
            // Incrementa quantidade
            var index = currentList.IndexOf(existing);
            currentList[index] = existing with { Quantity = existing.Quantity + 1 };
        }
        else
        {
            // Adiciona novo
            currentList.Add(new EquipmentEntry(eq.GcsId, eq.Name, eq.Value, eq.Weight ?? "0", 1, eq.Reference));
        }

        _wizard.Draft = _wizard.Draft with { Equipment = currentList };
        return Task.CompletedTask;
    }

    private void RemoveSelected()
    {
        if (SelectedAddedEquipment == null) return;
        
        var entry = SelectedAddedEquipment;

        var currentList = new List<EquipmentEntry>(_wizard.Draft.Equipment);
        if (entry.Quantity > 1)
        {
            var index = currentList.IndexOf(entry);
            currentList[index] = entry with { Quantity = entry.Quantity - 1 };
        }
        else
        {
            currentList.Remove(entry);
        }

        _wizard.Draft = _wizard.Draft with { Equipment = currentList };
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
                // Limpa a string de peso (tira "kg", "lb", etc) e pega apenas o número
                var part = e.Weight.Split(' ')[0];
                if (double.TryParse(part, System.Globalization.NumberStyles.Any,
                                       System.Globalization.CultureInfo.InvariantCulture, out var weightValue))
                {
                    // Se a unidade for LB (libra), converte para KG (1 lb = 0.453592 kg)
                    if (e.Weight.ToLower().Contains("lb"))
                    {
                        weightValue *= 0.453592;
                    }
                    return weightValue * e.Quantity;
                }
                return 0;
            });
        TotalWeight = $"{totalKg:F2} kg";
    }
}
