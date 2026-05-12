package com.example.recipe.parser;

import com.example.recipe.model.Recipe;
import org.jsoup.Jsoup;
import org.jsoup.nodes.Document;
import org.jsoup.nodes.Element;
import org.jsoup.select.Elements;

import java.io.IOException;
import java.util.*;
import java.util.regex.Pattern;
import java.util.stream.Collectors;

public class RecipeParser {
    private static final Pattern CATEGORY_SPLIT = 
            Pattern.compile("(?<=[а-яё])(?=[А-ЯЁ])");
    private static final Random RANDOM = new Random();

    public static Recipe parseRecipe(int rid) throws IOException, InterruptedException {
        // имитация задержки между запросами
        Thread.sleep(30_000 + RANDOM.nextInt(10_000));

        String currentUrl = "https://www.russianfood.com/recipes/recipe.php?rid=" + rid;
        Document doc = Jsoup.connect(currentUrl)
                .userAgent("Mozilla/5.0")
                .timeout(10_000)
                .get();

        Recipe recipe = new Recipe();
        recipe.setSource(currentUrl);

        // Название
        Element titleEl = doc.selectFirst("h1.title");
        if (titleEl != null) recipe.setName(titleEl.text());

        // Доп. информация
        Element subInfo = doc.selectFirst("div.sub_info");
        if (subInfo != null) {
            String text = subInfo.text().replace('\u00A0', ' '); // \xa0
            recipe.setAdditionalInfo(
                Arrays.stream(text.split("\\s+"))
                      .filter(s -> !s.isEmpty())
                      .collect(Collectors.toList())
            );
        }

        // Описание рецепта
        Element descContainer = doc.selectFirst("td.padding_l.padding_r");
        if (descContainer != null) {
            Document descSoup = Jsoup.parse(descContainer.html());
            Element p = descSoup.selectFirst("p");
            if (p != null) recipe.setRecipeDescription(p.text());
        }

        // Продукты и порции
        Element ppTitle = doc.selectFirst("td.padding_l.ingr_title");
        if (ppTitle != null) {
            recipe.setPortions(ppTitle.text().replaceAll("^\\s+|\\s+$", ""));
        }

        // Список продуктов
        List<String> products = new ArrayList<>();
        Elements rows0 = doc.select("tr.ingr_tr_0");
        Elements rows1 = doc.select("tr.ingr_tr_1");
        for (Element row : rows0) products.add(cleanProduct(row.text()));
        for (Element row : rows1) products.add(cleanProduct(row.text()));
        recipe.setProductsList(products);

        // Описание приготовления
        Element howDiv = doc.selectFirst("div#how");
        if (howDiv != null) {
            String howText = howDiv.text()
                    .replace("\r", "")
                    .replaceAll("\\s+", " ");
            recipe.setCookingDescription(howText.trim());
        }

        // Шаги приготовления
        Elements steps = doc.select("div.step_n");
        List<String> stepList = new ArrayList<>();
        for (Element step : steps) {
            stepList.add(cleanStep(step.text()));
        }
        recipe.setCookingSteps(stepList);

        // Категории
        Element categoryDiv = doc.selectFirst("div.razdels.padding_l");
        if (categoryDiv != null) {
            String[] lines = categoryDiv.text().split("\\n");
            String catStr = lines.length > 0 && !lines[0].isEmpty() ? lines[0] : (lines.length > 1 ? lines[1] : "");
            recipe.setCategory(splitCategories(catStr));
        }

        return recipe;
    }

    private static String cleanProduct(String s) {
        return s.replace('\u00A0', ' ')
                .replaceAll("^\\s+|\\s+$", "")
                .replaceAll("\\s+", " ");
    }

    private static String cleanStep(String s) {
        return s.replace('\u00A0', ' ')
                .replaceAll("^\\s+|\\s+$", "")
                .replaceAll("\\s+", " ")
                .replace("\r", "")
                .replaceAll("\\n+", "\n");
    }

    private static List<String> splitCategories(String text) {
        List<String> result = new ArrayList<>();
        String[] parts = text.split("›");
        for (String part : parts) {
            part = part.trim();
            if (part.isEmpty()) continue;
            String[] subparts = CATEGORY_SPLIT.split(part);
            for (String sp : subparts) {
                if (!sp.trim().isEmpty()) result.add(sp.trim());
            }
        }
        return result;
    }
}