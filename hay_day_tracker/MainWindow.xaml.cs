using System.Windows;
using HayDayPlanner.Models;
using HayDayPlanner.Services;
using HayDayPlanner.ViewModels;
using HayDayPlanner.Views;

namespace HayDayPlanner;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
  public MainWindow()
  {
    InitializeComponent();

    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
    string farmStatePath = System.IO.Path.Combine(baseDirectory, "farmstate.json");
    string gameDataPath = System.IO.Path.Combine(baseDirectory, "gamedata.json");
    string settingsPath = System.IO.Path.Combine(baseDirectory, "settings.json");

    FarmStateRepository farmStateRepository = new FarmStateRepository(farmStatePath);
    FarmStateService farmStateService = new FarmStateService(farmStateRepository);
    
    List<LevelInfo> levels = GameDataLoader.LoadLevels(gameDataPath);
    List<ProductInfo> products = GameDataLoader.LoadProducts(gameDataPath);
    List<BuildingInfo> buildings = GameDataLoader.LoadBuildings(gameDataPath);
    List<UpgradeItemInfo> upgradeItems = GameDataLoader.LoadUpgradeItems(gameDataPath);
    List<CapacityUpgradeInfo> capacityUpgrades = GameDataLoader.LoadCapacityUpgrades(gameDataPath);
    List<LeveledUpgradeInfo> leveledUpgrades = GameDataLoader.LoadLeveledUpgrades(gameDataPath);
    List<ShopUpgradeInfo> shopUpgrades = GameDataLoader.LoadShopUpgrades(gameDataPath);
    List<PersonalTrainLevelInfo> personalTrainLevels = GameDataLoader.LoadPersonalTrainLevels(gameDataPath);
    
    farmStateService.Current.Buildings = farmStateService.Current.Buildings.Where(x => buildings.Any(y => y.Id == x.Id)).ToList();

    List<BuildingInfo> upgradeableBuildings = new List<BuildingInfo>
    {
      new BuildingInfo { Id = "barn", Level = [], MasteryTime = [], MaxQuantity = 1, Name = "Barn" },
      new BuildingInfo { Id = "silo", Level = [], MasteryTime = [], MaxQuantity = 1, Name = "Silo" },
      new BuildingInfo { Id = "tackle_box", Level = [], MasteryTime = [], MaxQuantity = 1, Name = "Tackle Box" },
      new BuildingInfo { Id = "town_hall", Level = [], MasteryTime = [], MaxQuantity = 1, Name = "Town Hall" },
      new BuildingInfo { Id = "town_station", Level = [], MasteryTime = [], MaxQuantity = 1, Name = "Town Station" },
      new BuildingInfo { Id = "grocery_store", Level = [], MasteryTime = [], MaxQuantity = 1, Name = "Grocery Store" },
      new BuildingInfo { Id = "cinema", Level = [], MasteryTime = [], MaxQuantity = 1, Name = "Cinema" },
      new BuildingInfo { Id = "diner", Level = [], MasteryTime = [], MaxQuantity = 1, Name = "Diner" },
      new BuildingInfo { Id = "bed_and_breakfast", Level = [], MasteryTime = [], MaxQuantity = 1, Name = "Bed and Breakfast" },
      new BuildingInfo { Id = "spa", Level = [], MasteryTime = [], MaxQuantity = 1, Name = "Spa" },
      new BuildingInfo { Id = "gift_shop", Level = [], MasteryTime = [], MaxQuantity = 1, Name = "Gift Shop" },
      new BuildingInfo { Id = "beach_cafe", Level = [], MasteryTime = [], MaxQuantity = 1, Name = "Beach Cafe" }
    };

    SettingsRepository settingsRepository = new SettingsRepository(settingsPath);
    AppSettings settings = settingsRepository.Load();
    
    FarmOverviewViewModel farmOverviewViewModel = new FarmOverviewViewModel(farmStateService, levels);
    FarmOverviewView farmOverviewView = new FarmOverviewView(farmOverviewViewModel);
    FarmOverviewContent.Content = farmOverviewView;
    
    MachinesViewModel machinesViewModel = new MachinesViewModel(farmStateService, buildings);
    MachinesView machinesView = new MachinesView(machinesViewModel);
    BuildingsContent.Content = machinesView;

    ProductsViewModel productsViewModel = new ProductsViewModel(farmStateService, products, buildings);
    ProductsView productsView = new ProductsView(productsViewModel);
    ProductsContent.Content = productsView;
    
    UpgradesViewModel upgradesViewModel = new UpgradesViewModel(farmStateService, upgradeableBuildings, upgradeItems, capacityUpgrades, leveledUpgrades, shopUpgrades, personalTrainLevels);
    UpgradesView upgradesView = new UpgradesView(upgradesViewModel);
    UpgradesContent.Content = upgradesView;
    
    ShoppingListViewModel shoppingListViewModel = new ShoppingListViewModel(farmStateService, capacityUpgrades, leveledUpgrades, shopUpgrades, personalTrainLevels, upgradeItems, settings);
    ShoppingListView shoppingListView = new ShoppingListView(shoppingListViewModel);
    ShoppingListContent.Content = shoppingListView;
    
    SchedulingViewModel schedulingViewModel = new SchedulingViewModel(farmStateService, products, buildings);
    SchedulingView schedulingView = new SchedulingView(schedulingViewModel);
    ScheduleContent.Content = schedulingView;
  }
}