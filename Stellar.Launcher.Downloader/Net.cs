using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using _003CPrivateImplementationDetails_003E_007B06AF9570_002DDB29_002D425E_002DBDE9_002DAA3CBE8EC79D_007D;

namespace Stellar.Launcher.Downloader;

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
		_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cY(),
		_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cy(),
		_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cZ(),
		_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cz()
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
		defaultRequestHeaders.UserAgent.ParseAdd(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ck());
		defaultRequestHeaders.TryAddWithoutValidation(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cL(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cl());
		defaultRequestHeaders.TryAddWithoutValidation(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cM(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cm());
		defaultRequestHeaders.TryAddWithoutValidation(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cN(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cn());
		defaultRequestHeaders.TryAddWithoutValidation(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cO(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.co());
		defaultRequestHeaders.TryAddWithoutValidation(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cP(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cp());
		defaultRequestHeaders.TryAddWithoutValidation(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cQ(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cq());
		defaultRequestHeaders.TryAddWithoutValidation(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cR(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cr());
		defaultRequestHeaders.TryAddWithoutValidation(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cS(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cs());
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
				using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, text + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eo() + Uri.EscapeDataString(host) + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eP());
				request.Headers.TryAddWithoutValidation(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.ep(), _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eQ());
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
		string[] array = url.Split(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cT(), 2);
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
		if (first.StartsWith(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eD(), StringComparison.Ordinal) || first == _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eN())
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
			throw new IOException(first + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.en() + ex2.Message + _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eO());
		}
	}

	public static bool IsChallengeResponse(HttpResponseMessage response)
	{
		if (response.Headers.Contains(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cU()))
		{
			return true;
		}
		return (response.Content.Headers.ContentType?.MediaType)?.Contains(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cu(), StringComparison.OrdinalIgnoreCase) ?? false;
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
		if (!text.StartsWith(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cV(), StringComparison.Ordinal))
		{
			return false;
		}
		if (!text.StartsWith(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cv(), StringComparison.OrdinalIgnoreCase) && !text.StartsWith(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cW(), StringComparison.OrdinalIgnoreCase) && !text.Contains(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cw(), StringComparison.OrdinalIgnoreCase) && !text.Contains(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cX(), StringComparison.OrdinalIgnoreCase))
		{
			return text.Contains(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cx(), StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	public static async Task<string> FetchText(HttpClient client, string url, int attempts)
	{
		string text = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.em();
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
				text = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.el();
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
						throw new IOException(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eN());
					}
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 3);
					defaultInterpolatedStringHandler.AppendFormatted(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eD());
					defaultInterpolatedStringHandler.AppendFormatted((int)statusCode);
					defaultInterpolatedStringHandler.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cD());
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
					text = _8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eM();
					goto end_IL_019e;
				}
				catch (Exception ex4)
				{
					text = ex4.Message;
					goto end_IL_019e;
				}
				if (IsCloudflareChallenge(response, text2))
				{
					throw new IOException(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eN());
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
			throw new IOException(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.el());
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
			defaultInterpolatedStringHandler.AppendFormatted(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eD());
			defaultInterpolatedStringHandler.AppendFormatted((int)statusCode);
			defaultInterpolatedStringHandler.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cD());
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
			HttpClient httpClient2 = (await DohClient(url).ConfigureAwait(false)) ?? throw new IOException(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eL());
			try
			{
				using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(45.0));
				httpResponseMessage = await httpClient2.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationTokenSource.Token).ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
				throw new IOException(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.el());
			}
		}
		using (httpResponseMessage)
		{
			if (!httpResponseMessage.IsSuccessStatusCode)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 3);
				defaultInterpolatedStringHandler.AppendFormatted(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eD());
				defaultInterpolatedStringHandler.AppendFormatted((int)httpResponseMessage.StatusCode);
				defaultInterpolatedStringHandler.AppendLiteral(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.cD());
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
				throw new IOException(_8EC410F1_002D6E77_002D4690_002D9E9E_002DB3CEE3479BCC.eM());
			}
		}
	}
}
