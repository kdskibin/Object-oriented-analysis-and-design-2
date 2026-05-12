package com.example.recipe.model;

import java.util.List;

public class Recipe {
    private String source;
    private String name;
    private List<String> category;
    private List<String> additionalInfo;
    private String recipeDescription;
    private String portions;
    private List<String> productsList;
    private String cookingDescription;
    private List<String> cookingSteps;

    public String getSource() { return source; }
    public void setSource(String source) { this.source = source; }
    public String getName() { return name; }
    public void setName(String name) { this.name = name; }
    public List<String> getCategory() { return category; }
    public void setCategory(List<String> category) { this.category = category; }
    public List<String> getAdditionalInfo() { return additionalInfo; }
    public void setAdditionalInfo(List<String> additionalInfo) { this.additionalInfo = additionalInfo; }
    public String getRecipeDescription() { return recipeDescription; }
    public void setRecipeDescription(String recipeDescription) { this.recipeDescription = recipeDescription; }
    public String getPortions() { return portions; }
    public void setPortions(String portions) { this.portions = portions; }
    public List<String> getProductsList() { return productsList; }
    public void setProductsList(List<String> productsList) { this.productsList = productsList; }
    public String getCookingDescription() { return cookingDescription; }
    public void setCookingDescription(String cookingDescription) { this.cookingDescription = cookingDescription; }
    public List<String> getCookingSteps() { return cookingSteps; }
    public void setCookingSteps(List<String> cookingSteps) { this.cookingSteps = cookingSteps; }
}