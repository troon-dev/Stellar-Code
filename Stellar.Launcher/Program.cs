using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using _003CPrivateImplementationDetails_003E_007B06AF9570_002DDB29_002D425E_002DBDE9_002DAA3CBE8EC79D_007D;
using Photino.NET;
using Stellar.Launcher.Commands;
using Stellar.Launcher.Downloader;
using Stellar.Launcher.Ipc;
using Stellar.Launcher.Native;

namespace Stellar.Launcher;

internal static class Program
{
	private const int DEFAULT_CONNECTIONS = 6;

	private static readonly Bridge bridge = new Bridge();

	private static readonly Engine engine = new Engine(bridge);

	private static readonly Experience experience = new Experience(bridge);

	private static readonly Sockets sockets = new Sockets(bridge);

	private static readonly Rpc rpc = new Rpc();

	private static readonly Shortcuts shortcuts = new Shortcuts(bridge);

	private static readonly List<string> pendingDeepLinks = new List<string>();

	private static PhotinoWindow? overlay;

	[STAThread]
	private static void Main(string[] args)
	{
		if (!SingleInstance.Claim())
		{
			SingleInstance.Forward(args);
			return;
		}
		pendingDeepLinks.AddRange(SingleInstance.DeepLinksFrom(args));
		SingleInstance.RegisterScheme(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.g());
		SingleInstance.Listen(delegate(string[] forwarded)
		{
			List<string> list = SingleInstance.DeepLinksFrom(forwarded);
			if (list.Count > 0)
			{
				lock (pendingDeepLinks)
				{
					pendingDeepLinks.AddRange(list);
				}
				bridge.Emit(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Ea(), list);
			}
			AppWindows.Show(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.k());
			AppWindows.Unminimize(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.k());
			AppWindows.Focus(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.k());
		});
		RegisterCommands();
		Maintenance.Start();
		Migration.Run(DataDirectory());
		LegacyShortcuts.Clean();
		Content content = new Content(Path.Combine(AppContext.BaseDirectory, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.H(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.h()));
		content.Start();
		string origin = content.Origin;
		string text = origin + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.I();
		string overlayUrl = origin + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.i();
		PhotinoWindow window = new PhotinoWindow().SetTitle(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.J()).SetIconFile(Path.Combine(AppContext.BaseDirectory, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.j(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.K())).SetUseOsDefaultSize(false)
			.SetSize(new Size(1100, 660))
			.SetMinSize(1100, 660)
			.SetMaxSize(1200, 700)
			.SetChromeless(true)
			.SetResizable(true)
			.SetContextMenuEnabled(false)
			.SetDevToolsEnabled(false)
			.SetGrantBrowserPermissions(true)
			.SetTemporaryFilesPath(DataDirectory())
			.Center();
		bridge.Attach(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.k(), window);
		AppWindows.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.k(), window);
		window.RegisterWindowCreatedHandler(delegate
		{
			Chrome.Apply(window.WindowHandle, resizable: true);
		});
		AppWindows.OnOverlayNeeded(() => CreateOverlay(window, overlayUrl));
		window.RegisterWindowClosingHandler(delegate
		{
			rpc.Stop();
			Environment.Exit(0);
			return false;
		});
		window.Load(new Uri(text));
		window.WaitForClose();
	}

	private static string DataDirectory()
	{
		string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.L());
		Directory.CreateDirectory(text);
		return text;
	}

	private static ManagedWindow? CreateOverlay(PhotinoWindow parent, string url)
	{
		PhotinoWindow parent2 = parent;
		string url2 = url;
		if (overlay != null)
		{
			return AppWindows.Overlay;
		}
		parent2.Invoke(delegate
		{
			if (overlay == null)
			{
				overlay = new PhotinoWindow(parent2).SetTitle(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Em()).SetUseOsDefaultSize(false).SetSize(new Size(380, 240))
					.SetChromeless(true)
					.SetTransparent(true)
					.SetResizable(false)
					.SetTopMost(true)
					.SetContextMenuEnabled(false)
					.SetDevToolsEnabled(false)
					.SetTemporaryFilesPath(DataDirectory());
				bridge.Attach(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ax(), overlay);
				AppWindows.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ax(), overlay);
				overlay.RegisterWindowCreatedHandler(delegate
				{
					AppWindows.MakeOverlayChrome(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ax());
					AppWindows.Hide(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ax());
				});
				overlay.Load(new Uri(url2));
			}
		});
		return AppWindows.Overlay;
	}

	private static void RegisterCommands()
	{
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.l(), (Request request) => Build.CheckFileExists(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bU(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au())));
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.M(), (Request request) => Build.CheckFileExistsAndSize(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bU(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au()), request.OptionalNumber(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EB())));
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.m(), delegate(Request request)
		{
			Build.DeleteFiles(request.Strings(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Eb()));
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.N(), (Request request) => Build.SearchForVersion(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bU(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au())));
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.n(), (Request request) => Experience.Launch(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bU(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au()), request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EC(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au()), request.OptionalString(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Ec()), request.Strings(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ED())));
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.O(), async delegate(Request request)
		{
			List<FileEntry> files = request.Object<List<FileEntry>>(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Eb()) ?? new List<FileEntry>();
			await experience.DownloadFiles(files, request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.FV(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au())).ConfigureAwait(false);
			return (object?)null;
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.o(), delegate
		{
			Experience.ExitAll();
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.P(), (Request request) => Crypto.SocketEncrypt(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Ed(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au()), request.Bytes(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EE())));
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.p(), (Request request) => Crypto.SocketDecrypt(request.Bytes(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Ee()), request.Bytes(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EE())));
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Q(), (Request request) => Security.Status(request.Strings(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EF())));
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.q(), (Request request) => Security.ApplyExclusions(request.Strings(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EF())));
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.R(), delegate
		{
			Security.OpenSmartAppControl();
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.r(), delegate
		{
			Overlay.Show();
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.S(), delegate
		{
			Overlay.Hide();
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.s(), delegate(Request request)
		{
			Overlay.Resize(request.Double(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Ef()));
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.T(), delegate(Request request)
		{
			Overlay.SetPosition(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EG(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au()));
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.t(), delegate(Request request)
		{
			Overlay.SetInteractive(request.Bool(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Eg()));
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.U(), (Request _) => Overlay.Visible());
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.u(), (Request request) => Splash.SplashStatus(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EH(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au())));
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.V(), async delegate(Request request)
		{
			await Splash.SplashApply(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EH(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au()), request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eW(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au())).ConfigureAwait(false);
			return (object?)null;
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.v(), delegate(Request request)
		{
			Splash.SplashRestore(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EH(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au()));
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.W(), async (Request _) => await Engine.ListVersions().ConfigureAwait(false));
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.w(), async (Request request) => await engine.Install(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.FT(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au()), request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Ft(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au()), (int)request.OptionalNumber(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.FU()).GetValueOrDefault(6L)).ConfigureAwait(false));
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.X(), async (Request request) => await engine.Check(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.FT(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au()), request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bU(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au()), request.Bool(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Fu()), (int)request.OptionalNumber(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.FU()).GetValueOrDefault(6L)).ConfigureAwait(false));
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.x(), delegate
		{
			engine.Pause();
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Y(), delegate
		{
			engine.Resume();
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.y(), delegate
		{
			engine.Cancel();
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Z(), (Request _) => engine.CurrentSnapshot());
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.z(), (Request _) => engine.Busy);
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aA(), (Request _) => Updater.CurrentVersion());
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aa(), (Request _) => _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.J());
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aB(), (Request _) => Migration.Pull());
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ab(), delegate(Request request)
		{
			AppWindows.Minimize(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Eh(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.k()));
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aC(), delegate(Request request)
		{
			AppWindows.Unminimize(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Eh(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.k()));
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ac(), delegate(Request request)
		{
			AppWindows.Maximize(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Eh(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.k()));
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aD(), delegate(Request request)
		{
			AppWindows.Unminimize(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Eh(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.k()));
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ad(), delegate(Request request)
		{
			AppWindows.Show(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Eh(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.k()));
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aE(), delegate(Request request)
		{
			AppWindows.Hide(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Eh(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.k()));
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ae(), delegate
		{
			Environment.Exit(0);
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aF(), delegate(Request request)
		{
			AppWindows.Focus(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Eh(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.k()));
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.af(), (Request request) => AppWindows.IsVisible(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Eh(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.k())));
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aG(), (Request request) => AppWindows.IsMinimized(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Eh(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.k())));
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ag(), delegate(Request request)
		{
			AppWindows.SetTitle(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Eh(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.k()), request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aU(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au()));
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aH(), delegate(Request request)
		{
			AppWindows.StartDrag(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Eh(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.k()));
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ah(), delegate(Request request)
		{
			AppWindows.SetIgnoreCursorEvents(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Eh(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.k()), request.Bool(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EI()));
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aI(), (Request request) => OpenDialog(request));
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ai(), delegate(Request request)
		{
			PhotinoWindow photinoWindow2 = AppWindows.Main?.Window;
			if (photinoWindow2 == null)
			{
				return (object?)null;
			}
			string text4 = request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aU(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au());
			string text5 = request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aV(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au());
			return photinoWindow2.ShowSaveFile((text4.Length == 0) ? _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Ei() : text4, (text5.Length == 0) ? null : text5);
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aJ(), delegate(Request request)
		{
			(AppWindows.Main?.Window)?.ShowMessage(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aU(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.J()), request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EJ(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au()));
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aj(), delegate(Request request)
		{
			PhotinoWindow photinoWindow = AppWindows.Main?.Window;
			return (photinoWindow == null) ? ((object)false) : ((object)(photinoWindow.ShowMessage(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aU(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.J()), request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EJ(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au()), PhotinoDialogButtons.YesNo) == PhotinoDialogResult.Yes));
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aK(), delegate(Request request)
		{
			string fileName = request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bU(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au());
			try
			{
				System.Diagnostics.Process.Start(new ProcessStartInfo
				{
					FileName = fileName,
					UseShellExecute = true
				});
			}
			catch (Exception ex2)
			{
				throw new CommandFailure(ex2.Message);
			}
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ak(), delegate(Request request)
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo
			{
				FileName = request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Ej(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au()),
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};
			foreach (string item in request.Strings(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bi()))
			{
				processStartInfo.ArgumentList.Add(item);
			}
			using System.Diagnostics.Process process = System.Diagnostics.Process.Start(processStartInfo) ?? throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EK());
			string stdout = process.StandardOutput.ReadToEnd();
			string stderr = process.StandardError.ReadToEnd();
			process.WaitForExit();
			return new
			{
				code = process.ExitCode,
				stdout = stdout,
				stderr = stderr
			};
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aL(), delegate(Request request)
		{
			string text3 = request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bU(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au());
			try
			{
				System.Diagnostics.Process.Start(new ProcessStartInfo
				{
					FileName = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Ek(),
					Arguments = (Directory.Exists(text3) ? (_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BD() + text3 + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BD()) : (_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EL() + text3 + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BD())),
					UseShellExecute = true
				});
			}
			catch (Exception ex)
			{
				throw new CommandFailure(ex.Message);
			}
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.al(), delegate(Request request)
		{
			rpc.Stop();
			Environment.Exit((int)request.Number(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EC(), 0L));
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aM(), delegate
		{
			string processPath = Environment.ProcessPath;
			if (!string.IsNullOrEmpty(processPath))
			{
				try
				{
					System.Diagnostics.Process.Start(new ProcessStartInfo
					{
						FileName = processPath,
						UseShellExecute = true
					});
				}
				catch
				{
				}
			}
			rpc.Stop();
			Environment.Exit(0);
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.am(), async (Request _) => await Updater.Check().ConfigureAwait(false));
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aN(), async delegate
		{
			await Updater.Install().ConfigureAwait(false);
			return (object?)null;
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.an(), async delegate
		{
			await Updater.Install().ConfigureAwait(false);
			return (object?)null;
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aO(), delegate(Request request)
		{
			shortcuts.Register(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.El(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au()));
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ao(), delegate(Request request)
		{
			shortcuts.Unregister(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.El(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au()));
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aP(), (Request request) => shortcuts.IsRegistered(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.El(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au())));
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ap(), delegate
		{
			lock (pendingDeepLinks)
			{
				return pendingDeepLinks.ToList();
			}
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aQ(), delegate(Request request)
		{
			SingleInstance.RegisterScheme(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EM(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.g()));
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aq(), async delegate(Request request)
		{
			Dictionary<string, string> headers = request.Object<Dictionary<string, string>>(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Fv()) ?? new Dictionary<string, string>();
			return await sockets.Connect(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eW(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au()), headers).ConfigureAwait(false);
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aR(), async delegate(Request request)
		{
			JsonElement property = request.Args.GetProperty(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.FW());
			string text = property.GetProperty(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eY()).GetString() ?? _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Fg();
			string text2 = string.Empty;
			byte[] array = Array.Empty<byte>();
			if (property.TryGetProperty(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Fw(), out var jsonElement))
			{
				if (text == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.FG())
				{
					text2 = jsonElement.GetString() ?? string.Empty;
				}
				else if (jsonElement.ValueKind == JsonValueKind.Array)
				{
					array = new byte[jsonElement.GetArrayLength()];
					int num = 0;
					foreach (JsonElement item2 in jsonElement.EnumerateArray())
					{
						array[num++] = (byte)item2.GetInt32();
					}
				}
			}
			await sockets.Send(request.Number(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bh(), 0L), text, array, text2).ConfigureAwait(false);
			return (object?)null;
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ar(), async delegate(Request request)
		{
			await sockets.Disconnect(request.Number(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bh(), 0L)).ConfigureAwait(false);
			return (object?)null;
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aS(), async delegate(Request request)
		{
			await rpc.Start(request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.FX(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au())).ConfigureAwait(false);
			return (object?)null;
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.@as(), delegate
		{
			rpc.Stop();
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aT(), async delegate(Request request)
		{
			ActivityPayload activity = request.Object<ActivityPayload>(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Fx()) ?? new ActivityPayload();
			await rpc.SetActivity(activity).ConfigureAwait(false);
			return (object?)null;
		});
		bridge.Register(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.at(), async delegate
		{
			await rpc.ClearActivity().ConfigureAwait(false);
			return (object?)null;
		});
	}

	private static object? OpenDialog(Request request)
	{
		PhotinoWindow photinoWindow = AppWindows.Main?.Window;
		if (photinoWindow == null)
		{
			return new List<string>();
		}
		string text = request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aU(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au());
		string text2 = request.String(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aV(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.au());
		bool flag = request.Bool(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.av());
		if (!request.Bool(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aW()))
		{
			return photinoWindow.ShowOpenFile((text.Length == 0) ? _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aX() : text, (text2.Length == 0) ? null : text2, flag)?.ToList() ?? new List<string>();
		}
		return photinoWindow.ShowOpenFolder((text.Length == 0) ? _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aw() : text, (text2.Length == 0) ? null : text2, flag)?.ToList() ?? new List<string>();
	}
}
