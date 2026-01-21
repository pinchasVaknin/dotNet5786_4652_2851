# 📌 רשימת בונוסים ותוספות בפרויקט

לנוחיות הבודק/ת, להלן טבלה המפרטת את התוספות שבוצעו בפרויקט ואת מיקומן בקוד.

## 👤 משתמשים למערכת

השתמש בפרטים הבאים כדי להתחבר למערכת:

### 🛡️ כניסת מנהל (Admin)
* **שם משתמש:** `222222222`
* **סיסמה:** `admin100`

### 📦 כניסת שליח (Courier)
* **שם משתמש:** `234567890`
* **סיסמה:** `1234567890`

---

## 🛠️ סביבת פיתוח

| הבונוס | מיקום בקוד (לחץ לפתיחה) | שורות | הערות | ניקוד |
| :--- | :--- | :--- | :--- | :---: |
| **שימוש נכון ומלא ב-TryParse** | [`BITest/Program.cs`](./BITest/Program.cs) <br> [`DalTest/Program.cs`](./DalTest/Program.cs) | 58, 92... <br> 449, 470... | שימוש ב-`int.TryParse` למניעת קריסות בקליטת מספרים בתפריטים. | **1** |

## 🏗️ שכבות נתונים ולוגיקה (DAL & BL)

| הבונוס | מיקום בקוד (לחץ לפתיחה) | שורות | הערות | ניקוד |
| :--- | :--- | :--- | :--- | :---: |
| **הוספת תכונת סיסמא** | [`IConfig.cs`](./DalFacade/DalApi/IConfig.cs) <br> [`Courier.cs`](./DalFacade/DO/Courier.cs) | 7 <br> 12 | הוספת שדות וניהול סיסמאות למשתמשים. | **2** |
| **Singleton & Thread Safe** | [`DalList.cs`](./DalList/DalList.cs) <br> [`DalXml.cs`](./DalXml/DalXml.cs) | כל הקובץ | מימוש Thread Safe עם `lock` ו-Lazy Initialization. | **2** |

## 🎨 שכבת התצוגה PL - שיפורי תצוגה (WPF מתקדם)

| הבונוס | מיקום בקוד (לחץ לפתיחה) | שורות | הערות | ניקוד |
| :--- | :--- | :--- | :--- | :---: |
| **תצוגה גרפית אינטראקטיבית (שגיאות)** | [`LoginWindow.xaml`](./PL/LoginWindow.xaml) <br> [`InputNumericToColorConverter.cs`](./PL/Helpers/Converters/InputNumericToColorConverter.cs) | 78 <br> כל הקובץ | שינוי צבע הקלט (אדום/ירוק) בהתאם למצב הוולידציה. | **1** |
| **ולידציה בתוך Binding** | [`LoginWindow.xaml.cs`](./PL/LoginWindow.xaml.cs) | 66 | בדיקת תקינות (שהקלט לא ריק) כחלק מהלוגיקה. | **1** |
| **אייקון (Icon) בחלון ובמשימות** | [`Style1.xaml`](./PL/Helpers/Styles/Style1.xaml) | 154, 75 | אייקון מותאם אישית בכותרת החלון ובשורת המשימות. | **1** |
| **שימוש בטריגרים (Triggers)** | [`Style1.xaml`](./PL/Helpers/Styles/Style1.xaml) <br> [`LoginWindow.xaml`](./PL/LoginWindow.xaml) | 268, 179 <br> 67 | **תכונות:** שינוי צבע בעכבר.<br>**נתונים:** העלמת טקסט בהקלדה.<br>**אירועים:** אנימציה בפתיחת חלון. | **3** |
| **ערכות נושא (Theme)** | [`Style1.xaml`](./PL/Helpers/Styles/Style1.xaml) <br> [`App.xaml`](./PL/App.xaml) | 510 <br> 11 | עיצוב אחיד המחובר דרך `App.xaml` לכל האפליקציה. | **1** |
| **שימוש ב-ControlTemplate** | [`Style1.xaml`](./PL/Helpers/Styles/Style1.xaml) | 468 | עיצוב מותאם אישית לכפתורים (פינות עגולות). | **1** |
| **גרפיקה (Drawing, Shapes)** | [`LoginWindow.xaml`](./PL/LoginWindow.xaml) <br> [`Icon.xaml`](./PL/Helpers/Items/Icon.xaml) | 67, 77 | שימוש באלמנטים גרפיים וצורות (Shapes). | **1** |
| **התנהגויות (Behavior)** | [`InputValidators.cs`](./PL/Helpers/Behavior/InputValidators.cs) <br> [`OrderWindow.xaml`](./PL/Order/OrderWindow.xaml) | 170 | אימות קלט מתקדם ברמת ה-XAML באמצעות Behaviors. | **1** |
| **תכונות מצורפות (Attached Properties)** | [`InputValidators.cs`](./PL/Helpers/Behavior/InputValidators.cs) | 18, 21 | מימוש תכונות מצורפות לשימוש ב-XAML. | **1** |
| **לחיצה על Enter ככפתור** | [`LoginWindow.xaml`](./PL/LoginWindow.xaml) | 70 | הגדרת `IsDefault="True"` לאישור מהיר. | **1** |

## 🚀 שכבת התצוגה PL - שיפורים הקשורים לנושא הפרויקט

| הבונוס | מיקום בקוד (לחץ לפתיחה) | שורות | הערות | ניקוד |
| :--- | :--- | :--- | :--- | :---: |
| **קיבוץ רשימת קריאות (Grouping)** | [`OrderListWindow.xaml`](./PL/Order/OrderListWindow.xaml) <br> [`OrderListWindow.xaml.cs`](./PL/Order/OrderListWindow.xaml.cs) | 75 <br> 154 | קיבוץ הרשימה לפי קריטריונים (כגון סטטוס). | **2** |
| **הסתרת סיסמה (כוכביות)** | [`LoginWindow.xaml`](./PL/LoginWindow.xaml) | 64 | שימוש בפקד `PasswordBox` להצגת כוכביות. | **1** |
| **כפתור מחיקה חכם** | [`OrderListWindow.xaml`](./PL/Order/OrderListWindow.xaml) <br> [`Visibility.cs`](./PL/Helpers/Converters/Visibility.cs) | 122 | הכפתור מוסתר/מוצג דינמית רק אם המחיקה אפשרית (לוגית). | **2** |

---
**סה"כ נקודות בונוס: 22**