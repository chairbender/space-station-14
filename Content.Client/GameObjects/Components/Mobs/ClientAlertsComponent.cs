﻿using System;
using System.Collections.Generic;
using System.Linq;
using Content.Client.UserInterface;
using Content.Client.UserInterface.Stylesheets;
using Content.Client.Utility;
using Content.Shared.Alert;
using Content.Shared.GameObjects.Components.Mobs;
using Robust.Client.GameObjects;
using Robust.Client.Interfaces.ResourceManagement;
using Robust.Client.Interfaces.UserInterface;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.GameObjects;
using Robust.Shared.Input;
using Robust.Shared.Interfaces.GameObjects;
using Robust.Shared.Interfaces.Timing;
using Robust.Shared.IoC;
using Robust.Shared.Log;
using Robust.Shared.Maths;
using Robust.Shared.ViewVariables;
using Serilog;

namespace Content.Client.GameObjects.Components.Mobs
{
    /// <inheritdoc/>
    [RegisterComponent]
    [ComponentReference(typeof(SharedAlertsComponent))]
    public sealed class ClientAlertsComponent : SharedAlertsComponent
    {
        private static readonly float TooltipTextMaxWidth = 265;

        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly IResourceCache _resourceCache = default!;
        [Dependency] private readonly IUserInterfaceManager _userInterfaceManager = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;

        private AlertsUI _ui;
        private PanelContainer _tooltip;
        private RichTextLabel _stateName;
        private RichTextLabel _stateDescription;


        // TODO: Put cooldowns in here
        [ViewVariables]
        private Dictionary<AlertKey, AlertControl> _alertControls
            = new Dictionary<AlertKey, AlertControl>();

        /// <summary>
        /// Allows calculating if we need to act due to this component being controlled by the current mob
        /// </summary>
        [ViewVariables]
        private bool CurrentlyControlled => _playerManager.LocalPlayer != null && _playerManager.LocalPlayer.ControlledEntity == Owner;

        protected override void Shutdown()
        {
            base.Shutdown();
            PlayerDetached();
        }

        public override void HandleMessage(ComponentMessage message, IComponent component)
        {
            base.HandleMessage(message, component);
            switch (message)
            {
                case PlayerAttachedMsg _:
                    PlayerAttached();
                    break;
                case PlayerDetachedMsg _:
                    PlayerDetached();
                    break;
            }
        }

        public override void HandleComponentState(ComponentState curState, ComponentState nextState)
        {
            base.HandleComponentState(curState, nextState);

            if (!(curState is AlertsComponentState state))
            {
                return;
            }

            // update the dict of states based on the array we got in the message
            var newAlerts = new Dictionary<AlertKey, AlertState>();
            foreach (var alertState in state.Alerts)
            {
                if (AlertManager.TryDecode(alertState.AlertEncoded, out var alert))
                {
                    newAlerts[alert.AlertKey] = alertState;
                }
                else
                {
                    Logger.ErrorS("alert", "unrecognized encoded alert {0}", alertState.AlertEncoded);
                }
            }

            Alerts = newAlerts;

            UpdateAlertsControls();
        }

        private void PlayerAttached()
        {
            if (!CurrentlyControlled || _ui != null)
            {
                return;
            }
            _ui = new AlertsUI();
            _userInterfaceManager.StateRoot.AddChild(_ui);
            _tooltip = new PanelContainer
            {
                Visible = false,
                StyleClasses = { StyleNano.StyleClassTooltipPanel }
            };
            var tooltipVBox = new VBoxContainer
            {
                RectClipContent = true
            };
            _tooltip.AddChild(tooltipVBox);
            _stateName = new RichTextLabel
            {
                MaxWidth = TooltipTextMaxWidth,
                StyleClasses = { StyleNano.StyleClassTooltipAlertTitle }
            };
            tooltipVBox.AddChild(_stateName);
            _stateDescription = new RichTextLabel
            {
                MaxWidth = TooltipTextMaxWidth,
                StyleClasses = { StyleNano.StyleClassTooltipAlertDescription }
            };
            tooltipVBox.AddChild(_stateDescription);

            _userInterfaceManager.PopupRoot.AddChild(_tooltip);

            UpdateAlertsControls();
        }

        private void PlayerDetached()
        {
            _ui?.Dispose();
            _ui = null;
            _alertControls.Clear();
        }

        /// <summary>
        /// Updates the displayed alerts based on current state of Alerts, performing
        /// a diff to ensure we only change what's changed (this avoids active tooltips disappearing any
        /// time state changes)
        /// </summary>
        private void UpdateAlertsControls()
        {
            if (!CurrentlyControlled || _ui == null)
            {
                return;
            }

            // remove any controls with keys no longer present
            var toRemove = new List<AlertKey>();
            foreach (var existingKey in _alertControls.Keys)
            {
                if (!Alerts.ContainsKey(existingKey))
                {
                    toRemove.Add(existingKey);
                }
            }

            foreach (var alertKeyToRemove in toRemove)
            {
                // remove and dispose the control
                _alertControls.Remove(alertKeyToRemove, out var control);
                control?.Dispose();
            }

            // TODO: Sort based on a YML defined order list, minimal shuffling around of other alerts

            // now we know that alertControls contains alerts that should still exist but
            // may need to updated,
            // also there may be some new alerts we need to show.
            // further, we need to ensure they are ordered w.r.t their configured order
            foreach (var (alertKey, alertStatus) in Alerts)
            {
                if (!AlertManager.TryDecode(alertStatus.AlertEncoded, out var newAlert))
                {
                    Logger.ErrorS("alert", "Unable to decode alert {0}", alertStatus.AlertEncoded);
                    continue;
                }

                if (_alertControls.TryGetValue(alertKey, out var existingAlertControl))
                {
                    // already being shown for this key, but the actual alert id might change.
                    if (existingAlertControl.Alert.ID != newAlert.ID)
                    {
                        // id is changing to a new alert, replace the control
                        var newAlertControl = CreateAlertControl(newAlert, alertStatus);
                        var idx = existingAlertControl.GetPositionInParent();
                        existingAlertControl.Dispose();
                        _ui.VBox.Children.Add(newAlertControl);
                        newAlertControl.SetPositionInParent(idx);
                        _alertControls[alertKey] = newAlertControl;
                    }
                    else
                    {
                        // id is the same, simply update the existing control severity
                        existingAlertControl.SetSeverity(alertStatus.Severity);
                    }
                }
                else
                {
                    // this is a new alert + alert key, create the control and add it
                    var newAlertControl = CreateAlertControl(newAlert, alertStatus);
                    _ui.VBox.Children.Add(newAlertControl);
                    _alertControls[alertKey] = newAlertControl;
                }
            }
        }

        private AlertControl CreateAlertControl(AlertPrototype alert, AlertState alertState)
        {

            var alertControl = new AlertControl(alert, alertState.Severity, _resourceCache);
            // show custom tooltip for the status control
            alertControl.OnShowTooltip += AlertOnOnShowTooltip;
            alertControl.OnHideTooltip += AlertOnOnHideTooltip;
            alertControl.OnPressed += args => AlertPressed(args, alertControl);

            return alertControl;
        }

        private void AlertOnOnHideTooltip(object sender, EventArgs e)
        {
            _tooltip.Visible = false;
        }

        private void AlertOnOnShowTooltip(object sender, EventArgs e)
        {
            var alertControl = (AlertControl) sender;
            _tooltip.Visible = true;
            _stateName.SetMessage(alertControl.Alert.Name);
            _stateDescription.SetMessage(alertControl.Alert.Description);
            // TODO: Text display of cooldown
            Tooltips.PositionTooltip(_tooltip);
        }

        private void AlertPressed(BaseButton.ButtonEventArgs args, AlertControl alert)
        {
            if (args.Event.Function != EngineKeyFunctions.UIClick)
            {
                return;
            }

            if (AlertManager.TryEncode(alert.Alert, out var encoded))
            {

                SendNetworkMessage(new ClickAlertMessage(encoded));
            }
            else
            {
                Logger.ErrorS("alert", "unable to encode alert {0}", alert.Alert.ID);
            }

        }

        public void FrameUpdate(float frameTime)
        {
            foreach (var (alertKey, alertControl) in _alertControls)
            {
                // reconcile all alert controls with their current cooldowns
                var alert = Alerts[alertKey];
                alertControl.UpdateCooldown(alert.Cooldown, _gameTiming.CurTime);
            }
        }

        protected override void AfterClearAlert()
        {
            UpdateAlertsControls();
        }
    }
}