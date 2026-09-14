using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using _003CPrivateImplementationDetails_003E_007B06AF9570_002DDB29_002D425E_002DBDE9_002DAA3CBE8EC79D_007D;

namespace Stellar.Launcher.Ipc;

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
	private sealed class _003CCandidates_003Ed__17 : IEnumerable<int>, IEnumerable, IEnumerator<int>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private int _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private int _003Coffset_003E5__2;

		int IEnumerator<int>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCandidates_003Ed__17(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E2__current = 17435;
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				_003Coffset_003E5__2 = 1;
				goto IL_007c;
			case 2:
				_003C_003E1__state = -1;
				_003Coffset_003E5__2++;
				goto IL_007c;
			case 3:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_007c:
				if (_003Coffset_003E5__2 <= 32)
				{
					_003C_003E2__current = 17435 + _003Coffset_003E5__2;
					_003C_003E1__state = 2;
					return true;
				}
				_003C_003E2__current = 0;
				_003C_003E1__state = 3;
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
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				return this;
			}
			return new _003CCandidates_003Ed__17(0);
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
		[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bw()] = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bX(),
		[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bx()] = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bY(),
		[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.by()] = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bY(),
		[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bZ()] = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bz(),
		[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.CA()] = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Ca(),
		[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.CB()] = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Cb(),
		[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.CC()] = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Cc(),
		[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.CD()] = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Cc(),
		[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Cd()] = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.CE(),
		[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Ce()] = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.CF(),
		[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Cf()] = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.CG(),
		[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Cg()] = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.CH(),
		[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Ch()] = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.CI(),
		[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Ci()] = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.CJ(),
		[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Cj()] = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.CK(),
		[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Ck()] = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.CL(),
		[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Cl()] = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.CM(),
		[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Cm()] = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.CN(),
		[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Cn()] = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.CO(),
		[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Co()] = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.CP(),
		[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Cp()] = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.CQ(),
		[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Cq()] = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.CR(),
		[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Cr()] = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Ca(),
		[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.CS()] = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Cs()
	};

	private readonly string root;

	private TcpListener? listener;

	public int Port { get; private set; }

	public string Origin
	{
		get
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
			defaultInterpolatedStringHandler.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bj());
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
			return _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bK();
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
			throw new IOException(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bk());
		}
		Thread thread = new Thread(Accept);
		thread.IsBackground = true;
		thread.Name = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bL();
		thread.Start();
	}

	[IteratorStateMachine(typeof(_003CCandidates_003Ed__17))]
	private static IEnumerable<int> Candidates()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCandidates_003Ed__17(-2);
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
				if (text2.Equals(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ER(), StringComparison.OrdinalIgnoreCase))
				{
					closing = text3.Contains(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bs(), StringComparison.OrdinalIgnoreCase);
				}
				else if (text2.Equals(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Er(), StringComparison.OrdinalIgnoreCase))
				{
					int.TryParse(text3, out length);
				}
				else if (text2.Equals(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ES(), StringComparison.OrdinalIgnoreCase))
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
		if (array[2].EndsWith(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Es(), StringComparison.Ordinal))
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
			if (text4 == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Eq())
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
		byte[] bytes = Encoding.ASCII.GetBytes(Head(404, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.CR(), body.Length, null, closing));
		await stream.WriteAsync(bytes).ConfigureAwait(false);
		if (method != _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Eq())
		{
			await stream.WriteAsync(body).ConfigureAwait(false);
		}
		await stream.FlushAsync().ConfigureAwait(false);
	}

	private static string Head(int status, string contentType, long contentLength, Resource? resource, bool closing)
	{
		string text = status switch
		{
			200 => _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bl(), 
			304 => _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bM(), 
			_ => _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bm(), 
		};
		StringBuilder stringBuilder = new StringBuilder(256);
		stringBuilder.Append(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bN()).Append(status).Append(' ')
			.Append(text)
			.Append(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aZ());
		stringBuilder.Append(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bn()).Append(contentType).Append(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aZ());
		if (status != 304)
		{
			stringBuilder.Append(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bO()).Append(contentLength).Append(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aZ());
		}
		if (resource != null)
		{
			stringBuilder.Append(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bo()).Append(resource.Tag).Append(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aZ());
			stringBuilder.Append(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bP()).Append(resource.Immutable ? _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bQ() : _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bp()).Append(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aZ());
		}
		else
		{
			stringBuilder.Append(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bq());
		}
		stringBuilder.Append(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bR());
		stringBuilder.Append(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.br()).Append(closing ? _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bs() : _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bS()).Append(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aZ());
		if (!closing)
		{
			stringBuilder.Append(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bT());
		}
		stringBuilder.Append(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.aZ());
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
		if (text.StartsWith(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bt(), StringComparison.Ordinal))
		{
			string text3 = HttpUtility.ParseQueryString(text2)[_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bU()];
			if (!string.IsNullOrEmpty(text3))
			{
				return Describe(text3, immutable: false);
			}
			return null;
		}
		bool immutable = text.StartsWith(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bu(), StringComparison.OrdinalIgnoreCase);
		string fullPath = Path.GetFullPath((text.Length == 0) ? Path.Combine(root, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bV()) : Path.Combine(root, text.Replace('/', Path.DirectorySeparatorChar)));
		if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}
		Resource resource = Describe(fullPath, immutable);
		if (resource != null)
		{
			return resource;
		}
		return Describe(Path.Combine(root, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bV()), immutable: false);
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
		defaultInterpolatedStringHandler.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BD());
		defaultInterpolatedStringHandler.AppendFormatted(fileInfo.Length, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bv());
		defaultInterpolatedStringHandler.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bW());
		defaultInterpolatedStringHandler.AppendFormatted(fileInfo.LastWriteTimeUtc.Ticks, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.bv());
		defaultInterpolatedStringHandler.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.BD());
		obj2.Tag = defaultInterpolatedStringHandler.ToStringAndClear();
		obj2.Immutable = immutable;
		return obj2;
	}
}
