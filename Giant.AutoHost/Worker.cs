using System.Net.Http;

namespace Giant.AutoHost
{
    public class Worker : BackgroundService
    {
        private ILogger<Worker> Logger { get; set; }
        public IHttpClientFactory HttpFactory { get; }
        public IConfiguration Config { get; }
        private readonly string _hostPath;
        public Worker(IHttpClientFactory httpFactory, IConfiguration config, ILogger<Worker> logger)
        {
            HttpFactory = httpFactory;
            Config = config;
            Logger = logger;
            _hostPath = Path.Combine(Environment.SystemDirectory, "drivers\\etc\\hosts");
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                //���������ļ�
                var listConfig = this.Config.GetSection(nameof(HostConfig)).Get<HostConfig[]>();
                //�������ö�Ӧ��Host�ļ�
                var listTask = new List<Task>();
                foreach (var config in listConfig)
                {
                    if (config.Enable)
                        listTask.Add(LoadHostAsync(config, stoppingToken));
                }
                await Task.WhenAll(listTask);
                //д��Host�ļ�
                await WriteHostAsync(listConfig, stoppingToken);
                //���DNS����
                DnsApiHelper.FlushMyCache();
                await Task.Delay(60 * 60 * 1000, stoppingToken);
            }
        }
        private async Task LoadHostAsync(HostConfig config, CancellationToken stoppingToken)
        {
            if (config.Type == HostType.RemoteUrl)
            {
                var isLoaded = false;
                foreach (var uri in config.Uris)
                {
                    if (isLoaded) break;
                    try
                    {
                        var client = this.HttpFactory.CreateClient();
                        var tokenSoruce = new CancellationTokenSource(30 * 1000);
                        var response = await client.GetAsync(uri, tokenSoruce.Token);
                        if (response.IsSuccessStatusCode)
                        {
                            using (var stream = await response.Content.ReadAsStreamAsync(tokenSoruce.Token))
                            using (var reader = new StreamReader(stream))
                            {
                                while (await reader.ReadLineAsync() is string line)
                                {
                                    if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#')) continue;
                                    var keys = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                    if (keys.Length >= 2)
                                        config.SetHost(keys[1], keys[0]);
                                }
                            }
                            isLoaded = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex.Message, ex);
                    }
                }
            }
        }
        private async Task WriteHostAsync(HostConfig[] listConfig, CancellationToken stoppingToken)
        {
            File.SetAttributes(_hostPath, FileAttributes.Normal);//ȥ��ֻ��
            var lines = await File.ReadAllLinesAsync(_hostPath, stoppingToken);
            var listLines = lines.ToList();
            foreach (var config in listConfig)
            {
                if (!config.Enable) continue;
                var dicHost = config.GetHost();
                if (dicHost.Count == 0) continue;

                var startLine = $"#AutoHost {config.Name} Start";
                var endLine = $"#AutoHost {config.Name} End";
                var startIndex = listLines.IndexOf(startLine);
                var endIndex = listLines.IndexOf(endLine);
                if (startIndex != -1 && endIndex != -1)
                    listLines.RemoveRange(startIndex, endIndex - startIndex + 1);

                listLines.Add(startLine);
                listLines.Add($"# Update At {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
                var hostLines = dicHost.Select(s => $"{s.Value}\t\t{s.Key}").ToList();
                listLines.AddRange(hostLines);
                listLines.Add(endLine);
            }
            await File.WriteAllLinesAsync(_hostPath, listLines, stoppingToken);
            File.SetAttributes(_hostPath, FileAttributes.ReadOnly);//дֻ��
        }
    }
}