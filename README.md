# dotNet5786_4652_2851

# 📌 רשימת בונוסים ותוספות בפרויקט

לנוחיות הבודק/ת, להלן טבלה המפרטת את התוספות שבוצעו בפרויקט ואת מיקומן בקוד.

## 👤 משתמשים למערכת

השתמש בפרטים הבאים כדי להתחבר למערכת:

### 🛡️ כניסת מנהל (Admin)
* **שם משתמש:** `333333333`
* **סיסמה:** `1`

### 📦 כניסת שליח (Courier)
* **משתנה** (כל ID של שליח מהמערכת לאחר אתחול - ניתן לראות ב-XML)

---

## 🛠️ סביבת פיתוח

| הבונוס | מיקום בקוד (לחץ לפתיחה) | שורות | הערות | ניקוד |
| :--- | :--- | :--- | :--- | :---: |
| **שימוש נכון ומלא ב-TryParse** | [`BlTest/Program.cs`](./BlTest/Program.cs) <br> [`DalTest/Program.cs`](./DalTest/Program.cs) | 58, 92... <br> 449, 470... | שימוש ב-`int.TryParse` למניעת קריסות בקליטת מספרים בתפריטים. | **1** |

## 🏗️ שכבות נתונים ולוגיקה (DAL & BL)

| הבונוס | מיקום בקוד (לחץ לפתיחה) | שורות | הערות | ניקוד |
| :--- | :--- | :--- | :--- | :---: |
| **הוספת תכונת סיסמא** | [`DalFacade/DO/Courier.cs`](./DalFacade/DO/Courier.cs) | 7 | תכונת `CourierPassword` בישות השליח | **2** |
| **Singleton & Thread Safe** | [`DallList/DalList.cs`](./DallList/DalList.cs) <br> [`DalXml/DalXml.cs`](./DalXml/DalXml.cs) | 15-22 <br> 15-22 | מימוש Thread Safe עם `Lazy<T>` ו-Lazy Initialization. | **2** |
| **סיסמא ראשונית ע"י מנהל** | [`DalTest/Initialization.cs`](./DalTest/Initialization.cs) | 85 | יצירת סיסמאות ראשוניות לשליחים בזמן אתחול | **1** |
| **חישוב מרחק לפי סוג רכב** | [`BL/Helpers/Tools.cs`](./BL/Helpers/Tools.cs) | 295-340 | `GetActualDistanceAsync` משתמש ב-OSRM API עם פרופילים: car, bike, foot | **3** |

## 🎨 שכבת התצוגה PL - שיפורי תצוגה (WPF מתקדם)

| הבונוס | מיקום בקוד (לחץ לפתיחה) | שורות | הערות | ניקוד |
| :--- | :--- | :--- | :--- | :---: |
| **תצוגה גרפית אינטראקטיבית (שגיאות)** | [`PL/Login/LoginWindow.xaml`](./PL/Login/LoginWindow.xaml) | 68-72 | TextBlocks עם הודעות שגיאה באדום שמופיעות/נעלמות | **1** |
| **ולידציה בתוך Binding** | [`PL/Styles/TextBoxes.xaml`](./PL/Styles/TextBoxes.xaml) | 13-22 | Triggers לשינוי מראה לפי `IsReadOnly`; Converters לבדיקת enum | **1** |
| **אייקון (Icon) בחלון ובמשימות** | [`PL/Login/LoginWindow.xaml`](./PL/Login/LoginWindow.xaml) | 17 | אייקון מותאם אישית בכותרת החלון ובשורת המשימות | **1** |
| **טריגר תכונות (Property Trigger)** | [`PL/Styles/Buttons.xaml`](./PL/Styles/Buttons.xaml) | 23-30, 48-51 | `IsMouseOver`, `IsPressed` triggers לשינוי Opacity ו-Foreground | **1** |
| **טריגר נתונים (Data Trigger)** | [`PL/Styles/DataGrids.xaml`](./PL/Styles/DataGrids.xaml) | 10-14 | `EnumDataGridCellStyle` עם Converter לצביעה לפי Enum | **1** |
| **ערכות נושא (Theme)** | [`PL/App.xaml`](./PL/App.xaml) <br> [`PL/Styles/Colors.xaml`](./PL/Styles/Colors.xaml) | 11 <br> כל הקובץ | עיצוב אחיד המחובר דרך `App.xaml` לכל האפליקציה | **1** |
| **שימוש ב-ControlTemplate** | [`PL/Styles/Buttons.xaml`](./PL/Styles/Buttons.xaml) | 11-31 | Template מותאם אישית עם Border, ContentPresenter ו-Triggers | **1** |
| **לחיצה על Enter ככפתור** | [`PL/Login/LoginWindow.xaml.cs`](./PL/Login/LoginWindow.xaml.cs) | 74-88 | `HandleEnterKey` מטפל ב-`Key.Enter` להפעלת Login או מעבר פוקוס | **1** |

## 🚀 שכבת התצוגה PL - שיפורים הקשורים לנושא הפרויקט

| הבונוס | מיקום בקוד (לחץ לפתיחה) | שורות | הערות | ניקוד |
| :--- | :--- | :--- | :--- | :---: |
| **הסתרת סיסמה (כוכביות)** | [`PL/Login/LoginWindow.xaml`](./PL/Login/LoginWindow.xaml) | 64 | שימוש בפקד `PasswordBox` להצגת כוכביות | **1** |
| **כפתור מחיקה חכם (שליח)** | [`PL/Converters.cs`](./PL/Converters.cs) | 90-115 | `ConvertDeleteToEnabled` בודק `IsCourierDeletable()` לשליטה בכפתור | **2** |
| **כפתור ביטול חכם (הזמנה)** | [`PL/Converters.cs`](./PL/Converters.cs) | 117-135 | `ConvertCancelToEnabled` בודק `OrderStatus` (Open/InProgress) | **-** |

## 🔄 סימולטור

| הבונוס | מיקום בקוד (לחץ לפתיחה) | שורות | הערות | ניקוד |
| :--- | :--- | :--- | :--- | :---: |
| **חסימת פעולות בזמן סימולטור (BL)** | [`BL/Helpers/AdminManager.cs`](./BL/Helpers/AdminManager.cs) | 288-293 | `ThrowOnSimulatorIsRunning()` זורק `BlTemporaryNotAvailableException` | **-** |
| **תפיסת חריגות ב-PL** | [`PL/MainWindow.xaml.cs`](./PL/MainWindow.xaml.cs) | כל Button handlers | כל הכפתורים עטופים ב-try-catch עם MessageBox | **-** |
| **Thread-Safe Observer Pattern** | [`PL/Helpers/ObserverMutex.cs`](./PL/Helpers/ObserverMutex.cs) | כל הקובץ | Mutex pattern למניעת race conditions בעדכון UI | **-** |

---

## 📊 סיכום ניקוד

| קטגוריה | ניקוד |
|----------|--------|
| סביבת פיתוח (TryParse) | **1** |
| שכבת DAL (Singleton + Password) | **4** |
| שכבת BL (סיסמא + מרחק) | **4** |
| שכבת PL - WPF מתקדם | **8** |
| שכבת PL - שיפורים לפרויקט | **3** |
| **סה"כ** | **20** |

---

## 🏗️ ארכיטקטורת המערכת

### שכבות המערכת:
1. **DAL (Data Access Layer)** - אחסון נתונים ב-XML וברשימות עם Singleton Pattern
2. **BL (Business Logic)** - כללי עסקיים, ולידציה וסימולטור
3. **PL (Presentation Layer)** - ממשק WPF עם דפוסי MVVM

### תכונות עיקריות:
- **סימולטור מרובה תהליכונים** - הקצאת הזמנות ומשלוחים אוטומטית
- **Observer Pattern** - עדכוני UI בזמן אמת בכל החלונות
- **פעולות Thread-Safe** - הגנה מבוססת Mutex לגישה מקבילית
- **אינטגרציה עם API חיצוני** - Nominatim (geocoding) ו-OSRM (routing)

---

## 🚀 התחלה מהירה

1. פתח את `dotNet5786_4652_2851.sln` ב-Visual Studio 2022
2. הגדר את `PL` כפרויקט הפעלה
3. בנה והפעל את הפתרון
4. לחץ על "Initialize Database" ליצירת נתונים ראשוניים
5. התחבר עם פרטי המנהל:
   - **ID:** `333333333`
   - **סיסמה:** `1`

---

## 📁 מבנה הפרויקט

```
dotNet5786_4652_2851/
├── DalFacade/          # ממשקי DAL ואובייקטי נתונים (DO)
├── DallList/     # מימוש DAL בזיכרון
├── DalXml/  # מימוש DAL מבוסס XML
├── DalTest/       # אפליקציית בדיקות DAL
├── BL/       # שכבת הלוגיקה העסקית
├── BlTest/ # אפליקציית בדיקות BL
├── PL/        # שכבת התצוגה WPF
│   ├── Controls/       # פקדים מותאמים אישית
│   ├── Converters.cs   # Value Converters
│   ├── Helpers/        # כלי עזר
│   ├── Styles/         # Resource Dictionaries
│├── Courier/        # חלונות ניהול שליחים
│   ├── Order/          # חלונות ניהול הזמנות
│   ├── Login/     # חלונות התחברות
│   └── delivery/       # חלונות היסטוריית משלוחים
└── Stage0/             # פרויקט שלב 0
```

---

## 👥 מפתחים

- מספר סטודנט: 4652
- מספר סטודנט: 2851