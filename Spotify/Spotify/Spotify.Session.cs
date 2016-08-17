using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpotiFire.SpotifyLib;
using System.Windows.Forms;

namespace Spotify
{
    public partial class Spotify
    {
        private void SubscribeSessionEvents(ISession session)
        {
            session.LoginComplete += new SessionEventHandler(session_LoginComplete);
            session.LogoutComplete += new SessionEventHandler(session_LogoutComplete);
            session.MessageToUser += new SessionEventHandler(session_MessageToUser);
            session.ConnectionError += new SessionEventHandler(session_ConnectionError);
            session.Exception += new SessionEventHandler(session_Exception);
            session.PlayTokenLost += new SessionEventHandler(session_PlayTokenLost);
            session.ConnectionstateUpdated += new SessionEventHandler(session_ConnectionstateUpdated);          //LK, 11-jun-2016: Added missing session events
            session.PrivateSessionModeChanged += new SessionEventHandler(session_PrivateSessionModeChanged);    //LK, 11-jun-2016: Added missing session events
        }

        void session_LogoutComplete(ISession sender, SessionEventArgs e)
        {
            WriteLog("Logout completed");
            loginComplete = false;
        }

        void session_PlayTokenLost(ISession sender, SessionEventArgs e)
        {
            this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                {
                    WriteLog("Play token lost: " + e.Message + ", status = " + e.Status.ToString());
                    CF_displayMessage(pluginLang.ReadField("/AppLang/Spotify/PlayTokenLost"));
                    Pause();
                }));
        }

        void session_ConnectionstateUpdated(ISession sender, SessionEventArgs e)
        {
            this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                {
                    WriteLog("Connection state Update: " + e.Message + ", status = " + e.Status.ToString());
                }));
        }

        void session_PrivateSessionModeChanged(ISession sender, SessionEventArgs e)
        {
            this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                {
                    WriteLog("Private session mode changed: " + e.Message + ", status = " + e.Status.ToString());
                }));
        }

        void session_Exception(ISession sender, SessionEventArgs e)
        {
            this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                {
                    WriteError(e.Message);
                    CF_displayMessage(e.Status.ToString() + Environment.NewLine + e.Message);
                }));
        }

        void session_ConnectionError(ISession sender, SessionEventArgs e)
        {
                    WriteLog(e.Message + ", status = " + e.Status.ToString());
        }

        void session_MessageToUser(ISession sender, SessionEventArgs e)
        {
            this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                {
                    WriteLog(e.Message);
                    CF_displayMessage(e.Message);
                }));
        }

        bool loginComplete = false;
        void session_LoginComplete(ISession sender, SessionEventArgs e)
        {
            this.ParentForm.BeginInvoke(new MethodInvoker(() =>
                {
                    WriteLog(e.Message + ", status = " + e.Status.ToString());
                    if (e.Status != sp_error.OK)
                    {
                        CF_displayMessage("Login Failed: " + e.Status + Environment.NewLine + e.Message);
                    }
                    else
                    {
                        OnLoginComplete();
                    }
                }));
        }

        bool firstLogin = true;
        private void OnLoginComplete()
        {
            this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                {
                    WriteLog("Login completed");
                    loginComplete = true;
                    CF_systemCommand(centrafuse.Plugins.CF_Actions.HIDEINFO);
                    if (firstLogin)
                    {
                        firstLogin = false;
                        RestoreNowPlaying(true);
                    }
                }));
        }
    }
}
