using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using StarCube.Server.Game;

namespace StarCube.Server.Network
{
    public class ServerConnectionManager
    {
        /// <summary>
        /// 在指定端口开放，允许远程客户端连接到本服务器
        /// </summary>
        /// <param name="port"></param>
        public void OpenConnection(int port = 0)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
            listenSocket.Bind(endPoint);
            task.Start();
        }

        /// <summary>
        /// 当服务器关闭时，释放占用的资源
        /// </summary>
        public void Release()
        {
            running = false;
            task.Wait();
        }

        private void Dispose()
        {
            listenSocket.Shutdown(SocketShutdown.Both);
            listenSocket.Close();
            listenSocket.Dispose();
        }

        private void Run()
        {
            try
            {
                listenSocket.Listen(16);
                while (running)
                {
                    // 处理客户端接入请求
                    var socket = listenSocket.AcceptAsync();
                    var acceptTask = HandleConnect(socket);

                    // 处理客户端发来的包
                    foreach (ServerPlayer player in guidToServerPlayer.Values)
                    {

                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                Dispose();
            }
        }

        private async Task HandleConnect(Task<Socket> socketTask)
        {
            Socket socket = await socketTask;
            socket.ReceiveTimeout = 5000;
            var writer = threadLocalPipe.Value.Writer;
            var buffer = writer.GetMemory(2048);
            var count = await socket.ReceiveAsync(buffer, SocketFlags.None);

        }

        public ServerConnectionManager(ServerGame game)
        {
            this.game = game;
            task = new Task(Run, TaskCreationOptions.LongRunning);
            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public readonly ServerGame game;

        private readonly Task task;

        private readonly Socket listenSocket;

        private readonly ThreadLocal<Pipe> threadLocalPipe = new ThreadLocal<Pipe>(() => new Pipe());

        private readonly Dictionary<Guid, ServerPlayer> guidToServerPlayer = new Dictionary<Guid, ServerPlayer>();

        private volatile bool running = true;
    }
}
