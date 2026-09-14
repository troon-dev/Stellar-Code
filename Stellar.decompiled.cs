using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using <PrivateImplementationDetails>{06AF9570-DB29-425E-BDE9-AA3CBE8EC79D};
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Photino.NET;
using Stellar.Launcher.Commands;
using Stellar.Launcher.Downloader;
using Stellar.Launcher.Ipc;
using Stellar.Launcher.Native;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: TargetFramework(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]
[assembly: AssemblyCompany("Stellar")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("2.1.6.0")]
[assembly: AssemblyInformationalVersion("2.1.6+3265e13babf131eb46802305c3b524feb2372454")]
[assembly: AssemblyProduct("Stellar")]
[assembly: AssemblyTitle("Stellar")]
[assembly: TargetPlatform("Windows7.0")]
[assembly: SupportedOSPlatform("Windows7.0")]
[assembly: SecurityPermission(8, SkipVerification = true)]
[assembly: AssemblyVersion("2.1.6.0")]
[module: UnverifiableCode]
[module: RefSafetyRules(11)]
namespace Stellar.Launcher
{
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
			SingleInstance.RegisterScheme(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.g());
			SingleInstance.Listen(delegate(string[] forwarded)
			{
				List<string> list = SingleInstance.DeepLinksFrom(forwarded);
				if (list.Count > 0)
				{
					lock (pendingDeepLinks)
					{
						pendingDeepLinks.AddRange(list);
					}
					bridge.Emit(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Ea(), list);
				}
				AppWindows.Show(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.k());
				AppWindows.Unminimize(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.k());
				AppWindows.Focus(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.k());
			});
			RegisterCommands();
			Maintenance.Start();
			Migration.Run(DataDirectory());
			LegacyShortcuts.Clean();
			Content content = new Content(Path.Combine(AppContext.BaseDirectory, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.H(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.h()));
			content.Start();
			string origin = content.Origin;
			string text = origin + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.I();
			string overlayUrl = origin + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.i();
			PhotinoWindow window = new PhotinoWindow().SetTitle(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.J()).SetIconFile(Path.Combine(AppContext.BaseDirectory, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.j(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.K())).SetUseOsDefaultSize(false)
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
			bridge.Attach(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.k(), window);
			AppWindows.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.k(), window);
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
			string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.L());
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
					overlay = new PhotinoWindow(parent2).SetTitle(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Em()).SetUseOsDefaultSize(false).SetSize(new Size(380, 240))
						.SetChromeless(true)
						.SetTransparent(true)
						.SetResizable(false)
						.SetTopMost(true)
						.SetContextMenuEnabled(false)
						.SetDevToolsEnabled(false)
						.SetTemporaryFilesPath(DataDirectory());
					bridge.Attach(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ax(), overlay);
					AppWindows.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ax(), overlay);
					overlay.RegisterWindowCreatedHandler(delegate
					{
						AppWindows.MakeOverlayChrome(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ax());
						AppWindows.Hide(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ax());
					});
					overlay.Load(new Uri(url2));
				}
			});
			return AppWindows.Overlay;
		}

		private static void RegisterCommands()
		{
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.l(), (Request request) => Build.CheckFileExists(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bU(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au())));
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.M(), (Request request) => Build.CheckFileExistsAndSize(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bU(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au()), request.OptionalNumber(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EB())));
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.m(), delegate(Request request)
			{
				Build.DeleteFiles(request.Strings(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Eb()));
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.N(), (Request request) => Build.SearchForVersion(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bU(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au())));
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.n(), (Request request) => Experience.Launch(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bU(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au()), request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EC(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au()), request.OptionalString(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Ec()), request.Strings(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ED())));
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.O(), async delegate(Request request)
			{
				List<FileEntry> files = request.Object<List<FileEntry>>(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Eb()) ?? new List<FileEntry>();
				await experience.DownloadFiles(files, request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.FV(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au())).ConfigureAwait(false);
				return (object?)null;
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.o(), delegate
			{
				Experience.ExitAll();
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.P(), (Request request) => Crypto.SocketEncrypt(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Ed(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au()), request.Bytes(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EE())));
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.p(), (Request request) => Crypto.SocketDecrypt(request.Bytes(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Ee()), request.Bytes(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EE())));
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Q(), (Request request) => Security.Status(request.Strings(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EF())));
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.q(), (Request request) => Security.ApplyExclusions(request.Strings(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EF())));
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.R(), delegate
			{
				Security.OpenSmartAppControl();
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.r(), delegate
			{
				Overlay.Show();
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.S(), delegate
			{
				Overlay.Hide();
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.s(), delegate(Request request)
			{
				Overlay.Resize(request.Double(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Ef()));
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.T(), delegate(Request request)
			{
				Overlay.SetPosition(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EG(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au()));
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.t(), delegate(Request request)
			{
				Overlay.SetInteractive(request.Bool(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Eg()));
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.U(), (Request _) => Overlay.Visible());
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.u(), (Request request) => Splash.SplashStatus(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EH(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au())));
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.V(), async delegate(Request request)
			{
				await Splash.SplashApply(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EH(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au()), request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eW(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au())).ConfigureAwait(false);
				return (object?)null;
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.v(), delegate(Request request)
			{
				Splash.SplashRestore(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EH(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au()));
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.W(), async (Request _) => await Engine.ListVersions().ConfigureAwait(false));
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.w(), async (Request request) => await engine.Install(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.FT(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au()), request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Ft(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au()), (int)request.OptionalNumber(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.FU()).GetValueOrDefault(6L)).ConfigureAwait(false));
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.X(), async (Request request) => await engine.Check(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.FT(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au()), request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bU(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au()), request.Bool(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Fu()), (int)request.OptionalNumber(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.FU()).GetValueOrDefault(6L)).ConfigureAwait(false));
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.x(), delegate
			{
				engine.Pause();
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Y(), delegate
			{
				engine.Resume();
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.y(), delegate
			{
				engine.Cancel();
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Z(), (Request _) => engine.CurrentSnapshot());
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.z(), (Request _) => engine.Busy);
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aA(), (Request _) => Updater.CurrentVersion());
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aa(), (Request _) => 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.J());
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aB(), (Request _) => Migration.Pull());
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ab(), delegate(Request request)
			{
				AppWindows.Minimize(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Eh(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.k()));
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aC(), delegate(Request request)
			{
				AppWindows.Unminimize(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Eh(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.k()));
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ac(), delegate(Request request)
			{
				AppWindows.Maximize(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Eh(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.k()));
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aD(), delegate(Request request)
			{
				AppWindows.Unminimize(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Eh(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.k()));
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ad(), delegate(Request request)
			{
				AppWindows.Show(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Eh(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.k()));
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aE(), delegate(Request request)
			{
				AppWindows.Hide(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Eh(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.k()));
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ae(), delegate
			{
				Environment.Exit(0);
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aF(), delegate(Request request)
			{
				AppWindows.Focus(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Eh(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.k()));
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.af(), (Request request) => AppWindows.IsVisible(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Eh(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.k())));
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aG(), (Request request) => AppWindows.IsMinimized(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Eh(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.k())));
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ag(), delegate(Request request)
			{
				AppWindows.SetTitle(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Eh(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.k()), request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aU(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au()));
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aH(), delegate(Request request)
			{
				AppWindows.StartDrag(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Eh(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.k()));
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ah(), delegate(Request request)
			{
				AppWindows.SetIgnoreCursorEvents(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Eh(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.k()), request.Bool(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EI()));
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aI(), (Request request) => OpenDialog(request));
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ai(), delegate(Request request)
			{
				PhotinoWindow photinoWindow2 = AppWindows.Main?.Window;
				if (photinoWindow2 == null)
				{
					return (object?)null;
				}
				string text4 = request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aU(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au());
				string text5 = request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aV(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au());
				return photinoWindow2.ShowSaveFile((text4.Length == 0) ? 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Ei() : text4, (text5.Length == 0) ? null : text5);
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aJ(), delegate(Request request)
			{
				(AppWindows.Main?.Window)?.ShowMessage(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aU(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.J()), request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EJ(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au()));
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aj(), delegate(Request request)
			{
				PhotinoWindow photinoWindow = AppWindows.Main?.Window;
				return (photinoWindow == null) ? ((object)false) : ((object)(photinoWindow.ShowMessage(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aU(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.J()), request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EJ(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au()), PhotinoDialogButtons.YesNo) == PhotinoDialogResult.Yes));
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aK(), delegate(Request request)
			{
				string fileName = request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bU(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au());
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
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ak(), delegate(Request request)
			{
				ProcessStartInfo processStartInfo = new ProcessStartInfo
				{
					FileName = request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Ej(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au()),
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true
				};
				foreach (string item in request.Strings(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bi()))
				{
					processStartInfo.ArgumentList.Add(item);
				}
				using System.Diagnostics.Process process = System.Diagnostics.Process.Start(processStartInfo) ?? throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EK());
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
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aL(), delegate(Request request)
			{
				string text3 = request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bU(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au());
				try
				{
					System.Diagnostics.Process.Start(new ProcessStartInfo
					{
						FileName = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Ek(),
						Arguments = (Directory.Exists(text3) ? (8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BD() + text3 + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BD()) : (8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EL() + text3 + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BD())),
						UseShellExecute = true
					});
				}
				catch (Exception ex)
				{
					throw new CommandFailure(ex.Message);
				}
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.al(), delegate(Request request)
			{
				rpc.Stop();
				Environment.Exit((int)request.Number(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EC(), 0L));
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aM(), delegate
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
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.am(), async (Request _) => await Updater.Check().ConfigureAwait(false));
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aN(), async delegate
			{
				await Updater.Install().ConfigureAwait(false);
				return (object?)null;
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.an(), async delegate
			{
				await Updater.Install().ConfigureAwait(false);
				return (object?)null;
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aO(), delegate(Request request)
			{
				shortcuts.Register(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.El(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au()));
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ao(), delegate(Request request)
			{
				shortcuts.Unregister(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.El(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au()));
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aP(), (Request request) => shortcuts.IsRegistered(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.El(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au())));
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ap(), delegate
			{
				lock (pendingDeepLinks)
				{
					return pendingDeepLinks.ToList();
				}
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aQ(), delegate(Request request)
			{
				SingleInstance.RegisterScheme(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EM(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.g()));
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aq(), async delegate(Request request)
			{
				Dictionary<string, string> headers = request.Object<Dictionary<string, string>>(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Fv()) ?? new Dictionary<string, string>();
				return await sockets.Connect(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eW(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au()), headers).ConfigureAwait(false);
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aR(), async delegate(Request request)
			{
				JsonElement property = request.Args.GetProperty(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.FW());
				string text = property.GetProperty(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eY()).GetString() ?? 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Fg();
				string text2 = string.Empty;
				byte[] array = Array.Empty<byte>();
				if (property.TryGetProperty(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Fw(), out var jsonElement))
				{
					if (text == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.FG())
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
				await sockets.Send(request.Number(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bh(), 0L), text, array, text2).ConfigureAwait(false);
				return (object?)null;
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ar(), async delegate(Request request)
			{
				await sockets.Disconnect(request.Number(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bh(), 0L)).ConfigureAwait(false);
				return (object?)null;
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aS(), async delegate(Request request)
			{
				await rpc.Start(request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.FX(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au())).ConfigureAwait(false);
				return (object?)null;
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.@as(), delegate
			{
				rpc.Stop();
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aT(), async delegate(Request request)
			{
				ActivityPayload activity = request.Object<ActivityPayload>(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Fx()) ?? new ActivityPayload();
				await rpc.SetActivity(activity).ConfigureAwait(false);
				return (object?)null;
			});
			bridge.Register(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.at(), async delegate
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
			string text = request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aU(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au());
			string text2 = request.String(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aV(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au());
			bool flag = request.Bool(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.av());
			if (!request.Bool(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aW()))
			{
				return photinoWindow.ShowOpenFile((text.Length == 0) ? 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aX() : text, (text2.Length == 0) ? null : text2, flag)?.ToList() ?? new List<string>();
			}
			return photinoWindow.ShowOpenFolder((text.Length == 0) ? 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aw() : text, (text2.Length == 0) ? null : text2, flag)?.ToList() ?? new List<string>();
		}
	}
}
namespace Stellar.Launcher.Native
{
	public sealed class ManagedWindow
	{
		public required string Label { get; init; }

		public required PhotinoWindow Window { get; init; }

		public nint Handle => Window.WindowHandle;

		public void Post(Action action)
		{
			Window.Invoke(action);
		}
	}
	public static class AppWindows
	{
		private static readonly Dictionary<string, ManagedWindow> registry = new Dictionary<string, ManagedWindow>();

		private static Func<ManagedWindow?>? overlayFactory;

		public static ManagedWindow? Main { get; private set; }

		public static ManagedWindow? Overlay { get; private set; }

		public static void OnOverlayNeeded(Func<ManagedWindow?> factory)
		{
			overlayFactory = factory;
		}

		public static ManagedWindow? EnsureOverlay()
		{
			ManagedWindow managedWindow = Overlay;
			if (managedWindow == null)
			{
				Func<ManagedWindow?>? func = overlayFactory;
				if (func == null)
				{
					return null;
				}
				managedWindow = func();
			}
			return managedWindow;
		}

		public static void Register(string label, PhotinoWindow window)
		{
			ManagedWindow managedWindow = new ManagedWindow
			{
				Label = label,
				Window = window
			};
			lock (registry)
			{
				registry[label] = managedWindow;
			}
			if (label == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.k())
			{
				Main = managedWindow;
			}
			else if (label == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ax())
			{
				Overlay = managedWindow;
			}
		}

		public static ManagedWindow? Find(string label)
		{
			lock (registry)
			{
				ManagedWindow managedWindow;
				return registry.TryGetValue(label, out managedWindow) ? managedWindow : null;
			}
		}

		public static void Show(string label)
		{
			ManagedWindow managedWindow = Find(label);
			if (managedWindow != null)
			{
				Win32.ShowWindow(managedWindow.Handle, 5);
			}
		}

		public static void ShowWithoutActivating(string label)
		{
			ManagedWindow managedWindow = Find(label);
			if (managedWindow != null)
			{
				Win32.ShowWindow(managedWindow.Handle, 4);
			}
		}

		public static void Hide(string label)
		{
			ManagedWindow managedWindow = Find(label);
			if (managedWindow != null)
			{
				Win32.ShowWindow(managedWindow.Handle, 0);
			}
		}

		public static bool IsVisible(string label)
		{
			ManagedWindow managedWindow = Find(label);
			if (managedWindow != null)
			{
				return Win32.IsWindowVisible(managedWindow.Handle);
			}
			return false;
		}

		public static bool IsMinimized(string label)
		{
			ManagedWindow managedWindow = Find(label);
			if (managedWindow != null)
			{
				return Win32.IsIconic(managedWindow.Handle);
			}
			return false;
		}

		public static void Minimize(string label)
		{
			ManagedWindow managedWindow = Find(label);
			if (managedWindow != null)
			{
				Win32.ShowWindow(managedWindow.Handle, 6);
			}
		}

		public static void Unminimize(string label)
		{
			ManagedWindow managedWindow = Find(label);
			if (managedWindow != null)
			{
				Win32.ShowWindow(managedWindow.Handle, 9);
			}
		}

		public static void Maximize(string label)
		{
			ManagedWindow managedWindow = Find(label);
			if (managedWindow != null)
			{
				Win32.ShowWindow(managedWindow.Handle, 3);
			}
		}

		public static void Focus(string label)
		{
			ManagedWindow managedWindow = Find(label);
			if (managedWindow != null)
			{
				if (Win32.IsIconic(managedWindow.Handle))
				{
					Win32.ShowWindow(managedWindow.Handle, 9);
				}
				Win32.SetForegroundWindow(managedWindow.Handle);
				Win32.SetFocus(managedWindow.Handle);
			}
		}

		public static void SetTitle(string label, string title)
		{
			string title2 = title;
			ManagedWindow window = Find(label);
			if (window != null)
			{
				window.Post(delegate
				{
					window.Window.SetTitle(title2);
				});
			}
		}

		public static void StartDrag(string label)
		{
			ManagedWindow managedWindow = Find(label);
			if (managedWindow != null)
			{
				Win32.ReleaseCapture();
				Win32.SendMessage(managedWindow.Handle, 161, new IntPtr(2), IntPtr.Zero);
			}
		}

		public static void SetIgnoreCursorEvents(string label, bool ignore)
		{
			ManagedWindow managedWindow = Find(label);
			if (managedWindow != null)
			{
				int windowLong = Win32.GetWindowLong(managedWindow.Handle, -20);
				if (ignore)
				{
					windowLong |= 0x80020;
				}
				else
				{
					windowLong &= -33;
					windowLong |= 0x80000;
				}
				Win32.SetWindowLong(managedWindow.Handle, -20, windowLong);
				Win32.SetLayeredWindowAttributes(managedWindow.Handle, 0u, 255, 2u);
			}
		}

		public static void MakeOverlayChrome(string label)
		{
			ManagedWindow managedWindow = Find(label);
			if (managedWindow != null)
			{
				int windowLong = Win32.GetWindowLong(managedWindow.Handle, -20);
				windowLong |= 0x80800A0;
				Win32.SetWindowLong(managedWindow.Handle, -20, windowLong);
				Win32.SetLayeredWindowAttributes(managedWindow.Handle, 0u, 255, 2u);
				Win32.SetWindowPos(managedWindow.Handle, Win32.HWND_TOPMOST, 0, 0, 0, 0, 19u);
			}
		}

		public static Rectangle Bounds(string label)
		{
			ManagedWindow managedWindow = Find(label);
			if (managedWindow == null)
			{
				return Rectangle.Empty;
			}
			return new Rectangle(managedWindow.Window.Left, managedWindow.Window.Top, managedWindow.Window.Width, managedWindow.Window.Height);
		}

		public static void MoveResize(string label, int x, int y, int width, int height)
		{
			ManagedWindow window = Find(label);
			if (window != null)
			{
				window.Post(delegate
				{
					window.Window.SetSize(width, height);
					window.Window.SetLocation(new Point(x, y));
				});
			}
		}
	}
	internal static class Chrome
	{
		private delegate nint WindowProc(nint hWnd, uint message, nint wParam, nint lParam);

		private struct MARGINS
		{
			public int Left;

			public int Right;

			public int Top;

			public int Bottom;
		}

		private const int GWL_STYLE = -16;

		private const int WS_CAPTION = 12582912;

		private const int WS_THICKFRAME = 262144;

		private const int WS_SYSMENU = 524288;

		private const int WS_MINIMIZEBOX = 131072;

		private const int GWLP_WNDPROC = -4;

		private const int WM_NCCALCSIZE = 131;

		private const int WM_NCHITTEST = 132;

		private const int WM_NCACTIVATE = 134;

		private const int HTCLIENT = 1;

		private const int HTCAPTION = 2;

		private const int HTLEFT = 10;

		private const int HTRIGHT = 11;

		private const int HTTOP = 12;

		private const int HTTOPLEFT = 13;

		private const int HTTOPRIGHT = 14;

		private const int HTBOTTOM = 15;

		private const int HTBOTTOMLEFT = 16;

		private const int HTBOTTOMRIGHT = 17;

		private const uint SWP_FRAMECHANGED = 32u;

		private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;

		private const int DWMWCP_ROUND = 2;

		private const int GRIP = 6;

		private static readonly Dictionary<nint, nint> originals = new Dictionary<nint, nint>();

		private static readonly List<WindowProc> pinned = new List<WindowProc>();

		public static int Caption => 2;

		[DllImport("dwmapi.dll")]
		private static extern int DwmExtendFrameIntoClientArea(nint hWnd, ref MARGINS margins);

		[DllImport("dwmapi.dll")]
		private static extern int DwmSetWindowAttribute(nint hWnd, int attribute, ref int value, int size);

		[DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
		private static extern nint SetWindowLongPtr(nint hWnd, int index, nint value);

		[DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
		private static extern nint GetWindowLongPtr(nint hWnd, int index);

		[DllImport("user32.dll")]
		private static extern nint CallWindowProcW(nint previous, nint hWnd, uint message, nint wParam, nint lParam);

		[DllImport("user32.dll")]
		private static extern bool GetWindowRect(nint hWnd, out Win32.RECT rect);

		public static void Apply(nint handle, bool resizable)
		{
			if (handle == IntPtr.Zero)
			{
				return;
			}
			int windowLong = Win32.GetWindowLong(handle, -16);
			windowLong |= 0xCA0000;
			if (resizable)
			{
				windowLong |= 0x40000;
			}
			Win32.SetWindowLong(handle, -16, windowLong);
			int value = 2;
			DwmSetWindowAttribute(handle, 33, ref value, 4);
			MARGINS mARGINS = default(MARGINS);
			mARGINS.Left = 0;
			mARGINS.Right = 0;
			mARGINS.Top = 1;
			mARGINS.Bottom = 0;
			MARGINS margins = mARGINS;
			DwmExtendFrameIntoClientArea(handle, ref margins);
			WindowProc windowProc = (nint hWnd, uint message, nint wParam, nint lParam) => Dispatch(hWnd, message, wParam, lParam, resizable);
			lock (originals)
			{
				if (originals.ContainsKey(handle))
				{
					return;
				}
				pinned.Add(windowProc);
				nint num = SetWindowLongPtr(handle, -4, Marshal.GetFunctionPointerForDelegate(windowProc));
				originals[handle] = num;
			}
			Win32.SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, 51u);
		}

		private static nint Dispatch(nint handle, uint message, nint wParam, nint lParam, bool resizable)
		{
			nint zero;
			lock (originals)
			{
				if (!originals.TryGetValue(handle, out zero))
				{
					zero = IntPtr.Zero;
				}
			}
			switch (message)
			{
			case 131u:
				if (wParam != IntPtr.Zero)
				{
					return IntPtr.Zero;
				}
				break;
			case 134u:
				return CallWindowProcW(zero, handle, message, wParam, new IntPtr(-1));
			case 132u:
				if (resizable)
				{
					return new IntPtr(HitTest(handle, lParam));
				}
				break;
			}
			if (zero != IntPtr.Zero)
			{
				return CallWindowProcW(zero, handle, message, wParam, lParam);
			}
			return IntPtr.Zero;
		}

		private static int HitTest(nint handle, nint lParam)
		{
			if (!GetWindowRect(handle, out var rect))
			{
				return 1;
			}
			short num = (short)(((IntPtr)lParam).ToInt64() & 0xFFFF);
			short num2 = (short)((((IntPtr)lParam).ToInt64() >> 16) & 0xFFFF);
			bool flag = num < rect.Left + 6;
			bool flag2 = num >= rect.Right - 6;
			bool flag3 = num2 < rect.Top + 6;
			bool flag4 = num2 >= rect.Bottom - 6;
			if (flag3 && flag)
			{
				return 13;
			}
			if (flag3 && flag2)
			{
				return 14;
			}
			if (flag4 && flag)
			{
				return 16;
			}
			if (flag4 && flag2)
			{
				return 17;
			}
			if (flag)
			{
				return 10;
			}
			if (flag2)
			{
				return 11;
			}
			if (flag3)
			{
				return 12;
			}
			if (flag4)
			{
				return 15;
			}
			return 1;
		}
	}
	public static class LegacyShortcuts
	{
		private const string OLD_EXE = "stellar.exe";

		public static void Clean()
		{
			foreach (string item in Candidates())
			{
				try
				{
					File.Delete(item);
				}
				catch
				{
				}
			}
		}

		public static List<string> Candidates()
		{
			return Candidates(Environment.ProcessPath);
		}

		public static List<string> Candidates(string? current)
		{
			List<string> list = new List<string>();
			try
			{
				if (string.IsNullOrEmpty(current))
				{
					return list;
				}
				foreach (string item in new string[4]
				{
					Environment.GetFolderPath(Environment.SpecialFolder.Programs),
					Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms),
					Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
					Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory)
				}.Distinct())
				{
					if (string.IsNullOrEmpty(item) || !Directory.Exists(item))
					{
						continue;
					}
					IEnumerable<string> enumerable;
					try
					{
						enumerable = Directory.EnumerateFiles(item, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aY(), SearchOption.AllDirectories);
					}
					catch
					{
						continue;
					}
					foreach (string item2 in enumerable)
					{
						string text = ResolveTarget(item2);
						if (text != null && Path.GetFileName(text).Equals(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ay(), StringComparison.OrdinalIgnoreCase) && !text.Equals(current, StringComparison.OrdinalIgnoreCase))
						{
							list.Add(item2);
						}
					}
				}
			}
			catch
			{
			}
			return list;
		}

		private static string? ResolveTarget(string link)
		{
			try
			{
				byte[] array = File.ReadAllBytes(link);
				if (array.Length < 76 || BitConverter.ToUInt32(array, 0) != 76)
				{
					return null;
				}
				uint num = BitConverter.ToUInt32(array, 20);
				bool unicode = (num & 0x80) != 0;
				int num2 = 76;
				if ((num & (true ? 1u : 0u)) != 0)
				{
					if (num2 + 2 > array.Length)
					{
						return null;
					}
					num2 += 2 + BitConverter.ToUInt16(array, num2);
				}
				if ((num & 2u) != 0 && num2 + 28 <= array.Length)
				{
					string text = FromLinkInfo(array, num2);
					if (!string.IsNullOrEmpty(text))
					{
						return text;
					}
					num2 += (int)BitConverter.ToUInt32(array, num2);
				}
				if ((num & 4u) != 0)
				{
					num2 = SkipString(array, num2, unicode);
				}
				if ((num & 8u) != 0)
				{
					string text2 = ReadString(array, num2, unicode);
					if (!string.IsNullOrEmpty(text2))
					{
						string directoryName = Path.GetDirectoryName(link);
						return (directoryName == null) ? null : Path.GetFullPath(Path.Combine(directoryName, text2));
					}
				}
				return null;
			}
			catch
			{
				return null;
			}
		}

		private static string? FromLinkInfo(byte[] bytes, int position)
		{
			uint num = BitConverter.ToUInt32(bytes, position + 4);
			if ((BitConverter.ToUInt32(bytes, position + 8) & 1) == 0)
			{
				return null;
			}
			string text = null;
			if (num >= 36 && position + 36 <= bytes.Length)
			{
				uint num2 = BitConverter.ToUInt32(bytes, position + 28);
				if (num2 != 0)
				{
					text = ReadAnsiZ(bytes, position + (int)num2, unicode: true);
				}
			}
			if (text == null)
			{
				text = ReadAnsiZ(bytes, position + (int)BitConverter.ToUInt32(bytes, position + 16), unicode: false);
			}
			uint num3 = ((num >= 36 && position + 36 <= bytes.Length) ? BitConverter.ToUInt32(bytes, position + 32) : 0u);
			string text2 = ((num3 != 0) ? ReadAnsiZ(bytes, position + (int)num3, unicode: true) : ReadAnsiZ(bytes, position + (int)BitConverter.ToUInt32(bytes, position + 24), unicode: false));
			string text3 = text + text2;
			if (!string.IsNullOrEmpty(text3))
			{
				return text3;
			}
			return null;
		}

		private static int SkipString(byte[] bytes, int position, bool unicode)
		{
			ushort num = BitConverter.ToUInt16(bytes, position);
			return position + 2 + (unicode ? (num * 2) : num);
		}

		private static string ReadString(byte[] bytes, int position, bool unicode)
		{
			ushort num = BitConverter.ToUInt16(bytes, position);
			position += 2;
			if (!unicode)
			{
				return Encoding.Latin1.GetString(bytes, position, num);
			}
			return Encoding.Unicode.GetString(bytes, position, num * 2);
		}

		private static string ReadAnsiZ(byte[] data, int offset, bool unicode)
		{
			int i = offset;
			if (unicode)
			{
				for (; i + 1 < data.Length && (data[i] != 0 || data[i + 1] != 0); i += 2)
				{
				}
				return Encoding.Unicode.GetString(data, offset, i - offset);
			}
			for (; i < data.Length && data[i] != 0; i++)
			{
			}
			return Encoding.Latin1.GetString(data, offset, i - offset);
		}
	}
	public static class Minisign
	{
		private sealed record ParsedKey(byte[] KeyId, byte[] PublicKey)
		{
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EN());
				stringBuilder.Append(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.En());
				if (PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				RuntimeHelpers.EnsureSufficientExecutionStack();
				builder.Append(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EO());
				builder.Append(KeyId);
				builder.Append(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Eo());
				builder.Append(PublicKey);
				return true;
			}
		}

		private const string PUBLIC_KEY = "dW50cnVzdGVkIGNvbW1lbnQ6IG1pbmlzaWduIHB1YmxpYyBrZXk6IDUyMTRGQzI0NTJFNEJCODYKUldTR3UrUlNKUHdVVXJvckw1VWs2bWFLK0k0ZWJSdXZUcWZYck5NdXg1VXdaR0ZRMXFqek4vdEwK";

		private static string[] SplitLines(string text)
		{
			return text.Replace(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aZ(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.az()).Split('\n');
		}

		private static ParsedKey ParsePublicKey(string encoded)
		{
			string[] array = SplitLines(Encoding.UTF8.GetString(Convert.FromBase64String(encoded)));
			if (array.Length < 2)
			{
				throw new FormatException(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BA());
			}
			byte[] array2 = Convert.FromBase64String(array[1].Trim());
			if (array2.Length != 42)
			{
				throw new FormatException(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Ba());
			}
			return new ParsedKey(array2[2..10], array2[10..42]);
		}

		public static bool Verify(byte[] artifact, string signatureField)
		{
			return Verify(artifact, signatureField, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BB());
		}

		internal static bool Verify(byte[] artifact, string signatureField, string publicKey)
		{
			try
			{
				ParsedKey parsedKey = ParsePublicKey(publicKey);
				string[] array = SplitLines(Encoding.UTF8.GetString(Convert.FromBase64String(signatureField.Trim())));
				if (array.Length < 4)
				{
					return false;
				}
				byte[] array2 = Convert.FromBase64String(array[1].Trim());
				if (array2.Length != 74)
				{
					return false;
				}
				string @string = Encoding.ASCII.GetString(array2, 0, 2);
				byte[] subArray = array2[2..10];
				byte[] subArray2 = array2[10..74];
				if (!subArray.AsSpan().SequenceEqual(parsedKey.KeyId))
				{
					return false;
				}
				byte[] array3;
				if (@string == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Bb())
				{
					Blake2bDigest blake2bDigest = new Blake2bDigest(512);
					blake2bDigest.BlockUpdate(artifact, 0, artifact.Length);
					array3 = new byte[64];
					blake2bDigest.DoFinal(array3, 0);
				}
				else
				{
					if (!(@string == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BC()))
					{
						return false;
					}
					array3 = artifact;
				}
				Ed25519PublicKeyParameters ed25519PublicKeyParameters = new Ed25519PublicKeyParameters(parsedKey.PublicKey, 0);
				Ed25519Signer ed25519Signer = new Ed25519Signer();
				ed25519Signer.Init(false, ed25519PublicKeyParameters);
				ed25519Signer.BlockUpdate(array3, 0, array3.Length);
				if (!ed25519Signer.VerifySignature(subArray2))
				{
					return false;
				}
				if (!array[2].StartsWith(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Bc(), StringComparison.Ordinal))
				{
					return false;
				}
				byte[] bytes = Encoding.UTF8.GetBytes(array[2].Substring(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Bc().Length));
				byte[] array4 = Convert.FromBase64String(array[3].Trim());
				byte[] array5 = new byte[subArray2.Length + bytes.Length];
				Buffer.BlockCopy(subArray2, 0, array5, 0, subArray2.Length);
				Buffer.BlockCopy(bytes, 0, array5, subArray2.Length, bytes.Length);
				Ed25519Signer ed25519Signer2 = new Ed25519Signer();
				ed25519Signer2.Init(false, ed25519PublicKeyParameters);
				ed25519Signer2.BlockUpdate(array5, 0, array5.Length);
				return ed25519Signer2.VerifySignature(array4);
			}
			catch
			{
				return false;
			}
		}
	}
	internal sealed class Process
	{
		public nint Handle { get; init; }

		public nint Thread { get; init; }

		public uint Id { get; init; }

		public void Kill()
		{
			if (Handle != IntPtr.Zero)
			{
				Win32.TerminateProcess(Handle, 1u);
			}
		}

		public void Release()
		{
			if (Thread != IntPtr.Zero)
			{
				Win32.CloseHandle(Thread);
			}
			if (Handle != IntPtr.Zero)
			{
				Win32.CloseHandle(Handle);
			}
		}
	}
	internal static class ProcessLauncher
	{
		[StructLayout(0, CharSet = CharSet.Unicode)]
		private struct STARTUPINFO
		{
			public int cb;

			public nint lpReserved;

			public nint lpDesktop;

			public nint lpTitle;

			public int dwX;

			public int dwY;

			public int dwXSize;

			public int dwYSize;

			public int dwXCountChars;

			public int dwYCountChars;

			public int dwFillAttribute;

			public uint dwFlags;

			public short wShowWindow;

			public short cbReserved2;

			public nint lpReserved2;

			public nint hStdInput;

			public nint hStdOutput;

			public nint hStdError;
		}

		private struct PROCESS_INFORMATION
		{
			public nint hProcess;

			public nint hThread;

			public uint dwProcessId;

			public uint dwThreadId;
		}

		private const uint STARTF_USESTDHANDLES = 256u;

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern bool CreateProcessW(string? lpApplicationName, StringBuilder lpCommandLine, nint lpProcessAttributes, nint lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, nint lpEnvironment, string? lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

		public static string Quote(string value)
		{
			if (value.Length > 0 && value.IndexOfAny(new char[3] { ' ', '\t', '"' }) < 0)
			{
				return value;
			}
			StringBuilder stringBuilder = new StringBuilder(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BD());
			int num = 0;
			foreach (char c in value)
			{
				switch (c)
				{
				case '\\':
					num++;
					break;
				case '"':
					stringBuilder.Append('\\', num * 2 + 1);
					num = 0;
					stringBuilder.Append('"');
					break;
				default:
					stringBuilder.Append('\\', num);
					num = 0;
					stringBuilder.Append(c);
					break;
				}
			}
			stringBuilder.Append('\\', num * 2);
			stringBuilder.Append('"');
			return stringBuilder.ToString();
		}

		public static Process Spawn(string executable, IEnumerable<string> arguments, string workingDirectory, uint flags)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(Quote(executable));
			foreach (string argument in arguments)
			{
				if (argument.Length != 0)
				{
					stringBuilder.Append(' ').Append(Quote(argument));
				}
			}
			STARTUPINFO sTARTUPINFO = default(STARTUPINFO);
			sTARTUPINFO.cb = Marshal.SizeOf<STARTUPINFO>();
			sTARTUPINFO.dwFlags = 256u;
			sTARTUPINFO.hStdInput = IntPtr.Zero;
			sTARTUPINFO.hStdOutput = IntPtr.Zero;
			sTARTUPINFO.hStdError = IntPtr.Zero;
			STARTUPINFO lpStartupInfo = sTARTUPINFO;
			if (!CreateProcessW(executable, stringBuilder, IntPtr.Zero, IntPtr.Zero, bInheritHandles: false, flags, IntPtr.Zero, workingDirectory, ref lpStartupInfo, out var lpProcessInformation))
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
			return new Process
			{
				Handle = lpProcessInformation.hProcess,
				Thread = lpProcessInformation.hThread,
				Id = lpProcessInformation.dwProcessId
			};
		}

		public static List<uint> FindByName(params string[] names)
		{
			List<uint> list = new List<uint>();
			nint num = Win32.CreateToolhelp32Snapshot(2u, 0u);
			if (num == IntPtr.Zero || num == new IntPtr(-1))
			{
				return list;
			}
			Win32.PROCESSENTRY32 pROCESSENTRY = default(Win32.PROCESSENTRY32);
			pROCESSENTRY.dwSize = (uint)Marshal.SizeOf<Win32.PROCESSENTRY32>();
			pROCESSENTRY.szExeFile = string.Empty;
			Win32.PROCESSENTRY32 lppe = pROCESSENTRY;
			if (Win32.Process32FirstW(num, ref lppe))
			{
				do
				{
					foreach (string text in names)
					{
						if (string.Equals(lppe.szExeFile, text, StringComparison.OrdinalIgnoreCase))
						{
							list.Add(lppe.th32ProcessID);
							break;
						}
					}
				}
				while (Win32.Process32NextW(num, ref lppe));
			}
			Win32.CloseHandle(num);
			return list;
		}

		public static void KillPid(uint pid)
		{
			nint num = Win32.OpenProcess(1u, bInheritHandle: false, pid);
			if (num != IntPtr.Zero)
			{
				Win32.TerminateProcess(num, 1u);
				Win32.CloseHandle(num);
			}
		}
	}
	public sealed class Shortcuts
	{
		private const uint WM_HOTKEY = 786u;

		private const uint WM_APP_REGISTER = 32768u;

		private const uint WM_APP_UNREGISTER = 32769u;

		private const uint MOD_ALT = 1u;

		private const uint MOD_CONTROL = 2u;

		private const uint MOD_SHIFT = 4u;

		private const uint MOD_WIN = 8u;

		private const uint MOD_NOREPEAT = 16384u;

		private readonly Bridge bridge;

		private readonly object gate = new object();

		private readonly Dictionary<string, int> registered = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

		private readonly Dictionary<int, string> byId = new Dictionary<int, string>();

		private readonly Queue<(uint Message, string Shortcut)> queue = new Queue<(uint, string)>();

		private uint threadId;

		private int nextId = 1;

		public Shortcuts(Bridge bridge)
		{
			this.bridge = bridge;
			Thread thread = new Thread(Pump);
			thread.IsBackground = true;
			thread.Name = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Bd();
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
		}

		private void Pump()
		{
			threadId = Win32.GetCurrentThreadId();
			Win32.MSG lpMsg;
			while (Win32.GetMessage(out lpMsg, IntPtr.Zero, 0u, 0u) > 0)
			{
				(uint, string) tuple;
				if (lpMsg.message == 786)
				{
					string text;
					lock (gate)
					{
						byId.TryGetValue(((IntPtr)lpMsg.wParam).ToInt32(), out text);
					}
					if (text != null)
					{
						bridge.Emit(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BE() + text, new
						{
							shortcut = text,
							id = 0,
							state = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Be()
						});
					}
				}
				else
				{
					if (lpMsg.message != 32768 && lpMsg.message != 32769)
					{
						continue;
					}
					lock (gate)
					{
						if (queue.Count == 0)
						{
							continue;
						}
						tuple = queue.Dequeue();
						goto IL_00e6;
					}
				}
				continue;
				IL_00e6:
				if (tuple.Item1 == 32768)
				{
					Bind(tuple.Item2);
				}
				else
				{
					Unbind(tuple.Item2);
				}
			}
		}

		private static bool Parse(string shortcut, out uint modifiers, out uint key)
		{
			modifiers = 16384u;
			key = 0u;
			string[] array = shortcut.Split('+', StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i].Trim();
				string text2 = text.ToLowerInvariant();
				if (text2 != null)
				{
					switch (text2.Length)
					{
					case 4:
					{
						char c = text2[0];
						if (c != 'c')
						{
							if (c != 'm' || !(text2 == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Bf()))
							{
								break;
							}
							goto IL_01bb;
						}
						if (!(text2 == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BF()))
						{
							break;
						}
						goto IL_01a3;
					}
					case 7:
					{
						char c = text2[2];
						if (c != 'm')
						{
							if (c != 'n' || !(text2 == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BG()))
							{
								break;
							}
							goto IL_01a3;
						}
						if (!(text2 == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Bg()))
						{
							break;
						}
						goto IL_01bb;
					}
					case 3:
					{
						char c = text2[0];
						if (c != 'a')
						{
							if (c != 'w' || !(text2 == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Bi()))
							{
								break;
							}
							goto IL_01bb;
						}
						if (!(text2 == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BI()))
						{
							break;
						}
						goto IL_01ab;
					}
					case 5:
					{
						char c = text2[1];
						if (c != 'h')
						{
							if (c != 'u' || !(text2 == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BK()))
							{
								break;
							}
							goto IL_01bb;
						}
						if (!(text2 == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Bj()))
						{
							break;
						}
						modifiers |= 4u;
						continue;
					}
					case 16:
						if (!(text2 == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BH()))
						{
							break;
						}
						goto IL_01a3;
					case 9:
						if (!(text2 == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Bh()))
						{
							break;
						}
						goto IL_01a3;
					case 6:
						{
							if (!(text2 == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BJ()))
							{
								break;
							}
							goto IL_01ab;
						}
						IL_01bb:
						modifiers |= 8u;
						continue;
						IL_01ab:
						modifiers |= 1u;
						continue;
						IL_01a3:
						modifiers |= 2u;
						continue;
					}
				}
				if (!Code(text, out key))
				{
					return false;
				}
			}
			return key != 0;
		}

		private static bool Code(string part, out uint key)
		{
			key = 0u;
			if (part.Length == 1)
			{
				char c = char.ToUpperInvariant(part[0]);
				if ((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
				{
					key = c;
					return true;
				}
			}
			if (part.StartsWith(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Bk(), StringComparison.OrdinalIgnoreCase) && part.Length == 4)
			{
				key = char.ToUpperInvariant(part[3]);
				return true;
			}
			if (part.StartsWith(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BL(), StringComparison.OrdinalIgnoreCase) && part.Length == 6)
			{
				key = part[5];
				return true;
			}
			if (part.StartsWith(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Bl(), StringComparison.OrdinalIgnoreCase) && part.Length >= 2 && int.TryParse(part.Substring(1), out var num) && num >= 1 && num <= 24)
			{
				key = (uint)(112 + num - 1);
				return true;
			}
			string text = part.ToLowerInvariant();
			uint num2;
			if (text != null)
			{
				switch (text.Length)
				{
				case 5:
					break;
				case 6:
					goto IL_0131;
				case 3:
					goto IL_0177;
				case 9:
					goto IL_01a0;
				case 4:
					goto IL_01c9;
				case 8:
					goto IL_01f2;
				case 7:
					goto IL_0212;
				case 10:
					goto IL_0232;
				case 2:
					goto IL_03f6;
				case 11:
					goto IL_045f;
				default:
					goto IL_04d7;
				}
				switch (text[0])
				{
				case 's':
					break;
				case 'e':
					goto IL_0267;
				case 'r':
					goto IL_027c;
				case 'p':
					goto IL_0291;
				default:
					goto IL_04d7;
				}
				if (text == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BM())
				{
					num2 = 32u;
					goto IL_04d9;
				}
			}
			goto IL_04d7;
			IL_04a9:
			num2 = 40u;
			goto IL_04d9;
			IL_01c9:
			char c2 = text[0];
			if (c2 != 'd')
			{
				if (c2 != 'h')
				{
					if (c2 == 'l' && text == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Bu())
					{
						goto IL_04ae;
					}
				}
				else if (text == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Bt())
				{
					num2 = 36u;
					goto IL_04d9;
				}
			}
			else if (text == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BU())
			{
				goto IL_04a9;
			}
			goto IL_04d7;
			IL_0212:
			c2 = text[0];
			if (c2 != 'a')
			{
				if (c2 == 'n' && text == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BX())
				{
					num2 = 144u;
					goto IL_04d9;
				}
			}
			else if (text == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Bw())
			{
				goto IL_04a4;
			}
			goto IL_04d7;
			IL_045f:
			if (!(text == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.By()))
			{
				goto IL_04d7;
			}
			num2 = 44u;
			goto IL_04d9;
			IL_04b3:
			num2 = 39u;
			goto IL_04d9;
			IL_01f2:
			c2 = text[0];
			if (c2 != 'c')
			{
				if (c2 != 'p' || !(text == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BV()))
				{
					goto IL_04d7;
				}
				num2 = 34u;
			}
			else
			{
				if (!(text == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Bv()))
				{
					goto IL_04d7;
				}
				num2 = 20u;
			}
			goto IL_04d9;
			IL_04d9:
			key = num2;
			return key != 0;
			IL_047d:
			num2 = 27u;
			goto IL_04d9;
			IL_04a4:
			num2 = 38u;
			goto IL_04d9;
			IL_0291:
			if (!(text == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Bn()))
			{
				goto IL_04d7;
			}
			num2 = 19u;
			goto IL_04d9;
			IL_04ae:
			num2 = 37u;
			goto IL_04d9;
			IL_027c:
			if (text == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BN())
			{
				goto IL_04b3;
			}
			goto IL_04d7;
			IL_04d7:
			num2 = 0u;
			goto IL_04d9;
			IL_0267:
			if (text == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Bm())
			{
				goto IL_0473;
			}
			goto IL_04d7;
			IL_0131:
			c2 = text[0];
			if ((uint)c2 <= 101u)
			{
				if (c2 != 'd')
				{
					if (c2 == 'e' && text == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Bo())
					{
						goto IL_047d;
					}
				}
				else if (text == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BP())
				{
					num2 = 46u;
					goto IL_04d9;
				}
			}
			else if (c2 != 'i')
			{
				if (c2 != 'p')
				{
					if (c2 == 'r' && text == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BO())
					{
						goto IL_0473;
					}
				}
				else if (text == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BQ())
				{
					num2 = 33u;
					goto IL_04d9;
				}
			}
			else if (text == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Bp())
			{
				num2 = 45u;
				goto IL_04d9;
			}
			goto IL_04d7;
			IL_03f6:
			if (text == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BW())
			{
				goto IL_04a4;
			}
			goto IL_04d7;
			IL_0177:
			c2 = text[1];
			if (c2 != 'a')
			{
				if (c2 != 'n')
				{
					if (c2 == 's' && text == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BR())
					{
						goto IL_047d;
					}
				}
				else if (text == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Br())
				{
					num2 = 35u;
					goto IL_04d9;
				}
			}
			else if (text == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Bq())
			{
				num2 = 9u;
				goto IL_04d9;
			}
			goto IL_04d7;
			IL_01a0:
			c2 = text[5];
			if (c2 != 'd')
			{
				if (c2 != 'l')
				{
					if (c2 == 'p' && text == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BS())
					{
						num2 = 8u;
						goto IL_04d9;
					}
				}
				else if (text == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BT())
				{
					goto IL_04ae;
				}
			}
			else if (text == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Bs())
			{
				goto IL_04a9;
			}
			goto IL_04d7;
			IL_0232:
			c2 = text[0];
			if (c2 != 'a')
			{
				if (c2 == 's' && text == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BY())
				{
					num2 = 145u;
					goto IL_04d9;
				}
			}
			else if (text == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Bx())
			{
				goto IL_04b3;
			}
			goto IL_04d7;
			IL_0473:
			num2 = 13u;
			goto IL_04d9;
		}

		private void Bind(string shortcut)
		{
			if (!Parse(shortcut, out var modifiers, out var key))
			{
				return;
			}
			int num;
			lock (gate)
			{
				if (registered.ContainsKey(shortcut))
				{
					return;
				}
				num = nextId++;
			}
			if (!Win32.RegisterHotKey(IntPtr.Zero, num, modifiers, key))
			{
				return;
			}
			lock (gate)
			{
				registered[shortcut] = num;
				byId[num] = shortcut;
			}
		}

		private void Unbind(string shortcut)
		{
			int num;
			lock (gate)
			{
				if (!registered.TryGetValue(shortcut, out num))
				{
					return;
				}
				registered.Remove(shortcut);
				byId.Remove(num);
			}
			Win32.UnregisterHotKey(IntPtr.Zero, num);
		}

		private void Post(uint message, string shortcut)
		{
			lock (gate)
			{
				queue.Enqueue((message, shortcut));
			}
			uint num = threadId;
			if (num != 0)
			{
				Win32.PostThreadMessage(num, message, IntPtr.Zero, IntPtr.Zero);
			}
		}

		public void Register(string shortcut)
		{
			if (!Parse(shortcut, out var _, out var _))
			{
				throw new CommandFailure(shortcut + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BZ());
			}
			Post(32768u, shortcut);
		}

		public void Unregister(string shortcut)
		{
			Post(32769u, shortcut);
		}

		public bool IsRegistered(string shortcut)
		{
			lock (gate)
			{
				return registered.ContainsKey(shortcut);
			}
		}
	}
	public static class SingleInstance
	{
		private const string PIPE_NAME = "stellar-launcher-single-instance";

		private const string MUTEX_NAME = "Global\\dev.stellar.launcher";

		public const string SCHEME = "stellarv2";

		private static Mutex? guard;

		public static bool Claim()
		{
			guard = new Mutex(true, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Bz(), out var result);
			return result;
		}

		public static void Forward(string[] args)
		{
			try
			{
				using NamedPipeClientStream namedPipeClientStream = new NamedPipeClientStream(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bA(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ba(), PipeDirection.Out);
				namedPipeClientStream.Connect(2000);
				byte[] bytes = Encoding.UTF8.GetBytes(string.Join('\n', args));
				namedPipeClientStream.Write(bytes);
				namedPipeClientStream.Flush();
			}
			catch
			{
			}
		}

		public static void Listen(Action<string[]> handler)
		{
			Action<string[]> handler2 = handler;
			Thread thread = new Thread((ThreadStart)delegate
			{
				while (true)
				{
					try
					{
						using NamedPipeServerStream namedPipeServerStream = new NamedPipeServerStream(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ba(), PipeDirection.In, 1, PipeTransmissionMode.Byte);
						namedPipeServerStream.WaitForConnection();
						using StreamReader streamReader = new StreamReader(namedPipeServerStream, Encoding.UTF8);
						string[] array = streamReader.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries);
						if (array.Length != 0)
						{
							handler2(array);
						}
					}
					catch
					{
						Thread.Sleep(250);
					}
				}
			});
			thread.IsBackground = true;
			thread.Name = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bB();
			thread.Start();
		}

		public static void RegisterScheme(string scheme)
		{
			string processPath = Environment.ProcessPath;
			if (string.IsNullOrEmpty(processPath))
			{
				return;
			}
			try
			{
				using RegistryKey registryKey = Registry.CurrentUser.CreateSubKey(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bb() + scheme);
				registryKey.SetValue(string.Empty, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bC() + scheme);
				registryKey.SetValue(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bc(), string.Empty);
				using RegistryKey registryKey2 = registryKey.CreateSubKey(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bD());
				registryKey2.SetValue(string.Empty, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BD() + processPath + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bd());
				using RegistryKey registryKey3 = registryKey.CreateSubKey(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bE());
				registryKey3.SetValue(string.Empty, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BD() + processPath + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.be());
			}
			catch
			{
			}
		}

		public static List<string> DeepLinksFrom(string[] args)
		{
			return args.Where((string argument) => argument.StartsWith(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EP(), StringComparison.OrdinalIgnoreCase)).ToList();
		}
	}
	internal static class Win32
	{
		public struct MSG
		{
			public nint hwnd;

			public uint message;

			public nint wParam;

			public nint lParam;

			public uint time;

			public int ptX;

			public int ptY;
		}

		public struct RECT
		{
			public int Left;

			public int Top;

			public int Right;

			public int Bottom;
		}

		public struct MONITORINFO
		{
			public int cbSize;

			public RECT rcMonitor;

			public RECT rcWork;

			public uint dwFlags;
		}

		[StructLayout(0, CharSet = CharSet.Unicode)]
		public struct PROCESSENTRY32
		{
			public uint dwSize;

			public uint cntUsage;

			public uint th32ProcessID;

			public nint th32DefaultHeapID;

			public uint th32ModuleID;

			public uint cntThreads;

			public uint th32ParentProcessID;

			public int pcPriClassBase;

			public uint dwFlags;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szExeFile;
		}

		public const int SW_HIDE = 0;

		public const int SW_SHOWNOACTIVATE = 4;

		public const int SW_SHOW = 5;

		public const int SW_MINIMIZE = 6;

		public const int SW_RESTORE = 9;

		public const int SW_MAXIMIZE = 3;

		public const int GWL_EXSTYLE = -20;

		public const int WS_EX_LAYERED = 524288;

		public const int WS_EX_TRANSPARENT = 32;

		public const int WS_EX_TOOLWINDOW = 128;

		public const int WS_EX_NOACTIVATE = 134217728;

		public const uint SWP_NOSIZE = 1u;

		public const uint SWP_NOMOVE = 2u;

		public const uint SWP_NOACTIVATE = 16u;

		public const uint SWP_SHOWWINDOW = 64u;

		public const int WM_NCLBUTTONDOWN = 161;

		public const int HTCAPTION = 2;

		public const uint LWA_ALPHA = 2u;

		public const uint PROCESS_TERMINATE = 1u;

		public const uint TH32CS_SNAPPROCESS = 2u;

		public const uint CREATE_NO_WINDOW = 134217728u;

		public const uint CREATE_SUSPENDED = 4u;

		public static readonly nint HWND_TOPMOST = new IntPtr(-1);

		[DllImport("user32.dll")]
		public static extern bool ShowWindow(nint hWnd, int nCmdShow);

		[DllImport("user32.dll")]
		public static extern bool IsWindowVisible(nint hWnd);

		[DllImport("user32.dll")]
		public static extern bool IsIconic(nint hWnd);

		[DllImport("user32.dll")]
		public static extern bool SetForegroundWindow(nint hWnd);

		[DllImport("user32.dll")]
		public static extern nint SetFocus(nint hWnd);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern int GetWindowLong(nint hWnd, int nIndex);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern int SetWindowLong(nint hWnd, int nIndex, int dwNewLong);

		[DllImport("user32.dll")]
		public static extern bool SetLayeredWindowAttributes(nint hWnd, uint crKey, byte bAlpha, uint dwFlags);

		[DllImport("user32.dll")]
		public static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

		[DllImport("user32.dll")]
		public static extern bool ReleaseCapture();

		[DllImport("user32.dll")]
		public static extern nint SendMessage(nint hWnd, int msg, nint wParam, nint lParam);

		[DllImport("user32.dll")]
		public static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

		[DllImport("user32.dll")]
		public static extern bool UnregisterHotKey(nint hWnd, int id);

		[DllImport("user32.dll")]
		public static extern int GetMessage(out MSG lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

		[DllImport("user32.dll")]
		public static extern bool PostThreadMessage(uint threadId, uint msg, nint wParam, nint lParam);

		[DllImport("kernel32.dll")]
		public static extern uint GetCurrentThreadId();

		[DllImport("user32.dll")]
		public static extern nint MonitorFromWindow(nint hWnd, uint dwFlags);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern bool GetMonitorInfo(nint hMonitor, ref MONITORINFO lpmi);

		[DllImport("shcore.dll")]
		public static extern int GetDpiForMonitor(nint hMonitor, int dpiType, out uint dpiX, out uint dpiY);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern nint CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		public static extern bool Process32FirstW(nint hSnapshot, ref PROCESSENTRY32 lppe);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		public static extern bool Process32NextW(nint hSnapshot, ref PROCESSENTRY32 lppe);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool CloseHandle(nint hObject);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern nint OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool TerminateProcess(nint hProcess, uint uExitCode);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern bool GetDiskFreeSpaceExW(string lpDirectoryName, out ulong lpFreeBytesAvailableToCaller, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);
	}
}
namespace Stellar.Launcher.Ipc
{
	public sealed class Request
	{
		public required string Label { get; init; }

		public required JsonElement Args { get; init; }

		private bool Lookup(string name, out JsonElement value)
		{
			value = default(JsonElement);
			if (Args.ValueKind != JsonValueKind.Object)
			{
				return false;
			}
			if (Args.TryGetProperty(name, out value))
			{
				return value.ValueKind != JsonValueKind.Null;
			}
			return false;
		}

		public string String(string name, string fallback = "")
		{
			if (!Lookup(name, out var value) || value.ValueKind != JsonValueKind.String)
			{
				return fallback;
			}
			return value.GetString() ?? fallback;
		}

		public string? OptionalString(string name)
		{
			if (!Lookup(name, out var value) || value.ValueKind != JsonValueKind.String)
			{
				return null;
			}
			return value.GetString();
		}

		public bool Bool(string name, bool fallback = false)
		{
			if (!Lookup(name, out var value))
			{
				return fallback;
			}
			return value.ValueKind switch
			{
				JsonValueKind.True => true, 
				JsonValueKind.False => false, 
				_ => fallback, 
			};
		}

		public long Number(string name, long fallback = 0L)
		{
			if (!Lookup(name, out var value) || value.ValueKind != JsonValueKind.Number)
			{
				return fallback;
			}
			return value.GetInt64();
		}

		public long? OptionalNumber(string name)
		{
			if (!Lookup(name, out var value) || value.ValueKind != JsonValueKind.Number)
			{
				return null;
			}
			return value.GetInt64();
		}

		public double Double(string name, double fallback = 0.0)
		{
			if (!Lookup(name, out var value) || value.ValueKind != JsonValueKind.Number)
			{
				return fallback;
			}
			return value.GetDouble();
		}

		public List<string> Strings(string name)
		{
			List<string> list = new List<string>();
			if (!Lookup(name, out var value) || value.ValueKind != JsonValueKind.Array)
			{
				return list;
			}
			foreach (JsonElement item in value.EnumerateArray())
			{
				if (item.ValueKind == JsonValueKind.String)
				{
					list.Add(item.GetString() ?? string.Empty);
				}
			}
			return list;
		}

		public byte[] Bytes(string name)
		{
			if (!Lookup(name, out var value) || value.ValueKind != JsonValueKind.Array)
			{
				return Array.Empty<byte>();
			}
			byte[] array = new byte[value.GetArrayLength()];
			int num = 0;
			foreach (JsonElement item in value.EnumerateArray())
			{
				array[num++] = (byte)item.GetInt32();
			}
			return array;
		}

		public T? Object<T>(string name)
		{
			if (!Lookup(name, out var value))
			{
				return default(T);
			}
			return value.Deserialize<T>(Bridge.Options);
		}

		public T? Payload<T>()
		{
			return Args.Deserialize<T>(Bridge.Options);
		}
	}
	public sealed class CommandFailure : Exception
	{
		public CommandFailure(string message)
			: base(message)
		{
		}
	}
	public sealed class Bridge
	{
		public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			DefaultIgnoreCondition = JsonIgnoreCondition.Never,
			Converters = { (JsonConverter)new ByteArrayConverter() }
		};

		private readonly ConcurrentDictionary<string, Func<Request, Task<object?>>> handlers = new ConcurrentDictionary<string, Func<Request, Task<object>>>();

		private readonly List<(string Label, PhotinoWindow Window)> windows = new List<(string, PhotinoWindow)>();

		private long eventSequence;

		public void Register(string name, Func<Request, Task<object?>> handler)
		{
			handlers[name] = handler;
		}

		public void Register(string name, Func<Request, object?> handler)
		{
			Func<Request, object?> handler2 = handler;
			handlers[name] = (Request request) => Task.FromResult(handler2(request));
		}

		public void Register(string name, Action<Request> handler)
		{
			Action<Request> handler2 = handler;
			handlers[name] = delegate(Request request)
			{
				handler2(request);
				return Task.FromResult<object>(null);
			};
		}

		public void Attach(string label, PhotinoWindow window)
		{
			string label2 = label;
			lock (windows)
			{
				windows.Add((label2, window));
			}
			window.RegisterWebMessageReceivedHandler(delegate(object? _, string message)
			{
				Receive(label2, message);
			});
		}

		public void Emit(string name, object? payload = null)
		{
			long id = Interlocked.Increment(ref eventSequence);
			string encoded = JsonSerializer.Serialize(new
			{
				t = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bF(),
				id = id,
				@event = name,
				payload = payload
			}, Options);
			Send(encoded);
		}

		public void EmitTo(string label, string name, object? payload = null)
		{
			long id = Interlocked.Increment(ref eventSequence);
			string encoded = JsonSerializer.Serialize(new
			{
				t = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bF(),
				id = id,
				@event = name,
				payload = payload
			}, Options);
			Send(encoded, label);
		}

		private void Send(string encoded, string? only = null)
		{
			List<(string, PhotinoWindow)> list;
			lock (windows)
			{
				list = windows.ToList();
			}
			foreach (var (text, photinoWindow) in list)
			{
				if (only == null || !(text != only))
				{
					try
					{
						photinoWindow.SendWebMessage(encoded);
					}
					catch
					{
					}
				}
			}
		}

		private void Receive(string label, string raw)
		{
			JsonDocument jsonDocument;
			try
			{
				jsonDocument = JsonDocument.Parse(raw);
			}
			catch
			{
				return;
			}
			JsonElement rootElement = jsonDocument.RootElement;
			if (!rootElement.TryGetProperty(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bf(), out var jsonElement) || jsonElement.ValueKind != JsonValueKind.String)
			{
				jsonDocument.Dispose();
				return;
			}
			string @string = jsonElement.GetString();
			if (!(@string == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bG()))
			{
				if (@string == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bg())
				{
					long @int = rootElement.GetProperty(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bh()).GetInt64();
					string command = rootElement.GetProperty(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bI()).GetString() ?? string.Empty;
					JsonElement jsonElement2;
					JsonElement args = (rootElement.TryGetProperty(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bi(), out jsonElement2) ? jsonElement2.Clone() : default(JsonElement));
					jsonDocument.Dispose();
					Dispatch(label, @int, command, args);
				}
				else
				{
					jsonDocument.Dispose();
				}
			}
			else
			{
				string name = rootElement.GetProperty(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bF()).GetString() ?? string.Empty;
				JsonElement jsonElement3;
				object payload = (rootElement.TryGetProperty(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bH(), out jsonElement3) ? JsonSerializer.Deserialize<object>(jsonElement3.GetRawText(), Options) : null);
				jsonDocument.Dispose();
				Emit(name, payload);
			}
		}

		private async Task Dispatch(string label, long id, string command, JsonElement args)
		{
			object value = null;
			string error = null;
			try
			{
				if (!handlers.TryGetValue(command, out Func<Request, Task<object>> func))
				{
					throw new CommandFailure(command + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Ep());
				}
				value = await func(new Request
				{
					Label = label,
					Args = args
				}).ConfigureAwait(false);
			}
			catch (CommandFailure commandFailure)
			{
				error = commandFailure.Message;
			}
			catch (Exception ex)
			{
				error = ex.Message;
			}
			string encoded;
			try
			{
				encoded = JsonSerializer.Serialize(new
				{
					t = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EQ(),
					id = id,
					ok = (error == null),
					value = value,
					error = error
				}, Options);
			}
			catch (Exception ex2)
			{
				encoded = JsonSerializer.Serialize(new
				{
					t = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EQ(),
					id = id,
					ok = false,
					value = (object)null,
					error = ex2.Message
				}, Options);
			}
			Send(encoded, label);
		}
	}
	public sealed class ByteArrayConverter : JsonConverter<byte[]>
	{
		public override byte[] Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.String)
			{
				return reader.GetBytesFromBase64();
			}
			if (reader.TokenType != JsonTokenType.StartArray)
			{
				throw new JsonException(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bJ());
			}
			List<byte> list = new List<byte>();
			while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
			{
				list.Add(reader.GetByte());
			}
			return list.ToArray();
		}

		public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
		{
			writer.WriteStartArray();
			foreach (byte b in value)
			{
				writer.WriteNumberValue(b);
			}
			writer.WriteEndArray();
		}
	}
	public sealed class Content
	{
		private sealed class Resource
		{
			public required string Path { get; init; }

			public required string ContentType { get; init; }

			public required string Tag { get; init; }

			public required bool Immutable { get; init; }
		}

		[CompilerGenerated]
		private sealed class <Candidates>d__17 : IEnumerable<int>, IEnumerable, IEnumerator<int>, IEnumerator, IDisposable
		{
			private int <>1__state;

			private int <>2__current;

			private int <>l__initialThreadId;

			private int <offset>5__2;

			int IEnumerator<int>.Current
			{
				[DebuggerHidden]
				get
				{
					return <>2__current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return <>2__current;
				}
			}

			[DebuggerHidden]
			public <Candidates>d__17(int <>1__state)
			{
				this.<>1__state = <>1__state;
				<>l__initialThreadId = Environment.CurrentManagedThreadId;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				<>1__state = -2;
			}

			private bool MoveNext()
			{
				switch (<>1__state)
				{
				default:
					return false;
				case 0:
					<>1__state = -1;
					<>2__current = 17435;
					<>1__state = 1;
					return true;
				case 1:
					<>1__state = -1;
					<offset>5__2 = 1;
					goto IL_007c;
				case 2:
					<>1__state = -1;
					<offset>5__2++;
					goto IL_007c;
				case 3:
					{
						<>1__state = -1;
						return false;
					}
					IL_007c:
					if (<offset>5__2 <= 32)
					{
						<>2__current = 17435 + <offset>5__2;
						<>1__state = 2;
						return true;
					}
					<>2__current = 0;
					<>1__state = 3;
					return true;
				}
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator<int> IEnumerable<int>.GetEnumerator()
			{
				if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
				{
					<>1__state = 0;
					return this;
				}
				return new <Candidates>d__17(0);
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<int>)this).GetEnumerator();
			}
		}

		private const int PREFERRED_PORT = 17435;

		private const int IDLE_TIMEOUT = 120000;

		private const int MAX_REQUESTS_PER_CONNECTION = 1000;

		private const int BUFFER_SIZE = 131072;

		private static readonly Dictionary<string, string> types = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bw()] = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bX(),
			[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bx()] = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bY(),
			[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.by()] = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bY(),
			[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bZ()] = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bz(),
			[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.CA()] = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Ca(),
			[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.CB()] = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Cb(),
			[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.CC()] = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Cc(),
			[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.CD()] = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Cc(),
			[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Cd()] = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.CE(),
			[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Ce()] = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.CF(),
			[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Cf()] = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.CG(),
			[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Cg()] = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.CH(),
			[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Ch()] = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.CI(),
			[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Ci()] = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.CJ(),
			[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Cj()] = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.CK(),
			[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Ck()] = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.CL(),
			[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Cl()] = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.CM(),
			[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Cm()] = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.CN(),
			[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Cn()] = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.CO(),
			[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Co()] = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.CP(),
			[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Cp()] = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.CQ(),
			[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Cq()] = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.CR(),
			[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Cr()] = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Ca(),
			[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.CS()] = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Cs()
		};

		private readonly string root;

		private TcpListener? listener;

		public int Port { get; private set; }

		public string Origin
		{
			get
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
				defaultInterpolatedStringHandler.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bj());
				defaultInterpolatedStringHandler.AppendFormatted(Port);
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
		}

		public Content(string root)
		{
			this.root = Path.GetFullPath(root);
		}

		public static string TypeOf(string path)
		{
			if (!types.TryGetValue(Path.GetExtension(path), out string result))
			{
				return 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bK();
			}
			return result;
		}

		public void Start()
		{
			foreach (int item in Candidates())
			{
				try
				{
					TcpListener tcpListener = new TcpListener(IPAddress.Loopback, item);
					tcpListener.Start();
					listener = tcpListener;
					Port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
				}
				catch (SocketException)
				{
					continue;
				}
				break;
			}
			if (listener == null)
			{
				throw new IOException(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bk());
			}
			Thread thread = new Thread(Accept);
			thread.IsBackground = true;
			thread.Name = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bL();
			thread.Start();
		}

		[IteratorStateMachine(typeof(<Candidates>d__17))]
		private static IEnumerable<int> Candidates()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new <Candidates>d__17(-2);
		}

		private void Accept()
		{
			while (true)
			{
				TcpClient client;
				try
				{
					client = listener.AcceptTcpClient();
				}
				catch
				{
					break;
				}
				Task.Run(() => Handle(client));
			}
		}

		private async Task Handle(TcpClient client)
		{
			using (client)
			{
				client.NoDelay = true;
				client.ReceiveTimeout = 120000;
				client.SendTimeout = 120000;
				try
				{
					NetworkStream stream = client.GetStream();
					try
					{
						stream.ReadTimeout = 120000;
						stream.WriteTimeout = 120000;
						using StreamReader reader = new StreamReader(stream, Encoding.ASCII, false, 16384, true);
						for (int served = 0; served < 1000; served++)
						{
							if (!(await Serve(stream, reader).ConfigureAwait(false)))
							{
								return;
							}
						}
					}
					finally
					{
						if (stream != null)
						{
							await stream.DisposeAsync();
						}
					}
				}
				catch
				{
				}
			}
		}

		private async Task<bool> Serve(Stream stream, StreamReader reader)
		{
			string request = await reader.ReadLineAsync().ConfigureAwait(false);
			if (string.IsNullOrEmpty(request))
			{
				return false;
			}
			bool closing = false;
			int length = 0;
			string tag = null;
			while (true)
			{
				string text = await reader.ReadLineAsync().ConfigureAwait(false);
				if (text == null)
				{
					return false;
				}
				if (text.Length == 0)
				{
					break;
				}
				int num = text.IndexOf(':');
				if (num >= 0)
				{
					string text2 = text.Substring(0, num).Trim();
					string text3 = text.Substring(num + 1).Trim();
					if (text2.Equals(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ER(), StringComparison.OrdinalIgnoreCase))
					{
						closing = text3.Contains(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bs(), StringComparison.OrdinalIgnoreCase);
					}
					else if (text2.Equals(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Er(), StringComparison.OrdinalIgnoreCase))
					{
						int.TryParse(text3, out length);
					}
					else if (text2.Equals(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ES(), StringComparison.OrdinalIgnoreCase))
					{
						tag = text3;
					}
				}
			}
			if (length > 0)
			{
				char[] discard = new char[4096];
				int num2;
				for (int taken = 0; taken < length; taken += num2)
				{
					num2 = await reader.ReadAsync(discard, 0, Math.Min(discard.Length, length - taken)).ConfigureAwait(false);
					if (num2 == 0)
					{
						return false;
					}
				}
			}
			string[] array = request.Split(' ');
			if (array.Length < 3)
			{
				return false;
			}
			string text4 = array[0];
			string target = array[1];
			if (array[2].EndsWith(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Es(), StringComparison.Ordinal))
			{
				closing = true;
			}
			Resource resource = Resolve(target);
			if (resource == null)
			{
				await Missing(stream, text4, closing).ConfigureAwait(false);
				return !closing;
			}
			if (tag != null && tag == resource.Tag)
			{
				await stream.WriteAsync(Encoding.ASCII.GetBytes(Head(304, resource.ContentType, 0L, resource, closing))).ConfigureAwait(false);
				await stream.FlushAsync().ConfigureAwait(false);
				return !closing;
			}
			FileStream file;
			try
			{
				file = new FileStream(resource.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 0, FileOptions.Asynchronous | FileOptions.SequentialScan);
			}
			catch
			{
				await Missing(stream, text4, closing).ConfigureAwait(false);
				return !closing;
			}
			FileStream fileStream = file;
			try
			{
				byte[] bytes = Encoding.ASCII.GetBytes(Head(200, resource.ContentType, file.Length, resource, closing));
				if (text4 == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Eq())
				{
					await stream.WriteAsync(bytes).ConfigureAwait(false);
					await stream.FlushAsync().ConfigureAwait(false);
					return !closing;
				}
				byte[] buffer = ArrayPool<byte>.Shared.Rent(131072);
				try
				{
					Buffer.BlockCopy(bytes, 0, buffer, 0, bytes.Length);
					int filled;
					int num3;
					for (filled = bytes.Length; filled < buffer.Length; filled += num3)
					{
						num3 = await file.ReadAsync(buffer.AsMemory(filled)).ConfigureAwait(false);
						if (num3 == 0)
						{
							break;
						}
					}
					await stream.WriteAsync(buffer.AsMemory(0, filled)).ConfigureAwait(false);
					while (true)
					{
						int num4 = await file.ReadAsync(buffer).ConfigureAwait(false);
						if (num4 == 0)
						{
							break;
						}
						await stream.WriteAsync(buffer.AsMemory(0, num4)).ConfigureAwait(false);
					}
				}
				finally
				{
					ArrayPool<byte>.Shared.Return(buffer);
				}
				await stream.FlushAsync().ConfigureAwait(false);
			}
			finally
			{
				if (fileStream != null)
				{
					await fileStream.DisposeAsync();
				}
			}
			return !closing;
		}

		private static async Task Missing(Stream stream, string method, bool closing)
		{
			byte[] body = "not found"u8.ToArray();
			byte[] bytes = Encoding.ASCII.GetBytes(Head(404, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.CR(), body.Length, null, closing));
			await stream.WriteAsync(bytes).ConfigureAwait(false);
			if (method != 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Eq())
			{
				await stream.WriteAsync(body).ConfigureAwait(false);
			}
			await stream.FlushAsync().ConfigureAwait(false);
		}

		private static string Head(int status, string contentType, long contentLength, Resource? resource, bool closing)
		{
			string text = status switch
			{
				200 => 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bl(), 
				304 => 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bM(), 
				_ => 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bm(), 
			};
			StringBuilder stringBuilder = new StringBuilder(256);
			stringBuilder.Append(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bN()).Append(status).Append(' ')
				.Append(text)
				.Append(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aZ());
			stringBuilder.Append(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bn()).Append(contentType).Append(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aZ());
			if (status != 304)
			{
				stringBuilder.Append(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bO()).Append(contentLength).Append(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aZ());
			}
			if (resource != null)
			{
				stringBuilder.Append(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bo()).Append(resource.Tag).Append(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aZ());
				stringBuilder.Append(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bP()).Append(resource.Immutable ? 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bQ() : 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bp()).Append(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aZ());
			}
			else
			{
				stringBuilder.Append(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bq());
			}
			stringBuilder.Append(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bR());
			stringBuilder.Append(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.br()).Append(closing ? 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bs() : 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bS()).Append(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aZ());
			if (!closing)
			{
				stringBuilder.Append(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bT());
			}
			stringBuilder.Append(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.aZ());
			return stringBuilder.ToString();
		}

		private Resource? Resolve(string target)
		{
			string text = target;
			string text2 = string.Empty;
			int num = text.IndexOf('?');
			if (num >= 0)
			{
				text2 = text.Substring(num + 1);
				text = text.Substring(0, num);
			}
			int num2 = text.IndexOf('#');
			if (num2 >= 0)
			{
				text = text.Substring(0, num2);
			}
			text = HttpUtility.UrlDecode(text).TrimStart('/');
			if (text.StartsWith(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bt(), StringComparison.Ordinal))
			{
				string text3 = HttpUtility.ParseQueryString(text2)[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bU()];
				if (!string.IsNullOrEmpty(text3))
				{
					return Describe(text3, immutable: false);
				}
				return null;
			}
			bool immutable = text.StartsWith(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bu(), StringComparison.OrdinalIgnoreCase);
			string fullPath = Path.GetFullPath((text.Length == 0) ? Path.Combine(root, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bV()) : Path.Combine(root, text.Replace('/', Path.DirectorySeparatorChar)));
			if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
			{
				return null;
			}
			Resource resource = Describe(fullPath, immutable);
			if (resource != null)
			{
				return resource;
			}
			return Describe(Path.Combine(root, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bV()), immutable: false);
		}

		private static Resource? Describe(string path, bool immutable)
		{
			FileInfo fileInfo;
			try
			{
				fileInfo = new FileInfo(path);
				if (!fileInfo.Exists)
				{
					return null;
				}
			}
			catch
			{
				return null;
			}
			Resource obj2 = new Resource
			{
				Path = path,
				ContentType = TypeOf(path)
			};
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
			defaultInterpolatedStringHandler.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BD());
			defaultInterpolatedStringHandler.AppendFormatted(fileInfo.Length, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bv());
			defaultInterpolatedStringHandler.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bW());
			defaultInterpolatedStringHandler.AppendFormatted(fileInfo.LastWriteTimeUtc.Ticks, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bv());
			defaultInterpolatedStringHandler.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BD());
			obj2.Tag = defaultInterpolatedStringHandler.ToStringAndClear();
			obj2.Immutable = immutable;
			return obj2;
		}
	}
	public static class Maintenance
	{
		private static readonly TimeSpan TRIM_INTERVAL = TimeSpan.FromSeconds(1.0);

		private static readonly TimeSpan GC_INTERVAL = TimeSpan.FromSeconds(10.0);

		private static long lastGc;

		[DllImport("psapi.dll")]
		private static extern bool EmptyWorkingSet(nint hProcess);

		[DllImport("kernel32.dll")]
		private static extern nint GetCurrentProcess();

		[DllImport("user32.dll")]
		private static extern nint GetForegroundWindow();

		[DllImport("user32.dll")]
		private static extern int GetWindowThreadProcessId(nint hWnd, out int processId);

		public static void Start()
		{
			Thread thread = new Thread(Loop);
			thread.IsBackground = true;
			thread.Name = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.CT();
			thread.Start();
		}

		private static bool IsForeground()
		{
			nint foregroundWindow = GetForegroundWindow();
			if (foregroundWindow == IntPtr.Zero)
			{
				return false;
			}
			GetWindowThreadProcessId(foregroundWindow, out var processId);
			return processId == Environment.ProcessId;
		}

		private static void Loop()
		{
			while (true)
			{
				Thread.Sleep(TRIM_INTERVAL);
				long tickCount = Environment.TickCount64;
				if (!IsForeground() && (double)(tickCount - Volatile.Read(ref lastGc)) >= GC_INTERVAL.TotalMilliseconds)
				{
					GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
					GC.Collect(2, GCCollectionMode.Forced, true, true);
					GC.WaitForPendingFinalizers();
					Volatile.Write(ref lastGc, tickCount);
				}
				try
				{
					EmptyWorkingSet(GetCurrentProcess());
				}
				catch
				{
				}
			}
		}
	}
	public static class Migration
	{
		private const string ORIGIN = "http://tauri.localhost";

		private static Dictionary<string, string>? pending;

		public static void Run(string dataDir)
		{
			try
			{
				string profile = Path.Combine(dataDir, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Ct());
				string text = Path.Combine(profile, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.CU(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Cu(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.CV());
				if (!Directory.Exists(text) || !HasLegacy(text))
				{
					return;
				}
				byte[] prefix = Encoding.ASCII.GetBytes(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Cv());
				Dictionary<string, (ulong Seq, bool Del, string Val)> best = new Dictionary<string, (ulong, bool, string)>();
				string[] files = Directory.GetFiles(text, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.CW());
				foreach (string file2 in files)
				{
					Safe(delegate
					{
						ReadSst(File.ReadAllBytes(file2), prefix, best);
					});
				}
				files = Directory.GetFiles(text, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Cw());
				foreach (string file in files)
				{
					Safe(delegate
					{
						ReadLog(File.ReadAllBytes(file), prefix, best);
					});
				}
				Dictionary<string, string> dictionary = best.Where<KeyValuePair<string, (ulong, bool, string)>>((KeyValuePair<string, (ulong Seq, bool Del, string Val)> p) => !p.Value.Del).ToDictionary<KeyValuePair<string, (ulong, bool, string)>, string, string>((KeyValuePair<string, (ulong Seq, bool Del, string Val)> p) => p.Key, (KeyValuePair<string, (ulong Seq, bool Del, string Val)> p) => p.Value.Val);
				if (dictionary.Count > 0)
				{
					pending = dictionary;
				}
				Safe(delegate
				{
					Directory.Delete(profile, true);
				});
			}
			catch
			{
			}
		}

		public static Dictionary<string, string>? Pull()
		{
			Dictionary<string, string>? result = pending;
			pending = null;
			return result;
		}

		private static void Safe(Action action)
		{
			try
			{
				action();
			}
			catch
			{
			}
		}

		private static bool HasLegacy(string leveldb)
		{
			byte[] array = "tauri.localhost"u8.ToArray();
			foreach (string item in Directory.EnumerateFiles(leveldb))
			{
				try
				{
					if (File.ReadAllBytes(item).AsSpan().IndexOf(array) >= 0)
					{
						return true;
					}
				}
				catch
				{
				}
			}
			return false;
		}

		private static (ulong Value, int Next) Varint(ReadOnlySpan<byte> data, int i)
		{
			ulong num = 0uL;
			int num2 = 0;
			while (true)
			{
				byte b = data[i++];
				num |= (ulong)((long)(b & 0x7F) << num2);
				if ((b & 0x80) == 0)
				{
					break;
				}
				num2 += 7;
			}
			return (Value: num, Next: i);
		}

		private static string Decode(ReadOnlySpan<byte> d)
		{
			if (d.Length == 0)
			{
				return 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au();
			}
			return d[0] switch
			{
				1 => Encoding.Latin1.GetString(d.Slice(1)), 
				0 => Encoding.Unicode.GetString(d.Slice(1)), 
				_ => Encoding.Latin1.GetString(d), 
			};
		}

		private static byte[] Snappy(ReadOnlySpan<byte> data)
		{
			List<byte> list = new List<byte>(data.Length * 2);
			int num = Varint(data, 0).Next;
			while (num < data.Length)
			{
				int num2 = data[num++];
				if ((num2 & 3) == 0)
				{
					int num3 = num2 >> 2;
					if (num3 >= 60)
					{
						int num4 = num3 - 59;
						num3 = 0;
						for (int i = 0; i < num4; i++)
						{
							num3 |= data[num + i] << 8 * i;
						}
						num += num4;
					}
					num3++;
					for (int j = 0; j < num3; j++)
					{
						list.Add(data[num + j]);
					}
					num += num3;
					continue;
				}
				int num5;
				int num6;
				switch (num2 & 3)
				{
				case 1:
					num5 = 4 + ((num2 >> 2) & 7);
					num6 = (num2 >> 5 << 8) | data[num++];
					break;
				case 2:
					num5 = 1 + (num2 >> 2);
					num6 = data[num] | (data[num + 1] << 8);
					num += 2;
					break;
				default:
					num5 = 1 + (num2 >> 2);
					num6 = data[num] | (data[num + 1] << 8) | (data[num + 2] << 16) | (data[num + 3] << 24);
					num += 4;
					break;
				}
				int k = 0;
				int num7 = list.Count - num6;
				for (; k < num5; k++)
				{
					list.Add(list[num7 + k]);
				}
			}
			return list.ToArray();
		}

		private static byte[] Block(byte[] file, int offset, int size)
		{
			return file[offset + size] switch
			{
				0 => file.AsSpan(offset, size).ToArray(), 
				1 => Snappy(file.AsSpan(offset, size)), 
				_ => throw new InvalidDataException(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.CX()), 
			};
		}

		private static List<(byte[] Key, byte[] Value)> Entries(byte[] block)
		{
			int num = block.Length - 4 - 4 * BitConverter.ToInt32(block, block.Length - 4);
			List<(byte[], byte[])> list = new List<(byte[], byte[])>();
			byte[] array = Array.Empty<byte>();
			int num2 = 0;
			while (num2 < num)
			{
				(ulong Value, int Next) tuple = Varint(block, num2);
				ulong item = tuple.Value;
				int item2 = tuple.Next;
				(ulong Value, int Next) tuple2 = Varint(block, item2);
				ulong item3 = tuple2.Value;
				int item4 = tuple2.Next;
				(ulong Value, int Next) tuple3 = Varint(block, item4);
				ulong item5 = tuple3.Value;
				int item6 = tuple3.Next;
				byte[] array2 = new byte[(int)item + (int)item3];
				Array.Copy(array, 0, array2, 0, (int)item);
				Array.Copy(block, item6, array2, (int)item, (int)item3);
				num2 = item6 + (int)item3;
				list.Add((array2, block.AsSpan(num2, (int)item5).ToArray()));
				num2 += (int)item5;
				array = array2;
			}
			return list;
		}

		private static void ReadSst(byte[] file, byte[] prefix, Dictionary<string, (ulong, bool, string)> best)
		{
			Span<byte> span = file.AsSpan(file.Length - 48);
			(ulong Value, int Next) obj = Varint(i: Varint(i: Varint(span, 0).Next, data: span).Next, data: span);
			ulong item3 = obj.Value;
			ulong item5 = Varint(i: obj.Next, data: span).Value;
			foreach (var item10 in Entries(Block(file, (int)item3, (int)item5)))
			{
				byte[] item6 = item10.Value;
				(ulong Value, int Next) tuple = Varint(item6, 0);
				ulong item7 = tuple.Value;
				ulong item9 = Varint(i: tuple.Next, data: item6).Value;
				List<(byte[], byte[])> list;
				try
				{
					list = Entries(Block(file, (int)item7, (int)item9));
				}
				catch
				{
					continue;
				}
				foreach (var (array, value) in list)
				{
					if (array.Length >= 8)
					{
						ulong num = BitConverter.ToUInt64(array, array.Length - 8);
						Accept(array.AsSpan(0, array.Length - 8), num >> 8, (byte)(num & 0xFF), value, prefix, best);
					}
				}
			}
		}

		private static void ReadLog(byte[] file, byte[] prefix, Dictionary<string, (ulong, bool, string)> best)
		{
			List<byte> list = new List<byte>();
			int num = 0;
			while (num + 7 <= file.Length)
			{
				int num2 = num % 32768;
				if (32768 - num2 < 7)
				{
					num += 32768 - num2;
					continue;
				}
				int num3 = file[num + 4] | (file[num + 5] << 8);
				byte b = file[num + 6];
				int num4 = num + 7;
				if (num4 + num3 <= file.Length)
				{
					byte[] array = file.AsSpan(num4, num3).ToArray();
					num = num4 + num3;
					switch (b)
					{
					case 1:
						Batch(array, prefix, best);
						break;
					case 2:
						list.Clear();
						list.AddRange(array);
						break;
					case 3:
						list.AddRange(array);
						break;
					case 4:
						list.AddRange(array);
						Batch(list.ToArray(), prefix, best);
						list.Clear();
						break;
					}
					continue;
				}
				break;
			}
		}

		private static void Batch(byte[] batch, byte[] prefix, Dictionary<string, (ulong, bool, string)> best)
		{
			if (batch.Length < 12)
			{
				return;
			}
			ulong num = BitConverter.ToUInt64(batch, 0);
			int num2 = BitConverter.ToInt32(batch, 8);
			int i = 0;
			int num3 = 12;
			for (; i < num2; i++)
			{
				if (num3 >= batch.Length)
				{
					break;
				}
				byte b = batch[num3++];
				(ulong Value, int Next) tuple = Varint(batch, num3);
				ulong item = tuple.Value;
				int item2 = tuple.Next;
				byte[] array = batch.AsSpan(item2, (int)item).ToArray();
				num3 = item2 + (int)item;
				byte[] value = Array.Empty<byte>();
				if (b == 1)
				{
					(ulong Value, int Next) tuple2 = Varint(batch, num3);
					ulong item3 = tuple2.Value;
					int item4 = tuple2.Next;
					value = batch.AsSpan(item4, (int)item3).ToArray();
					num3 = item4 + (int)item3;
				}
				Accept(array, num + (ulong)i, (b != 0) ? ((byte)1) : ((byte)0), value, prefix, best);
			}
		}

		private static void Accept(ReadOnlySpan<byte> key, ulong sequence, byte type, byte[] value, byte[] prefix, Dictionary<string, (ulong Seq, bool Del, string Val)> best)
		{
			if (key.StartsWith(prefix))
			{
				string text = Decode(key.Slice(prefix.Length));
				if (!best.TryGetValue(text, out (ulong, bool, string) tuple) || tuple.Item1 < sequence)
				{
					best[text] = ((type == 0) ? (sequence, true, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.au()) : (sequence, false, Decode(value)));
				}
			}
		}
	}
}
namespace Stellar.Launcher.Downloader
{
	public sealed class Control
	{
		private int cancel;

		private int paused;

		private readonly SemaphoreSlim notify = new SemaphoreSlim(0);

		public bool Cancelled => Volatile.Read(ref cancel) != 0;

		public bool IsPaused => Volatile.Read(ref paused) != 0;

		public void Pause()
		{
			Volatile.Write(ref paused, 1);
		}

		public void Resume()
		{
			Volatile.Write(ref paused, 0);
			Wake();
		}

		public void Stop()
		{
			Volatile.Write(ref cancel, 1);
			Volatile.Write(ref paused, 0);
			Wake();
		}

		private void Wake()
		{
			try
			{
				notify.Release();
			}
			catch (SemaphoreFullException)
			{
			}
		}

		public async Task WaitWhilePaused()
		{
			while (IsPaused && !Cancelled)
			{
				await notify.WaitAsync(TimeSpan.FromMilliseconds(200.0)).ConfigureAwait(false);
			}
		}
	}
	public sealed class Context
	{
		public required HttpClient Client { get; init; }

		public required string ChunksUrl { get; init; }

		public required Control Control { get; init; }

		public required Counter DoneBytes { get; init; }
	}
	public sealed class Counter
	{
		private long value;

		public long Value => Interlocked.Read(ref value);

		public Counter(long initial)
		{
			value = initial;
		}

		public void Add(long amount)
		{
			Interlocked.Add(ref value, amount);
		}

		public void Subtract(long amount)
		{
			if (amount <= 0)
			{
				return;
			}
			long num = Interlocked.Read(ref value);
			while (true)
			{
				long num2 = Math.Max(num - amount, 0L);
				long num3 = Interlocked.CompareExchange(ref value, num2, num);
				if (num3 == num)
				{
					break;
				}
				num = num3;
			}
		}
	}
	public static class ChunkDownload
	{
		private const int MAX_ATTEMPTS = 10;

		private const int MAX_RESTARTS = 3;

		public const string CANCELLED = "cancelled";

		private static void Rollback(Context context, long amount)
		{
			context.DoneBytes.Subtract(amount);
		}

		public static async Task Download(Context context, Chunk chunk)
		{
			string url = context.ChunksUrl + chunk.Hash + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ET();
			string text = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Et();
			for (int restart = 0; restart < 3; restart++)
			{
				if (context.Control.Cancelled)
				{
					throw new IOException(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EU());
				}
				if (restart > 0)
				{
					await Task.Delay(600).ConfigureAwait(false);
				}
				try
				{
					await Run(context, chunk, url).ConfigureAwait(false);
					return;
				}
				catch (IOException ex) when (ex.Message == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EU())
				{
					throw;
				}
				catch (Exception ex2)
				{
					text = ex2.Message;
				}
			}
			throw new IOException(text);
		}

		private static async Task Run(Context context, Chunk chunk, string url)
		{
			using IncrementalHash hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
			using ScatterWriter writer = new ScatterWriter(chunk.Spans);
			writer.Reset(0L);
			long position = 0L;
			long contributed = 0L;
			int attempts = 0;
			bool complete = false;
			string last = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Eu();
			byte[] buffer = new byte[262144];
			while (attempts < 10)
			{
				if (context.Control.Cancelled)
				{
					Rollback(context, contributed);
					throw new IOException(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EU());
				}
				await context.Control.WaitWhilePaused().ConfigureAwait(false);
				if (context.Control.Cancelled)
				{
					Rollback(context, contributed);
					throw new IOException(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EU());
				}
				if (attempts > 0)
				{
					await Task.Delay((int)Math.Min(300L << Math.Min(attempts, 5), 8000L)).ConfigureAwait(false);
				}
				RangeStream rangeStream;
				try
				{
					rangeStream = await Net.OpenRange(context.Client, url, position, CancellationToken.None).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					last = ex.Message;
					attempts++;
					continue;
				}
				if (!rangeStream.Resumed)
				{
					Rollback(context, contributed);
					contributed = 0L;
					position = 0L;
					hasher.GetHashAndReset();
					writer.Reset(0L);
				}
				bool interrupted = false;
				bool paused = false;
				using (rangeStream.Response)
				{
					Stream body;
					try
					{
						body = await rangeStream.Response.Content.ReadAsStreamAsync().ConfigureAwait(false);
					}
					catch (Exception ex2)
					{
						last = ex2.Message;
						attempts++;
						goto end_IL_031e;
					}
					Stream stream = body;
					try
					{
						do
						{
							if (context.Control.Cancelled)
							{
								Rollback(context, contributed);
								throw new IOException(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EU());
							}
							if (context.Control.IsPaused)
							{
								paused = true;
								break;
							}
							int num;
							try
							{
								using CancellationTokenSource timeout = new CancellationTokenSource(Net.STALL_TIMEOUT);
								num = await body.ReadAsync(buffer, timeout.Token).ConfigureAwait(false);
							}
							catch (OperationCanceledException)
							{
								last = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EV();
								interrupted = true;
								break;
							}
							catch (Exception ex4)
							{
								last = ex4.Message;
								interrupted = true;
								break;
							}
							if (num != 0)
							{
								long num2 = Math.Max(chunk.Size - position, 0L);
								if (num2 != 0L)
								{
									int num3 = (int)Math.Min(num, num2);
									hasher.AppendData(buffer.AsSpan(0, num3));
									writer.Write(buffer.AsSpan(0, num3));
									position += num3;
									contributed += num3;
									context.DoneBytes.Add(num3);
									continue;
								}
								break;
							}
							break;
						}
						while (position < chunk.Size);
					}
					finally
					{
						if (stream != null)
						{
							await stream.DisposeAsync();
						}
					}
					goto IL_06b4;
					end_IL_031e:;
				}
				continue;
				IL_06b4:
				if (position >= chunk.Size)
				{
					complete = true;
					break;
				}
				if (!paused)
				{
					if (!interrupted)
					{
						last = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Ev();
					}
					attempts++;
				}
			}
			writer.Finish();
			if (!complete)
			{
				Rollback(context, contributed);
				throw new IOException(last);
			}
			if (Convert.ToHexString(hasher.GetHashAndReset()).ToLowerInvariant() != chunk.Hash)
			{
				Rollback(context, contributed);
				throw new IOException(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EW());
			}
		}
	}
	public sealed class Session
	{
		public readonly object Gate = new object();

		public required Control Control { get; init; }

		public Snapshot Snapshot { get; set; } = Stellar.Launcher.Downloader.Snapshot.Simple(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Cx(), string.Empty);

	}
	public sealed class Engine
	{
		private sealed class Tracker
		{
			public required Counter DoneBytes { get; init; }

			public required Counter DoneChunks { get; init; }

			public required long TotalBytes { get; init; }

			public required int TotalChunks { get; init; }

			public required string Version { get; init; }

			public required string Path { get; init; }
		}

		private static readonly string[] VERSIONS_URLS = new string[1] { 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cd() };

		private const string PROGRESS_EVENT = "download:progress";

		private const string DONE_EVENT = "download:done";

		private const string FAILED_EVENT = "download:failed";

		private static readonly TimeSpan SAVE_INTERVAL = TimeSpan.FromSeconds(2.0);

		private readonly Bridge bridge;

		private readonly object gate = new object();

		private Session? active;

		public bool Busy => Current() != null;

		public Engine(Bridge bridge)
		{
			this.bridge = bridge;
		}

		private Session? Current()
		{
			lock (gate)
			{
				return active;
			}
		}

		private void Claim(Session session)
		{
			lock (gate)
			{
				if (active != null)
				{
					throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.CY());
				}
				active = session;
			}
		}

		private void Release()
		{
			lock (gate)
			{
				active = null;
			}
		}

		public Snapshot? CurrentSnapshot()
		{
			Session session = Current();
			if (session == null)
			{
				return null;
			}
			lock (session.Gate)
			{
				return session.Snapshot.Clone();
			}
		}

		public void Pause()
		{
			(Current() ?? throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Cy())).Control.Pause();
		}

		public void Resume()
		{
			(Current() ?? throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Cy())).Control.Resume();
		}

		public void Cancel()
		{
			(Current() ?? throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Cy())).Control.Stop();
		}

		private static string Sanitize(string name)
		{
			string text = new string(name.Select((char value) => (!8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Ew().Contains(value)) ? value : '_').ToArray()).Trim().Trim('.');
			if (text.Length != 0)
			{
				return text;
			}
			return 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.CZ();
		}

		private static long? FreeSpace(string path)
		{
			if (!Win32.GetDiskFreeSpaceExW(path, out var lpFreeBytesAvailableToCaller, out var _, out var _))
			{
				return null;
			}
			return (long)lpFreeBytesAvailableToCaller;
		}

		private static string NearestExisting(string path)
		{
			string text = path;
			while (!Directory.Exists(text) && !File.Exists(text))
			{
				string directoryName = Path.GetDirectoryName(text);
				if (string.IsNullOrEmpty(directoryName) || directoryName == text)
				{
					break;
				}
				text = directoryName;
			}
			return text;
		}

		private static async Task<(HttpClient Client, string Body)> FetchVersions(int attempts)
		{
			List<string> failures = new List<string>();
			string[] vERSIONS_URLS = VERSIONS_URLS;
			foreach (string url in vERSIONS_URLS)
			{
				try
				{
					return await Net.FetchTextResilient(url, attempts).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					failures.Add(url + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Dc() + ex.Message);
				}
			}
			throw new CommandFailure(failures.Count switch
			{
				0 => 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ed(), 
				1 => failures[0], 
				_ => 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eE() + string.Join(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ee(), failures) + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eF(), 
			});
		}

		private static async Task<(HttpClient Client, string Body)> FetchManifest(HttpClient client, string url)
		{
			string first;
			try
			{
				return (Client: client, Body: await Net.FetchText(client, url, 3).ConfigureAwait(false));
			}
			catch (Exception ex)
			{
				first = ex.Message;
			}
			if (first.StartsWith(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eD(), StringComparison.Ordinal))
			{
				throw new CommandFailure(first);
			}
			try
			{
				return await Net.FetchTextResilient(url, 3).ConfigureAwait(false);
			}
			catch
			{
				throw new CommandFailure(first);
			}
		}

		private static async Task<(HttpClient Client, Version Version)> LoadVersion(string name)
		{
			string name2 = name;
			(HttpClient, string) obj = await FetchVersions(3).ConfigureAwait(false);
			HttpClient item = obj.Item1;
			Version version2 = ManifestReader.ParseVersions(obj.Item2).FirstOrDefault((Version version) => version.Name == name2) ?? throw new CommandFailure(name2 + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ek());
			return (Client: item, Version: version2);
		}

		public static async Task<List<Version>> ListVersions()
		{
			return ManifestReader.ParseVersions((await FetchVersions(2).ConfigureAwait(false)).Item2);
		}

		private CancellationTokenSource SpawnReporter(Session session, Tracker tracker, string state)
		{
			Tracker tracker2 = tracker;
			Session session2 = session;
			string state2 = state;
			CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
			CancellationToken token = cancellationTokenSource.Token;
			Task.Run(async delegate
			{
				SpeedTracker speed = new SpeedTracker(tracker2.DoneBytes.Value);
				while (!token.IsCancellationRequested)
				{
					try
					{
						await Task.Delay(400, token).ConfigureAwait(false);
					}
					catch (OperationCanceledException)
					{
						break;
					}
					long value = tracker2.DoneBytes.Value;
					double speed2 = speed.Sample(value);
					long remaining = Math.Max(tracker2.TotalBytes - value, 0L);
					string state3 = (session2.Control.IsPaused ? 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.FY() : state2);
					Emit(session2, BuildSnapshot(tracker2, state3, speed2, speed.Eta(remaining), string.Empty));
				}
			}, token);
			return cancellationTokenSource;
		}

		private void Emit(Session session, Snapshot snapshot)
		{
			lock (session.Gate)
			{
				session.Snapshot = snapshot.Clone();
			}
			bridge.Emit(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Cz(), snapshot);
		}

		private static Snapshot BuildSnapshot(Tracker tracker, string state, double speed, long eta, string message)
		{
			long num = Math.Min(tracker.DoneBytes.Value, tracker.TotalBytes);
			return new Snapshot
			{
				State = state,
				Version = tracker.Version,
				Path = tracker.Path,
				DoneBytes = num,
				TotalBytes = tracker.TotalBytes,
				DoneChunks = (int)tracker.DoneChunks.Value,
				TotalChunks = tracker.TotalChunks,
				Percent = ProgressMath.Percent(num, tracker.TotalBytes),
				Speed = speed,
				Eta = eta,
				Message = message
			};
		}

		public async Task<string> Install(string versionName, string destination, int connections)
		{
			Control control = new Control();
			Session session = new Session
			{
				Control = control,
				Snapshot = Snapshot.Simple(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Cx(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ef())
			};
			Claim(session);
			string path = null;
			Exception failure = null;
			try
			{
				path = await InstallInner(session, versionName, destination, connections).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				failure = ex;
			}
			finally
			{
				Release();
			}
			if (failure == null)
			{
				bridge.Emit(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eG(), path);
				return path;
			}
			bridge.Emit(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EY(), failure.Message);
			throw new CommandFailure(failure.Message);
		}

		private async Task<string> InstallInner(Session session, string versionName, string destination, int connections)
		{
			Emit(session, Snapshot.Simple(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Cx(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eg()));
			Version version;
			HttpClient client;
			(client, version) = await LoadVersion(versionName).ConfigureAwait(false);
			if (!version.Chunks.EndsWith('/'))
			{
				version.Chunks += 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Ey();
			}
			Emit(session, Snapshot.Simple(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Cx(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eH()));
			(HttpClient, string) tuple2 = await FetchManifest(client, version.Manifest).ConfigureAwait(false);
			client = tuple2.Item1;
			string root = Path.Combine(destination, Sanitize(version.Label));
			Plan plan = ManifestReader.BuildPlan(tuple2.Item2, root);
			try
			{
				Directory.CreateDirectory(root);
			}
			catch (Exception ex)
			{
				throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DC() + root + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Dc() + ex.Message);
			}
			SaveState saveState = DownloadState.Load(root);
			HashSet<string> completed = ((saveState.Manifest == version.Manifest) ? new HashSet<string>(saveState.Completed, StringComparer.Ordinal) : new HashSet<string>(StringComparer.Ordinal));
			HashSet<string> known = new HashSet<string>(plan.Chunks.Select((Chunk item) => item.Hash), StringComparer.Ordinal);
			completed.RemoveWhere((string hash) => !known.Contains(hash));
			List<Chunk> pending = plan.Chunks.Where((Chunk item) => !completed.Contains(item.Hash)).ToList();
			long initial = plan.Chunks.Where((Chunk item) => completed.Contains(item.Hash)).Sum((Chunk item) => item.Size);
			long num = pending.Sum((Chunk item) => item.Size);
			long? num2 = FreeSpace(NearestExisting(root));
			if (num2.HasValue)
			{
				long valueOrDefault = num2.GetValueOrDefault();
				if (valueOrDefault < num + 536870912)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(37, 2);
					defaultInterpolatedStringHandler.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eh());
					defaultInterpolatedStringHandler.AppendFormatted(FormatSize(valueOrDefault));
					defaultInterpolatedStringHandler.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eI());
					defaultInterpolatedStringHandler.AppendFormatted(FormatSize(num));
					defaultInterpolatedStringHandler.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ei());
					throw new CommandFailure(defaultInterpolatedStringHandler.ToStringAndClear());
				}
			}
			Tracker tracker = new Tracker
			{
				DoneBytes = new Counter(initial),
				DoneChunks = new Counter(plan.Chunks.Count - pending.Count),
				TotalBytes = plan.TotalBytes,
				TotalChunks = plan.Chunks.Count,
				Version = version.Label,
				Path = root
			};
			Emit(session, BuildSnapshot(tracker, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Cx(), 0.0, 0L, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eJ()));
			await Task.Run(delegate
			{
				Writer.Prepare(plan);
			}).ConfigureAwait(false);
			if (pending.Count == 0)
			{
				DownloadState.Clear(root);
				Emit(session, BuildSnapshot(tracker, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eB(), 0.0, 0L, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ej()));
				return root;
			}
			await RunChunks(session, tracker, root, version, client, pending, completed, connections).ConfigureAwait(false);
			DownloadState.Clear(root);
			Emit(session, BuildSnapshot(tracker, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eB(), 0.0, 0L, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eK()));
			return root;
		}

		private async Task RunChunks(Session session, Tracker tracker, string root, Version version, HttpClient client, List<Chunk> pending, HashSet<string> completed, int connections)
		{
			Context context = new Context
			{
				Client = client,
				ChunksUrl = version.Chunks,
				Control = session.Control,
				DoneBytes = tracker.DoneBytes
			};
			SemaphoreSlim permits = new SemaphoreSlim(Math.Clamp(connections, 1, 16));
			try
			{
				List<Task<string>> list = new List<Task<string>>(pending.Count);
				foreach (Chunk item in pending)
				{
					list.Add(Task.Run(async delegate
					{
						await permits.WaitAsync().ConfigureAwait(false);
						try
						{
							if (context.Control.Cancelled)
							{
								throw new IOException(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EU());
							}
							await ChunkDownload.Download(context, item).ConfigureAwait(false);
							return item.Hash;
						}
						finally
						{
							permits.Release();
						}
					}));
				}
				using CancellationTokenSource reporter = SpawnReporter(session, tracker, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eb());
				Stopwatch lastSave = Stopwatch.StartNew();
				string failure = null;
				List<Task<string?>> remaining = new List<Task<string>>(list);
				while (remaining.Count > 0)
				{
					Task<string> task = await Task.WhenAny(remaining).ConfigureAwait(false);
					remaining.Remove(task);
					if (task.IsCompletedSuccessfully)
					{
						completed.Add(task.Result);
						tracker.DoneChunks.Add(1L);
						if (lastSave.Elapsed >= SAVE_INTERVAL)
						{
							try
							{
								DownloadState.Save(root, version.Name, version.Manifest, completed);
							}
							catch
							{
							}
							lastSave.Restart();
						}
					}
					else
					{
						string text = task.Exception?.GetBaseException().Message ?? 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EU();
						if (text != 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EU() && failure == null)
						{
							failure = text;
							session.Control.Stop();
						}
					}
				}
				reporter.Cancel();
				try
				{
					DownloadState.Save(root, version.Name, version.Manifest, completed);
				}
				catch
				{
				}
				if (failure != null)
				{
					throw new CommandFailure(failure);
				}
				if (session.Control.Cancelled)
				{
					throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EU());
				}
			}
			finally
			{
				if (permits != null)
				{
					((IDisposable)permits).Dispose();
				}
			}
		}

		public async Task<Report> Check(string versionName, string installPath, bool repair, int connections)
		{
			Control control = new Control();
			Session session = new Session
			{
				Control = control,
				Snapshot = Snapshot.Simple(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EX(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Ex())
			};
			Claim(session);
			Report report = null;
			Exception failure = null;
			try
			{
				report = await CheckInner(session, versionName, installPath, repair, connections).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				failure = ex;
			}
			finally
			{
				Release();
			}
			if (failure != null)
			{
				bridge.Emit(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EY(), failure.Message);
				throw new CommandFailure(failure.Message);
			}
			return report;
		}

		private async Task<Report> CheckInner(Session session, string versionName, string installPath, bool repair, int connections)
		{
			Session session2 = session;
			Version version;
			HttpClient client;
			(client, version) = await LoadVersion(versionName).ConfigureAwait(false);
			if (!version.Chunks.EndsWith('/'))
			{
				version.Chunks += 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Ey();
			}
			(HttpClient, string) tuple2 = await FetchManifest(client, version.Manifest).ConfigureAwait(false);
			client = tuple2.Item1;
			if (!Directory.Exists(installPath))
			{
				throw new CommandFailure(version.Label + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EZ());
			}
			Plan plan = ManifestReader.BuildPlan(tuple2.Item2, installPath);
			Tracker tracker = new Tracker
			{
				DoneBytes = new Counter(0L),
				DoneChunks = new Counter(0L),
				TotalBytes = plan.TotalBytes,
				TotalChunks = plan.Chunks.Count,
				Version = version.Label,
				Path = installPath
			};
			List<string> missing = Verify.CheckSizes(plan.Files);
			int num = Math.Clamp(Environment.ProcessorCount, 2, 8);
			SemaphoreSlim permits = new SemaphoreSlim(num);
			try
			{
				List<Task<string>> list = new List<Task<string>>(plan.Chunks.Count);
				foreach (Chunk item2 in plan.Chunks)
				{
					list.Add(Task.Run(async delegate
					{
						await permits.WaitAsync().ConfigureAwait(false);
						try
						{
							if (session2.Control.Cancelled)
							{
								return (string?)null;
							}
							long size = item2.Size;
							string hash = item2.Hash;
							bool num2 = await Task.Run(() => Verify.VerifyChunk(item2)).ConfigureAwait(false);
							tracker.DoneBytes.Add(size);
							return num2 ? null : hash;
						}
						finally
						{
							permits.Release();
						}
					}));
				}
				List<string> broken = new List<string>();
				using (CancellationTokenSource reporter = SpawnReporter(session2, tracker, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EX()))
				{
					List<Task<string?>> remaining = new List<Task<string>>(list);
					while (remaining.Count > 0)
					{
						Task<string> task = await Task.WhenAny(remaining).ConfigureAwait(false);
						remaining.Remove(task);
						if (task.IsCompletedSuccessfully)
						{
							string result = task.Result;
							if (result != null)
							{
								broken.Add(result);
							}
						}
						tracker.DoneChunks.Add(1L);
					}
					reporter.Cancel();
				}
				if (session2.Control.Cancelled)
				{
					throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EU());
				}
				Report report = new Report
				{
					TotalChunks = plan.Chunks.Count,
					BadChunks = broken.Count,
					CheckedBytes = plan.TotalBytes,
					Healthy = (broken.Count == 0 && missing.Count == 0),
					MissingFiles = missing
				};
				if (!repair || broken.Count == 0)
				{
					string text;
					if (!report.Healthy)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(22, 2);
						defaultInterpolatedStringHandler.AppendFormatted(broken.Count);
						defaultInterpolatedStringHandler.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Ez());
						defaultInterpolatedStringHandler.AppendFormatted(plan.Chunks.Count);
						defaultInterpolatedStringHandler.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eA());
						text = defaultInterpolatedStringHandler.ToStringAndClear();
					}
					else
					{
						text = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ea();
					}
					string message = text;
					Emit(session2, BuildSnapshot(tracker, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eB(), 0.0, 0L, message));
					return report;
				}
				HashSet<string> damaged = new HashSet<string>(broken, StringComparer.Ordinal);
				List<Chunk> pending = plan.Chunks.Where((Chunk item) => damaged.Contains(item.Hash)).ToList();
				long totalBytes = pending.Sum((Chunk item) => item.Size);
				Tracker repairTracker = new Tracker
				{
					DoneBytes = new Counter(0L),
					DoneChunks = new Counter(0L),
					TotalBytes = totalBytes,
					TotalChunks = pending.Count,
					Version = version.Label,
					Path = installPath
				};
				Emit(session2, BuildSnapshot(repairTracker, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eb(), 0.0, 0L, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eC()));
				await Task.Run(delegate
				{
					Writer.Prepare(plan);
				}).ConfigureAwait(false);
				HashSet<string> completed = new HashSet<string>(from item in plan.Chunks
					where !damaged.Contains(item.Hash)
					select item.Hash, StringComparer.Ordinal);
				await RunChunks(session2, repairTracker, installPath, version, client, pending, completed, connections).ConfigureAwait(false);
				DownloadState.Clear(installPath);
				Emit(session2, BuildSnapshot(repairTracker, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eB(), 0.0, 0L, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ec()));
				return new Report
				{
					TotalChunks = report.TotalChunks,
					BadChunks = report.BadChunks,
					CheckedBytes = report.CheckedBytes,
					Healthy = true,
					MissingFiles = new List<string>()
				};
			}
			finally
			{
				if (permits != null)
				{
					((IDisposable)permits).Dispose();
				}
			}
		}

		public static string FormatSize(long bytes)
		{
			string[] array = new string[5]
			{
				8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cA(),
				8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ca(),
				8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cB(),
				8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cb(),
				8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cC()
			};
			double num = bytes;
			int num2 = 0;
			while (num >= 1024.0 && num2 < array.Length - 1)
			{
				num /= 1024.0;
				num2++;
			}
			if (num2 != 0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
				defaultInterpolatedStringHandler.AppendFormatted(num, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cc());
				defaultInterpolatedStringHandler.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cD());
				defaultInterpolatedStringHandler.AppendFormatted(array[num2]);
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(1, 2);
			defaultInterpolatedStringHandler2.AppendFormatted(bytes);
			defaultInterpolatedStringHandler2.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cD());
			defaultInterpolatedStringHandler2.AppendFormatted(array[num2]);
			return defaultInterpolatedStringHandler2.ToStringAndClear();
		}
	}
	public sealed class Version
	{
		public string Name { get; set; } = string.Empty;


		public string Game { get; set; } = string.Empty;


		public string Release { get; set; } = string.Empty;


		public string Label { get; set; } = string.Empty;


		public string Manifest { get; set; } = string.Empty;


		public string Chunks { get; set; } = string.Empty;

	}
	public sealed class Span
	{
		public required string Path { get; init; }

		public required long Offset { get; init; }

		public required long Start { get; init; }

		public required long End { get; init; }
	}
	public sealed class Chunk
	{
		public required string Hash { get; init; }

		public required long Size { get; init; }

		public required List<Span> Spans { get; init; }
	}
	public sealed class Plan
	{
		public required List<Chunk> Chunks { get; init; }

		public required List<string> Empties { get; init; }

		public required List<(string Path, long Size)> Files { get; init; }

		public required long TotalBytes { get; init; }
	}
	public static class ManifestReader
	{
		private sealed class VersionEntry
		{
			[JsonPropertyName("manifest")]
			public string? Manifest { get; set; }

			[JsonPropertyName("chunks")]
			public string? Chunks { get; set; }
		}

		private sealed class RawInfo
		{
			[JsonPropertyName("name")]
			public string Name { get; set; } = string.Empty;


			[JsonPropertyName("offset")]
			public long Offset { get; set; }

			[JsonPropertyName("start")]
			public long Start { get; set; }

			[JsonPropertyName("end")]
			public long End { get; set; }
		}

		private sealed class RawManifest
		{
			[JsonPropertyName("empty")]
			public List<string> Empty { get; set; } = new List<string>();


			[JsonPropertyName("chunks")]
			public Dictionary<string, List<RawInfo>> Chunks { get; set; } = new Dictionary<string, List<RawInfo>>();

		}

		private static string? Between(string text, string open, string close)
		{
			int num = text.IndexOf(open, StringComparison.Ordinal);
			if (num < 0)
			{
				return null;
			}
			string text2 = text.Substring(num + open.Length);
			int num2 = text2.IndexOf(close, StringComparison.Ordinal);
			if (num2 >= 0)
			{
				return text2.Substring(0, num2);
			}
			return text2;
		}

		private static string GameOf(string name)
		{
			return Between(name, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cE(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ce()) ?? 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.CZ();
		}

		private static string ReleaseOf(string name)
		{
			return Between(name, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cF(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cf()) ?? string.Empty;
		}

		private static (uint Major, uint Minor) ReleaseValue(string release)
		{
			string[] array = release.Split('.');
			uint num;
			uint num2 = ((array.Length != 0 && uint.TryParse(array[0].Trim(), out num)) ? num : 0);
			uint num3;
			uint num4 = ((array.Length > 1 && uint.TryParse(array[1].Trim(), out num3)) ? num3 : 0u);
			return (Major: num2, Minor: num4);
		}

		public static List<Version> ParseVersions(string body)
		{
			Dictionary<string, VersionEntry> dictionary;
			try
			{
				dictionary = JsonSerializer.Deserialize<Dictionary<string, VersionEntry>>(body);
			}
			catch (Exception ex)
			{
				throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cG() + ex.Message);
			}
			List<Version> list = new List<Version>();
			foreach (string item in (dictionary ?? new Dictionary<string, VersionEntry>()).Keys.OrderBy<string, string>((string key) => key, StringComparer.Ordinal))
			{
				VersionEntry versionEntry = dictionary[item];
				if (!string.IsNullOrEmpty(versionEntry.Manifest) && !string.IsNullOrEmpty(versionEntry.Chunks))
				{
					string text = GameOf(item);
					string text2 = ReleaseOf(item);
					list.Add(new Version
					{
						Label = (text + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cD() + text2).Trim(),
						Game = text,
						Release = text2,
						Name = item,
						Manifest = versionEntry.Manifest,
						Chunks = versionEntry.Chunks
					});
				}
			}
			list.Sort(delegate(Version a, Version b)
			{
				(uint, uint) tuple = ReleaseValue(b.Release);
				(uint, uint) tuple2 = ReleaseValue(a.Release);
				return (tuple.Item1 == tuple2.Item1) ? tuple.Item2.CompareTo(tuple2.Item2) : tuple.Item1.CompareTo(tuple2.Item1);
			});
			if (list.Count == 0)
			{
				throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cg());
			}
			return list;
		}

		private static string SafeJoin(string root, string raw)
		{
			string text = root;
			bool flag = false;
			string[] array = raw.Replace('\\', '/').Split('/');
			for (int i = 0; i < array.Length; i++)
			{
				string text2 = array[i].Trim();
				if (text2.Length != 0 && !(text2 == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bA()))
				{
					if (text2 == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cH() || text2.Contains(':'))
					{
						throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ch() + raw);
					}
					text = Path.Combine(text, text2);
					flag = true;
				}
			}
			if (!flag || !text.StartsWith(root, StringComparison.OrdinalIgnoreCase))
			{
				throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ch() + raw);
			}
			return text;
		}

		public static Plan BuildPlan(string body, string root)
		{
			RawManifest rawManifest;
			try
			{
				rawManifest = JsonSerializer.Deserialize<RawManifest>(body);
			}
			catch (Exception ex)
			{
				throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cI() + ex.Message);
			}
			if (rawManifest == null)
			{
				rawManifest = new RawManifest();
			}
			List<Chunk> list = new List<Chunk>(rawManifest.Chunks.Count);
			Dictionary<string, long> dictionary = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
			long num = 0L;
			foreach (string item in rawManifest.Chunks.Keys.OrderBy<string, string>((string key) => key, StringComparer.Ordinal))
			{
				List<RawInfo> list2 = rawManifest.Chunks[item];
				if (item.Length != 40 || !item.All(Uri.IsHexDigit))
				{
					throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ci() + item);
				}
				if (list2.Count == 0)
				{
					continue;
				}
				List<Span> list3 = new List<Span>(list2.Count);
				foreach (RawInfo item2 in list2)
				{
					if (item2.End < item2.Start)
					{
						throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cJ() + item + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cj());
					}
					string text = SafeJoin(root, item2.Name);
					long num2 = item2.End - item2.Start;
					long num3 = item2.Offset + num2;
					if (dictionary.TryGetValue(text, out var num4))
					{
						if (num3 > num4)
						{
							dictionary[text] = num3;
						}
					}
					else
					{
						dictionary[text] = num3;
					}
					num += num2;
					list3.Add(new Span
					{
						Path = text,
						Offset = item2.Offset,
						Start = item2.Start,
						End = item2.End
					});
				}
				list3.Sort((Span a, Span b) => a.Start.CompareTo(b.Start));
				long size = ((list3.Count == 0) ? 0 : list3.Max((Span span) => span.End));
				list.Add(new Chunk
				{
					Hash = item,
					Size = size,
					Spans = list3
				});
			}
			if (list.Count == 0)
			{
				throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cK());
			}
			List<string> list4 = new List<string>(rawManifest.Empty.Count);
			foreach (string item3 in rawManifest.Empty)
			{
				list4.Add(SafeJoin(root, item3));
			}
			List<(string, long)> files = dictionary.Select((KeyValuePair<string, long> pair) => (Path: pair.Key, Size: pair.Value)).OrderBy<(string, long), string>(((string Path, long Size) pair) => pair.Path, StringComparer.Ordinal).ToList();
			return new Plan
			{
				Chunks = list,
				Empties = list4,
				Files = files,
				TotalBytes = num
			};
		}
	}
	public sealed class RangeStream
	{
		public required HttpResponseMessage Response { get; init; }

		public required bool Resumed { get; init; }
	}
	public static class Net
	{
		private sealed class DohAnswer
		{
			[JsonPropertyName("type")]
			public int Kind { get; set; }

			[JsonPropertyName("data")]
			public string Data { get; set; } = string.Empty;

		}

		private sealed class DohReply
		{
			[JsonPropertyName("Answer")]
			public List<DohAnswer> Answer { get; set; } = new List<DohAnswer>();

		}

		public static readonly TimeSpan STALL_TIMEOUT = TimeSpan.FromSeconds(30.0);

		private static readonly string[] DOH_ENDPOINTS = new string[4]
		{
			8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cY(),
			8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cy(),
			8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cZ(),
			8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cz()
		};

		private const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36";

		public const string HTTP_STATUS_PREFIX = "the server returned ";

		private static readonly ConcurrentDictionary<string, List<IPAddress>> cache = new ConcurrentDictionary<string, List<IPAddress>>();

		private static readonly object fallbackGate = new object();

		private static (int Count, HttpClient Client)? fallback;

		private static HttpClient? shared;

		public const string CHALLENGE_MESSAGE = "the CDN's bot protection (Cloudflare) blocked this device; try again on a different network or without a VPN";

		public static HttpClient BuildClient()
		{
			HttpClient httpClient = Volatile.Read(ref shared);
			if (httpClient != null)
			{
				return httpClient;
			}
			HttpClient httpClient2 = new HttpClient(new SocketsHttpHandler
			{
				ConnectTimeout = TimeSpan.FromSeconds(30.0),
				PooledConnectionIdleTimeout = TimeSpan.FromSeconds(90.0),
				MaxConnectionsPerServer = 64,
				KeepAlivePingDelay = TimeSpan.FromSeconds(30.0),
				AutomaticDecompression = DecompressionMethods.All,
				ConnectCallback = async delegate(SocketsHttpConnectionContext context, CancellationToken token)
				{
					string host = context.DnsEndPoint.Host;
					int port = context.DnsEndPoint.Port;
					Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp)
					{
						NoDelay = true
					};
					try
					{
						List<IPAddress> addresses = null;
						IPAddress iPAddress;
						if (cache.TryGetValue(host, out List<IPAddress> list) && list.Count > 0)
						{
							addresses = list;
						}
						else if (!IPAddress.TryParse(host, out iPAddress))
						{
							List<IPAddress> list2 = await ResolveCached(host).ConfigureAwait(false);
							if (list2.Count > 0)
							{
								addresses = list2;
							}
						}
						if (addresses == null || addresses.Count <= 0)
						{
							await socket.ConnectAsync(host, port, token).ConfigureAwait(false);
						}
						else
						{
							await socket.ConnectAsync(addresses.ToArray(), port, token).ConfigureAwait(false);
						}
						return new NetworkStream(socket, true);
					}
					catch
					{
						socket.Dispose();
						throw;
					}
				}
			}, true)
			{
				Timeout = Timeout.InfiniteTimeSpan
			};
			HttpRequestHeaders defaultRequestHeaders = httpClient2.DefaultRequestHeaders;
			defaultRequestHeaders.UserAgent.ParseAdd(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ck());
			defaultRequestHeaders.TryAddWithoutValidation(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cL(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cl());
			defaultRequestHeaders.TryAddWithoutValidation(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cM(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cm());
			defaultRequestHeaders.TryAddWithoutValidation(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cN(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cn());
			defaultRequestHeaders.TryAddWithoutValidation(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cO(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.co());
			defaultRequestHeaders.TryAddWithoutValidation(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cP(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cp());
			defaultRequestHeaders.TryAddWithoutValidation(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cQ(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cq());
			defaultRequestHeaders.TryAddWithoutValidation(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cR(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cr());
			defaultRequestHeaders.TryAddWithoutValidation(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cS(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cs());
			HttpClient httpClient3 = Interlocked.CompareExchange(ref shared, httpClient2, null);
			if (httpClient3 != null)
			{
				httpClient2.Dispose();
				return httpClient3;
			}
			return httpClient2;
		}

		public static async Task<List<IPAddress>> ResolveCached(string host)
		{
			if (cache.TryGetValue(host, out List<IPAddress> result))
			{
				return result;
			}
			List<IPAddress> list = await ResolveOverDoh(host).ConfigureAwait(false);
			if (list.Count > 0)
			{
				cache[host] = list;
			}
			return list;
		}

		public static async Task<HttpClient?> DohClient(string url)
		{
			string text = HostOf(url);
			if (text == null)
			{
				return null;
			}
			if ((await ResolveCached(text).ConfigureAwait(false)).Count == 0)
			{
				return null;
			}
			int count = cache.Count;
			lock (fallbackGate)
			{
				(int, HttpClient)? tuple = fallback;
				if (tuple.HasValue)
				{
					(int, HttpClient) valueOrDefault = tuple.GetValueOrDefault();
					if (valueOrDefault.Item1 == count)
					{
						return valueOrDefault.Item2;
					}
				}
				HttpClient httpClient = BuildClient();
				fallback = (count, httpClient);
				return httpClient;
			}
		}

		public static async Task<List<IPAddress>> ResolveOverDoh(string host)
		{
			using HttpClient client = new HttpClient(new SocketsHttpHandler
			{
				ConnectTimeout = TimeSpan.FromSeconds(10.0)
			})
			{
				Timeout = TimeSpan.FromSeconds(15.0)
			};
			string[] dOH_ENDPOINTS = DOH_ENDPOINTS;
			foreach (string text in dOH_ENDPOINTS)
			{
				try
				{
					using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, text + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eo() + Uri.EscapeDataString(host) + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eP());
					request.Headers.TryAddWithoutValidation(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ep(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eQ());
					using (HttpResponseMessage reply = await client.SendAsync(request).ConfigureAwait(false))
					{
						if (!reply.IsSuccessStatusCode)
						{
							goto end_IL_0145;
						}
						DohReply dohReply = JsonSerializer.Deserialize<DohReply>(await reply.Content.ReadAsStringAsync().ConfigureAwait(false));
						if (dohReply == null)
						{
							goto end_IL_0145;
						}
						List<IPAddress> list = new List<IPAddress>();
						foreach (DohAnswer item in dohReply.Answer)
						{
							if (item.Kind == 1 && IPAddress.TryParse(item.Data, out IPAddress iPAddress) && iPAddress.AddressFamily == AddressFamily.InterNetwork)
							{
								list.Add(iPAddress);
							}
						}
						if (list.Count > 0)
						{
							return list;
						}
						goto end_IL_00a8;
						end_IL_0145:;
					}
					end_IL_00a8:;
				}
				catch
				{
				}
			}
			return new List<IPAddress>();
		}

		public static string? HostOf(string url)
		{
			string[] array = url.Split(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cT(), 2);
			if (array.Length < 2)
			{
				return null;
			}
			string text = array[1].Split('/')[0];
			int num = text.LastIndexOf('@');
			if (num >= 0)
			{
				text = text.Substring(num + 1);
			}
			text = text.Split(':')[0];
			if (text.Length != 0)
			{
				return text;
			}
			return null;
		}

		private static long? ContentRangeTotal(HttpResponseMessage response)
		{
			if (!response.Content.Headers.TryGetValues(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ct(), out IEnumerable<string> enumerable))
			{
				return null;
			}
			foreach (string item in enumerable)
			{
				if (long.TryParse(item.Split('/').Last().Trim(), out var num))
				{
					return num;
				}
			}
			return null;
		}

		public static async Task<long?> ProbeSize(HttpClient client, string url)
		{
			try
			{
				using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
				request.Headers.Range = new RangeHeaderValue(0L, 0L);
				using HttpResponseMessage httpResponseMessage = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
				if (httpResponseMessage.IsSuccessStatusCode)
				{
					return ContentRangeTotal(httpResponseMessage);
				}
				return null;
			}
			catch
			{
			}
			HttpClient httpClient = await DohClient(url).ConfigureAwait(false);
			if (httpClient == null)
			{
				return null;
			}
			try
			{
				using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
				request.Headers.Range = new RangeHeaderValue(0L, 0L);
				using HttpResponseMessage httpResponseMessage2 = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
				return httpResponseMessage2.IsSuccessStatusCode ? ContentRangeTotal(httpResponseMessage2) : null;
			}
			catch
			{
				return null;
			}
		}

		public static async Task<(HttpClient Client, string Body)> FetchTextResilient(string url, int attempts)
		{
			HttpClient httpClient = BuildClient();
			string first;
			try
			{
				HttpClient httpClient2 = httpClient;
				return (Client: httpClient2, Body: await FetchText(httpClient, url, attempts).ConfigureAwait(false));
			}
			catch (Exception ex)
			{
				first = ex.Message;
			}
			if (first.StartsWith(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eD(), StringComparison.Ordinal) || first == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eN())
			{
				throw new IOException(first);
			}
			HttpClient httpClient3 = await DohClient(url).ConfigureAwait(false);
			if (httpClient3 == null)
			{
				throw new IOException(first);
			}
			try
			{
				HttpClient httpClient2 = httpClient3;
				return (Client: httpClient2, Body: await FetchText(httpClient3, url, attempts).ConfigureAwait(false));
			}
			catch (Exception ex2)
			{
				throw new IOException(first + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.en() + ex2.Message + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eO());
			}
		}

		public static bool IsChallengeResponse(HttpResponseMessage response)
		{
			if (response.Headers.Contains(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cU()))
			{
				return true;
			}
			return (response.Content.Headers.ContentType?.MediaType)?.Contains(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cu(), StringComparison.OrdinalIgnoreCase) ?? false;
		}

		private static bool IsCloudflareChallenge(HttpResponseMessage response, string? body)
		{
			if (IsChallengeResponse(response))
			{
				return true;
			}
			if (body == null)
			{
				return false;
			}
			string text = body.TrimStart();
			if (!text.StartsWith(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cV(), StringComparison.Ordinal))
			{
				return false;
			}
			if (!text.StartsWith(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cv(), StringComparison.OrdinalIgnoreCase) && !text.StartsWith(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cW(), StringComparison.OrdinalIgnoreCase) && !text.Contains(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cw(), StringComparison.OrdinalIgnoreCase) && !text.Contains(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cX(), StringComparison.OrdinalIgnoreCase))
			{
				return text.Contains(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cx(), StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}

		public static async Task<string> FetchText(HttpClient client, string url, int attempts)
		{
			string text = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.em();
			for (int attempt = 0; attempt < Math.Max(attempts, 1); attempt++)
			{
				if (attempt > 0)
				{
					await Task.Delay(400 * attempt).ConfigureAwait(false);
				}
				HttpResponseMessage response;
				try
				{
					using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(45.0));
					response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationTokenSource.Token).ConfigureAwait(false);
				}
				catch (OperationCanceledException)
				{
					text = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.el();
					continue;
				}
				catch (Exception ex2)
				{
					text = ex2.Message;
					continue;
				}
				using (response)
				{
					HttpStatusCode statusCode = response.StatusCode;
					if (!response.IsSuccessStatusCode)
					{
						if (IsCloudflareChallenge(response, null))
						{
							throw new IOException(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eN());
						}
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 3);
						defaultInterpolatedStringHandler.AppendFormatted(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eD());
						defaultInterpolatedStringHandler.AppendFormatted((int)statusCode);
						defaultInterpolatedStringHandler.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cD());
						defaultInterpolatedStringHandler.AppendFormatted(statusCode);
						text = defaultInterpolatedStringHandler.ToStringAndClear();
						if (statusCode < HttpStatusCode.InternalServerError && statusCode != HttpStatusCode.RequestTimeout && statusCode != HttpStatusCode.TooManyRequests)
						{
							throw new IOException(text);
						}
						continue;
					}
					string text2;
					try
					{
						using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120.0));
						text2 = await response.Content.ReadAsStringAsync(cancellationTokenSource.Token).ConfigureAwait(false);
					}
					catch (OperationCanceledException)
					{
						text = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eM();
						goto end_IL_019e;
					}
					catch (Exception ex4)
					{
						text = ex4.Message;
						goto end_IL_019e;
					}
					if (IsCloudflareChallenge(response, text2))
					{
						throw new IOException(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eN());
					}
					return text2;
					end_IL_019e:;
				}
			}
			throw new IOException(text);
		}

		private static async Task<HttpResponseMessage> SendRange(HttpClient client, string url, long from, CancellationToken token)
		{
			using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
			if (from > 0)
			{
				request.Headers.Range = new RangeHeaderValue(from, null);
			}
			using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
			timeout.CancelAfter(TimeSpan.FromSeconds(45.0));
			try
			{
				return await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(false);
			}
			catch (OperationCanceledException) when (!token.IsCancellationRequested)
			{
				throw new IOException(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.el());
			}
		}

		public static async Task<RangeStream> OpenRange(HttpClient client, string url, long from, CancellationToken token)
		{
			HttpResponseMessage response;
			try
			{
				response = await SendRange(client, url, from, token).ConfigureAwait(false);
			}
			catch (Exception first)
			{
				HttpClient httpClient = await DohClient(url).ConfigureAwait(false);
				if (httpClient == null)
				{
					throw new IOException(first.Message);
				}
				try
				{
					response = await SendRange(httpClient, url, from, token).ConfigureAwait(false);
				}
				catch
				{
					throw new IOException(first.Message);
				}
			}
			if (!response.IsSuccessStatusCode)
			{
				HttpStatusCode statusCode = response.StatusCode;
				response.Dispose();
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 3);
				defaultInterpolatedStringHandler.AppendFormatted(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eD());
				defaultInterpolatedStringHandler.AppendFormatted((int)statusCode);
				defaultInterpolatedStringHandler.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cD());
				defaultInterpolatedStringHandler.AppendFormatted(statusCode);
				throw new IOException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			return new RangeStream
			{
				Resumed = (from == 0L || response.StatusCode == HttpStatusCode.PartialContent),
				Response = response
			};
		}

		public static async Task<byte[]> FetchBytesResilient(string url)
		{
			HttpClient httpClient = BuildClient();
			HttpResponseMessage httpResponseMessage = null;
			try
			{
				using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(45.0));
				httpResponseMessage = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationTokenSource.Token).ConfigureAwait(false);
			}
			catch
			{
				httpResponseMessage = null;
			}
			if (httpResponseMessage == null)
			{
				HttpClient httpClient2 = (await DohClient(url).ConfigureAwait(false)) ?? throw new IOException(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eL());
				try
				{
					using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(45.0));
					httpResponseMessage = await httpClient2.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationTokenSource.Token).ConfigureAwait(false);
				}
				catch (OperationCanceledException)
				{
					throw new IOException(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.el());
				}
			}
			using (httpResponseMessage)
			{
				if (!httpResponseMessage.IsSuccessStatusCode)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 3);
					defaultInterpolatedStringHandler.AppendFormatted(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eD());
					defaultInterpolatedStringHandler.AppendFormatted((int)httpResponseMessage.StatusCode);
					defaultInterpolatedStringHandler.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cD());
					defaultInterpolatedStringHandler.AppendFormatted(httpResponseMessage.StatusCode);
					throw new IOException(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				try
				{
					using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120.0));
					return await httpResponseMessage.Content.ReadAsByteArrayAsync(cancellationTokenSource.Token).ConfigureAwait(false);
				}
				catch (OperationCanceledException)
				{
					throw new IOException(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eM());
				}
			}
		}
	}
	public sealed class Snapshot
	{
		public string State { get; set; } = string.Empty;


		public string Version { get; set; } = string.Empty;


		public string Path { get; set; } = string.Empty;


		public long DoneBytes { get; set; }

		public long TotalBytes { get; set; }

		public int DoneChunks { get; set; }

		public int TotalChunks { get; set; }

		public double Percent { get; set; }

		public double Speed { get; set; }

		public long Eta { get; set; }

		public string Message { get; set; } = string.Empty;


		public static Snapshot Simple(string state, string message)
		{
			return new Snapshot
			{
				State = state,
				Version = string.Empty,
				Path = string.Empty,
				DoneBytes = 0L,
				TotalBytes = 0L,
				DoneChunks = 0,
				TotalChunks = 0,
				Percent = 0.0,
				Speed = 0.0,
				Eta = 0L,
				Message = message
			};
		}

		public Snapshot Clone()
		{
			return (Snapshot)MemberwiseClone();
		}
	}
	public sealed class SpeedTracker
	{
		private long lastBytes;

		private long lastAt;

		private double speed;

		public SpeedTracker(long bytes)
		{
			lastBytes = bytes;
			lastAt = Stopwatch.GetTimestamp();
			speed = 0.0;
		}

		public double Sample(long bytes)
		{
			long timestamp = Stopwatch.GetTimestamp();
			double num = (double)(timestamp - lastAt) / (double)Stopwatch.Frequency;
			if (num < 0.05)
			{
				return speed;
			}
			double num2 = (double)Math.Max(bytes - lastBytes, 0L) / num;
			speed = ((speed <= 0.0) ? num2 : (speed * 0.7 + num2 * 0.3));
			lastBytes = bytes;
			lastAt = timestamp;
			return speed;
		}

		public long Eta(long remaining)
		{
			if (speed < 1024.0)
			{
				return 0L;
			}
			return (long)Math.Max(Math.Round((double)remaining / speed), 0.0);
		}
	}
	public static class ProgressMath
	{
		public static double Percent(long done, long total)
		{
			if (total == 0L)
			{
				return 0.0;
			}
			return Math.Clamp((double)done / (double)total * 100.0, 0.0, 100.0);
		}
	}
	public sealed class SaveState
	{
		public string Version { get; set; } = string.Empty;


		public string Manifest { get; set; } = string.Empty;


		public List<string> Completed { get; set; } = new List<string>();

	}
	public static class DownloadState
	{
		private const string STATE_FILE = ".stellar-download.json";

		public static string StatePath(string root)
		{
			return Path.Combine(root, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DA());
		}

		public static SaveState Load(string root)
		{
			string text = StatePath(root);
			try
			{
				return JsonSerializer.Deserialize<SaveState>(File.ReadAllText(text), Bridge.Options) ?? new SaveState();
			}
			catch
			{
				return new SaveState();
			}
		}

		public static void Save(string root, string version, string manifest, HashSet<string> completed)
		{
			List<string> list = completed.ToList();
			list.Sort(StringComparer.Ordinal);
			SaveState saveState = new SaveState
			{
				Version = version,
				Manifest = manifest,
				Completed = list
			};
			string text;
			try
			{
				text = JsonSerializer.Serialize(saveState, Bridge.Options);
			}
			catch (Exception ex)
			{
				throw new CommandFailure(ex.Message);
			}
			string text2 = StatePath(root);
			string text3 = Path.ChangeExtension(text2, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Da());
			try
			{
				File.WriteAllText(text3, text);
			}
			catch (Exception ex2)
			{
				throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DB() + ex2.Message);
			}
			try
			{
				File.Move(text3, text2, true);
			}
			catch (Exception ex3)
			{
				throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DB() + ex3.Message);
			}
		}

		public static void Clear(string root)
		{
			try
			{
				File.Delete(StatePath(root));
			}
			catch
			{
			}
			try
			{
				File.Delete(Path.ChangeExtension(StatePath(root), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Da()));
			}
			catch
			{
			}
			try
			{
				File.Delete(Path.Combine(root, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Db()));
			}
			catch
			{
			}
		}
	}
	public sealed class Report
	{
		public int TotalChunks { get; set; }

		public int BadChunks { get; set; }

		public long CheckedBytes { get; set; }

		public bool Healthy { get; set; }

		public List<string> MissingFiles { get; set; } = new List<string>();

	}
	public static class Verify
	{
		private const int READ_SIZE = 1048576;

		public static List<string> CheckSizes(List<(string Path, long Size)> files)
		{
			List<string> list = new List<string>();
			foreach (var (text, num) in files)
			{
				try
				{
					FileInfo fileInfo = new FileInfo(text);
					if (fileInfo.Exists && fileInfo.Length == num)
					{
						continue;
					}
				}
				catch
				{
				}
				list.Add(text);
			}
			return list;
		}

		public static bool VerifyChunk(Chunk chunk)
		{
			using SHA1 sHA = SHA1.Create();
			byte[] array = new byte[1048576];
			long num = 0L;
			foreach (Span span in chunk.Spans)
			{
				if (span.Start != num)
				{
					return false;
				}
				SafeFileHandle safeFileHandle;
				try
				{
					safeFileHandle = File.OpenHandle(span.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, FileOptions.None, 0L);
				}
				catch
				{
					return false;
				}
				using (safeFileHandle)
				{
					long num2 = span.End - span.Start;
					int num4;
					for (long num3 = 0L; num3 < num2; num3 += num4)
					{
						num4 = (int)Math.Min(num2 - num3, array.Length);
						int num5;
						try
						{
							num5 = Writer.ReadAt(safeFileHandle, array.AsSpan(0, num4), span.Offset + num3);
						}
						catch
						{
							return false;
						}
						if (num5 != num4)
						{
							return false;
						}
						sHA.TransformBlock(array, 0, num4, null, 0);
					}
				}
				num = span.End;
			}
			if (num != chunk.Size)
			{
				return false;
			}
			sHA.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
			return Convert.ToHexString(sHA.Hash).ToLowerInvariant() == chunk.Hash;
		}
	}
	public static class Writer
	{
		private const int FLUSH_SIZE = 8388608;

		public static void WriteAt(SafeFileHandle file, ReadOnlySpan<byte> buffer, long offset)
		{
			RandomAccess.Write(file, buffer, offset);
		}

		public static int ReadAt(SafeFileHandle file, Span<byte> buffer, long offset)
		{
			int i;
			int num;
			for (i = 0; i < buffer.Length; i += num)
			{
				num = RandomAccess.Read(file, buffer.Slice(i), offset + i);
				if (num == 0)
				{
					break;
				}
			}
			return i;
		}

		public static SafeFileHandle OpenWrite(string path)
		{
			string directoryName = Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(directoryName))
			{
				try
				{
					Directory.CreateDirectory(directoryName);
				}
				catch (Exception ex)
				{
					throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DC() + directoryName + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Dc() + ex.Message);
				}
			}
			try
			{
				return File.OpenHandle(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, FileOptions.None, 0L);
			}
			catch (Exception ex2)
			{
				throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DD() + path + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Dc() + ex2.Message);
			}
		}

		public static void Prepare(Plan plan)
		{
			foreach (string empty in plan.Empties)
			{
				string directoryName = Path.GetDirectoryName(empty);
				if (!string.IsNullOrEmpty(directoryName))
				{
					try
					{
						Directory.CreateDirectory(directoryName);
					}
					catch (Exception ex)
					{
						throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DC() + directoryName + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Dc() + ex.Message);
					}
				}
				if (!File.Exists(empty))
				{
					try
					{
						File.Create(empty).Dispose();
					}
					catch (Exception ex2)
					{
						throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DC() + empty + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Dc() + ex2.Message);
					}
				}
			}
			foreach (var (text, num) in plan.Files)
			{
				using SafeFileHandle safeFileHandle = OpenWrite(text);
				long length;
				try
				{
					length = RandomAccess.GetLength(safeFileHandle);
				}
				catch (Exception ex3)
				{
					throw new CommandFailure(ex3.Message);
				}
				if (length != num)
				{
					try
					{
						RandomAccess.SetLength(safeFileHandle, num);
					}
					catch (Exception ex4)
					{
						throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Dd() + text + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Dc() + ex4.Message);
					}
				}
			}
		}
	}
	public sealed class ScatterWriter : IDisposable
	{
		private const int FLUSH_SIZE = 8388608;

		private readonly List<Span> spans;

		private readonly Dictionary<string, SafeFileHandle> handles = new Dictionary<string, SafeFileHandle>(StringComparer.OrdinalIgnoreCase);

		private byte[] staging = new byte[8388608];

		private int stagingLength;

		private long stagingAt;

		private int spanIndex;

		public ScatterWriter(List<Span> spans)
		{
			this.spans = spans;
		}

		public void Reset(long position)
		{
			stagingLength = 0;
			stagingAt = position;
			spanIndex = spans.FindIndex((Span span) => span.End > position);
			if (spanIndex < 0)
			{
				spanIndex = spans.Count;
			}
		}

		public void Write(ReadOnlySpan<byte> data)
		{
			if (stagingLength + data.Length > staging.Length)
			{
				Array.Resize(ref staging, Math.Max(staging.Length * 2, stagingLength + data.Length));
			}
			data.CopyTo(staging.AsSpan(stagingLength));
			stagingLength += data.Length;
			if (stagingLength >= 8388608)
			{
				Flush();
			}
		}

		public void Flush()
		{
			if (stagingLength == 0)
			{
				return;
			}
			int num = stagingLength;
			int i = spanIndex;
			int num2 = 0;
			long num3 = stagingAt;
			while (num2 < num)
			{
				long num4;
				for (num4 = num3 + num2; i < spans.Count && num4 >= spans[i].End; i++)
				{
				}
				if (i >= spans.Count)
				{
					break;
				}
				Span span = spans[i];
				if (num4 < span.Start)
				{
					num2 += (int)(span.Start - num4);
					continue;
				}
				int num5 = (int)Math.Min(span.End - num4, num - num2);
				if (!handles.TryGetValue(span.Path, out SafeFileHandle safeFileHandle))
				{
					safeFileHandle = Writer.OpenWrite(span.Path);
					handles[span.Path] = safeFileHandle;
				}
				long offset = span.Offset + (num4 - span.Start);
				try
				{
					Writer.WriteAt(safeFileHandle, staging.AsSpan(num2, num5), offset);
				}
				catch (Exception ex)
				{
					throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DE() + span.Path + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Dc() + ex.Message);
				}
				num2 += num5;
			}
			spanIndex = i;
			stagingAt = num3 + num;
			stagingLength = 0;
		}

		public void Finish()
		{
			Flush();
			Dispose();
		}

		public void Dispose()
		{
			foreach (SafeFileHandle value in handles.Values)
			{
				value.Dispose();
			}
			handles.Clear();
		}
	}
}
namespace Stellar.Launcher.Commands
{
	public static class Build
	{
		public static void DeleteFiles(List<string> files)
		{
			foreach (string file in files)
			{
				try
				{
					File.Delete(file);
				}
				catch (FileNotFoundException)
				{
				}
				catch (DirectoryNotFoundException)
				{
				}
				catch (Exception ex3)
				{
					throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.De() + file + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DF() + ex3.Message);
				}
			}
		}

		public static bool CheckFileExistsAndSize(string path, long? size)
		{
			if (!File.Exists(path))
			{
				return false;
			}
			if (!size.HasValue)
			{
				return true;
			}
			long length;
			try
			{
				length = new FileInfo(path).Length;
			}
			catch (Exception ex)
			{
				throw new CommandFailure(ex.Message);
			}
			return length == size.Value;
		}

		public static bool CheckFileExists(string path)
		{
			return File.Exists(path);
		}

		public static List<string> SearchForVersion(string path)
		{
			byte[] array;
			try
			{
				array = File.ReadAllBytes(path);
			}
			catch (Exception ex)
			{
				throw new CommandFailure(ex.Message);
			}
			byte[] array2 = new byte[22]
			{
				43, 0, 43, 0, 70, 0, 111, 0, 114, 0,
				116, 0, 110, 0, 105, 0, 116, 0, 101, 0,
				43, 0
			};
			List<string> list = new List<string>();
			for (int i = 0; i + array2.Length <= array.Length; i++)
			{
				if (Matches(array, i, array2))
				{
					int to = Math.Min(i + array2.Length + 64, array.Length);
					int? num = FindEnd(array, i + array2.Length, to);
					if (num.HasValue)
					{
						string @string = Encoding.Unicode.GetString(array, i, (array2.Length + num.Value) / 2 * 2);
						list.Add(@string.TrimEnd('\0'));
					}
				}
			}
			return list;
		}

		private static bool Matches(byte[] buffer, int offset, byte[] pattern)
		{
			for (int i = 0; i < pattern.Length; i++)
			{
				if (buffer[offset + i] != pattern[i])
				{
					return false;
				}
			}
			return true;
		}

		private static int? FindEnd(byte[] data, int from, int to)
		{
			for (int i = 0; from + i + 1 < to; i += 2)
			{
				if (data[from + i] == 0 && data[from + i + 1] == 0)
				{
					return i;
				}
			}
			return null;
		}
	}
	public static class Crypto
	{
		[DllImport("stellar_native", CallingConvention = CallingConvention.Cdecl)]
		private static extern nint StellarSocketEncrypt(byte[] plaintext, nuint plaintextLen, byte[] sessionKey, nuint sessionKeyLen, out nuint outLen);

		[DllImport("stellar_native", CallingConvention = CallingConvention.Cdecl)]
		private static extern nint StellarSocketDecrypt(byte[] packet, nuint packetLen, byte[] sessionKey, nuint sessionKeyLen, out nuint outLen);

		[DllImport("stellar_native", CallingConvention = CallingConvention.Cdecl)]
		private static extern void StellarFree(nint ptr, nuint len);

		public static byte[] SocketEncrypt(string plaintext, byte[] sessionKey)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(plaintext);
			nuint outLen;
			nint num = StellarSocketEncrypt(bytes, (nuint)bytes.Length, sessionKey, (nuint)sessionKey.Length, out outLen);
			if (num == IntPtr.Zero)
			{
				throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Df());
			}
			try
			{
				byte[] array = new byte[outLen];
				Marshal.Copy(num, array, 0, (int)outLen);
				return array;
			}
			finally
			{
				StellarFree(num, outLen);
			}
		}

		public static string? SocketDecrypt(byte[] packet, byte[] sessionKey)
		{
			nuint outLen;
			nint num = StellarSocketDecrypt(packet, (nuint)packet.Length, sessionKey, (nuint)sessionKey.Length, out outLen);
			if (num == IntPtr.Zero)
			{
				return null;
			}
			try
			{
				byte[] array = new byte[outLen];
				Marshal.Copy(num, array, 0, (int)outLen);
				return Encoding.UTF8.GetString(array);
			}
			finally
			{
				StellarFree(num, outLen);
			}
		}
	}
	public sealed class FileEntry
	{
		[JsonPropertyName("name")]
		public string Name { get; set; } = string.Empty;


		[JsonPropertyName("path")]
		public string Path { get; set; } = string.Empty;


		[JsonPropertyName("url")]
		public string Url { get; set; } = string.Empty;

	}
	public sealed class Experience
	{
		private const uint CREATE_NO_WINDOW = 134217728u;

		private readonly Bridge bridge;

		public Experience(Bridge bridge)
		{
			this.bridge = bridge;
		}

		public async Task DownloadFiles(List<FileEntry> files, string baseDir)
		{
			HttpClient client = Net.BuildClient();
			int totalFiles = files.Count;
			string basePath = Path.GetFullPath(baseDir);
			List<long> fileSizes = new List<long>(totalFiles);
			foreach (FileEntry file2 in files)
			{
				List<long> list = fileSizes;
				list.Add((await Net.ProbeSize(client, file2.Url).ConfigureAwait(false)).GetValueOrDefault());
			}
			long overallTotal = fileSizes.Sum();
			long completedBytes = 0L;
			for (int index = 0; index < files.Count; index++)
			{
				FileEntry file = files[index];
				string text = Path.Combine(basePath, file.Path);
				if (Path.GetFileName(text) != file.Name)
				{
					text = Path.Combine(text, file.Name);
				}
				if (text.Split('\\', '/').Any((string part) => part == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cH()))
				{
					throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eq());
				}
				string normalized = Path.GetFullPath(text);
				if (!normalized.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
				{
					throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eq());
				}
				string directoryName = Path.GetDirectoryName(normalized);
				if (!string.IsNullOrEmpty(directoryName))
				{
					try
					{
						Directory.CreateDirectory(directoryName);
					}
					catch (Exception ex)
					{
						throw new CommandFailure(ex.Message);
					}
				}
				long expectedSize = fileSizes[index];
				if (expectedSize > 0 && File.Exists(normalized) && Length(normalized) == expectedSize)
				{
					completedBytes += expectedSize;
					bridge.Emit(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eR(), new
					{
						build = baseDir,
						fileIndex = index + 1,
						totalFiles = totalFiles,
						downloaded = completedBytes,
						total = overallTotal,
						skipped = true
					});
					continue;
				}
				string partPath = normalized + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.er();
				int attempt = 0;
				long num2;
				while (true)
				{
					attempt++;
					long resumeFrom = (File.Exists(partPath) ? Length(partPath) : 0);
					string failure = null;
					long? serverTotal;
					long downloaded;
					try
					{
						using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, file.Url);
						if (resumeFrom > 0)
						{
							request.Headers.Range = new RangeHeaderValue(resumeFrom, null);
						}
						using HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
						HttpStatusCode statusCode = response.StatusCode;
						if (statusCode == HttpStatusCode.RequestedRangeNotSatisfiable)
						{
							try
							{
								File.Delete(partPath);
							}
							catch
							{
							}
							throw new IOException(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eS());
						}
						if (!response.IsSuccessStatusCode)
						{
							if (Net.IsChallengeResponse(response))
							{
								throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eN());
							}
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 3);
							defaultInterpolatedStringHandler.AppendFormatted(file.Url);
							defaultInterpolatedStringHandler.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.es());
							defaultInterpolatedStringHandler.AppendFormatted((int)statusCode);
							defaultInterpolatedStringHandler.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.cD());
							defaultInterpolatedStringHandler.AppendFormatted(statusCode);
							throw new IOException(defaultInterpolatedStringHandler.ToStringAndClear());
						}
						if (Net.IsChallengeResponse(response))
						{
							throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eN());
						}
						bool flag = resumeFrom > 0 && statusCode == HttpStatusCode.PartialContent;
						serverTotal = (flag ? ContentRangeTotal(response) : response.Content.Headers.ContentLength);
						downloaded = (flag ? resumeFrom : 0);
						FileStream output = new FileStream(partPath, flag ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read, 1048576);
						try
						{
							Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
							try
							{
								byte[] buffer = new byte[262144];
								Stopwatch lastEmit = Stopwatch.StartNew();
								while (true)
								{
									int read;
									try
									{
										using CancellationTokenSource timeout = new CancellationTokenSource(TimeSpan.FromSeconds(60.0));
										read = await stream.ReadAsync(buffer, timeout.Token).ConfigureAwait(false);
									}
									catch (OperationCanceledException)
									{
										throw new IOException(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eT());
									}
									if (read == 0)
									{
										break;
									}
									await output.WriteAsync(buffer.AsMemory(0, read)).ConfigureAwait(false);
									downloaded += read;
									if (lastEmit.ElapsedMilliseconds >= 100)
									{
										lastEmit.Restart();
										bridge.Emit(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eR(), new
										{
											build = baseDir,
											fileIndex = index + 1,
											totalFiles = totalFiles,
											downloaded = completedBytes + downloaded,
											total = overallTotal,
											skipped = false
										});
									}
								}
								await output.FlushAsync().ConfigureAwait(false);
								output.Flush(true);
							}
							finally
							{
								if (stream != null)
								{
									await stream.DisposeAsync();
								}
							}
						}
						finally
						{
							if (output != null)
							{
								await output.DisposeAsync();
							}
						}
					}
					catch (Exception ex3)
					{
						failure = ex3.Message;
						downloaded = 0L;
						serverTotal = null;
					}
					if (failure == null)
					{
						long? num = ((expectedSize > 0) ? new long?(expectedSize) : serverTotal);
						if (!num.HasValue)
						{
							num2 = downloaded;
							break;
						}
						if (downloaded == num.Value)
						{
							num2 = downloaded;
							break;
						}
						if (downloaded > num.Value)
						{
							try
							{
								File.Delete(partPath);
							}
							catch
							{
							}
							if (attempt >= 6)
							{
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(30, 3);
								defaultInterpolatedStringHandler2.AppendFormatted(file.Name);
								defaultInterpolatedStringHandler2.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.et());
								defaultInterpolatedStringHandler2.AppendFormatted(downloaded);
								defaultInterpolatedStringHandler2.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eU());
								defaultInterpolatedStringHandler2.AppendFormatted(num.Value);
								throw new CommandFailure(defaultInterpolatedStringHandler2.ToStringAndClear());
							}
						}
						else if (attempt >= 6)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(28, 3);
							defaultInterpolatedStringHandler3.AppendFormatted(file.Name);
							defaultInterpolatedStringHandler3.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eu());
							defaultInterpolatedStringHandler3.AppendFormatted(downloaded);
							defaultInterpolatedStringHandler3.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Ez());
							defaultInterpolatedStringHandler3.AppendFormatted(num.Value);
							defaultInterpolatedStringHandler3.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eV());
							throw new CommandFailure(defaultInterpolatedStringHandler3.ToStringAndClear());
						}
					}
					else
					{
						if (failure == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eN())
						{
							throw new CommandFailure(failure);
						}
						if (attempt >= 6)
						{
							throw new CommandFailure(failure);
						}
						HttpClient httpClient = await Net.DohClient(file.Url).ConfigureAwait(false);
						client = ((httpClient == null) ? Net.BuildClient() : httpClient);
					}
					await Task.Delay(500 * attempt).ConfigureAwait(false);
				}
				try
				{
					File.Move(partPath, normalized, true);
				}
				catch (Exception ex4)
				{
					throw new CommandFailure(ex4.Message);
				}
				completedBytes += num2;
				bridge.Emit(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eR(), new
				{
					build = baseDir,
					fileIndex = index + 1,
					totalFiles = totalFiles,
					downloaded = completedBytes,
					total = overallTotal,
					skipped = false
				});
			}
			bridge.Emit(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ev(), new
			{
				build = baseDir
			});
		}

		private static long Length(string path)
		{
			try
			{
				return new FileInfo(path).Length;
			}
			catch
			{
				return 0L;
			}
		}

		private static long? ContentRangeTotal(HttpResponseMessage response)
		{
			if (!response.Content.Headers.TryGetValues(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ct(), out IEnumerable<string> enumerable))
			{
				return null;
			}
			foreach (string item in enumerable)
			{
				if (long.TryParse(item.Split('/').Last().Trim(), out var num))
				{
					return num;
				}
			}
			return null;
		}

		private static string LaunchError(string exe, Exception error)
		{
			switch ((error is Win32Exception ex) ? ex.NativeErrorCode : 0)
			{
			case 2:
			case 3:
				return 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DG() + exe + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Dg();
			case 1260:
			case 4556:
				return 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DH() + exe + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Dh();
			case 5:
				return 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DI() + exe + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Di();
			default:
				return 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DJ() + error.Message;
			}
		}

		private static List<uint> EpicLauncherPids()
		{
			return ProcessLauncher.FindByName(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Dj());
		}

		private static void WaitUntilReady(string[] paths)
		{
			TimeSpan timeSpan = TimeSpan.FromSeconds(20.0);
			foreach (string text in paths)
			{
				DateTime dateTime = DateTime.UtcNow + timeSpan;
				Exception ex = null;
				while (true)
				{
					try
					{
						using (File.Open(text, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
						{
						}
					}
					catch (FileNotFoundException)
					{
						throw new CommandFailure(text + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DK());
					}
					catch (DirectoryNotFoundException)
					{
						throw new CommandFailure(text + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DK());
					}
					catch (Exception ex4)
					{
						ex = ex4;
						goto IL_0071;
					}
					break;
					IL_0071:
					if (DateTime.UtcNow >= dateTime)
					{
						string text2 = ex?.Message ?? 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Dk();
						string text3 = ((ex is UnauthorizedAccessException) ? 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Dl() : 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DL());
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(24, 3);
						defaultInterpolatedStringHandler.AppendFormatted(text);
						defaultInterpolatedStringHandler.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DM());
						defaultInterpolatedStringHandler.AppendFormatted(text2);
						defaultInterpolatedStringHandler.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Dm());
						defaultInterpolatedStringHandler.AppendFormatted(text3);
						throw new CommandFailure(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					Thread.Sleep(100);
				}
			}
		}

		public static bool Launch(string path, string code, string? identity, List<string> extraArgs)
		{
			string text = Path.Combine(path, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DN());
			if (File.Exists(text))
			{
				int num = 0;
				int num2 = 50;
				while (true)
				{
					try
					{
						File.Delete(text);
					}
					catch (Exception ex)
					{
						num++;
						if (num >= num2)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(40, 2);
							defaultInterpolatedStringHandler.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Dn());
							defaultInterpolatedStringHandler.AppendFormatted(num2);
							defaultInterpolatedStringHandler.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DO());
							defaultInterpolatedStringHandler.AppendFormatted(ex.Message);
							throw new CommandFailure(defaultInterpolatedStringHandler.ToStringAndClear());
						}
						if (!File.Exists(text))
						{
							break;
						}
						Thread.Sleep(20);
						continue;
					}
					break;
				}
			}
			string text2 = Path.Combine(path, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Do());
			if (!Directory.Exists(text2))
			{
				try
				{
					Directory.CreateDirectory(text2);
				}
				catch (Exception ex2)
				{
					throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DP() + ex2.Message);
				}
			}
			string text3 = Path.Combine(path, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Dp());
			string text4 = Path.Combine(path, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DQ());
			string text5 = Path.Combine(path, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Dq());
			string text6 = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DR() + code;
			string text7 = ((identity == null) ? null : (8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Dr() + identity));
			List<string> list = new List<string>
			{
				8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DS(),
				8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Ds(),
				8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DT(),
				8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Dt(),
				8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DU(),
				8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Du(),
				8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DV(),
				8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Dv(),
				8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DW(),
				8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Dw(),
				8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DX(),
				8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Dx(),
				text6,
				8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DY(),
				text7 ?? string.Empty
			};
			list.AddRange(extraArgs);
			WaitUntilReady(new string[3] { text3, text4, text5 });
			List<uint> preexistingEpic = EpicLauncherPids();
			string workingDirectory = Path.GetDirectoryName(text3) ?? path;
			string workingDirectory2 = Path.GetDirectoryName(text4) ?? path;
			Stellar.Launcher.Native.Process process;
			try
			{
				process = ProcessLauncher.Spawn(text3, list, workingDirectory, 134217728u);
			}
			catch (Exception error)
			{
				throw new CommandFailure(LaunchError(text3, error));
			}
			List<Stellar.Launcher.Native.Process> list2 = new List<Stellar.Launcher.Native.Process>();
			(string, uint)[] array = new(string, uint)[2]
			{
				(text4, 134217732u),
				(text5, 134217732u)
			};
			for (int i = 0; i < array.Length; i++)
			{
				var (text8, flags) = array[i];
				try
				{
					list2.Add(ProcessLauncher.Spawn(text8, list, workingDirectory2, flags));
				}
				catch (Exception error2)
				{
					string message = LaunchError(text8, error2);
					process.Kill();
					process.Release();
					foreach (Stellar.Launcher.Native.Process item in list2)
					{
						item.Kill();
						item.Release();
					}
					throw new CommandFailure(message);
				}
			}
			process.Release();
			foreach (Stellar.Launcher.Native.Process item2 in list2)
			{
				item2.Release();
			}
			Thread thread = new Thread((ThreadStart)delegate
			{
				DateTime dateTime = DateTime.UtcNow + TimeSpan.FromSeconds(90.0);
				while (DateTime.UtcNow < dateTime)
				{
					foreach (uint item3 in EpicLauncherPids())
					{
						if (!preexistingEpic.Contains(item3))
						{
							ProcessLauncher.KillPid(item3);
						}
					}
					Thread.Sleep(TimeSpan.FromSeconds(2.0));
				}
			});
			thread.IsBackground = true;
			thread.Start();
			return true;
		}

		public static void ExitAll()
		{
			foreach (uint item in ProcessLauncher.FindByName(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Dy(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.DZ(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Dz(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dA(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.da(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dB(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.db(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Dj()))
			{
				ProcessLauncher.KillPid(item);
			}
			Thread.Sleep(1000);
		}
	}
	public static class Overlay
	{
		private const double OVERLAY_WIDTH = 380.0;

		private const double OVERLAY_MARGIN = 16.0;

		private static readonly object cornerGate = new object();

		private static string corner = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dD();

		private static bool TargetMonitor(ManagedWindow window, out Win32.RECT area, out double scale)
		{
			area = default(Win32.RECT);
			scale = 1.0;
			nint num = Win32.MonitorFromWindow(window.Handle, 2u);
			if (num == IntPtr.Zero)
			{
				return false;
			}
			Win32.MONITORINFO mONITORINFO = default(Win32.MONITORINFO);
			mONITORINFO.cbSize = Marshal.SizeOf<Win32.MONITORINFO>();
			Win32.MONITORINFO lpmi = mONITORINFO;
			if (!Win32.GetMonitorInfo(num, ref lpmi))
			{
				return false;
			}
			area = lpmi.rcMonitor;
			if (Win32.GetDpiForMonitor(num, 0, out var dpiX, out var _) == 0 && dpiX != 0)
			{
				scale = (double)dpiX / 96.0;
			}
			return true;
		}

		private static void Place(ManagedWindow window, double? height)
		{
			if (TargetMonitor(window, out var area, out var scale))
			{
				int num = (int)Math.Round(16.0 * scale);
				int num2 = area.Right - area.Left;
				int num3 = area.Bottom - area.Top;
				(int, int) valueTuple;
				if (height.HasValue)
				{
					double valueOrDefault = height.GetValueOrDefault();
					valueTuple = ((int)Math.Round(380.0 * scale), (int)Math.Round(valueOrDefault * scale));
				}
				else
				{
					valueTuple = (window.Window.Width, window.Window.Height);
				}
				(int, int) tuple = valueTuple;
				string text;
				lock (cornerGate)
				{
					text = corner;
				}
				int x = (text.EndsWith(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BN(), StringComparison.Ordinal) ? (area.Left + num2 - tuple.Item1 - num) : (area.Left + num));
				int y = (text.StartsWith(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dC(), StringComparison.Ordinal) ? (area.Top + num3 - tuple.Item2 - num) : (text.StartsWith(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dc(), StringComparison.Ordinal) ? (area.Top + (num3 - tuple.Item2) / 2) : (area.Top + num)));
				Win32.SetWindowPos(window.Handle, Win32.HWND_TOPMOST, x, y, tuple.Item1, tuple.Item2, 0x10u | ((!height.HasValue) ? 1u : 0u));
			}
		}

		public static void SetPosition(string position)
		{
			lock (cornerGate)
			{
				corner = position;
			}
			ManagedWindow overlay = AppWindows.Overlay;
			if (overlay != null)
			{
				Place(overlay, null);
			}
		}

		public static void Show()
		{
			ManagedWindow managedWindow = AppWindows.EnsureOverlay();
			if (managedWindow != null)
			{
				AppWindows.SetIgnoreCursorEvents(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ax(), ignore: true);
				Place(managedWindow, null);
				AppWindows.ShowWithoutActivating(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ax());
				Win32.SetWindowPos(managedWindow.Handle, Win32.HWND_TOPMOST, 0, 0, 0, 0, 19u);
			}
		}

		public static void Hide()
		{
			if (AppWindows.Overlay != null)
			{
				AppWindows.Hide(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ax());
			}
		}

		public static void Resize(double height)
		{
			ManagedWindow overlay = AppWindows.Overlay;
			if (overlay != null)
			{
				double num = 720.0;
				if (TargetMonitor(overlay, out var area, out var scale))
				{
					num = (double)(area.Bottom - area.Top) / scale - 32.0;
				}
				double num2 = Math.Min(Math.Max(height, 1.0), num);
				Place(overlay, num2);
			}
		}

		public static void SetInteractive(bool interactive)
		{
			if (AppWindows.Overlay != null && (!interactive || AppWindows.IsVisible(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ax())))
			{
				AppWindows.SetIgnoreCursorEvents(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ax(), !interactive);
				if (interactive)
				{
					AppWindows.Focus(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ax());
				}
			}
		}

		public static bool Visible()
		{
			if (AppWindows.Overlay != null)
			{
				return AppWindows.IsVisible(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ax());
			}
			return false;
		}
	}
	public sealed class ActivityTimestamps
	{
		[JsonPropertyName("start")]
		public long? Start { get; set; }

		[JsonPropertyName("end")]
		public long? End { get; set; }
	}
	public sealed class ActivityAssets
	{
		[JsonPropertyName("largeImage")]
		public string? LargeImage { get; set; }

		[JsonPropertyName("largeText")]
		public string? LargeText { get; set; }

		[JsonPropertyName("smallImage")]
		public string? SmallImage { get; set; }

		[JsonPropertyName("smallText")]
		public string? SmallText { get; set; }
	}
	public sealed class ActivityButton
	{
		[JsonPropertyName("label")]
		public string Label { get; set; } = string.Empty;


		[JsonPropertyName("url")]
		public string Url { get; set; } = string.Empty;

	}
	public sealed class ActivityPayload
	{
		[JsonPropertyName("state")]
		public string? State { get; set; }

		[JsonPropertyName("details")]
		public string? Details { get; set; }

		[JsonPropertyName("type")]
		public int? Type { get; set; }

		[JsonPropertyName("timestamps")]
		public ActivityTimestamps? Timestamps { get; set; }

		[JsonPropertyName("assets")]
		public ActivityAssets? Assets { get; set; }

		[JsonPropertyName("buttons")]
		public List<ActivityButton> Buttons { get; set; } = new List<ActivityButton>();

	}
	public sealed class Rpc
	{
		private readonly object gate = new object();

		private NamedPipeClientStream? pipe;

		private string clientId = string.Empty;

		public async Task Start(string id)
		{
			NamedPipeClientStream namedPipeClientStream;
			lock (gate)
			{
				namedPipeClientStream = pipe;
				clientId = id;
			}
			if (namedPipeClientStream != null && namedPipeClientStream.IsConnected)
			{
				return;
			}
			for (int index = 0; index < 10; index++)
			{
				string text = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bA();
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(12, 1);
				defaultInterpolatedStringHandler.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Fc());
				defaultInterpolatedStringHandler.AppendFormatted(index);
				NamedPipeClientStream stream = new NamedPipeClientStream(text, defaultInterpolatedStringHandler.ToStringAndClear(), PipeDirection.InOut, PipeOptions.Asynchronous);
				try
				{
					await stream.ConnectAsync(500).ConfigureAwait(false);
				}
				catch
				{
					await stream.DisposeAsync().ConfigureAwait(false);
					continue;
				}
				try
				{
					await Frame(stream, 0, JsonSerializer.Serialize(new
					{
						v = 1,
						client_id = id
					})).ConfigureAwait(false);
				}
				catch
				{
					await stream.DisposeAsync().ConfigureAwait(false);
					continue;
				}
				lock (gate)
				{
					pipe?.Dispose();
					pipe = stream;
				}
				Task.Run(() => Drain(stream));
				return;
			}
			throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.FD());
		}

		private static async Task Frame(Stream stream, int opcode, string payload)
		{
			byte[] body = Encoding.UTF8.GetBytes(payload);
			byte[] array = new byte[8];
			BinaryPrimitives.WriteInt32LittleEndian(array.AsSpan(0, 4), opcode);
			BinaryPrimitives.WriteInt32LittleEndian(array.AsSpan(4, 4), body.Length);
			await stream.WriteAsync(array).ConfigureAwait(false);
			await stream.WriteAsync(body).ConfigureAwait(false);
			await stream.FlushAsync().ConfigureAwait(false);
		}

		private static async Task Drain(NamedPipeClientStream stream)
		{
			byte[] header = new byte[8];
			try
			{
				while (stream.IsConnected && await stream.ReadAsync(header).ConfigureAwait(false) >= 8)
				{
					int length = BinaryPrimitives.ReadInt32LittleEndian(header.AsSpan(4, 4));
					if (length <= 0)
					{
						continue;
					}
					byte[] body = new byte[length];
					int num;
					for (int taken = 0; taken < length; taken += num)
					{
						num = await stream.ReadAsync(body.AsMemory(taken)).ConfigureAwait(false);
						if (num == 0)
						{
							return;
						}
					}
				}
			}
			catch
			{
			}
		}

		public async Task SetActivity(ActivityPayload activity)
		{
			NamedPipeClientStream namedPipeClientStream;
			lock (gate)
			{
				namedPipeClientStream = pipe;
			}
			if (namedPipeClientStream == null || !namedPipeClientStream.IsConnected)
			{
				return;
			}
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			if (!string.IsNullOrEmpty(activity.State))
			{
				dictionary[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eX()] = activity.State;
			}
			if (!string.IsNullOrEmpty(activity.Details))
			{
				dictionary[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ex()] = activity.Details;
			}
			int? type = activity.Type;
			if (type.HasValue)
			{
				int valueOrDefault = type.GetValueOrDefault();
				dictionary[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eY()] = valueOrDefault;
			}
			ActivityTimestamps timestamps = activity.Timestamps;
			if (timestamps != null)
			{
				Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
				long? start = timestamps.Start;
				if (start.HasValue)
				{
					long valueOrDefault2 = start.GetValueOrDefault();
					dictionary2[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ey()] = valueOrDefault2;
				}
				start = timestamps.End;
				if (start.HasValue)
				{
					long valueOrDefault3 = start.GetValueOrDefault();
					dictionary2[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Br()] = valueOrDefault3;
				}
				if (dictionary2.Count > 0)
				{
					dictionary[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eZ()] = dictionary2;
				}
			}
			ActivityAssets assets = activity.Assets;
			if (assets != null)
			{
				Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
				if (!string.IsNullOrEmpty(assets.LargeImage))
				{
					dictionary3[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ez()] = assets.LargeImage;
				}
				if (!string.IsNullOrEmpty(assets.LargeText))
				{
					dictionary3[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.FA()] = assets.LargeText;
				}
				if (!string.IsNullOrEmpty(assets.SmallImage))
				{
					dictionary3[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Fa()] = assets.SmallImage;
				}
				if (!string.IsNullOrEmpty(assets.SmallText))
				{
					dictionary3[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.FB()] = assets.SmallText;
				}
				if (dictionary3.Count > 0)
				{
					dictionary[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Fb()] = dictionary3;
				}
			}
			if (activity.Buttons.Count > 0)
			{
				dictionary[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.FC()] = activity.Buttons.Select((ActivityButton button) => new Dictionary<string, string>
				{
					[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Eh()] = button.Label,
					[8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.eW()] = button.Url
				}).ToList();
			}
			string payload = JsonSerializer.Serialize(new
			{
				cmd = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ew(),
				nonce = Guid.NewGuid().ToString(),
				args = new
				{
					pid = Environment.ProcessId,
					activity = dictionary
				}
			});
			try
			{
				await Frame(namedPipeClientStream, 1, payload).ConfigureAwait(false);
			}
			catch
			{
			}
		}

		public async Task ClearActivity()
		{
			NamedPipeClientStream namedPipeClientStream;
			lock (gate)
			{
				namedPipeClientStream = pipe;
			}
			if (namedPipeClientStream == null || !namedPipeClientStream.IsConnected)
			{
				return;
			}
			string payload = JsonSerializer.Serialize(new
			{
				cmd = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ew(),
				nonce = Guid.NewGuid().ToString(),
				args = new
				{
					pid = Environment.ProcessId,
					activity = (object)null
				}
			});
			try
			{
				await Frame(namedPipeClientStream, 1, payload).ConfigureAwait(false);
			}
			catch
			{
			}
		}

		public void Stop()
		{
			lock (gate)
			{
				pipe?.Dispose();
				pipe = null;
			}
		}
	}
	public sealed class SecurityTarget
	{
		public string Label { get; set; } = string.Empty;


		public string Path { get; set; } = string.Empty;


		public bool Exists { get; set; }
	}
	public sealed class SecurityStatus
	{
		public string SmartAppControl { get; set; } = string.Empty;


		public bool IsAdmin { get; set; }

		public List<SecurityTarget> Targets { get; set; } = new List<SecurityTarget>();

	}
	public static class Security
	{
		private static string ReadSacState()
		{
			try
			{
				using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dd());
				if (registryKey == null)
				{
					return 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dE();
				}
				object value = registryKey.GetValue(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.de());
				if (!(value is int))
				{
					return 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dE();
				}
				return (int)value switch
				{
					0 => 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dF(), 
					1 => 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.df(), 
					2 => 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dG(), 
					_ => 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dg(), 
				};
			}
			catch
			{
				return 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dE();
			}
		}

		private static bool RunningAsAdmin()
		{
			try
			{
				using WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();
				return new WindowsPrincipal(windowsIdentity).IsInRole(WindowsBuiltInRole.Administrator);
			}
			catch
			{
				return false;
			}
		}

		private static string? CurrentExe()
		{
			return Environment.ProcessPath;
		}

		private static string? LauncherDir()
		{
			string text = CurrentExe();
			if (text != null)
			{
				return Path.GetDirectoryName(text);
			}
			return null;
		}

		private static List<SecurityTarget> CollectTargets(List<string> buildPaths)
		{
			List<SecurityTarget> list = new List<SecurityTarget>();
			string text = CurrentExe();
			if (text != null)
			{
				list.Add(new SecurityTarget
				{
					Label = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dH(),
					Path = text,
					Exists = File.Exists(text)
				});
			}
			string text2 = LauncherDir();
			if (text2 != null)
			{
				list.Add(new SecurityTarget
				{
					Label = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dh(),
					Path = text2,
					Exists = Directory.Exists(text2)
				});
			}
			foreach (string buildPath in buildPaths)
			{
				string fileName = Path.GetFileName(buildPath.TrimEnd('\\', '/'));
				list.Add(new SecurityTarget
				{
					Label = (string.IsNullOrEmpty(fileName) ? 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dI() : fileName),
					Path = buildPath,
					Exists = File.Exists(Path.Combine(buildPath, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.di(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.db()))
				});
			}
			return list;
		}

		public static SecurityStatus Status(List<string> buildPaths)
		{
			return new SecurityStatus
			{
				SmartAppControl = ReadSacState(),
				IsAdmin = RunningAsAdmin(),
				Targets = CollectTargets(buildPaths)
			};
		}

		public static List<string> ApplyExclusions(List<string> buildPaths)
		{
			List<string> list = (from target in CollectTargets(buildPaths)
				where target.Exists
				select target.Path).ToList();
			if (list.Count == 0)
			{
				throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dJ());
			}
			List<string> list2 = list.Select((string path) => 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Fd() + path.Replace(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Fd(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.FE()) + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Fd()).ToList();
			string text = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dj() + string.Join(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dK(), list2);
			ProcessStartInfo processStartInfo = new ProcessStartInfo
			{
				FileName = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dk(),
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};
			processStartInfo.ArgumentList.Add(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dL());
			processStartInfo.ArgumentList.Add(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dl());
			processStartInfo.ArgumentList.Add(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dM());
			processStartInfo.ArgumentList.Add(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dm());
			processStartInfo.ArgumentList.Add(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dN());
			processStartInfo.ArgumentList.Add(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dn() + text.Replace(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BD(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dO()) + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BD());
			System.Diagnostics.Process process;
			try
			{
				process = System.Diagnostics.Process.Start(processStartInfo) ?? throw new IOException(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.@do());
			}
			catch (Exception ex)
			{
				throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dP() + ex.Message);
			}
			using (process)
			{
				string text2 = process.StandardError.ReadToEnd();
				process.StandardOutput.ReadToEnd();
				process.WaitForExit();
				if (process.ExitCode != 0)
				{
					string text3 = text2.Trim();
					throw new CommandFailure((text3.Length == 0) ? 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dQ() : (8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dp() + text3));
				}
				return list;
			}
		}

		public static void OpenSmartAppControl()
		{
			try
			{
				System.Diagnostics.Process.Start(new ProcessStartInfo
				{
					FileName = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dq(),
					UseShellExecute = true
				});
			}
			catch (Exception ex)
			{
				throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dR() + ex.Message);
			}
		}
	}
	public sealed class Sockets
	{
		private sealed class Connection
		{
			public readonly SemaphoreSlim SendGate = new SemaphoreSlim(1, 1);

			public required ClientWebSocket Socket { get; init; }

			public required CancellationTokenSource Cancellation { get; init; }
		}

		private readonly Bridge bridge;

		private readonly ConcurrentDictionary<long, Connection> connections = new ConcurrentDictionary<long, Connection>();

		private long sequence;

		public Sockets(Bridge bridge)
		{
			this.bridge = bridge;
		}

		public async Task<long> Connect(string url, Dictionary<string, string> headers)
		{
			ClientWebSocket socket = new ClientWebSocket();
			foreach (var (text3, text4) in headers)
			{
				if (string.Equals(text3, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Fe(), StringComparison.OrdinalIgnoreCase))
				{
					socket.Options.SetRequestHeader(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Fe(), text4);
				}
				else if (!string.Equals(text3, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.FF(), StringComparison.OrdinalIgnoreCase))
				{
					try
					{
						socket.Options.SetRequestHeader(text3, text4);
					}
					catch
					{
					}
				}
			}
			try
			{
				await socket.ConnectAsync(new Uri(url), CancellationToken.None).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				socket.Dispose();
				throw new CommandFailure(ex.Message);
			}
			long id = Interlocked.Increment(ref sequence);
			Connection connection = new Connection
			{
				Socket = socket,
				Cancellation = new CancellationTokenSource()
			};
			connections[id] = connection;
			Task.Run(() => Receive(id, connection));
			return id;
		}

		private async Task Receive(long id, Connection connection)
		{
			byte[] buffer = new byte[65536];
			MemoryStream accumulated = new MemoryStream();
			try
			{
				while (connection.Socket.State == WebSocketState.Open && !connection.Cancellation.IsCancellationRequested)
				{
					WebSocketReceiveResult webSocketReceiveResult;
					try
					{
						webSocketReceiveResult = await connection.Socket.ReceiveAsync(new ArraySegment<byte>(buffer), connection.Cancellation.Token).ConfigureAwait(false);
					}
					catch
					{
						break;
					}
					if (webSocketReceiveResult.MessageType == WebSocketMessageType.Close)
					{
						break;
					}
					accumulated.Write(buffer, 0, webSocketReceiveResult.Count);
					if (webSocketReceiveResult.EndOfMessage)
					{
						byte[] array = accumulated.ToArray();
						accumulated.SetLength(0L);
						if (webSocketReceiveResult.MessageType == WebSocketMessageType.Text)
						{
							Bridge obj2 = bridge;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(13, 1);
							defaultInterpolatedStringHandler.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Ff());
							defaultInterpolatedStringHandler.AppendFormatted(id);
							obj2.Emit(defaultInterpolatedStringHandler.ToStringAndClear(), new
							{
								type = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.FG(),
								data = Encoding.UTF8.GetString(array)
							});
						}
						else
						{
							Bridge obj3 = bridge;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(13, 1);
							defaultInterpolatedStringHandler2.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Ff());
							defaultInterpolatedStringHandler2.AppendFormatted(id);
							obj3.Emit(defaultInterpolatedStringHandler2.ToStringAndClear(), new
							{
								type = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Fg(),
								data = Convert.ToBase64String(array)
							});
						}
					}
				}
			}
			finally
			{
				Bridge obj4 = bridge;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(13, 1);
				defaultInterpolatedStringHandler3.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Ff());
				defaultInterpolatedStringHandler3.AppendFormatted(id);
				obj4.Emit(defaultInterpolatedStringHandler3.ToStringAndClear(), new
				{
					type = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.FH(),
					data = new
					{
						code = (int)connection.Socket.CloseStatus.GetValueOrDefault(WebSocketCloseStatus.NormalClosure),
						reason = (connection.Socket.CloseStatusDescription ?? string.Empty)
					}
				});
				Teardown(id);
			}
		}

		public async Task Send(long id, string type, byte[] binary, string text)
		{
			if (!connections.TryGetValue(id, out Connection connection))
			{
				throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Fh());
			}
			await connection.SendGate.WaitAsync().ConfigureAwait(false);
			try
			{
				if (type == 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.FG())
				{
					await connection.Socket.SendAsync(Encoding.UTF8.GetBytes(text), WebSocketMessageType.Text, true, CancellationToken.None).ConfigureAwait(false);
				}
				else
				{
					await connection.Socket.SendAsync(binary, WebSocketMessageType.Binary, true, CancellationToken.None).ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				throw new CommandFailure(ex.Message);
			}
			finally
			{
				connection.SendGate.Release();
			}
		}

		public async Task Disconnect(long id)
		{
			if (!connections.TryGetValue(id, out Connection connection))
			{
				return;
			}
			try
			{
				if (connection.Socket.State == WebSocketState.Open)
				{
					await connection.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).ConfigureAwait(false);
				}
			}
			catch
			{
			}
			Teardown(id);
		}

		private void Teardown(long id)
		{
			if (connections.TryRemove(id, out Connection connection))
			{
				connection.Cancellation.Cancel();
				connection.Cancellation.Dispose();
				connection.Socket.Dispose();
			}
		}
	}
	public static class Splash
	{
		private static readonly string[] SPLASH_DIR = new string[2]
		{
			8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.di(),
			8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.du()
		};

		private const string SPLASH_FILE = "Splash.png";

		private const string BACKUP_FILE = "Splash.original.png";

		private static string SplashDir(string build)
		{
			string text = build;
			string[] sPLASH_DIR = SPLASH_DIR;
			foreach (string text2 in sPLASH_DIR)
			{
				text = Path.Combine(text, text2);
			}
			return text;
		}

		private static void EnsureBackup(string dir)
		{
			string text = Path.Combine(dir, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dr());
			string text2 = Path.Combine(dir, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dS());
			if (File.Exists(text2) || !File.Exists(text))
			{
				return;
			}
			try
			{
				File.Copy(text, text2);
			}
			catch (Exception ex)
			{
				throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.ds() + ex.Message);
			}
		}

		public static object SplashStatus(string build)
		{
			string text = SplashDir(build);
			return new
			{
				supported = Directory.Exists(text),
				custom = File.Exists(Path.Combine(text, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dS()))
			};
		}

		public static async Task SplashApply(string build, string url)
		{
			string dir = SplashDir(build);
			if (!Directory.Exists(dir))
			{
				throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.FI());
			}
			EnsureBackup(dir);
			byte[] array;
			try
			{
				array = await Net.FetchBytesResilient(url).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Fi() + ex.Message);
			}
			if (array.Length < 8 || array[0] != 137 || array[1] != 80 || array[2] != 78 || array[3] != 71 || array[4] != 13 || array[5] != 10 || array[6] != 26 || array[7] != 10)
			{
				throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.FJ());
			}
			try
			{
				await File.WriteAllBytesAsync(Path.Combine(dir, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dr()), array).ConfigureAwait(false);
			}
			catch (Exception ex2)
			{
				throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Fj() + ex2.Message);
			}
		}

		public static void SplashRestore(string build)
		{
			string text = SplashDir(build);
			string text2 = Path.Combine(text, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dS());
			if (!File.Exists(text2))
			{
				throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dT());
			}
			try
			{
				File.Copy(text2, Path.Combine(text, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dr()), true);
			}
			catch (Exception ex)
			{
				throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dt() + ex.Message);
			}
			try
			{
				File.Delete(text2);
			}
			catch (Exception ex2)
			{
				throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dU() + ex2.Message);
			}
		}
	}
	public static class Updater
	{
		private sealed class Platform
		{
			[JsonPropertyName("signature")]
			public string Signature { get; set; } = string.Empty;


			[JsonPropertyName("url")]
			public string Url { get; set; } = string.Empty;

		}

		private sealed class Release
		{
			[JsonPropertyName("version")]
			public string Version { get; set; } = string.Empty;


			[JsonPropertyName("notes")]
			public string Notes { get; set; } = string.Empty;


			[JsonPropertyName("pub_date")]
			public string PubDate { get; set; } = string.Empty;


			[JsonPropertyName("url")]
			public string Url { get; set; } = string.Empty;


			[JsonPropertyName("signature")]
			public string Signature { get; set; } = string.Empty;


			[JsonPropertyName("platforms")]
			public Dictionary<string, Platform> Platforms { get; set; } = new Dictionary<string, Platform>();

		}

		private const string ENDPOINT = "https://prod-api-v1.stellarfn.dev/stellar/launcher/v2/update/{target}/{arch}/{version}";

		private static Release? cached;

		public static string CurrentVersion()
		{
			System.Version version = typeof(Updater).Assembly.GetName().Version;
			if ((object)version != null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 3);
				defaultInterpolatedStringHandler.AppendFormatted(version.Major);
				defaultInterpolatedStringHandler.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bA());
				defaultInterpolatedStringHandler.AppendFormatted(version.Minor);
				defaultInterpolatedStringHandler.AppendLiteral(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bA());
				defaultInterpolatedStringHandler.AppendFormatted(version.Build);
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
			return 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dV();
		}

		private static string Target()
		{
			return 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dv();
		}

		private static string Arch()
		{
			if (!Environment.Is64BitProcess)
			{
				return 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dW();
			}
			return 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dw();
		}

		private static int Compare(string left, string right)
		{
			string[] array = left.TrimStart('v').Split('.');
			string[] array2 = right.TrimStart('v').Split('.');
			for (int i = 0; i < Math.Max(array.Length, array2.Length); i++)
			{
				int num;
				int num2 = ((i < array.Length && int.TryParse(array[i], out num)) ? num : 0);
				int num3;
				int num4 = ((i < array2.Length && int.TryParse(array2[i], out num3)) ? num3 : 0);
				if (num2 != num4)
				{
					return num2.CompareTo(num4);
				}
			}
			return 0;
		}

		private static (string Url, string Signature)? Artifact(Release release)
		{
			if (!string.IsNullOrEmpty(release.Url))
			{
				return (release.Url, release.Signature);
			}
			string text = Target() + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.bW() + Arch();
			if (release.Platforms.TryGetValue(text, out Platform platform) && !string.IsNullOrEmpty(platform.Url))
			{
				return (platform.Url, platform.Signature);
			}
			Platform platform2 = release.Platforms.Values.FirstOrDefault((Platform entry) => !string.IsNullOrEmpty(entry.Url));
			if (platform2 != null)
			{
				return (platform2.Url, platform2.Signature);
			}
			return null;
		}

		public static async Task<object?> Check()
		{
			string current = CurrentVersion();
			string url = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Fk().Replace(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.FL(), Target()).Replace(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Fl(), Arch())
				.Replace(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.FM(), current);
			string text;
			try
			{
				text = await Net.FetchText(Net.BuildClient(), url, 2).ConfigureAwait(false);
			}
			catch
			{
				return null;
			}
			if (string.IsNullOrWhiteSpace(text))
			{
				return null;
			}
			Release release;
			try
			{
				release = JsonSerializer.Deserialize<Release>(text);
			}
			catch
			{
				return null;
			}
			if (release == null || string.IsNullOrEmpty(release.Version) || Compare(release.Version, current) <= 0)
			{
				return null;
			}
			cached = release;
			return new
			{
				available = true,
				version = release.Version,
				currentVersion = current,
				date = release.PubDate,
				body = release.Notes
			};
		}

		public static async Task Install()
		{
			Release release = cached ?? throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Fm());
			var (url, signature) = Artifact(release) ?? throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.FN());
			if (string.IsNullOrEmpty(signature))
			{
				throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Fn());
			}
			byte[] array;
			try
			{
				array = await Net.FetchBytesResilient(url).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.FO() + ex.Message);
			}
			if (!Minisign.Verify(array, signature))
			{
				throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Fo());
			}
			string staging = Path.Combine(Path.GetTempPath(), 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.FP() + release.Version);
			try
			{
				Directory.CreateDirectory(staging);
			}
			catch (Exception ex2)
			{
				throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Fp() + ex2.Message);
			}
			string text = Path.GetFileName(new Uri(url).LocalPath);
			if (string.IsNullOrEmpty(text))
			{
				text = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.FQ();
			}
			string artifact = Path.Combine(staging, text);
			try
			{
				await File.WriteAllBytesAsync(artifact, array).ConfigureAwait(false);
			}
			catch (Exception ex3)
			{
				throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Fq() + ex3.Message);
			}
			if (artifact.EndsWith(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.FR(), StringComparison.OrdinalIgnoreCase))
			{
				try
				{
					ZipFile.ExtractToDirectory(artifact, staging, true);
				}
				catch (Exception ex4)
				{
					throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Fr() + ex4.Message);
				}
				artifact = Directory.EnumerateFiles(staging, 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.FS(), SearchOption.AllDirectories).FirstOrDefault((string file) => file.EndsWith(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dX(), StringComparison.OrdinalIgnoreCase) || file.EndsWith(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.FK(), StringComparison.OrdinalIgnoreCase)) ?? throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.Fs());
			}
			Launch(artifact);
		}

		private static void Launch(string artifact)
		{
			string text = Environment.ProcessPath ?? string.Empty;
			try
			{
				if (artifact.EndsWith(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dX(), StringComparison.OrdinalIgnoreCase))
				{
					string text2 = (string.IsNullOrEmpty(text) ? string.Empty : (8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dx() + text + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.BD()));
					string arguments = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dY() + artifact + 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dy() + text2;
					System.Diagnostics.Process.Start(new ProcessStartInfo
					{
						FileName = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dZ(),
						Arguments = arguments,
						UseShellExecute = true,
						CreateNoWindow = true,
						WindowStyle = ProcessWindowStyle.Hidden,
						Verb = 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.dz()
					});
				}
				else
				{
					System.Diagnostics.Process.Start(new ProcessStartInfo
					{
						FileName = artifact,
						UseShellExecute = true
					});
				}
			}
			catch (Exception ex)
			{
				throw new CommandFailure(8EC410F1-6E77-4690-9E9E-B3CEE3479BCC.EA() + ex.Message);
			}
		}
	}
}
namespace <PrivateImplementationDetails>{06AF9570-DB29-425E-BDE9-AA3CBE8EC79D}
{
	[StructLayout(3, CharSet = CharSet.Auto)]
	internal class 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC
	{
		[StructLayout(2, Pack = 1, Size = 9589)]
		private struct 2
		{
		}

		internal static 2 3/* Not supported: data(D1 D0 88 CB DB C6 C0 C9 82 9E 80 DA 96 DA 88 85 DC D2 D4 DC F7 D1 D8 D8 CA 93 8D 91 CD 86 C9 99 AA FF E7 FD EF E3 CA E4 EE E6 F3 A1 BB A7 FF B7 E7 B7 B8 FD F1 E8 F2 F1 FD F2 F4 F4 F2 B7 A9 B5 91 D8 95 C5 CE 9B 83 99 83 8F C0 DC C6 9C D0 98 D6 DB 8B 92 97 8F 8C 98 96 D3 CD D1 8D C2 89 D5 B7 B6 B3 B2 EE AD B9 A4 AE A7 E0 FC E6 BC F4 B8 FA A6 A5 A2 A5 FF AA FD EF F3 AB E1 AB FB F4 B6 46 42 4D 47 5A 70 45 49 02 1E 00 5A 17 5A 04 58 47 40 43 19 5D 52 58 1D 0F 13 4B 01 4B 1B 14 5B 65 65 6B 6C 2E 32 2C 76 33 7E 2C 21 67 75 63 76 3A 26 38 62 2C 62 3C 60 6F 68 6B 31 66 7E 70 35 57 4B 13 59 13 43 4C 0C 01 17 09 17 0F 13 1D 45 47 5B 03 48 03 5F 01 00 09 08 50 05 0F 07 11 55 77 6B 33 79 33 63 6C 29 23 37 21 61 7B 67 3F 74 27 7B 25 24 25 24 7C 3E 3D 37 35 71 6B 77 2F 65 D7 87 88 DB CB CE DF C2 CC 83 9D 81 DD 96 D9 85 C7 C6 C3 C2 9E CC C9 CD C2 DC C2 C5 D3 D3 94 88 AA F0 B8 F4 A2 AF EF F8 F1 F7 EF EC A6 BA A4 FE AB E6 B8 E4 E3 E4 E7 BD F3 E5 F1 F8 FA F6 F6 F9 8F CB D5 C9 95 DF 91 C1 C2 95 85 93 95 8E 8B 8B DA C6 D8 82 CF 82 D0 DD 91 86 82 83 93 99 80 A3 AF B9 BB A0 A1 A1 EC F0 E2 B8 F2 BC EA E7 A0 A4 AE BE F8 E4 FE A4 EF A0 FE F3 B2 BE B2 AE F4 E8 0A 50 1C 54 0E 52 51 56 59 03 54 01 1B 07 5F 15 47 17 18 50 5A 1F 01 1D 49 02 4D 1D 16 52 42 50 64 7F 28 34 2E 74 3E 70 2E 23 70 60 7F 6B 6B 64 7E 3B 25 39 65 2C 61 3D 6F 6E 6B 6A 36 63 34 28 4A 10 58 14 42 4F 05 09 42 5E 40 1A 57 1A 48 45 15 10 58 44 5E 04 4E 00 5E 53 06 10 1A 02 11 55 77 6B 33 7A 33 63 6C 28 30 31 2F 33 66 7A 64 3E 6E 26 78 24 23 24 27 7D 21 3B 3F 23 22 34 21 21 8A 96 88 D2 9E D2 80 8D CB C7 80 9C 86 DC 95 D8 96 9B CB CD DF CB D9 9D 8F 93 CB 83 CB 97 C9 C8 F1 F0 A8 EA E1 EB E9 AD BF A3 FB B1 FB AB A4 F6 EE FF F7 EC EA BF A1 BD E9 A2 ED BD B6 E4 E0 F1 8F 99 9A C9 D3 CF 97 DF 9F C3 9D 9C 95 93 81 89 96 9A 8A 8F CC 99 8E 92 9C 87 95 9F 92 93 9D 86 BE E4 F7 A5 AF AD A9 A1 FF AE A1 A8 A8 E8 AB B3 BF A9 B4 B8 A7 E0 B0 BC B0 B6 BC EC B9 A1 B1 A7 46 4A 51 7A 5A 4A 40 41 43 51 49 42 49 49 57 4C 59 54 56 17 57 5C 53 50 53 5A 5E 55 53 41 1A 46 7E 6E 64 65 6F 7D 22 61 63 76 6E 62 6E 62 76 66 72 7E 7B 72 41 79 75 71 77 4C 75 69 7F 64 60 66 09 03 0D 0A 05 30 0A 04 0E 06 3F 04 1E 0E 17 11 09 24 19 17 1A 20 0F 14 08 16 14 14 1A 12 00 10 15 2D 21 25 2B 3C 3F 28 23 31 23 29 19 21 2B 37 05 2D 3D 2B 2D 36 33 33 3E 32 25 3F 35 3F 30 3A DD C5 C4 C6 CF CB F3 CB CB CF C5 D2 C3 DF CD D1 E5 DA D4 D5 CD D0 DF D6 D7 C7 EF D4 D8 D4 C6 CC FA FF FB E6 ED E4 E9 F9 DD E7 E5 E2 F4 FE F4 F1 E9 FE FB EC EC F6 E8 E4 CD E0 E4 F0 E2 E2 E7 F4 9A 9B 84 90 B1 8A 94 8E 8E 96 93 88 89 89 97 8A 8A 9E 96 A6 8D 92 9D 8F 86 AC 91 81 86 A8 97 9A A4 BF BA A6 A2 A0 BA A8 B0 AF A1 B8 99 B4 AC AA AD B4 AE BC AC B3 BD A4 8D BB B9 B5 B3 B8 A2 B0 58 47 49 50 71 5D 49 5E 4B 59 45 4E 50 42 56 49 5B 42 67 4A 5B 4B 63 4D 5D 40 59 45 5F 58 5A 5A 7C 6E 7A 65 6F 76 53 7E 67 77 5F 68 68 73 61 77 7B 78 6C 70 68 7A 73 6B 77 61 7C 70 6F 48 62 7C 19 02 0A 05 0B 1C 1C 01 03 10 08 3E 15 13 05 11 0F 08 0B 09 12 1E 0F 15 2D 12 00 01 1A 0E 07 05 26 2A 3B 21 11 3D 29 3E 36 2C 32 24 22 28 33 2B 36 34 39 3D 3B 2D 03 2B 37 21 23 38 39 39 27 31 C5 DC C6 C5 C1 CE C8 C8 D0 FC C9 CF D5 D3 C5 C9 D6 DF D7 CE D0 D3 D3 DC D6 D6 C2 EE C0 D2 C6 DC EC F2 EC E6 F9 E1 E0 E2 E3 E7 E5 F3 D9 F7 E5 F0 E9 FE FC F6 E9 F1 F0 F2 F3 F7 F5 E3 C9 E5 F1 E6 9F 86 8D 8D 81 98 82 81 8D 82 84 84 94 B8 87 84 94 98 9D 95 9A 90 8B 93 9E 9C 91 95 93 85 AB 86 BE AA BC BC BD AB A3 BA AC AF AF A0 A2 A2 B6 9A B8 AE AB A0 BF AF AC 82 A4 B6 A2 A2 BF B8 BA B4 5A 5B 77 47 4F 42 49 40 4B 44 52 40 52 4E 4B 4B 65 4B 4D 55 52 48 55 53 56 5C 47 6E 5B 5E 5A 5C 67 62 72 6C 79 66 62 69 6D 74 5F 74 68 6A 6D 6B 73 76 71 63 7B 68 75 73 76 7C 67 4E 7B 76 6C 7C 07 02 12 0C 19 06 02 09 0D 14 3F 14 08 0A 05 1D 13 16 11 03 1B 08 15 13 16 1C 07 2E 05 1F 1B 02 3D 22 26 2D 21 38 13 25 2B 27 25 36 2F 29 20 2A 2D 04 3B 35 31 2C 39 2A 3B 3D 34 3E 21 08 27 30 DE F4 CE C6 CD DA DF DA CB CD C4 CE D1 F8 CD D6 E5 CD D1 CA D7 DD D0 D8 C5 DA DE D5 D9 C0 EB DC F9 D4 E5 E0 E0 E6 E1 E4 F8 E6 E4 F6 EF E9 E0 EA ED C4 EB FC EA C0 E8 F4 E6 FF F5 E6 FF F9 F0 FA 9D B4 9B 9D 8F 9D 98 B2 86 91 81 86 91 8E 8A 81 95 8C A7 8A 9B 8B A3 94 95 9D 9F 83 93 A8 97 80 B8 B8 A7 BB 91 AA BA A8 AC B7 B3 A5 AF A6 A8 AA BD 84 B7 A9 BB B1 B8 B4 B3 BF BF B6 89 A4 B5 A3 4F 4F 41 48 42 40 4B 72 4F 46 53 52 47 40 41 41 53 5A 54 56 59 60 5D 4E 59 40 58 54 5A 5B 6B 5A 7A 6E 66 7A 66 6A 60 61 5D 66 78 64 65 72 70 60 75 6B 7D 77 7B 6D 43 6F 77 65 75 70 7A 67 66 7A 09 0E 1B 1A 31 0A 14 04 16 13 12 0E 05 02 17 16 25 09 1D 15 1F 0A 12 1E 1A 06 00 15 17 03 11 07 15 28 20 2C 2D 24 39 3D 26 22 34 24 34 18 20 2A 2D 35 34 36 3F 3B 29 2D 36 32 24 34 24 08 3D 3B D9 DF C9 C5 C2 DC C4 C2 D0 D7 C3 D4 D2 F8 D6 C0 DD D2 CB CD DB CD CF D5 DD C1 C4 D2 C3 C3 EB C0 E4 F9 ED EE E7 FC F8 E8 F0 F0 E8 EE F4 F3 E7 F0 EE C4 F1 EA C1 ED F9 FA FB E0 E4 F4 E4 F2 F0 F1 8F 8E 98 B6 82 86 82 86 BD 80 95 93 94 82 8A 91 9E 9E 9D 89 A1 93 95 93 99 AC 82 94 91 9E 87 81 AF B9 BF BA 91 AC A3 A3 AC A6 A3 B5 B1 B4 9B B6 BF B5 BC AE AD 80 B8 B4 A1 B0 BF BF B8 B2 B7 A1 4E 59 58 4A 71 5C 58 4C 50 57 44 53 56 44 7B 56 4E 54 48 5D 4C 4F 5F 62 41 56 44 6E 57 54 40 5C 7C 62 7C 70 6A 7D 7C 6E 5D 60 6C 64 67 75 5B 64 79 6F 71 6F 77 6B 65 69 7B 67 7C 74 72 72 72 74 1F 07 1C 39 0F 1B 04 00 17 0F 14 08 16 0B 01 01 13 09 1D 1A 0A 10 0E 04 21 16 1C 14 15 03 54 14 6A 2D 27 25 2A 2A 3E 1E 27 2F 25 22 32 67 25 65 3C 32 34 3C 31 29 39 2F 3E 32 29 7B 78 3B 3A 3E D9 DF CD C5 C2 CE DE 83 C7 DB C5 AC AC AD D4 D0 D8 D7 D1 DA 9E D4 D9 C4 92 D1 DC DE D5 DC 94 DC F9 AB E5 E0 FD FC E5 E3 E5 A3 E9 F5 F5 A7 F4 E4 E3 F7 F7 F8 FA BF F0 F4 FC F6 E0 E4 F4 FB FD F6 CA 80 8D 90 CE 86 9F CD 8F 82 8C 87 89 95 89 80 9E 9F AF CC CE 9C 92 AB 88 97 B7 A7 9D BE B3 BB BC A9 9F F8 A2 AD A2 9C F4 8A 87 F0 B6 A5 A9 A9 A0 BA 8F BD AB 96 94 9F E3 8A BD A9 A6 8E AD 97 58 71 70 42 18 66 68 78 5B 6E 74 73 61 76 5E 6C 0A 75 6C 73 78 71 79 77 71 7C 74 68 7D 62 58 51 5E 59 3B 5C 7C 5A 60 43 49 56 48 65 50 51 5C 4F 6C 78 73 6E 2F 49 4B 6E 20 71 47 57 5A 5C 24 7E 5A 31 3F 23 3D 0B 34 37 37 00 37 3B 3F 04 0F 50 34 1F 20 1E 4F 29 24 19 13 21 40 2B 24 3A 2C 33 3B 2E 23 7D 38 2B 09 3A 09 06 04 04 22 33 36 30 29 2F 3D 3D 7E 3C 33 30 3F 36 3E 25 6C 77 76 26 DE CE C4 C5 CF DD 81 DE CA CC D2 D5 C5 D2 D0 D6 C9 D3 D7 CB CA DC C9 C9 88 9C 9F E1 C4 D2 C7 C6 EF EF EB FD FC E3 E1 E8 F6 E2 E3 EE E8 F3 F6 EA F6 F8 F7 F4 F3 FE F2 F9 F1 FC FD FC F7 F9 F0 FA 98 88 87 87 9A 9D 83 81 81 8E 84 8E 94 84 90 97 96 9A 94 8D 89 96 92 92 82 87 99 9E 98 84 9C 9C AC BF BB BC BE AA BE 86 A7 BA 84 A8 A1 AE B0 83 A9 AB B9 BA BB BA B2 A9 B7 A1 A2 B8 B1 BF A0 A5 4B 5E 5B 4C 5C 4A 58 58 50 4D 45 52 45 46 54 40 5E 5E 54 5C 4A 5A 55 53 41 56 42 45 46 56 53 50 7F 7B 7C 68 6C 6A 7F 6E 67 6D 64 63 67 64 6F 76 6A 7A 7B 7C 7F 6D 6E 72 65 77 7F 66 78 76 66 67 05 1C 04 0C 08 1B 04 02 0F 06 04 0E 11 09 08 00 1C 0F 08 18 19 1A 18 12 05 1D 13 10 06 04 18 1A 29 20 3D 39 2F 3D 3E 22 35 36 30 2F 33 2A 28 2A 39 30 39 2B 2C 30 2B 2F 3B 34 38 25 25 34 26 3A C6 C7 C4 C6 CD C4 DC DF CB CD D4 D2 C5 D5 C1 C0 D4 9B D1 CA 9E D1 D3 C9 92 D2 90 C2 DE D8 C6 C1 E9 FE FC A9 D9 E6 E2 E9 ED F4 F3 A1 F3 E9 E0 E0 E8 E8 EC F8 F0 FB EF DA FE FC F2 F0 FA CB F0 F0 9C C5 9B 9D 8B 83 80 8C 90 CD 8C 80 93 89 87 8D 9F 89 D6 8A 8A 9A 90 91 93 81 DD 9D 97 82 9A 96 A2 AE BA E4 BD A6 A2 AA AE A6 ED A8 A8 B4 B0 A4 B4 B8 BD AA AA BA B0 B1 B3 A1 FD A2 BF B9 B3 B9 4F 06 41 47 5D 5B 4D 43 41 46 73 4E 40 53 53 44 48 5E 64 7A 52 5E 4F 4E 57 40 6C 64 64 7B 0E 60 58 47 28 59 7C 60 78 62 61 6C 6C 45 63 61 65 70 76 6F 51 7A 71 71 3E 31 22 60 78 74 7A 7B 48 7A 1A 0E 06 35 0D 00 01 00 03 0D 04 43 46 45 41 54 58 1E 0E 1C 10 0B 08 18 1F 1A 04 18 18 01 1B 1E 2F 3B 29 30 22 20 2D 29 2B 27 23 2C 22 26 36 22 29 3E 20 29 3B 3C 28 38 36 73 31 3F 76 36 26 27 CB D2 88 C6 C8 8F CE D4 D6 C6 D3 C9 D2 D3 D4 9F 95 94 89 8B 89 91 8C 93 82 9D 81 8B D7 C7 C4 D9 E3 E8 E9 FD E7 E0 E2 A2 ED E0 F4 E4 F2 AA F7 F1 E8 FE F9 F4 F0 F0 BC F1 FD FC E0 F3 F7 F4 FF B5 9A 84 9A 9D CE 98 8D 9E C2 82 96 80 8F 8B 85 87 96 9E D8 9F 91 8D DC 89 9A 96 D0 9D 97 82 9A 96 A2 AE BA EE BD EF AF A2 AC B7 A5 AF B2 E7 B7 A0 A8 AD BD AB AD AB B9 B1 BE B2 A2 FC B5 B8 BA A1 4F 45 5C 66 65 61 43 59 02 6E 4F 45 4F 41 4D 40 5E 75 57 4D 1E 79 53 48 5C 57 78 65 62 67 1B 04 24 3A 28 4A 61 61 78 68 6C 77 2D 55 7F 77 61 3F 3A 58 77 77 6A 7A 72 69 3F 5F 75 7F 71 63 7C 2F 4A 2E 3C 08 09 55 4C 2E 03 00 08 04 4B 24 0B 0B 0E 09 17 15 44 5F 12 12 5F 10 11 12 1E 12 04 00 28 27 21 2A 62 6F 21 2C 3A 6E 21 26 23 7A 77 74 6F 68 6E 69 6E 6F 70 7D 3B 3E 3D 24 22 36 36 39 CF E8 C9 CA C6 CA 81 EE CD CD D4 D3 C9 CB 9E 85 D4 D4 95 CA CA D0 CE D8 BF B9 F1 D2 D5 D2 C7 C6 A7 C8 E7 E7 FA FD E3 E1 AF C2 EC ED E9 F0 A9 CA E8 F2 FF F0 F0 A5 BC B7 9F 99 D3 FE F8 F9 F1 F6 9E 82 87 87 D4 CF 87 88 87 93 CD 80 8A 8E 92 80 99 97 97 8A 9B B4 99 98 82 DE B1 9D 9F 81 91 CF EA BF A1 A4 AB A0 B9 B9 FF F2 F2 F1 CB CD 9B 9A BB A8 AB BC AA AF BD A9 BA B2 A3 A2 B3 A3 A7 FA 43 45 4C 4C 56 01 44 59 4F 4F 58 0C 08 4F 50 48 56 4F 5D 41 4A 10 54 49 5F 5F 0B 11 55 5F 55 47 79 6E 7C 34 7B 7B 6A 20 3A 2D 6A 72 72 62 7C 71 35 71 79 6F 7F 6C 7F 6F 7B 63 64 2A 36 74 7C 74 18 18 0D 1D 53 1A 18 0B 4F 5B 4E 0C 0C 14 4A 06 09 08 0C 1C 06 0B 53 1E 01 00 4B 51 15 1F 15 07 39 2E 3C 74 3B 3B 2A 60 7A 6D 2A 32 29 29 25 35 2A 37 31 3A 3F 2B 35 32 3C 7C 3A 22 39 39 6F 75 C9 C3 C9 DB DD CA D8 90 D7 D7 C6 8C 9E 89 D4 CB DD D2 D5 D8 D9 DA 93 CD DC D4 9E DB C6 D0 DD D8 EB EC ED A6 E4 FF E9 EA AC E9 F0 E4 E1 A9 E3 EC FC F2 F5 F8 F9 FA B3 FA FB F5 BE E2 E0 F0 FD F8 8B 8C 8D C6 9D 99 8B C6 9A 8E 8C CF 91 82 86 95 93 96 99 9E 9B D0 8B 98 90 83 DE 93 9B 87 9D 98 AB AC AD E6 AC A2 BC E3 AB A0 AF A8 AB A6 A3 A0 F5 A3 F5 B0 BD B0 B2 F3 A5 BC B6 B7 B0 B8 BA A1 05 5C 47 4F 48 01 5B 42 44 45 12 47 49 49 50 0A 4D 54 5E 5F 0C 11 48 49 54 55 5F 5F 42 18 40 41 6C 25 67 7D 68 69 63 63 76 2C 6F 75 60 29 69 75 2E 6D 71 7D 7B 70 33 70 62 27 3E 66 73 75 79 63 03 0F 0D 06 41 18 09 0F 0F 4D 0D 11 55 06 11 01 13 14 57 14 0E 1A 1B 53 05 12 06 10 03 13 1D 1A 65 3C 29 3F 60 3B 34 39 36 26 38 35 69 37 28 24 33 35 63 79 3D 37 3D 2F 21 36 24 6C 23 23 32 78 92 85 C5 C8 DE 81 DB CC D1 CE C1 D1 D6 CB CD C6 DB CF D1 D6 D0 90 CB DC C1 DE C3 C5 D3 DB D8 D4 F8 A6 E5 E8 E7 E1 F8 E8 EC E2 EE E2 E3 C2 C6 D2 FF F9 CE F0 FB E8 D8 F8 F4 F2 E5 FD E2 DB FB F6 8B 87 C8 BA 9A 80 9E 8C 85 86 8C 84 90 82 88 81 98 A4 90 8D 8A 8F C6 D2 DD 87 91 84 84 9E DA 99 A5 A8 A9 A5 A6 A0 BF B9 C2 E9 EE AD A2 A5 EE EB B6 B4 BF BA B1 B2 AC AF B7 A0 A3 B8 B9 B9 A4 A7 4F 5B 49 5B 47 41 4B 4C 02 47 4F 56 48 4B 4B 44 5E 1B 51 4A 1E 5E 50 4F 57 52 54 48 16 45 41 5B 64 62 66 6E 60 60 2C 69 6D 74 6E 6D 69 66 60 25 73 68 38 6B 6B 71 72 74 7C 74 56 7E 64 63 7A 7C 1E 0E 0C 06 19 01 00 02 03 07 5A 11 14 08 03 17 1F 08 0B 3B 35 3D 31 3F 35 31 24 33 30 45 54 1D 3E 3F 38 3A 74 60 63 2E 26 2D 72 6F 27 29 20 37 6B 2C 2F 77 3A 3A 2A 72 24 36 22 22 3F 38 3A 26 84 C1 DB C6 C0 84 87 86 F0 C6 CC C4 C7 D4 C1 88 E8 DE D4 DC DF CC D9 90 9F F0 FC 9C D5 D8 C1 D9 EE AB E6 E6 FA AF FE E8 E3 E7 A0 F5 EE E2 A4 F3 FF E9 EB F0 F1 F1 BC F1 FB E0 E4 AB B6 E3 FC F0 CA 9D 8D 9B 9D 86 83 83 C2 8F 89 92 92 C7 93 84 89 DB 9D 94 8E 8B 85 D3 DC 9E 91 9F 9F 91 91 86 BE EB AB A6 A0 BB AD A4 AC B0 E0 A0 A8 E7 B1 AB A9 BA BE BC FE AF BD A9 BA E9 F0 B2 B9 A2 B8 B1 0A 45 47 5D 0E 5D 49 4C 46 03 54 49 43 07 49 44 54 52 5E 5C 4D 4B 06 1D 5F 52 5E 58 50 52 47 41 2A 68 67 67 7A 6E 65 63 71 23 61 6F 26 6E 6A 73 7B 77 71 7D 3E 7C 74 68 7C 78 30 78 72 2D 34 76 02 1E 06 02 4E 4F 04 0C 11 43 01 41 14 02 12 00 08 08 1D 1D 5E 0D 1D 13 15 16 04 19 13 57 19 14 24 22 2E 2C 3D 3B 6C 25 23 27 60 2F 29 67 27 2D 2F 35 33 2A 13 30 26 34 3E 3F 31 7E 63 79 64 75 82 FC C1 C7 CA C0 DB DE 82 ED F4 81 97 97 8A 95 81 9B EF D0 D0 89 88 86 92 CB 86 85 9F 97 F5 C5 FA E7 ED DE EB ED C7 E4 F6 AC B5 B2 B1 A9 B7 B3 BA B3 D3 D1 CA D2 D0 B1 B2 FF F9 FA F3 B7 D3 F0 89 80 87 C0 CE AC 84 9F 8D 8E 85 CE D7 D5 D1 CB CA D5 C8 D7 CE DF AF 9C 94 92 82 98 D9 C2 C7 C2 E4 F8 FE 88 AD AC A9 BD B6 E9 EF EB 87 A4 A7 A0 AA AF F5 95 BF B1 BB A8 B3 B4 B5 B4 B8 FA 81 86 06 4E 46 12 5F 12 1C 03 1B 50 45 42 0B 44 4C 08 4F 5A 1A 7A 56 4D 53 50 5B 46 5D 13 0D 41 09 17 3B 39 3D 2B 22 2F 2E 43 6D 77 2E 40 29 45 76 64 74 7F 3A 22 68 22 3E 2F 26 31 3C 31 34 50 7B 7A 0D 07 0D 49 2D 07 1E 02 0F 06 42 5A 10 5A 46 54 48 4E 5A 0A 1B 1C 51 1E 1A 5E 05 10 5B 1A 1B 17 23 27 2D 76 7E 3C 29 2E 6F 20 28 6C 33 26 69 35 36 3A 2C 3F 31 2D 31 7F 05 3A 3E 35 39 20 27 77 F9 CE CB 84 E8 CA D8 CE CA 8E F3 C8 D2 C2 CA CA D4 DE EB DC DD 92 FA D8 C6 D0 D8 9C FB D8 D0 D0 E9 E4 FA FA DD EA EF A0 C4 E6 F4 E2 EE AA C0 E0 E9 EF FD F4 EE EB E5 A7 BD BC D3 FE F8 E3 F1 FB 9E C6 BA 88 80 88 89 8E 84 CE 8D 88 92 8E 83 84 8E 9E 9C 91 8A 92 90 C1 CE D2 B4 BE B5 A3 AD A5 8F F7 A0 BD A3 A3 86 B8 B1 B7 E0 A0 E6 AA AB A8 BF B5 AC BA B6 BE B0 B1 B7 BD B7 B4 B5 B1 F9 BD 5E 5F 58 5A 14 00 03 1C 0C 12 0E 10 08 16 0B 41 54 48 15 48 4B 5A 4E 44 5A 47 44 41 45 0D 1B 1A 3B 25 38 27 3E 21 3D 22 66 6D 73 2C 77 72 61 77 63 73 6C 6D 6E 6C 26 32 3D 2B 3E 29 38 2F 3A 2D 45 19 0D 1A 01 03 1A 08 0A 17 14 11 15 5D 4B 4A 42 55 40 57 4A 51 48 52 00 16 03 1E 1A 01 11 5B 39 3F 2D 25 22 2E 3E 60 26 2C 37 2F 2A 28 25 21 74 31 2B 36 30 71 36 2E 3D 3D 7E 25 3B 27 37 3A DF C7 CC 89 C0 C0 D8 8D D1 C2 D6 C4 86 C3 CB D2 D4 D7 D7 D8 DA 9F CC CF DD D4 C2 D4 C5 C4 8E 95 A4 EF E4 EA E1 FA E0 E9 A2 ED EF F5 A6 E4 F6 E0 FB EF FD B9 A4 BF FF F2 E7 FF F4 B1 F8 F8 E0 B5 85 9B 8D 87 CE 8C 83 98 8E 87 C0 8F 89 93 C4 96 93 81 9D D9 9D 90 89 91 96 D3 9E 9E 82 D7 83 87 A3 BF AD E9 A8 AE A5 A1 A7 A7 E0 B5 A9 E7 A0 A0 B6 BE AC BC FE F8 FB E7 F2 B6 BE B2 A4 AE A4 A1 43 44 46 09 48 4E 45 41 47 47 66 40 4F 4B 41 41 1A 4F 57 19 4D 4B 5D 4F 46 13 63 45 53 5B 58 54 78 31 28 5E 67 61 68 62 75 70 20 62 69 72 68 61 3A 75 77 6D 3E 79 75 73 76 33 3E 31 42 7F 71 35 08 1E 01 05 0A 4F 0A 02 0E 07 05 13 46 0A 05 1C 5A 13 19 0F 1B 5F 1E 18 17 1D 50 1C 19 01 11 11 66 6B 3A 2C 20 2E 21 28 26 63 2F 33 66 37 25 37 2E 37 21 79 3A 3A 30 38 26 36 34 7F 10 36 3D 39 CF CF 88 DD C1 8F DF D9 C3 D1 D4 81 F5 D3 C1 C9 D6 DA CA 83 9E E8 D5 D3 D6 DC C7 C2 96 D5 D8 DA E9 E0 ED ED AE A1 AC C2 F2 E6 EE A1 D5 E2 F0 F1 F3 F5 FF EA BE EB F4 F8 FC B3 C3 F4 F5 E2 E6 FC 9E 92 C8 80 80 CF 98 85 87 C3 8C 80 93 89 87 8D 9F 89 D8 98 90 9B DC 8F 87 9D D0 85 9E 92 D4 93 A3 B3 AD BA E2 EF A3 BF E2 B7 B5 B3 A8 E7 AB A3 BC FB 8B B4 BF AD A8 FD 93 A3 A0 F1 95 B8 BA A1 58 44 44 07 68 4E 45 41 47 47 00 55 49 07 57 51 5B 49 4C 19 6D 4B 59 51 5E 52 42 0B 16 56 57 56 6F 78 7B 29 7A 60 2C 2D 75 62 73 21 62 62 6A 6C 7F 7F 36 39 5F 71 68 74 64 7A 62 64 65 37 79 74 13 4B 00 08 18 0A 4C 1C 17 02 12 00 08 13 0D 0B 1F 1F 58 10 0A 53 5C 12 00 53 04 19 13 57 18 14 3F 25 2B 21 2B 3D 6C 23 27 26 24 32 66 33 2B 65 28 2E 36 79 3F 2C 7C 3C 36 3E 39 3F 3F 24 20 27 CB DF C7 DB 80 E9 CD C4 CE C6 C4 81 D2 C8 84 D6 CE DA CA CD 9E EC C8 D8 DE DF D1 C3 8C 97 F1 C5 E3 E8 CF E8 E3 EA FF C1 E3 F6 EE E2 EE E2 F6 AB FF E3 FD B9 F7 EC BC F0 FB E0 E3 F8 F8 F0 BA B5 BE 83 8D C9 8C 9A 85 81 86 C3 89 92 C6 8E 8A 86 95 96 88 95 9B 8B 99 DD 9D 81 D0 90 D6 84 91 96 BF B9 A1 BD B7 EF B8 A2 AD AF E0 B3 A3 AA AB B3 BF BF F8 AD B6 BA FC BB BB BF B5 F1 FB F7 A2 B0 58 42 4E 50 0E 5B 44 48 02 4A 4E 52 52 46 48 49 1A 5A 56 5D 1E 4B 4E 44 12 52 57 50 5F 59 1A 41 62 6E 28 6F 67 63 69 2D 61 6C 75 6D 62 27 6A 6A 6E 3B 7A 7C 3E 70 6C 78 7C 76 74 50 78 78 60 7D 0F 19 48 19 1C 00 0B 1F 03 0E 40 08 15 47 0C 0A 16 1F 11 17 19 5F 08 15 17 53 16 18 1A 12 54 58 6A 28 24 26 3D 2A 6C 39 2A 26 60 26 27 2A 21 65 35 29 78 20 31 2A 2E 7D 33 3D 24 38 20 3E 26 20 D9 8B C9 C7 CA 8F D8 DF DB 83 C1 C6 C7 CE CA 8B ED D2 D6 DD D1 C8 CF 9D DB C0 90 D3 DA D8 D7 DE E3 E5 EF A9 EF EC EF E8 F1 F0 A0 AC A6 E4 EC E0 F9 F0 B8 F8 F0 EB F5 EB FB E1 E5 E2 B6 F8 E6 B5 98 9E 86 C9 88 9D 83 80 C2 82 C0 87 89 8B 80 80 88 DB 81 96 8B DF 93 8A 9C DD D0 92 99 82 98 91 EA A5 A7 BD EE AD A9 ED AD B3 A5 AF A3 A3 FE E5 F4 FB 9D B7 B9 B6 B2 B8 8E 91 B9 BF B7 A5 BD B0 59 77 7C 41 47 5D 48 7D 43 51 54 58 7A 69 72 6C 7E 72 79 65 70 69 5D 5B 46 56 42 5C 57 43 5C 69 5D 62 66 3F 3A 53 4B 4B 51 47 4B 5E 47 61 70 60 68 76 79 6D 76 40 50 74 70 3D 68 27 22 39 70 79 06 0D 09 00 02 0A 08 4D 16 0C 40 13 03 0A 0B 13 1F 5B 1F 1F 0D 1B 17 5D 13 15 04 14 04 57 54 14 3E 3F 2D 24 3E 3B 3F 77 62 06 2E 26 2F 29 21 19 18 32 36 38 2C 36 39 2E 0E 07 38 38 24 33 04 34 D8 DF D1 F5 E0 F9 E5 E9 EB E2 FC EF F0 C6 C2 D1 DF C9 D5 D8 CA D7 E0 EA DB DD 86 85 D0 D6 DD D9 EF EF A8 FD E1 AF EF FF E7 E2 F4 E4 A6 E3 ED F7 BA FD F7 EB BE F8 FA EE F6 F8 AA B1 D7 E5 F7 C9 AB 99 8B C7 8B 97 89 AB 8D 91 94 8F 8F 93 81 A2 9B 96 9D A5 BC 96 92 9C 80 9A 95 82 AA A0 9D 9B FC FF 94 8F A1 BD B8 A3 AB B7 A5 8D A7 B2 AA A6 B2 BE AA F7 BB A7 B9 9B BD A1 A4 BF BF A3 B1 92 4B 46 4D 75 6C 46 42 4C 50 4A 45 52 7A 70 4D 4B 0C 0F 64 7F 51 4D 48 53 5B 47 55 72 5A 5E 51 5B 7E 26 5F 60 60 39 38 20 51 6B 69 71 76 6E 6A 62 45 59 5D 37 7B 67 79 30 53 46 44 59 49 47 55 46 39 3C 27 3B 2A 52 41 19 5F 4E 05 11 0F 04 05 15 0A 46 3E 16 0C 0B 12 14 06 16 5D 14 06 1E 17 10 24 3D 75 19 3C 20 28 60 27 33 29 22 2A 28 27 24 36 3E 65 3C 30 72 29 2E 7F 3D 3F 33 33 7A 3A 3A DF CA CB 84 C0 C0 CF C2 C6 C6 C7 D4 C7 D5 C0 D6 97 DE C8 D0 DD CF D3 CF C6 D2 DC 9C D0 C5 DB D8 EC E7 B5 EC EF EC A1 EB EE F7 EF EA E3 E9 B9 B6 FE F9 AB FB FF AA F8 FE F0 F7 A2 F4 A7 A1 A3 A5 D9 8D DB D0 D9 D7 88 C0 81 82 8C 85 83 95 85 D8 9F 82 B2 91 9C B8 9F 94 BD 9A BA B7 A3 8D BD C4 84 A2 81 BA 87 A1 9E F8 A1 80 89 F7 8F AC B4 9D 8C 98 92 E0 F0 BA A5 97 BA 8A E2 9F A0 B3 83 E0 1A 73 1A 45 45 66 46 42 4B 7A 4D 74 13 7D 63 60 0B 62 42 73 53 66 51 6B 5A 7D 74 60 41 79 06 7C 73 51 62 58 79 55 5B 47 6A 5A 57 50 32 49 50 69 72 41 5C 48 77 53 5F 57 7C 49 47 24 7A 74 79 53 5A 31 3F 38 07 20 06 28 50 2E 1A 06 55 2A 30 06 03 35 02 1E 0D 36 11 33 1A 11 37 23 1A 14 19 33 02 2F 1F 25 25 06 26 22 2B 0E 3A 26 3E 0A 03 0C 6E 15 32 14 2A 12 31 18 60 1D 03 61 66 19 10 00 99 E7 FC C5 C7 E1 F8 CA D6 ED E7 F3 CE FE CE EB D3 F5 FC DE C7 E6 E8 DA 80 FA D9 C6 DF EE E3 FB DB E8 E5 B0 BC EE DB DF EE E0 E9 C8 B0 CE EF D3 F2 F8 AB F5 DC FD F2 CF E2 C2 A2 F9 FA CE CC C4 83 A7 AB A3 9B 8D DF BF 8E 80 99 A8 D0 AE 8D AC 89 B2 95 A3 96 9D BB 85 9B AA A7 BF 84 BE 9E 85 A7 92 9F B1 B4 95 94 FD EC 95 81 96 97 85 F2 F2 88 8F A0 B1 B7 88 93 A5 AA E4 94 93 BC B9 AE 91 44 73 51 50 6B 41 74 1A 6D 4F 4A 6B 4B 0A 4E 17 5E 03 00 7E 61 68 5B 4A 63 0A 47 43 73 01 58 42 47 4E 40 53 46 65 4E 69 33 4A 53 4B 62 52 4B 34 4F 4D 4D 68 75 79 50 79 47 26 7E 7E 70 55 45 66 47 18 03 00 1E 1F 0D 19 01 0B 03 09 03 04 0F 48 3B 2E 2C 31 21 33 33 3A 3B 3D 4D 5C 37 22 20 3D 15 1F 11 19 0B 72 29 35 21 2B 21 2F 21 22 27 2A 3E 3E 1E 36 2C 2B 32 34 26 36 13 3D 3F 32 3A 21 87 FC C1 C7 98 9B 81 FE CA CA D0 D1 CF C9 C3 8B DF C3 DD FF D1 CD C8 D3 DB C7 D5 FD D7 C2 DA D6 E2 EE FA A7 EB F7 E9 CB ED F1 F4 EF EF F3 E1 C6 F6 F2 FD F7 EA B2 CB F4 FC A5 A4 BC C5 FF FD E5 9A 82 86 8E B1 AA AD AE CC 86 98 84 A0 88 96 91 94 92 8C 9C BD 93 95 98 9C 87 DD A6 9F 99 C2 C1 E7 98 A0 A0 BE BF A5 A3 A5 9C 82 84 E8 A2 BC A0 9F BA AB A0 9F B1 A8 B4 91 BB B5 B0 A2 88 91 9A 79 05 4D 51 4B 6A 5C 44 41 74 45 43 6E 42 48 55 5F 49 16 5C 46 5A 7D 4F 51 1D 55 49 53 55 5B 41 7E 64 65 64 67 6B 68 61 67 77 6F 71 2B 6B 61 63 6E 48 41 4A 4A 5A 51 41 51 66 62 63 73 79 60 56 05 05 1C 1B 01 03 3F 08 16 3F 23 0E 08 13 16 0A 16 27 3B 30 22 2F 13 11 1B 10 09 04 18 04 01 05 3A 24 3A 3D 2B 2B 1A 28 30 2A 26 28 23 23 05 2B 3E 09 3D 29 2B 2B 3D 3F 3E 36 00 3E 3A 3E 37 2C F9 DF C9 DD CB C0 CA CB CD CD C5 D7 C7 CB D1 C4 CE D2 D7 D7 CB D1 D7 D3 DD C4 DE E2 C2 D2 D8 D9 EB F9 A8 E5 EF FA E2 EE EA E6 F2 CD E7 F2 EA E6 F2 FE EA B9 F8 F0 F0 F9 F7 E1 D2 E4 FF FB F0 D4 98 88 A6 86 9A 87 85 83 85 C3 94 8E C6 82 9C 86 96 8E 9C 9C DE 86 99 89 DC D3 B4 9E 81 99 98 9A AB AF E8 A8 EE AD B9 A4 AE A7 E0 A7 AF B5 B7 B1 F4 9A BC BD F3 92 AC 8D A0 B6 B6 B4 A4 B2 BA B6 4F 0B 05 6C 56 4C 40 58 51 4A 4F 4F 76 46 50 4D 1A 17 48 56 49 5A 4E 4E 5A 56 5C 5D 1B 79 5B 65 78 64 6E 60 62 6A 21 43 6D 6D 49 6F 72 62 76 64 79 6F 71 6F 7B 32 59 65 77 70 65 65 7F 78 7A 45 05 07 01 0A 17 2D 15 1D 03 10 13 4C 25 08 09 08 1B 15 1C 2A 0A 1E 0E 09 5F 23 02 1E 15 12 07 06 6A 3B 27 3E 2B 3D 3F 25 27 2F 2C 61 6B 11 21 37 38 7B 0A 2C 30 1E 2F 7D 7F 04 39 3F 32 38 23 06 DE D2 C4 CC 8E E7 C5 C9 C6 C6 CE 81 8B F0 C5 CC CE 9B 95 F8 CC D8 C9 D0 D7 DD C4 FD DF C4 C0 95 AD A6 C6 E6 DE FD E3 EB EB EF E5 A6 AA A0 A9 CB F5 F5 D1 F7 EA FA EE FC F1 E7 F9 E7 F3 B0 B8 B2 C7 AE 90 8C 8D 9A 98 84 8D 8D B0 8E 8A 8E 87 9C DD D7 DF BB 87 8F 9D 8E 81 D4 DC D6 DB B4 9B 98 A7 AA A6 AD E9 E3 EE AD E0 B3 AF B6 A3 B5 B7 AD BF B7 B4 F9 BA B6 B8 FD BC BC A4 F1 A5 A3 B5 A7 5E 68 47 5C 42 4B 0C 43 4D 57 00 53 53 49 04 72 53 55 5C 56 49 4C 1C 6E 57 50 45 43 5F 43 4D 0F 2A 5C 61 67 6A 60 7B 7E 22 50 65 62 73 75 6D 71 63 3B 6A 7C 78 6A 6F 78 76 33 64 79 73 37 77 7D 0B 05 0F 0C 54 4F 3B 04 0C 07 0F 16 15 47 37 00 19 0E 0A 10 0A 06 5C 0F 17 15 05 02 13 13 54 01 22 2E 68 2A 26 2E 22 2A 27 6D 60 00 36 37 36 2A 2C 3E 78 2D 36 3A 7C 3C 36 3E 39 3F 76 27 26 3A C7 DB DC 89 CF C1 C8 8D D6 D1 D9 81 C7 C0 C5 CC D4 95 CF D0 D0 DB D3 CA C1 D7 D5 D7 D3 D9 D0 D0 F8 B1 A7 A6 FD E2 ED FF F6 E2 F0 F1 E5 E8 EA F1 E8 F4 F4 DA F1 EA F0 F9 B2 FD FF E5 B6 F8 E4 F0 84 CB BF 80 80 8B 83 9A 91 C3 B3 84 85 92 96 8C 8E 82 C2 D9 AD 8F 90 9C 81 9B DE 81 98 90 A7 85 A6 AA BB A1 E0 A0 BE A4 A5 AA AE A0 AA E9 B4 AB BD B8 B7 AC B2 BB FC B3 BD A7 F0 B3 B7 B4 BF F5 5F 5B 08 5D 46 4A 0C 42 50 4A 47 48 48 46 48 05 49 4B 54 58 4D 57 06 1D 46 5B 55 43 53 17 5D 46 2A 65 67 29 61 7D 65 6A 6B 6D 61 6D 26 74 74 69 7B 68 70 39 6D 7E 6A 78 76 33 76 7E 64 37 60 7D 03 18 48 0B 1B 06 00 09 01 0C 15 0D 02 47 0A 0A 0E 5B 0A 1C 0D 0B 13 0F 17 53 04 19 13 57 07 05 26 2A 3B 21 74 6F 2F 22 37 2F 24 61 28 28 30 65 39 37 3D 38 2C 7F 28 35 37 73 32 30 35 3C 21 25 90 8B FB D9 C2 CE DF C5 92 8D 90 8F 96 D0 CD CB DE D4 CF CA D7 89 84 8B CA 8B 86 EE 80 83 9A D8 F9 E2 A8 AF A8 AF FF F9 E3 F1 F4 A1 A4 A5 A4 A7 B5 F8 B8 F4 ED F6 F9 E5 F7 F0 B0 BE FF B7 B6 B7 CA C4 99 8B CE C0 82 82 90 86 93 95 87 95 90 86 97 9F D6 9C 86 9A 8E 88 9C 92 83 92 99 82 98 91 EA A5 A7 BD EE BC B8 AC B0 B7 E0 B5 AE A2 E4 AC B4 A8 AC B8 B2 B3 B9 AF E8 F3 B4 B4 B3 A7 F9 B9 43 45 43 13 01 00 42 48 55 0E 55 53 4A 54 4D 5F 5F 5D 51 55 5B 4C 5F 52 56 56 59 55 53 59 40 5C 7E 72 6D 71 7A 7D 6D 4C 70 64 73 71 6A 66 6D 6B 6E 7E 60 6D 6D 7A 6F 6E 7B 7C 7E 5A 73 6E 64 74 09 00 0D 1D 0C 1A 05 01 06 33 01 15 0E 14 0C 00 13 1C 10 0D 0E 10 0F 14 06 1A 1F 1F 1F 19 00 10 38 2A 2B 3D 27 39 29 2F 37 2A 2C 25 2A 26 26 20 36 32 3F 37 31 2D 39 0E 33 25 35 25 33 2F 20 25 D8 C4 CF DB CF C2 D8 C5 C7 83 D0 D3 C9 C4 C1 D6 C9 9B DC D0 DA 9F D2 D2 C6 93 C3 C5 D7 C5 C0 D0 F2 FB E4 E6 FC EA FE A3 E7 FB E5 AE F5 E2 E8 E0 F9 EF B4 BB ED F7 F3 EF E6 F0 E5 E5 E5 F4 FC F0 87 8E BB 9D 8B 83 80 8C 90 C3 AF 97 83 95 88 84 83 AB 99 8B 8D 9A 98 B6 97 8A D0 8A D6 BC 91 8C 83 AF E8 F4 EE E3 EC 9D B7 A1 AC A8 A5 8C A1 BC FA E6 F8 AA AA BA B0 B1 B3 A1 A6 E3 EC F8 FB F5 43 58 08 47 41 5B 0C 4C 02 48 4E 4E 51 49 04 46 55 56 55 58 50 5B 4E 58 41 46 5C 45 7E 72 75 71 49 64 66 67 6B 6C 78 64 6D 6D 43 6E 68 73 61 6B 6E 36 54 7C 70 78 68 75 5B 75 3D 5F 79 79 71 38 27 0A 1C 0A 06 5E 42 5D 4C 00 08 14 08 0C 10 0D 1F 5B 1C 16 09 11 10 12 13 17 50 15 1F 13 54 1B 25 3F 68 3A 3A 2E 3E 39 21 22 2E 22 23 2B 28 20 3E 2F 30 3C 7E 3C 33 33 3C 36 33 25 3F 38 3A 75 CC CA C1 C5 CB CB D8 C5 C7 83 C3 CE C8 C9 C1 C6 CE D2 D7 D7 9E CC C8 DC DE DF D5 D5 C2 DF D1 95 F9 EE FA FF EB FD AC EE EE EC F3 E4 E2 A7 F0 ED FF BB FB F6 F0 F1 F9 FE E6 FA FF FF B6 F2 F5 E7 86 92 9C 81 8B CF 88 82 95 8D 8C 8E 87 83 81 81 DA 9F 99 8D 9F DF 98 94 96 D3 9E 9E 82 D7 99 94 BE A8 A0 E9 BA A7 A9 ED AF A2 AE A8 A0 A2 B7 B1 86 F4 E2 F3 E1 FD E0 E3 AE A5 B5 A3 BF B1 AD BC 44 4C 6B 41 4B 4C 47 44 4C 44 00 58 49 52 56 05 5C 52 54 5C 4D 5B 53 4A 5C 5F 5F 50 52 0D 52 54 63 67 6D 6D 21 2F 65 7E 22 6D 6F 75 26 6E 6A 76 6E 7A 74 75 7B 7B 3C 75 77 61 75 31 79 71 34 35 1A 0A 1A 1D 1D 4F 02 08 07 07 40 13 03 17 05 0C 08 3E 0E 1C 0C 06 08 15 1B 1D 17 51 15 1F 11 16 21 38 68 26 3B 3B 28 22 2C 26 24 2E 31 29 28 2A 3B 3F 31 37 39 0D 39 2D 33 3A 22 38 38 30 74 31 CB C6 C9 CE CB CB 8C CB CB CF C5 D2 F4 C2 D4 C4 D3 C9 98 DA D1 D2 CC D1 D7 C7 D5 C5 DE D2 94 C6 EF F9 FE EC FC AF FE E8 F6 F6 F2 EF E3 E3 A4 EB F5 BB EE FC EC EC F5 F2 FC B3 E3 FE E3 E5 F7 F0 99 CB 89 9B 8B CF 8F 82 8C 85 89 86 93 95 81 81 9F 8D 9D 8B 87 DF 8A 98 80 80 99 9E 98 D7 87 9A BF B9 AB AC EE A9 AD A4 AE A6 A4 E1 EE FC E4 EC 9D BE AC AD B7 B1 BB FD A6 BB B9 BF B1 A4 F4 A7 4F 4A 4C 50 4A 40 5B 43 4E 4C 41 45 1C 43 4B 4B 5F 77 57 56 55 56 52 5A 12 46 40 11 42 5F 51 15 68 7E 61 65 6A 5D 69 6C 66 6A 6E 66 26 73 6C 60 3A 76 79 77 77 79 79 6E 66 7D 7F 65 36 72 7A 7A 1F 0C 00 49 0A 06 1F 06 42 10 10 00 05 02 5E 45 5A 1D 0A 1C 1B 53 5C 5D 1C 16 15 15 13 13 35 19 26 24 2B 28 3A 26 22 2A 62 25 29 2D 23 34 05 29 28 3E 39 3D 27 7F 35 33 21 27 31 3D 3A 32 30 11 C5 DC C6 C5 C1 CE C8 8D C1 CC CD D1 CA C2 D0 C0 9A D2 CB 99 D0 D0 9C D1 DD DD D7 D4 C4 97 D5 C3 EB E2 E4 E8 EC E3 E9 EE ED F6 EC E5 A6 E9 EB F1 BA E9 FD F8 FD F7 BC E9 FA F6 B0 E2 F3 E5 E2 F0 98 9F 80 8C CE 9C 89 9F 94 86 92 C1 92 88 8B 8E DA 8F 97 96 DE 93 93 93 95 D3 84 9E D6 85 91 86 BA A4 A6 AD BA A7 A9 ED A6 AC B7 AF AA A8 A5 A1 FA AF B1 B4 BB BB FC B2 A7 A7 A2 B4 A7 A2 B1 A6 5E 0B 46 4C 58 4A 5E 0D 50 42 4E 55 4E 42 04 66 7E 75 1F 4A 1E 5D 53 49 12 43 42 5E 42 52 57 41 63 64 66 29 26 4C 60 62 77 67 66 6D 67 75 61 2C 3A 79 74 76 7D 74 79 79 32 67 78 78 65 37 70 70 1C 02 0B 0C 55 4F 18 1F 1B 43 01 06 07 0E 0A 45 15 15 58 18 5E 1B 15 1B 14 16 02 14 18 03 54 1B 2F 3F 3F 26 3C 24 6C 22 30 63 37 28 32 2F 2B 30 2E 7B 39 79 08 0F 12 7D 7A 32 3E 35 76 77 3B 23 CF D9 88 ED E0 FC 81 C2 D4 C6 D2 8C EE F3 F0 F5 E9 92 87 D7 DF D2 D9 80 94 C7 C9 C1 D3 8A F5 D4 E9 E8 ED F9 FA EE FC FD EE EA E3 E0 F2 EE EB EB B5 FF F6 EA B3 F5 EF F2 FC DA FE E7 F7 FB FD F1 CA 9B 89 9D 86 C1 88 82 95 8D 8C 8E 87 83 C9 95 88 94 9F 8B 9B 8C 8F D3 82 92 82 85 9F 99 82 94 A6 A2 AC E9 BE AE BE B9 AB A2 AC E1 A0 AE A8 A0 E0 FB AA BC AD AB BD AF A6 BA BE B6 F6 B3 BB A2 44 47 47 48 4A 0F 5E 48 56 56 52 4F 43 43 04 6D 6E 6F 68 19 5A 50 4B 53 5E 5C 51 55 16 44 40 54 66 67 6D 6D 34 2F 62 62 22 67 61 75 67 27 76 60 79 7E 71 6F 7B 7B 26 3D 76 7C 67 7F 7A 78 75 71 0F 0F 48 49 0C 16 18 08 11 4F 40 04 1E 17 01 06 0E 1E 1C 59 44 5F 15 13 11 1C 1D 01 1A 12 00 10 66 6B 2F 26 3A 6F 6C 2F 3B 37 25 32 22 28 33 2B 36 34 39 3D 73 3C 33 30 22 3F 35 25 33 22 26 39 F9 EE FC F6 EF EC F8 E4 F4 EA F4 F8 D5 D3 C5 D1 DF DF DD CD DF D6 D0 CE C6 CA C0 D4 C5 C3 D5 C7 FE FF E1 E4 EB FC F8 EC EF F3 F3 ED E7 F5 E3 E0 C5 F2 F5 F8 F9 FA F0 FC E0 F4 F5 CE E2 F2 EC E1 99 86 89 85 82 B0 85 80 83 84 85 92 8B 86 88 89 A5 8F 9D 81 8A 9E 8F 8E 97 87 83 93 83 83 80 9A A4 B8 AC A0 BD AC A3 BF A6 EE A9 B1 A5 EA 80 AC A9 B8 B7 AB BA FF B5 AE F2 BD BF A5 F6 A5 A1 BB 44 42 46 4E 09 08 0B 78 51 46 52 0C 67 40 41 4B 4E 78 57 57 4A 5A 52 49 1F 67 49 41 53 40 47 0F 25 24 65 6C 7D 7C 6D 6A 67 2C 54 64 7E 73 46 6C 74 7A 6A 60 5D 73 73 6E 77 67 78 70 62 37 67 7A 09 00 0D 1D 4E 06 1F 4D 01 0F 0F 12 03 03 10 0D 13 08 58 1B 0B 16 10 19 52 1B 11 02 56 19 1B 55 0B 39 2B 69 3D 3F 20 2C 31 2B 60 27 29 2B 20 20 28 38 37 2C 32 3B 7C 33 3D 27 70 35 39 20 3A 39 C5 CA CC 89 DA C7 CD D9 82 D0 D0 CD C7 D4 CC 9F 9A CF D0 D8 CA 9F DA D4 DE D6 90 D8 C5 97 DA DA FE AB E9 A9 DE C1 CB EE ED F6 EC E5 A6 E9 EB F1 BA EC EA F0 EA FA BC E9 FA F6 B0 E2 E6 FB F5 E6 82 D1 C8 C7 8B 97 89 85 96 97 90 92 DC C8 CB 95 88 94 9C D4 9F 8F 95 D0 84 C2 DE 82 82 92 98 99 AB B9 AE A7 E0 AB A9 BB ED B0 B4 A4 AA AB A5 B7 F5 B7 B9 AC B0 BC B4 B8 A0 FC A6 E3 F9 A2 A4 B1 4B 5F 4D 06 55 5B 4D 5F 45 46 54 5C 09 5C 45 57 59 53 45 16 45 49 59 4F 41 5A 5F 5F 4B 4C 40 54 78 6C 6D 7D 73 74 6D 7F 61 6B 7D 7A 70 62 76 76 73 74 76 64 70 70 3C 68 62 77 71 65 73 37 7C 74 19 4B 0A 0C 0B 01 4C 0E 0A 06 03 0A 03 03 44 03 15 09 58 00 1B 0B 08 15 1B 00 50 04 06 13 15 01 2F 6B 20 28 3D 6F 22 22 62 2A 2E 32 32 26 28 29 3F 29 78 3F 31 2D 7C 24 3D 26 22 71 25 2E 27 21 CF C6 DC C1 C7 DC 8C D8 D2 C7 C1 D5 C3 87 CD D6 9A CE D6 CA D7 D8 D2 D8 D6 93 D1 DF D2 97 C3 D4 F9 AB FA EC E8 FA FF E8 E6 E0 EF F4 EA E3 A4 EB F5 EF B8 FD F1 E8 F2 F1 FD F2 F4 B1 E2 FF F1 B5 9F 9B 8C 88 9A 8A D6 CD 96 8B 85 C1 93 97 80 84 8E 9E DF 8A DE 8C 95 9A 9C 92 84 84 84 92 D4 91 A3 AF E8 A7 A1 BB EC A0 A3 B7 A3 A9 E6 A6 AA A1 FA AC B9 AA FE AD B9 BB A7 A0 B5 B5 A5 A3 B1 B9 46 4A 5A 04 5B 5F 48 4C 56 46 0D 42 49 52 48 41 1A 55 57 4D 1E 4C 48 5C 55 56 10 45 5E 52 14 40 7A 6F 69 7D 6B 35 2C 5E 76 66 6C 6D 67 75 29 70 6A 7F 79 6D 7B 31 71 6E 7B 70 7F 64 7A 73 34 7B 05 1F 48 1E 1C 06 18 08 42 17 08 04 46 12 14 01 1B 0F 1D 43 5E 51 06 14 02 10 1F 04 1A 13 54 1B 25 3F 68 3C 20 3F 2D 2E 29 63 34 29 23 67 31 35 3E 3A 2C 3C 64 7F 76 73 78 27 38 34 76 22 24 31 CB DF CD 89 DE CE CF C6 C3 C4 C5 81 CE C6 C0 85 D4 D4 98 D0 D0 CC C8 DC DE DF D5 C3 96 DE DA C6 E3 EF ED FF EB FD FF E4 ED ED E4 E4 F5 F3 ED EB FB EF F1 F6 F0 FC F3 F3 FC F6 F3 E5 FF F8 FA E6 98 8E 98 88 87 9D 8E 8C 91 86 A4 88 94 8F 81 84 9E 9E 8A 8A 93 9A 8F 8E 93 94 95 95 97 83 95 96 A6 A2 AD A7 BA 86 A8 AC A1 B7 A9 B7 AF B3 BD B5 BB AE AB BC BA) */;

		internal static byte[] 4 = new byte[9589]
		{
			209, 208, 136, 203, 219, 198, 192, 201, 130, 158,
			128, 218, 150, 218, 136, 133, 220, 210, 212, 220,
			247, 209, 216, 216, 202, 147, 141, 145, 205, 134,
			201, 153, 170, 255, 231, 253, 239, 227, 202, 228,
			238, 230, 243, 161, 187, 167, 255, 183, 231, 183,
			184, 253, 241, 232, 242, 241, 253, 242, 244, 244,
			242, 183, 169, 181, 145, 216, 149, 197, 206, 155,
			131, 153, 131, 143, 192, 220, 198, 156, 208, 152,
			214, 219, 139, 146, 151, 143, 140, 152, 150, 211,
			205, 209, 141, 194, 137, 213, 183, 182, 179, 178,
			238, 173, 185, 164, 174, 167, 224, 252, 230, 188,
			244, 184, 250, 166, 165, 162, 165, 255, 170, 253,
			239, 243, 171, 225, 171, 251, 244, 182, 70, 66,
			77, 71, 90, 112, 69, 73, 2, 30, 0, 90,
			23, 90, 4, 88, 71, 64, 67, 25, 93, 82,
			88, 29, 15, 19, 75, 1, 75, 27, 20, 91,
			101, 101, 107, 108, 46, 50, 44, 118, 51, 126,
			44, 33, 103, 117, 99, 118, 58, 38, 56, 98,
			44, 98, 60, 96, 111, 104, 107, 49, 102, 126,
			112, 53, 87, 75, 19, 89, 19, 67, 76, 12,
			1, 23, 9, 23, 15, 19, 29, 69, 71, 91,
			3, 72, 3, 95, 1, 0, 9, 8, 80, 5,
			15, 7, 17, 85, 119, 107, 51, 121, 51, 99,
			108, 41, 35, 55, 33, 97, 123, 103, 63, 116,
			39, 123, 37, 36, 37, 36, 124, 62, 61, 55,
			53, 113, 107, 119, 47, 101, 215, 135, 136, 219,
			203, 206, 223, 194, 204, 131, 157, 129, 221, 150,
			217, 133, 199, 198, 195, 194, 158, 204, 201, 205,
			194, 220, 194, 197, 211, 211, 148, 136, 170, 240,
			184, 244, 162, 175, 239, 248, 241, 247, 239, 236,
			166, 186, 164, 254, 171, 230, 184, 228, 227, 228,
			231, 189, 243, 229, 241, 248, 250, 246, 246, 249,
			143, 203, 213, 201, 149, 223, 145, 193, 194, 149,
			133, 147, 149, 142, 139, 139, 218, 198, 216, 130,
			207, 130, 208, 221, 145, 134, 130, 131, 147, 153,
			128, 163, 175, 185, 187, 160, 161, 161, 236, 240,
			226, 184, 242, 188, 234, 231, 160, 164, 174, 190,
			248, 228, 254, 164, 239, 160, 254, 243, 178, 190,
			178, 174, 244, 232, 10, 80, 28, 84, 14, 82,
			81, 86, 89, 3, 84, 1, 27, 7, 95, 21,
			71, 23, 24, 80, 90, 31, 1, 29, 73, 2,
			77, 29, 22, 82, 66, 80, 100, 127, 40, 52,
			46, 116, 62, 112, 46, 35, 112, 96, 127, 107,
			107, 100, 126, 59, 37, 57, 101, 44, 97, 61,
			111, 110, 107, 106, 54, 99, 52, 40, 74, 16,
			88, 20, 66, 79, 5, 9, 66, 94, 64, 26,
			87, 26, 72, 69, 21, 16, 88, 68, 94, 4,
			78, 0, 94, 83, 6, 16, 26, 2, 17, 85,
			119, 107, 51, 122, 51, 99, 108, 40, 48, 49,
			47, 51, 102, 122, 100, 62, 110, 38, 120, 36,
			35, 36, 39, 125, 33, 59, 63, 35, 34, 52,
			33, 33, 138, 150, 136, 210, 158, 210, 128, 141,
			203, 199, 128, 156, 134, 220, 149, 216, 150, 155,
			203, 205, 223, 203, 217, 157, 143, 147, 203, 131,
			203, 151, 201, 200, 241, 240, 168, 234, 225, 235,
			233, 173, 191, 163, 251, 177, 251, 171, 164, 246,
			238, 255, 247, 236, 234, 191, 161, 189, 233, 162,
			237, 189, 182, 228, 224, 241, 143, 153, 154, 201,
			211, 207, 151, 223, 159, 195, 157, 156, 149, 147,
			129, 137, 150, 154, 138, 143, 204, 153, 142, 146,
			156, 135, 149, 159, 146, 147, 157, 134, 190, 228,
			247, 165, 175, 173, 169, 161, 255, 174, 161, 168,
			168, 232, 171, 179, 191, 169, 180, 184, 167, 224,
			176, 188, 176, 182, 188, 236, 185, 161, 177, 167,
			70, 74, 81, 122, 90, 74, 64, 65, 67, 81,
			73, 66, 73, 73, 87, 76, 89, 84, 86, 23,
			87, 92, 83, 80, 83, 90, 94, 85, 83, 65,
			26, 70, 126, 110, 100, 101, 111, 125, 34, 97,
			99, 118, 110, 98, 110, 98, 118, 102, 114, 126,
			123, 114, 65, 121, 117, 113, 119, 76, 117, 105,
			127, 100, 96, 102, 9, 3, 13, 10, 5, 48,
			10, 4, 14, 6, 63, 4, 30, 14, 23, 17,
			9, 36, 25, 23, 26, 32, 15, 20, 8, 22,
			20, 20, 26, 18, 0, 16, 21, 45, 33, 37,
			43, 60, 63, 40, 35, 49, 35, 41, 25, 33,
			43, 55, 5, 45, 61, 43, 45, 54, 51, 51,
			62, 50, 37, 63, 53, 63, 48, 58, 221, 197,
			196, 198, 207, 203, 243, 203, 203, 207, 197, 210,
			195, 223, 205, 209, 229, 218, 212, 213, 205, 208,
			223, 214, 215, 199, 239, 212, 216, 212, 198, 204,
			250, 255, 251, 230, 237, 228, 233, 249, 221, 231,
			229, 226, 244, 254, 244, 241, 233, 254, 251, 236,
			236, 246, 232, 228, 205, 224, 228, 240, 226, 226,
			231, 244, 154, 155, 132, 144, 177, 138, 148, 142,
			142, 150, 147, 136, 137, 137, 151, 138, 138, 158,
			150, 166, 141, 146, 157, 143, 134, 172, 145, 129,
			134, 168, 151, 154, 164, 191, 186, 166, 162, 160,
			186, 168, 176, 175, 161, 184, 153, 180, 172, 170,
			173, 180, 174, 188, 172, 179, 189, 164, 141, 187,
			185, 181, 179, 184, 162, 176, 88, 71, 73, 80,
			113, 93, 73, 94, 75, 89, 69, 78, 80, 66,
			86, 73, 91, 66, 103, 74, 91, 75, 99, 77,
			93, 64, 89, 69, 95, 88, 90, 90, 124, 110,
			122, 101, 111, 118, 83, 126, 103, 119, 95, 104,
			104, 115, 97, 119, 123, 120, 108, 112, 104, 122,
			115, 107, 119, 97, 124, 112, 111, 72, 98, 124,
			25, 2, 10, 5, 11, 28, 28, 1, 3, 16,
			8, 62, 21, 19, 5, 17, 15, 8, 11, 9,
			18, 30, 15, 21, 45, 18, 0, 1, 26, 14,
			7, 5, 38, 42, 59, 33, 17, 61, 41, 62,
			54, 44, 50, 36, 34, 40, 51, 43, 54, 52,
			57, 61, 59, 45, 3, 43, 55, 33, 35, 56,
			57, 57, 39, 49, 197, 220, 198, 197, 193, 206,
			200, 200, 208, 252, 201, 207, 213, 211, 197, 201,
			214, 223, 215, 206, 208, 211, 211, 220, 214, 214,
			194, 238, 192, 210, 198, 220, 236, 242, 236, 230,
			249, 225, 224, 226, 227, 231, 229, 243, 217, 247,
			229, 240, 233, 254, 252, 246, 233, 241, 240, 242,
			243, 247, 245, 227, 201, 229, 241, 230, 159, 134,
			141, 141, 129, 152, 130, 129, 141, 130, 132, 132,
			148, 184, 135, 132, 148, 152, 157, 149, 154, 144,
			139, 147, 158, 156, 145, 149, 147, 133, 171, 134,
			190, 170, 188, 188, 189, 171, 163, 186, 172, 175,
			175, 160, 162, 162, 182, 154, 184, 174, 171, 160,
			191, 175, 172, 130, 164, 182, 162, 162, 191, 184,
			186, 180, 90, 91, 119, 71, 79, 66, 73, 64,
			75, 68, 82, 64, 82, 78, 75, 75, 101, 75,
			77, 85, 82, 72, 85, 83, 86, 92, 71, 110,
			91, 94, 90, 92, 103, 98, 114, 108, 121, 102,
			98, 105, 109, 116, 95, 116, 104, 106, 109, 107,
			115, 118, 113, 99, 123, 104, 117, 115, 118, 124,
			103, 78, 123, 118, 108, 124, 7, 2, 18, 12,
			25, 6, 2, 9, 13, 20, 63, 20, 8, 10,
			5, 29, 19, 22, 17, 3, 27, 8, 21, 19,
			22, 28, 7, 46, 5, 31, 27, 2, 61, 34,
			38, 45, 33, 56, 19, 37, 43, 39, 37, 54,
			47, 41, 32, 42, 45, 4, 59, 53, 49, 44,
			57, 42, 59, 61, 52, 62, 33, 8, 39, 48,
			222, 244, 206, 198, 205, 218, 223, 218, 203, 205,
			196, 206, 209, 248, 205, 214, 229, 205, 209, 202,
			215, 221, 208, 216, 197, 218, 222, 213, 217, 192,
			235, 220, 249, 212, 229, 224, 224, 230, 225, 228,
			248, 230, 228, 246, 239, 233, 224, 234, 237, 196,
			235, 252, 234, 192, 232, 244, 230, 255, 245, 230,
			255, 249, 240, 250, 157, 180, 155, 157, 143, 157,
			152, 178, 134, 145, 129, 134, 145, 142, 138, 129,
			149, 140, 167, 138, 155, 139, 163, 148, 149, 157,
			159, 131, 147, 168, 151, 128, 184, 184, 167, 187,
			145, 170, 186, 168, 172, 183, 179, 165, 175, 166,
			168, 170, 189, 132, 183, 169, 187, 177, 184, 180,
			179, 191, 191, 182, 137, 164, 181, 163, 79, 79,
			65, 72, 66, 64, 75, 114, 79, 70, 83, 82,
			71, 64, 65, 65, 83, 90, 84, 86, 89, 96,
			93, 78, 89, 64, 88, 84, 90, 91, 107, 90,
			122, 110, 102, 122, 102, 106, 96, 97, 93, 102,
			120, 100, 101, 114, 112, 96, 117, 107, 125, 119,
			123, 109, 67, 111, 119, 101, 117, 112, 122, 103,
			102, 122, 9, 14, 27, 26, 49, 10, 20, 4,
			22, 19, 18, 14, 5, 2, 23, 22, 37, 9,
			29, 21, 31, 10, 18, 30, 26, 6, 0, 21,
			23, 3, 17, 7, 21, 40, 32, 44, 45, 36,
			57, 61, 38, 34, 52, 36, 52, 24, 32, 42,
			45, 53, 52, 54, 63, 59, 41, 45, 54, 50,
			36, 52, 36, 8, 61, 59, 217, 223, 201, 197,
			194, 220, 196, 194, 208, 215, 195, 212, 210, 248,
			214, 192, 221, 210, 203, 205, 219, 205, 207, 213,
			221, 193, 196, 210, 195, 195, 235, 192, 228, 249,
			237, 238, 231, 252, 248, 232, 240, 240, 232, 238,
			244, 243, 231, 240, 238, 196, 241, 234, 193, 237,
			249, 250, 251, 224, 228, 244, 228, 242, 240, 241,
			143, 142, 152, 182, 130, 134, 130, 134, 189, 128,
			149, 147, 148, 130, 138, 145, 158, 158, 157, 137,
			161, 147, 149, 147, 153, 172, 130, 148, 145, 158,
			135, 129, 175, 185, 191, 186, 145, 172, 163, 163,
			172, 166, 163, 181, 177, 180, 155, 182, 191, 181,
			188, 174, 173, 128, 184, 180, 161, 176, 191, 191,
			184, 178, 183, 161, 78, 89, 88, 74, 113, 92,
			88, 76, 80, 87, 68, 83, 86, 68, 123, 86,
			78, 84, 72, 93, 76, 79, 95, 98, 65, 86,
			68, 110, 87, 84, 64, 92, 124, 98, 124, 112,
			106, 125, 124, 110, 93, 96, 108, 100, 103, 117,
			91, 100, 121, 111, 113, 111, 119, 107, 101, 105,
			123, 103, 124, 116, 114, 114, 114, 116, 31, 7,
			28, 57, 15, 27, 4, 0, 23, 15, 20, 8,
			22, 11, 1, 1, 19, 9, 29, 26, 10, 16,
			14, 4, 33, 22, 28, 20, 21, 3, 84, 20,
			106, 45, 39, 37, 42, 42, 62, 30, 39, 47,
			37, 34, 50, 103, 37, 101, 60, 50, 52, 60,
			49, 41, 57, 47, 62, 50, 41, 123, 120, 59,
			58, 62, 217, 223, 205, 197, 194, 206, 222, 131,
			199, 219, 197, 172, 172, 173, 212, 208, 216, 215,
			209, 218, 158, 212, 217, 196, 146, 209, 220, 222,
			213, 220, 148, 220, 249, 171, 229, 224, 253, 252,
			229, 227, 229, 163, 233, 245, 245, 167, 244, 228,
			227, 247, 247, 248, 250, 191, 240, 244, 252, 246,
			224, 228, 244, 251, 253, 246, 202, 128, 141, 144,
			206, 134, 159, 205, 143, 130, 140, 135, 137, 149,
			137, 128, 158, 159, 175, 204, 206, 156, 146, 171,
			136, 151, 183, 167, 157, 190, 179, 187, 188, 169,
			159, 248, 162, 173, 162, 156, 244, 138, 135, 240,
			182, 165, 169, 169, 160, 186, 143, 189, 171, 150,
			148, 159, 227, 138, 189, 169, 166, 142, 173, 151,
			88, 113, 112, 66, 24, 102, 104, 120, 91, 110,
			116, 115, 97, 118, 94, 108, 10, 117, 108, 115,
			120, 113, 121, 119, 113, 124, 116, 104, 125, 98,
			88, 81, 94, 89, 59, 92, 124, 90, 96, 67,
			73, 86, 72, 101, 80, 81, 92, 79, 108, 120,
			115, 110, 47, 73, 75, 110, 32, 113, 71, 87,
			90, 92, 36, 126, 90, 49, 63, 35, 61, 11,
			52, 55, 55, 0, 55, 59, 63, 4, 15, 80,
			52, 31, 32, 30, 79, 41, 36, 25, 19, 33,
			64, 43, 36, 58, 44, 51, 59, 46, 35, 125,
			56, 43, 9, 58, 9, 6, 4, 4, 34, 51,
			54, 48, 41, 47, 61, 61, 126, 60, 51, 48,
			63, 54, 62, 37, 108, 119, 118, 38, 222, 206,
			196, 197, 207, 221, 129, 222, 202, 204, 210, 213,
			197, 210, 208, 214, 201, 211, 215, 203, 202, 220,
			201, 201, 136, 156, 159, 225, 196, 210, 199, 198,
			239, 239, 235, 253, 252, 227, 225, 232, 246, 226,
			227, 238, 232, 243, 246, 234, 246, 248, 247, 244,
			243, 254, 242, 249, 241, 252, 253, 252, 247, 249,
			240, 250, 152, 136, 135, 135, 154, 157, 131, 129,
			129, 142, 132, 142, 148, 132, 144, 151, 150, 154,
			148, 141, 137, 150, 146, 146, 130, 135, 153, 158,
			152, 132, 156, 156, 172, 191, 187, 188, 190, 170,
			190, 134, 167, 186, 132, 168, 161, 174, 176, 131,
			169, 171, 185, 186, 187, 186, 178, 169, 183, 161,
			162, 184, 177, 191, 160, 165, 75, 94, 91, 76,
			92, 74, 88, 88, 80, 77, 69, 82, 69, 70,
			84, 64, 94, 94, 84, 92, 74, 90, 85, 83,
			65, 86, 66, 69, 70, 86, 83, 80, 127, 123,
			124, 104, 108, 106, 127, 110, 103, 109, 100, 99,
			103, 100, 111, 118, 106, 122, 123, 124, 127, 109,
			110, 114, 101, 119, 127, 102, 120, 118, 102, 103,
			5, 28, 4, 12, 8, 27, 4, 2, 15, 6,
			4, 14, 17, 9, 8, 0, 28, 15, 8, 24,
			25, 26, 24, 18, 5, 29, 19, 16, 6, 4,
			24, 26, 41, 32, 61, 57, 47, 61, 62, 34,
			53, 54, 48, 47, 51, 42, 40, 42, 57, 48,
			57, 43, 44, 48, 43, 47, 59, 52, 56, 37,
			37, 52, 38, 58, 198, 199, 196, 198, 205, 196,
			220, 223, 203, 205, 212, 210, 197, 213, 193, 192,
			212, 155, 209, 202, 158, 209, 211, 201, 146, 210,
			144, 194, 222, 216, 198, 193, 233, 254, 252, 169,
			217, 230, 226, 233, 237, 244, 243, 161, 243, 233,
			224, 224, 232, 232, 236, 248, 240, 251, 239, 218,
			254, 252, 242, 240, 250, 203, 240, 240, 156, 197,
			155, 157, 139, 131, 128, 140, 144, 205, 140, 128,
			147, 137, 135, 141, 159, 137, 214, 138, 138, 154,
			144, 145, 147, 129, 221, 157, 151, 130, 154, 150,
			162, 174, 186, 228, 189, 166, 162, 170, 174, 166,
			237, 168, 168, 180, 176, 164, 180, 184, 189, 170,
			170, 186, 176, 177, 179, 161, 253, 162, 191, 185,
			179, 185, 79, 6, 65, 71, 93, 91, 77, 67,
			65, 70, 115, 78, 64, 83, 83, 68, 72, 94,
			100, 122, 82, 94, 79, 78, 87, 64, 108, 100,
			100, 123, 14, 96, 88, 71, 40, 89, 124, 96,
			120, 98, 97, 108, 108, 69, 99, 97, 101, 112,
			118, 111, 81, 122, 113, 113, 62, 49, 34, 96,
			120, 116, 122, 123, 72, 122, 26, 14, 6, 53,
			13, 0, 1, 0, 3, 13, 4, 67, 70, 69,
			65, 84, 88, 30, 14, 28, 16, 11, 8, 24,
			31, 26, 4, 24, 24, 1, 27, 30, 47, 59,
			41, 48, 34, 32, 45, 41, 43, 39, 35, 44,
			34, 38, 54, 34, 41, 62, 32, 41, 59, 60,
			40, 56, 54, 115, 49, 63, 118, 54, 38, 39,
			203, 210, 136, 198, 200, 143, 206, 212, 214, 198,
			211, 201, 210, 211, 212, 159, 149, 148, 137, 139,
			137, 145, 140, 147, 130, 157, 129, 139, 215, 199,
			196, 217, 227, 232, 233, 253, 231, 224, 226, 162,
			237, 224, 244, 228, 242, 170, 247, 241, 232, 254,
			249, 244, 240, 240, 188, 241, 253, 252, 224, 243,
			247, 244, 255, 181, 154, 132, 154, 157, 206, 152,
			141, 158, 194, 130, 150, 128, 143, 139, 133, 135,
			150, 158, 216, 159, 145, 141, 220, 137, 154, 150,
			208, 157, 151, 130, 154, 150, 162, 174, 186, 238,
			189, 239, 175, 162, 172, 183, 165, 175, 178, 231,
			183, 160, 168, 173, 189, 171, 173, 171, 185, 177,
			190, 178, 162, 252, 181, 184, 186, 161, 79, 69,
			92, 102, 101, 97, 67, 89, 2, 110, 79, 69,
			79, 65, 77, 64, 94, 117, 87, 77, 30, 121,
			83, 72, 92, 87, 120, 101, 98, 103, 27, 4,
			36, 58, 40, 74, 97, 97, 120, 104, 108, 119,
			45, 85, 127, 119, 97, 63, 58, 88, 119, 119,
			106, 122, 114, 105, 63, 95, 117, 127, 113, 99,
			124, 47, 74, 46, 60, 8, 9, 85, 76, 46,
			3, 0, 8, 4, 75, 36, 11, 11, 14, 9,
			23, 21, 68, 95, 18, 18, 95, 16, 17, 18,
			30, 18, 4, 0, 40, 39, 33, 42, 98, 111,
			33, 44, 58, 110, 33, 38, 35, 122, 119, 116,
			111, 104, 110, 105, 110, 111, 112, 125, 59, 62,
			61, 36, 34, 54, 54, 57, 207, 232, 201, 202,
			198, 202, 129, 238, 205, 205, 212, 211, 201, 203,
			158, 133, 212, 212, 149, 202, 202, 208, 206, 216,
			191, 185, 241, 210, 213, 210, 199, 198, 167, 200,
			231, 231, 250, 253, 227, 225, 175, 194, 236, 237,
			233, 240, 169, 202, 232, 242, 255, 240, 240, 165,
			188, 183, 159, 153, 211, 254, 248, 249, 241, 246,
			158, 130, 135, 135, 212, 207, 135, 136, 135, 147,
			205, 128, 138, 142, 146, 128, 153, 151, 151, 138,
			155, 180, 153, 152, 130, 222, 177, 157, 159, 129,
			145, 207, 234, 191, 161, 164, 171, 160, 185, 185,
			255, 242, 242, 241, 203, 205, 155, 154, 187, 168,
			171, 188, 170, 175, 189, 169, 186, 178, 163, 162,
			179, 163, 167, 250, 67, 69, 76, 76, 86, 1,
			68, 89, 79, 79, 88, 12, 8, 79, 80, 72,
			86, 79, 93, 65, 74, 16, 84, 73, 95, 95,
			11, 17, 85, 95, 85, 71, 121, 110, 124, 52,
			123, 123, 106, 32, 58, 45, 106, 114, 114, 98,
			124, 113, 53, 113, 121, 111, 127, 108, 127, 111,
			123, 99, 100, 42, 54, 116, 124, 116, 24, 24,
			13, 29, 83, 26, 24, 11, 79, 91, 78, 12,
			12, 20, 74, 6, 9, 8, 12, 28, 6, 11,
			83, 30, 1, 0, 75, 81, 21, 31, 21, 7,
			57, 46, 60, 116, 59, 59, 42, 96, 122, 109,
			42, 50, 41, 41, 37, 53, 42, 55, 49, 58,
			63, 43, 53, 50, 60, 124, 58, 34, 57, 57,
			111, 117, 201, 195, 201, 219, 221, 202, 216, 144,
			215, 215, 198, 140, 158, 137, 212, 203, 221, 210,
			213, 216, 217, 218, 147, 205, 220, 212, 158, 219,
			198, 208, 221, 216, 235, 236, 237, 166, 228, 255,
			233, 234, 172, 233, 240, 228, 225, 169, 227, 236,
			252, 242, 245, 248, 249, 250, 179, 250, 251, 245,
			190, 226, 224, 240, 253, 248, 139, 140, 141, 198,
			157, 153, 139, 198, 154, 142, 140, 207, 145, 130,
			134, 149, 147, 150, 153, 158, 155, 208, 139, 152,
			144, 131, 222, 147, 155, 135, 157, 152, 171, 172,
			173, 230, 172, 162, 188, 227, 171, 160, 175, 168,
			171, 166, 163, 160, 245, 163, 245, 176, 189, 176,
			178, 243, 165, 188, 182, 183, 176, 184, 186, 161,
			5, 92, 71, 79, 72, 1, 91, 66, 68, 69,
			18, 71, 73, 73, 80, 10, 77, 84, 94, 95,
			12, 17, 72, 73, 84, 85, 95, 95, 66, 24,
			64, 65, 108, 37, 103, 125, 104, 105, 99, 99,
			118, 44, 111, 117, 96, 41, 105, 117, 46, 109,
			113, 125, 123, 112, 51, 112, 98, 39, 62, 102,
			115, 117, 121, 99, 3, 15, 13, 6, 65, 24,
			9, 15, 15, 77, 13, 17, 85, 6, 17, 1,
			19, 20, 87, 20, 14, 26, 27, 83, 5, 18,
			6, 16, 3, 19, 29, 26, 101, 60, 41, 63,
			96, 59, 52, 57, 54, 38, 56, 53, 105, 55,
			40, 36, 51, 53, 99, 121, 61, 55, 61, 47,
			33, 54, 36, 108, 35, 35, 50, 120, 146, 133,
			197, 200, 222, 129, 219, 204, 209, 206, 193, 209,
			214, 203, 205, 198, 219, 207, 209, 214, 208, 144,
			203, 220, 193, 222, 195, 197, 211, 219, 216, 212,
			248, 166, 229, 232, 231, 225, 248, 232, 236, 226,
			238, 226, 227, 194, 198, 210, 255, 249, 206, 240,
			251, 232, 216, 248, 244, 242, 229, 253, 226, 219,
			251, 246, 139, 135, 200, 186, 154, 128, 158, 140,
			133, 134, 140, 132, 144, 130, 136, 129, 152, 164,
			144, 141, 138, 143, 198, 210, 221, 135, 145, 132,
			132, 158, 218, 153, 165, 168, 169, 165, 166, 160,
			191, 185, 194, 233, 238, 173, 162, 165, 238, 235,
			182, 180, 191, 186, 177, 178, 172, 175, 183, 160,
			163, 184, 185, 185, 164, 167, 79, 91, 73, 91,
			71, 65, 75, 76, 2, 71, 79, 86, 72, 75,
			75, 68, 94, 27, 81, 74, 30, 94, 80, 79,
			87, 82, 84, 72, 22, 69, 65, 91, 100, 98,
			102, 110, 96, 96, 44, 105, 109, 116, 110, 109,
			105, 102, 96, 37, 115, 104, 56, 107, 107, 113,
			114, 116, 124, 116, 86, 126, 100, 99, 122, 124,
			30, 14, 12, 6, 25, 1, 0, 2, 3, 7,
			90, 17, 20, 8, 3, 23, 31, 8, 11, 59,
			53, 61, 49, 63, 53, 49, 36, 51, 48, 69,
			84, 29, 62, 63, 56, 58, 116, 96, 99, 46,
			38, 45, 114, 111, 39, 41, 32, 55, 107, 44,
			47, 119, 58, 58, 42, 114, 36, 54, 34, 34,
			63, 56, 58, 38, 132, 193, 219, 198, 192, 132,
			135, 134, 240, 198, 204, 196, 199, 212, 193, 136,
			232, 222, 212, 220, 223, 204, 217, 144, 159, 240,
			252, 156, 213, 216, 193, 217, 238, 171, 230, 230,
			250, 175, 254, 232, 227, 231, 160, 245, 238, 226,
			164, 243, 255, 233, 235, 240, 241, 241, 188, 241,
			251, 224, 228, 171, 182, 227, 252, 240, 202, 157,
			141, 155, 157, 134, 131, 131, 194, 143, 137, 146,
			146, 199, 147, 132, 137, 219, 157, 148, 142, 139,
			133, 211, 220, 158, 145, 159, 159, 145, 145, 134,
			190, 235, 171, 166, 160, 187, 173, 164, 172, 176,
			224, 160, 168, 231, 177, 171, 169, 186, 190, 188,
			254, 175, 189, 169, 186, 233, 240, 178, 185, 162,
			184, 177, 10, 69, 71, 93, 14, 93, 73, 76,
			70, 3, 84, 73, 67, 7, 73, 68, 84, 82,
			94, 92, 77, 75, 6, 29, 95, 82, 94, 88,
			80, 82, 71, 65, 42, 104, 103, 103, 122, 110,
			101, 99, 113, 35, 97, 111, 38, 110, 106, 115,
			123, 119, 113, 125, 62, 124, 116, 104, 124, 120,
			48, 120, 114, 45, 52, 118, 2, 30, 6, 2,
			78, 79, 4, 12, 17, 67, 1, 65, 20, 2,
			18, 0, 8, 8, 29, 29, 94, 13, 29, 19,
			21, 22, 4, 25, 19, 87, 25, 20, 36, 34,
			46, 44, 61, 59, 108, 37, 35, 39, 96, 47,
			41, 103, 39, 45, 47, 53, 51, 42, 19, 48,
			38, 52, 62, 63, 49, 126, 99, 121, 100, 117,
			130, 252, 193, 199, 202, 192, 219, 222, 130, 237,
			244, 129, 151, 151, 138, 149, 129, 155, 239, 208,
			208, 137, 136, 134, 146, 203, 134, 133, 159, 151,
			245, 197, 250, 231, 237, 222, 235, 237, 199, 228,
			246, 172, 181, 178, 177, 169, 183, 179, 186, 179,
			211, 209, 202, 210, 208, 177, 178, 255, 249, 250,
			243, 183, 211, 240, 137, 128, 135, 192, 206, 172,
			132, 159, 141, 142, 133, 206, 215, 213, 209, 203,
			202, 213, 200, 215, 206, 223, 175, 156, 148, 146,
			130, 152, 217, 194, 199, 194, 228, 248, 254, 136,
			173, 172, 169, 189, 182, 233, 239, 235, 135, 164,
			167, 160, 170, 175, 245, 149, 191, 177, 187, 168,
			179, 180, 181, 180, 184, 250, 129, 134, 6, 78,
			70, 18, 95, 18, 28, 3, 27, 80, 69, 66,
			11, 68, 76, 8, 79, 90, 26, 122, 86, 77,
			83, 80, 91, 70, 93, 19, 13, 65, 9, 23,
			59, 57, 61, 43, 34, 47, 46, 67, 109, 119,
			46, 64, 41, 69, 118, 100, 116, 127, 58, 34,
			104, 34, 62, 47, 38, 49, 60, 49, 52, 80,
			123, 122, 13, 7, 13, 73, 45, 7, 30, 2,
			15, 6, 66, 90, 16, 90, 70, 84, 72, 78,
			90, 10, 27, 28, 81, 30, 26, 94, 5, 16,
			91, 26, 27, 23, 35, 39, 45, 118, 126, 60,
			41, 46, 111, 32, 40, 108, 51, 38, 105, 53,
			54, 58, 44, 63, 49, 45, 49, 127, 5, 58,
			62, 53, 57, 32, 39, 119, 249, 206, 203, 132,
			232, 202, 216, 206, 202, 142, 243, 200, 210, 194,
			202, 202, 212, 222, 235, 220, 221, 146, 250, 216,
			198, 208, 216, 156, 251, 216, 208, 208, 233, 228,
			250, 250, 221, 234, 239, 160, 196, 230, 244, 226,
			238, 170, 192, 224, 233, 239, 253, 244, 238, 235,
			229, 167, 189, 188, 211, 254, 248, 227, 241, 251,
			158, 198, 186, 136, 128, 136, 137, 142, 132, 206,
			141, 136, 146, 142, 131, 132, 142, 158, 156, 145,
			138, 146, 144, 193, 206, 210, 180, 190, 181, 163,
			173, 165, 143, 247, 160, 189, 163, 163, 134, 184,
			177, 183, 224, 160, 230, 170, 171, 168, 191, 181,
			172, 186, 182, 190, 176, 177, 183, 189, 183, 180,
			181, 177, 249, 189, 94, 95, 88, 90, 20, 0,
			3, 28, 12, 18, 14, 16, 8, 22, 11, 65,
			84, 72, 21, 72, 75, 90, 78, 68, 90, 71,
			68, 65, 69, 13, 27, 26, 59, 37, 56, 39,
			62, 33, 61, 34, 102, 109, 115, 44, 119, 114,
			97, 119, 99, 115, 108, 109, 110, 108, 38, 50,
			61, 43, 62, 41, 56, 47, 58, 45, 69, 25,
			13, 26, 1, 3, 26, 8, 10, 23, 20, 17,
			21, 93, 75, 74, 66, 85, 64, 87, 74, 81,
			72, 82, 0, 22, 3, 30, 26, 1, 17, 91,
			57, 63, 45, 37, 34, 46, 62, 96, 38, 44,
			55, 47, 42, 40, 37, 33, 116, 49, 43, 54,
			48, 113, 54, 46, 61, 61, 126, 37, 59, 39,
			55, 58, 223, 199, 204, 137, 192, 192, 216, 141,
			209, 194, 214, 196, 134, 195, 203, 210, 212, 215,
			215, 216, 218, 159, 204, 207, 221, 212, 194, 212,
			197, 196, 142, 149, 164, 239, 228, 234, 225, 250,
			224, 233, 162, 237, 239, 245, 166, 228, 246, 224,
			251, 239, 253, 185, 164, 191, 255, 242, 231, 255,
			244, 177, 248, 248, 224, 181, 133, 155, 141, 135,
			206, 140, 131, 152, 142, 135, 192, 143, 137, 147,
			196, 150, 147, 129, 157, 217, 157, 144, 137, 145,
			150, 211, 158, 158, 130, 215, 131, 135, 163, 191,
			173, 233, 168, 174, 165, 161, 167, 167, 224, 181,
			169, 231, 160, 160, 182, 190, 172, 188, 254, 248,
			251, 231, 242, 182, 190, 178, 164, 174, 164, 161,
			67, 68, 70, 9, 72, 78, 69, 65, 71, 71,
			102, 64, 79, 75, 65, 65, 26, 79, 87, 25,
			77, 75, 93, 79, 70, 19, 99, 69, 83, 91,
			88, 84, 120, 49, 40, 94, 103, 97, 104, 98,
			117, 112, 32, 98, 105, 114, 104, 97, 58, 117,
			119, 109, 62, 121, 117, 115, 118, 51, 62, 49,
			66, 127, 113, 53, 8, 30, 1, 5, 10, 79,
			10, 2, 14, 7, 5, 19, 70, 10, 5, 28,
			90, 19, 25, 15, 27, 95, 30, 24, 23, 29,
			80, 28, 25, 1, 17, 17, 102, 107, 58, 44,
			32, 46, 33, 40, 38, 99, 47, 51, 102, 55,
			37, 55, 46, 55, 33, 121, 58, 58, 48, 56,
			38, 54, 52, 127, 16, 54, 61, 57, 207, 207,
			136, 221, 193, 143, 223, 217, 195, 209, 212, 129,
			245, 211, 193, 201, 214, 218, 202, 131, 158, 232,
			213, 211, 214, 220, 199, 194, 150, 213, 216, 218,
			233, 224, 237, 237, 174, 161, 172, 194, 242, 230,
			238, 161, 213, 226, 240, 241, 243, 245, 255, 234,
			190, 235, 244, 248, 252, 179, 195, 244, 245, 226,
			230, 252, 158, 146, 200, 128, 128, 207, 152, 133,
			135, 195, 140, 128, 147, 137, 135, 141, 159, 137,
			216, 152, 144, 155, 220, 143, 135, 157, 208, 133,
			158, 146, 212, 147, 163, 179, 173, 186, 226, 239,
			163, 191, 226, 183, 181, 179, 168, 231, 171, 163,
			188, 251, 139, 180, 191, 173, 168, 253, 147, 163,
			160, 241, 149, 184, 186, 161, 88, 68, 68, 7,
			104, 78, 69, 65, 71, 71, 0, 85, 73, 7,
			87, 81, 91, 73, 76, 25, 109, 75, 89, 81,
			94, 82, 66, 11, 22, 86, 87, 86, 111, 120,
			123, 41, 122, 96, 44, 45, 117, 98, 115, 33,
			98, 98, 106, 108, 127, 127, 54, 57, 95, 113,
			104, 116, 100, 122, 98, 100, 101, 55, 121, 116,
			19, 75, 0, 8, 24, 10, 76, 28, 23, 2,
			18, 0, 8, 19, 13, 11, 31, 31, 88, 16,
			10, 83, 92, 18, 0, 83, 4, 25, 19, 87,
			24, 20, 63, 37, 43, 33, 43, 61, 108, 35,
			39, 38, 36, 50, 102, 51, 43, 101, 40, 46,
			54, 121, 63, 44, 124, 60, 54, 62, 57, 63,
			63, 36, 32, 39, 203, 223, 199, 219, 128, 233,
			205, 196, 206, 198, 196, 129, 210, 200, 132, 214,
			206, 218, 202, 205, 158, 236, 200, 216, 222, 223,
			209, 195, 140, 151, 241, 197, 227, 232, 207, 232,
			227, 234, 255, 193, 227, 246, 238, 226, 238, 226,
			246, 171, 255, 227, 253, 185, 247, 236, 188, 240,
			251, 224, 227, 248, 248, 240, 186, 181, 190, 131,
			141, 201, 140, 154, 133, 129, 134, 195, 137, 146,
			198, 142, 138, 134, 149, 150, 136, 149, 155, 139,
			153, 221, 157, 129, 208, 144, 214, 132, 145, 150,
			191, 185, 161, 189, 183, 239, 184, 162, 173, 175,
			224, 179, 163, 170, 171, 179, 191, 191, 248, 173,
			182, 186, 252, 187, 187, 191, 181, 241, 251, 247,
			162, 176, 88, 66, 78, 80, 14, 91, 68, 72,
			2, 74, 78, 82, 82, 70, 72, 73, 26, 90,
			86, 93, 30, 75, 78, 68, 18, 82, 87, 80,
			95, 89, 26, 65, 98, 110, 40, 111, 103, 99,
			105, 45, 97, 108, 117, 109, 98, 39, 106, 106,
			110, 59, 122, 124, 62, 112, 108, 120, 124, 118,
			116, 80, 120, 120, 96, 125, 15, 25, 72, 25,
			28, 0, 11, 31, 3, 14, 64, 8, 21, 71,
			12, 10, 22, 31, 17, 23, 25, 95, 8, 21,
			23, 83, 22, 24, 26, 18, 84, 88, 106, 40,
			36, 38, 61, 42, 108, 57, 42, 38, 96, 38,
			39, 42, 33, 101, 53, 41, 120, 32, 49, 42,
			46, 125, 51, 61, 36, 56, 32, 62, 38, 32,
			217, 139, 201, 199, 202, 143, 216, 223, 219, 131,
			193, 198, 199, 206, 202, 139, 237, 210, 214, 221,
			209, 200, 207, 157, 219, 192, 144, 211, 218, 216,
			215, 222, 227, 229, 239, 169, 239, 236, 239, 232,
			241, 240, 160, 172, 166, 228, 236, 224, 249, 240,
			184, 248, 240, 235, 245, 235, 251, 225, 229, 226,
			182, 248, 230, 181, 152, 158, 134, 201, 136, 157,
			131, 128, 194, 130, 192, 135, 137, 139, 128, 128,
			136, 219, 129, 150, 139, 223, 147, 138, 156, 221,
			208, 146, 153, 130, 152, 145, 234, 165, 167, 189,
			238, 173, 169, 237, 173, 179, 165, 175, 163, 163,
			254, 229, 244, 251, 157, 183, 185, 182, 178, 184,
			142, 145, 185, 191, 183, 165, 189, 176, 89, 119,
			124, 65, 71, 93, 72, 125, 67, 81, 84, 88,
			122, 105, 114, 108, 126, 114, 121, 101, 112, 105,
			93, 91, 70, 86, 66, 92, 87, 67, 92, 105,
			93, 98, 102, 63, 58, 83, 75, 75, 81, 71,
			75, 94, 71, 97, 112, 96, 104, 118, 121, 109,
			118, 64, 80, 116, 112, 61, 104, 39, 34, 57,
			112, 121, 6, 13, 9, 0, 2, 10, 8, 77,
			22, 12, 64, 19, 3, 10, 11, 19, 31, 91,
			31, 31, 13, 27, 23, 93, 19, 21, 4, 20,
			4, 87, 84, 20, 62, 63, 45, 36, 62, 59,
			63, 119, 98, 6, 46, 38, 47, 41, 33, 25,
			24, 50, 54, 56, 44, 54, 57, 46, 14, 7,
			56, 56, 36, 51, 4, 52, 216, 223, 209, 245,
			224, 249, 229, 233, 235, 226, 252, 239, 240, 198,
			194, 209, 223, 201, 213, 216, 202, 215, 224, 234,
			219, 221, 134, 133, 208, 214, 221, 217, 239, 239,
			168, 253, 225, 175, 239, 255, 231, 226, 244, 228,
			166, 227, 237, 247, 186, 253, 247, 235, 190, 248,
			250, 238, 246, 248, 170, 177, 215, 229, 247, 201,
			171, 153, 139, 199, 139, 151, 137, 171, 141, 145,
			148, 143, 143, 147, 129, 162, 155, 150, 157, 165,
			188, 150, 146, 156, 128, 154, 149, 130, 170, 160,
			157, 155, 252, 255, 148, 143, 161, 189, 184, 163,
			171, 183, 165, 141, 167, 178, 170, 166, 178, 190,
			170, 247, 187, 167, 185, 155, 189, 161, 164, 191,
			191, 163, 177, 146, 75, 70, 77, 117, 108, 70,
			66, 76, 80, 74, 69, 82, 122, 112, 77, 75,
			12, 15, 100, 127, 81, 77, 72, 83, 91, 71,
			85, 114, 90, 94, 81, 91, 126, 38, 95, 96,
			96, 57, 56, 32, 81, 107, 105, 113, 118, 110,
			106, 98, 69, 89, 93, 55, 123, 103, 121, 48,
			83, 70, 68, 89, 73, 71, 85, 70, 57, 60,
			39, 59, 42, 82, 65, 25, 95, 78, 5, 17,
			15, 4, 5, 21, 10, 70, 62, 22, 12, 11,
			18, 20, 6, 22, 93, 20, 6, 30, 23, 16,
			36, 61, 117, 25, 60, 32, 40, 96, 39, 51,
			41, 34, 42, 40, 39, 36, 54, 62, 101, 60,
			48, 114, 41, 46, 127, 61, 63, 51, 51, 122,
			58, 58, 223, 202, 203, 132, 192, 192, 207, 194,
			198, 198, 199, 212, 199, 213, 192, 214, 151, 222,
			200, 208, 221, 207, 211, 207, 198, 210, 220, 156,
			208, 197, 219, 216, 236, 231, 181, 236, 239, 236,
			161, 235, 238, 247, 239, 234, 227, 233, 185, 182,
			254, 249, 171, 251, 255, 170, 248, 254, 240, 247,
			162, 244, 167, 161, 163, 165, 217, 141, 219, 208,
			217, 215, 136, 192, 129, 130, 140, 133, 131, 149,
			133, 216, 159, 130, 178, 145, 156, 184, 159, 148,
			189, 154, 186, 183, 163, 141, 189, 196, 132, 162,
			129, 186, 135, 161, 158, 248, 161, 128, 137, 247,
			143, 172, 180, 157, 140, 152, 146, 224, 240, 186,
			165, 151, 186, 138, 226, 159, 160, 179, 131, 224,
			26, 115, 26, 69, 69, 102, 70, 66, 75, 122,
			77, 116, 19, 125, 99, 96, 11, 98, 66, 115,
			83, 102, 81, 107, 90, 125, 116, 96, 65, 121,
			6, 124, 115, 81, 98, 88, 121, 85, 91, 71,
			106, 90, 87, 80, 50, 73, 80, 105, 114, 65,
			92, 72, 119, 83, 95, 87, 124, 73, 71, 36,
			122, 116, 121, 83, 90, 49, 63, 56, 7, 32,
			6, 40, 80, 46, 26, 6, 85, 42, 48, 6,
			3, 53, 2, 30, 13, 54, 17, 51, 26, 17,
			55, 35, 26, 20, 25, 51, 2, 47, 31, 37,
			37, 6, 38, 34, 43, 14, 58, 38, 62, 10,
			3, 12, 110, 21, 50, 20, 42, 18, 49, 24,
			96, 29, 3, 97, 102, 25, 16, 0, 153, 231,
			252, 197, 199, 225, 248, 202, 214, 237, 231, 243,
			206, 254, 206, 235, 211, 245, 252, 222, 199, 230,
			232, 218, 128, 250, 217, 198, 223, 238, 227, 251,
			219, 232, 229, 176, 188, 238, 219, 223, 238, 224,
			233, 200, 176, 206, 239, 211, 242, 248, 171, 245,
			220, 253, 242, 207, 226, 194, 162, 249, 250, 206,
			204, 196, 131, 167, 171, 163, 155, 141, 223, 191,
			142, 128, 153, 168, 208, 174, 141, 172, 137, 178,
			149, 163, 150, 157, 187, 133, 155, 170, 167, 191,
			132, 190, 158, 133, 167, 146, 159, 177, 180, 149,
			148, 253, 236, 149, 129, 150, 151, 133, 242, 242,
			136, 143, 160, 177, 183, 136, 147, 165, 170, 228,
			148, 147, 188, 185, 174, 145, 68, 115, 81, 80,
			107, 65, 116, 26, 109, 79, 74, 107, 75, 10,
			78, 23, 94, 3, 0, 126, 97, 104, 91, 74,
			99, 10, 71, 67, 115, 1, 88, 66, 71, 78,
			64, 83, 70, 101, 78, 105, 51, 74, 83, 75,
			98, 82, 75, 52, 79, 77, 77, 104, 117, 121,
			80, 121, 71, 38, 126, 126, 112, 85, 69, 102,
			71, 24, 3, 0, 30, 31, 13, 25, 1, 11,
			3, 9, 3, 4, 15, 72, 59, 46, 44, 49,
			33, 51, 51, 58, 59, 61, 77, 92, 55, 34,
			32, 61, 21, 31, 17, 25, 11, 114, 41, 53,
			33, 43, 33, 47, 33, 34, 39, 42, 62, 62,
			30, 54, 44, 43, 50, 52, 38, 54, 19, 61,
			63, 50, 58, 33, 135, 252, 193, 199, 152, 155,
			129, 254, 202, 202, 208, 209, 207, 201, 195, 139,
			223, 195, 221, 255, 209, 205, 200, 211, 219, 199,
			213, 253, 215, 194, 218, 214, 226, 238, 250, 167,
			235, 247, 233, 203, 237, 241, 244, 239, 239, 243,
			225, 198, 246, 242, 253, 247, 234, 178, 203, 244,
			252, 165, 164, 188, 197, 255, 253, 229, 154, 130,
			134, 142, 177, 170, 173, 174, 204, 134, 152, 132,
			160, 136, 150, 145, 148, 146, 140, 156, 189, 147,
			149, 152, 156, 135, 221, 166, 159, 153, 194, 193,
			231, 152, 160, 160, 190, 191, 165, 163, 165, 156,
			130, 132, 232, 162, 188, 160, 159, 186, 171, 160,
			159, 177, 168, 180, 145, 187, 181, 176, 162, 136,
			145, 154, 121, 5, 77, 81, 75, 106, 92, 68,
			65, 116, 69, 67, 110, 66, 72, 85, 95, 73,
			22, 92, 70, 90, 125, 79, 81, 29, 85, 73,
			83, 85, 91, 65, 126, 100, 101, 100, 103, 107,
			104, 97, 103, 119, 111, 113, 43, 107, 97, 99,
			110, 72, 65, 74, 74, 90, 81, 65, 81, 102,
			98, 99, 115, 121, 96, 86, 5, 5, 28, 27,
			1, 3, 63, 8, 22, 63, 35, 14, 8, 19,
			22, 10, 22, 39, 59, 48, 34, 47, 19, 17,
			27, 16, 9, 4, 24, 4, 1, 5, 58, 36,
			58, 61, 43, 43, 26, 40, 48, 42, 38, 40,
			35, 35, 5, 43, 62, 9, 61, 41, 43, 43,
			61, 63, 62, 54, 0, 62, 58, 62, 55, 44,
			249, 223, 201, 221, 203, 192, 202, 203, 205, 205,
			197, 215, 199, 203, 209, 196, 206, 210, 215, 215,
			203, 209, 215, 211, 221, 196, 222, 226, 194, 210,
			216, 217, 235, 249, 168, 229, 239, 250, 226, 238,
			234, 230, 242, 205, 231, 242, 234, 230, 242, 254,
			234, 185, 248, 240, 240, 249, 247, 225, 210, 228,
			255, 251, 240, 212, 152, 136, 166, 134, 154, 135,
			133, 131, 133, 195, 148, 142, 198, 130, 156, 134,
			150, 142, 156, 156, 222, 134, 153, 137, 220, 211,
			180, 158, 129, 153, 152, 154, 171, 175, 232, 168,
			238, 173, 185, 164, 174, 167, 224, 167, 175, 181,
			183, 177, 244, 154, 188, 189, 243, 146, 172, 141,
			160, 182, 182, 180, 164, 178, 186, 182, 79, 11,
			5, 108, 86, 76, 64, 88, 81, 74, 79, 79,
			118, 70, 80, 77, 26, 23, 72, 86, 73, 90,
			78, 78, 90, 86, 92, 93, 27, 121, 91, 101,
			120, 100, 110, 96, 98, 106, 33, 67, 109, 109,
			73, 111, 114, 98, 118, 100, 121, 111, 113, 111,
			123, 50, 89, 101, 119, 112, 101, 101, 127, 120,
			122, 69, 5, 7, 1, 10, 23, 45, 21, 29,
			3, 16, 19, 76, 37, 8, 9, 8, 27, 21,
			28, 42, 10, 30, 14, 9, 95, 35, 2, 30,
			21, 18, 7, 6, 106, 59, 39, 62, 43, 61,
			63, 37, 39, 47, 44, 97, 107, 17, 33, 55,
			56, 123, 10, 44, 48, 30, 47, 125, 127, 4,
			57, 63, 50, 56, 35, 6, 222, 210, 196, 204,
			142, 231, 197, 201, 198, 198, 206, 129, 139, 240,
			197, 204, 206, 155, 149, 248, 204, 216, 201, 208,
			215, 221, 196, 253, 223, 196, 192, 149, 173, 166,
			198, 230, 222, 253, 227, 235, 235, 239, 229, 166,
			170, 160, 169, 203, 245, 245, 209, 247, 234, 250,
			238, 252, 241, 231, 249, 231, 243, 176, 184, 178,
			199, 174, 144, 140, 141, 154, 152, 132, 141, 141,
			176, 142, 138, 142, 135, 156, 221, 215, 223, 187,
			135, 143, 157, 142, 129, 212, 220, 214, 219, 180,
			155, 152, 167, 170, 166, 173, 233, 227, 238, 173,
			224, 179, 175, 182, 163, 181, 183, 173, 191, 183,
			180, 249, 186, 182, 184, 253, 188, 188, 164, 241,
			165, 163, 181, 167, 94, 104, 71, 92, 66, 75,
			12, 67, 77, 87, 0, 83, 83, 73, 4, 114,
			83, 85, 92, 86, 73, 76, 28, 110, 87, 80,
			69, 67, 95, 67, 77, 15, 42, 92, 97, 103,
			106, 96, 123, 126, 34, 80, 101, 98, 115, 117,
			109, 113, 99, 59, 106, 124, 120, 106, 111, 120,
			118, 51, 100, 121, 115, 55, 119, 125, 11, 5,
			15, 12, 84, 79, 59, 4, 12, 7, 15, 22,
			21, 71, 55, 0, 25, 14, 10, 16, 10, 6,
			92, 15, 23, 21, 5, 2, 19, 19, 84, 1,
			34, 46, 104, 42, 38, 46, 34, 42, 39, 109,
			96, 0, 54, 55, 54, 42, 44, 62, 120, 45,
			54, 58, 124, 60, 54, 62, 57, 63, 118, 39,
			38, 58, 199, 219, 220, 137, 207, 193, 200, 141,
			214, 209, 217, 129, 199, 192, 197, 204, 212, 149,
			207, 208, 208, 219, 211, 202, 193, 215, 213, 215,
			211, 217, 208, 208, 248, 177, 167, 166, 253, 226,
			237, 255, 246, 226, 240, 241, 229, 232, 234, 241,
			232, 244, 244, 218, 241, 234, 240, 249, 178, 253,
			255, 229, 182, 248, 228, 240, 132, 203, 191, 128,
			128, 139, 131, 154, 145, 195, 179, 132, 133, 146,
			150, 140, 142, 130, 194, 217, 173, 143, 144, 156,
			129, 155, 222, 129, 152, 144, 167, 133, 166, 170,
			187, 161, 224, 160, 190, 164, 165, 170, 174, 160,
			170, 233, 180, 171, 189, 184, 183, 172, 178, 187,
			252, 179, 189, 167, 240, 179, 183, 180, 191, 245,
			95, 91, 8, 93, 70, 74, 12, 66, 80, 74,
			71, 72, 72, 70, 72, 5, 73, 75, 84, 88,
			77, 87, 6, 29, 70, 91, 85, 67, 83, 23,
			93, 70, 42, 101, 103, 41, 97, 125, 101, 106,
			107, 109, 97, 109, 38, 116, 116, 105, 123, 104,
			112, 57, 109, 126, 106, 120, 118, 51, 118, 126,
			100, 55, 96, 125, 3, 24, 72, 11, 27, 6,
			0, 9, 1, 12, 21, 13, 2, 71, 10, 10,
			14, 91, 10, 28, 13, 11, 19, 15, 23, 83,
			4, 25, 19, 87, 7, 5, 38, 42, 59, 33,
			116, 111, 47, 34, 55, 47, 36, 97, 40, 40,
			48, 101, 57, 55, 61, 56, 44, 127, 40, 53,
			55, 115, 50, 48, 53, 60, 33, 37, 144, 139,
			251, 217, 194, 206, 223, 197, 146, 141, 144, 143,
			150, 208, 205, 203, 222, 212, 207, 202, 215, 137,
			132, 139, 202, 139, 134, 238, 128, 131, 154, 216,
			249, 226, 168, 175, 168, 175, 255, 249, 227, 241,
			244, 161, 164, 165, 164, 167, 181, 248, 184, 244,
			237, 246, 249, 229, 247, 240, 176, 190, 255, 183,
			182, 183, 202, 196, 153, 139, 206, 192, 130, 130,
			144, 134, 147, 149, 135, 149, 144, 134, 151, 159,
			214, 156, 134, 154, 142, 136, 156, 146, 131, 146,
			153, 130, 152, 145, 234, 165, 167, 189, 238, 188,
			184, 172, 176, 183, 224, 181, 174, 162, 228, 172,
			180, 168, 172, 184, 178, 179, 185, 175, 232, 243,
			180, 180, 179, 167, 249, 185, 67, 69, 67, 19,
			1, 0, 66, 72, 85, 14, 85, 83, 74, 84,
			77, 95, 95, 93, 81, 85, 91, 76, 95, 82,
			86, 86, 89, 85, 83, 89, 64, 92, 126, 114,
			109, 113, 122, 125, 109, 76, 112, 100, 115, 113,
			106, 102, 109, 107, 110, 126, 96, 109, 109, 122,
			111, 110, 123, 124, 126, 90, 115, 110, 100, 116,
			9, 0, 13, 29, 12, 26, 5, 1, 6, 51,
			1, 21, 14, 20, 12, 0, 19, 28, 16, 13,
			14, 16, 15, 20, 6, 26, 31, 31, 31, 25,
			0, 16, 56, 42, 43, 61, 39, 57, 41, 47,
			55, 42, 44, 37, 42, 38, 38, 32, 54, 50,
			63, 55, 49, 45, 57, 14, 51, 37, 53, 37,
			51, 47, 32, 37, 216, 196, 207, 219, 207, 194,
			216, 197, 199, 131, 208, 211, 201, 196, 193, 214,
			201, 155, 220, 208, 218, 159, 210, 210, 198, 147,
			195, 197, 215, 197, 192, 208, 242, 251, 228, 230,
			252, 234, 254, 163, 231, 251, 229, 174, 245, 226,
			232, 224, 249, 239, 180, 187, 237, 247, 243, 239,
			230, 240, 229, 229, 229, 244, 252, 240, 135, 142,
			187, 157, 139, 131, 128, 140, 144, 195, 175, 151,
			131, 149, 136, 132, 131, 171, 153, 139, 141, 154,
			152, 182, 151, 138, 208, 138, 214, 188, 145, 140,
			131, 175, 232, 244, 238, 227, 236, 157, 183, 161,
			172, 168, 165, 140, 161, 188, 250, 230, 248, 170,
			170, 186, 176, 177, 179, 161, 166, 227, 236, 248,
			251, 245, 67, 88, 8, 71, 65, 91, 12, 76,
			2, 72, 78, 78, 81, 73, 4, 70, 85, 86,
			85, 88, 80, 91, 78, 88, 65, 70, 92, 69,
			126, 114, 117, 113, 73, 100, 102, 103, 107, 108,
			120, 100, 109, 109, 67, 110, 104, 115, 97, 107,
			110, 54, 84, 124, 112, 120, 104, 117, 91, 117,
			61, 95, 121, 121, 113, 56, 39, 10, 28, 10,
			6, 94, 66, 93, 76, 0, 8, 20, 8, 12,
			16, 13, 31, 91, 28, 22, 9, 17, 16, 18,
			19, 23, 80, 21, 31, 19, 84, 27, 37, 63,
			104, 58, 58, 46, 62, 57, 33, 34, 46, 34,
			35, 43, 40, 32, 62, 47, 48, 60, 126, 60,
			51, 51, 60, 54, 51, 37, 63, 56, 58, 117,
			204, 202, 193, 197, 203, 203, 216, 197, 199, 131,
			195, 206, 200, 201, 193, 198, 206, 210, 215, 215,
			158, 204, 200, 220, 222, 223, 213, 213, 194, 223,
			209, 149, 249, 238, 250, 255, 235, 253, 172, 238,
			238, 236, 243, 228, 226, 167, 240, 237, 255, 187,
			251, 246, 240, 241, 249, 254, 230, 250, 255, 255,
			182, 242, 245, 231, 134, 146, 156, 129, 139, 207,
			136, 130, 149, 141, 140, 142, 135, 131, 129, 129,
			218, 159, 153, 141, 159, 223, 152, 148, 150, 211,
			158, 158, 130, 215, 153, 148, 190, 168, 160, 233,
			186, 167, 169, 237, 175, 162, 174, 168, 160, 162,
			183, 177, 134, 244, 226, 243, 225, 253, 224, 227,
			174, 165, 181, 163, 191, 177, 173, 188, 68, 76,
			107, 65, 75, 76, 71, 68, 76, 68, 0, 88,
			73, 82, 86, 5, 92, 82, 84, 92, 77, 91,
			83, 74, 92, 95, 95, 80, 82, 13, 82, 84,
			99, 103, 109, 109, 33, 47, 101, 126, 34, 109,
			111, 117, 38, 110, 106, 118, 110, 122, 116, 117,
			123, 123, 60, 117, 119, 97, 117, 49, 121, 113,
			52, 53, 26, 10, 26, 29, 29, 79, 2, 8,
			7, 7, 64, 19, 3, 23, 5, 12, 8, 62,
			14, 28, 12, 6, 8, 21, 27, 29, 23, 81,
			21, 31, 17, 22, 33, 56, 104, 38, 59, 59,
			40, 34, 44, 38, 36, 46, 49, 41, 40, 42,
			59, 63, 49, 55, 57, 13, 57, 45, 51, 58,
			34, 56, 56, 48, 116, 49, 203, 198, 201, 206,
			203, 203, 140, 203, 203, 207, 197, 210, 244, 194,
			212, 196, 211, 201, 152, 218, 209, 210, 204, 209,
			215, 199, 213, 197, 222, 210, 148, 198, 239, 249,
			254, 236, 252, 175, 254, 232, 246, 246, 242, 239,
			227, 227, 164, 235, 245, 187, 238, 252, 236, 236,
			245, 242, 252, 179, 227, 254, 227, 229, 247, 240,
			153, 203, 137, 155, 139, 207, 143, 130, 140, 133,
			137, 134, 147, 149, 129, 129, 159, 141, 157, 139,
			135, 223, 138, 152, 128, 128, 153, 158, 152, 215,
			135, 154, 191, 185, 171, 172, 238, 169, 173, 164,
			174, 166, 164, 225, 238, 252, 228, 236, 157, 190,
			172, 173, 183, 177, 187, 253, 166, 187, 185, 191,
			177, 164, 244, 167, 79, 74, 76, 80, 74, 64,
			91, 67, 78, 76, 65, 69, 28, 67, 75, 75,
			95, 119, 87, 86, 85, 86, 82, 90, 18, 70,
			64, 17, 66, 95, 81, 21, 104, 126, 97, 101,
			106, 93, 105, 108, 102, 106, 110, 102, 38, 115,
			108, 96, 58, 118, 121, 119, 119, 121, 121, 110,
			102, 125, 127, 101, 54, 114, 122, 122, 31, 12,
			0, 73, 10, 6, 31, 6, 66, 16, 16, 0,
			5, 2, 94, 69, 90, 29, 10, 28, 27, 83,
			92, 93, 28, 22, 21, 21, 19, 19, 53, 25,
			38, 36, 43, 40, 58, 38, 34, 42, 98, 37,
			41, 45, 35, 52, 5, 41, 40, 62, 57, 61,
			39, 127, 53, 51, 33, 39, 49, 61, 58, 50,
			48, 17, 197, 220, 198, 197, 193, 206, 200, 141,
			193, 204, 205, 209, 202, 194, 208, 192, 154, 210,
			203, 153, 208, 208, 156, 209, 221, 221, 215, 212,
			196, 151, 213, 195, 235, 226, 228, 232, 236, 227,
			233, 238, 237, 246, 236, 229, 166, 233, 235, 241,
			186, 233, 253, 248, 253, 247, 188, 233, 250, 246,
			176, 226, 243, 229, 226, 240, 152, 159, 128, 140,
			206, 156, 137, 159, 148, 134, 146, 193, 146, 136,
			139, 142, 218, 143, 151, 150, 222, 147, 147, 147,
			149, 211, 132, 158, 214, 133, 145, 134, 186, 164,
			166, 173, 186, 167, 169, 237, 166, 172, 183, 175,
			170, 168, 165, 161, 250, 175, 177, 180, 187, 187,
			252, 178, 167, 167, 162, 180, 167, 162, 177, 166,
			94, 11, 70, 76, 88, 74, 94, 13, 80, 66,
			78, 85, 78, 66, 4, 102, 126, 117, 31, 74,
			30, 93, 83, 73, 18, 67, 66, 94, 66, 82,
			87, 65, 99, 100, 102, 41, 38, 76, 96, 98,
			119, 103, 102, 109, 103, 117, 97, 44, 58, 121,
			116, 118, 125, 116, 121, 121, 50, 103, 120, 120,
			101, 55, 112, 112, 28, 2, 11, 12, 85, 79,
			24, 31, 27, 67, 1, 6, 7, 14, 10, 69,
			21, 21, 88, 24, 94, 27, 21, 27, 20, 22,
			2, 20, 24, 3, 84, 27, 47, 63, 63, 38,
			60, 36, 108, 34, 48, 99, 55, 40, 50, 47,
			43, 48, 46, 123, 57, 121, 8, 15, 18, 125,
			122, 50, 62, 53, 118, 119, 59, 35, 207, 217,
			136, 237, 224, 252, 129, 194, 212, 198, 210, 140,
			238, 243, 240, 245, 233, 146, 135, 215, 223, 210,
			217, 128, 148, 199, 201, 193, 211, 138, 245, 212,
			233, 232, 237, 249, 250, 238, 252, 253, 238, 234,
			227, 224, 242, 238, 235, 235, 181, 255, 246, 234,
			179, 245, 239, 242, 252, 218, 254, 231, 247, 251,
			253, 241, 202, 155, 137, 157, 134, 193, 136, 130,
			149, 141, 140, 142, 135, 131, 201, 149, 136, 148,
			159, 139, 155, 140, 143, 211, 130, 146, 130, 133,
			159, 153, 130, 148, 166, 162, 172, 233, 190, 174,
			190, 185, 171, 162, 172, 225, 160, 174, 168, 160,
			224, 251, 170, 188, 173, 171, 189, 175, 166, 186,
			190, 182, 246, 179, 187, 162, 68, 71, 71, 72,
			74, 15, 94, 72, 86, 86, 82, 79, 67, 67,
			4, 109, 110, 111, 104, 25, 90, 80, 75, 83,
			94, 92, 81, 85, 22, 68, 64, 84, 102, 103,
			109, 109, 52, 47, 98, 98, 34, 103, 97, 117,
			103, 39, 118, 96, 121, 126, 113, 111, 123, 123,
			38, 61, 118, 124, 103, 127, 122, 120, 117, 113,
			15, 15, 72, 73, 12, 22, 24, 8, 17, 79,
			64, 4, 30, 23, 1, 6, 14, 30, 28, 89,
			68, 95, 21, 19, 17, 28, 29, 1, 26, 18,
			0, 16, 102, 107, 47, 38, 58, 111, 108, 47,
			59, 55, 37, 50, 34, 40, 51, 43, 54, 52,
			57, 61, 115, 60, 51, 48, 34, 63, 53, 37,
			51, 34, 38, 57, 249, 238, 252, 246, 239, 236,
			248, 228, 244, 234, 244, 248, 213, 211, 197, 209,
			223, 223, 221, 205, 223, 214, 208, 206, 198, 202,
			192, 212, 197, 195, 213, 199, 254, 255, 225, 228,
			235, 252, 248, 236, 239, 243, 243, 237, 231, 245,
			227, 224, 197, 242, 245, 248, 249, 250, 240, 252,
			224, 244, 245, 206, 226, 242, 236, 225, 153, 134,
			137, 133, 130, 176, 133, 128, 131, 132, 133, 146,
			139, 134, 136, 137, 165, 143, 157, 129, 138, 158,
			143, 142, 151, 135, 131, 147, 131, 131, 128, 154,
			164, 184, 172, 160, 189, 172, 163, 191, 166, 238,
			169, 177, 165, 234, 128, 172, 169, 184, 183, 171,
			186, 255, 181, 174, 242, 189, 191, 165, 246, 165,
			161, 187, 68, 66, 70, 78, 9, 8, 11, 120,
			81, 70, 82, 12, 103, 64, 65, 75, 78, 120,
			87, 87, 74, 90, 82, 73, 31, 103, 73, 65,
			83, 64, 71, 15, 37, 36, 101, 108, 125, 124,
			109, 106, 103, 44, 84, 100, 126, 115, 70, 108,
			116, 122, 106, 96, 93, 115, 115, 110, 119, 103,
			120, 112, 98, 55, 103, 122, 9, 0, 13, 29,
			78, 6, 31, 77, 1, 15, 15, 18, 3, 3,
			16, 13, 19, 8, 88, 27, 11, 22, 16, 25,
			82, 27, 17, 2, 86, 25, 27, 85, 11, 57,
			43, 105, 61, 63, 32, 44, 49, 43, 96, 39,
			41, 43, 32, 32, 40, 56, 55, 44, 50, 59,
			124, 51, 61, 39, 112, 53, 57, 32, 58, 57,
			197, 202, 204, 137, 218, 199, 205, 217, 130, 208,
			208, 205, 199, 212, 204, 159, 154, 207, 208, 216,
			202, 159, 218, 212, 222, 214, 144, 216, 197, 151,
			218, 218, 254, 171, 233, 169, 222, 193, 203, 238,
			237, 246, 236, 229, 166, 233, 235, 241, 186, 236,
			234, 240, 234, 250, 188, 233, 250, 246, 176, 226,
			230, 251, 245, 230, 130, 209, 200, 199, 139, 151,
			137, 133, 150, 151, 144, 146, 220, 200, 203, 149,
			136, 148, 156, 212, 159, 143, 149, 208, 132, 194,
			222, 130, 130, 146, 152, 153, 171, 185, 174, 167,
			224, 171, 169, 187, 237, 176, 180, 164, 170, 171,
			165, 183, 245, 183, 185, 172, 176, 188, 180, 184,
			160, 252, 166, 227, 249, 162, 164, 177, 75, 95,
			77, 6, 85, 91, 77, 95, 69, 70, 84, 92,
			9, 92, 69, 87, 89, 83, 69, 22, 69, 73,
			89, 79, 65, 90, 95, 95, 75, 76, 64, 84,
			120, 108, 109, 125, 115, 116, 109, 127, 97, 107,
			125, 122, 112, 98, 118, 118, 115, 116, 118, 100,
			112, 112, 60, 104, 98, 119, 113, 101, 115, 55,
			124, 116, 25, 75, 10, 12, 11, 1, 76, 14,
			10, 6, 3, 10, 3, 3, 68, 3, 21, 9,
			88, 0, 27, 11, 8, 21, 27, 0, 80, 4,
			6, 19, 21, 1, 47, 107, 32, 40, 61, 111,
			34, 34, 98, 42, 46, 50, 50, 38, 40, 41,
			63, 41, 120, 63, 49, 45, 124, 36, 61, 38,
			34, 113, 37, 46, 39, 33, 207, 198, 220, 193,
			199, 220, 140, 216, 210, 199, 193, 213, 195, 135,
			205, 214, 154, 206, 214, 202, 215, 216, 210, 216,
			214, 147, 209, 223, 210, 151, 195, 212, 249, 171,
			250, 236, 232, 250, 255, 232, 230, 224, 239, 244,
			234, 227, 164, 235, 245, 239, 184, 253, 241, 232,
			242, 241, 253, 242, 244, 177, 226, 255, 241, 181,
			159, 155, 140, 136, 154, 138, 214, 205, 150, 139,
			133, 193, 147, 151, 128, 132, 142, 158, 223, 138,
			222, 140, 149, 154, 156, 146, 132, 132, 132, 146,
			212, 145, 163, 175, 232, 167, 161, 187, 236, 160,
			163, 183, 163, 169, 230, 166, 170, 161, 250, 172,
			185, 170, 254, 173, 185, 187, 167, 160, 181, 181,
			165, 163, 177, 185, 70, 74, 90, 4, 91, 95,
			72, 76, 86, 70, 13, 66, 73, 82, 72, 65,
			26, 85, 87, 77, 30, 76, 72, 92, 85, 86,
			16, 69, 94, 82, 20, 64, 122, 111, 105, 125,
			107, 53, 44, 94, 118, 102, 108, 109, 103, 117,
			41, 112, 106, 127, 121, 109, 123, 49, 113, 110,
			123, 112, 127, 100, 122, 115, 52, 123, 5, 31,
			72, 30, 28, 6, 24, 8, 66, 23, 8, 4,
			70, 18, 20, 1, 27, 15, 29, 67, 94, 81,
			6, 20, 2, 16, 31, 4, 26, 19, 84, 27,
			37, 63, 104, 60, 32, 63, 45, 46, 41, 99,
			52, 41, 35, 103, 49, 53, 62, 58, 44, 60,
			100, 127, 118, 115, 120, 39, 56, 52, 118, 34,
			36, 49, 203, 223, 205, 137, 222, 206, 207, 198,
			195, 196, 197, 129, 206, 198, 192, 133, 212, 212,
			152, 208, 208, 204, 200, 220, 222, 223, 213, 195,
			150, 222, 218, 198, 227, 239, 237, 255, 235, 253,
			255, 228, 237, 237, 228, 228, 245, 243, 237, 235,
			251, 239, 241, 246, 240, 252, 243, 243, 252, 246,
			243, 229, 255, 248, 250, 230, 152, 142, 152, 136,
			135, 157, 142, 140, 145, 134, 164, 136, 148, 143,
			129, 132, 158, 158, 138, 138, 147, 154, 143, 142,
			147, 148, 149, 149, 151, 131, 149, 150, 166, 162,
			173, 167, 186, 134, 168, 172, 161, 183, 169, 183,
			175, 179, 189, 181, 187, 174, 171, 188, 186
		};

		internal static string[] 5 = new string[569];

		private static string 6(int P_0, int P_1, int P_2)
		{
			string @string = Encoding.UTF8.GetString(4, P_1, P_2);
			5[P_0] = @string;
			return @string;
		}

		public static string A()
		{
			return 5[0] ?? 6(0, 0, 98);
		}

		public static string a()
		{
			return 5[1] ?? 6(1, 98, 17);
		}

		public static string B()
		{
			return 5[2] ?? 6(2, 115, 30);
		}

		public static string b()
		{
			return 5[3] ?? 6(3, 145, 40);
		}

		public static string C()
		{
			return 5[4] ?? 6(4, 185, 31);
		}

		public static string c()
		{
			return 5[5] ?? 6(5, 216, 28);
		}

		public static string D()
		{
			return 5[6] ?? 6(6, 244, 30);
		}

		public static string d()
		{
			return 5[7] ?? 6(7, 274, 35);
		}

		public static string E()
		{
			return 5[8] ?? 6(8, 309, 82);
		}

		public static string e()
		{
			return 5[9] ?? 6(9, 391, 51);
		}

		public static string F()
		{
			return 5[10] ?? 6(10, 442, 59);
		}

		public static string f()
		{
			return 5[11] ?? 6(11, 501, 43);
		}

		public static string G()
		{
			return 5[12] ?? 6(12, 544, 44);
		}

		public static string g()
		{
			return 5[13] ?? 6(13, 588, 9);
		}

		public static string H()
		{
			return 5[14] ?? 6(14, 597, 8);
		}

		public static string h()
		{
			return 5[15] ?? 6(15, 605, 4);
		}

		public static string I()
		{
			return 5[16] ?? 6(16, 609, 12);
		}

		public static string i()
		{
			return 5[17] ?? 6(17, 621, 22);
		}

		public static string J()
		{
			return 5[18] ?? 6(18, 643, 7);
		}

		public static string j()
		{
			return 5[19] ?? 6(19, 650, 5);
		}

		public static string K()
		{
			return 5[20] ?? 6(20, 655, 8);
		}

		public static string k()
		{
			return 5[21] ?? 6(21, 663, 4);
		}

		public static string L()
		{
			return 5[22] ?? 6(22, 667, 20);
		}

		public static string l()
		{
			return 5[23] ?? 6(23, 687, 17);
		}

		public static string M()
		{
			return 5[24] ?? 6(24, 704, 26);
		}

		public static string m()
		{
			return 5[25] ?? 6(25, 730, 12);
		}

		public static string N()
		{
			return 5[26] ?? 6(26, 742, 18);
		}

		public static string n()
		{
			return 5[27] ?? 6(27, 760, 6);
		}

		public static string O()
		{
			return 5[28] ?? 6(28, 766, 14);
		}

		public static string o()
		{
			return 5[29] ?? 6(29, 780, 8);
		}

		public static string P()
		{
			return 5[30] ?? 6(30, 788, 14);
		}

		public static string p()
		{
			return 5[31] ?? 6(31, 802, 14);
		}

		public static string Q()
		{
			return 5[32] ?? 6(32, 816, 15);
		}

		public static string q()
		{
			return 5[33] ?? 6(33, 831, 16);
		}

		public static string R()
		{
			return 5[34] ?? 6(34, 847, 22);
		}

		public static string r()
		{
			return 5[35] ?? 6(35, 869, 12);
		}

		public static string S()
		{
			return 5[36] ?? 6(36, 881, 12);
		}

		public static string s()
		{
			return 5[37] ?? 6(37, 893, 14);
		}

		public static string T()
		{
			return 5[38] ?? 6(38, 907, 20);
		}

		public static string t()
		{
			return 5[39] ?? 6(39, 927, 23);
		}

		public static string U()
		{
			return 5[40] ?? 6(40, 950, 15);
		}

		public static string u()
		{
			return 5[41] ?? 6(41, 965, 13);
		}

		public static string V()
		{
			return 5[42] ?? 6(42, 978, 12);
		}

		public static string v()
		{
			return 5[43] ?? 6(43, 990, 14);
		}

		public static string W()
		{
			return 5[44] ?? 6(44, 1004, 19);
		}

		public static string w()
		{
			return 5[45] ?? 6(45, 1023, 18);
		}

		public static string X()
		{
			return 5[46] ?? 6(46, 1041, 17);
		}

		public static string x()
		{
			return 5[47] ?? 6(47, 1058, 16);
		}

		public static string Y()
		{
			return 5[48] ?? 6(48, 1074, 17);
		}

		public static string y()
		{
			return 5[49] ?? 6(49, 1091, 17);
		}

		public static string Z()
		{
			return 5[50] ?? 6(50, 1108, 17);
		}

		public static string z()
		{
			return 5[51] ?? 6(51, 1125, 15);
		}

		public static string aA()
		{
			return 5[52] ?? 6(52, 1140, 11);
		}

		public static string aa()
		{
			return 5[53] ?? 6(53, 1151, 8);
		}

		public static string aB()
		{
			return 5[54] ?? 6(54, 1159, 14);
		}

		public static string ab()
		{
			return 5[55] ?? 6(55, 1173, 15);
		}

		public static string aC()
		{
			return 5[56] ?? 6(56, 1188, 17);
		}

		public static string ac()
		{
			return 5[57] ?? 6(57, 1205, 15);
		}

		public static string aD()
		{
			return 5[58] ?? 6(58, 1220, 17);
		}

		public static string ad()
		{
			return 5[59] ?? 6(59, 1237, 11);
		}

		public static string aE()
		{
			return 5[60] ?? 6(60, 1248, 11);
		}

		public static string ae()
		{
			return 5[61] ?? 6(61, 1259, 12);
		}

		public static string aF()
		{
			return 5[62] ?? 6(62, 1271, 16);
		}

		public static string af()
		{
			return 5[63] ?? 6(63, 1287, 17);
		}

		public static string aG()
		{
			return 5[64] ?? 6(64, 1304, 19);
		}

		public static string ag()
		{
			return 5[65] ?? 6(65, 1323, 16);
		}

		public static string aH()
		{
			return 5[66] ?? 6(66, 1339, 17);
		}

		public static string ah()
		{
			return 5[67] ?? 6(67, 1356, 31);
		}

		public static string aI()
		{
			return 5[68] ?? 6(68, 1387, 11);
		}

		public static string ai()
		{
			return 5[69] ?? 6(69, 1398, 11);
		}

		public static string aJ()
		{
			return 5[70] ?? 6(70, 1409, 14);
		}

		public static string aj()
		{
			return 5[71] ?? 6(71, 1423, 10);
		}

		public static string aK()
		{
			return 5[72] ?? 6(72, 1433, 10);
		}

		public static string ak()
		{
			return 5[73] ?? 6(73, 1443, 13);
		}

		public static string aL()
		{
			return 5[74] ?? 6(74, 1456, 13);
		}

		public static string al()
		{
			return 5[75] ?? 6(75, 1469, 12);
		}

		public static string aM()
		{
			return 5[76] ?? 6(76, 1481, 16);
		}

		public static string am()
		{
			return 5[77] ?? 6(77, 1497, 13);
		}

		public static string aN()
		{
			return 5[78] ?? 6(78, 1510, 16);
		}

		public static string an()
		{
			return 5[79] ?? 6(79, 1526, 15);
		}

		public static string aO()
		{
			return 5[80] ?? 6(80, 1541, 17);
		}

		public static string ao()
		{
			return 5[81] ?? 6(81, 1558, 19);
		}

		public static string aP()
		{
			return 5[82] ?? 6(82, 1577, 22);
		}

		public static string ap()
		{
			return 5[83] ?? 6(83, 1599, 17);
		}

		public static string aQ()
		{
			return 5[84] ?? 6(84, 1616, 18);
		}

		public static string aq()
		{
			return 5[85] ?? 6(85, 1634, 10);
		}

		public static string aR()
		{
			return 5[86] ?? 6(86, 1644, 7);
		}

		public static string ar()
		{
			return 5[87] ?? 6(87, 1651, 13);
		}

		public static string aS()
		{
			return 5[88] ?? 6(88, 1664, 10);
		}

		public static string @as()
		{
			return 5[89] ?? 6(89, 1674, 9);
		}

		public static string aT()
		{
			return 5[90] ?? 6(90, 1683, 17);
		}

		public static string at()
		{
			return 5[91] ?? 6(91, 1700, 19);
		}

		public static string aU()
		{
			return 5[92] ?? 6(92, 1719, 5);
		}

		public static string au()
		{
			return 5[93] ?? 6(93, 1724, 0);
		}

		public static string aV()
		{
			return 5[94] ?? 6(94, 1724, 11);
		}

		public static string av()
		{
			return 5[95] ?? 6(95, 1735, 8);
		}

		public static string aW()
		{
			return 5[96] ?? 6(96, 1743, 9);
		}

		public static string aw()
		{
			return 5[97] ?? 6(97, 1752, 15);
		}

		public static string aX()
		{
			return 5[98] ?? 6(98, 1767, 13);
		}

		public static string ax()
		{
			return 5[99] ?? 6(99, 1780, 7);
		}

		public static string aY()
		{
			return 5[100] ?? 6(100, 1787, 5);
		}

		public static string ay()
		{
			return 5[101] ?? 6(101, 1792, 11);
		}

		public static string aZ()
		{
			return 5[102] ?? 6(102, 1803, 2);
		}

		public static string az()
		{
			return 5[103] ?? 6(103, 1805, 1);
		}

		public static string BA()
		{
			return 5[104] ?? 6(104, 1806, 44);
		}

		public static string Ba()
		{
			return 5[105] ?? 6(105, 1850, 23);
		}

		public static string BB()
		{
			return 5[106] ?? 6(106, 1873, 152);
		}

		public static string Bb()
		{
			return 5[107] ?? 6(107, 2025, 2);
		}

		public static string BC()
		{
			return 5[108] ?? 6(108, 2027, 2);
		}

		public static string Bc()
		{
			return 5[109] ?? 6(109, 2029, 17);
		}

		public static string BD()
		{
			return 5[110] ?? 6(110, 2046, 1);
		}

		public static string Bd()
		{
			return 5[111] ?? 6(111, 2047, 17);
		}

		public static string BE()
		{
			return 5[112] ?? 6(112, 2064, 11);
		}

		public static string Be()
		{
			return 5[113] ?? 6(113, 2075, 7);
		}

		public static string BF()
		{
			return 5[114] ?? 6(114, 2082, 4);
		}

		public static string Bf()
		{
			return 5[115] ?? 6(115, 2086, 4);
		}

		public static string BG()
		{
			return 5[116] ?? 6(116, 2090, 7);
		}

		public static string Bg()
		{
			return 5[117] ?? 6(117, 2097, 7);
		}

		public static string BH()
		{
			return 5[118] ?? 6(118, 2104, 16);
		}

		public static string Bh()
		{
			return 5[119] ?? 6(119, 2120, 9);
		}

		public static string BI()
		{
			return 5[120] ?? 6(120, 2129, 3);
		}

		public static string Bi()
		{
			return 5[121] ?? 6(121, 2132, 3);
		}

		public static string BJ()
		{
			return 5[122] ?? 6(122, 2135, 6);
		}

		public static string Bj()
		{
			return 5[123] ?? 6(123, 2141, 5);
		}

		public static string BK()
		{
			return 5[124] ?? 6(124, 2146, 5);
		}

		public static string Bk()
		{
			return 5[125] ?? 6(125, 2151, 3);
		}

		public static string BL()
		{
			return 5[126] ?? 6(126, 2154, 5);
		}

		public static string Bl()
		{
			return 5[127] ?? 6(127, 2159, 1);
		}

		public static string BM()
		{
			return 5[128] ?? 6(128, 2160, 5);
		}

		public static string Bm()
		{
			return 5[129] ?? 6(129, 2165, 5);
		}

		public static string BN()
		{
			return 5[130] ?? 6(130, 2170, 5);
		}

		public static string Bn()
		{
			return 5[131] ?? 6(131, 2175, 5);
		}

		public static string BO()
		{
			return 5[132] ?? 6(132, 2180, 6);
		}

		public static string Bo()
		{
			return 5[133] ?? 6(133, 2186, 6);
		}

		public static string BP()
		{
			return 5[134] ?? 6(134, 2192, 6);
		}

		public static string Bp()
		{
			return 5[135] ?? 6(135, 2198, 6);
		}

		public static string BQ()
		{
			return 5[136] ?? 6(136, 2204, 6);
		}

		public static string Bq()
		{
			return 5[137] ?? 6(137, 2210, 3);
		}

		public static string BR()
		{
			return 5[138] ?? 6(138, 2213, 3);
		}

		public static string Br()
		{
			return 5[139] ?? 6(139, 2216, 3);
		}

		public static string BS()
		{
			return 5[140] ?? 6(140, 2219, 9);
		}

		public static string Bs()
		{
			return 5[141] ?? 6(141, 2228, 9);
		}

		public static string BT()
		{
			return 5[142] ?? 6(142, 2237, 9);
		}

		public static string Bt()
		{
			return 5[143] ?? 6(143, 2246, 4);
		}

		public static string BU()
		{
			return 5[144] ?? 6(144, 2250, 4);
		}

		public static string Bu()
		{
			return 5[145] ?? 6(145, 2254, 4);
		}

		public static string BV()
		{
			return 5[146] ?? 6(146, 2258, 8);
		}

		public static string Bv()
		{
			return 5[147] ?? 6(147, 2266, 8);
		}

		public static string BW()
		{
			return 5[148] ?? 6(148, 2274, 2);
		}

		public static string Bw()
		{
			return 5[149] ?? 6(149, 2276, 7);
		}

		public static string BX()
		{
			return 5[150] ?? 6(150, 2283, 7);
		}

		public static string Bx()
		{
			return 5[151] ?? 6(151, 2290, 10);
		}

		public static string BY()
		{
			return 5[152] ?? 6(152, 2300, 10);
		}

		public static string By()
		{
			return 5[153] ?? 6(153, 2310, 11);
		}

		public static string BZ()
		{
			return 5[154] ?? 6(154, 2321, 38);
		}

		public static string Bz()
		{
			return 5[155] ?? 6(155, 2359, 27);
		}

		public static string bA()
		{
			return 5[156] ?? 6(156, 2386, 1);
		}

		public static string ba()
		{
			return 5[157] ?? 6(157, 2387, 32);
		}

		public static string bB()
		{
			return 5[158] ?? 6(158, 2419, 23);
		}

		public static string bb()
		{
			return 5[159] ?? 6(159, 2442, 17);
		}

		public static string bC()
		{
			return 5[160] ?? 6(160, 2459, 4);
		}

		public static string bc()
		{
			return 5[161] ?? 6(161, 2463, 12);
		}

		public static string bD()
		{
			return 5[162] ?? 6(162, 2475, 11);
		}

		public static string bd()
		{
			return 5[163] ?? 6(163, 2486, 3);
		}

		public static string bE()
		{
			return 5[164] ?? 6(164, 2489, 18);
		}

		public static string be()
		{
			return 5[165] ?? 6(165, 2507, 6);
		}

		public static string bF()
		{
			return 5[166] ?? 6(166, 2513, 5);
		}

		public static string bf()
		{
			return 5[167] ?? 6(167, 2518, 1);
		}

		public static string bG()
		{
			return 5[168] ?? 6(168, 2519, 4);
		}

		public static string bg()
		{
			return 5[169] ?? 6(169, 2523, 6);
		}

		public static string bH()
		{
			return 5[170] ?? 6(170, 2529, 7);
		}

		public static string bh()
		{
			return 5[171] ?? 6(171, 2536, 2);
		}

		public static string bI()
		{
			return 5[172] ?? 6(172, 2538, 3);
		}

		public static string bi()
		{
			return 5[173] ?? 6(173, 2541, 4);
		}

		public static string bJ()
		{
			return 5[174] ?? 6(174, 2545, 26);
		}

		public static string bj()
		{
			return 5[175] ?? 6(175, 2571, 17);
		}

		public static string bK()
		{
			return 5[176] ?? 6(176, 2588, 24);
		}

		public static string bk()
		{
			return 5[177] ?? 6(177, 2612, 64);
		}

		public static string bL()
		{
			return 5[178] ?? 6(178, 2676, 15);
		}

		public static string bl()
		{
			return 5[179] ?? 6(179, 2691, 2);
		}

		public static string bM()
		{
			return 5[180] ?? 6(180, 2693, 12);
		}

		public static string bm()
		{
			return 5[181] ?? 6(181, 2705, 9);
		}

		public static string bN()
		{
			return 5[182] ?? 6(182, 2714, 9);
		}

		public static string bn()
		{
			return 5[183] ?? 6(183, 2723, 14);
		}

		public static string bO()
		{
			return 5[184] ?? 6(184, 2737, 16);
		}

		public static string bo()
		{
			return 5[185] ?? 6(185, 2753, 6);
		}

		public static string bP()
		{
			return 5[186] ?? 6(186, 2759, 15);
		}

		public static string bp()
		{
			return 5[187] ?? 6(187, 2774, 8);
		}

		public static string bQ()
		{
			return 5[188] ?? 6(188, 2782, 35);
		}

		public static string bq()
		{
			return 5[189] ?? 6(189, 2817, 25);
		}

		public static string bR()
		{
			return 5[190] ?? 6(190, 2842, 32);
		}

		public static string br()
		{
			return 5[191] ?? 6(191, 2874, 12);
		}

		public static string bS()
		{
			return 5[192] ?? 6(192, 2886, 10);
		}

		public static string bs()
		{
			return 5[193] ?? 6(193, 2896, 5);
		}

		public static string bT()
		{
			return 5[194] ?? 6(194, 2901, 25);
		}

		public static string bt()
		{
			return 5[195] ?? 6(195, 2926, 7);
		}

		public static string bU()
		{
			return 5[196] ?? 6(196, 2933, 4);
		}

		public static string bu()
		{
			return 5[197] ?? 6(197, 2937, 7);
		}

		public static string bV()
		{
			return 5[198] ?? 6(198, 2944, 10);
		}

		public static string bv()
		{
			return 5[199] ?? 6(199, 2954, 1);
		}

		public static string bW()
		{
			return 5[200] ?? 6(200, 2955, 1);
		}

		public static string bw()
		{
			return 5[201] ?? 6(201, 2956, 5);
		}

		public static string bX()
		{
			return 5[202] ?? 6(202, 2961, 24);
		}

		public static string bx()
		{
			return 5[203] ?? 6(203, 2985, 3);
		}

		public static string bY()
		{
			return 5[204] ?? 6(204, 2988, 30);
		}

		public static string by()
		{
			return 5[205] ?? 6(205, 3018, 4);
		}

		public static string bZ()
		{
			return 5[206] ?? 6(206, 3022, 4);
		}

		public static string bz()
		{
			return 5[207] ?? 6(207, 3026, 23);
		}

		public static string CA()
		{
			return 5[208] ?? 6(208, 3049, 5);
		}

		public static string Ca()
		{
			return 5[209] ?? 6(209, 3054, 31);
		}

		public static string CB()
		{
			return 5[210] ?? 6(210, 3085, 4);
		}

		public static string Cb()
		{
			return 5[211] ?? 6(211, 3089, 9);
		}

		public static string CC()
		{
			return 5[212] ?? 6(212, 3098, 4);
		}

		public static string Cc()
		{
			return 5[213] ?? 6(213, 3102, 10);
		}

		public static string CD()
		{
			return 5[214] ?? 6(214, 3112, 5);
		}

		public static string Cd()
		{
			return 5[215] ?? 6(215, 3117, 4);
		}

		public static string CE()
		{
			return 5[216] ?? 6(216, 3121, 9);
		}

		public static string Ce()
		{
			return 5[217] ?? 6(217, 3130, 4);
		}

		public static string CF()
		{
			return 5[218] ?? 6(218, 3134, 13);
		}

		public static string Cf()
		{
			return 5[219] ?? 6(219, 3147, 5);
		}

		public static string CG()
		{
			return 5[220] ?? 6(220, 3152, 10);
		}

		public static string Cg()
		{
			return 5[221] ?? 6(221, 3162, 4);
		}

		public static string CH()
		{
			return 5[222] ?? 6(222, 3166, 9);
		}

		public static string Ch()
		{
			return 5[223] ?? 6(223, 3175, 4);
		}

		public static string CI()
		{
			return 5[224] ?? 6(224, 3179, 12);
		}

		public static string Ci()
		{
			return 5[225] ?? 6(225, 3191, 5);
		}

		public static string CJ()
		{
			return 5[226] ?? 6(226, 3196, 9);
		}

		public static string Cj()
		{
			return 5[227] ?? 6(227, 3205, 6);
		}

		public static string CK()
		{
			return 5[228] ?? 6(228, 3211, 10);
		}

		public static string Ck()
		{
			return 5[229] ?? 6(229, 3221, 4);
		}

		public static string CL()
		{
			return 5[230] ?? 6(230, 3225, 8);
		}

		public static string Cl()
		{
			return 5[231] ?? 6(231, 3233, 4);
		}

		public static string CM()
		{
			return 5[232] ?? 6(232, 3237, 8);
		}

		public static string Cm()
		{
			return 5[233] ?? 6(233, 3245, 4);
		}

		public static string CN()
		{
			return 5[234] ?? 6(234, 3249, 9);
		}

		public static string Cn()
		{
			return 5[235] ?? 6(235, 3258, 5);
		}

		public static string CO()
		{
			return 5[236] ?? 6(236, 3263, 10);
		}

		public static string Co()
		{
			return 5[237] ?? 6(237, 3273, 4);
		}

		public static string CP()
		{
			return 5[238] ?? 6(238, 3277, 10);
		}

		public static string Cp()
		{
			return 5[239] ?? 6(239, 3287, 4);
		}

		public static string CQ()
		{
			return 5[240] ?? 6(240, 3291, 9);
		}

		public static string Cq()
		{
			return 5[241] ?? 6(241, 3300, 4);
		}

		public static string CR()
		{
			return 5[242] ?? 6(242, 3304, 25);
		}

		public static string Cr()
		{
			return 5[243] ?? 6(243, 3329, 4);
		}

		public static string CS()
		{
			return 5[244] ?? 6(244, 3333, 5);
		}

		public static string Cs()
		{
			return 5[245] ?? 6(245, 3338, 16);
		}

		public static string CT()
		{
			return 5[246] ?? 6(246, 3354, 19);
		}

		public static string Ct()
		{
			return 5[247] ?? 6(247, 3373, 9);
		}

		public static string CU()
		{
			return 5[248] ?? 6(248, 3382, 7);
		}

		public static string Cu()
		{
			return 5[249] ?? 6(249, 3389, 13);
		}

		public static string CV()
		{
			return 5[250] ?? 6(250, 3402, 7);
		}

		public static string Cv()
		{
			return 5[251] ?? 6(251, 3409, 24);
		}

		public static string CW()
		{
			return 5[252] ?? 6(252, 3433, 5);
		}

		public static string Cw()
		{
			return 5[253] ?? 6(253, 3438, 5);
		}

		public static string CX()
		{
			return 5[254] ?? 6(254, 3443, 11);
		}

		public static string Cx()
		{
			return 5[255] ?? 6(255, 3454, 9);
		}

		public static string CY()
		{
			return 5[256] ?? 6(256, 3463, 29);
		}

		public static string Cy()
		{
			return 5[257] ?? 6(257, 3492, 22);
		}

		public static string CZ()
		{
			return 5[258] ?? 6(258, 3514, 8);
		}

		public static string Cz()
		{
			return 5[259] ?? 6(259, 3522, 17);
		}

		public static string cA()
		{
			return 5[260] ?? 6(260, 3539, 1);
		}

		public static string ca()
		{
			return 5[261] ?? 6(261, 3540, 2);
		}

		public static string cB()
		{
			return 5[262] ?? 6(262, 3542, 2);
		}

		public static string cb()
		{
			return 5[263] ?? 6(263, 3544, 2);
		}

		public static string cC()
		{
			return 5[264] ?? 6(264, 3546, 2);
		}

		public static string cc()
		{
			return 5[265] ?? 6(265, 3548, 2);
		}

		public static string cD()
		{
			return 5[266] ?? 6(266, 3550, 1);
		}

		public static string cd()
		{
			return 5[267] ?? 6(267, 3551, 38);
		}

		public static string cE()
		{
			return 5[268] ?? 6(268, 3589, 2);
		}

		public static string ce()
		{
			return 5[269] ?? 6(269, 3591, 9);
		}

		public static string cF()
		{
			return 5[270] ?? 6(270, 3600, 8);
		}

		public static string cf()
		{
			return 5[271] ?? 6(271, 3608, 4);
		}

		public static string cG()
		{
			return 5[272] ?? 6(272, 3612, 33);
		}

		public static string cg()
		{
			return 5[273] ?? 6(273, 3645, 26);
		}

		public static string cH()
		{
			return 5[274] ?? 6(274, 3671, 2);
		}

		public static string ch()
		{
			return 5[275] ?? 6(275, 3673, 34);
		}

		public static string cI()
		{
			return 5[276] ?? 6(276, 3707, 29);
		}

		public static string ci()
		{
			return 5[277] ?? 6(277, 3736, 39);
		}

		public static string cJ()
		{
			return 5[278] ?? 6(278, 3775, 6);
		}

		public static string cj()
		{
			return 5[279] ?? 6(279, 3781, 21);
		}

		public static string cK()
		{
			return 5[280] ?? 6(280, 3802, 26);
		}

		public static string ck()
		{
			return 5[281] ?? 6(281, 3828, 111);
		}

		public static string cL()
		{
			return 5[282] ?? 6(282, 3939, 6);
		}

		public static string cl()
		{
			return 5[283] ?? 6(283, 3945, 3);
		}

		public static string cM()
		{
			return 5[284] ?? 6(284, 3948, 15);
		}

		public static string cm()
		{
			return 5[285] ?? 6(285, 3963, 14);
		}

		public static string cN()
		{
			return 5[286] ?? 6(286, 3977, 9);
		}

		public static string cn()
		{
			return 5[287] ?? 6(287, 3986, 65);
		}

		public static string cO()
		{
			return 5[288] ?? 6(288, 4051, 16);
		}

		public static string co()
		{
			return 5[289] ?? 6(289, 4067, 2);
		}

		public static string cP()
		{
			return 5[290] ?? 6(290, 4069, 18);
		}

		public static string cp()
		{
			return 5[291] ?? 6(291, 4087, 9);
		}

		public static string cQ()
		{
			return 5[292] ?? 6(292, 4096, 14);
		}

		public static string cq()
		{
			return 5[293] ?? 6(293, 4110, 4);
		}

		public static string cR()
		{
			return 5[294] ?? 6(294, 4114, 14);
		}

		public static string cr()
		{
			return 5[295] ?? 6(295, 4128, 4);
		}

		public static string cS()
		{
			return 5[296] ?? 6(296, 4132, 14);
		}

		public static string cs()
		{
			return 5[297] ?? 6(297, 4146, 5);
		}

		public static string cT()
		{
			return 5[298] ?? 6(298, 4151, 3);
		}

		public static string ct()
		{
			return 5[299] ?? 6(299, 4154, 13);
		}

		public static string cU()
		{
			return 5[300] ?? 6(300, 4167, 12);
		}

		public static string cu()
		{
			return 5[301] ?? 6(301, 4179, 4);
		}

		public static string cV()
		{
			return 5[302] ?? 6(302, 4183, 1);
		}

		public static string cv()
		{
			return 5[303] ?? 6(303, 4184, 9);
		}

		public static string cW()
		{
			return 5[304] ?? 6(304, 4193, 5);
		}

		public static string cw()
		{
			return 5[305] ?? 6(305, 4198, 13);
		}

		public static string cX()
		{
			return 5[306] ?? 6(306, 4211, 9);
		}

		public static string cx()
		{
			return 5[307] ?? 6(307, 4220, 3);
		}

		public static string cY()
		{
			return 5[308] ?? 6(308, 4223, 25);
		}

		public static string cy()
		{
			return 5[309] ?? 6(309, 4248, 25);
		}

		public static string cZ()
		{
			return 5[310] ?? 6(310, 4273, 23);
		}

		public static string cz()
		{
			return 5[311] ?? 6(311, 4296, 23);
		}

		public static string DA()
		{
			return 5[312] ?? 6(312, 4319, 22);
		}

		public static string Da()
		{
			return 5[313] ?? 6(313, 4341, 9);
		}

		public static string DB()
		{
			return 5[314] ?? 6(314, 4350, 34);
		}

		public static string Db()
		{
			return 5[315] ?? 6(315, 4384, 3);
		}

		public static string DC()
		{
			return 5[316] ?? 6(316, 4387, 17);
		}

		public static string Dc()
		{
			return 5[317] ?? 6(317, 4404, 2);
		}

		public static string DD()
		{
			return 5[318] ?? 6(318, 4406, 15);
		}

		public static string Dd()
		{
			return 5[319] ?? 6(319, 4421, 15);
		}

		public static string DE()
		{
			return 5[320] ?? 6(320, 4436, 16);
		}

		public static string De()
		{
			return 5[321] ?? 6(321, 4452, 18);
		}

		public static string DF()
		{
			return 5[322] ?? 6(322, 4470, 3);
		}

		public static string Df()
		{
			return 5[323] ?? 6(323, 4473, 17);
		}

		public static string DG()
		{
			return 5[324] ?? 6(324, 4490, 48);
		}

		public static string Dg()
		{
			return 5[325] ?? 6(325, 4538, 66);
		}

		public static string DH()
		{
			return 5[326] ?? 6(326, 4604, 41);
		}

		public static string Dh()
		{
			return 5[327] ?? 6(327, 4645, 95);
		}

		public static string DI()
		{
			return 5[328] ?? 6(328, 4740, 35);
		}

		public static string Di()
		{
			return 5[329] ?? 6(329, 4775, 94);
		}

		public static string DJ()
		{
			return 5[330] ?? 6(330, 4869, 25);
		}

		public static string Dj()
		{
			return 5[331] ?? 6(331, 4894, 21);
		}

		public static string DK()
		{
			return 5[332] ?? 6(332, 4915, 108);
		}

		public static string Dk()
		{
			return 5[333] ?? 6(333, 5023, 28);
		}

		public static string DL()
		{
			return 5[334] ?? 6(334, 5051, 85);
		}

		public static string Dl()
		{
			return 5[335] ?? 6(335, 5136, 74);
		}

		public static string DM()
		{
			return 5[336] ?? 6(336, 5210, 22);
		}

		public static string Dm()
		{
			return 5[337] ?? 6(337, 5232, 2);
		}

		public static string DN()
		{
			return 5[338] ?? 6(338, 5234, 79);
		}

		public static string Dn()
		{
			return 5[339] ?? 6(339, 5313, 29);
		}

		public static string DO()
		{
			return 5[340] ?? 6(340, 5342, 11);
		}

		public static string Do()
		{
			return 5[341] ?? 6(341, 5353, 51);
		}

		public static string DP()
		{
			return 5[342] ?? 6(342, 5404, 32);
		}

		public static string Dp()
		{
			return 5[343] ?? 6(343, 5436, 11);
		}

		public static string DQ()
		{
			return 5[344] ?? 6(344, 5447, 48);
		}

		public static string Dq()
		{
			return 5[345] ?? 6(345, 5495, 64);
		}

		public static string DR()
		{
			return 5[346] ?? 6(346, 5559, 15);
		}

		public static string Dr()
		{
			return 5[347] ?? 6(347, 5574, 3);
		}

		public static string DS()
		{
			return 5[348] ?? 6(348, 5577, 17);
		}

		public static string Ds()
		{
			return 5[349] ?? 6(349, 5594, 13);
		}

		public static string DT()
		{
			return 5[350] ?? 6(350, 5607, 17);
		}

		public static string Dt()
		{
			return 5[351] ?? 6(351, 5624, 5);
		}

		public static string DU()
		{
			return 5[352] ?? 6(352, 5629, 6);
		}

		public static string Du()
		{
			return 5[353] ?? 6(353, 5635, 13);
		}

		public static string DV()
		{
			return 5[354] ?? 6(354, 5648, 11);
		}

		public static string Dv()
		{
			return 5[355] ?? 6(355, 5659, 11);
		}

		public static string DW()
		{
			return 5[356] ?? 6(356, 5670, 33);
		}

		public static string Dw()
		{
			return 5[357] ?? 6(357, 5703, 377);
		}

		public static string DX()
		{
			return 5[358] ?? 6(358, 6080, 15);
		}

		public static string Dx()
		{
			return 5[359] ?? 6(359, 6095, 12);
		}

		public static string DY()
		{
			return 5[360] ?? 6(360, 6107, 23);
		}

		public static string Dy()
		{
			return 5[361] ?? 6(361, 6130, 33);
		}

		public static string DZ()
		{
			return 5[362] ?? 6(362, 6163, 20);
		}

		public static string Dz()
		{
			return 5[363] ?? 6(363, 6183, 37);
		}

		public static string dA()
		{
			return 5[364] ?? 6(364, 6220, 36);
		}

		public static string da()
		{
			return 5[365] ?? 6(365, 6256, 21);
		}

		public static string dB()
		{
			return 5[366] ?? 6(366, 6277, 17);
		}

		public static string db()
		{
			return 5[367] ?? 6(367, 6294, 7);
		}

		public static string dC()
		{
			return 5[368] ?? 6(368, 6301, 6);
		}

		public static string dc()
		{
			return 5[369] ?? 6(369, 6307, 6);
		}

		public static string dD()
		{
			return 5[370] ?? 6(370, 6313, 8);
		}

		public static string dd()
		{
			return 5[371] ?? 6(371, 6321, 42);
		}

		public static string dE()
		{
			return 5[372] ?? 6(372, 6363, 11);
		}

		public static string de()
		{
			return 5[373] ?? 6(373, 6374, 31);
		}

		public static string dF()
		{
			return 5[374] ?? 6(374, 6405, 3);
		}

		public static string df()
		{
			return 5[375] ?? 6(375, 6408, 2);
		}

		public static string dG()
		{
			return 5[376] ?? 6(376, 6410, 10);
		}

		public static string dg()
		{
			return 5[377] ?? 6(377, 6420, 7);
		}

		public static string dH()
		{
			return 5[378] ?? 6(378, 6427, 16);
		}

		public static string dh()
		{
			return 5[379] ?? 6(379, 6443, 15);
		}

		public static string dI()
		{
			return 5[380] ?? 6(380, 6458, 5);
		}

		public static string di()
		{
			return 5[381] ?? 6(381, 6463, 3);
		}

		public static string dJ()
		{
			return 5[382] ?? 6(382, 6466, 47);
		}

		public static string dj()
		{
			return 5[383] ?? 6(383, 6513, 32);
		}

		public static string dK()
		{
			return 5[384] ?? 6(384, 6545, 1);
		}

		public static string dk()
		{
			return 5[385] ?? 6(385, 6546, 10);
		}

		public static string dL()
		{
			return 5[386] ?? 6(386, 6556, 10);
		}

		public static string dl()
		{
			return 5[387] ?? 6(387, 6566, 15);
		}

		public static string dM()
		{
			return 5[388] ?? 6(388, 6581, 16);
		}

		public static string dm()
		{
			return 5[389] ?? 6(389, 6597, 6);
		}

		public static string dN()
		{
			return 5[390] ?? 6(390, 6603, 8);
		}

		public static string dn()
		{
			return 5[391] ?? 6(391, 6611, 148);
		}

		public static string dO()
		{
			return 5[392] ?? 6(392, 6759, 2);
		}

		public static string @do()
		{
			return 5[393] ?? 6(393, 6761, 24);
		}

		public static string dP()
		{
			return 5[394] ?? 6(394, 6785, 32);
		}

		public static string dp()
		{
			return 5[395] ?? 6(395, 6817, 37);
		}

		public static string dQ()
		{
			return 5[396] ?? 6(396, 6854, 76);
		}

		public static string dq()
		{
			return 5[397] ?? 6(397, 6930, 33);
		}

		public static string dR()
		{
			return 5[398] ?? 6(398, 6963, 33);
		}

		public static string dr()
		{
			return 5[399] ?? 6(399, 6996, 10);
		}

		public static string dS()
		{
			return 5[400] ?? 6(400, 7006, 19);
		}

		public static string ds()
		{
			return 5[401] ?? 6(401, 7025, 39);
		}

		public static string dT()
		{
			return 5[402] ?? 6(402, 7064, 48);
		}

		public static string dt()
		{
			return 5[403] ?? 6(403, 7112, 30);
		}

		public static string dU()
		{
			return 5[404] ?? 6(404, 7142, 28);
		}

		public static string du()
		{
			return 5[405] ?? 6(405, 7170, 6);
		}

		public static string dV()
		{
			return 5[406] ?? 6(406, 7176, 5);
		}

		public static string dv()
		{
			return 5[407] ?? 6(407, 7181, 7);
		}

		public static string dW()
		{
			return 5[408] ?? 6(408, 7188, 4);
		}

		public static string dw()
		{
			return 5[409] ?? 6(409, 7192, 6);
		}

		public static string dX()
		{
			return 5[410] ?? 6(410, 7198, 4);
		}

		public static string dx()
		{
			return 5[411] ?? 6(411, 7202, 14);
		}

		public static string dY()
		{
			return 5[412] ?? 6(412, 7216, 15);
		}

		public static string dy()
		{
			return 5[413] ?? 6(413, 7231, 16);
		}

		public static string dZ()
		{
			return 5[414] ?? 6(414, 7247, 7);
		}

		public static string dz()
		{
			return 5[415] ?? 6(415, 7254, 5);
		}

		public static string EA()
		{
			return 5[416] ?? 6(416, 7259, 31);
		}

		public static string Ea()
		{
			return 5[417] ?? 6(417, 7290, 19);
		}

		public static string EB()
		{
			return 5[418] ?? 6(418, 7309, 4);
		}

		public static string Eb()
		{
			return 5[419] ?? 6(419, 7313, 5);
		}

		public static string EC()
		{
			return 5[420] ?? 6(420, 7318, 4);
		}

		public static string Ec()
		{
			return 5[421] ?? 6(421, 7322, 8);
		}

		public static string ED()
		{
			return 5[422] ?? 6(422, 7330, 9);
		}

		public static string Ed()
		{
			return 5[423] ?? 6(423, 7339, 9);
		}

		public static string EE()
		{
			return 5[424] ?? 6(424, 7348, 10);
		}

		public static string Ee()
		{
			return 5[425] ?? 6(425, 7358, 6);
		}

		public static string EF()
		{
			return 5[426] ?? 6(426, 7364, 10);
		}

		public static string Ef()
		{
			return 5[427] ?? 6(427, 7374, 6);
		}

		public static string EG()
		{
			return 5[428] ?? 6(428, 7380, 8);
		}

		public static string Eg()
		{
			return 5[429] ?? 6(429, 7388, 11);
		}

		public static string EH()
		{
			return 5[430] ?? 6(430, 7399, 5);
		}

		public static string Eh()
		{
			return 5[431] ?? 6(431, 7404, 5);
		}

		public static string EI()
		{
			return 5[432] ?? 6(432, 7409, 6);
		}

		public static string Ei()
		{
			return 5[433] ?? 6(433, 7415, 4);
		}

		public static string EJ()
		{
			return 5[434] ?? 6(434, 7419, 4);
		}

		public static string Ej()
		{
			return 5[435] ?? 6(435, 7423, 7);
		}

		public static string EK()
		{
			return 5[436] ?? 6(436, 7430, 25);
		}

		public static string Ek()
		{
			return 5[437] ?? 6(437, 7455, 12);
		}

		public static string EL()
		{
			return 5[438] ?? 6(438, 7467, 9);
		}

		public static string El()
		{
			return 5[439] ?? 6(439, 7476, 8);
		}

		public static string EM()
		{
			return 5[440] ?? 6(440, 7484, 6);
		}

		public static string Em()
		{
			return 5[441] ?? 6(441, 7490, 15);
		}

		public static string EN()
		{
			return 5[442] ?? 6(442, 7505, 9);
		}

		public static string En()
		{
			return 5[443] ?? 6(443, 7514, 3);
		}

		public static string EO()
		{
			return 5[444] ?? 6(444, 7517, 8);
		}

		public static string Eo()
		{
			return 5[445] ?? 6(445, 7525, 14);
		}

		public static string EP()
		{
			return 5[446] ?? 6(446, 7539, 12);
		}

		public static string Ep()
		{
			return 5[447] ?? 6(447, 7551, 23);
		}

		public static string EQ()
		{
			return 5[448] ?? 6(448, 7574, 6);
		}

		public static string Eq()
		{
			return 5[449] ?? 6(449, 7580, 4);
		}

		public static string ER()
		{
			return 5[450] ?? 6(450, 7584, 10);
		}

		public static string Er()
		{
			return 5[451] ?? 6(451, 7594, 14);
		}

		public static string ES()
		{
			return 5[452] ?? 6(452, 7608, 13);
		}

		public static string Es()
		{
			return 5[453] ?? 6(453, 7621, 3);
		}

		public static string ET()
		{
			return 5[454] ?? 6(454, 7624, 6);
		}

		public static string Et()
		{
			return 5[455] ?? 6(455, 7630, 26);
		}

		public static string EU()
		{
			return 5[456] ?? 6(456, 7656, 9);
		}

		public static string Eu()
		{
			return 5[457] ?? 6(457, 7665, 21);
		}

		public static string EV()
		{
			return 5[458] ?? 6(458, 7686, 22);
		}

		public static string Ev()
		{
			return 5[459] ?? 6(459, 7708, 38);
		}

		public static string EW()
		{
			return 5[460] ?? 6(460, 7746, 46);
		}

		public static string Ew()
		{
			return 5[461] ?? 6(461, 7792, 9);
		}

		public static string EX()
		{
			return 5[462] ?? 6(462, 7801, 9);
		}

		public static string Ex()
		{
			return 5[463] ?? 6(463, 7810, 19);
		}

		public static string EY()
		{
			return 5[464] ?? 6(464, 7829, 15);
		}

		public static string Ey()
		{
			return 5[465] ?? 6(465, 7844, 1);
		}

		public static string EZ()
		{
			return 5[466] ?? 6(466, 7845, 22);
		}

		public static string Ez()
		{
			return 5[467] ?? 6(467, 7867, 4);
		}

		public static string eA()
		{
			return 5[468] ?? 6(468, 7871, 18);
		}

		public static string ea()
		{
			return 5[469] ?? 6(469, 7889, 21);
		}

		public static string eB()
		{
			return 5[470] ?? 6(470, 7910, 4);
		}

		public static string eb()
		{
			return 5[471] ?? 6(471, 7914, 11);
		}

		public static string eC()
		{
			return 5[472] ?? 6(472, 7925, 23);
		}

		public static string ec()
		{
			return 5[473] ?? 6(473, 7948, 15);
		}

		public static string eD()
		{
			return 5[474] ?? 6(474, 7963, 20);
		}

		public static string ed()
		{
			return 5[475] ?? 6(475, 7983, 33);
		}

		public static string eE()
		{
			return 5[476] ?? 6(476, 8016, 29);
		}

		public static string ee()
		{
			return 5[477] ?? 6(477, 8045, 2);
		}

		public static string eF()
		{
			return 5[478] ?? 6(478, 8047, 1);
		}

		public static string ef()
		{
			return 5[479] ?? 6(479, 8048, 20);
		}

		public static string eG()
		{
			return 5[480] ?? 6(480, 8068, 13);
		}

		public static string eg()
		{
			return 5[481] ?? 6(481, 8081, 20);
		}

		public static string eH()
		{
			return 5[482] ?? 6(482, 8101, 20);
		}

		public static string eh()
		{
			return 5[483] ?? 6(483, 8121, 23);
		}

		public static string eI()
		{
			return 5[484] ?? 6(484, 8144, 7);
		}

		public static string ei()
		{
			return 5[485] ?? 6(485, 8151, 7);
		}

		public static string eJ()
		{
			return 5[486] ?? 6(486, 8158, 16);
		}

		public static string ej()
		{
			return 5[487] ?? 6(487, 8174, 17);
		}

		public static string eK()
		{
			return 5[488] ?? 6(488, 8191, 17);
		}

		public static string ek()
		{
			return 5[489] ?? 6(489, 8208, 23);
		}

		public static string eL()
		{
			return 5[490] ?? 6(490, 8231, 26);
		}

		public static string el()
		{
			return 5[491] ?? 6(491, 8257, 35);
		}

		public static string eM()
		{
			return 5[492] ?? 6(492, 8292, 22);
		}

		public static string em()
		{
			return 5[493] ?? 6(493, 8314, 17);
		}

		public static string eN()
		{
			return 5[494] ?? 6(494, 8331, 108);
		}

		public static string en()
		{
			return 5[495] ?? 6(495, 8439, 6);
		}

		public static string eO()
		{
			return 5[496] ?? 6(496, 8445, 21);
		}

		public static string eo()
		{
			return 5[497] ?? 6(497, 8466, 6);
		}

		public static string eP()
		{
			return 5[498] ?? 6(498, 8472, 7);
		}

		public static string ep()
		{
			return 5[499] ?? 6(499, 8479, 6);
		}

		public static string eQ()
		{
			return 5[500] ?? 6(500, 8485, 20);
		}

		public static string eq()
		{
			return 5[501] ?? 6(501, 8505, 13);
		}

		public static string eR()
		{
			return 5[502] ?? 6(502, 8518, 17);
		}

		public static string er()
		{
			return 5[503] ?? 6(503, 8535, 5);
		}

		public static string eS()
		{
			return 5[504] ?? 6(504, 8540, 41);
		}

		public static string es()
		{
			return 5[505] ?? 6(505, 8581, 15);
		}

		public static string eT()
		{
			return 5[506] ?? 6(506, 8596, 34);
		}

		public static string et()
		{
			return 5[507] ?? 6(507, 8630, 13);
		}

		public static string eU()
		{
			return 5[508] ?? 6(508, 8643, 17);
		}

		public static string eu()
		{
			return 5[509] ?? 6(509, 8660, 18);
		}

		public static string eV()
		{
			return 5[510] ?? 6(510, 8678, 6);
		}

		public static string ev()
		{
			return 5[511] ?? 6(511, 8684, 17);
		}

		public static string eW()
		{
			return 5[512] ?? 6(512, 8701, 3);
		}

		public static string ew()
		{
			return 5[513] ?? 6(513, 8704, 12);
		}

		public static string eX()
		{
			return 5[514] ?? 6(514, 8716, 5);
		}

		public static string ex()
		{
			return 5[515] ?? 6(515, 8721, 7);
		}

		public static string eY()
		{
			return 5[516] ?? 6(516, 8728, 4);
		}

		public static string ey()
		{
			return 5[517] ?? 6(517, 8732, 5);
		}

		public static string eZ()
		{
			return 5[518] ?? 6(518, 8737, 10);
		}

		public static string ez()
		{
			return 5[519] ?? 6(519, 8747, 11);
		}

		public static string FA()
		{
			return 5[520] ?? 6(520, 8758, 10);
		}

		public static string Fa()
		{
			return 5[521] ?? 6(521, 8768, 11);
		}

		public static string FB()
		{
			return 5[522] ?? 6(522, 8779, 10);
		}

		public static string Fb()
		{
			return 5[523] ?? 6(523, 8789, 6);
		}

		public static string FC()
		{
			return 5[524] ?? 6(524, 8795, 7);
		}

		public static string Fc()
		{
			return 5[525] ?? 6(525, 8802, 12);
		}

		public static string FD()
		{
			return 5[526] ?? 6(526, 8814, 22);
		}

		public static string Fd()
		{
			return 5[527] ?? 6(527, 8836, 1);
		}

		public static string FE()
		{
			return 5[528] ?? 6(528, 8837, 2);
		}

		public static string Fe()
		{
			return 5[529] ?? 6(529, 8839, 10);
		}

		public static string FF()
		{
			return 5[530] ?? 6(530, 8849, 12);
		}

		public static string Ff()
		{
			return 5[531] ?? 6(531, 8861, 13);
		}

		public static string FG()
		{
			return 5[532] ?? 6(532, 8874, 4);
		}

		public static string Fg()
		{
			return 5[533] ?? 6(533, 8878, 6);
		}

		public static string FH()
		{
			return 5[534] ?? 6(534, 8884, 5);
		}

		public static string Fh()
		{
			return 5[535] ?? 6(535, 8889, 21);
		}

		public static string FI()
		{
			return 5[536] ?? 6(536, 8910, 35);
		}

		public static string Fi()
		{
			return 5[537] ?? 6(537, 8945, 32);
		}

		public static string FJ()
		{
			return 5[538] ?? 6(538, 8977, 22);
		}

		public static string Fj()
		{
			return 5[539] ?? 6(539, 8999, 28);
		}

		public static string FK()
		{
			return 5[540] ?? 6(540, 9027, 4);
		}

		public static string Fk()
		{
			return 5[541] ?? 6(541, 9031, 86);
		}

		public static string FL()
		{
			return 5[542] ?? 6(542, 9117, 8);
		}

		public static string Fl()
		{
			return 5[543] ?? 6(543, 9125, 6);
		}

		public static string FM()
		{
			return 5[544] ?? 6(544, 9131, 9);
		}

		public static string Fm()
		{
			return 5[545] ?? 6(545, 9140, 34);
		}

		public static string FN()
		{
			return 5[546] ?? 6(546, 9174, 44);
		}

		public static string Fn()
		{
			return 5[547] ?? 6(547, 9218, 39);
		}

		public static string FO()
		{
			return 5[548] ?? 6(548, 9257, 31);
		}

		public static string Fo()
		{
			return 5[549] ?? 6(549, 9288, 52);
		}

		public static string FP()
		{
			return 5[550] ?? 6(550, 9340, 15);
		}

		public static string Fp()
		{
			return 5[551] ?? 6(551, 9355, 28);
		}

		public static string FQ()
		{
			return 5[552] ?? 6(552, 9383, 18);
		}

		public static string Fq()
		{
			return 5[553] ?? 6(553, 9401, 28);
		}

		public static string FR()
		{
			return 5[554] ?? 6(554, 9429, 4);
		}

		public static string Fr()
		{
			return 5[555] ?? 6(555, 9433, 29);
		}

		public static string FS()
		{
			return 5[556] ?? 6(556, 9462, 3);
		}

		public static string Fs()
		{
			return 5[557] ?? 6(557, 9465, 42);
		}

		public static string FT()
		{
			return 5[558] ?? 6(558, 9507, 7);
		}

		public static string Ft()
		{
			return 5[559] ?? 6(559, 9514, 11);
		}

		public static string FU()
		{
			return 5[560] ?? 6(560, 9525, 11);
		}

		public static string Fu()
		{
			return 5[561] ?? 6(561, 9536, 6);
		}

		public static string FV()
		{
			return 5[562] ?? 6(562, 9542, 7);
		}

		public static string Fv()
		{
			return 5[563] ?? 6(563, 9549, 7);
		}

		public static string FW()
		{
			return 5[564] ?? 6(564, 9556, 7);
		}

		public static string Fw()
		{
			return 5[565] ?? 6(565, 9563, 4);
		}

		public static string FX()
		{
			return 5[566] ?? 6(566, 9567, 8);
		}

		public static string Fx()
		{
			return 5[567] ?? 6(567, 9575, 8);
		}

		public static string FY()
		{
			return 5[568] ?? 6(568, 9583, 6);
		}

		static 8EC410F1-6E77-4690-9E9E-B3CEE3479BCC()
		{
			for (int i = 0; i < 4.Length; i++)
			{
				4[i] = (byte)((uint)(4[i] ^ i) ^ 0xAAu);
			}
		}
	}
}
