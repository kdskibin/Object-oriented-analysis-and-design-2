package com.example.recipe.service;

import com.example.recipe.model.Recipe;
import com.example.recipe.parser.RecipeParser;
import com.example.recipe.repository.RecipeRepository;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.ArrayList;
import java.util.List;
import java.util.Random;
import java.util.concurrent.atomic.AtomicInteger;

public class RecipeService {
    private static final Logger log = LoggerFactory.getLogger(RecipeService.class);
    private final RecipeRepository repository;

    // Поля для чтения прогресса из веб-интерфейса
    private volatile int currentRid = -1;
    private volatile int totalRids = 0;

    public RecipeService(RecipeRepository repository) {
        this.repository = repository;
    }

    public int getCurrentRid() {
        return currentRid;
    }

    public int getTotalRids() {
        return totalRids;
    }

    public void parseAndSave(int startRid, int endRid) {
        totalRids = endRid - startRid + 1;
        List<Recipe> buffer = new ArrayList<>();
        for (int rid = startRid; rid <= endRid; rid++) {
            currentRid = rid;   // обновляем прогресс
            try {
                log.info("Parsing recipe rid={}", rid);
                Recipe recipe = RecipeParser.parseRecipe(rid);
                buffer.add(recipe);

                if (buffer.size() >= 10) {
                    repository.saveAll(buffer);
                    buffer.clear();
                }

                if ((rid - startRid + 1) % 20 == 0) {
                    log.info("Long pause after 20 requests...");
                    Thread.sleep(660_000 + new Random().nextInt(120_000));
                }
            } catch (Exception e) {
                log.error("Failed to process rid={}", rid, e);
            }
        }
        // сохраняем оставшиеся
        if (!buffer.isEmpty()) {
            repository.saveAll(buffer);
        }
        currentRid = -1;   // признак окончания
    }
}