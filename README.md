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
| **שימוש נכון ומלא ב-TryParse** | [`DalTest/Program.cs`](./DalTest/Program.cs) | 274, 280, 358 | תוספת רכזים | **1** |

## 🏗️ שכבות נתונים ולוגיקה (DAL & BL)

| הבונוס | מיקום בקוד (לחץ לפתיחה) | שורות | הערות | ניקוד |
| :--- | :--- | :--- | :--- | :---: |
| **הוספת תכונת סיסמא** | [`DalFacade/DO/Courier.cs`](./DalFacade/DO/Courier.cs) | 9 | תוספת רכזים | **2** |
| **Singleton & Thread Safe** | [`DallList/DalList.cs`](./DallList/DalList.cs) <br> [`DalXml/DalXml.cs`](./DalXml/DalXml.cs) | 17-22 <br> 17-22 | תוספת רכזים | **2** |
| **סיסמא ראשונית ע"י מנהל** | [`DalTest/Initialization.cs`](./DalTest/Initialization.cs) | 96 | תוספת רכזים | **1** |
| **חישוב מרחק לפי סוג רכב** | [`BL/Helpers/Tools.cs`](./BL/Helpers/Tools.cs) | 504–547 | תוספת רכזים | **3** |

## 🎨 שכבת התצוגה PL - שיפורי תצוגה (WPF מתקדם)

| הבונוס | מיקום בקוד (לחץ לפתיחה) | שורות | הערות | ניקוד |
| :--- | :--- | :--- | :--- | :---: |
| **תצוגה גרפית אינטראקטיבית (שג-errors)** | [`PL/Login/LoginWindow.xaml`](./PL/Login/LoginWindow.xaml) | ErrId: 61–64 · ErrPass: 74–77 | תוספת רכזים | **1** |
| **ולידציה בתוך Binding** | [`PL/Styles/TextBoxes.xaml`](./PL/Styles/TextBoxes.xaml) | 16–21 | תוספת רכזים | **1** |
| **אייקון (Icon) בחלון ובמשימות** | [`PL/Login/LoginWindow.xaml`](./PL/Login/LoginWindow.xaml) | 19-24 | תוספת רכזים | **1** |
| **טריגר תכונות (Property Trigger)** | [`PL/Styles/Buttons.xaml`](./PL/Styles/Buttons.xaml) | 21–30, 47–51 | תוספת רכזים | **1** |
| **טריגר נתונים (Data Trigger)** | [`PL/Styles/DataGrids.xaml`](./PL/Styles/DataGrids.xaml) | 11–15 | תוספת רכזים | **1** |
| **ערכות נושא (Theme)** | [`PL/App.xaml`](./PL/App.xaml) <br> [`PL/Styles/Colors.xaml`](./PL/Styles/Colors.xaml) | 13–21 <br>  | תוספת רכזים | **1** |
| **שימוש ב-ControlTemplate** | [`PL/Styles/Buttons.xaml`](./PL/Styles/Buttons.xaml) | 10–33 | תוספת רכזים | **1** |
| **לחיצה על Enter ככפתור** | [`PL/Login/LoginWindow.xaml.cs`](./PL/Login/LoginWindow.xaml.cs) | 125–144 | תוספת סטודנטים | **1** |

## 🚀 שכבת התצוגה PL - שיפורים הקשורים לנושא הפרויקט

| הבונוס | מיקום בקוד (לחץ לפתיחה) | שורות | הערות | ניקוד |
| :--- | :--- | :--- | :--- | :---: |
| **הסתרת סיסמה (כוכביות)** | [`PL/Login/LoginWindow.xaml`](./PL/Login/LoginWindow.xaml) | 69-73 | תוספת סטודנטים | **1** |
| **כפתור מחיקה חכם (שליח)** | [`PL/Converters.cs`](./PL/Converters.cs) | 173–198 | תוספת רכזים | **2** |
| **כפתור ביטול חכם (הזמנה)** | [`PL/Converters.cs`](./PL/Converters.cs) | 200–225 | תוספת רכזים אותו הדבר | **-** |

## 🔄 סימולטור

| הבונוס | מיקום בקוד (לחץ לפתיחה) | שורות | הערות | ניקוד |
| :--- | :--- | :--- | :--- | :---: |
| **חסימת פעולות בזמן סימולטור (BL)** | [`BL/Helpers/AdminManager.cs`](./BL/Helpers/AdminManager.cs) | 345–350 | תוספת רכזים | **2** |
| **תפיסת חריגות ב-PL** | [`PL/MainWindow.xaml.cs`](./PL/MainWindow.xaml.cs) | כל Button handlers | אני | **-** |
| **Thread-Safe Observer Pattern** | [`PL/Helpers/ObserverMutex.cs`](./PL/Helpers/ObserverMutex.cs) | כל הקובץ | אני | **-** |

---

## 📊 סיכום ניקוד

| קטגוריה | ניקוד |
|----------|--------|
| סביבת פיתוח (TryParse) | **1** |
| שכבת DAL (Singleton + Password) | **4** |
| שכבת BL (סיסמא + מרחק) | **4** |
| שכבת PL - WPF מתקדם | **8** |
| שכבת PL - שיפורים לפרויקט | **3** |
| שכבת PL - סימולטור | **2** |
| **סה"כ** | **22** |

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
├── DalFacade/    # ממשקי DAL ואובייקטי נתונים (DO)
├── DallList/       # מימוש DAL בזיכרון
├── DalXml/     # מימוש DAL מבוסס XML
├── DalTest/            # אפליקציית בדיקות DAL
├── BL/        # שכבת הלוגיקה העסקית
├── BlTest/      # אפליקציית בדיקות BL
├── PL/   # שכבת התצוגה WPF
│   ├── Controls/       # פקדים מותאמים אישית
│   ├── Converters.cs   # Value Converters
│├── Helpers/        # כלי עזר
│   ├── Styles/         # Resource Dictionaries
│   ├── Courier/        # חלונות ניהול שליחים
│   ├── Order/          # חלונות ניהול הזמנות
│   ├── Login/     # חלונות התחברות
│   └── delivery/       # חלונות היסטוריית משלוחים
└── Stage0/    # פרויקט שלב 0
```

---

## 👥 מפתחים

- מספר סטודנט: 4652
- מספר סטודנט: 2851