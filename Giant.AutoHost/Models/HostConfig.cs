using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giant.AutoHost;

public class HostConfig
{
    /// <summary>
    /// host字典
    /// </summary>
    private Dictionary<string, string> _dicHost = new Dictionary<string, string>();
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Host加载类型
    /// </summary>
    public HostType Type { get; set; }
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enable { get; set; }
    /// <summary>
    /// Uri地址
    /// </summary>
    public string[] Uris { get; set; }
    public void SetHost(string key, string value)
    {
        if (_dicHost.ContainsKey(key))
            _dicHost[key] = value;
        else
            _dicHost.Add(key, value);
    }
    public Dictionary<string, string> GetHost() { return _dicHost; }
}
/// <summary>
/// Host类型
/// </summary>
public enum HostType
{
    /// <summary>
    /// 远程URL加载
    /// </summary>
    RemoteUrl,
    /// <summary>
    /// 本地文件加载
    /// </summary>
    LocalFile
}
