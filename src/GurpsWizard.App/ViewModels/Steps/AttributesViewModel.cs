using GurpsWizard.App.ViewModels;
using GurpsWizard.Core.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GurpsWizard.App.ViewModels.Steps;

public class AttributesViewModel : ReactiveObject
{
    private readonly WizardViewModel _wizard;
    private bool _syncing;

    [Reactive] public int ST { get; set; } = 10;
    [Reactive] public int DX { get; set; } = 10;
    [Reactive] public int IQ { get; set; } = 10;
    [Reactive] public int HT { get; set; } = 10;

    // Custo individual exibido ao lado de cada atributo
    [Reactive] public int CostST { get; private set; }
    [Reactive] public int CostDX { get; private set; }
    [Reactive] public int CostIQ { get; private set; }
    [Reactive] public int CostHT { get; private set; }

    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> IncrementST { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> DecrementST { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> IncrementDX { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> DecrementDX { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> IncrementIQ { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> DecrementIQ { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> IncrementHT { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> DecrementHT { get; }

    public AttributesViewModel(WizardViewModel wizard)
    {
        _wizard = wizard;

        IncrementST = ReactiveCommand.Create(() => { ST++; });
        DecrementST = ReactiveCommand.Create(() => { ST = Math.Max(1, ST - 1); });
        IncrementDX = ReactiveCommand.Create(() => { DX++; });
        DecrementDX = ReactiveCommand.Create(() => { DX = Math.Max(1, DX - 1); });
        IncrementIQ = ReactiveCommand.Create(() => { IQ++; });
        DecrementIQ = ReactiveCommand.Create(() => { IQ = Math.Max(1, IQ - 1); });
        IncrementHT = ReactiveCommand.Create(() => { HT++; });
        DecrementHT = ReactiveCommand.Create(() => { HT = Math.Max(1, HT - 1); });

        // Sincroniza localmente ao receber novo Draft
        wizard.WhenAnyValue(x => x.Draft)
              .Subscribe(d =>
              {
                  _syncing = true;
                  ST = d.Attributes.ST;
                  DX = d.Attributes.DX;
                  IQ = d.Attributes.IQ;
                  HT = d.Attributes.HT;
                  _syncing = false;
                  RefreshCosts();
              });

        // Propaga para Draft
        this.WhenAnyValue(x => x.ST, x => x.DX, x => x.IQ, x => x.HT,
                          (st, dx, iq, ht) => new Attributes(st, dx, iq, ht))
            .Where(_ => !_syncing)
            .Subscribe(attrs =>
            {
                _wizard.Draft = _wizard.Draft with { Attributes = attrs };
                RefreshCosts();
            });
    }

    private void RefreshCosts()
    {
        CostST = (ST - 10) * 10;
        CostDX = (DX - 10) * 20;
        CostIQ = (IQ - 10) * 20;
        CostHT = (HT - 10) * 10;
    }
}
