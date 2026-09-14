using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using _003CPrivateImplementationDetails_003E_007B06AF9570_002DDB29_002D425E_002DBDE9_002DAA3CBE8EC79D_007D;
using Stellar.Launcher.Ipc;
using Stellar.Launcher.Native;

namespace Stellar.Launcher.Downloader;

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

	private static readonly string[] VERSIONS_URLS = new string[1] { _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cd() };

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
				throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.CY());
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
		(Current() ?? throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Cy())).Control.Pause();
	}

	public void Resume()
	{
		(Current() ?? throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Cy())).Control.Resume();
	}

	public void Cancel()
	{
		(Current() ?? throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Cy())).Control.Stop();
	}

	private static string Sanitize(string name)
	{
		string text = new string(name.Select((char value) => (!_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Ew().Contains(value)) ? value : '_').ToArray()).Trim().Trim('.');
		if (text.Length != 0)
		{
			return text;
		}
		return _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.CZ();
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
				failures.Add(url + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Dc() + ex.Message);
			}
		}
		throw new CommandFailure(failures.Count switch
		{
			0 => _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ed(), 
			1 => failures[0], 
			_ => _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eE() + string.Join(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ee(), failures) + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eF(), 
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
		if (first.StartsWith(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eD(), StringComparison.Ordinal))
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
		Version version2 = ManifestReader.ParseVersions(obj.Item2).FirstOrDefault((Version version) => version.Name == name2) ?? throw new CommandFailure(name2 + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ek());
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
				string state3 = (session2.Control.IsPaused ? _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.FY() : state2);
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
		bridge.Emit(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Cz(), snapshot);
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
			Snapshot = Snapshot.Simple(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Cx(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ef())
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
			bridge.Emit(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eG(), path);
			return path;
		}
		bridge.Emit(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EY(), failure.Message);
		throw new CommandFailure(failure.Message);
	}

	private async Task<string> InstallInner(Session session, string versionName, string destination, int connections)
	{
		Emit(session, Snapshot.Simple(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Cx(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eg()));
		Version version;
		HttpClient client;
		(client, version) = await LoadVersion(versionName).ConfigureAwait(false);
		if (!version.Chunks.EndsWith('/'))
		{
			version.Chunks += _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Ey();
		}
		Emit(session, Snapshot.Simple(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Cx(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eH()));
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
			throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DC() + root + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Dc() + ex.Message);
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
				defaultInterpolatedStringHandler.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eh());
				defaultInterpolatedStringHandler.AppendFormatted(FormatSize(valueOrDefault));
				defaultInterpolatedStringHandler.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eI());
				defaultInterpolatedStringHandler.AppendFormatted(FormatSize(num));
				defaultInterpolatedStringHandler.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ei());
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
		Emit(session, BuildSnapshot(tracker, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Cx(), 0.0, 0L, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eJ()));
		await Task.Run(delegate
		{
			Writer.Prepare(plan);
		}).ConfigureAwait(false);
		if (pending.Count == 0)
		{
			DownloadState.Clear(root);
			Emit(session, BuildSnapshot(tracker, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eB(), 0.0, 0L, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ej()));
			return root;
		}
		await RunChunks(session, tracker, root, version, client, pending, completed, connections).ConfigureAwait(false);
		DownloadState.Clear(root);
		Emit(session, BuildSnapshot(tracker, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eB(), 0.0, 0L, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eK()));
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
							throw new IOException(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EU());
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
			using CancellationTokenSource reporter = SpawnReporter(session, tracker, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eb());
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
					string text = task.Exception?.GetBaseException().Message ?? _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EU();
					if (text != _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EU() && failure == null)
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
				throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EU());
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
			Snapshot = Snapshot.Simple(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EX(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Ex())
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
			bridge.Emit(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EY(), failure.Message);
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
			version.Chunks += _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Ey();
		}
		(HttpClient, string) tuple2 = await FetchManifest(client, version.Manifest).ConfigureAwait(false);
		client = tuple2.Item1;
		if (!Directory.Exists(installPath))
		{
			throw new CommandFailure(version.Label + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EZ());
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
			using (CancellationTokenSource reporter = SpawnReporter(session2, tracker, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EX()))
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
				throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.EU());
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
					defaultInterpolatedStringHandler.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Ez());
					defaultInterpolatedStringHandler.AppendFormatted(plan.Chunks.Count);
					defaultInterpolatedStringHandler.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eA());
					text = defaultInterpolatedStringHandler.ToStringAndClear();
				}
				else
				{
					text = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ea();
				}
				string message = text;
				Emit(session2, BuildSnapshot(tracker, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eB(), 0.0, 0L, message));
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
			Emit(session2, BuildSnapshot(repairTracker, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eb(), 0.0, 0L, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eC()));
			await Task.Run(delegate
			{
				Writer.Prepare(plan);
			}).ConfigureAwait(false);
			HashSet<string> completed = new HashSet<string>(from item in plan.Chunks
				where !damaged.Contains(item.Hash)
				select item.Hash, StringComparer.Ordinal);
			await RunChunks(session2, repairTracker, installPath, version, client, pending, completed, connections).ConfigureAwait(false);
			DownloadState.Clear(installPath);
			Emit(session2, BuildSnapshot(repairTracker, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eB(), 0.0, 0L, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ec()));
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
			_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cA(),
			_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ca(),
			_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cB(),
			_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cb(),
			_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cC()
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
			defaultInterpolatedStringHandler.AppendFormatted(num, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cc());
			defaultInterpolatedStringHandler.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cD());
			defaultInterpolatedStringHandler.AppendFormatted(array[num2]);
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}
		DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(1, 2);
		defaultInterpolatedStringHandler2.AppendFormatted(bytes);
		defaultInterpolatedStringHandler2.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cD());
		defaultInterpolatedStringHandler2.AppendFormatted(array[num2]);
		return defaultInterpolatedStringHandler2.ToStringAndClear();
	}
}
