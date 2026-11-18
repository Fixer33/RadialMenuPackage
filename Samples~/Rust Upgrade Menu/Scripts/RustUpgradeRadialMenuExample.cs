using System;
using RadialMenu;
using RadialMenu.Contracts;
using RadialMenu.Enums;
using RadialMenu.MenuTypes;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rust_Upgrade_Menu
{
    public class RustUpgradeRadialMenuExample : MonoBehaviour
    {
        [SerializeField] private StyleSheet _additionalStyle;
        [SerializeField] private Sprite _demolishSprite;
        [SerializeField] private Sprite _woodSprite;
        [SerializeField] private Sprite _stoneSprite;
        [SerializeField] private Sprite _metalSprite;
        [SerializeField] private Sprite _hqmSprite;
        [SerializeField] private PanelSettings _panelSettings;
        private IRadialMenu _radialMenu;
        private IRadialMenuItem[] _items;
        
        private void Start()
        {
            Color textPrimary = new Color32(0xE8, 0xEC, 0xF3, 0xFF);
            _items = new IRadialMenuItem[]
            {
                new UpgradeOption("Upgrade to wood", "Upgrade the element to wood", _woodSprite, textPrimary, Color.white),
                new UpgradeOption("Upgrade to stone", "Upgrade the element to stone", _stoneSprite, textPrimary, Color.white),
                new UpgradeOption("Upgrade to metal", "Upgrade the element to metal", _metalSprite, textPrimary, Color.white),
                new UpgradeOption("Upgrade to hqm", "Upgrade the element to hqm", _hqmSprite, textPrimary, Color.white),
                new UpgradeOption("Demolish", "Demolish the element", _demolishSprite, textPrimary, Color.white),
            };
            
            // Design tokens: --bg-raised, --border-default, --accent, --bg-overlay, --border-strong
            Color raised = new Color32(0x1E, 0x22, 0x2B, 0xFF);
            Color border = new Color32(0x34, 0x3B, 0x49, 0xFF);
            Color accent = new Color32(0x3B, 0x82, 0xF6, 0xFF);
            Color centerBackground = new Color32(0x27, 0x2C, 0x37, 0xFF);
            Color centerStroke = new Color32(0x47, 0x50, 0x66, 0xFF);
            _radialMenu = RadialMenuBuilder
                .Create(_panelSettings, _items)
                .WithVisibilityAnimationTime(0.1f)
                .WithAdditionalStyleSheet(_additionalStyle)
                .InScreenPosition(new Vector2(Screen.width * .5f, Screen.height * .5f))
                .WithMainOuterRadius(236, 240)
                .WithMainInnerRadius(150, 160)
                .WithMainColors(raised, accent, border, accent)
                .WithCenterElementRadius(50)
                .WithCenterElementColors(centerBackground, centerBackground, centerStroke, centerStroke)
                .WithActionOnClickOutOfBounds(RadialMenuAction.PerformItemAndClose)
                .Build<ScaledRadialMenu>();
        }
        
        private void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                _radialMenu.Show();
            }
            else if (Input.GetMouseButtonUp(1))
            {
                _radialMenu.Hide();
            }
        }

        private void OnDestroy()
        {
            _radialMenu?.Destroy();
        }
    }
}