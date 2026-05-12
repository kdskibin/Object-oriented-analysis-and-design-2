package com.example.recipe.repository;

import com.example.recipe.model.Recipe;
import java.util.List;

public interface RecipeRepository {
    void save(Recipe recipe);
    void saveAll(List<Recipe> recipes);
    List<Recipe> findAll();
}