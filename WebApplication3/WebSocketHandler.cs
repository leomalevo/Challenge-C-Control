using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using System.Runtime.InteropServices;

public static class WebSocketHandler
{
    [DllImport("user32.dll")]
    private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    private static Dictionary<int, Process> activeWindows = new Dictionary<int, Process>();
    private static WebSocket? _socket; // Guardamos el WebSocket para enviar actualizaciones

    public static async Task HandleConnection(WebSocket socket)
    {
        _socket = socket;
        var buffer = new byte[1024 * 4];

        try
        {
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count).Trim();

                if (string.IsNullOrEmpty(message)) continue;

                Console.WriteLine($"Message received: {message}");

                try
                {
                    var request = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(message);
                    if (request != null) ProcessAction(request);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing JSON: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WebSocket error: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("WebSocket closed.");
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal closure", CancellationToken.None);
        }
    }

    private static void ProcessAction(Dictionary<string, JsonElement> data)
    {
        if (!data.ContainsKey("action") || !data.ContainsKey("id")) return;

        int id = data["id"].GetInt32();
        string action = data["action"].GetString();

        Console.WriteLine($"Executing action: {action} for window {id}");

        switch (action)
        {
            case "open":
                OpenWindow(id);
                break;
            case "move":
                HandleMove(id, data);
                break;
            case "resize":
                HandleResize(id, data);
                break;
            case "close":
                HandleClose(id);
                break;
        }
    }

    private static void OpenWindow(int id)
    {
        if (!activeWindows.ContainsKey(id))
        {
            Console.WriteLine($"Opening Notepad instance for window {id}...");
            var process = Process.Start("notepad.exe");
            process.WaitForInputIdle();
            activeWindows[id] = process;

            SendUpdateToFrontend(id, 100, 100, 400, 300); // Posición inicial
        }
    }

    private static void HandleMove(int id, Dictionary<string, JsonElement> data)
    {
        if (!data.ContainsKey("x") || !data.ContainsKey("y")) return;

        int x = data["x"].GetInt32();
        int y = data["y"].GetInt32();
        int width = data.ContainsKey("width") ? data["width"].GetInt32() : 400;  // ✅ Si no se envía el tamaño, conserva el último conocido
        int height = data.ContainsKey("height") ? data["height"].GetInt32() : 300;

        ModifyWindow(id, x, y, width, height);
    }

    private static void HandleResize(int id, Dictionary<string, JsonElement> data)
    {
        if (!data.ContainsKey("width") || !data.ContainsKey("height"))
        {
            Console.WriteLine("Error: Faltan parámetros 'width' o 'height' en la acción de redimensionar.");
            return;
        }

        int width = data["width"].GetInt32();
        int height = data["height"].GetInt32();

        // ✅ Mantener posición actual si se envía en la petición
        int x = data.ContainsKey("x") ? data["x"].GetInt32() : GetCurrentX(id);
        int y = data.ContainsKey("y") ? data["y"].GetInt32() : GetCurrentY(id);

        ModifyWindow(id, x, y, width, height);
    }

    // ✅ Funciones auxiliares para obtener la posición actual si no se envía
    private static int GetCurrentX(int id)
    {
        if (activeWindows.ContainsKey(id))
            return activeWindows[id].MainWindowHandle != IntPtr.Zero ? GetWindowX(activeWindows[id].MainWindowHandle) : 100; // Fallback

        return 100; // Valor por defecto si la ventana no existe
    }

    private static int GetCurrentY(int id)
    {
        if (activeWindows.ContainsKey(id))
            return activeWindows[id].MainWindowHandle != IntPtr.Zero ? GetWindowY(activeWindows[id].MainWindowHandle) : 100; // Fallback

        return 100; // Valor por defecto si la ventana no existe
    }

    // ✅ Métodos auxiliares para obtener coordenadas reales desde la API de Windows
    private static int GetWindowX(IntPtr hWnd)
    {
        RECT rect;
        GetWindowRect(hWnd, out rect);
        return rect.Left;
    }

    private static int GetWindowY(IntPtr hWnd)
    {
        RECT rect;
        GetWindowRect(hWnd, out rect);
        return rect.Top;
    }

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    private static void HandleClose(int id)
    {
        if (activeWindows.ContainsKey(id))
        {
            activeWindows[id].Kill();
            activeWindows.Remove(id);
            Console.WriteLine($"Window {id} closed.");

            // ✅ Enviar evento de cierre a Angular
            SendUpdateToFrontend(id);
        }
    }

    private static async void SendUpdateToFrontend(int id)
    {
        if (_socket == null || _socket.State != WebSocketState.Open) return;

        var updateMessage = new
        {
            action = "closed", // ✅ Nuevo evento para eliminar ventanas en Angular
            id
        };

        string json = JsonSerializer.Serialize(updateMessage);
        var bytes = Encoding.UTF8.GetBytes(json);
        await _socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }


    private static void ModifyWindow(int id, int x, int y, int width, int height)
    {
        if (!activeWindows.ContainsKey(id))
        {
            OpenWindow(id); // Si la ventana no existe, abrir una nueva
        }

        IntPtr hWnd = activeWindows[id].MainWindowHandle;
        MoveWindow(hWnd, x, y, width, height, true);
        Console.WriteLine($"Window {id} modified: X={x}, Y={y}, Width={width}, Height={height}");

        // Enviar actualización a Angular
        SendUpdateToFrontend(id, x, y, width, height);
    }

    private static async void SendUpdateToFrontend(int id, int x, int y, int width, int height)
    {
        if (_socket == null || _socket.State != WebSocketState.Open) return;

        var updateMessage = new
        {
            action = "update",
            id,
            x,
            y,
            width,
            height
        };

        string json = JsonSerializer.Serialize(updateMessage);
        var bytes = Encoding.UTF8.GetBytes(json);
        await _socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }
}