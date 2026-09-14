using System;
using System.Collections.Generic;
using System.Drawing;
using _003CPrivateImplementationDetails_003E_007B06AF9570_002DDB29_002D425E_002DBDE9_002DAA3CBE8EC79D_007D;
using Photino.NET;

namespace Stellar.Launcher.Native;

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
		if (label == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.k())
		{
			Main = managedWindow;
		}
		else if (label == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ax())
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
