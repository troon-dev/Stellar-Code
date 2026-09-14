using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using _003CPrivateImplementationDetails_003E_007B06AF9570_002DDB29_002D425E_002DBDE9_002DAA3CBE8EC79D_007D;
using Stellar.Launcher.Downloader;
using Stellar.Launcher.Ipc;
using Stellar.Launcher.Native;

namespace Stellar.Launcher.Commands;

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
			if (text.Split('\\', '/').Any((string part) => part == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cH()))
			{
				throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eq());
			}
			string normalized = Path.GetFullPath(text);
			if (!normalized.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
			{
				throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eq());
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
				bridge.Emit(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eR(), new
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
			string partPath = normalized + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.er();
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
						throw new IOException(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eS());
					}
					if (!response.IsSuccessStatusCode)
					{
						if (Net.IsChallengeResponse(response))
						{
							throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eN());
						}
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 3);
						defaultInterpolatedStringHandler.AppendFormatted(file.Url);
						defaultInterpolatedStringHandler.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.es());
						defaultInterpolatedStringHandler.AppendFormatted((int)statusCode);
						defaultInterpolatedStringHandler.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cD());
						defaultInterpolatedStringHandler.AppendFormatted(statusCode);
						throw new IOException(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					if (Net.IsChallengeResponse(response))
					{
						throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eN());
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
									throw new IOException(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eT());
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
									bridge.Emit(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eR(), new
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
							defaultInterpolatedStringHandler2.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.et());
							defaultInterpolatedStringHandler2.AppendFormatted(downloaded);
							defaultInterpolatedStringHandler2.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eU());
							defaultInterpolatedStringHandler2.AppendFormatted(num.Value);
							throw new CommandFailure(defaultInterpolatedStringHandler2.ToStringAndClear());
						}
					}
					else if (attempt >= 6)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(28, 3);
						defaultInterpolatedStringHandler3.AppendFormatted(file.Name);
						defaultInterpolatedStringHandler3.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eu());
						defaultInterpolatedStringHandler3.AppendFormatted(downloaded);
						defaultInterpolatedStringHandler3.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Ez());
						defaultInterpolatedStringHandler3.AppendFormatted(num.Value);
						defaultInterpolatedStringHandler3.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eV());
						throw new CommandFailure(defaultInterpolatedStringHandler3.ToStringAndClear());
					}
				}
				else
				{
					if (failure == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eN())
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
			bridge.Emit(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eR(), new
			{
				build = baseDir,
				fileIndex = index + 1,
				totalFiles = totalFiles,
				downloaded = completedBytes,
				total = overallTotal,
				skipped = false
			});
		}
		bridge.Emit(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ev(), new
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
		if (!response.Content.Headers.TryGetValues(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ct(), out IEnumerable<string> enumerable))
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
			return _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DG() + exe + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Dg();
		case 1260:
		case 4556:
			return _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DH() + exe + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Dh();
		case 5:
			return _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DI() + exe + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Di();
		default:
			return _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DJ() + error.Message;
		}
	}

	private static List<uint> EpicLauncherPids()
	{
		return ProcessLauncher.FindByName(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Dj());
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
					throw new CommandFailure(text + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DK());
				}
				catch (DirectoryNotFoundException)
				{
					throw new CommandFailure(text + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DK());
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
					string text2 = ex?.Message ?? _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Dk();
					string text3 = ((ex is UnauthorizedAccessException) ? _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Dl() : _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DL());
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(24, 3);
					defaultInterpolatedStringHandler.AppendFormatted(text);
					defaultInterpolatedStringHandler.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DM());
					defaultInterpolatedStringHandler.AppendFormatted(text2);
					defaultInterpolatedStringHandler.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Dm());
					defaultInterpolatedStringHandler.AppendFormatted(text3);
					throw new CommandFailure(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				Thread.Sleep(100);
			}
		}
	}

	public static bool Launch(string path, string code, string? identity, List<string> extraArgs)
	{
		string text = Path.Combine(path, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DN());
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
						defaultInterpolatedStringHandler.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Dn());
						defaultInterpolatedStringHandler.AppendFormatted(num2);
						defaultInterpolatedStringHandler.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DO());
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
		string text2 = Path.Combine(path, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Do());
		if (!Directory.Exists(text2))
		{
			try
			{
				Directory.CreateDirectory(text2);
			}
			catch (Exception ex2)
			{
				throw new CommandFailure(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DP() + ex2.Message);
			}
		}
		string text3 = Path.Combine(path, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Dp());
		string text4 = Path.Combine(path, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DQ());
		string text5 = Path.Combine(path, _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Dq());
		string text6 = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DR() + code;
		string text7 = ((identity == null) ? null : (_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Dr() + identity));
		List<string> list = new List<string>
		{
			_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DS(),
			_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Ds(),
			_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DT(),
			_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Dt(),
			_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DU(),
			_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Du(),
			_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DV(),
			_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Dv(),
			_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DW(),
			_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Dw(),
			_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DX(),
			_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Dx(),
			text6,
			_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DY(),
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
		foreach (uint item in ProcessLauncher.FindByName(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Dy(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.DZ(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Dz(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dA(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.da(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.dB(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.db(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.Dj()))
		{
			ProcessLauncher.KillPid(item);
		}
		Thread.Sleep(1000);
	}
}
