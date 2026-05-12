package com.example.recipe.repository.mongo;

import com.example.recipe.model.Recipe;
import com.example.recipe.repository.RecipeRepository;
import com.mongodb.client.MongoClient;
import com.mongodb.client.MongoClients;
import com.mongodb.client.MongoCollection;
import com.mongodb.client.MongoDatabase;
import org.bson.Document;

import java.util.ArrayList;
import java.util.List;

public class MongoRecipeRepository implements RecipeRepository {
    private final MongoCollection<Document> collection;
    private final MongoClient mongoClient;

    public MongoRecipeRepository(String connectionString, String dbName, String collectionName) {
        mongoClient = MongoClients.create(connectionString);
        MongoDatabase database = mongoClient.getDatabase(dbName);
        collection = database.getCollection(collectionName);
    }

    @Override
    public void save(Recipe recipe) {
        Document doc = recipeToDocument(recipe);
        collection.insertOne(doc);
    }

    @Override
    public void saveAll(List<Recipe> recipes) {
        List<Document> docs = new ArrayList<>();
        for (Recipe r : recipes) {
            docs.add(recipeToDocument(r));
        }
        collection.insertMany(docs);
    }

    @Override
    public List<Recipe> findAll() {
        List<Recipe> list = new ArrayList<>();
        for (Document doc : collection.find()) {
            list.add(documentToRecipe(doc));
        }
        return list;
    }

    private Document recipeToDocument(Recipe r) {
        return new Document()
                .append("source", r.getSource())
                .append("name", r.getName())
                .append("category", r.getCategory())
                .append("additionalInfo", r.getAdditionalInfo())
                .append("recipeDescription", r.getRecipeDescription())
                .append("portions", r.getPortions())
                .append("productsList", r.getProductsList())
                .append("cookingDescription", r.getCookingDescription())
                .append("cookingSteps", r.getCookingSteps());
    }

    private Recipe documentToRecipe(Document doc) {
        Recipe r = new Recipe();
        r.setSource(doc.getString("source"));
        r.setName(doc.getString("name"));
        r.setCategory(doc.getList("category", String.class));
        r.setAdditionalInfo(doc.getList("additionalInfo", String.class));
        r.setRecipeDescription(doc.getString("recipeDescription"));
        r.setPortions(doc.getString("portions"));
        r.setProductsList(doc.getList("productsList", String.class));
        r.setCookingDescription(doc.getString("cookingDescription"));
        r.setCookingSteps(doc.getList("cookingSteps", String.class));
        return r;
    }

    public void close() {
        mongoClient.close();
    }
}