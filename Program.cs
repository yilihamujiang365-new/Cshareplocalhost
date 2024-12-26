using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Policy;
using System.Threading.Tasks;

class Program
{
    private static HttpListener _listener;
    private static string _directoryToServe;
    private static string _localAddress;

    static async Task Main(string[] args)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        // 获取程序集的版本
        Version version = assembly.GetName().Version;
        string frameworkVersion = Environment.Version.ToString();


        Console.WriteLine(@" ____  _____ _____   _                     _   _                  _   
| ___||___ /|___  | | |  ___    ___  __ _ | | | |__    ___   ___ | |_ 
|___ \  |_ \   / /  | | / _ \  / __|/ _` || | | '_ \  / _ \ / __|| __|
 ___) |___) | / /   | || (_) || (__| (_| || | | | | || (_) |\__ \| |_ 
|____/|____/ /_/    |_| \___/  \___|\__,_||_| |_| |_| \___/ |___/ \__|
                                                                      ");

Console.WriteLine($"欢迎使用537工作室开发的本地服务器托管程序，版本号:{version}\n.NET Framework版本是:{frameworkVersion}\n开发者:yilihamujiang365@outlook.coom\n\n");
        Console.Write("请输入要托管的文件夹路径：");

        _directoryToServe = Console.ReadLine();
        Console.Write("\n请输入要监听的端口号:");
        _localAddress=Console.ReadLine();

        if (string.IsNullOrEmpty(_directoryToServe) || !Directory.Exists(_directoryToServe))
        {
            Console.WriteLine("提供的文件夹路径不存在，请检查后重新输入。");
            return;
        }

        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://*:{_localAddress}/");

        try
        {
            _listener.Start();
            Console.WriteLine($"服务器已启动，访问 http://localhost:{_localAddress}/");
            openinbrower($"http://localhost:{_localAddress}");
           

            // 异步接受连接
            await AcceptConnections();
        }
        catch (HttpListenerException ex)
        {
            Console.WriteLine($"启动服务器失败: {ex.Message}");
            
            if (ex.ErrorCode == 5) // 访问被拒绝
            {
                Console.WriteLine("需要管理员权限来启动服务器。");
               
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"启动服务器时发生错误: {ex.Message}"); 
           
        }

    }
    static void openinbrower(string url)
    {
        // 使用Process.Start方法启动默认浏览器
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
        private static async Task AcceptConnections()
    {
        while (_listener.IsListening)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                ProcessRequest(context);
            }
            catch (Exception e)
            {
                Console.WriteLine($"处理请求时发生错误: {e.Message}");
              
            }
        }
    }

    private static void ProcessRequest(HttpListenerContext context)
    {
        var response = context.Response;
        var request = context.Request;

        string path = request.Url.AbsolutePath;
        if (path == "/")
        {
            path = "/index.html"; // 默认页面
        }

        path = Path.Combine(_directoryToServe, path.TrimStart('/'));

        if (File.Exists(path))
        {
            byte[] buffer = File.ReadAllBytes(path);
            response.ContentLength64 = buffer.Length;

            var contentType = GetContentType(Path.GetExtension(path));
            response.ContentType = contentType;

            response.OutputStream.Write(buffer, 0, buffer.Length);
        }
        else
        {
            response.StatusCode = (int)HttpStatusCode.NotFound;
            response.StatusDescription = "Not Found";
        }

        response.Close();
    }

    private static string GetContentType(string extension)
    {
        switch (extension)
        {
            case ".html":
                return "text/html";
            case ".css":
                return "text/css";
            case ".js":
                return "application/javascript";
            case ".png":
                return "image/png";
            case ".jpg":
                return "image/jpeg";
            case ".gif":
                return "image/gif";
            default:
                return "application/octet-stream";
        }
       
    }
    
}