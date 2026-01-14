using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Bug报告工具
/// </summary>
public class BugReporter : MonoBehaviour
{
    [Header("报告设置")]
    [SerializeField] private bool autoReportErrors = true;
    [SerializeField] private bool includeScreenshot = true;
    [SerializeField] private bool includeSystemInfo = true;

    [Header("调试信息")]
    [SerializeField] private bool logToConsole = true;
    [SerializeField] private bool saveToFile = true;

    // Bug日志
    private List<BugReport> bugReports = new List<BugReport>();
    private StringBuilder consoleLog = new StringBuilder();

    // 单例
    private static BugReporter _instance;
    public static BugReporter Instance => _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            SetupErrorHandling();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetupErrorHandling()
    {
        if (autoReportErrors)
        {
            Application.logMessageReceived += HandleLog;
        }
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error)
        {
            BugReport report = new BugReport
            {
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                message = logString,
                stackTrace = stackTrace,
                logType = type.ToString(),
                sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            };

            bugReports.Add(report);

            if (logToConsole)
            {
                consoleLog.AppendLine($"[{report.timestamp}] {type}: {logString}");
                if (saveToFile)
                {
                    SaveLogToFile(logString, stackTrace, type);
                }
            }

            Debug.LogError($"Bug已捕获: {logString}");
        }
    }

    /// <summary>
    /// 手动报告Bug
    /// </summary>
    public void ReportBug(string title, string description, string reproductionSteps = "")
    {
        BugReport report = new BugReport
        {
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            message = title,
            description = description,
            reproductionSteps = reproductionSteps,
            logType = "Manual",
            sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        };

        bugReports.Add(report);

        Debug.Log($"Bug已报告: {title}");
        SaveBugReportToFile(report);
    }

    /// <summary>
    /// 获取所有Bug报告
    /// </summary>
    public List<BugReport> GetAllReports()
    {
        return new List<BugReport>(bugReports);
    }

    /// <summary>
    /// 清空Bug报告
    /// </summary>
    public void ClearReports()
    {
        bugReports.Clear();
        consoleLog.Clear();
        Debug.Log("Bug报告已清空");
    }

    /// <summary>
    /// 导出Bug报告
    /// </summary>
    public string ExportBugReport()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== BUG 报告 ===");
        sb.AppendLine($"生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"总Bug数: {bugReports.Count}");
        sb.AppendLine();

        foreach (var report in bugReports)
        {
            sb.AppendLine("---");
            sb.AppendLine($"时间: {report.timestamp}");
            sb.AppendLine($"类型: {report.logType}");
            sb.AppendLine($"场景: {report.sceneName}");
            sb.AppendLine($"消息: {report.message}");

            if (!string.IsNullOrEmpty(report.description))
            {
                sb.AppendLine($"描述: {report.description}");
            }

            if (!string.IsNullOrEmpty(report.reproductionSteps))
            {
                sb.AppendLine($"复现步骤: {report.reproductionSteps}");
            }

            if (!string.IsNullOrEmpty(report.stackTrace))
            {
                sb.AppendLine($"堆栈:\n{report.stackTrace}");
            }

            sb.AppendLine();
        }

        if (includeSystemInfo)
        {
            sb.AppendLine("=== 系统信息 ===");
            sb.AppendLine($"操作系统: {SystemInfo.operatingSystem}");
            sb.AppendLine($"处理器: {SystemInfo.processorType}");
            sb.AppendLine($"内存: {SystemInfo.systemMemorySize} MB");
            sb.AppendLine($"图形设备: {SystemInfo.graphicsDeviceName}");
            sb.AppendLine($"Unity版本: {Application.unityVersion}");
            sb.AppendLine($"应用版本: {Application.version}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// 保存日志到文件
    /// </summary>
    private void SaveLogToFile(string logString, string stackTrace, LogType type)
    {
        try
        {
            string logPath = GetLogFilePath();
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {type}: {logString}\n{stackTrace}\n";

            System.IO.File.AppendAllText(logPath, logEntry);
        }
        catch (Exception e)
        {
            Debug.LogError($"保存日志失败: {e.Message}");
        }
    }

    /// <summary>
    /// 保存Bug报告到文件
    /// </summary>
    private void SaveBugReportToFile(BugReport report)
    {
        try
        {
            string reportPath = GetBugReportFilePath();
            string reportText = $"[{report.timestamp}] {report.logType}: {report.message}\n";

            if (!string.IsNullOrEmpty(report.description))
            {
                reportText += $"描述: {report.description}\n";
            }

            if (!string.IsNullOrEmpty(report.reproductionSteps))
            {
                reportText += $"复现: {report.reproductionSteps}\n";
            }

            reportText += "---\n";

            System.IO.File.AppendAllText(reportPath, reportText);
        }
        catch (Exception e)
        {
            Debug.LogError($"保存Bug报告失败: {e.Message}");
        }
    }

    private string GetLogFilePath()
    {
        string directory = System.IO.Path.Combine(Application.persistentDataPath, "Logs");
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }
        return System.IO.Path.Combine(directory, $"game_log_{DateTime.Now:yyyyMMdd}.txt");
    }

    private string GetBugReportFilePath()
    {
        string directory = System.IO.Path.Combine(Application.persistentDataPath, "BugReports");
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }
        return System.IO.Path.Combine(directory, $"bugs_{DateTime.Now:yyyyMMdd}.txt");
    }

    /// <summary>
    /// 获取日志文件路径
    /// </summary>
    public string GetLatestLogPath()
    {
        return GetLogFilePath();
    }
}

[System.Serializable]
public class BugReport
{
    public string timestamp;
    public string message;
    public string description;
    public string reproductionSteps;
    public string stackTrace;
    public string logType;
    public string sceneName;
}
