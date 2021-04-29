using Amazon.DynamoDBv2;
using Amazon.Runtime;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace DynamoDbLocal
{
    public class DynamoDbLocalWrapper
    {
        private static readonly string InMemoryParameter = "inMemory";
        private static readonly string SharedDbParamenter = "sharedDb";
        private static readonly string DynamoDbLocalFolder = "dynamodb_local_latest";
        private static readonly string DynamoDbLocalJarName = "DynamoDBLocal.jar";
        private static readonly string DynamoDbLocalLibFolder = "DynamoDBLocal_lib";
        private static readonly string CurrentDir = AppDomain.CurrentDomain.BaseDirectory;

        public static Process CreateInMemoryDbProcess(int port = 8000, string? environmentVariableName = null) => 
            CreateDbProcess(InMemoryParameter, port, JarFileEnvironmentPath(environmentVariableName));

        public static Process CreateSharedDbProcess(int port = 8000, string? environmentVariableName = null) => 
            CreateDbProcess(SharedDbParamenter, port, JarFileEnvironmentPath(environmentVariableName));

        public static Process CreateProcess(string input, string workingDirectory = "")
        {
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var argumentsPrefix = isWindows ? "/c" : "-c";
            var processFileName = isWindows ? "cmd" : "bash";
            var arguments = @$"{argumentsPrefix} ""{input}""";

            return new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = processFileName,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };
        }

        public static AmazonDynamoDBClient CreateClient(int port = 8000, string accessKey = "ACCESS_KEY", string secretKey = "SECRET_KEY") => new (
            new BasicAWSCredentials(accessKey, secretKey),
            new AmazonDynamoDBConfig { ServiceURL = $"http://localhost:{port}" });

        private static Process CreateDbProcess(string type, int port = 8000, string? jarFileEnvironmentPath = null)
        {
            return CreateProcess(
                GetJavaArgumentsForLocalDynamoDb(type, port),
                jarFileEnvironmentPath ?? GetJarFilePathForLocalDynamoDb);
        }

        private static string GetJavaArgumentsForLocalDynamoDb(string type, int port) =>
            $"java -DJava.library.path={Path.DirectorySeparatorChar}{DynamoDbLocalLibFolder} -jar {DynamoDbLocalJarName} -{type} -port {port}";

        private static string? JarFileEnvironmentPath(string? environmentVariableName) => 
            Environment.GetEnvironmentVariable(environmentVariableName ?? string.Empty);

        private static string GetJarFilePathForLocalDynamoDb => Path.GetFullPath(Path.Combine(
            GetRootPath(CurrentDir) + Path.DirectorySeparatorChar + DynamoDbLocalFolder));

        private static string GetRootPath(string path)
        {
            var currentDir = Directory.GetParent(path).Parent.FullName;

            if (Directory.Exists($"{currentDir}{Path.DirectorySeparatorChar}{DynamoDbLocalFolder}"))
            {
                return currentDir;
            }

            return GetRootPath(currentDir);
        }
    }
}
