package com.example.recipe.repository.postgres;

import com.example.recipe.model.Recipe;
import com.example.recipe.repository.RecipeRepository;
import com.fasterxml.jackson.databind.ObjectMapper;
import java.sql.*;
import java.util.ArrayList;
import java.util.List;

public class PostgresRecipeRepository implements RecipeRepository {
    private final String url;
    private final String user;
    private final String password;
    private final ObjectMapper mapper = new ObjectMapper();

    public PostgresRecipeRepository(String url, String user, String password) {
        this.url = url;
        this.user = user;
        this.password = password;
        initTable();
    }

    private void initTable() {
        try (Connection conn = DriverManager.getConnection(url, user, password);
             Statement stmt = conn.createStatement()) {
            stmt.execute("CREATE TABLE IF NOT EXISTS recipes (" +
                         "id SERIAL PRIMARY KEY, " +
                         "source VARCHAR(512), " +
                         "data JSONB NOT NULL)");
        } catch (SQLException e) {
            throw new RuntimeException("Table init failed", e);
        }
    }

    @Override
    public void save(Recipe recipe) {
        String sql = "INSERT INTO recipes (source, data) VALUES (?, ?::jsonb)";
        try (Connection conn = DriverManager.getConnection(url, user, password);
             PreparedStatement ps = conn.prepareStatement(sql)) {
            ps.setString(1, recipe.getSource());
            ps.setString(2, mapper.writeValueAsString(recipe));
            ps.executeUpdate();
        } catch (Exception e) {
            throw new RuntimeException("Save failed", e);
        }
    }

    @Override
    public void saveAll(List<Recipe> recipes) {
        String sql = "INSERT INTO recipes (source, data) VALUES (?, ?::jsonb)";
        try (Connection conn = DriverManager.getConnection(url, user, password)) {
            conn.setAutoCommit(false);
            try (PreparedStatement ps = conn.prepareStatement(sql)) {
                for (Recipe r : recipes) {
                    ps.setString(1, r.getSource());
                    ps.setString(2, mapper.writeValueAsString(r));
                    ps.addBatch();
                }
                ps.executeBatch();
                conn.commit();
            }
        } catch (Exception e) {
            throw new RuntimeException("Batch save failed", e);
        }
    }

    @Override
    public List<Recipe> findAll() {
        List<Recipe> list = new ArrayList<>();
        try (Connection conn = DriverManager.getConnection(url, user, password);
             Statement stmt = conn.createStatement();
             ResultSet rs = stmt.executeQuery("SELECT data FROM recipes")) {
            while (rs.next()) {
                list.add(mapper.readValue(rs.getString("data"), Recipe.class));
            }
        } catch (Exception e) {
            throw new RuntimeException("Find all failed", e);
        }
        return list;
    }
}