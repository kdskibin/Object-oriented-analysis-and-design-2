# Recipe Parser
  
Парсер собирает рецепты с сайта russianfood.com и сохраняет их в одну из двух баз данных: PostgreSQL или MongoDB.

---

**Separated Interface** - это паттерн, при котором интерфейс репозитория (или любого другого компонента) выносится в отдельный пакет или модуль, а реализации этого интерфейса находятся в собственных пакетах.  
Клиентский код (сервис) зависит только от абстракции (интерфейса), а конкретная реализация предоставляется извне - например, через фабрику или внедрение зависимостей.

Сервис `RecipeService` работает исключительно с `RecipeRepository`, не зная о существовании конкретных классов. Выбор нужной реализации происходит в фабрике `RepositoryFactory` на основе аргументов командной строки.

---

## Преимущества использования паттерна в проекте

1. **Сильная связанность**  
   Класс `RecipeService` не зависит от деталей работы с MongoDB или PostgreSQL. Его можно использовать с любой реализацией `RecipeRepository` без изменений.

2. **Простота переключения между базами данных**  

3. **Удобство тестирования**  

4. **Расширяемость**  
   При необходимости можно добавить поддержку новой СУБД (например, MySQL, Redis) – достаточно создать ещё одну реализацию `RecipeRepository` и зарегистрировать ее.
   
---

## Пример кода без использования паттерна

Если бы интерфейс `RecipeRepository` отсутствовал, сервис был бы жёстко привязан к конкретной реализации, например:

```java
// Антипаттерн: прямая зависимость от MongoRecipeRepository
public class RecipeService {
    private final MongoRecipeRepository repository;

    public RecipeService() {
        this.repository = new MongoRecipeRepository(
            "mongodb://localhost:27017", "recipes_db", "recipes"
        );
    }

    public void parseAndSave(int startRid, int endRid) {
        // использование repository напрямую
    }
}

Проблемы такого подхода:

- Чтобы перейти на PostgreSQL, придётся переписывать код RecipeService.
- Невозможно протестировать сервис изолированно.
- Добавление нового типа хранилища потребует модификации всех классов, которые его используют.

С паттерном «Отделённый интерфейс»
// Интерфейс
public interface RecipeRepository {
    void save(Recipe recipe);
    void saveAll(List<Recipe> recipes);
    List<Recipe> findAll();
}

// Сервис зависит от интерфейса, а не от конкретного класса
public class RecipeService {
    private final RecipeRepository repository;

    public RecipeService(RecipeRepository repository) {
        this.repository = repository;
    }
    // ...
}

// Фабрика предоставляет нужную реализацию
public class RepositoryFactory {
    public static RecipeRepository createRepository(String type, String... params) {
        switch (type) {
            case "postgres": return new PostgresRecipeRepository(params[0], params[1], params[2]);
            case "mongo":    return new MongoRecipeRepository(params[0], params[1], params[2]);
            default: throw new IllegalArgumentException(...);
        }
    }
}

Отделение интерфейса от реализации упрощает поддержку, тестирование и развитие приложения.