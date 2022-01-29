using ColossalFramework.UI;
using UnityEngine;

namespace PrisonHelicopter.Utils {
   public static class UiUtil
    {
        private const int ButtonSize = 16;

        public static UIFont GetUIFont(string name)
        {
            UIFont[] fonts = Resources.FindObjectsOfTypeAll<UIFont>();

            foreach (UIFont font in fonts)
            {
                if (font.name.CompareTo(name) == 0)
                {
                    return font;
                }
            }

            return null;
        }

        public static UIPanel CreatePanel(UIComponent parent, string name)
        {
            UIPanel panel = parent.AddUIComponent<UIPanel>();
            panel.name = name;

            return panel;
        }

        public static UISprite CreateSprite(UIComponent parentComponent, MouseEventHandler handler, Vector3 offset)
        {
            return CreateSprite("AllowPrisonHelicoptersButton", null, offset,
                parentComponent, handler);
        }

        public static UISprite CreateSprite(string buttonName, string tooltip, Vector3 offset,
            UIComponent parentComponent, MouseEventHandler handler)
        {

            var sprite = UIView.GetAView().AddUIComponent(typeof(UISprite)) as UISprite;
            if (sprite == null)
            {
                return null;
            }
            sprite.canFocus = false;
            sprite.name = buttonName;
            sprite.width = ButtonSize;
            sprite.height = ButtonSize;
            sprite.tooltip = tooltip;
            sprite.eventClick += handler;
            sprite.AlignTo(parentComponent, UIAlignAnchor.TopRight);
            sprite.relativePosition = offset;
            return sprite;
        }

        public static UILabel CreateLabel(string text, UIComponent parentComponent, Vector3 offset)
        {
            var label = UIView.GetAView().AddUIComponent(typeof(UILabel)) as UILabel;
            if (label == null)
            {
                return null;
            }
            label.text = text;
            label.AlignTo(parentComponent, UIAlignAnchor.TopRight);
            label.relativePosition = offset;
            return label;
        }

        public static UICheckBox CreateCheckBox(UIComponent parent, string name, string text, bool state)
        {
            UICheckBox checkBox = parent.AddUIComponent<UICheckBox>();
            checkBox.name = name;

            checkBox.height = 16f;
            checkBox.width = parent.width - 10f;

            UISprite uncheckedSprite = checkBox.AddUIComponent<UISprite>();
            uncheckedSprite.spriteName = "check-unchecked";
            uncheckedSprite.size = new Vector2(16f, 16f);
            uncheckedSprite.relativePosition = Vector3.zero;

            UISprite checkedSprite = checkBox.AddUIComponent<UISprite>();
            checkedSprite.spriteName = "check-checked";
            checkedSprite.size = new Vector2(16f, 16f);
            checkedSprite.relativePosition = Vector3.zero;
            checkBox.checkedBoxObject = checkedSprite;

            checkBox.label = checkBox.AddUIComponent<UILabel>();
            checkBox.label.text = text;
            checkBox.label.font = GetUIFont("OpenSans-Regular");
            checkBox.label.autoSize = false;
            checkBox.label.height = 20f;
            checkBox.label.verticalAlignment = UIVerticalAlignment.Middle;
            checkBox.label.relativePosition = new Vector3(20f, 0f);

            checkBox.isChecked = state;

            return checkBox;
        }
    }
}
