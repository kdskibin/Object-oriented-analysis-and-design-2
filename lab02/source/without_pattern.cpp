#include <windows.h>
#include <commctrl.h>
#include <vector>
#include <string>
#include <random>
#include <chrono>
#include <sstream>
#include <iomanip>

using namespace std;

#pragma comment(lib, "comctl32.lib")

int generate_random_number(int lower_bound, int upper_bound) {
    static random_device rd;
    static mt19937 engine(rd());
    uniform_int_distribution<int> dist(lower_bound, upper_bound);
    return dist(engine);
}

int get_current_day() {
    auto now = chrono::system_clock::now();
    time_t now_time_t = chrono::system_clock::to_time_t(now);
    tm now_tm = {};
    localtime_s(&now_tm, &now_time_t);
    return now_tm.tm_mday;
}

int generate_random_balance() {
    int current_day = get_current_day();
    if ((current_day > 7 && current_day < 14) || (current_day > 21 && current_day < 28))
        return generate_random_number(5000, 10000);
    else
        return generate_random_number(10000, 20000);
}

int current_balance = generate_random_balance();

bool theft_activity() {
    double theft_proba = static_cast<double>(generate_random_number(1, 100)) / 100.0;
    if (theft_proba > 0.9) {
        current_balance = 0;
        return true;
    }
    return false;
}

bool ParseDouble(const wstring& str, double& out) {
    wstringstream ss(str);
    ss >> out;
    return !ss.fail() && ss.eof();
}

class AlphaBank {
public:
    void makePayment(double amount, int MCC) {
        wstring msg;
        if (MCC == 5262) {
            msg = L"AlphaBank: платёж на сумму " + to_wstring(amount) +
                L". Бонусная программа не действует на данный магазин, бонусы зачислены не будут.";
        }
        else {
            msg = L"AlphaBank: платёж на сумму " + to_wstring(amount) +
                L".\nДанный магазин участвует в бонусной программе, вам будет начислено " +
                to_wstring(static_cast<int>(amount / 10)) + L" бонусных рублей!";
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
    void pay(double amount, string card_number, int code, string date) {
        wstring wcard(card_number.begin(), card_number.end());
        wstring wdate(date.begin(), date.end());
        wstring msg = L"Онлайн оплата на сумму " + to_wstring(amount) +
            L" с карты " + wcard + L".";
        MessageBox(NULL, msg.c_str(), L"Онлайн-оплата", MB_OK);
    }
};

HWND hCombo;
HWND hEditSum;
HWND hButton;
HWND hResultStatic;
HWND hMoneyStatic;

vector<wstring> bankNames = { L"AlphaBank", L"BetaBank", L"GammaBank", L"Картой онлайн" };

AlphaBank alpha_bank;
BetaBank beta_bank;
GammaBank gamma_bank;
OnlinePayment online_pay;

LRESULT CALLBACK WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam) {
    switch (message) {
    case WM_CREATE:
    {
        // Выпадающий список
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

        // Отображение текущего баланса
        wstringstream ss;
        ss << fixed << setprecision(2) << current_balance;
        wstring current_balance_str = L"Ваш баланс: " + ss.str();
        hMoneyStatic = CreateWindow(L"STATIC", current_balance_str.c_str(),
            WS_VISIBLE | WS_CHILD | SS_CENTER,
            50, 210, 300, 30, hWnd, NULL, NULL, NULL);
    }
    break;

    case WM_COMMAND:
        if (LOWORD(wParam) == 101) { // кнопка Оплатить
            int sel = SendMessage(hCombo, CB_GETCURSEL, 0, 0);
            if (sel == CB_ERR) {
                MessageBox(hWnd, L"Выберите банк.", L"Ошибка", MB_OK);
                return 0;
            }

            // Чтение суммы из поля
            wchar_t buffer[64];
            GetWindowText(hEditSum, buffer, 64);
            wstring input(buffer);
            double amount;
            if (!ParseDouble(input, amount) || amount <= 0.0) {
                MessageBox(hWnd, L"Введите положительное число (например, 99.99 или 99,99).",
                    L"Ошибка ввода", MB_OK);
                return 0;
            }

            // Проверка кражи
            bool theft = theft_activity();
            if (theft) {
                MessageBox(hWnd, L"К сожалению, вы попали на фишинговый сайт. Все ваши деньги были украдены.",
                    L"Нет денег.", MB_OK);
                SetWindowText(hMoneyStatic, L"Ваш баланс: 0.00");
                SetWindowText(hResultStatic, L"Кража! Баланс обнулён.");
                return 0;
            }

            // Проверка наличия средств
            if (current_balance <= 0) {
                MessageBox(hWnd, L"У вас недостаточно средств!", L"Ошибка", MB_OK);
                return 0;
            }

            if (amount > current_balance) {
                MessageBox(hWnd, L"Недостаточно средств!", L"Ошибка", MB_OK);
                return 0;
            }

            // Выполняем оплату напрямую через соответствующий банк
            switch (sel) {
            case 0: // AlphaBank
                alpha_bank.makePayment(amount, 1052);
                break;
            case 1: // BetaBank
                beta_bank.doPay(amount);
                break;
            case 2: // GammaBank
                gamma_bank.sendMoney(amount);
                break;
            case 3: // Картой онлайн
                online_pay.pay(amount, "2202 0597 3205 1132", 123, "01/27");
                break;
            }

            // Списываем сумму
            current_balance -= amount;

            // Обновляем отображение баланса
            wstringstream ss;
            ss << fixed << setprecision(2) << current_balance;
            SetWindowText(hMoneyStatic, (L"Ваш баланс: " + ss.str()).c_str());

            // Обновляем строку результата
            ss.str(L"");
            ss << fixed << setprecision(2) << amount;
            wstring result = L"Оплачено через " + bankNames[sel] + L": " + ss.str() + L" руб.";
            SetWindowText(hResultStatic, result.c_str());
        }
        break;

    case WM_DESTROY:
        PostQuitMessage(0);
        break;

    default:
        return DefWindowProc(hWnd, message, wParam, lParam);
    }
    return 0;
}

int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nCmdShow) {
    // Инициализация общих элементов управления
    INITCOMMONCONTROLSEX icex = {};
    icex.dwSize = sizeof(INITCOMMONCONTROLSEX);
    icex.dwICC = ICC_STANDARD_CLASSES;
    InitCommonControlsEx(&icex);

    // Регистрация класса окна
    WNDCLASS wc = {};
    wc.lpfnWndProc = WndProc;
    wc.hInstance = hInstance;
    wc.lpszClassName = L"NoAdapterDemoClass";
    wc.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);
    wc.hCursor = LoadCursor(NULL, IDC_ARROW);

    if (!RegisterClass(&wc)) {
        MessageBox(NULL, L"Не удалось зарегистрировать класс окна", L"Ошибка", MB_OK);
        return 0;
    }

    // Создание главного окна
    HWND hWnd = CreateWindow(wc.lpszClassName, L"Нажми на кнопку, получишь результат",
        WS_OVERLAPPEDWINDOW & ~WS_MAXIMIZEBOX & ~WS_THICKFRAME,
        CW_USEDEFAULT, CW_USEDEFAULT, 400, 300,
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