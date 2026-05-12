package com.example.recipe;

import com.example.recipe.service.RecipeService;
import com.sun.net.httpserver.HttpExchange;
import com.sun.net.httpserver.HttpServer;

import java.io.*;
import java.net.InetSocketAddress;
import java.nio.charset.StandardCharsets;
import java.util.HashMap;
import java.util.Map;
import java.util.concurrent.atomic.AtomicBoolean;

public class WebServer {
    private final RecipeService service;
    private final int port;
    private final AtomicBoolean running = new AtomicBoolean(false);
    private Thread parsingThread = null;

    public WebServer(RecipeService service) {
        this(service, 8080);
    }

    public WebServer(RecipeService service, int port) {
        this.service = service;
        this.port = port;
    }

    public void start() throws IOException {
        HttpServer server = HttpServer.create(new InetSocketAddress(port), 0);
        server.createContext("/", this::handleRoot);
        server.createContext("/start", this::handleStart);
        server.createContext("/status", this::handleStatus);
        server.setExecutor(null); // используем дефолтный
        server.start();
        System.out.println("Web interface started at http://localhost:" + port);
    }

    // Главная страница с формой
    private void handleRoot(HttpExchange exchange) throws IOException {
        if (!"GET".equalsIgnoreCase(exchange.getRequestMethod())) {
            exchange.sendResponseHeaders(405, -1);
            return;
        }
        String html = buildMainPage();
        byte[] bytes = html.getBytes(StandardCharsets.UTF_8);
        exchange.getResponseHeaders().set("Content-Type", "text/html; charset=UTF-8");
        exchange.sendResponseHeaders(200, bytes.length);
        OutputStream os = exchange.getResponseBody();
        os.write(bytes);
        os.close();
    }

    // Запуск парсинга (POST)
    private void handleStart(HttpExchange exchange) throws IOException {
        if (!"POST".equalsIgnoreCase(exchange.getRequestMethod())) {
            exchange.sendResponseHeaders(405, -1);
            return;
        }

        // Читаем параметры из тела запроса
        Map<String, String> params = parseFormData(exchange);
        String startStr = params.get("startRid");
        String endStr = params.get("endRid");

        if (startStr == null || endStr == null) {
            sendRedirect(exchange, "/?error=missing+parameters");
            return;
        }

        int startRid, endRid;
        try {
            startRid = Integer.parseInt(startStr);
            endRid = Integer.parseInt(endStr);
        } catch (NumberFormatException e) {
            sendRedirect(exchange, "/?error=invalid+numbers");
            return;
        }

        if (startRid <= 0 || endRid < startRid) {
            sendRedirect(exchange, "/?error=invalid+range");
            return;
        }

        // Запускаем парсинг, только если ещё не запущен
        if (running.compareAndSet(false, true)) {
            parsingThread = new Thread(() -> {
                try {
                    service.parseAndSave(startRid, endRid);
                } finally {
                    running.set(false);
                }
            });
            parsingThread.start();
            sendRedirect(exchange, "/?started=1");
        } else {
            sendRedirect(exchange, "/?error=already+running");
        }
    }

    // Статус парсинга (JSON)
    private void handleStatus(HttpExchange exchange) throws IOException {
        if (!"GET".equalsIgnoreCase(exchange.getRequestMethod())) {
            exchange.sendResponseHeaders(405, -1);
            return;
        }

        int current = service.getCurrentRid();
        int total = service.getTotalRids();
        boolean isRunning = running.get();

        String json = String.format(
            "{\"running\": %b, \"currentRid\": %d, \"totalRids\": %d}",
            isRunning, current, total
        );
        byte[] bytes = json.getBytes(StandardCharsets.UTF_8);
        exchange.getResponseHeaders().set("Content-Type", "application/json");
        exchange.sendResponseHeaders(200, bytes.length);
        OutputStream os = exchange.getResponseBody();
        os.write(bytes);
        os.close();
    }

    // Утилиты
    private String buildMainPage() {
        boolean isRunning = running.get();
        int current = service.getCurrentRid();
        int total = service.getTotalRids();
        String disabled = isRunning ? "disabled" : "";

        return "<!DOCTYPE html>\n" +
        "<html><head><meta charset='UTF-8'><title>Парсер рецептов</title>\n" +
        "<style>\n" +
        "  body { font-family: sans-serif; max-width: 600px; margin: 40px auto; padding: 0 20px; }\n" +
        "  .status { margin-top: 20px; padding: 10px; background: #f0f0f0; border-radius: 5px; }\n" +
        "  input[type=number] { width: 80px; margin-right: 10px; }\n" +
        "  button { padding: 5px 15px; }\n" +
        "</style>\n" +
        "<script>\n" +
        "  let interval;\n" +
        "  async function updateStatus() {\n" +
        "    try {\n" +
        "      let resp = await fetch('/status');\n" +
        "      let data = await resp.json();\n" +
        "      let div = document.getElementById('status');\n" +
        "      let btn = document.getElementById('startBtn');\n" +
        "      if (data.running) {\n" +
        "        div.innerHTML = `<b>Парсинг идёт:</b> шаг ${data.currentRid} из ${data.totalRids}`;\n" +
        "        btn.disabled = true;\n" +
        "      } else {\n" +
        "        div.innerHTML = '<b>Готов к запуску</b>';\n" +
        "        btn.disabled = false;\n" +
        "      }\n" +
        "    } catch(e) {}\n" +
        "  }\n" +
        "  window.onload = function() {\n" +
        "    updateStatus();\n" +
        "    interval = setInterval(updateStatus, 2000);\n" +
        "  };\n" +
        "</script>\n" +
        "</head><body>\n" +
        "<h1>Парсер рецептов с RussianFood</h1>\n" +
        "<form id='form' action='/start' method='post'>\n" +
        "  <label>Начальный RID: <input type='number' name='startRid' min='1' required " + disabled + "></label>\n" +
        "  <label>Конечный RID: <input type='number' name='endRid' min='1' required " + disabled + "></label>\n" +
        "  <button id='startBtn' type='submit' " + disabled + ">Запустить</button>\n" +
        "</form>\n" +
        "<div id='status' class='status'></div>\n" +
        "</body></html>";
    }

    private Map<String, String> parseFormData(HttpExchange exchange) throws IOException {
        InputStreamReader isr = new InputStreamReader(exchange.getRequestBody(), StandardCharsets.UTF_8);
        BufferedReader br = new BufferedReader(isr);
        StringBuilder body = new StringBuilder();
        String line;
        while ((line = br.readLine()) != null) {
            body.append(line);
        }
        String data = body.toString();
        Map<String, String> result = new HashMap<>();
        if (data.isEmpty()) return result;
        String[] pairs = data.split("&");
        for (String pair : pairs) {
            String[] kv = pair.split("=", 2);
            if (kv.length == 2) {
                result.put(java.net.URLDecoder.decode(kv[0], "UTF-8"),
                          java.net.URLDecoder.decode(kv[1], "UTF-8"));
            }
        }
        return result;
    }

    private void sendRedirect(HttpExchange exchange, String location) throws IOException {
        exchange.getResponseHeaders().set("Location", location);
        exchange.sendResponseHeaders(302, -1);
    }
}