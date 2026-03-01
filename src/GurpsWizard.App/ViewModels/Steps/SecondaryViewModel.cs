using GurpsWizard.App.ViewModels;
using GurpsWizard.Core.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace GurpsWizard.App.ViewModels.Steps;

public class SecondaryViewModel : ReactiveObject
{
    private readonly WizardViewModel _wizard;
    private bool _syncing;

    // ── Ajustes de bônus (editáveis) ──────────────────────────────────────────
    [Reactive] public int HPBonus { get; set; }
    [Reactive] public int FPBonus { get; set; }
    [Reactive] public int WillBonus { get; set; }
    [Reactive] public int PerBonus { get; set; }
    [Reactive] public int BasicSpeedBonus { get; set; }
    [Reactive] public int BasicMoveBonus { get; set; }

    // ── Valores calculados (somente leitura) ──────────────────────────────────
    [Reactive] public int HP { get; private set; }
    [Reactive] public int FP { get; private set; }
    [Reactive] public int Will { get; private set; }
    [Reactive] public int Per { get; private set; }
    [Reactive] public double BasicSpeed { get; private set; }
    [Reactive] public int BasicMove { get; private set; }

    // ── Custos individuais ────────────────────────────────────────────────────
    [Reactive] public int CostHP { get; private set; }
    [Reactive] public int CostFP { get; private set; }
    [Reactive] public int CostWill { get; private set; }
    [Reactive] public int CostPer { get; private set; }
    [Reactive] public int CostBasicSpeed { get; private set; }
    [Reactive] public int CostBasicMove { get; private set; }

    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> IncrementHP { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> DecrementHP { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> IncrementFP { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> DecrementFP { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> IncrementWill { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> DecrementWill { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> IncrementPer { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> DecrementPer { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> IncrementSpeed { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> DecrementSpeed { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> IncrementMove { get; }
    public ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit> DecrementMove { get; }

    public SecondaryViewModel(WizardViewModel wizard)
    {
        _wizard = wizard;

        IncrementHP = ReactiveCommand.Create(() => { HPBonus++; });
        DecrementHP = ReactiveCommand.Create(() => { HPBonus--; });
        IncrementFP = ReactiveCommand.Create(() => { FPBonus++; });
        DecrementFP = ReactiveCommand.Create(() => { FPBonus--; });
        IncrementWill = ReactiveCommand.Create(() => { WillBonus++; });
        DecrementWill = ReactiveCommand.Create(() => { WillBonus--; });
        IncrementPer = ReactiveCommand.Create(() => { PerBonus++; });
        DecrementPer = ReactiveCommand.Create(() => { PerBonus--; });
        IncrementSpeed = ReactiveCommand.Create(() => { BasicSpeedBonus++; });
        DecrementSpeed = ReactiveCommand.Create(() => { BasicSpeedBonus = Math.Max(0, BasicSpeedBonus - 1); });
        IncrementMove = ReactiveCommand.Create(() => { BasicMoveBonus++; });
        DecrementMove = ReactiveCommand.Create(() => { BasicMoveBonus = Math.Max(0, BasicMoveBonus - 1); });

        // Sincroniza ao receber Draft
        wizard.WhenAnyValue(x => x.Draft)
              .Subscribe(d =>
              {
                  _syncing         = true;
                  HPBonus          = d.SecondaryAttributes.HPBonus;
                  FPBonus          = d.SecondaryAttributes.FPBonus;
                  WillBonus        = d.SecondaryAttributes.WillBonus;
                  PerBonus         = d.SecondaryAttributes.PerBonus;
                  BasicSpeedBonus  = d.SecondaryAttributes.BasicSpeedBonus;
                  BasicMoveBonus   = d.SecondaryAttributes.BasicMoveBonus;
                  _syncing         = false;
                  Recalculate(d);
              });

        // Propaga ajustes de volta ao Draft
        this.WhenAnyValue(
                x => x.HPBonus, x => x.FPBonus, x => x.WillBonus,
                x => x.PerBonus, x => x.BasicSpeedBonus, x => x.BasicMoveBonus,
                (hp, fp, wl, pe, bs, bm) =>
                    new SecondaryAttributes(hp, fp, wl, pe, bs, bm))
            .Where(_ => !_syncing)
            .Subscribe(sec =>
            {
                _wizard.Draft = _wizard.Draft with { SecondaryAttributes = sec };
                Recalculate(_wizard.Draft);
            });
    }

    private void Recalculate(CharacterDraft d)
    {
        var a = d.Attributes;
        var s = d.SecondaryAttributes;

        HP         = a.ST  + s.HPBonus;
        FP         = a.HT  + s.FPBonus;
        Will       = a.IQ  + s.WillBonus;
        Per        = a.IQ  + s.PerBonus;
        BasicSpeed = Math.Round((a.DX + a.HT) / 4.0 + s.BasicSpeedBonus * 0.25, 2);
        BasicMove  = (int)Math.Floor(BasicSpeed) + s.BasicMoveBonus;

        CostHP         = s.HPBonus         * 2;
        CostFP         = s.FPBonus         * 2;
        CostWill       = s.WillBonus       * 2;
        CostPer        = s.PerBonus        * 2;
        CostBasicSpeed = s.BasicSpeedBonus * 5;
        CostBasicMove  = s.BasicMoveBonus  * 5;
    }
}
