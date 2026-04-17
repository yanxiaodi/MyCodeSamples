// Adapted from https://github.com/microsoft/agent-framework/blob/main/dotnet/samples/02-agents/AgentSkills/SubprocessScriptRunner.cs
// Executes file-based skill scripts as local subprocesses.
// For demonstration purposes only — add sandboxing, resource limits, and audit logging for production use.

using System.Diagnostics;
using System.Text;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

#pragma warning disable MAAI001

internal static class SubprocessScriptRunner
{
    public static async Task<object?> RunAsync(
        AgentFileSkill skill,
        AgentFileSkillScript script,
        AIFunctionArguments arguments,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(script.FullPath))
            return $"Error: Script file not found: {script.FullPath}";

        string? interpreter = Path.GetExtension(script.FullPath) switch
        {
            ".py"  => "python3",
            ".js"  => "node",
            ".sh"  => "bash",
            ".ps1" => "pwsh",
            _      => null,
        };

        var startInfo = new ProcessStartInfo
        {
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding  = Encoding.UTF8,
            UseShellExecute        = false,
            CreateNoWindow         = true,
            WorkingDirectory       = Path.GetDirectoryName(script.FullPath) ?? ".",
        };

        // Ensure Python outputs UTF-8 (important on Windows where the default code page is not UTF-8)
        startInfo.Environment["PYTHONIOENCODING"] = "utf-8";
        startInfo.Environment["PYTHONUTF8"]       = "1";

        if (interpreter is not null)
        {
            startInfo.FileName = interpreter;
            startInfo.ArgumentList.Add(script.FullPath);
        }
        else
        {
            startInfo.FileName = script.FullPath;
        }

        foreach (var (key, value) in arguments ?? [])
        {
            if (value is bool b)
            {
                if (b) startInfo.ArgumentList.Add(NormalizeKey(key));
            }
            else if (value is not null)
            {
                startInfo.ArgumentList.Add(NormalizeKey(key));
                startInfo.ArgumentList.Add(value.ToString()!);
            }
        }

        Process? process = null;
        try
        {
            process = Process.Start(startInfo);
            if (process is null) return $"Error: Failed to start script '{script.Name}'.";

            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var errorTask  = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

            string output = await outputTask.ConfigureAwait(false);
            string error  = await errorTask.ConfigureAwait(false);

            if (!string.IsNullOrEmpty(error))  output += $"\nStderr:\n{error}";
            if (process.ExitCode != 0)         output += $"\nScript exited with code {process.ExitCode}";

            return string.IsNullOrEmpty(output) ? "(no output)" : output.Trim();
        }
        catch (OperationCanceledException) { process?.Kill(entireProcessTree: true); throw; }
        catch (Exception ex)               { return $"Error: Failed to execute script '{script.Name}': {ex.Message}"; }
        finally                            { process?.Dispose(); }
    }

    private static string NormalizeKey(string key) => "--" + key.TrimStart('-');
}
