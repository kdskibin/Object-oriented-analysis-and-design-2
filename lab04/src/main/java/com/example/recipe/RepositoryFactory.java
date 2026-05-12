package com.example.recipe;

import com.example.recipe.repository.RecipeRepository;
import com.example.recipe.repository.postgres.PostgresRecipeRepository;
import com.example.recipe.repository.mongo.MongoRecipeRepository;

public class RepositoryFactory {
    public static RecipeRepository createRepository(String type, String... params) {
        switch (type.toLowerCase()) {
            case "postgres":
                // params: url, user, password
                if (params.length < 3) throw new IllegalArgumentException("Postgres requires 3 params: url, user, password");
                return new PostgresRecipeRepository(params[0], params[1], params[2]);
            case "mongo":
                // params: connectionString, dbName, collectionName
                if (params.length < 3) throw new IllegalArgumentException("Mongo requires 3 params: connectionString, dbName, collectionName");
                return new MongoRecipeRepository(params[0], params[1], params[2]);
            default:
                throw new IllegalArgumentException("Unsupported repository type: " + type);
        }
    }
}