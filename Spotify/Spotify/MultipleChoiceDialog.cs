using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using centrafuse.Plugins;
using System.Drawing;
using System.Windows.Forms;

namespace Spotify
{
    internal class MultipleChoiceDialog : CFDialog
    {
        private const string PLUGIN_NAME = "Spotify";
        private IEnumerable<string> buttonTexts;
        private string title;
        public MultipleChoiceDialog(int display, bool rearscreen, string title, IEnumerable<string> buttonTexts)
		{
            if (buttonTexts.Count() < 2 || buttonTexts.Count() > 5)
            {
                throw new Exception("Button count has to be between 2 and 5");
            }

            this.buttonTexts = buttonTexts;
            this.title = title;

            this.CF_displayHooks.displayNumber = display;
            this.CF_displayHooks.rearScreen = rearscreen;
        }

        public override void CF_pluginInit()
        {
            base.CF3_initPlugin(PLUGIN_NAME, true);
            this.CF_localskinsetup();
        }

        private const string dynamicButtonIDFormat = "DynamicButton{0}";
        public override void CF_localskinsetup()
        {
            base.CF3_initDialog("MultipleChoiceDialog");
            CF_updateText("Spotify.TitleLabel", title);
            var cancelButton = buttonArray[CF_getButtonID("Spotify.Cancel")];
            cancelButton.Text = this.pluginLang.ReadField("/APPLANG/SPOTIFY/CANCEL");
            cancelButton.buttonText = true;

            for (int i = 1; i <= 5; i++)
            {
                string buttonID = string.Format(dynamicButtonIDFormat, i);
                int arrayIx = i - 1;
                if (buttonTexts.Count() > arrayIx)
                {
                    var button = buttonArray[CF_getButtonID(buttonID)];
                    button.buttonText = true;
                    button.buttonEnabled = true;
                    button.Text = buttonTexts.ElementAt(arrayIx);
                }
                else
                {
                    CF_setButtonEnableFlag(buttonID, false);
                }
            }
            this.Invalidate();
        }

        public int Choice
        {
            get;
            private set;
        }

        public override bool CF_pluginCMLCommand(string command, string[] strparams, CF_ButtonState state, int zone)
        {
            if (command.Equals("Spotify.Cancel"))
            {
                if (state >= CF_ButtonState.Click)
                {
                    base.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                }
                return true;
            }
            else if (command.StartsWith("Spotify.Choice"))
            {
                if (state >= CF_ButtonState.Click)
                {
                    int choiceNumber = int.Parse(command.Last().ToString());
                    choiceNumber--; //they are off by 1
                    Choice = choiceNumber;
                    base.DialogResult = System.Windows.Forms.DialogResult.OK;
                }
                return true;
            }
            else
                return base.CF_pluginCMLCommand(command, strparams, state, zone);
        }
    }
}
