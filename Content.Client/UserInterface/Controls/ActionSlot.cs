﻿#nullable enable
using System;
using Content.Client.UserInterface.Stylesheets;
using Content.Shared.Actions;
using OpenToolkit.Mathematics;
using Robust.Client.GameObjects;
using Robust.Client.Interfaces.GameObjects.Components;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.Utility;
using Robust.Shared.Input;
using Robust.Shared.Interfaces.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Utility;

namespace Content.Client.UserInterface.Controls
{
    /// <summary>
    /// A slot in the action hotbar.
    /// Note that this should never be Disabled internally, it always needs to be clickable regardless
    /// of whether the action is disabled (so actions can still be dragged / unassigned).
    /// Thus any event handlers should check if the action is enabled.
    /// </summary>
    public class ActionSlot : ContainerButton
    {
        // shorter than default tooltip delay so user can more easily
        // see what actions they've been given
        private const float CustomTooltipDelay = 0.5f;

        private static readonly string EnabledColor = "#7b7e9e";
        private static readonly string DisabledColor = "#950000";

        /// <summary>
        /// Current action in this slot.
        /// </summary>
        public BaseActionPrototype? Action { get; private set; }

        /// <summary>
        /// true if there is an action or itemaction assigned to the slot
        /// </summary>
        public bool HasAssignment => Action != null;

        /// <summary>
        /// Whether the action in this slot is currently shown as usable.
        /// Not to be confused with Control.Disabled.
        /// </summary>
        public bool ActionEnabled { get; private set; }

        /// <summary>
        /// Item the action is provided by, only valid if Action is an ItemActionPrototype. May be null
        /// if the item action is not yet tied to an item.
        /// </summary>
        public IEntity? Item { get; private set; }

        /// <summary>
        /// Separate from Pressed, if true, this button will be displayed as pressed
        /// regardless of the Pressed setting.
        /// </summary>
        public bool ToggledOn
        {
            get => _toggledOn;
            set
            {
                if (_toggledOn == value) return;
                _toggledOn = value;
                DrawModeChanged();
            }

        }

        /// <summary>
        /// 1-10 corresponding to the number label on the slot (10 is labeled as 0)
        /// </summary>
        public byte SlotNumber { get; private set; }
        public byte SlotIndex => (byte) (SlotNumber - 1);

        /// <summary>
        /// Total duration of the current cooldown in seconds. TimeSpan.Zero if no duration / cooldown.
        /// </summary>
        public TimeSpan TotalDuration { get; private set; }
        /// <summary>
        /// Remaining cooldown in seconds. TimeSpan.Zero if no cooldown or cooldown
        /// is over.
        /// </summary>
        public TimeSpan CooldownRemaining { get; private set; }

        public bool IsOnCooldown => CooldownRemaining != TimeSpan.Zero;

        private readonly RichTextLabel _number;
        private readonly TextureRect _icon;
        private readonly SpriteView _itemSpriteView;
        private readonly CooldownGraphic _cooldownGraphic;

        private bool _toggledOn;

        /// <summary>
        /// Creates an action slot for the specified number
        /// </summary>
        /// <param name="slotNumber">slot this corresponds to, 1-10 (10 is labeled as 0). Any value
        /// greater than 10 will have a blank number</param>
        public ActionSlot(byte slotNumber)
        {
            SlotNumber = slotNumber;

            CustomMinimumSize = (64, 64);

            SizeFlagsVertical = SizeFlags.None;
            TooltipDelay = CustomTooltipDelay;

            _number = new RichTextLabel
            {
                StyleClasses = {StyleNano.StyleClassHotbarSlotNumber}
            };
            _number.SetMessage(SlotNumberLabel());

            _icon = new TextureRect
            {
                SizeFlagsHorizontal = SizeFlags.FillExpand,
                SizeFlagsVertical = SizeFlags.FillExpand,
                Stretch = TextureRect.StretchMode.Scale,
                Visible = false
            };
            _itemSpriteView = new SpriteView
            {
                SizeFlagsHorizontal = SizeFlags.ShrinkEnd,
                SizeFlagsVertical = SizeFlags.ShrinkEnd,
                Visible = false
            };
            _cooldownGraphic = new CooldownGraphic();

            // padding to the left of the number to shift it right
            var paddingBox = new HBoxContainer()
            {
                SizeFlagsHorizontal = SizeFlags.FillExpand,
                SizeFlagsVertical = SizeFlags.FillExpand,
                CustomMinimumSize = (64, 64)
            };
            paddingBox.AddChild(new Control()
            {
                CustomMinimumSize = (4, 4),
                SizeFlagsVertical = SizeFlags.Fill
            });
            paddingBox.AddChild(_number);
            // padding to the left of the item icon
            // TODO: which corner do we want it im
            var paddingBoxItemIcon = new HBoxContainer()
            {
                SizeFlagsHorizontal = SizeFlags.FillExpand,
                SizeFlagsVertical = SizeFlags.FillExpand,
                CustomMinimumSize = (64, 64)
            };
            paddingBoxItemIcon.AddChild(new Control()
            {
                CustomMinimumSize = (32, 32),
                SizeFlagsVertical = SizeFlags.Fill
            });
            paddingBoxItemIcon.AddChild(_itemSpriteView);
            AddChild(_icon);
            AddChild(_cooldownGraphic);
            AddChild(paddingBox);
            AddChild(paddingBoxItemIcon);

            UpdateCooldown(null, TimeSpan.Zero);

        }

        /// <summary>
        /// Updates the displayed cooldown amount, clearing cooldown if alertCooldown is null
        /// </summary>
        /// <param name="alertCooldown">cooldown start and end</param>
        /// <param name="curTime">current game time</param>
        public void UpdateCooldown((TimeSpan Start, TimeSpan End)? alertCooldown, in TimeSpan curTime)
        {
            if (!alertCooldown.HasValue)
            {
                _cooldownGraphic.Progress = 0;
                _cooldownGraphic.Visible = false;
                TotalDuration = TimeSpan.Zero;
                CooldownRemaining = TimeSpan.Zero;
            }
            else
            {

                var start = alertCooldown.Value.Start;
                var end = alertCooldown.Value.End;

                TotalDuration = end - start;
                var length = TotalDuration.TotalSeconds;
                var progress = (curTime - start).TotalSeconds / length;
                var ratio = (progress <= 1 ? (1 - progress) : (curTime - end).TotalSeconds * -5);

                CooldownRemaining = end > curTime ? (end - curTime) : TimeSpan.Zero;

                _cooldownGraphic.Progress = MathHelper.Clamp((float)ratio, -1, 1);
                _cooldownGraphic.Visible = ratio > -1f;
            }
        }

        /// <summary>
        /// Updates the action assigned to this slot.
        /// </summary>
        /// <param name="action">action to assign</param>
        /// <param name="actionEnabled">whether action should initially appear enable or disabled</param>
        public void Assign(ActionPrototype action, bool actionEnabled)
        {
            // already assigned
            if (Action != null && Action == action) return;

            Action = action;
            Item = null;
            _icon.Texture = Action.Icon.Frame0();
            _icon.Visible = true;
            _itemSpriteView.Visible = false;
            _itemSpriteView.Sprite = null;
            Pressed = false;
            ToggledOn = false;
            ActionEnabled = actionEnabled;
            DrawModeChanged();
            _number.SetMessage(SlotNumberLabel());
        }

        /// <summary>
        /// Updates the item action assigned to this slot. The action will always be shown as disabled
        /// until it is tied to a specific item.
        /// </summary>
        /// <param name="action">action to assign</param>
        public void Assign(ItemActionPrototype action)
        {
            // already assigned
            if (Action != null && Action == action && Item == null) return;

            Action = action;
            Item = null;
            _icon.Texture = action.Icon.Frame0();
            _icon.Visible = true;
            _itemSpriteView.Visible = false;
            _itemSpriteView.Sprite = null;
            Pressed = false;
            ToggledOn = false;
            ActionEnabled = false;
            DrawModeChanged();
            _number.SetMessage(SlotNumberLabel());
        }

        /// <summary>
        /// Updates the item action assigned to this slot, tied to a specific item.
        /// </summary>
        /// <param name="action">action to assign</param>
        /// <param name="item">item the action is provided by</param>
        /// <param name="actionEnabled">whether action should initially appear enable or disabled</param>
        public void Assign(ItemActionPrototype action, IEntity item, bool actionEnabled)
        {
            // already assigned
            if (Action != null && Action == action && Item == item) return;

            Action = action;
            Item = item;
            _icon.Texture = action.Icon.Frame0();
            _icon.Visible = true;
            if (Item.TryGetComponent<ISpriteComponent>(out var spriteComponent))
            {
                _itemSpriteView.Sprite = spriteComponent;
                _itemSpriteView.Visible = true;
            }
            else
            {
                _itemSpriteView.Visible = false;
                _itemSpriteView.Sprite = null;
            }

            Pressed = false;
            ToggledOn = false;
            ActionEnabled = false;
            DrawModeChanged();
            _number.SetMessage(SlotNumberLabel());
        }

        /// <summary>
        /// Clears the action assigned to this slot
        /// </summary>
        public void Clear()
        {
            if (!HasAssignment) return;
            Action = null;
            Item = null;
            _icon.Texture = null;
            _icon.Visible = false;
            _itemSpriteView.Visible = false;
            _itemSpriteView.Sprite = null;
            ToggledOn = false;
            Pressed = false;
            DrawModeChanged();
            UpdateCooldown(null, TimeSpan.Zero);
            _number.SetMessage(SlotNumberLabel());
        }

        /// <summary>
        /// Display the action in this slot (if there is one) as enabled
        /// </summary>
        public void EnableAction()
        {
            if (ActionEnabled || !HasAssignment) return;

            ActionEnabled = true;
            DrawModeChanged();
            _number.SetMessage(SlotNumberLabel());
        }

        /// <summary>
        /// Display the action in this slot (if there is one) as disabled.
        /// The slot is still clickable.
        /// </summary>
        public void DisableAction()
        {
            if (!ActionEnabled || !HasAssignment) return;

            ActionEnabled = false;
            DrawModeChanged();
            _number.SetMessage(SlotNumberLabel());
            UpdateCooldown(null, TimeSpan.Zero);
        }

        private FormattedMessage SlotNumberLabel()
        {
            if (SlotNumber > 10) return FormattedMessage.FromMarkup("");
            var number = SlotNumber == 10 ? "0" : SlotNumber.ToString();
            var color = (ActionEnabled || !HasAssignment) ? EnabledColor : DisabledColor;
            return FormattedMessage.FromMarkup("[color=" + color + "]" + number + "[/color]");
        }


        protected override void DrawModeChanged()
        {
            base.DrawModeChanged();
            // when there's no action or its on cooldown or disabled, it should
            // not appear as if it's interactable (no mouseover or press style)
            if (!HasAssignment)
            {
                SetOnlyStylePseudoClass(StylePseudoClassNormal);
            }
            else if (_cooldownGraphic.Visible && ActionEnabled)
            {
                SetOnlyStylePseudoClass(ToggledOn ? StylePseudoClassPressed : StylePseudoClassNormal);
            }
            else if (!ActionEnabled)
            {
                SetOnlyStylePseudoClass(ToggledOn ? StylePseudoClassPressed : StylePseudoClassDisabled);
            }
            else if (DrawMode != DrawModeEnum.Hover && ToggledOn)
            {
                SetOnlyStylePseudoClass(StylePseudoClassPressed);
            }
        }

        /// <summary>
        /// Simulates clicking on this, but being done via a keybind
        /// </summary>
        public void HandleKeybind(BoundKeyState keyState)
        {
            // simulate a click, using UIClick so it won't be treated as a possible drag / drop attempt
            var guiArgs = new GUIBoundKeyEventArgs(EngineKeyFunctions.UIClick,
                keyState, new ScreenCoordinates(GlobalPixelPosition), true,
                default,
                default);
            if (keyState == BoundKeyState.Down)
            {
                KeyBindDown(guiArgs);
            }
            else
            {
                KeyBindUp(guiArgs);
            }

        }

    }
}