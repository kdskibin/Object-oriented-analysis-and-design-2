// PaymentAdapterDemo.cpp
// Демонстрация паттерна "Адаптер" на примере интеграции разных платёжных систем
// Добавлено поле для ввода суммы платежа

#include <windows.h>
#include <commctrl.h>
#include <vector>
#include <string>
#include <cstdlib>   // для wcstod
#include <sstream>   // для wstringstream
#include <iomanip>   // для setprecision

using namespace std;

#pragma comment(lib, "comctl32.lib")


class PaymentGateway {
public:
    virtual ~PaymentGateway() {}
    virtual void pay(double amount) = 0; // единый метод оплаты
};

// Адаптируемые классы
class AlphaBank {
public:
    void makePayment(double amount, int MCC) {
        wstring msg;
        if (MCC == 5262) {
            msg = L"AlphaBank: платёж на сумму " + to_wstring(amount) + L". Бонусная программа не действует на данный магазин, бонусы зачислены не будут.";
        }
        else {
            msg = L"AlphaBank: платёж на сумму " + to_wstring(amount) + L".\nДанный магазин участвует в бонусной программе, вам будет начислено " + to_wstring(int(amount/10)) + L" бонусный рублей!";
        }
        MessageBox(NULL, msg.c_str(), L"AlphaBank", MB_OK);
    }
};

class BetaBank {
public:
    void doPay(double amount) {
        wstring msg = L"BetaBank: платёж на сумму " + to_wstring(amount);
        MessageBox(NULL, msg.c_str(), L"BetaBank", MB_OK);
    }
};

class GammaBank {
public:
    void sendMoney(double amount) {
        wstring msg = L"GammaBank: платёж на сумму " + to_wstring(amount);
        MessageBox(NULL, msg.c_str(), L"GammaBank", MB_OK);
    }
};

class OnlinePayment {
public:
    void pay(double amount, std::string card_number, int code, std::string date) {
        // Преобразуем std::string в std::wstring (для ASCII достаточно так)
        std::wstring wcard(card_number.begin(), card_number.end());
        std::wstring wdate(date.begin(), date.end());

        std::wstring msg = L"Онлайн оплата на сумму " + std::to_wstring(amount) +
            L" с карты " + wcard + L" .";
        MessageBox(NULL, msg.c_str(), L"GammaBank", MB_OK);
};


// Адаптеры
class AlphaBankAdapter : public PaymentGateway {
private:
    AlphaBank* bank;
public:
    AlphaBankAdapter(AlphaBank* b) : bank(b) {}
    void pay(double amount) override {
        bank->makePayment(amount, 1052);
    }
};

class BetaBankAdapter : public PaymentGateway {
private:
    BetaBank* bank;
public:
    BetaBankAdapter(BetaBank* b) : bank(b) {}
    void pay(double amount) override {
        bank->doPay(amount);
    }
};

class GammaBankAdapter : public PaymentGateway {
private:
    GammaBank* bank;
public:
    GammaBankAdapter(GammaBank* b) : bank(b) {}
    void pay(double amount) override {
        bank->sendMoney(amount);
    }
};


// Глобальные данные для GUI
HWND hCombo;
HWND hEditSum;
HWND hButton;
HWND hResultStatic;

vector<PaymentGateway*> gateways;
vector<wstring> bankNames = { L"AlphaBank", L"BetaBank", L"GammaBank" };

// Объекты банков
AlphaBank bankA;
BetaBank bankB;
GammaBank bankC;

// Вспомогательная функция для безопасного преобразования строки в double
bool ParseDouble(const wstring& str, double& out) {
    wstringstream ss(str);
    //ss.imbue(locale(""));
    ss >> out;
    return !ss.fail() && ss.eof();
}

LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam) {
    switch (message) {
    case WM_CREATE:
    {
        // Выпадающий список банков
        hCombo = CreateWindow(WC_COMBOBOX, L"",
            CBS_DROPDOWNLIST | WS_CHILD | WS_VISIBLE | WS_VSCROLL,
            50, 50, 200, 200, hWnd, (HMENU)100, NULL, NULL);
        for (const auto& name : bankNames)
            SendMessage(hCombo, CB_ADDSTRING, 0, (LPARAM)name.c_str());
        SendMessage(hCombo, CB_SETCURSEL, 0, 0);

        // Подпись "Сумма:"
        CreateWindow(L"STATIC", L"Сумма:",
            WS_VISIBLE | WS_CHILD,
            50, 90, 50, 20, hWnd, NULL, NULL, NULL);

        // Без стиля ES_NUMBER, чтобы можно было вводить точку/запятую
        hEditSum = CreateWindow(L"EDIT", L"100.0",
            WS_VISIBLE | WS_CHILD | WS_BORDER | ES_RIGHT,
            110, 90, 100, 20, hWnd, (HMENU)103, NULL, NULL);

        // Кнопка
        hButton = CreateWindow(L"BUTTON", L"Оплатить",
            WS_TABSTOP | WS_VISIBLE | WS_CHILD | BS_DEFPUSHBUTTON,
            50, 130, 100, 30, hWnd, (HMENU)101, NULL, NULL);

        // Строка результата
        hResultStatic = CreateWindow(L"STATIC", L"Выберите банк, введите сумму и нажмите Оплатить",
            WS_VISIBLE | WS_CHILD | SS_CENTER,
            50, 170, 300, 30, hWnd, NULL, NULL, NULL);

        // Адаптеры
        gateways.push_back(new AlphaBankAdapter(&bankA));
        gateways.push_back(new BetaBankAdapter(&bankB));
        gateways.push_back(new GammaBankAdapter(&bankC));
    }
    break;

    case WM_COMMAND:
        if (LOWORD(wParam) == 101) { // кнопка
            int sel = SendMessage(hCombo, CB_GETCURSEL, 0, 0);
            if (sel == CB_ERR) {
                MessageBox(hWnd, L"Выберите банк.", L"Ошибка", MB_OK);
                return 0;
            }

            // Чтение суммы из поля
            wchar_t buffer[64];
            GetWindowText(hEditSum, buffer, 64);
            std::wstring input(buffer);

            double amount;
            if (!ParseDouble(input, amount) || amount <= 0.0) {
                MessageBox(hWnd, L"Введите положительное число (например, 99.99 или 99,99).", L"Ошибка ввода", MB_OK);
                return 0;
            }

            // Выполняем оплату
            gateways[sel]->pay(amount);

            // Форматируем сумму с двумя знаками после запятой для красивого вывода
            std::wstringstream ss;
            ss.imbue(std::locale(""));
            ss << std::fixed << std::setprecision(2) << amount;
            std::wstring amountStr = ss.str();

            std::wstring result = L"Оплачено через " + bankNames[sel] + L": " + amountStr + L" руб.";
            SetWindowText(hResultStatic, result.c_str());
        }
        break;

    case WM_DESTROY:
        for (auto* g : gateways) delete g;
        PostQuitMessage(0);
        break;

    default:
        return DefWindowProc(hWnd, message, wParam, lParam);
    }
    return 0;
}


// Точка входа
int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nCmdShow) {
    // Инициализация общих элементов управления
    INITCOMMONCONTROLSEX icex;
    icex.dwSize = sizeof(INITCOMMONCONTROLSEX);
    icex.dwICC = ICC_STANDARD_CLASSES;
    InitCommonControlsEx(&icex);

    // Регистрация класса окна
    WNDCLASS wc = {};
    wc.lpfnWndProc = WndProc;
    wc.hInstance = hInstance;
    wc.lpszClassName = L"AdapterDemoClass";
    wc.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);
    wc.hCursor = LoadCursor(NULL, IDC_ARROW);

    if (!RegisterClass(&wc)) {
        MessageBox(NULL, L"Не удалось зарегистрировать класс окна", L"Ошибка", MB_OK);
        return 0;
    }

    // Создание главного окна (высота увеличена до 300)
    HWND hWnd = CreateWindow(wc.lpszClassName, L"Демонстрация паттерна Адаптер (Платежные системы)",
        WS_OVERLAPPEDWINDOW & ~WS_MAXIMIZEBOX & ~WS_THICKFRAME,
        CW_USEDEFAULT, CW_USEDEFAULT, 400, 300,  // высота теперь 300
        NULL, NULL, hInstance, NULL);

    if (!hWnd) {
        MessageBox(NULL, L"Не удалось создать окно", L"Ошибка", MB_OK);
        return 0;
    }

    ShowWindow(hWnd, nCmdShow);
    UpdateWindow(hWnd);

    // Цикл сообщений
    MSG msg;
    while (GetMessage(&msg, NULL, 0, 0)) {
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }

    return (int)msg.wParam;
}