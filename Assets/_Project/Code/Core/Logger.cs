using System;
using System.IO;
using UnityEngine;

namespace VoxelSandbox.Core
{
    /// <summary>
    /// Structured logger with file output and log rotation.
    /// Logs are written to Application.persistentDataPath/logs/
    /// </summary>
    public static class Logger
    {
        private static string _logDirectory;
        private static string _currentLogFile;
        private static StreamWriter _logWriter;
        private static readonly object _lock = new object();
        private const int MAX_LOG_FILES = 5;

        public static void Initialize()
        {
            _logDirectory = Path.Combine(Application.persistentDataPath, "logs");
            Directory.CreateDirectory(_logDirectory);

            RotateLogs();

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            _currentLogFile = Path.Combine(_logDirectory, $"voxelsandbox_{timestamp}.log");

            try
            {
                _logWriter = new StreamWriter(_currentLogFile, true) { AutoFlush = true };
                Info("Logger initialized");
                Info($"Unity Version: {Application.unityVersion}");
                Info($"Platform: {Application.platform}");
                Info($"Log file: {_currentLogFile}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to initialize logger: {e.Message}");
            }
        }

        public static void Info(string message)
        {
            Log("INFO", message);
            Debug.Log(message);
        }

        public static void Warning(string message)
        {
            Log("WARN", message);
            Debug.LogWarning(message);
        }

        public static void Error(string message)
        {
            Log("ERROR", message);
            Debug.LogError(message);
        }

        private static void Log(string level, string message)
        {
            if (_logWriter == null) return;

            lock (_lock)
            {
                try
                {
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    string logLine = $"[{timestamp}] [{level}] {message}";
                    _logWriter.WriteLine(logLine);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to write to log: {e.Message}");
                }
            }
        }

        private static void RotateLogs()
        {
            try
            {
                var logFiles = Directory.GetFiles(_logDirectory, "voxelsandbox_*.log");
                
                if (logFiles.Length >= MAX_LOG_FILES)
                {
                    Array.Sort(logFiles);
                    
                    int filesToDelete = logFiles.Length - MAX_LOG_FILES + 1;
                    for (int i = 0; i < filesToDelete && i < logFiles.Length; i++)
                    {
                        File.Delete(logFiles[i]);
                        Debug.Log($"Rotated old log file: {Path.GetFileName(logFiles[i])}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to rotate logs: {e.Message}");
            }
        }

        public static void Shutdown()
        {
            if (_logWriter != null)
            {
                Info("Logger shutting down");
                _logWriter.Close();
                _logWriter = null;
            }
        }
    }
}
