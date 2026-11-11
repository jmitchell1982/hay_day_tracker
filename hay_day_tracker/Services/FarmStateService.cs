using HayDayPlanner.Models;

namespace HayDayPlanner.Services;

public class FarmStateService
{
  private readonly FarmStateRepository _repository;
  public FarmState Current { get; }

  /// <summary>
  /// Raised after Current has been persisted. Since every tab's edits funnel
  /// through Save() before anything else observes the change, this is the
  /// single correct place for the Scheduling tab (or anything else) to know
  /// FarmState moved and it should recalculate.
  /// </summary>
  public event EventHandler? FarmStateChanged;

  public FarmStateService(FarmStateRepository repository)
  {
    _repository = repository;
    Current = _repository.Load();
  }

  public void Save()
  {
    _repository.Save(Current);
    FarmStateChanged?.Invoke(this, EventArgs.Empty);
  }
}