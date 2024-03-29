﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using Sanet.MagicalYatzy.Models;
using Sanet.MagicalYatzy.Models.Game;
using Sanet.MagicalYatzy.ViewModels.Base;
using Sanet.MagicalYatzy.Services.Game;
using Sanet.MagicalYatzy.Services.Localization;
using Sanet.MagicalYatzy.ViewModels.ObservableWrappers;

namespace Sanet.MagicalYatzy.ViewModels;

public class LobbyViewModel: DicePanelViewModel
{
    public const int MaxPlayers = 4;
        
    private readonly IPlayerService _playerService;
    private bool _canAddBot = true;
    private bool _canAddHuman = true;
    private readonly IRulesService _rulesService;
    private readonly IGameService _gameService;
    private readonly ILocalizationService _localizationService;

    public LobbyViewModel(IDicePanel dicePanel, 
        IPlayerService playerService, 
        IRulesService rulesService,
        IGameService gameService,
        ILocalizationService localizationService) : base(dicePanel)
    {
        _playerService = playerService;
        _rulesService = rulesService;
        _gameService = gameService;
        _localizationService = localizationService;
    }

    public string PlayersTitle => _localizationService.GetLocalizedString("PlayersLabel").ToUpper();

    public string RulesTitle => _localizationService.GetLocalizedString("RulesLabel").ToUpper();
        
    public string StartTitle => _localizationService.GetLocalizedString("StartGameButton");

    public string AddBotLabel => _localizationService.GetLocalizedString("AddBotLabel");
        
    public string AddPlayerLabel => _localizationService.GetLocalizedString("AddPlayerLabel");
        
    public string AddBotImage => "AddBot.png";
        
    public string AddPlayerImage => "AddPlayer.png";

    public string StartImage => "Start.png";
        
    public string BackImage => "Back.png";
        
    public ObservableCollection<PlayerViewModel> Players { get; } = new();

    public bool CanAddBot
    {
        get => _canAddBot;
        private set => SetProperty(ref _canAddBot, value);
    }

    public bool CanAddHuman
    {
        get => _canAddHuman;
        private set => SetProperty(ref _canAddHuman, value);
    }

    public ObservableCollection<RuleViewModel> Rules { get; } = new ObservableCollection<RuleViewModel>();

    private void AddDefaultPlayer()
    {
        if (!Players.Any())
            AddPlayer(new PlayerViewModel(_playerService.CurrentPlayer, _localizationService));
    }

    public override void AttachHandlers()
    {
        base.AttachHandlers();
        AddDefaultPlayer();
        CheckCanAddPlayers();
        LoadRules();
    }

    public override void DetachHandlers()
    {
        base.DetachHandlers();

        foreach (var ruleViewModel in Rules)
        {
            ruleViewModel.RuleSelected -= OnRuleSelected;
        }
        Rules.Clear();
    }

    internal void AddPlayer(PlayerViewModel playerViewModel)
    {
        if (Players.Count >= MaxPlayers)
            return;
        playerViewModel.PlayerDeleted += PlayerViewModelOnPlayerDeleted;
        Players.Add(playerViewModel);
        CheckPossibilityToDeletePlayers();
        CheckCanAddPlayers();
    }

    private void PlayerViewModelOnPlayerDeleted(object sender, EventArgs e)
    {
        if (!(sender is PlayerViewModel playerVm)) return;
        playerVm.PlayerDeleted -= PlayerViewModelOnPlayerDeleted;
        Players.Remove(playerVm);
        CheckPossibilityToDeletePlayers();
        CheckCanAddPlayers();
    }

    private void CheckPossibilityToDeletePlayers()
    {
        foreach (var playerViewModel in Players)
        {
            playerViewModel.CanBeDeleted = Players.Count != 1;
        }
    }

    private void CheckCanAddPlayers()
    {
        CanAddBot = CanAddHuman = Players.Count < MaxPlayers;
        NotifyPropertyChanged(nameof(CanStartGame));
    }

    public void LoadRules()
    {
        if (Rules.Any())
            return;
        var rules = _rulesService.GetAllRules().Select(r=>new RuleViewModel(r, _localizationService));
        foreach (var rule in rules)
        {
            rule.RuleSelected += OnRuleSelected;
            Rules.Add(rule);
        }

        SelectRule(Models.Game.Rules.krSimple);
    }

    private void OnRuleSelected(object sender, EventArgs e)
    {
        SelectRule(((RuleViewModel)sender).Rule);
    }

    private void SelectRule(Rules? rule)
    {
        if (rule == null)
        {
            return;
        }
        var ruleToSelect = Rules.FirstOrDefault(r => r.Rule == rule);
        if (ruleToSelect == null || ruleToSelect.IsSelected)
        {
            return;
        }
        foreach (var ruleViewModel in Rules)
        {
            ruleViewModel.IsSelected = false;
        }

        ruleToSelect.IsSelected = true;
        NotifyPropertyChanged(nameof(SelectedRule));
        NotifyPropertyChanged(nameof(CanStartGame));
    }

    public RuleViewModel SelectedRule
    {
        get => Rules.FirstOrDefault(r => r.IsSelected);
        set => SelectRule(value?.Rule);
    }

    public bool CanStartGame => Rules.FirstOrDefault(f => f.IsSelected) != null && Players.Any();

    public ICommand StartGameCommand => new AsyncCommand(async () =>
    {
        if (!CanStartGame) return;
        var rule = Rules.FirstOrDefault(f => f.IsSelected);
        if (rule==null)
            return;
        var game = await _gameService.CreateNewLocalGameAsync(rule.Rule);
        foreach (var playerViewModel in Players)
        {
            playerViewModel.Player.IsReady = false;
            game.JoinGame(playerViewModel.Player);
        }

        await NavigationService.NavigateToViewModelAsync<GameViewModel>();
    });

    public ICommand AddBotCommand => new SimpleCommand(() =>
    {
        if (!CanAddBot)
            return;
        var newBot = _playerService.CreateLocalPlayer(
            _localizationService.GetLocalizedString("BotNameDefault"),
            PlayerType.AI, Players.Select(p => p.Name).ToList());
        
        AddPlayer(new PlayerViewModel(newBot, _localizationService));
    });

    public ICommand AddHumanCommand => new SimpleCommand( () =>
    {
        if (!CanAddHuman)
            return;
        var player = _playerService.CreateLocalPlayer(
            _localizationService.GetLocalizedString("PlayerNameDefault"),
            PlayerType.Local, Players.Select(p => p.Name).ToList());
        
        AddPlayer(new PlayerViewModel(player, _localizationService));
    });
}