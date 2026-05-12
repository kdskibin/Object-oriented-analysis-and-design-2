package com.example.recipe;

import com.example.recipe.repository.RecipeRepository;
import com.example.recipe.service.RecipeService;

public class Main {
    public static void main(String[] args) throws Exception {
        // Для запуска веб-интерфейса нужно передать параметры БД как раньше
        if (args.length < 4) {
            System.out.println("Usage: java -jar recipe-parser.jar <dbType> <params...>");
            System.out.println("  web server will start on port 8080 (set web.port for custom)");
            System.exit(1);
        }

        String dbType = args[0];
        String[] dbParams = new String[args.length - 1];
        System.arraycopy(args, 1, dbParams, 0, dbParams.length);

        RecipeRepository repository = RepositoryFactory.createRepository(dbType, dbParams);
        RecipeService service = new RecipeService(repository);

        int port = Integer.parseInt(System.getProperty("web.port", "8080"));
        WebServer webServer = new WebServer(service, port);
        webServer.start();
    }
}